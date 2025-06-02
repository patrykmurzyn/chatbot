using System;
using System.Threading.Tasks;

namespace ChatbotAI.Application.Services
{
    /// <summary>
    /// Defines methods for streaming textual responses.
    /// </summary>
    public interface IStreamingService
    {
        /// <summary>
        /// Gets the full text synchronously, if supported.
        /// </summary>
        /// <returns>The complete text.</returns>
        string GetFullText();

        /// <summary>
        /// Streams text in chunks, invoking callbacks for each chunk and upon completion.
        /// </summary>
        /// <param name="prompt">The input prompt or query.</param>
        /// <param name="character">The character associated with the prompt.</param>
        /// <param name="chunkSize">The maximum number of characters per chunk.</param>
        /// <param name="delayMs">Delay in milliseconds between chunks (ignored if not supported).</param>
        /// <param name="onChunkReady">Callback invoked when a new chunk is available.</param>
        /// <param name="onComplete">Callback invoked when streaming is complete.</param>
        Task StreamTextAsync(string prompt, string character, int chunkSize, int delayMs, Func<string, Task> onChunkReady, Func<string, Task> onComplete);
    }
} 