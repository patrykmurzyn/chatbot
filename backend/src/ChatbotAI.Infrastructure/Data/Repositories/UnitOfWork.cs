using System.Threading.Tasks;
using ChatbotAI.Domain.Repositories;
using ChatbotAI.Infrastructure.Data;

namespace ChatbotAI.Infrastructure.Data.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        public ISessionRepository Sessions { get; }
        public IMessageRepository Messages { get; }
        public ICharacterRepository Characters { get; }
        public IMessageRatingRepository MessageRatings { get; }

        public UnitOfWork(ApplicationDbContext context,
                          IMessageRepository messages,
                          ISessionRepository sessions,
                          ICharacterRepository characters,
                          IMessageRatingRepository messageRatings)
        {
            _context = context;
            Messages = messages;
            Sessions = sessions;
            Characters = characters;
            MessageRatings = messageRatings;
        }

        public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();
    }
} 