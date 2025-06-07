using ChatbotAI.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatbotAI.Application.Services
{
    public interface IMessageService
    {
        Task<IEnumerable<MessageDto>> GetMessagesBySessionAsync(Guid sessionId);
        Task<MessageDto> SendMessageAsync(Guid sessionId, string content, int characterId);
        Task<MessageDto> GetMessageByIdAsync(Guid messageId);
        Task RateMessageAsync(Guid messageId, bool isPositive);
    }
} 