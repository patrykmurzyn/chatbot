using System;
using System.Text;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using ChatbotAI.API.Hubs;
using ChatbotAI.Domain.Models;
using ChatbotAI.Infrastructure.Data;
using ChatbotAI.Application.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ChatbotAI.API;

namespace ChatbotAI.API.Services
{
    public class ChatService : IChatService
    {
        private readonly IStreamingService _streamingService;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ChatService> _logger;
        private readonly ConcurrentDictionary<string, CancellationTokenSource> _activeGenerations;
        private readonly ChatSettings _settings;

        public ChatService(IStreamingService streamingService, IHubContext<ChatHub> hubContext, IServiceScopeFactory scopeFactory, ILogger<ChatService> logger, IOptions<ChatSettings> settings)
        {
            _streamingService = streamingService;
            _hubContext = hubContext;
            _scopeFactory = scopeFactory;
            _logger = logger;
            _settings = settings.Value;
            _activeGenerations = new ConcurrentDictionary<string, CancellationTokenSource>();
        }

        public async Task ReplayPendingMessagesAsync(Guid sessionId, string connectionId)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var pendingMessages = await dbContext.Messages
                .Where(m => m.SessionId == sessionId && m.IsPartial)
                .ToListAsync();

            if (pendingMessages.Any())
            {
                await _hubContext.Clients.Client(connectionId).SendAsync("ResponseGenerationStarted");
                foreach (var msg in pendingMessages)
                {
                    await _hubContext.Clients.Client(connectionId)
                        .SendAsync("ResponseGenerationProgress", msg.Content, msg.Id.ToString());
                }
            }
        }

        public async Task StartGenerationAsync(Guid sessionId, string userMessage, int characterId)
        {
            // Validate non-empty message
            if (string.IsNullOrWhiteSpace(userMessage))
            {
                _logger.LogWarning("Empty user message for session {SessionId}", sessionId);
                await _hubContext.Clients.Group(sessionId.ToString()).SendAsync("Error", "Message cannot be empty.");
                return;
            }
            // Validate message length
            if (userMessage.Length > _settings.MaxUserMessageLength)
            {
                _logger.LogWarning("User message too long for session {SessionId}. Length: {Length}", sessionId, userMessage.Length);
                await _hubContext.Clients.Group(sessionId.ToString())
                    .SendAsync("Error", $"Message too long. Maximum length is {_settings.MaxUserMessageLength} characters.");
                return;
            }
            if (_activeGenerations.TryRemove(sessionId.ToString(), out var oldCts))
            {
                oldCts.Cancel();
                oldCts.Dispose();
            }
            var cts = new CancellationTokenSource();
            _activeGenerations[sessionId.ToString()] = cts;
            var cancellationToken = cts.Token;

            var group = _hubContext.Clients.Group(sessionId.ToString());
            await group.SendAsync("ResponseGenerationStarted");

            var creationScope = _scopeFactory.CreateScope();
            try
            {
                var dbContext = creationScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Resolve character entity by ID
                var characterEntity = await dbContext.Characters.FindAsync(new object[] { characterId }, cancellationToken)
                    ?? throw new InvalidOperationException($"Character with ID {characterId} not found.");
                var characterKey = characterEntity.Key;

                // Prepare initial bot message seed
                var botMessage = new Message
                {
                    Id = Guid.NewGuid(),
                    SessionId = sessionId,
                    Content = string.Empty,
                    IsFromUser = false,
                    IsPartial = true,
                    CreatedAt = DateTime.UtcNow,
                    CharacterId = characterEntity.Id
                };
                dbContext.Messages.Add(botMessage);
                await dbContext.SaveChangesAsync(cancellationToken);

                _ = StreamResponseAsync(botMessage.Id, sessionId.ToString(), userMessage, characterKey, cancellationToken);
            }
            finally
            {
                creationScope.Dispose();
            }
        }

        private async Task StreamResponseAsync(Guid messageId, string sessionId, string userMessage, string character, CancellationToken cancellationToken)
        {
            int chunkSize = _settings.DefaultChunkSize;
            int delayMs = _settings.DefaultDelayMs;

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var backgroundDb = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var backgroundMessage = await backgroundDb.Messages.FindAsync(new object[] { messageId }, cancellationToken)
                    ?? throw new InvalidOperationException($"Message {messageId} not found.");
                var fullResponse = new StringBuilder();
                var isFirstChunk = true;

                await _streamingService.StreamTextAsync(
                    userMessage, character, chunkSize, delayMs,
                    async chunk =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        fullResponse.Append(chunk);
                        backgroundMessage.Content = fullResponse.ToString();
                        await backgroundDb.SaveChangesAsync(cancellationToken);

                        if (isFirstChunk)
                        {
                            await _hubContext.Clients.Group(sessionId)
                                .SendAsync("ResponseGenerationProgress", chunk, messageId.ToString(), cancellationToken);
                            isFirstChunk = false;
                        }
                        else
                        {
                            await _hubContext.Clients.Group(sessionId)
                                .SendAsync("ResponseGenerationProgress", chunk, null, cancellationToken);
                        }
                    },
                    async fullText =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        backgroundMessage.IsPartial = false;
                        await backgroundDb.SaveChangesAsync(cancellationToken);
                        await _hubContext.Clients.Group(sessionId)
                            .SendAsync("ResponseGenerationCompleted", messageId.ToString(), cancellationToken);
                    });
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Generation cancelled for message {MessageId} in session {SessionId}", messageId, sessionId);
                using var scope = _scopeFactory.CreateScope();
                var backgroundDb = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var backgroundMessage = await backgroundDb.Messages.FindAsync(new object[] { messageId });
                if (backgroundMessage != null)
                {
                    backgroundMessage.IsPartial = false;
                    await backgroundDb.SaveChangesAsync();
                }
                await _hubContext.Clients.Group(sessionId).SendAsync("ResponseGenerationCancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error streaming response for message {MessageId} in session {SessionId}", messageId, sessionId);
                await _hubContext.Clients.Group(sessionId).SendAsync("Error", "An error occurred during message streaming.");
            }
            finally
            {
                if (_activeGenerations.TryRemove(sessionId, out var finalCts))
                {
                    finalCts.Dispose();
                }
            }
        }

        public void CancelGeneration(string sessionId)
        {
            if (_activeGenerations.TryRemove(sessionId, out var cts))
            {
                _logger.LogInformation("Cancelling generation for session {SessionId}", sessionId);
                cts.Cancel();
                cts.Dispose();
            }
        }
    }
} 