using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatbotAI.Domain.Models;
using ChatbotAI.Domain.Repositories;
using ChatbotAI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChatbotAI.Infrastructure.Data.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private readonly ApplicationDbContext _context;
        public MessageRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Message>> GetMessagesBySessionIdAsync(Guid sessionId)
        {
            return await _context.Messages
                .Where(m => m.SessionId == sessionId)
                .Include(m => m.Rating)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();
        }

        public Task<Message> AddAsync(Message message)
        {
            _context.Messages.Add(message);
            return Task.FromResult(message);
        }

        public async Task<Message?> GetByIdAsync(Guid messageId)
        {
            return await _context.Messages
                .Include(m => m.Rating)
                .FirstOrDefaultAsync(m => m.Id == messageId);
        }
    }
} 