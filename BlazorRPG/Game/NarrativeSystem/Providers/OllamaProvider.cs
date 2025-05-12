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

    public async Task<string> GetCompletionAsync(List<ConversationEntry> messages, string model, string fallbackModel)
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
            Stream = false,
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

        HttpResponseMessage response = await _httpClient.PostAsync(
            endpoint,
            new StringContent(jsonRequest, Encoding.UTF8, "application/json")
        );

        response.EnsureSuccessStatusCode();

        string jsonResponse = await response.Content.ReadAsStringAsync();

        // Use our custom parser instead of JsonSerializer.Deserialize
        string content = OllamaResponseParser.ExtractMessageContent(jsonResponse);
        return content;
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