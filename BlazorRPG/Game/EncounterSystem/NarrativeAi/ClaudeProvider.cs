using System.Text;
using System.Text.Json;

public class ClaudeProvider : IAIProvider
{
    private const string RequestUri = "https://api.anthropic.com/v1/messages";
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;
    private readonly ILogger<EncounterSystem> _logger;

    // Retry configuration
    private const int MaxRetryAttempts = 10;
    private const int InitialDelayMilliseconds = 1000; // 1 second
    private readonly Random _jitterer = new Random();

    public string Name => "Anthropic Claude";

    public ClaudeProvider(IConfiguration configuration, ILogger<EncounterSystem> logger)
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
        List<ConversationEntry> messagesList = messages.ToList();

        // Find and extract system message
        ConversationEntry systemEntry = messagesList.FirstOrDefault(m => m.Role.ToLower() == "system");
        string systemMessage = null;
        if (systemEntry != null)
        {
            systemMessage = systemEntry.Content;
            messagesList.Remove(systemEntry);
        }

        // Format the request according to Anthropic's API requirements
        var requestBody = new
        {
            model = _model,
            messages = messagesList.Select(m => new
            {
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

        return await ExecuteWithRetryAsync(content);
    }

    private async Task<string> ExecuteWithRetryAsync(StringContent content)
    {
        int attempts = 0;
        int delay = InitialDelayMilliseconds;

        while (true)
        {
            try
            {
                _logger?.LogInformation($"Sending request to Anthropic API (Attempt {attempts + 1}) for model {_model}");
                HttpResponseMessage response = await TalkToClaude(content);
                _logger?.LogInformation($"Received response with status code: {response.StatusCode}");

                // Check if the request was successful
                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    _logger?.LogInformation($"Raw response first 100 chars: {responseBody.Substring(0, Math.Min(100, responseBody.Length))}...");

                    JsonElement responseObject = JsonSerializer.Deserialize<JsonElement>(responseBody);
                    return responseObject.GetProperty("content")[0].GetProperty("text").GetString();
                }

                // If not successful and max attempts reached, throw
                if (++attempts >= MaxRetryAttempts)
                {
                    response.EnsureSuccessStatusCode(); // This will throw an exception
                }

                // Calculate exponential backoff with jitter
                delay = CalculateDelay(attempts, delay);

                _logger?.LogWarning($"Request failed. Retrying in {delay} ms. Status code: {response.StatusCode}");
                await Task.Delay(delay);
            }
            catch (HttpRequestException ex)
            {
                _logger?.LogError($"HTTP Request Error (Attempt {attempts + 1}): {ex.Message}");

                // If max attempts reached, rethrow
                if (++attempts >= MaxRetryAttempts)
                {
                    _logger?.LogError($"Final attempt failed. Inner Exception: {ex.InnerException?.Message}");
                    throw;
                }

                // Calculate exponential backoff with jitter
                delay = CalculateDelay(attempts, delay);

                _logger?.LogWarning($"Retrying in {delay} ms after HTTP error");
                await Task.Delay(delay);
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

    /// <summary>
    /// Calculates delay with exponential backoff and adds jitter to prevent thundering herd problem
    /// </summary>
    /// <param name="attempt">Current retry attempt</param>
    /// <param name="currentDelay">Current delay</param>
    /// <returns>Calculated delay with jitter</returns>
    private int CalculateDelay(int attempt, int currentDelay)
    {
        // Exponential backoff
        int exponentialDelay = currentDelay * 2;

        // Add randomness to prevent synchronized retries
        double jitterFactor = 1 + (0.1 * _jitterer.NextDouble());

        return (int)(exponentialDelay * jitterFactor);
    }

    /// <summary>
    /// Talk to Claude
    /// </summary>
    /// <param name="content">Request content</param>
    /// <returns>HTTP response</returns>
    private async Task<HttpResponseMessage> TalkToClaude(StringContent content)
    {
        return await _httpClient.PostAsync(RequestUri, content);
    }
}