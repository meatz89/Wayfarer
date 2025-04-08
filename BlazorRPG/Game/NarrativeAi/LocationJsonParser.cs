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
        details.DetailedDescription = GetStringProperty(root, "detailedDescription", string.Empty);
        details.History = GetStringProperty(root, "history", string.Empty);
        details.PointsOfInterest = GetStringProperty(root, "pointsOfInterest", string.Empty);
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

                // Extract environmental properties from first spot if available
                if (details.NewLocationSpots.Count == 1 && spot.EnvironmentalProperties.Count > 0)
                {
                    details.EnvironmentalProperties = ConvertEnvironmentalProperties(spot.EnvironmentalProperties);
                }
            }
        }

        // Parse action definitions
        details.NewActions = new List<NewAction>();
        if (root.TryGetProperty("actionDefinitions", out JsonElement actionsArray) &&
            actionsArray.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement actionElement in actionsArray.EnumerateArray())
            {
                NewAction action = ParseNewAction(actionElement);
                details.NewActions.Add(action);

                // Associate action with spot
                string spotId = GetStringProperty(actionElement, "locationSpotId", "");
                foreach (SpotDetails spot in details.NewLocationSpots)
                {
                    if (spot.Id == spotId)
                    {
                        if (spot.ActionNames == null)
                            spot.ActionNames = new List<string>();

                        spot.ActionNames.Add(action.Name);
                        break;
                    }
                }
            }
        }

        // Initialize empty collections for other fields
        details.TimeProperties = new Dictionary<string, List<IEnvironmentalProperty>>();
        details.StrategicTags = new List<StrategicTag>();
        details.NarrativeTags = new List<NarrativeTag>();

        // If there are environmental properties, create a basic time property entry
        if (details.EnvironmentalProperties.Count > 0)
        {
            details.TimeProperties["Day"] = new List<IEnvironmentalProperty>(details.EnvironmentalProperties);
        }

        return details;
    }

    private static SpotDetails ParseSpotDetails(JsonElement element)
    {
        SpotDetails spot = new SpotDetails
        {
            Id = GetStringProperty(element, "id", Guid.NewGuid().ToString()),
            Name = GetStringProperty(element, "name", "Unnamed Spot"),
            Description = GetStringProperty(element, "description", "No description available."),
            InteractionType = GetStringProperty(element, "interactionType", "Feature"),
            InteractionDescription = GetStringProperty(element, "interactionDescription", ""),
            Position = GetStringProperty(element, "position", "Center"),
            ActionNames = new List<string>(),
            EnvironmentalProperties = new Dictionary<string, string>()
        };

        // Parse environmental properties if available
        if (element.TryGetProperty("environmentalProperties", out JsonElement envProps) &&
            envProps.ValueKind == JsonValueKind.Object)
        {
            foreach (JsonProperty prop in envProps.EnumerateObject())
            {
                string propName = prop.Name;
                string propValue = prop.Value.GetString() ?? "";
                spot.EnvironmentalProperties[propName] = propValue;
            }
        }

        return spot;
    }

    private static NewAction ParseNewAction(JsonElement element)
    {
        NewAction action = new NewAction
        {
            Name = GetStringProperty(element, "name", "Unnamed Action"),
            Description = GetStringProperty(element, "description", "No description available."),
            LocationName = GetStringProperty(element, "locationName", ""),
            SpotName = "", // This will be populated later when we match with spots
            ActionType = ConvertActionType(GetStringProperty(element, "type", "Encounter")),
            IsRepeatable = GetBoolProperty(element, "isRepeatable", true),
            EnergyCost = 1,  // Default energy cost
        };

        // Get cost information
        if (element.TryGetProperty("cost", out JsonElement costElement))
        {
            action.EnergyCost = GetIntProperty(costElement, "energy", 1);
        }

        // Get encounter definition info
        if (element.TryGetProperty("encounterDefinition", out JsonElement encounterElement))
        {
            action.Goal = GetStringProperty(encounterElement, "goal", "");
            action.Complication = GetStringProperty(encounterElement, "complication", "");
        }

        return action;
    }

    private static List<IEnvironmentalProperty> ConvertEnvironmentalProperties(Dictionary<string, string> properties)
    {
        List<IEnvironmentalProperty> result = new List<IEnvironmentalProperty>();

        foreach (KeyValuePair<string, string> pair in properties)
        {
            IEnvironmentalProperty prop = CreateEnvironmentalProperty(pair.Key, pair.Value);
            if (prop != null)
            {
                result.Add(prop);
            }
        }

        return result;
    }

    private static IEnvironmentalProperty CreateEnvironmentalProperty(string type, string value)
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

    private static string ConvertActionType(string type)
    {
        // Map action type to the format expected by the system
        switch (type.ToLower())
        {
            case "encounter": return "Discuss";
            case "direct": return "Execute";
            case "travel": return "Travel";
            default: return "Discuss";
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