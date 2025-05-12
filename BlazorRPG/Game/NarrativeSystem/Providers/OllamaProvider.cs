using System.Text.Json;
using System.Text;

public class OllamaProvider : IAIProvider
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _modelName = "gemma3:12b-it-qat";
    private readonly string _fallbackModel = "gemma3:2b-it";

    public string Name => "Ollama";

    public OllamaProvider(string baseUrl)
    {
        _baseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));

        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromMinutes(5);
    }

    public async Task<string> GetCompletionAsync(
        List<ConversationEntry> messages,
        string model,
        string fallbackModel,
        IResponseStreamWatcher watcher)
    {
        string endpoint = $"{_baseUrl}/api/chat";
        string actualModel = string.IsNullOrEmpty(model) ? _modelName : model;

        if (string.IsNullOrEmpty(actualModel))
        {
            actualModel = string.IsNullOrEmpty(fallbackModel) ? _fallbackModel : fallbackModel;
        }

        List<OllamaMessage> ollamaMessages = new List<OllamaMessage>();
        foreach (ConversationEntry entry in messages)
        {
            ollamaMessages.Add(new OllamaMessage
            {
                Role = entry.Role,
                Content = entry.Content
            });
        }

        OllamaChatRequest request = new OllamaChatRequest
        {
            Model = actualModel,
            Messages = ollamaMessages,
            Stream = watcher != null, // Use streaming if we have a watcher
            Options = new OllamaOptions
            {
                Temperature = 0.7,
                TopP = 0.9,
                NumPredict = 1024
            }
        };

        string jsonRequest = JsonSerializer.Serialize(request, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        try
        {
            // If streaming is enabled, handle it differently
            if (watcher != null)
            {
                return await StreamCompletionAsync(endpoint, jsonRequest, watcher);
            }
            else
            {
                HttpResponseMessage response = await _httpClient.PostAsync(
                    endpoint,
                    new StringContent(jsonRequest, Encoding.UTF8, "application/json")
                );

                response.EnsureSuccessStatusCode();
                string jsonResponse = await response.Content.ReadAsStringAsync();
                string content = OllamaResponseParser.ExtractMessageContent(jsonResponse);
                return content;
            }
        }
        catch (Exception ex)
        {
            watcher?.OnError(ex);
            throw;
        }
    }

    private async Task<string> StreamCompletionAsync(
        string endpoint,
        string jsonRequest,
        IResponseStreamWatcher watcher)
    {
        using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new StringContent(jsonRequest, Encoding.UTF8, "application/json")
        };

        using HttpResponseMessage response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead);

        response.EnsureSuccessStatusCode();

        using Stream stream = await response.Content.ReadAsStreamAsync();
        using StreamReader reader = new StreamReader(stream);

        StringBuilder fullResponseBuilder = new StringBuilder();
        string fullMessageContent = string.Empty;

        // Process the stream line by line
        while (!reader.EndOfStream)
        {
            string line = await reader.ReadLineAsync();

            if (string.IsNullOrEmpty(line) || line == "[DONE]")
                continue;

            try
            {
                // Each line is a JSON object
                using JsonDocument doc = JsonDocument.Parse(line);
                JsonElement root = doc.RootElement;

                if (root.TryGetProperty("message", out JsonElement messageElement) &&
                    messageElement.TryGetProperty("content", out JsonElement contentElement))
                {
                    string chunk = contentElement.GetString() ?? string.Empty;
                    fullResponseBuilder.Append(chunk);

                    // Report the chunk to the watcher
                    watcher.OnResponseChunk(chunk);
                }

                // When we reach a "done": true message, we have the complete response
                if (root.TryGetProperty("done", out JsonElement doneElement) &&
                    doneElement.ValueKind == JsonValueKind.True)
                {
                    fullMessageContent = fullResponseBuilder.ToString();
                }
            }
            catch (JsonException)
            {
                // Skip malformed JSON lines
                continue;
            }
        }

        string finalResponse = fullMessageContent;
        watcher.OnResponseComplete(finalResponse);
        return finalResponse;
    }
}

public class OllamaMessage
{
    public string Role { get; set; }
    public string Content { get; set; }
}

public class OllamaChatRequest
{
    public string Model { get; set; }
    public List<OllamaMessage> Messages { get; set; }
    public bool Stream { get; set; }
    public OllamaOptions Options { get; set; }
}

public class OllamaOptions
{
    public double Temperature { get; set; }
    public double TopP { get; set; }
    public int NumPredict { get; set; }
}

public class OllamaChatResponse
{
    public OllamaMessage Message { get; set; }
    public string Model { get; set; }
    public string Created_at { get; set; }
    public int Done { get; set; }
}