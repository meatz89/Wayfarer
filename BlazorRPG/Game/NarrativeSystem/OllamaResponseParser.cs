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

    public static NpcCharacter ParseCharacterJson(string messageContent)
    {
        try
        {
            // Find JSON within the content (sometimes it's wrapped in ```json blocks)
            int startIndex = messageContent.IndexOf('{');
            int endIndex = messageContent.LastIndexOf('}');

            if (startIndex >= 0 && endIndex >= 0 && endIndex > startIndex)
            {
                string jsonString = messageContent.Substring(startIndex, endIndex - startIndex + 1);

                using JsonDocument doc = JsonDocument.Parse(jsonString);
                JsonElement root = doc.RootElement;

                NpcCharacter character = new NpcCharacter
                {
                    Name = GetStringProperty(root, "name", "Unknown"),
                    Age = GetIntProperty(root, "age", 30),
                    Gender = GetStringProperty(root, "gender", "unknown"),
                    Occupation = GetStringProperty(root, "occupation", "unknown"),
                    Appearance = GetStringProperty(root, "appearance", string.Empty),
                    Background = GetStringProperty(root, "background", string.Empty),
                    Personality = GetStringProperty(root, "personality", string.Empty),
                    Motivation = GetStringProperty(root, "motivation", string.Empty),
                    Quirk = GetStringProperty(root, "quirk", string.Empty),
                    Secret = GetStringProperty(root, "secret", string.Empty),
                    Possessions = GetStringArray(root, "possessions"),
                    Skills = GetStringArray(root, "skills"),
                    Relationships = GetStringArray(root, "relationships")
                };

                return character;
            }

            throw new FormatException("Could not find JSON in the response");
        }
        catch (Exception ex)
        {
            throw new FormatException($"Failed to parse character data: {ex.Message}", ex);
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

    private static int GetIntProperty(JsonElement element, string propertyName, int defaultValue)
    {
        if (element.TryGetProperty(propertyName, out JsonElement property))
        {
            if (property.ValueKind == JsonValueKind.Number)
            {
                return property.GetInt32();
            }
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

    private static List<string> GetStringArray(JsonElement element, string propertyName)
    {
        List<string> results = new List<string>();

        if (element.TryGetProperty(propertyName, out JsonElement arrayElement) &&
            arrayElement.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement item in arrayElement.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.String)
                {
                    string value = item.GetString() ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        results.Add(value);
                    }
                }
            }
        }

        return results;
    }
}
