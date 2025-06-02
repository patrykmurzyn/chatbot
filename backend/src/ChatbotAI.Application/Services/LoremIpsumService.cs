using System;
using System.Text;
using System.Threading.Tasks;

namespace ChatbotAI.Application.Services
{
    public class LoremIpsumService : IStreamingService
    {
        private readonly string _loremIpsumText = @"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Fusce eu fringilla lectus. Suspendisse potenti. Etiam tristique id ante vel fringilla. Donec egestas mi id nisl porttitor, nec sagittis leo lacinia. Fusce laoreet ac nunc vitae posuere. Pellentesque in mollis leo. Nullam rhoncus scelerisque tellus, et dictum eros molestie et. Vestibulum malesuada ac augue auctor sollicitudin. Proin ultrices vitae mauris eget aliquet. Sed nec eleifend nisi. Curabitur luctus magna ut risus bibendum, porttitor tempus ligula maximus. Etiam bibendum enim leo, quis maximus purus pharetra sed. Morbi tristique elit nec ante bibendum maximus. Nulla efficitur vel ex ut faucibus.

Aliquam blandit ipsum eget ex porta tincidunt. Nulla venenatis fermentum placerat. In mollis tellus quis mattis lacinia. Integer ultrices molestie elit, a fermentum sapien semper porta. Maecenas quis sapien maximus, dapibus nibh pharetra, faucibus ligula. Nulla quis risus et nisi aliquet pellentesque. Nam eget vestibulum dui. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Pellentesque metus turpis, laoreet eleifend sagittis sed, egestas id nulla.";

        public string GetFullText()
        {
            return _loremIpsumText;
        }

        public async Task StreamTextAsync(
            string prompt,
            string character,
            int chunkSize,
            int delayMs,
            Func<string, Task> onChunkReady,
            Func<string, Task> onComplete)
        {
            var text = _loremIpsumText;
            var currentIndex = 0;

            while (currentIndex < text.Length)
            {
                int remainingChars = text.Length - currentIndex;
                int charsToTake = Math.Min(chunkSize, remainingChars);
                
                string chunk = text.Substring(currentIndex, charsToTake);
                await onChunkReady(chunk);
                
                currentIndex += charsToTake;
                
                // Add delay between chunks
                if (currentIndex < text.Length)
                {
                    await Task.Delay(delayMs);
                }
            }

            // Signal completion with the full text
            await onComplete(text);
        }
    }
} 