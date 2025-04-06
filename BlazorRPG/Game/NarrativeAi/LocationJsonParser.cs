using System.Text.Json;

public static class LocationJsonParser
{
    public static LocationDetails ParseLocationDetails(string response)
    {
        // Extract JSON from text response if needed
        string json = ExtractJsonFromText(response);

        // First get flat data
        FlatLocationResponse flatResponse = ParseFlatResponse(json);

        // Then build the structured result
        return BuildLocationDetails(flatResponse);
    }

    private static FlatLocationResponse ParseFlatResponse(string json)
    {
        FlatLocationResponse result = new FlatLocationResponse();

        try
        {
            using JsonDocument doc = JsonDocument.Parse(json);
            JsonElement root = doc.RootElement;

            // Parse player location update
            if (root.TryGetProperty("playerLocationUpdate", out JsonElement locationUpdate))
            {
                result.PlayerLocationUpdate = new PlayerLocationUpdate
                {
                    NewLocationName = GetStringProperty(locationUpdate, "newLocationName", string.Empty),
                    LocationChanged = GetBoolProperty(locationUpdate, "locationChanged", false)
                };
            }

            // Parse location spots
            result.LocationSpots = GetArrayOfType(
                root,
                "locationSpots",
                element => ParseLocationSpot(element));

            // Parse action definitions
            result.ActionDefinitions = GetArrayOfType(
                root,
                "actionDefinitions",
                element => ParseActionDefinition(element));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing location response: {ex.Message}");
        }

        return result;
    }

    private static LocationDetails BuildLocationDetails(FlatLocationResponse flatResponse)
    {
        LocationDetails details = new LocationDetails
        {
            // Initialize with default values
            Name = flatResponse.PlayerLocationUpdate?.NewLocationName ?? "",
            Description = "",
            DetailedDescription = "",
            History = "",
            PointsOfInterest = "",
            TravelTimeMinutes = 0,
            TravelDescription = "",
            LocationUpdate = flatResponse.PlayerLocationUpdate ?? new PlayerLocationUpdate()
        };

        // Process location spots
        details.NewLocationSpots = new List<SpotDetails>();
        foreach (FlatLocationSpot spotDef in flatResponse.LocationSpots)
        {
            SpotDetails spot = new SpotDetails
            {
                Id = spotDef.Id,
                Name = spotDef.Name,
                Description = spotDef.Description,
                InteractionType = spotDef.InteractionType,
                Position = spotDef.Position,
                ActionNames = new List<string>(),
                EnvironmentalProperties = spotDef.EnvironmentalProperties ?? new Dictionary<string, string>()
            };

            details.NewLocationSpots.Add(spot);
        }

        // Add environmental properties from the first spot if available
        if (details.NewLocationSpots.Count > 0 &&
            details.NewLocationSpots[0].EnvironmentalProperties.Count > 0)
        {
            details.EnvironmentalProperties = ParseEnvironmentalPropertiesFromDictionary(
                details.NewLocationSpots[0].EnvironmentalProperties);
        }

        // Process action definitions
        details.NewActions = new List<NewAction>();
        foreach (FlatActionDefinition actionDef in flatResponse.ActionDefinitions)
        {
            NewAction action = new NewAction
            {
                Name = actionDef.Name,
                Description = actionDef.Description,
                LocationName = actionDef.LocationName,
                SpotName = GetSpotNameById(details.NewLocationSpots, actionDef.LocationSpotId),
                Goal = actionDef.EncounterDefinition?.Goal ?? "",
                Complication = actionDef.EncounterDefinition?.Complication ?? "",
                ActionType = ConvertActionType(actionDef.Type),
                IsRepeatable = true
            };

            details.NewActions.Add(action);

            // Associate action with spot
            foreach (SpotDetails spot in details.NewLocationSpots)
            {
                if (spot.Id == actionDef.LocationSpotId)
                {
                    spot.ActionNames.Add(actionDef.Name);
                    break;
                }
            }
        }

        return details;
    }

    private static string GetSpotNameById(List<SpotDetails> spots, string spotId)
    {
        foreach (SpotDetails spot in spots)
        {
            if (spot.Id == spotId)
            {
                return spot.Name;
            }
        }
        return "";
    }

    #region Helper Methods for Parsing

