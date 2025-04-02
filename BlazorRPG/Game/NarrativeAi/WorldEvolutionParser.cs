using System.Text.Json;

public class WorldEvolutionParser
{
    private readonly ActionRepository _actionRepository;
    private readonly ILogger<WorldEvolutionParser> _logger;

    public WorldEvolutionParser(ActionRepository actionRepository, ILogger<WorldEvolutionParser> logger = null)
    {
        _actionRepository = actionRepository;
        _logger = logger;
    }

    /// <summary>
    /// Parses the AI response into a structured WorldEvolutionResponse object.
    /// </summary>
    public WorldEvolutionResponse ParseWorldEvolutionResponse(string response)
    {
        WorldEvolutionResponse result = InitializeEmptyResponse();

        try
        {
            using JsonDocument doc = JsonDocument.Parse(response);
            JsonElement root = doc.RootElement;

            // Process all entity types
            ProcessAllEntityTypes(root, result);
        }
        catch (JsonException ex)
        {
            LogError("Error parsing JSON response", ex);
        }
        catch (Exception ex)
        {
            LogError("Unexpected error parsing response", ex);
        }

        return result;
    }

    #region Main Processing Methods

    private void ProcessAllEntityTypes(JsonElement root, WorldEvolutionResponse result)
    {
        // Process player status changes
        ProcessPlayerLocationUpdate(root, result);
        ProcessResourceChanges(root, result);
        ProcessRelationshipChanges(root, result);
        ProcessCoinChange(root, result);

        // Process world entity changes
        ProcessLocationSpots(root, result);
        ProcessNewActions(root, result);
        ProcessNewCharacters(root, result);
        ProcessNewLocations(root, result);
        ProcessNewOpportunities(root, result);
    }

    private WorldEvolutionResponse InitializeEmptyResponse()
    {
        return new WorldEvolutionResponse
        {
            NewLocationSpots = new List<LocationSpot>(),
            NewActions = new List<NewAction>(),
            NewCharacters = new List<Character>(),
            NewLocations = new List<Location>(),
            NewOpportunities = new List<Opportunity>(),
            LocationUpdate = new PlayerLocationUpdate(),
            ResourceChanges = new ResourceChanges(),
            RelationshipChanges = new List<RelationshipChange>(),
            CoinChange = 0
        };
    }

    #endregion

    #region Player State Change Processors

    private void ProcessPlayerLocationUpdate(JsonElement root, WorldEvolutionResponse result)
    {
        if (root.TryGetProperty("playerLocationUpdate", out JsonElement locationUpdateElement) &&
            locationUpdateElement.ValueKind == JsonValueKind.Object)
        {
            PlayerLocationUpdate locationUpdate = new PlayerLocationUpdate();

            locationUpdate.NewLocationName = GetStringProperty(locationUpdateElement, "newLocationName", string.Empty);
            locationUpdate.LocationChanged = GetBoolProperty(locationUpdateElement, "locationChanged", false);

            result.LocationUpdate = locationUpdate;
        }
    }

    private void ProcessResourceChanges(JsonElement root, WorldEvolutionResponse result)
    {
        if (root.TryGetProperty("resourceChanges", out JsonElement resourceChangesElement) &&
            resourceChangesElement.ValueKind == JsonValueKind.Object)
        {
            ResourceChanges resourceChanges = new ResourceChanges();

            // Process items added and removed
            resourceChanges.ItemsAdded = GetStringArray(resourceChangesElement, "itemsAdded");
            resourceChanges.ItemsRemoved = GetStringArray(resourceChangesElement, "itemsRemoved");

            // Process coin change (also maintain the top-level coinChange for backward compatibility)
            if (resourceChangesElement.TryGetProperty("coinChange", out JsonElement coinChangeElement) &&
                coinChangeElement.ValueKind == JsonValueKind.Number)
            {
                result.CoinChange = coinChangeElement.GetInt32();
            }

            result.ResourceChanges = resourceChanges;
        }
    }

    private void ProcessRelationshipChanges(JsonElement root, WorldEvolutionResponse result)
    {
        result.RelationshipChanges = GetArrayOfType(
            root,
            "relationshipChanges",
            element => ParseRelationshipChange(element));
    }

