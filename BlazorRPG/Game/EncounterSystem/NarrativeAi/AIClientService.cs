using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

public class AIClientService
{
    private readonly HttpClient _httpClient;
    private readonly string _modelName;
    private readonly string _apiKey;
    private readonly NarrativeLogManager _logManager;
    private readonly ILogger<AIClientService> _logger;

    // Rate limiting parameters
    private readonly SemaphoreSlim _throttleSemaphore;
    private readonly int _maxConcurrentRequests;
    private readonly int _maxRequestsPerMinute;
    private readonly Queue<DateTime> _requestTimestamps;
    private readonly TimeSpan _retryDelay = TimeSpan.FromSeconds(2);
    private readonly int _maxRetries = 5;

    public AIClientService(IConfiguration configuration, string gameInstanceId = null, ILogger<AIClientService> logger = null)
    {
        _httpClient = new HttpClient();
        _modelName = configuration.GetValue<string>("OpenAI:ModelName") ?? "gpt-4";
        _apiKey = configuration.GetValue<string>("OpenAI:ApiKey");
        _logManager = new NarrativeLogManager(gameInstanceId);
        _logger = logger;

        // Initialize rate limiting
        _maxConcurrentRequests = configuration.GetValue<int>("OpenAI:MaxConcurrentRequests", 2);
        _maxRequestsPerMinute = configuration.GetValue<int>("OpenAI:MaxRequestsPerMinute", 20);
        _throttleSemaphore = new SemaphoreSlim(_maxConcurrentRequests, _maxConcurrentRequests);
        _requestTimestamps = new Queue<DateTime>();
    }

