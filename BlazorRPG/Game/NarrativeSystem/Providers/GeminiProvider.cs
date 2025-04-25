using System.Text;
using System.Text.Json;

public class GeminiProvider : IAIProvider
{
    private const string ApiBaseUrl = "https://generativelanguage.googleapis.com/v1beta/models";
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<EncounterSystem> _logger;

    // Minimal retry configuration for speed
    private const int MaxRetryAttempts = 3;
    private const int InitialDelayMilliseconds = 500;

    public string Name
    {
        get
        {
            return "Google Gemini";
        }
    }

    public GeminiProvider(IConfiguration configuration, ILogger<EncounterSystem> logger)
    {
        _httpClient = new HttpClient();
        _apiKey = configuration.GetValue<string>("Google:ApiKey");
        _logger = logger;
    }

    public async Task<string> GetCompletionAsync(List<ConversationEntry> messages, string model, string fallbackModel)
    {
        // For speed optimization, we ignore the fallback model
        return await ExecuteWithRetryAsync(messages, model);
    }

    private async Task<string> ExecuteWithRetryAsync(List<ConversationEntry> messages, string model)
    {
        int attempts = 0;
        int delay = InitialDelayMilliseconds;

        // Extract system message for Gemini (handled differently than in OpenAI)
        string systemMessage = "";
        List<object> formattedMessages = new List<object>();

        foreach (ConversationEntry message in messages)
        {
            if (message.Role.ToLower() == "system")
            {
                systemMessage = message.Content;
            }
            else
            {
                formattedMessages.Add(new
                {
                    role = message.Role.ToLower() == "assistant" ? "model" : "user",
                    parts = new[] { new { text = message.Content } }
                });
            }
        }

        while (true)
        {
            try
            {
                // Format the request with minimal settings for speed
                var requestBody = new
                {
                    contents = formattedMessages,
                    systemInstruction = new { text = systemMessage },
                    generationConfig = new
                    {
                        temperature = 0.7,
                        maxOutputTokens = 4000,
                    }
                };

                StringContent content = new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    Encoding.UTF8,
                    "application/json"
                );

                // Note the model and API key are in the URL for Gemini
                string requestUrl = $"{ApiBaseUrl}/{model}:generateContent?key={_apiKey}";
                HttpResponseMessage response = await _httpClient.PostAsync(requestUrl, content);

                // Fast path for success
                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    JsonElement responseObject = JsonSerializer.Deserialize<JsonElement>(responseBody);

                    // Extract the text response from Gemini's structure
                    return responseObject
                        .GetProperty("candidates")[0]
                        .GetProperty("content")
                        .GetProperty("parts")[0]
                        .GetProperty("text")
                        .GetString();
                }

                // Fast fail for client errors
                if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                {
                    return "Error: Gemini API client error";
                }

                // Simple retry for server errors
                if (++attempts >= MaxRetryAttempts)
                {
                    return "Error: Gemini API server error after retries";
                }

                await Task.Delay(delay);
                delay *= 2; // Simple exponential backoff
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Gemini API Error: {ex.Message}");
                return "Error: Gemini API exception";
            }
        }
    }
}