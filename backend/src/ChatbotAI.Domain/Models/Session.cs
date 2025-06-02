using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ChatbotAI.Domain.Models
{
    public class Session
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastActivity { get; set; }
        
        [NotMapped]
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
        
        public string MetadataJson 
        { 
            get => JsonSerializer.Serialize(Metadata);
            set => Metadata = value == null 
                ? new Dictionary<string, string>() 
                : JsonSerializer.Deserialize<Dictionary<string, string>>(value) ?? new Dictionary<string, string>();
        }
        
        [JsonIgnore]
        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
} 