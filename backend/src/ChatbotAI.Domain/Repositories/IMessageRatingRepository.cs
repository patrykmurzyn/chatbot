using System;
using System.Threading.Tasks;
using ChatbotAI.Domain.Models;

namespace ChatbotAI.Domain.Repositories
{
    public interface IMessageRatingRepository
    {
        Task<MessageRating?> GetByMessageIdAsync(Guid messageId);
        Task AddAsync(MessageRating rating);
        Task UpdateAsync(MessageRating rating);
    }
} 