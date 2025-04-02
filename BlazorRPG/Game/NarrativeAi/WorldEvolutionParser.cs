using System.Text.Json;

public class WorldEvolutionParser
{
    private readonly ILogger<WorldEvolutionParser> _logger;

    public WorldEvolutionParser(
        ILogger<WorldEvolutionParser> logger = null)
    {
        _logger = logger;
    }

    public async Task<WorldEvolutionResponse> ParseWorldEvolutionResponseAsync(string response)
    {
        FlatWorldEvolutionResponse flatResponse = ParseFlatResponse(response);
        return await BuildNestedResponseAsync(flatResponse);
    }

    private FlatWorldEvolutionResponse ParseFlatResponse(string response)
    {
        FlatWorldEvolutionResponse result = new FlatWorldEvolutionResponse();
        response = response.Replace("```json", "");
        response = response.Replace("```", "");
        
        try
        {
            using JsonDocument doc = JsonDocument.Parse(response);
            JsonElement root = doc.RootElement;

            // Parse player state changes
            if (root.TryGetProperty("playerLocationUpdate", out JsonElement locationUpdateElement))
            {
                result.PlayerLocationUpdate = new PlayerLocationUpdate
                {
                    NewLocationName = GetStringProperty(locationUpdateElement, "newLocationName", string.Empty),
                    LocationChanged = GetBoolProperty(locationUpdateElement, "locationChanged", false)
                };
            }

            // Parse resource changes
            if (root.TryGetProperty("resourceChanges", out JsonElement resourceElement))
            {
                result.ResourceChanges = new ResourceChanges
                {
                    CoinChange = GetIntProperty(resourceElement, "coinChange", 0),
                    ItemsAdded = GetStringArray(resourceElement, "itemsAdded"),
                    ItemsRemoved = GetStringArray(resourceElement, "itemsRemoved")
                };
            }

            // Parse relationship changes
            result.RelationshipChanges = GetArrayOfType(
                root,
                "relationshipChanges",
                element => ParseRelationshipChange(element));

            // Parse locations
            result.Locations = GetArrayOfType(
                root,
                "locations",
                element => ParseLocationDefinition(element));

            // Parse location spots
            result.LocationSpots = GetArrayOfType(
                root,
                "locationSpots",
                element => ParseLocationSpotDefinition(element));

            // Parse action definitions
            result.ActionDefinitions = GetArrayOfType(
                root,
                "actionDefinitions",
                element => ParseActionDefinition(element));

            // Parse characters
            result.Characters = GetArrayOfType(
                root,
                "characters",
                element => ParseCharacter(element));

            // Parse opportunities
            result.Opportunities = GetArrayOfType(
                root,
                "opportunities",
                element => ParseOpportunity(element));
        }
        catch (Exception ex)
        {
            LogError("Error parsing flat response", ex);
        }

        return result;
    }


    private async Task<WorldEvolutionResponse> BuildNestedResponseAsync(FlatWorldEvolutionResponse flatResponse)
    {
        WorldEvolutionResponse result = new WorldEvolutionResponse
        {
            LocationUpdate = flatResponse.PlayerLocationUpdate ?? new PlayerLocationUpdate(),
            ResourceChanges = flatResponse.ResourceChanges ?? new ResourceChanges(),
            RelationshipChanges = flatResponse.RelationshipChanges ?? new List<RelationshipChange>(),
            CoinChange = flatResponse.ResourceChanges?.CoinChange ?? 0,
            NewLocationSpots = new List<LocationSpot>(),
            NewActions = new List<NewAction>(),
            NewCharacters = flatResponse.Characters ?? new List<Character>(),
            NewLocations = new List<Location>(),
            NewOpportunities = flatResponse.Opportunities ?? new List<Opportunity>()
        };

        // Step 1: Process all action definitions first
        Dictionary<string, Dictionary<string, List<string>>> actionsByLocationAndSpot =
            new Dictionary<string, Dictionary<string, List<string>>>();

        foreach (ActionDefinition actionDef in flatResponse.ActionDefinitions)
        {
            try
            {
                if (!actionsByLocationAndSpot.ContainsKey(actionDef.LocationName))
                {
                    actionsByLocationAndSpot[actionDef.LocationName] = new Dictionary<string, List<string>>();
                }

                if (!actionsByLocationAndSpot[actionDef.LocationName].ContainsKey(actionDef.SpotName))
                {
                    actionsByLocationAndSpot[actionDef.LocationName][actionDef.SpotName] = new List<string>();
                }

                actionsByLocationAndSpot[actionDef.LocationName][actionDef.SpotName].Add(actionDef.Name);

                // Also add to the NewActions collection for standalone tracking
                result.NewActions.Add(new NewAction
                {
                    SpotName = actionDef.SpotName,
                    LocationName = actionDef.LocationName,
                    Name = actionDef.Name,
                    Description = actionDef.Description,
                    Goal = actionDef.Goal,
                    Complication = actionDef.Complication,
                    ActionType = actionDef.ActionType
                });
            }
            catch (Exception ex)
            {
                LogError($"Failed to generate action for {actionDef.Name}", ex);
            }
        }

        // Step 2: Create location spots (referencing actions by name)
        Dictionary<string, List<LocationSpot>> spotsByLocation = new Dictionary<string, List<LocationSpot>>();

        foreach (LocationSpotDefinition spotDef in flatResponse.LocationSpots)
        {
            LocationSpot spot = new LocationSpot
            {
                Name = spotDef.Name,
                Description = spotDef.Description,
                InteractionType = spotDef.InteractionType,
                LocationName = spotDef.LocationName,
                ActionTemplates = new List<string>()
            };

            // Add to spots by location
            if (!spotsByLocation.ContainsKey(spotDef.LocationName))
            {
                spotsByLocation[spotDef.LocationName] = new List<LocationSpot>();
            }
            spotsByLocation[spotDef.LocationName].Add(spot);

            // Also add to NewLocationSpots if it's not part of a new location
            if (!flatResponse.Locations.Any(l => l.Name == spotDef.LocationName))
            {
                result.NewLocationSpots.Add(spot);
            }
        }

        // Step 3: Create locations with their spots
        foreach (LocationDefinition locDef in flatResponse.Locations)
        {
            Location location = new Location
            {
                Name = locDef.Name,
                Description = locDef.Description,
                Difficulty = locDef.Difficulty,
                ConnectedTo = locDef.ConnectedTo,
                EnvironmentalProperties = ParseEnvironmentalProperties(locDef.EnvironmentalProperties),
                Spots = new List<LocationSpot>()
            };

            // Add spots for this location
            if (spotsByLocation.TryGetValue(locDef.Name, out List<LocationSpot>? spots))
            {
                location.Spots.AddRange(spots);
            }

            result.NewLocations.Add(location);
        }

        return result;
    }

