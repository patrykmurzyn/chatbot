using System;

namespace ChatbotAI.Application.DTOs
{
    public record MessageDto
    {
        public Guid Id { get; init; }
        public Guid SessionId { get; init; }
        public string Content { get; init; } = default!;
        public bool IsFromUser { get; init; }
        public bool IsPartial { get; init; }
        public DateTime CreatedAt { get; init; }
        public int CharacterId { get; init; }
        public bool? Rating { get; init; }
    }
} 