    private void ProcessCoinChange(JsonElement root, WorldEvolutionResponse result)
    {
        if (root.TryGetProperty("coinChange", out JsonElement coinChangeElement) &&
            coinChangeElement.ValueKind == JsonValueKind.Number)
        {
            result.CoinChange = coinChangeElement.GetInt32();
        }
    }

    #endregion

    #region World Entity Processors

    private void ProcessLocationSpots(JsonElement root, WorldEvolutionResponse result)
    {
        result.NewLocationSpots = GetArrayOfType(
            root,
            "newLocationSpots",
            element => ParseLocationSpot(element));
    }

    private void ProcessNewActions(JsonElement root, WorldEvolutionResponse result)
    {
        result.NewActions = GetArrayOfType(
            root,
            "newActions",
            element => ParseNewAction(element));
    }

    private void ProcessNewCharacters(JsonElement root, WorldEvolutionResponse result)
    {
        result.NewCharacters = GetArrayOfType(
            root,
            "newCharacters",
            element => ParseCharacter(element));
    }

    private void ProcessNewLocations(JsonElement root, WorldEvolutionResponse result)
    {
        result.NewLocations = GetArrayOfType(
            root,
            "newLocations",
            element => ParseLocation(element));
    }

    private void ProcessNewOpportunities(JsonElement root, WorldEvolutionResponse result)
    {
        result.NewOpportunities = GetArrayOfType(
            root,
            "newOpportunities",
            element => ParseOpportunity(element));
    }

    #endregion

    #region Entity Parsers

    private RelationshipChange ParseRelationshipChange(JsonElement element)
    {
        return SafeParseEntity("relationship change", () => new RelationshipChange
        {
            CharacterName = GetStringProperty(element, "characterName", "Unknown Character"),
            ChangeAmount = GetIntProperty(element, "changeAmount", 0),
            Reason = GetStringProperty(element, "reason", "Unspecified reason")
        });
    }

