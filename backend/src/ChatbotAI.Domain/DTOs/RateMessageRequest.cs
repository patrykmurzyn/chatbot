namespace ChatbotAI.Domain.DTOs
{
    /// <summary>
    /// Request to rate a message.
    /// </summary>
    public class RateMessageRequest
    {
        /// <summary>
        /// Indicates whether the rating is positive.
        /// </summary>
        public bool IsPositive { get; set; }
    }
} 