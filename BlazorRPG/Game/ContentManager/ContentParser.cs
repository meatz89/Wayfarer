using System.Text.Json;

public static class ContentParser
{
    public static Location ParseLocation(string json)
    {
        using JsonDocument doc = JsonDocument.Parse(json);
        JsonElement root = doc.RootElement;

        string id = GetStringProperty(root, "id", "");
        Location location = new Location(id)
        {
            Name = GetStringProperty(root, "name", id),
            Description = GetStringProperty(root, "description", ""),
            DetailedDescription = GetStringProperty(root, "detailedDescription", ""),
            ConnectedTo = GetStringArray(root, "connectedTo")
        };

        return location;
    }

    public static LocationSpot ParseLocationSpot(string json)
    {
        using JsonDocument doc = JsonDocument.Parse(json);
        JsonElement root = doc.RootElement;

        string name = GetStringProperty(root, "name", "");
        string locationId = GetStringProperty(root, "locationId", "");

        LocationSpot spot = new LocationSpot(name, locationId)
        {
            Description = GetStringProperty(root, "description", ""),
            CurrentLevel = GetIntProperty(root, "currentLevel", 1),
            CurrentSpotXP = GetIntProperty(root, "currentXP", 0),
            XPToNextLevel = GetIntProperty(root, "xpToNextLevel", 100)
        };

        // Parse levels
        spot.LevelData = new List<SpotLevel>();
        if (root.TryGetProperty("levels", out JsonElement levelsArray) &&
            levelsArray.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement levelElement in levelsArray.EnumerateArray())
            {
                SpotLevel level = new SpotLevel
                {
                    Level = GetIntProperty(levelElement, "level", 1),
                    Description = GetStringProperty(levelElement, "description", ""),
                    AddedActionIds = GetStringArray(levelElement, "actionIds")
                };

                // Parse level-up encounter if present
                if (levelElement.TryGetProperty("levelUpEncounter", out JsonElement encounterElement))
                {
                    level.EncounterActionId = GetStringProperty(encounterElement, "id", "");
                }

                // Parse removed actions
                level.RemovedActionIds = GetStringArray(levelElement, "removedActionIds");

                spot.LevelData.Add(level);
            }
        }

        return spot;
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
