using System.Threading.Tasks;
using ChatbotAI.Domain.Repositories;
using ChatbotAI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using ChatbotAI.Domain.DTOs;

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

        public async Task<IEnumerable<CharacterDto>> GetAllAsync()
        {
            return await _context.Characters
                .Select(c => new CharacterDto { Id = c.Id, Key = c.Key })
                .ToListAsync();
        }
    }
} 