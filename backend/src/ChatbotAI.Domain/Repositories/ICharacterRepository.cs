using System.Threading.Tasks;

namespace ChatbotAI.Domain.Repositories
{
    public interface ICharacterRepository
    {
        Task<bool> ExistsAsync(int characterId);
    }
} 