    #region Parser Helper Methods

    private RelationshipChange ParseRelationshipChange(JsonElement element)
    {
        return SafeParseEntity("relationship change", () => new RelationshipChange
        {
            CharacterName = GetStringProperty(element, "characterName", "Unknown Character"),
            ChangeAmount = GetIntProperty(element, "changeAmount", 0),
            Reason = GetStringProperty(element, "reason", "Unspecified reason")
        });
    }

    private LocationDefinition ParseLocationDefinition(JsonElement element)
    {
        return SafeParseEntity("location", () => new LocationDefinition
        {
            Name = GetStringProperty(element, "name", "Unnamed Location"),
            Description = GetStringProperty(element, "description", "No description available."),
            Difficulty = GetIntProperty(element, "difficulty", 1),
            ConnectedTo = GetStringArrayOrSingle(element, "connectedTo"),
            EnvironmentalProperties = GetStringArray(element, "environmentalProperties")
        });
    }

    private LocationSpotDefinition ParseLocationSpotDefinition(JsonElement element)
    {
        return SafeParseEntity("location spot", () => new LocationSpotDefinition
        {
            Name = GetStringProperty(element, "name", "Unnamed Spot"),
            Description = GetStringProperty(element, "description", "No description available."),
            InteractionType = GetStringProperty(element, "interactionType", "Feature"),
            LocationName = GetStringProperty(element, "locationName", "Unknown Location")
        });
    }

    private ActionDefinition ParseActionDefinition(JsonElement element)
    {
        return SafeParseEntity("action definition", () => new ActionDefinition
        {
            Name = GetStringProperty(element, "name", "Unnamed Action"),
            Description = GetStringProperty(element, "description", "No description available."),
            Goal = GetStringProperty(element, "goal", "Unknown goal"),
            Complication = GetStringProperty(element, "complication", "Unknown complication"),
            ActionType = GetStringProperty(element, "actionType", "Discuss"),
            SpotName = GetStringProperty(element, "spotName", "Unknown Spot"),
            LocationName = GetStringProperty(element, "locationName", "Unknown Location")
        });
    }

    private Character ParseCharacter(JsonElement element)
    {
        return SafeParseEntity("character", () => new Character
        {
            Name = GetStringProperty(element, "name", "Unnamed Character"),
            Role = GetStringProperty(element, "role", "Unknown Role"),
            Description = GetStringProperty(element, "description", "No description available."),
            Location = GetStringProperty(element, "location", "Unknown Location")
        });
    }

    private Opportunity ParseOpportunity(JsonElement element)
    {
        return SafeParseEntity("opportunity", () => new Opportunity
        {
            Name = GetStringProperty(element, "name", "Unnamed Opportunity"),
            Type = GetStringProperty(element, "type", "Unknown Type"),
            Description = GetStringProperty(element, "description", "No description available."),
            Location = GetStringProperty(element, "location", "Unknown Location"),
            RelatedCharacter = GetStringProperty(element, "relatedCharacter", "Unknown Character")
        });
    }