    public async Task<string> GetCompletionAsync(List<ConversationEntry> conversationHistory)
    {
        // Determine conversation ID for logging
        string conversationId = ExtractConversationId(conversationHistory);

        // Check the type of request for potential fallback
        string requestType = DetermineRequestType(conversationHistory);

        // Try to acquire semaphore to respect concurrent request limit
        if (!await _throttleSemaphore.WaitAsync(TimeSpan.FromSeconds(30)))
        {
            _logger?.LogWarning("Failed to acquire request semaphore after 30 seconds - using fallback response");

            // Log the failure
            await _logManager.LogApiInteractionAsync(
                conversationId,
                conversationHistory,
                new { model = _modelName, error = "Failed to acquire request semaphore" },
                null,
                GetFallbackResponse(requestType),
                "Request timeout - could not acquire semaphore"
            );

            return GetFallbackResponse(requestType);
        }

        try
        {
            // Apply rate limiting based on requests per minute
            await EnforceRateLimitAsync();

            // Prepare API messages from conversation history
            var messages = conversationHistory.Select(m => new { role = m.Role, content = m.Content }).ToArray();

            var requestBody = new
            {
                model = _modelName,
                messages = messages,
                temperature = 0.7,
                max_tokens = 500 // Limit response size for speed
            };

            // Prepare API call
            StringContent content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            // Implement retry logic with exponential backoff
            int retryAttempt = 0;
            HttpResponseMessage response = null;
            string jsonResponse = string.Empty;

            while (retryAttempt <= _maxRetries)
            {
                try
                {
                    // Track request time for rate limiting
                    lock (_requestTimestamps)
                    {
                        _requestTimestamps.Enqueue(DateTime.UtcNow);

                        // Clean up old timestamps
                        while (_requestTimestamps.Count > 0 &&
                              (DateTime.UtcNow - _requestTimestamps.Peek()).TotalMinutes > 1)
                        {
                            _requestTimestamps.Dequeue();
                        }
                    }

                    // Make the API call
                    response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);

                    // Success! Break the retry loop
                    if (response.IsSuccessStatusCode)
                    {
                        jsonResponse = await response.Content.ReadAsStringAsync();
                        break;
                    }

                    // Handle rate limiting errors specifically
                    if (response.StatusCode == HttpStatusCode.TooManyRequests)
                    {
                        retryAttempt++;
                        if (retryAttempt <= _maxRetries)
                        {
                            // Parse retry-after header if available, or use exponential backoff
                            TimeSpan delay = _retryDelay;
                            if (response.Headers.TryGetValues("Retry-After", out var values) &&
                                int.TryParse(values.FirstOrDefault(), out int seconds))
                            {
                                delay = TimeSpan.FromSeconds(seconds);
                            }
                            else
                            {
                                // Exponential backoff with jitter
                                int delayMilliseconds = (int)Math.Pow(2, retryAttempt) * 1000;
                                delayMilliseconds += new Random().Next(0, 1000); // Add jitter
                                delay = TimeSpan.FromMilliseconds(delayMilliseconds);
                            }

                            _logger?.LogWarning($"Rate limited by OpenAI API. Retrying in {delay.TotalSeconds} seconds (attempt {retryAttempt}/{_maxRetries})");
                            await Task.Delay(delay);
                            continue;
                        }
                    }

                    // Other error, log it
                    string errorContent = await response.Content.ReadAsStringAsync();
                    _logger?.LogError($"OpenAI API error: {response.StatusCode} - {errorContent}");

                    // If we're not retrying, break out of the loop
                    if (retryAttempt >= _maxRetries)
                    {
                        break;
                    }

                    // Backoff for other errors
                    retryAttempt++;
                    await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Exception during OpenAI API call");

                    if (retryAttempt >= _maxRetries)
                    {
                        // Log the failure with the exception
                        await _logManager.LogApiInteractionAsync(
                            conversationId,
                            conversationHistory,
                            requestBody,
                            null,
                            GetFallbackResponse(requestType),
                            $"Exception after {retryAttempt} retries: {ex.Message}"
                        );

                        return GetFallbackResponse(requestType);
                    }

                    retryAttempt++;
                    await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
                }
            }

            // Process successful response
            if (response != null && response.IsSuccessStatusCode)
            {
                try
                {
                    string generatedContent = string.Empty;

                    using (JsonDocument document = JsonDocument.Parse(jsonResponse))
                    {
                        JsonElement root = document.RootElement;
                        JsonElement choices = root.GetProperty("choices");
                        JsonElement firstChoice = choices[0];
                        JsonElement message = firstChoice.GetProperty("message");
                        generatedContent = message.GetProperty("content").GetString() ?? string.Empty;
                    }

                    // Log the successful interaction
                    await _logManager.LogApiInteractionAsync(
                        conversationId,
                        conversationHistory,
                        requestBody,
                        jsonResponse,
                        generatedContent
                    );

                    return generatedContent;
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error parsing OpenAI API response");

                    // Log the failure when parsing the response
                    await _logManager.LogApiInteractionAsync(
                        conversationId,
                        conversationHistory,
                        requestBody,
                        jsonResponse,
                        GetFallbackResponse(requestType),
                        $"Error parsing response: {ex.Message}"
                    );

                    return GetFallbackResponse(requestType);
                }
            }

            // If we got here, all retries failed
            string errorMessage = response != null ?
                $"{response.StatusCode}" :
                "Failed to connect to OpenAI API";

            _logger?.LogError($"All OpenAI API requests failed: {errorMessage}");

            // Log the failed request
            string responseContent = response != null ? await response.Content.ReadAsStringAsync() : null;
            await _logManager.LogApiInteractionAsync(
                conversationId,
                conversationHistory,
                requestBody,
                responseContent,
                GetFallbackResponse(requestType),
                $"All retries failed: {errorMessage}"
            );

            return GetFallbackResponse(requestType);
        }
        finally
        {
            // Always release the semaphore
            _throttleSemaphore.Release();
        }
    }

    private async Task EnforceRateLimitAsync()
    {
        lock (_requestTimestamps)
        {
            // Clean up old timestamps
            while (_requestTimestamps.Count > 0 &&
                  (DateTime.UtcNow - _requestTimestamps.Peek()).TotalMinutes > 1)
            {
                _requestTimestamps.Dequeue();
            }

            // Check if we're at the rate limit
            if (_requestTimestamps.Count >= _maxRequestsPerMinute)
            {
                // Calculate how long to wait
                DateTime oldestRequest = _requestTimestamps.Peek();
                TimeSpan timeToWait = TimeSpan.FromMinutes(1) - (DateTime.UtcNow - oldestRequest);

                if (timeToWait > TimeSpan.Zero)
                {
                    _logger?.LogInformation($"Rate limit reached. Waiting {timeToWait.TotalSeconds:F1} seconds");
                    // We'll wait outside the lock to avoid blocking other threads
                    Monitor.Exit(_requestTimestamps);
                    try
                    {
                        // Wait for the rate limit window to advance
                        Task.Delay(timeToWait).Wait();
                    }
                    finally
                    {
                        // Re-acquire the lock
                        Monitor.Enter(_requestTimestamps);
                    }
                }
            }
        }

        await Task.CompletedTask; // Just to keep the async signature
    }

    private string ExtractConversationId(List<ConversationEntry> conversationHistory)
    {
        // Try to extract a meaningful ID from the first user message
        string userMessage = conversationHistory
            .FirstOrDefault(e => e.Role == "user")?.Content ?? string.Empty;

        // Look for a location name or other identifier in the message
        if (userMessage.Contains("Location:"))
        {
            int locationStart = userMessage.IndexOf("Location:") + 9;
            int locationEnd = userMessage.IndexOf('\n', locationStart);
            if (locationEnd > locationStart)
            {
                return userMessage.Substring(locationStart, locationEnd - locationStart).Trim();
            }
        }

        // Fallback to a generic conversation ID
        return $"conversation_{DateTime.Now:yyyyMMdd_HHmmss}";
    }

    private string DetermineRequestType(List<ConversationEntry> conversationHistory)
    {
        // Try to determine if this is an introduction, reaction, or choices request
        if (conversationHistory.Count <= 2) // Usually system + first message
        {
            return "introduction";
        }

        string lastUserMessage = conversationHistory
            .LastOrDefault(e => e.Role == "user")?.Content ?? string.Empty;

        if (lastUserMessage.Contains("Generate PURE NARRATIVE descriptions for 4 choices"))
        {
            return "choices";
        }

        return "reaction"; // Default to reaction
    }

    private string GetFallbackResponse(string requestType)
    {
        return "default";
    }

}