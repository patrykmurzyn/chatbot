using System.Threading.Tasks;
using System.Collections.Generic;
using ChatbotAI.Domain.DTOs;

namespace ChatbotAI.Domain.Repositories
{
    public interface ICharacterRepository
    {
        Task<bool> ExistsAsync(int characterId);
        Task<IEnumerable<CharacterDto>> GetAllAsync();
    }
} 