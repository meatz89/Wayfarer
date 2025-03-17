using System.Text;
using System.Text.Json;

public class Gemma3Provider : IAIProvider
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _endpoint;
    private readonly ILogger<EncounterSystem> _logger;

    public string Name => "Google Gemma 3 27B";

    public Gemma3Provider(IConfiguration configuration, ILogger<EncounterSystem> logger)
    {
        _httpClient = new HttpClient();
        _apiKey = configuration.GetValue<string>("Gemma:ApiKey");
        _endpoint = configuration.GetValue<string>("Gemma:Endpoint");
        _logger = logger;

        // Set API key in Authorization header
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

        // Optional: Set SSL protocols to be more permissive during development
        // ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
    }

    public async Task<string> GetCompletionAsync(IEnumerable<ConversationEntry> messages)
    {
        var requestBody = new
        {
            model = "gemma-27b",
            messages = messages.Select(m => new { role = m.Role, content = m.Content }),
            temperature = 0.7,
            max_output_tokens = 500
        };

        StringContent content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        try
        {
            _logger?.LogInformation($"Sending request to Gemma API: {_endpoint}");
            HttpResponseMessage response = await _httpClient.PostAsync(_endpoint, content);

            // Log status code for debugging
            _logger?.LogInformation($"Received response with status code: {response.StatusCode}");

            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            _logger?.LogInformation($"Raw response: {responseBody.Substring(0, Math.Min(100, responseBody.Length))}...");

            JsonElement responseObject = JsonSerializer.Deserialize<JsonElement>(responseBody);
            return responseObject.GetProperty("candidates")[0].GetProperty("content").GetString();
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