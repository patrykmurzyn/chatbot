using System;
using System.Threading.Tasks;

namespace ChatbotAI.API.Services
{
    public interface IChatService
    {
        Task ReplayPendingMessagesAsync(Guid sessionId, string connectionId);
        Task StartGenerationAsync(Guid sessionId, string message, int characterId);
        void CancelGeneration(string sessionId);
    }
} 