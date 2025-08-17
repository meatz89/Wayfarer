using System.Text.Json;

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
            MinDeadlineInHours = GetIntProperty(root, "minDeadlineInHours", 72),
            MaxDeadlineInHours = GetIntProperty(root, "maxDeadlineInHours", 120),
            MinPayment = GetIntProperty(root, "minPayment", 3),
            MaxPayment = GetIntProperty(root, "maxPayment", 5)
        };

        // Parse token type
        string tokenTypeStr = GetStringProperty(root, "tokenType", "");
        template.TokenType = ParseConnectionType(tokenTypeStr);

        // Parse optional arrays
        template.PossibleSenders = GetStringArray(root, "possibleSenders").ToArray();
        template.PossibleRecipients = GetStringArray(root, "possibleRecipients").ToArray();

        // Parse special letter properties
        string specialTypeStr = GetStringProperty(root, "specialType", "None");
        template.SpecialType = ParseSpecialType(specialTypeStr);
        template.SpecialTargetId = GetStringProperty(root, "specialTargetId", "");

        // Parse human context and consequences
        template.HumanContext = GetStringProperty(root, "humanContext", "");
        template.ConsequenceIfLate = GetStringProperty(root, "consequenceIfLate", "");
        template.ConsequenceIfDelivered = GetStringProperty(root, "consequenceIfDelivered", "");

        // Parse emotional weight
        string emotionalWeightStr = GetStringProperty(root, "emotionalWeight", "MEDIUM");
        template.EmotionalWeight = ParseEmotionalWeight(emotionalWeightStr);

        // Parse stakes
        string stakesStr = GetStringProperty(root, "stakes", "REPUTATION");
        template.Stakes = ParseStakeType(stakesStr);

        return template;
    }

    private static ConnectionType ParseConnectionType(string connectionTypeStr)
    {
        return connectionTypeStr switch
        {
            "Trust" => ConnectionType.Trust,
            "Trade" => ConnectionType.Commerce,
            "Noble" => ConnectionType.Status,
            "Common" => ConnectionType.Trust,
            "Shadow" => ConnectionType.Shadow,
            _ => ConnectionType.Trust // Default fallback
        };
    }

    private static LetterSpecialType ParseSpecialType(string specialTypeStr)
    {
        return specialTypeStr switch
        {
            "Introduction" => LetterSpecialType.Introduction,
            "AccessPermit" => LetterSpecialType.AccessPermit,
            "Endorsement" => LetterSpecialType.Endorsement,
            "Information" => LetterSpecialType.Information,
            "None" => LetterSpecialType.None,
            _ => LetterSpecialType.None // Default fallback
        };
    }

    private static EmotionalWeight ParseEmotionalWeight(string weightStr)
    {
        return weightStr?.ToUpper() switch
        {
            "LOW" => EmotionalWeight.LOW,
            "MEDIUM" => EmotionalWeight.MEDIUM,
            "HIGH" => EmotionalWeight.HIGH,
            "CRITICAL" => EmotionalWeight.CRITICAL,
            _ => EmotionalWeight.MEDIUM // Default fallback
        };
    }

    private static StakeType ParseStakeType(string stakeStr)
    {
        return stakeStr?.ToUpper() switch
        {
            "REPUTATION" => StakeType.REPUTATION,
            "WEALTH" => StakeType.WEALTH,
            "SAFETY" => StakeType.SAFETY,
            "SECRET" => StakeType.SECRET,
            _ => StakeType.REPUTATION // Default fallback
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