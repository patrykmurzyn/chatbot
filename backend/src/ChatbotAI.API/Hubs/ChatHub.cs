using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using ChatbotAI.API.Services;
using Microsoft.Extensions.Logging;

namespace ChatbotAI.API.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(IChatService chatService, ILogger<ChatHub> logger)
        {
            _chatService = chatService;
            _logger = logger;
        }

        public async Task JoinSession(string sessionId)
        {
            if (!Guid.TryParse(sessionId, out var sessionGuid))
            {
                await Clients.Caller.SendAsync("Error", "Invalid session ID.");
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);
            await Clients.Caller.SendAsync("JoinedSession", sessionId);

            await _chatService.ReplayPendingMessagesAsync(sessionGuid, Context.ConnectionId);
        }

        public async Task SendMessage(string sessionId, string message, int characterId)
        {
            if (!Guid.TryParse(sessionId, out var sessionGuid))
            {
                await Clients.Caller.SendAsync("Error", "Invalid session ID.");
                return;
            }

            try
            {
                await _chatService.StartGenerationAsync(sessionGuid, message, characterId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating response for session {SessionId}", sessionId);
                await Clients.Group(sessionId).SendAsync("Error", "An error occurred while generating response.");
            }
        }

        public async Task CancelGeneration(string sessionId)
        {
            if (!Guid.TryParse(sessionId, out _))
            {
                await Clients.Caller.SendAsync("Error", "Invalid session ID.");
                return;
            }

            _chatService.CancelGeneration(sessionId);
            await Clients.Group(sessionId).SendAsync("ResponseGenerationCancelled");
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // Optionally handle cleanup or cancellation here
            await base.OnDisconnectedAsync(exception);
        }
    }
} 