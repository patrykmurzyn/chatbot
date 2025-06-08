using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ChatbotAI.Domain.DTOs;
using ChatbotAI.Domain.Repositories;

namespace ChatbotAI.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class CharactersController : ControllerBase
    {
        private readonly ICharacterRepository _repo;

        public CharactersController(ICharacterRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CharacterDto>>> Get()
        {
            var list = await _repo.GetAllAsync();
            return Ok(list);
        }
    }
} 