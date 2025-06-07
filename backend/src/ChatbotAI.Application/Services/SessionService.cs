using System;
using System.Linq;
using System.Threading.Tasks;
using ChatbotAI.Application.DTOs;
using ChatbotAI.Domain.Repositories;
using ChatbotAI.Domain.Models;

namespace ChatbotAI.Application.Services
{
    public class SessionService : ISessionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public SessionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> CreateSessionAsync(string userAgent, string ip)
        {
            var session = new Session
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                LastActivity = DateTime.UtcNow
            };
            session.Metadata["UserAgent"] = userAgent;
            session.Metadata["IP"] = ip;

            await _unitOfWork.Sessions.CreateAsync(session);
            await _unitOfWork.SaveChangesAsync();
            return session.Id;
        }

        public async Task<SessionDto> GetSessionAsync(Guid sessionId)
        {
            var session = await _unitOfWork.Sessions.GetByIdWithMessagesAsync(sessionId);
            if (session == null)
                throw new KeyNotFoundException($"Session with ID {sessionId} not found.");

            session.Messages = session.Messages.OrderBy(m => m.CreatedAt).ToList();
            session.LastActivity = DateTime.UtcNow;
            await _unitOfWork.Sessions.UpdateAsync(session);
            await _unitOfWork.SaveChangesAsync();

            return new SessionDto
            {
                Id = session.Id,
                CreatedAt = session.CreatedAt,
                LastActivity = session.LastActivity,
                Metadata = session.Metadata,
                Messages = session.Messages.Select(m => new MessageDto
                {
                    Id = m.Id,
                    SessionId = m.SessionId,
                    Content = m.Content,
                    IsFromUser = m.IsFromUser,
                    IsPartial = m.IsPartial,
                    CreatedAt = m.CreatedAt,
                    CharacterId = m.CharacterId,
                    Rating = m.Rating?.IsPositive
                })
            };
        }
    }
} 