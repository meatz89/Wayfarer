using System.Text.Json;
public static class LocationJsonParser
{
    public static LocationDetails ParseLocationDetails(string jsonResponse)
    {
        try
        {
            // Extract JSON from text response if needed
            string json = ExtractJsonFromText(jsonResponse);
            JsonElement jsonObj = JsonDocument.Parse(json).RootElement;

            // Create the location details
            LocationDetails details = new LocationDetails
            {
                Name = GetStringProperty(jsonObj, "name") ?? "UnknownLocation",
                Description = GetStringProperty(jsonObj, "description") ?? "",
                DetailedDescription = GetStringProperty(jsonObj, "detailedDescription") ?? "",
                History = GetStringProperty(jsonObj, "history") ?? "",
                PointsOfInterest = GetStringProperty(jsonObj, "pointsOfInterest") ?? "",
                TravelTimeMinutes = GetIntProperty(jsonObj, "travelTimeMinutes") ?? 120,
                TravelDescription = GetStringProperty(jsonObj, "travelDescription") ?? "",
                ConnectedLocationIds = ParseStringArray(jsonObj, "connectedLocationIds"),
                TimeProperties = ParseTimeProperties(jsonObj, "timeProperties"),
                Spots = ParseSpots(jsonObj, "spots"),
            };

            return details;
        }
        catch (Exception ex)
        {
            // Return a default location if parsing fails
            return CreateDefaultLocation("DefaultLocation");
        }
    }

