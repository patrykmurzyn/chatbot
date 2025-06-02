using ChatbotAI.Domain.Models;
using ChatbotAI.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatbotAI.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class SessionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SessionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: api/v1/sessions
        [HttpPost]
        public async Task<ActionResult<Session>> CreateSession()
        {
            var session = new Session
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                LastActivity = DateTime.UtcNow
            };
            
            // Add metadata
            session.Metadata["UserAgent"] = Request.Headers["User-Agent"].ToString();
            session.Metadata["IP"] = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            _context.Sessions.Add(session);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSession), new { id = session.Id }, new { sessionId = session.Id });
        }

        // GET: api/v1/sessions/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Session>> GetSession(Guid id)
        {
            var session = await _context.Sessions
                .Include(s => s.Messages)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (session == null)
            {
                return NotFound();
            }

            // Ensure messages are in chronological order
            session.Messages = session.Messages.OrderBy(m => m.CreatedAt).ToList();

            // Update last activity timestamp
            session.LastActivity = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return session;
        }

        // GET: api/v1/sessions/{sessionId}/messages
        [HttpGet("{sessionId}/messages")]
        public async Task<ActionResult<IEnumerable<Message>>> GetMessages(Guid sessionId)
        {
            // Check session existence
            var exists = await _context.Sessions.AnyAsync(s => s.Id == sessionId);
            if (!exists)
            {
                return NotFound();
            }

            // Fetch and return messages in chronological order
            var messages = await _context.Messages
                .Where(m => m.SessionId == sessionId)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();

            return Ok(messages);
        }

        // POST: api/v1/sessions/{sessionId}/messages
        [HttpPost("{sessionId}/messages")]
        public async Task<ActionResult<Message>> SendMessage(Guid sessionId, [FromBody] SendMessageRequest request)
        {
            var session = await _context.Sessions.FindAsync(sessionId);

            if (session == null)
            {
                return NotFound();
            }

            // Validate characterId exists
            if (!await _context.Characters.AnyAsync(c => c.Id == request.CharacterId))
            {
                return BadRequest($"Character with ID '{request.CharacterId}' not found.");
            }
            var message = new Message
            {
                Id = Guid.NewGuid(),
                SessionId = sessionId,
                Content = request.Content,
                IsFromUser = true,
                IsPartial = false,
                CreatedAt = DateTime.UtcNow,
                CharacterId = request.CharacterId
            };

            _context.Messages.Add(message);
            
            // Update session's last activity
            session.LastActivity = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMessage), new { id = message.Id }, message);
        }

        // GET: api/v1/messages/{id}
        [HttpGet("~/api/v1/messages/{id}")]
        public async Task<ActionResult<Message>> GetMessage(Guid id)
        {
            var message = await _context.Messages.FindAsync(id);

            if (message == null)
            {
                return NotFound();
            }

            return message;
        }

        // PUT: api/v1/messages/{messageId}/rating
        [HttpPut("~/api/v1/messages/{messageId}/rating")]
        public async Task<IActionResult> RateMessage(Guid messageId, [FromBody] RateMessageRequest request)
        {
            try
            {
                // First, check if the message exists
                var messageExists = await _context.Messages.AnyAsync(m => m.Id == messageId);
                if (!messageExists)
                {
                    return NotFound($"Message with ID {messageId} not found.");
                }

                // Check if a rating already exists
                var existingRating = await _context.Set<MessageRating>()
                    .FirstOrDefaultAsync(r => r.MessageId == messageId);

                if (existingRating != null)
                {
                    // Update existing rating
                    existingRating.IsPositive = request.IsPositive;
                    existingRating.UpdatedAt = DateTime.UtcNow;
                    _context.Update(existingRating);
                }
                else
                {
                    // Create new rating
                    var newRating = new MessageRating
                    {
                        Id = Guid.NewGuid(),
                        MessageId = messageId,
                        IsPositive = request.IsPositive,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Add(newRating);
                }

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error rating message {messageId}: {ex}");
                return StatusCode(500, new { error = "Failed to rate message", details = ex.Message });
            }
        }
    }

    public class SendMessageRequest
    {
        /// <summary>
        /// The content of the user message.
        /// </summary>
        public required string Content { get; set; }

        /// <summary>
        /// The ID of the character selected by the user.
        /// </summary>
        public required int CharacterId { get; set; }
    }

    public class RateMessageRequest
    {
        public bool IsPositive { get; set; }
    }
} 