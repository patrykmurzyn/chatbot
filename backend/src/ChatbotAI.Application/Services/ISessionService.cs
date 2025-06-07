using System;
using System.Threading.Tasks;
using ChatbotAI.Application.DTOs;

namespace ChatbotAI.Application.Services
{
    public interface ISessionService
    {
        Task<Guid> CreateSessionAsync(string userAgent, string ip);
        Task<SessionDto> GetSessionAsync(Guid sessionId);
    }
} 