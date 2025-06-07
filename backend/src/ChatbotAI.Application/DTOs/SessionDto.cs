using System;
using System.Collections.Generic;

namespace ChatbotAI.Application.DTOs
{
    public record SessionDto
    {
        public Guid Id { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime LastActivity { get; init; }
        public Dictionary<string, string> Metadata { get; init; } = new();
        public IEnumerable<MessageDto> Messages { get; init; } = Array.Empty<MessageDto>();
    }
} 