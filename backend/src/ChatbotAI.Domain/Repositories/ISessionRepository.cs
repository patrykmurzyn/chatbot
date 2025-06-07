using System;
using System.Threading.Tasks;
using ChatbotAI.Domain.Models;

namespace ChatbotAI.Domain.Repositories
{
    public interface ISessionRepository
    {
        Task<bool> ExistsAsync(Guid sessionId);
        Task<Session> CreateAsync(Session session);
        Task<Session?> GetByIdWithMessagesAsync(Guid sessionId);
        Task UpdateAsync(Session session);
    }
} 