namespace ChatbotAI.Domain.DTOs
{
    /// <summary>
    /// Request to send a message within a session.
    /// </summary>
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
} 