    private LocationSpot ParseLocationSpot(JsonElement element)
    {
        return SafeParseEntity("location spot", () =>
        {
            LocationSpot spot = new LocationSpot
            {
                Name = GetStringProperty(element, "name", "Unnamed Spot"),
                Description = GetStringProperty(element, "description", "No description available."),
                InteractionType = GetStringProperty(element, "interactionType", "Feature"),
                LocationName = GetStringProperty(element, "locationName", ""),
                ActionTemplates = new List<string>()
            };

            // Process actions for this spot
            if (element.TryGetProperty("actions", out JsonElement actionsElement) &&
                actionsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement actionElement in actionsElement.EnumerateArray())
                {
                    ActionTemplate actionTemplate = ParseAction(actionElement);
                    if (actionTemplate != null)
                    {
                        string actionName = _actionRepository.GetOrCreateActionTemplate(
                            actionTemplate.Name,
                            actionTemplate.Goal,
                            actionTemplate.Complication,
                            actionTemplate.ActionType,
                            actionTemplate.EncounterTemplateName
                        );

                        spot.ActionTemplates.Add(actionName);
                    }
                }
            }

            return spot;
        });
    }

    private NewAction ParseNewAction(JsonElement element)
    {
        return SafeParseEntity("new action", () => new NewAction
        {
            SpotName = GetStringProperty(element, "spotName", "Unknown Spot"),
            LocationName = GetStringProperty(element, "locationName", "Unknown Location"),
            Name = GetStringProperty(element, "name", "Unnamed Action"),
            Description = GetStringProperty(element, "description", "No description available."),
            Goal = GetStringProperty(element, "goal", "Unknown goal"),
            Complication = GetStringProperty(element, "complication", "Unknown complication"),
            ActionType = GetStringProperty(element, "actionType", "Unknown Action Type")
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

    private Location ParseLocation(JsonElement element)
    {
        return SafeParseEntity("location", () =>
        {
            Location location = new Location
            {
                Name = GetStringProperty(element, "name", "Unnamed Location"),
                Description = GetStringProperty(element, "description", "No description available."),
                EnvironmentalProperties = new List<IEnvironmentalProperty>(),
                ConnectedTo = new List<string>(),
                Spots = new List<LocationSpot>()
            };

            // Parse difficulty level
            if (element.TryGetProperty("difficulty", out JsonElement difficultyElement))
            {
                if (difficultyElement.ValueKind == JsonValueKind.Number)
                {
                    location.Difficulty = difficultyElement.GetInt32();
                }
                else if (difficultyElement.ValueKind == JsonValueKind.String &&
                         int.TryParse(difficultyElement.GetString(), out int difficultyValue))
                {
                    location.Difficulty = difficultyValue;
                }
            }

            // Parse connected locations
            location.ConnectedTo = GetStringArrayOrSingle(element, "connectedTo");

            // Parse environmental properties
            location.EnvironmentalProperties = GetArrayOfType(
                element,
                "environmentalProperties",
                prop => ParseEnvironmentalProperty(prop.GetString() ?? ""));

            // Parse location spots
            location.Spots = GetArrayOfType(
                element,
                "spots",
                spotElement => ParseLocationSpot(spotElement));

            return location;
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

    private ActionTemplate ParseAction(JsonElement element)
    {
        return SafeParseEntity("action", () =>
        {
            string actionName = GetStringProperty(element, "name", "Unnamed Action");
            string actionDesc = GetStringProperty(element, "description", "No description available.");
            string goal = GetStringProperty(element, "goal", actionDesc);
            string complication = GetStringProperty(element, "complication", "AI-generated action");

            // Try to get the action type directly, or infer it
            BasicActionTypes actionType = ParseActionType(element);

            // Create action template
            return new ActionTemplateBuilder()
                .WithName(actionName)
                .WithGoal(goal)
                .WithComplication(complication)
                .WithActionType(actionType)
                .Build();
        });
    }

    #endregion

    #region Helper Methods

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
        if (element.TryGetProperty(propertyName, out JsonElement property) &&
            property.ValueKind == JsonValueKind.Number)
        {
            return property.GetInt32();
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
        }
        return defaultValue;
    }

    private BasicActionTypes ParseActionType(JsonElement element)
    {
        // Try to get the action type directly from the element
        string actionTypeStr = GetStringProperty(element, "actionType", "");

        if (Enum.TryParse<BasicActionTypes>(actionTypeStr, true, out BasicActionTypes actionType))
        {
            return actionType;
        }

        // If not found, infer from name and description
        string name = GetStringProperty(element, "name", "");
        string description = GetStringProperty(element, "description", "");

        return DetermineActionType(name, description);
    }

    private BasicActionTypes DetermineActionType(string name, string description)
    {
        Dictionary<BasicActionTypes, List<string>> keywordMap = new Dictionary<BasicActionTypes, List<string>>
        {
            {
                BasicActionTypes.Discuss,
                new List<string> { "talk", "discuss", "meet", "conversation", "speak", "chat" }
            },
            {
                BasicActionTypes.Persuade,
                new List<string> { "trade", "bargain", "persuade", "negotiate", "convince", "deal" }
            },
            {
                BasicActionTypes.Travel,
                new List<string> { "travel", "journey", "path", "road", "trek", "walk" }
            },
            {
                BasicActionTypes.Rest,
                new List<string> { "rest", "sleep", "recover", "relax", "lodge" }
            },
            {
                BasicActionTypes.Investigate,
                new List<string> { "investigate", "search", "examine", "clues", "discover", "find", "look" }
            }
        };

        // Combine name and description for keyword matching
        string combined = $"{name} {description}".ToLowerInvariant();

        foreach (BasicActionTypes actionType in keywordMap.Keys)
        {
            foreach (string keyword in keywordMap[actionType])
            {
                if (combined.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    return actionType;
                }
            }
        }

        // Default
        return BasicActionTypes.Discuss;
    }

    private IEnvironmentalProperty ParseEnvironmentalProperty(string propertyString)
    {
        // Create a dictionary to map property strings to their respective objects
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

        // Look up the property in the dictionary
        if (propertyMap.TryGetValue(propertyString, out IEnvironmentalProperty property))
        {
            return property;
        }

        // Default fallback
        return Illumination.Bright; // Provide a default rather than null
    }

    #endregion
}