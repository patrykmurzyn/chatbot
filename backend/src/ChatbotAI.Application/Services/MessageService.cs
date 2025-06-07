using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatbotAI.Application.DTOs;
using ChatbotAI.Domain.Repositories;
using ChatbotAI.Domain.Models;

namespace ChatbotAI.Application.Services
{
    public class MessageService : IMessageService
    {
        private readonly IUnitOfWork _unitOfWork;

        public MessageService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<MessageDto>> GetMessagesBySessionAsync(Guid sessionId)
        {
            var exists = await _unitOfWork.Sessions.ExistsAsync(sessionId);
            if (!exists)
            {
                throw new KeyNotFoundException($"Session with ID {sessionId} not found.");
            }

            var messages = await _unitOfWork.Messages.GetMessagesBySessionIdAsync(sessionId);
            return messages.Select(m => new MessageDto
            {
                Id = m.Id,
                SessionId = m.SessionId,
                Content = m.Content,
                IsFromUser = m.IsFromUser,
                IsPartial = m.IsPartial,
                CreatedAt = m.CreatedAt,
                CharacterId = m.CharacterId,
                Rating = m.Rating != null ? m.Rating.IsPositive : (bool?)null
            });
        }

        public async Task<MessageDto> SendMessageAsync(Guid sessionId, string content, int characterId)
        {
            if (!await _unitOfWork.Sessions.ExistsAsync(sessionId))
                throw new KeyNotFoundException($"Session with ID {sessionId} not found.");
            if (!await _unitOfWork.Characters.ExistsAsync(characterId))
                throw new ArgumentException($"Character with ID {characterId} not found.");

            var message = new Message
            {
                Id = Guid.NewGuid(),
                SessionId = sessionId,
                Content = content,
                IsFromUser = true,
                IsPartial = false,
                CreatedAt = DateTime.UtcNow,
                CharacterId = characterId
            };

            var created = await _unitOfWork.Messages.AddAsync(message);
            await _unitOfWork.SaveChangesAsync();
            return new MessageDto
            {
                Id = created.Id,
                SessionId = created.SessionId,
                Content = created.Content,
                IsFromUser = created.IsFromUser,
                IsPartial = created.IsPartial,
                CreatedAt = created.CreatedAt,
                CharacterId = created.CharacterId,
                Rating = null
            };
        }

        public async Task<MessageDto> GetMessageByIdAsync(Guid messageId)
        {
            var message = await _unitOfWork.Messages.GetByIdAsync(messageId);
            if (message == null)
                throw new KeyNotFoundException($"Message with ID {messageId} not found.");
            return new MessageDto
            {
                Id = message.Id,
                SessionId = message.SessionId,
                Content = message.Content,
                IsFromUser = message.IsFromUser,
                IsPartial = message.IsPartial,
                CreatedAt = message.CreatedAt,
                CharacterId = message.CharacterId,
                Rating = message.Rating?.IsPositive
            };
        }

        public async Task RateMessageAsync(Guid messageId, bool isPositive)
        {
            var existingMessage = await _unitOfWork.Messages.GetByIdAsync(messageId);
            if (existingMessage == null)
                throw new KeyNotFoundException($"Message with ID {messageId} not found.");

            var existingRating = await _unitOfWork.MessageRatings.GetByMessageIdAsync(messageId);
            if (existingRating != null)
            {
                existingRating.IsPositive = isPositive;
                existingRating.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.MessageRatings.UpdateAsync(existingRating);
                await _unitOfWork.SaveChangesAsync();
            }
            else
            {
                var newRating = new MessageRating
                {
                    Id = Guid.NewGuid(),
                    MessageId = messageId,
                    IsPositive = isPositive,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.MessageRatings.AddAsync(newRating);
                await _unitOfWork.SaveChangesAsync();
            }
        }
    }
} 