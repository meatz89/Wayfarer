using System.Text.Json;

public static class OllamaResponseParser
{
    public static string ExtractMessageContent(string jsonResponse)
    {
        try
        {
            using JsonDocument doc = JsonDocument.Parse(jsonResponse);
            JsonElement root = doc.RootElement;

            // Extract the message content, which is what we really care about
            if (root.TryGetProperty("message", out JsonElement messageElement))
            {
                if (messageElement.TryGetProperty("content", out JsonElement contentElement) &&
                    contentElement.ValueKind == JsonValueKind.String)
                {
                    return contentElement.GetString() ?? string.Empty;
                }
            }

            throw new JsonException("Message content not found in response");
        }
        catch (JsonException ex)
        {
            throw new JsonException($"Failed to parse Ollama response: {ex.Message}", ex);
        }
    }

    public static OllamaResponseData ParseFullResponse(string jsonResponse)
    {
        try
        {
            using JsonDocument doc = JsonDocument.Parse(jsonResponse);
            JsonElement root = doc.RootElement;

            OllamaResponseData response = new OllamaResponseData
            {
                Model = GetStringProperty(root, "model", string.Empty),
                CreatedAt = GetStringProperty(root, "created_at", string.Empty),
                Done = GetBoolProperty(root, "done", false),
                DoneReason = GetStringProperty(root, "done_reason", string.Empty)
            };

            // Extract the message
            if (root.TryGetProperty("message", out JsonElement messageElement))
            {
                response.MessageRole = GetStringProperty(messageElement, "role", "assistant");
                response.MessageContent = GetStringProperty(messageElement, "content", string.Empty);
            }

            return response;
        }
        catch (JsonException ex)
        {
            throw new JsonException($"Failed to parse Ollama response: {ex.Message}", ex);
        }
    }

    private static string GetStringProperty(JsonElement element, string propertyName, string defaultValue)
    {
        if (element.TryGetProperty(propertyName, out JsonElement property) &&
            property.ValueKind == JsonValueKind.String)
        {
            string value = property.GetString() ?? defaultValue;
            return !string.IsNullOrWhiteSpace(value) ? value : defaultValue;
        }
        return defaultValue;
    }

    private static bool GetBoolProperty(JsonElement element, string propertyName, bool defaultValue)
    {
        if (element.TryGetProperty(propertyName, out JsonElement property))
        {
            if (property.ValueKind == JsonValueKind.True || property.ValueKind == JsonValueKind.False)
            {
                return property.GetBoolean();
            }
        }
        return defaultValue;
    }
}
