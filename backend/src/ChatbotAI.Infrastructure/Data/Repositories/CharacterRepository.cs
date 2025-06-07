using System.Threading.Tasks;
using ChatbotAI.Domain.Repositories;
using ChatbotAI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChatbotAI.Infrastructure.Data.Repositories
{
    public class CharacterRepository : ICharacterRepository
    {
        private readonly ApplicationDbContext _context;
        public CharacterRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ExistsAsync(int characterId)
        {
            return await _context.Characters.AnyAsync(c => c.Id == characterId);
        }
    }
} 