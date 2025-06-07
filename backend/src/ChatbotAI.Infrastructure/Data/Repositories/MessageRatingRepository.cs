using System;
using System.Threading.Tasks;
using ChatbotAI.Domain.Models;
using ChatbotAI.Domain.Repositories;
using ChatbotAI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChatbotAI.Infrastructure.Data.Repositories
{
    public class MessageRatingRepository : IMessageRatingRepository
    {
        private readonly ApplicationDbContext _context;
        public MessageRatingRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<MessageRating?> GetByMessageIdAsync(Guid messageId)
        {
            return await _context.MessageRatings
                .FirstOrDefaultAsync(r => r.MessageId == messageId);
        }

        public Task AddAsync(MessageRating rating)
        {
            _context.MessageRatings.Add(rating);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(MessageRating rating)
        {
            _context.MessageRatings.Update(rating);
            return Task.CompletedTask;
        }
    }
} 