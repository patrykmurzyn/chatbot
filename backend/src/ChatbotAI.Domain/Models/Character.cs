using System.Collections.Generic;

namespace ChatbotAI.Domain.Models
{
    public class Character
    {
        public int Id { get; set; }
        public string Key { get; set; } = null!;
        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
} 