using System.Text.Json;
using Wayfarer.GameState;

namespace Wayfarer.Content;

public static class LetterTemplateParser
{
    public static LetterTemplate ParseLetterTemplate(string json)
    {
        JsonDocumentOptions options = new JsonDocumentOptions
        {
            AllowTrailingCommas = true
        };

        using JsonDocument doc = JsonDocument.Parse(json, options);
        JsonElement root = doc.RootElement;

        LetterTemplate template = new LetterTemplate
        {
            Id = GetStringProperty(root, "id", ""),
            Description = GetStringProperty(root, "description", ""),
            MinDeadline = GetIntProperty(root, "minDeadline", 3),
            MaxDeadline = GetIntProperty(root, "maxDeadline", 5),
            MinPayment = GetIntProperty(root, "minPayment", 3),
            MaxPayment = GetIntProperty(root, "maxPayment", 5)
        };

        // Parse token type
        string tokenTypeStr = GetStringProperty(root, "tokenType", "");
        template.TokenType = ParseConnectionType(tokenTypeStr);

        // Parse optional arrays
        template.PossibleSenders = GetStringArray(root, "possibleSenders").ToArray();
        template.PossibleRecipients = GetStringArray(root, "possibleRecipients").ToArray();

        return template;
    }

    private static ConnectionType ParseConnectionType(string connectionTypeStr)
    {
        return connectionTypeStr switch
        {
            "Trust" => ConnectionType.Trust,
            "Trade" => ConnectionType.Trade,
            "Noble" => ConnectionType.Noble,
            "Common" => ConnectionType.Common,
            "Shadow" => ConnectionType.Shadow,
            _ => ConnectionType.Common // Default fallback
        };
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
        if (element.TryGetProperty(propertyName, out JsonElement property) &&
            property.ValueKind == JsonValueKind.Number)
        {
            return property.GetInt32();
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