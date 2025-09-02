using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

public static class LetterTemplateParser
{
    /// <summary>
    /// Convert a LetterTemplateDTO to a LetterTemplate domain model
    /// </summary>
    public static LetterTemplate ConvertDTOToLetterTemplate(LetterTemplateDTO dto)
    {
        LetterTemplate template = new LetterTemplate
        {
            Id = dto.Id ?? "",
            Description = dto.Description ?? "",
            MinDeadlineInMinutes = dto.MinDeadlineInMinutes,
            MaxDeadlineInMinutes = dto.MaxDeadlineInMinutes,
            MinPayment = dto.MinPayment,
            MaxPayment = dto.MaxPayment,
            MinTokensRequired = dto.MinTokensRequired ?? 1
        };

        // Parse token type
        if (!string.IsNullOrEmpty(dto.TokenType))
        {
            template.TokenType = ParseConnectionType(dto.TokenType);
        }

        // Parse optional arrays
        template.PossibleSenders = dto.PossibleSenders?.ToArray() ?? new string[0];
        template.PossibleRecipients = dto.PossibleRecipients?.ToArray() ?? new string[0];

        // Parse special letter properties
        template.SpecialType = ParseSpecialType(dto.SpecialType ?? "None");
        template.SpecialTargetId = dto.SpecialTargetId ?? "";

        // Parse human context and consequences
        template.ConsequenceIfLate = dto.ConsequenceIfLate ?? "";
        template.ConsequenceIfDelivered = dto.ConsequenceIfDelivered ?? "";

        // Parse emotional weight
        template.EmotionalWeight = ParseEmotionalWeight(dto.EmotionalWeight ?? "MEDIUM");

        // Parse stakes
        template.Stakes = ParseStakeType(dto.Stakes ?? "REPUTATION");

        // Parse category
        if (!string.IsNullOrEmpty(dto.Category))
        {
            if (Enum.TryParse<LetterCategory>(dto.Category, out var category))
            {
                template.Category = category;
            }
        }

        // Parse tier level
        if (!string.IsNullOrEmpty(dto.TierLevel))
        {
            if (Enum.TryParse<TierLevel>(dto.TierLevel, out var tier))
            {
                template.TierLevel = tier;
            }
        }

        // Parse size
        if (!string.IsNullOrEmpty(dto.Size))
        {
            if (Enum.TryParse<SizeCategory>(dto.Size, out var size))
            {
                template.Size = size;
            }
        }

        return template;
    }
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
            MinDeadlineInMinutes = GetIntProperty(root, "minDeadlineInHours", 72),
            MaxDeadlineInMinutes = GetIntProperty(root, "maxDeadlineInHours", 120),
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
            _ => throw new ArgumentException($"Unknown connection type in JSON: '{connectionTypeStr}' - add to connection type mapping")
        };
    }

    private static LetterSpecialType ParseSpecialType(string specialTypeStr)
    {
        return specialTypeStr switch
        {
            "Introduction" => LetterSpecialType.Introduction,
            "AccessPermit" => LetterSpecialType.AccessPermit,
            "None" => LetterSpecialType.None,
            _ => throw new ArgumentException($"Unknown special type in JSON: '{specialTypeStr}' - add to special type mapping")
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
            _ => throw new ArgumentException($"Unknown emotional weight in JSON: '{weightStr}' - add to emotional weight mapping")
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
            _ => throw new ArgumentException($"Unknown stake type in JSON: '{stakeStr}' - add to stake type mapping")
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