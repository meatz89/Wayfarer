using System.Text;
using System.Text.Json;
// OpenAI Implementation
public class OpenAIProvider : IAIProvider
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;

    public string Name => "OpenAI GPT-4o-mini";

    public OpenAIProvider(IConfiguration configuration)
    {
        _httpClient = new HttpClient();
        _apiKey = configuration.GetValue<string>("OpenAI:ApiKey");
        _model = configuration.GetValue<string>("OpenAI:Model") ?? "gpt-4o-mini";
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
    }

    public async Task<string> GetCompletionAsync(IEnumerable<ConversationEntry> messages)
    {
        var requestBody = new
        {
            model = _model,
            messages = messages.Select(m => new { role = m.Role, content = m.Content }),
            temperature = 0.7,
            max_tokens = 500
        };

        StringContent content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        HttpResponseMessage response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);

        response.EnsureSuccessStatusCode();
        string responseBody = await response.Content.ReadAsStringAsync();
        JsonElement responseObject = JsonSerializer.Deserialize<JsonElement>(responseBody);

        return responseObject.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
    }
}
