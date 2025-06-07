namespace ChatbotAI.API
{
    public class ChatSettings
    {
        /// <summary>
        /// Number of tokens per chunk when streaming responses.
        /// </summary>
        public int DefaultChunkSize { get; set; } = 3;

        /// <summary>
        /// Delay in milliseconds between streaming chunks.
        /// </summary>
        public int DefaultDelayMs { get; set; } = 100;

        /// <summary>
        /// Maximum allowed length for user messages.
        /// </summary>
        public int MaxUserMessageLength { get; set; } = 1000;

        /// <summary>
        /// Use the MCP ShapeshifterService for streaming instead of LoremIpsumService.
        /// </summary>
        public bool UseMcpService { get; set; } = true;

        /// <summary>
        /// Base address for the MCP Shapeshifter service.
        /// </summary>
        public string McpBaseUrl { get; set; } = "http://localhost:3000";
    }
} 