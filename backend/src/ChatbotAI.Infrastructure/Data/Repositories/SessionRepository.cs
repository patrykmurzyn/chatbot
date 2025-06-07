using System;
using System.Threading.Tasks;
using ChatbotAI.Domain.Repositories;
using ChatbotAI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ChatbotAI.Domain.Models;

namespace ChatbotAI.Infrastructure.Data.Repositories
{
    public class SessionRepository : ISessionRepository
    {
        private readonly ApplicationDbContext _context;
        public SessionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ExistsAsync(Guid sessionId)
        {
            return await _context.Sessions.AnyAsync(s => s.Id == sessionId);
        }

        public Task<Session> CreateAsync(Session session)
        {
            _context.Sessions.Add(session);
            return Task.FromResult(session);
        }

        public async Task<Session?> GetByIdWithMessagesAsync(Guid sessionId)
        {
            return await _context.Sessions
                .Include(s => s.Messages)
                    .ThenInclude(m => m.Rating)
                .FirstOrDefaultAsync(s => s.Id == sessionId);
        }

        public Task UpdateAsync(Session session)
        {
            _context.Sessions.Update(session);
            return Task.CompletedTask;
        }
    }
} 