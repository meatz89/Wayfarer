using System.Text;
using System.Text.Json;

public class OpenAIProvider : IAIProvider
{
    private const string RequestUri = "https://api.openai.com/v1/chat/completions";
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<EncounterSystem> _logger;

    // Minimal retry configuration
    private const int MaxRetryAttempts = 3;
    private const int InitialDelayMilliseconds = 500;

    public string Name
    {
        get
        {
            return "OpenAI";
        }
    }

    public OpenAIProvider(IConfiguration configuration, ILogger<EncounterSystem> logger)
    {
        _httpClient = new HttpClient();
        _apiKey = configuration.GetValue<string>("OpenAI:ApiKey");
        _logger = logger;

        // OpenAI header setup
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
    }

    public async Task<string> GetCompletionAsync(List<ConversationEntry> messages, string model, string fallbackModel)
    {
        // o4-mini doesn't need a fallback model since we're optimizing for speed
        return await ExecuteWithRetryAsync(messages, model);
    }

    private async Task<string> ExecuteWithRetryAsync(List<ConversationEntry> messages, string model)
    {
        int attempts = 0;
        int delay = InitialDelayMilliseconds;

        // Format for OpenAI - simple mapping from our generic format
        var formattedMessages = messages.Select(m =>
        {
            return new
            {
                role = m.Role.ToLower(),
                content = m.Content
            };
        }).ToArray();

        while (true)
        {
            try
            {
                // Format the request with minimal settings for speed
                var requestBody = new
                {
                    model = model,
                    messages = formattedMessages,
                    max_tokens = 4000,
                    temperature = 0.7
                };

                StringContent content = new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    Encoding.UTF8,
                    "application/json"
                );

                HttpResponseMessage response = await _httpClient.PostAsync(RequestUri, content);

                // Fast path for success
                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    JsonElement responseObject = JsonSerializer.Deserialize<JsonElement>(responseBody);
                    return responseObject.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
                }

                // Fast fail for client errors
                if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                {
                    return "Error: OpenAI API client error";
                }

                // Simple retry for server errors
                if (++attempts >= MaxRetryAttempts)
                {
                    return "Error: OpenAI API server error after retries";
                }

                await Task.Delay(delay);
                delay *= 2; // Simple exponential backoff
            }
            catch (Exception ex)
            {
                _logger?.LogError($"OpenAI API Error: {ex.Message}");
                return "Error: OpenAI API exception";
            }
        }
    }
}