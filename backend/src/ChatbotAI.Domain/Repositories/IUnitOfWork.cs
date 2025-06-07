using System.Threading.Tasks;
using ChatbotAI.Domain.Repositories;

namespace ChatbotAI.Domain.Repositories
{
    public interface IUnitOfWork
    {
        ISessionRepository Sessions { get; }
        IMessageRepository Messages { get; }
        ICharacterRepository Characters { get; }
        IMessageRatingRepository MessageRatings { get; }
        Task<int> SaveChangesAsync();
    }
} 