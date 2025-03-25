using System.Text;
using System.Text.Json;

public class ClaudeProvider : IAIProvider
{
    private const string RequestUri = "https://api.anthropic.com/v1/messages";
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;
    private readonly string _backupModel;
    private readonly ILogger<EncounterSystem> _logger;

    // Retry configuration
    private const int MaxRetryAttempts = 10;
    private const int FallbackToBackupAfterAttempts = 5;
    private const int InitialDelayMilliseconds = 1000;
    private readonly Random _jitterer = new Random();

    public string Name => "Anthropic Claude";

    public ClaudeProvider(IConfiguration configuration, ILogger<EncounterSystem> logger)
    {
        _httpClient = new HttpClient();
        _apiKey = configuration.GetValue<string>("Anthropic:ApiKey");
        _logger = logger;

        bool _costSaving = configuration.GetValue<bool>("costSaving");
        if (!_costSaving)
        {
            _model = configuration.GetValue<string>("Anthropic:Model") ?? "claude-3-7-sonnet-latest";
            _backupModel = configuration.GetValue<string>("Anthropic:BackupModel") ?? "claude-3-5-haiku-latest";
        }
        else
        {
            _model = configuration.GetValue<string>("Anthropic:BackupModel") ?? "claude-3-5-haiku-latest";
            _backupModel = configuration.GetValue<string>("Anthropic:BackupModel") ?? "claude-3-5-haiku-latest";
        }

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

        // Format messages for Claude API
        var formattedMessages = messagesList.Select(m => new
        {
            role = m.Role.ToLower() == "assistant" ? "assistant" : "user",
            content = m.Content
        }).ToArray();

        return await ExecuteWithRetryAsync(formattedMessages, systemMessage);
    }

    private async Task<string> ExecuteWithRetryAsync(object[] messages, string systemMessage)
    {
        int attempts = 0;
        int delay = InitialDelayMilliseconds;
        string currentModel = _model; // Start with primary model

        while (true)
        {
            // Switch to backup model after specified number of attempts
            if (attempts >= FallbackToBackupAfterAttempts)
            {
                currentModel = _backupModel;
            }

            try
            {
                _logger?.LogInformation($"Sending request to Anthropic API (Attempt {attempts + 1}) using model: {currentModel}");

                // Format the request with the current model
                var requestBody = new
                {
                    model = currentModel,
                    messages = messages,
                    system = systemMessage,
                    max_tokens = 1500,
                    temperature = 0.7
                };

                StringContent content = new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    Encoding.UTF8,
                    "application/json"
                );

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
                else if (response.StatusCode.ToString() != "529")
                {
                    string content400 = await response.Content.ReadAsStringAsync();
                    _logger?.LogInformation($"Received response with status code: {content400}");
                    return string.Empty;
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
                _logger?.LogError($"HTTP Request Error (Attempt {attempts + 1}) with model {currentModel}: {ex.Message}");

                // If max attempts reached, rethrow
                if (++attempts >= MaxRetryAttempts)
                {
                    _logger?.LogError($"Final attempt failed. Inner Exception: {ex.InnerException?.Message}");
                    throw;
                }

                // Calculate exponential backoff with jitter
                delay = CalculateDelay(attempts, delay);

                _logger?.LogWarning($"Retrying in {delay} ms after HTTP error. Will use {(attempts >= FallbackToBackupAfterAttempts ? _backupModel : currentModel)}");
                await Task.Delay(delay);
            }
            catch (JsonException ex)
            {
                _logger?.LogError($"JSON Parsing Error with model {currentModel}: {ex.Message}");
                throw;
            }
            catch (KeyNotFoundException ex)
            {
                _logger?.LogError($"Key Not Found Error with model {currentModel}: {ex.Message}");
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