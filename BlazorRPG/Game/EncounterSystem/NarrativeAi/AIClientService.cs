
// Handles communication with AI service
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;

public class AIClientService
{
    private readonly HttpClient _httpClient;
    private readonly string _modelName;
    private readonly string _apiKey;
    private readonly NarrativeLogManager _logManager;

    public AIClientService(IConfiguration configuration)
    {
        _httpClient = new HttpClient();
        _modelName = "gpt-4";
        _apiKey = configuration.GetValue<string>("OpenAiApiKey");
        _logManager = new NarrativeLogManager();
    }

    public async Task<string> GetCompletionAsync(List<ConversationEntry> conversationHistory)
    {
        // Get a unique log file path for this request
        string logFilePath = _logManager.GetNextLogFilePath();

        // Prepare API messages from conversation history
        var messages = conversationHistory.Select(m => new { role = m.Role, content = m.Content }).ToArray();

        var requestBody = new
        {
            model = _modelName,
            messages = messages,
            temperature = 0.7,
            max_tokens = 500 // Limit response size for speed
        };

        // Log request to file
        await LogRequestAsync(logFilePath, conversationHistory, requestBody);

        // Make API call
        StringContent content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json");

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        HttpResponseMessage response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);

        if (response.IsSuccessStatusCode)
        {
            string jsonResponse = await response.Content.ReadAsStringAsync();

            // Parse response
            using (JsonDocument document = JsonDocument.Parse(jsonResponse))
            {
                JsonElement root = document.RootElement;
                JsonElement choices = root.GetProperty("choices");
                JsonElement firstChoice = choices[0];
                JsonElement message = firstChoice.GetProperty("message");
                string? generatedContent = message.GetProperty("content").GetString();

                // Log response
                await LogResponseAsync(logFilePath, jsonResponse, generatedContent);

                return generatedContent;
            }
        }

        // Log error
        await LogErrorAsync(logFilePath, response);

        throw new Exception($"Failed to get response from GPT: {response.StatusCode}");
    }

    private async Task LogRequestAsync(string logFilePath, List<ConversationEntry> history, object requestBody)
    {
        using (StreamWriter writer = File.CreateText(logFilePath))
        {
            var options = new JsonSerializerOptions { WriteIndented = true };

            // Write conversation history
            await writer.WriteLineAsync("// Full Conversation History");
            await writer.WriteLineAsync(JsonSerializer.Serialize(history, options));

            // Write API request
            await writer.WriteLineAsync("\n// API Request");
            await writer.WriteLineAsync(JsonSerializer.Serialize(requestBody, options));
        }
    }

    private async Task LogResponseAsync(string logFilePath, string jsonResponse, string generatedContent)
    {
        using (StreamWriter writer = File.AppendText(logFilePath))
        {
            await writer.WriteLineAsync("\n// API Response");
            await writer.WriteLineAsync(jsonResponse);
            await writer.WriteLineAsync("\n// Final Content");
            await writer.WriteLineAsync(generatedContent);
        }
    }

    private async Task LogErrorAsync(string logFilePath, HttpResponseMessage response)
    {
        using (StreamWriter writer = File.AppendText(logFilePath))
        {
            await writer.WriteLineAsync("\n// API Error");
            await writer.WriteLineAsync($"Status Code: {response.StatusCode}");
            await writer.WriteLineAsync(await response.Content.ReadAsStringAsync());
        }
    }
}


