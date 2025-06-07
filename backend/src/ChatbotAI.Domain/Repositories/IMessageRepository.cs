using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatbotAI.Domain.Models;

namespace ChatbotAI.Domain.Repositories
{
    public interface IMessageRepository
    {
        Task<IEnumerable<Message>> GetMessagesBySessionIdAsync(Guid sessionId);
        Task<Message> AddAsync(Message message);
        Task<Message?> GetByIdAsync(Guid messageId);
    }
} 