    private static FlatLocationSpot ParseLocationSpot(JsonElement element)
    {
        FlatLocationSpot spot = new FlatLocationSpot
        {
            Id = GetStringProperty(element, "id", ""),
            Name = GetStringProperty(element, "name", "Unnamed Spot"),
            Description = GetStringProperty(element, "description", "No description available."),
            InteractionType = GetStringProperty(element, "interactionType", "Feature"),
            Position = GetStringProperty(element, "position", "Center"),
            LocationName = GetStringProperty(element, "locationName", "")
        };

        // Parse environmental properties if available
        if (element.TryGetProperty("environmentalProperties", out JsonElement envProps) &&
            envProps.ValueKind == JsonValueKind.Object)
        {
            spot.EnvironmentalProperties = new Dictionary<string, string>();

            foreach (JsonProperty prop in envProps.EnumerateObject())
            {
                string propName = prop.Name;
                string propValue = prop.Value.GetString() ?? "";
                spot.EnvironmentalProperties[propName] = propValue;
            }
        }

        return spot;
    }

    private static FlatActionDefinition ParseActionDefinition(JsonElement element)
    {
        FlatActionDefinition action = new FlatActionDefinition
        {
            Id = GetStringProperty(element, "id", ""),
            Name = GetStringProperty(element, "name", "Unnamed Action"),
            Description = GetStringProperty(element, "description", "No description available."),
            Type = GetStringProperty(element, "type", "Encounter"),
            LocationName = GetStringProperty(element, "locationName", ""),
            LocationSpotId = GetStringProperty(element, "locationSpotId", "")
        };

        // Parse encounter definition if available
        if (element.TryGetProperty("encounterDefinition", out JsonElement encounterElement))
        {
            action.EncounterDefinition = new EncounterDefinition
            {
                Goal = GetStringProperty(encounterElement, "goal", ""),
                Complication = GetStringProperty(encounterElement, "complication", ""),
                Momentum = GetIntProperty(encounterElement, "momentum", 0),
                Pressure = GetIntProperty(encounterElement, "pressure", 0),
                StrategicTags = GetStringArray(encounterElement, "strategicTags")
            };
        }

        return action;
    }

    private static List<IEnvironmentalProperty> ParseEnvironmentalPropertiesFromDictionary(Dictionary<string, string> properties)
    {
        if (properties == null) return new List<IEnvironmentalProperty>();

        List<IEnvironmentalProperty> result = new List<IEnvironmentalProperty>();

        foreach (KeyValuePair<string, string> pair in properties)
        {
            IEnvironmentalProperty property = CreateEnvironmentalProperty(pair.Key, pair.Value);
            if (property != null)
            {
                result.Add(property);
            }
        }

        return result;
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

    private static IEnvironmentalProperty CreateEnvironmentalProperty(string type, string value)
    {
        // Using the same implementation as in the post-encounter parser
        Dictionary<string, IEnvironmentalProperty> propertyMap = new Dictionary<string, IEnvironmentalProperty>(StringComparer.OrdinalIgnoreCase)
        {
            // Illumination properties
            { "bright", Illumination.Bright },
            { "shadowy", Illumination.Shadowy },
            { "dark", Illumination.Dark },
            
            // Population properties
            { "crowded", Population.Crowded },
            { "quiet", Population.Quiet },
            { "isolated", Population.Isolated },
            
            // Economic properties
            { "wealthy", Economic.Wealthy },
            { "commercial", Economic.Commercial },
            { "humble", Economic.Humble },
            
            // Physical properties
            { "confined", Physical.Confined },
            { "expansive", Physical.Expansive },
            { "hazardous", Physical.Hazardous },
            
            // Atmosphere properties
            { "tense", Atmosphere.Tense },
            { "formal", Atmosphere.Formal },
            { "chaotic", Atmosphere.Chaotic }
        };

        if (propertyMap.TryGetValue(value.ToLower(), out IEnvironmentalProperty property))
        {
            return property;
        }

        // Default fallback - this should be rare
        return Illumination.Bright;
    }

    // Existing helper methods for JSON parsing
    private static string? GetStringProperty(JsonElement element, string propertyName, string defaultValue = null)
    {
        if (element.TryGetProperty(propertyName, out JsonElement property) && property.ValueKind == JsonValueKind.String)
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

    private static List<T> GetArrayOfType<T>(JsonElement element, string propertyName, Func<JsonElement, T> parser)
    {
        List<T> results = new List<T>();

        if (element.TryGetProperty(propertyName, out JsonElement arrayElement) &&
            arrayElement.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement item in arrayElement.EnumerateArray())
            {
                T parsedItem = parser(item);
                if (parsedItem != null)
                {
                    results.Add(parsedItem);
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

    #endregion
}