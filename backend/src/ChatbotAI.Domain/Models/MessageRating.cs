using System;

namespace ChatbotAI.Domain.Models
{
    public class MessageRating
    {
        public Guid Id { get; set; }
        public Guid MessageId { get; set; }
        public bool IsPositive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Message? Message { get; set; }
    }
} 