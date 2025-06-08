using System;

namespace ChatbotAI.Domain.Models
{
    public class Message
    {
        public Guid Id { get; set; }
        public Guid SessionId { get; set; }
        public required string Content { get; set; }
        public bool IsFromUser { get; set; }
        public bool IsPartial { get; set; }
        public DateTime CreatedAt { get; set; }

        public int CharacterId { get; set; }
        public Character Character { get; set; } = null!;

        public MessageRating? Rating { get; set; }

        public Session? Session { get; set; }
    }
} 