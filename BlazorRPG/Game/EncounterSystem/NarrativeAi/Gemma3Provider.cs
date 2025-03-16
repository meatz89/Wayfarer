using System.Text;
using System.Text.Json;
// Gemma Implementation
public class Gemma3Provider : IAIProvider
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _endpoint;

    public string Name => "Google Gemma 3 27B";

    public Gemma3Provider(IConfiguration configuration)
    {
        _httpClient = new HttpClient();
        _apiKey = configuration.GetValue<string>("Gemma:ApiKey");
        _endpoint = configuration.GetValue<string>("Gemma:Endpoint");
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
    }

    public async Task<string> GetCompletionAsync(IEnumerable<ConversationEntry> messages)
    {
        var requestBody = new
        {
            model = "gemma-3-27b",
            messages = messages.Select(m => new { role = m.Role, content = m.Content }),
            temperature = 0.7,
            max_tokens = 500
        };

        StringContent content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        HttpResponseMessage response = await _httpClient.PostAsync(_endpoint, content);

        response.EnsureSuccessStatusCode();
        string responseBody = await response.Content.ReadAsStringAsync();
        JsonElement responseObject = JsonSerializer.Deserialize<JsonElement>(responseBody);

        return responseObject.GetProperty("candidates")[0].GetProperty("content").GetString();
    }
}
