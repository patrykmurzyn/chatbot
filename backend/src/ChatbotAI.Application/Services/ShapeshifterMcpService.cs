using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChatbotAI.Application.Services
{
    /// <summary>
    /// Streams data from the MCP Shapeshifter endpoint using server-sent events.
    /// </summary>
    public class ShapeshifterMcpService : IStreamingService
    {
        private readonly HttpClient _httpClient;

        public ShapeshifterMcpService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public string GetFullText()
        {
            throw new NotSupportedException("GetFullText is not supported by ShapeshifterMcpService.");
        }

        public async Task StreamTextAsync(string prompt, string character, int chunkSize, int delayMs, Func<string, Task> onChunkReady, Func<string, Task> onComplete)
        {
            // Build JSON-RPC payload
            var rpcRequest = new
            {
                jsonrpc = "2.0",
                method = "tools/call",
                @params = new
                {
                    name = "ask-perplexity-stream",
                    arguments = new
                    {
                        question = prompt,
                        character = character
                    }
                },
                id = 1
            };
            var content = new StringContent(JsonSerializer.Serialize(rpcRequest), Encoding.UTF8, "application/json");

            using var request = new HttpRequestMessage(HttpMethod.Post, "/mcp")
            {
                Content = content
            };
            request.Headers.Accept.Clear();
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (line == null)
                    break;

                if (line.StartsWith("data:"))
                {
                    var json = line.Substring("data:".Length).Trim();
                    if (string.IsNullOrWhiteSpace(json))
                        continue;

                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;

                    // Stream chunk events
                    if (root.TryGetProperty("method", out var methodProp)
                        && methodProp.GetString() == "perplexity/stream-chunk")
                    {
                        var chunk = root.GetProperty("params").GetProperty("chunk").GetString();
                        if (!string.IsNullOrEmpty(chunk))
                        {
                            await onChunkReady(chunk);
                        }
                    }
                    // Completion event
                    else if (root.TryGetProperty("result", out _))
                    {
                        await onComplete(string.Empty);
                        break;
                    }
                }
            }
        }
    }
} 