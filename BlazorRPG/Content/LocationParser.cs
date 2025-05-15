using System.Text.Json;

public static class LocationParser
{
    public static Location ParseLocation(string json)
    {
        var options = new JsonDocumentOptions
        {
            AllowTrailingCommas = true
        };

        using JsonDocument doc = JsonDocument.Parse(json, options);
        JsonElement root = doc.RootElement;

        string id = GetStringProperty(root, "id", "id");
        string name = GetStringProperty(root, "name", "name");

        Location location = new Location(id, name)
        {
            Description = GetStringProperty(root, "description", ""),
            ConnectedTo = GetStringArray(root, "connectedTo"),
            LocationSpotIds = GetStringArray(root, "locationSpots")
        };

        if (root.TryGetProperty("environmentalProperties", out JsonElement envProps) &&
            envProps.ValueKind == JsonValueKind.Object)
        {
            location.MorningProperties = GetStringArrayFromProperty(envProps, "morning");
            location.AfternoonProperties = GetStringArrayFromProperty(envProps, "afternoon");
            location.EveningProperties = GetStringArrayFromProperty(envProps, "evening");
            location.NightProperties = GetStringArrayFromProperty(envProps, "night");
        }

        return location;
    }

    public static LocationSpot ParseLocationSpot(string json)
    {
        var options = new JsonDocumentOptions
        {
            AllowTrailingCommas = true
        };

        using JsonDocument doc = JsonDocument.Parse(json, options);
        JsonElement root = doc.RootElement;

        string id = GetStringProperty(root, "id", "");
        string name = GetStringProperty(root, "name", "");

        LocationSpot spot = new LocationSpot(id, name)
        {
            Description = GetStringProperty(root, "description", ""),
            InitialState = GetStringProperty(root, "initialState", ""),
            CurrentLevel = GetIntProperty(root, "currentLevel", 1),
            CurrentSpotXP = GetIntProperty(root, "currentXP", 0),
            XPToNextLevel = GetIntProperty(root, "xpToNextLevel", 100),
            Type = Enum.Parse<LocationSpotTypes>(GetStringProperty(root, "type", "FEATURE"), true)
        };

        // Parse time windows
        if (root.TryGetProperty("timeWindows", out JsonElement timeWindowsArray) &&
            timeWindowsArray.ValueKind == JsonValueKind.Array)
        {
            spot.TimeWindows = TimeWindows.None;
            foreach (JsonElement item in timeWindowsArray.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.String)
                {
                    string value = item.GetString();
                    if (!string.IsNullOrEmpty(value) &&
                        Enum.TryParse(value, true, out TimeWindowTypes window))
                    {
                        spot.TimeWindows.Add(window);
                    }
                }
            }
            if (spot.TimeWindows.Values.Count == 0)
            {
                // fallback to always open
                spot.TimeWindows = TimeWindows.All;
            }
        }
        else
        {
            spot.TimeWindows = TimeWindows.All;
        }

        return spot;
    }

    private static List<string> GetStringArrayFromProperty(JsonElement element, string propertyName)
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
