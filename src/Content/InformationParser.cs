using System.Text.Json;
using Wayfarer.Game.ActionSystem;
using Wayfarer.Game.MainSystem;

namespace Wayfarer.Content;

/// <summary>
/// Parser for Information objects from JSON content
/// Follows the same architectural pattern as ItemParser, ActionParser, etc.
/// </summary>
public static class InformationParser
{
    public static Information ParseInformation(string json)
    {
        JsonDocumentOptions options = new JsonDocumentOptions
        {
            AllowTrailingCommas = true
        };

        using JsonDocument doc = JsonDocument.Parse(json, options);
        JsonElement root = doc.RootElement;

        // Parse required properties
        string id = GetStringProperty(root, "id", "");
        string title = GetStringProperty(root, "title", "");
        string typeStr = GetStringProperty(root, "type", "Market_Intelligence");

        if (!Enum.TryParse<InformationType>(typeStr, out InformationType type))
        {
            type = InformationType.Market_Intelligence; // Default fallback
        }

        // Create Information object
        Information information = new Information(id, title, type)
        {
            Content = GetStringProperty(root, "content", ""),
            Source = GetStringProperty(root, "source", ""),
            Value = GetIntProperty(root, "value", 10),
            IsPublic = GetBoolProperty(root, "isPublic", false),
            DaysToExpire = GetIntProperty(root, "daysToExpire", -1),
            LocationId = GetStringProperty(root, "locationId", ""),
            NPCId = GetStringProperty(root, "npcId", "")
        };

        // Parse categorical properties
        string qualityStr = GetStringProperty(root, "quality", "Reliable");
        if (Enum.TryParse<InformationQuality>(qualityStr, out InformationQuality quality))
        {
            information.Quality = quality;
        }

        // Parse related entity arrays
        List<string> relatedItemIds = GetStringArray(root, "relatedItemIds");
        information.RelatedItemIds.AddRange(relatedItemIds);

        List<string> relatedLocationIds = GetStringArray(root, "relatedLocationIds");
        information.RelatedLocationIds.AddRange(relatedLocationIds);

        // Set acquisition date to current time (will be updated when actually acquired)
        information.AcquiredDate = DateTime.Now;

        // Set default expiration if not specified
        if (information.DaysToExpire == -1)
        {
            information.DaysToExpire = GetDefaultExpirationDays(information.Type);
        }

        return information;
    }

    /// <summary>
    /// Parse multiple Information objects from JSON array
    /// </summary>
    public static List<Information> ParseInformationArray(string json)
    {
        List<Information> informationList = new List<Information>();

        JsonDocumentOptions options = new JsonDocumentOptions
        {
            AllowTrailingCommas = true
        };

        using JsonDocument doc = JsonDocument.Parse(json, options);
        JsonElement root = doc.RootElement;

        if (root.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement infoElement in root.EnumerateArray())
            {
                try
                {
                    Information information = ParseInformation(infoElement.GetRawText());
                    informationList.Add(information);
                }
                catch (Exception ex)
                {
                    // Log parsing error but continue with other entries
                    Console.WriteLine($"Error parsing information entry: {ex.Message}");
                }
            }
        }

        return informationList;
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

    private static bool GetBoolProperty(JsonElement element, string propertyName, bool defaultValue)
    {
        if (element.TryGetProperty(propertyName, out JsonElement property) &&
            (property.ValueKind == JsonValueKind.True || property.ValueKind == JsonValueKind.False))
        {
            return property.GetBoolean();
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

    private static int GetDefaultExpirationDays(InformationType type)
    {
        return type switch
        {
            InformationType.Market_Intelligence => 3,      // Market prices change frequently
            InformationType.Route_Conditions => 7,        // Travel conditions relatively stable
            InformationType.Professional_Knowledge => 30,  // Professional knowledge more stable
            InformationType.Location_Secrets => 90,       // Secrets don't change often
            InformationType.Political_News => 7,          // Political situations change
            InformationType.Personal_History => 365,      // Personal history very stable
            InformationType.Resource_Availability => 5,    // Resource locations change
            _ => 7
        };
    }
}