    private static string? GetStringProperty(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out JsonElement property) && property.ValueKind == JsonValueKind.String)
            return property.GetString();
        return null;
    }

    private static int? GetIntProperty(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out JsonElement property) && property.ValueKind == JsonValueKind.Number)
            return property.GetInt32();
        return null;
    }

    private static List<string> ParseStringArray(JsonElement root, string propertyName)
    {
        List<string> result = new List<string>();

        if (root.TryGetProperty(propertyName, out JsonElement arrayElement) &&
            arrayElement.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement item in arrayElement.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.String)
                    result.Add(item.GetString() ?? "");
            }
        }

        return result;
    }

    private static Dictionary<string, List<IEnvironmentalProperty>> ParseTimeProperties(JsonElement root, string propertyName)
    {
        Dictionary<string, List<IEnvironmentalProperty>> timeProperties = new Dictionary<string, List<IEnvironmentalProperty>>();

        if (root.TryGetProperty(propertyName, out JsonElement timeObject) &&
            timeObject.ValueKind == JsonValueKind.Object)
        {
            foreach (JsonProperty timeProp in timeObject.EnumerateObject())
            {
                string timeOfDay = timeProp.Name;
                List<IEnvironmentalProperty> properties = new List<IEnvironmentalProperty>();

                if (timeProp.Value.ValueKind == JsonValueKind.Array)
                {
                    foreach (JsonElement prop in timeProp.Value.EnumerateArray())
                    {
                        if (prop.TryGetProperty("type", out JsonElement typeElement) &&
                            prop.TryGetProperty("value", out JsonElement valueElement))
                        {
                            string type = typeElement.GetString() ?? "";
                            string value = valueElement.GetString() ?? "";

                            IEnvironmentalProperty? property = CreateEnvironmentalProperty(type, value);
                            if (property != null)
                                properties.Add(property);
                        }
                    }
                }

                timeProperties[timeOfDay] = properties;
            }
        }

        return timeProperties;
    }

    private static List<SpotDetails> ParseSpots(JsonElement root, string propertyName)
    {
        List<SpotDetails> spots = new List<SpotDetails>();

        if (root.TryGetProperty(propertyName, out JsonElement arrayElement) &&
            arrayElement.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement spot in arrayElement.EnumerateArray())
            {
                SpotDetails spotDetails = new SpotDetails
                {
                    Name = GetStringProperty(spot, "name") ?? "UnknownSpot",
                    Description = GetStringProperty(spot, "description") ?? "",
                    InteractionType = GetStringProperty(spot, "interactionType") ?? "",
                    InteractionDescription = GetStringProperty(spot, "interactionDescription") ?? "",
                    Position = GetStringProperty(spot, "position") ?? "",
                    ActionNames = ParseActionNames(spot)
                };

                spots.Add(spotDetails);
            }
        }

        return spots;
    }

    private static List<string> ParseActionNames(JsonElement spot)
    {
        List<string> actionNames = new List<string>();

        if (spot.TryGetProperty("actionNames", out JsonElement arrayElement) &&
            arrayElement.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement action in arrayElement.EnumerateArray())
            {
                if (action.ValueKind == JsonValueKind.String)
                {
                    string actionName = action.GetString() ?? "";
                    actionNames.Add(actionName);
                }
            }
        }

        return actionNames;
    }

    private static IEnvironmentalProperty? CreateEnvironmentalProperty(string type, string value)
    {
        switch (type.ToLower())
        {
            case "illumination":
                switch (value.ToLower())
                {
                    case "bright": return Illumination.Bright;
                    case "shadowy": return Illumination.Shadowy;
                    case "dark": return Illumination.Dark;
                    default: return null;
                }

            case "population":
                switch (value.ToLower())
                {
                    case "crowded": return Population.Crowded;
                    case "quiet": return Population.Quiet;
                    case "isolated": return Population.Isolated;
                    default: return null;
                }

            case "atmosphere":
                switch (value.ToLower())
                {
                    case "tense": return Atmosphere.Tense;
                    case "formal": return Atmosphere.Formal;
                    case "chaotic": return Atmosphere.Chaotic;
                    default: return null;
                }

            case "economic":
                switch (value.ToLower())
                {
                    case "wealthy": return Economic.Wealthy;
                    case "commercial": return Economic.Commercial;
                    case "humble": return Economic.Humble;
                    default: return null;
                }

            case "physical":
                switch (value.ToLower())
                {
                    case "confined": return Physical.Confined;
                    case "expansive": return Physical.Expansive;
                    case "hazardous": return Physical.Hazardous;
                    default: return null;
                }

            default:
                return null;
        }
    }

    private static LocationDetails CreateDefaultLocation(string name)
    {
        // Create a basic default location if parsing fails
        LocationDetails details = new LocationDetails
        {
            Name = name,
            Description = "A basic location",
            DetailedDescription = "This location was created as a fallback due to parsing errors.",
            TravelTimeMinutes = 60,
            TravelDescription = "A simple path leads to this location.",
            ConnectedLocationIds = new List<string> { "Village" },
            EnvironmentalProperties = new List<IEnvironmentalProperty>
        {
            Illumination.Bright,
            Population.Quiet,
            Atmosphere.Formal,
            Economic.Humble,
            Physical.Confined
        }
        };

        // Add default time properties
        details.TimeProperties = new Dictionary<string, List<IEnvironmentalProperty>>
    {
        { "Morning", new List<IEnvironmentalProperty> { Illumination.Bright, Population.Quiet } },
        { "Afternoon", new List<IEnvironmentalProperty> { Illumination.Bright, Population.Crowded } },
        { "Evening", new List<IEnvironmentalProperty> { Illumination.Shadowy, Population.Crowded } },
        { "Night", new List<IEnvironmentalProperty> { Illumination.Dark, Population.Quiet } }
    };

        // Add a default spot
        details.Spots = new List<SpotDetails>
        {
            new SpotDetails
            {
                Name = "Central Area",
                Description = "The main area of this location.",
                InteractionType = "Feature",
                InteractionDescription = "Explore the area",
                Position = "Center",
                ActionNames = new List<string> { ActionNames.RentRoom.ToString() }
            }
        };

        return details;
    }

    // Helper to extract JSON from text that might contain other content
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