    private List<IEnvironmentalProperty> ParseEnvironmentalProperties(List<string> propertyStrings)
    {
        List<IEnvironmentalProperty> properties = new List<IEnvironmentalProperty>();

        foreach (string propString in propertyStrings)
        {
            IEnvironmentalProperty property = ParseEnvironmentalProperty(propString);
            if (property != null)
            {
                properties.Add(property);
            }
        }

        return properties;
    }

    private IEnvironmentalProperty ParseEnvironmentalProperty(string propertyString)
    {
        Dictionary<string, IEnvironmentalProperty> propertyMap = new Dictionary<string, IEnvironmentalProperty>(StringComparer.OrdinalIgnoreCase)
        {
            // Illumination properties
            { "Bright", Illumination.Bright },
            { "Shadowy", Illumination.Shadowy },
            { "Dark", Illumination.Dark },
            
            // Population properties
            { "Crowded", Population.Crowded },
            { "Quiet", Population.Quiet },
            { "Isolated", Population.Isolated },
            
            // Economic properties
            { "Wealthy", Economic.Wealthy },
            { "Commercial", Economic.Commercial },
            { "Humble", Economic.Humble },
            
            // Physical properties
            { "Confined", Physical.Confined },
            { "Expansive", Physical.Expansive },
            { "Hazardous", Physical.Hazardous },
            
            // Atmosphere properties
            { "Tense", Atmosphere.Tense },
            { "Formal", Atmosphere.Formal },
            { "Chaotic", Atmosphere.Chaotic }
        };

        if (propertyMap.TryGetValue(propertyString, out IEnvironmentalProperty property))
        {
            return property;
        }

        return Illumination.Bright; // Default fallback
    }

    private BasicActionTypes ParseActionType(string actionTypeStr)
    {
        if (Enum.TryParse<BasicActionTypes>(actionTypeStr, true, out BasicActionTypes actionType))
        {
            return actionType;
        }

        return BasicActionTypes.Discuss;
    }

    #endregion

    #region Utility Methods

    private T SafeParseEntity<T>(string entityType, Func<T> parser) where T : class
    {
        try
        {
            return parser();
        }
        catch (Exception ex)
        {
            LogError($"Failed to parse {entityType}", ex);
            return null;
        }
    }

    private void LogError(string message, Exception ex)
    {
        _logger?.LogError(ex, message);
        Console.WriteLine($"{message}: {ex.Message}");
    }

    private List<string> GetStringArray(JsonElement element, string propertyName)
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

    private List<string> GetStringArrayOrSingle(JsonElement element, string propertyName)
    {
        List<string> results = new List<string>();

        if (element.TryGetProperty(propertyName, out JsonElement propElement))
        {
            if (propElement.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement item in propElement.EnumerateArray())
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
            else if (propElement.ValueKind == JsonValueKind.String)
            {
                string value = propElement.GetString() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(value))
                {
                    results.Add(value);
                }
            }
        }

        return results;
    }

    private List<T> GetArrayOfType<T>(JsonElement element, string propertyName, Func<JsonElement, T> parser) where T : class
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

    private string GetStringProperty(JsonElement element, string propertyName, string defaultValue)
    {
        if (element.TryGetProperty(propertyName, out JsonElement property) &&
            property.ValueKind == JsonValueKind.String)
        {
            string value = property.GetString() ?? defaultValue;
            return !string.IsNullOrWhiteSpace(value) ? value : defaultValue;
        }
        return defaultValue;
    }

    private int GetIntProperty(JsonElement element, string propertyName, int defaultValue)
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

    private bool GetBoolProperty(JsonElement element, string propertyName, bool defaultValue)
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

    #endregion
}

public class FlatWorldEvolutionResponse
{
    public PlayerLocationUpdate PlayerLocationUpdate { get; set; }
    public ResourceChanges ResourceChanges { get; set; }
    public List<RelationshipChange> RelationshipChanges { get; set; } = new List<RelationshipChange>();
    public List<LocationDefinition> Locations { get; set; } = new List<LocationDefinition>();
    public List<LocationSpotDefinition> LocationSpots { get; set; } = new List<LocationSpotDefinition>();
    public List<ActionDefinition> ActionDefinitions { get; set; } = new List<ActionDefinition>();
    public List<Character> Characters { get; set; } = new List<Character>();
    public List<Opportunity> Opportunities { get; set; } = new List<Opportunity>();
}

public class LocationDefinition
{
    public string Name { get; set; }
    public string Description { get; set; }
    public int Difficulty { get; set; }
    public List<string> ConnectedTo { get; set; } = new List<string>();
    public List<string> EnvironmentalProperties { get; set; } = new List<string>();
}

public class LocationSpotDefinition
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string InteractionType { get; set; }
    public string LocationName { get; set; }
}

public class ActionDefinition
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Goal { get; set; }
    public string Complication { get; set; }
    public string ActionType { get; set; }
    public string SpotName { get; set; }
    public string LocationName { get; set; }
}