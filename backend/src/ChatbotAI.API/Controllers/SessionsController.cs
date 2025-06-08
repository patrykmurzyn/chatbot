using ChatbotAI.Application.Services;
using ChatbotAI.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatbotAI.Domain.DTOs;

namespace ChatbotAI.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class SessionsController : ControllerBase
    {
        private readonly ISessionService _sessionService;
        private readonly IMessageService _messageService;

        public SessionsController(ISessionService sessionService, IMessageService messageService)
        {
            _sessionService = sessionService;
            _messageService = messageService;
        }

        // POST: api/v1/sessions
        [HttpPost]
        public async Task<ActionResult> CreateSession()
        {
            var userAgent = Request.Headers["User-Agent"].ToString();
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var sessionId = await _sessionService.CreateSessionAsync(userAgent, ip);
            return CreatedAtAction(nameof(GetSession), new { id = sessionId }, new { sessionId });
        }

        // GET: api/v1/sessions/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<SessionDto>> GetSession(Guid id)
        {
            try
            {
                var sessionDto = await _sessionService.GetSessionAsync(id);
                return Ok(sessionDto);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // GET: api/v1/sessions/{sessionId}/messages
        [HttpGet("{sessionId}/messages")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessages(Guid sessionId)
        {
            try
            {
                var messages = await _messageService.GetMessagesBySessionAsync(sessionId);
                return Ok(messages);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // POST: api/v1/sessions/{sessionId}/messages
        [HttpPost("{sessionId}/messages")]
        public async Task<ActionResult<MessageDto>> SendMessage(Guid sessionId, [FromBody] SendMessageRequest request)
        {
            try
            {
                var messageDto = await _messageService.SendMessageAsync(sessionId, request.Content, request.CharacterId);
                return CreatedAtAction(nameof(GetMessage), new { id = messageDto.Id }, messageDto);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET: api/v1/messages/{id}
        [HttpGet("~/api/v1/messages/{id}")]
        public async Task<ActionResult<MessageDto>> GetMessage(Guid id)
        {
            try
            {
                var messageDto = await _messageService.GetMessageByIdAsync(id);
                return Ok(messageDto);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // PUT: api/v1/messages/{messageId}/rating
        [HttpPut("~/api/v1/messages/{messageId}/rating")]
        public async Task<IActionResult> RateMessage(Guid messageId, [FromBody] RateMessageRequest request)
        {
            try
            {
                await _messageService.RateMessageAsync(messageId, request.IsPositive);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to rate message", details = ex.Message });
            }
        }
    }
} 