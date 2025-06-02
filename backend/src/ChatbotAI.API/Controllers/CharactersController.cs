using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChatbotAI.Infrastructure.Data;

namespace ChatbotAI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CharactersController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public CharactersController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CharacterDto>>> Get()
        {
            var list = await _dbContext.Characters
                .Select(c => new CharacterDto { Id = c.Id, Key = c.Key })
                .ToListAsync();
            return Ok(list);
        }
    }

    public class CharacterDto
    {
        /// <summary>
        /// Identifier of the character.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Key (name) of the character.
        /// </summary>
        public string Key { get; set; } = null!;
    }
} 