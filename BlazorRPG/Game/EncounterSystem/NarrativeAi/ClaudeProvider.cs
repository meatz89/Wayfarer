using System.Text;
using System.Text.Json;

public class ClaudeProvider : IAIProvider
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;
    private readonly ILogger<ClaudeProvider> _logger;

    public string Name => "Anthropic Claude";

    public ClaudeProvider(IConfiguration configuration, ILogger<ClaudeProvider> logger = null)
    {
        _httpClient = new HttpClient();
        _apiKey = configuration.GetValue<string>("Anthropic:ApiKey");
        _model = configuration.GetValue<string>("Anthropic:Model") ?? "claude-3-5-haiku-latest";
        _logger = logger;

        // Anthropic uses different headers than OpenAI and Gemini
        _httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);
        _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
    }

    public async Task<string> GetCompletionAsync(IEnumerable<ConversationEntry> messages)
    {
        // Extract system message if present
        string systemMessage = null;
        List<ConversationEntry> messagesList = messages.ToList();

        // Find and extract system message
        ConversationEntry systemEntry = messagesList.FirstOrDefault(m => m.Role.ToLower() == "system");
        if (systemEntry != null)
        {
            systemMessage = systemEntry.Content;
            messagesList.Remove(systemEntry);
        }

        // Format the request according to Anthropic's API requirements
        var requestBody = new
        {
            model = _model,
            messages = messagesList.Select(m => new {
                role = m.Role.ToLower() == "assistant" ? "assistant" : "user",
                content = m.Content
            }).ToArray(),
            system = systemMessage,
            max_tokens = 500,
            temperature = 0.7
        };

        StringContent content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json"
        );

        try
        {
            _logger?.LogInformation($"Sending request to Anthropic API for model {_model}");
            HttpResponseMessage response = await _httpClient.PostAsync("https://api.anthropic.com/v1/messages", content);

            _logger?.LogInformation($"Received response with status code: {response.StatusCode}");

            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            _logger?.LogInformation($"Raw response first 100 chars: {responseBody.Substring(0, Math.Min(100, responseBody.Length))}...");

            JsonElement responseObject = JsonSerializer.Deserialize<JsonElement>(responseBody);
            return responseObject.GetProperty("content")[0].GetProperty("text").GetString();
        }
        catch (HttpRequestException ex)
        {
            _logger?.LogError($"HTTP Request Error: {ex.Message}");
            _logger?.LogError($"Inner Exception: {ex.InnerException?.Message}");
            throw;
        }
        catch (JsonException ex)
        {
            _logger?.LogError($"JSON Parsing Error: {ex.Message}");
            throw;
        }
        catch (KeyNotFoundException ex)
        {
            _logger?.LogError($"Key Not Found Error: {ex.Message}");
            throw;
        }
    }
}