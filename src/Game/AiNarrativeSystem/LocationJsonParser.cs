using System.Text.Json;

public static class LocationJsonParser
{
    public static LocationDetails ParseLocationDetails(string response)
    {
        response = response.Replace("```json", "");
        response = response.Replace("```", "");

        // Extract JSON from text response if needed
        string json = ExtractJsonFromText(response);

        using JsonDocument doc = JsonDocument.Parse(json);
        JsonElement root = doc.RootElement;

        // Create basic LocationDetails object
        LocationDetails details = new LocationDetails();

        // Parse player location update
        if (root.TryGetProperty("playerLocationUpdate", out JsonElement locationUpdate))
        {
            details.LocationUpdate = new PlayerLocationUpdate
            {
                NewLocationName = GetStringProperty(locationUpdate, "newLocationName", string.Empty),
                LocationChanged = GetBoolProperty(locationUpdate, "locationChanged", false)
            };

            // Set the location name from the update
            details.Name = details.LocationUpdate.NewLocationName;
        }

        // Set default values for required fields
        details.Name = GetStringProperty(root, "name", "Unknown Location");
        details.Description = GetStringProperty(root, "description", "A newly discovered location");
        details.History = GetStringProperty(root, "history", string.Empty);
        details.PointsOfInterest = GetStringArray(root, "pointsOfInterest");
        details.TravelTimeMinutes = GetIntProperty(root, "travelTimeMinutes", 60);
        details.TravelDescription = GetStringProperty(root, "travelDescription", string.Empty);

        // Parse connected location IDs
        details.ConnectedLocationIds = GetStringArray(root, "connectedLocationIds");

        // Parse location spots
        details.NewLocationSpots = new List<SpotDetails>();
        if (root.TryGetProperty("locationSpots", out JsonElement spotsArray) &&
            spotsArray.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement spotElement in spotsArray.EnumerateArray())
            {
                SpotDetails spot = ParseSpotDetails(spotElement);
                details.NewLocationSpots.Add(spot);
            }
        }

        // Action system removed - using command system instead

        // Initialize empty collections for other fields
        details.StrategicTags = new List<string>();
        details.NarrativeTags = new List<string>();

        return details;
    }

    private static SpotDetails ParseSpotDetails(JsonElement element)
    {
        SpotDetails spot = new SpotDetails
        {
            Name = GetStringProperty(element, "name", "Unnamed Spot"),
            Description = GetStringProperty(element, "description", "No description available."),
            InteractionDescription = GetStringProperty(element, "interactionDescription", ""),
            EnvironmentalProperties = new List<string>()
        };

        // Parse environmental properties if available
        if (element.TryGetProperty("environmentalProperties", out JsonElement envProps) &&
            envProps.ValueKind == JsonValueKind.Object)
        {
            foreach (JsonProperty prop in envProps.EnumerateObject())
            {
                string propName = prop.Name;
                string propValue = prop.Value.GetString() ?? "";
                spot.EnvironmentalProperties.Add($"{propName}:{propValue}");
            }
        }

        return spot;
    }


    private static ILocationProperty CreateEnvironmentalProperty(string type, string value)
    {
        switch (type.ToLower())
        {
            case "illumination":
                switch (value.ToLower())
                {
                    case "bright": return Illumination.Bright;
                    case "Thiefy": return Illumination.Thiefy;
                    case "dark": return Illumination.Dark;
                    default: return Illumination.Any;
                }

            case "population":
                switch (value.ToLower())
                {
                    case "crowded": return Population.Crowded;
                    case "quiet": return Population.Quiet;
                    case "isolated": return Population.Scholarly;
                    default: return Population.Any;
                }

            case "atmosphere":
                switch (value.ToLower())
                {
                    case "tense": return Atmosphere.Rough;
                    case "formal": return Atmosphere.Tense;
                    case "chaotic": return Atmosphere.Chaotic;
                    default: return Atmosphere.Any;
                }

            case "physical":
                switch (value.ToLower())
                {
                    case "confined": return Physical.Confined;
                    case "expansive": return Physical.Expansive;
                    case "hazardous": return Physical.Hazardous;
                    default: return Physical.Any;
                }

            default:
                return null;
        }
    }

    // Utility methods for JSON parsing
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
            else if (property.ValueKind == JsonValueKind.String &&
                     int.TryParse(property.GetString(), out int value))
            {
                return value;
            }
        }
        return defaultValue;
    }

    private static bool GetBoolProperty(JsonElement element, string propertyName, bool defaultValue)
    {
        if (element.TryGetProperty(propertyName, out JsonElement property))
        {
            if (property.ValueKind == JsonValueKind.True)
                return true;
            else if (property.ValueKind == JsonValueKind.False)
                return false;
            else if (property.ValueKind == JsonValueKind.String &&
                     bool.TryParse(property.GetString(), out bool value))
            {
                return value;
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

    private static string ExtractJsonFromText(string text)
    {
        // Find JSON start and end
        int startIndex = text.IndexOf('{');
        int endIndex = text.LastIndexOf('}');

        if (startIndex >= 0 && endIndex > startIndex)
        {
            return text.Substring(startIndex, endIndex - startIndex + 1);
        }

        // If no JSON is found, return an empty object
        return "{}";
    }
}