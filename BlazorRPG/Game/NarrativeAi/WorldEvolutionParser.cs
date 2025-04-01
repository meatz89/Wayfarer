using System.Text.Json;

public class WorldEvolutionParser
{
    public ActionRepository ActionRepository { get; }

    public WorldEvolutionParser(ActionRepository actionRepository)
    {
        ActionRepository = actionRepository;
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

            // Process each entity type separately
            ProcessLocationSpots(root, result);
            ProcessNewActions(root, result);
            ProcessNewCharacters(root, result);
            ProcessNewLocations(root, result);
            ProcessNewOpportunities(root, result);
            ProcessCoinChange(root, result);
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

    #region Entity Processing Methods

    private void ProcessLocationSpots(JsonElement root, WorldEvolutionResponse result)
    {
        ProcessArrayProperty(root, "newLocationSpots", element =>
        {
            LocationSpot spot = ParseLocationSpot(element);
            if (spot != null)
            {
                result.NewLocationSpots.Add(spot);
            }
        });
    }

    private void ProcessNewActions(JsonElement root, WorldEvolutionResponse result)
    {
        ProcessArrayProperty(root, "newActions", element =>
        {
            NewAction action = ParseNewAction(element);
            if (action != null)
            {
                result.NewActions.Add(action);
            }
        });
    }

    private void ProcessNewCharacters(JsonElement root, WorldEvolutionResponse result)
    {
        ProcessArrayProperty(root, "newCharacters", element =>
        {
            Character character = ParseCharacter(element);
            if (character != null)
            {
                result.NewCharacters.Add(character);
            }
        });
    }

    private void ProcessNewLocations(JsonElement root, WorldEvolutionResponse result)
    {
        ProcessArrayProperty(root, "newLocations", element =>
        {
            Location location = ParseLocation(element);
            if (location != null)
            {
                result.NewLocations.Add(location);
            }
        });
    }

    private void ProcessNewOpportunities(JsonElement root, WorldEvolutionResponse result)
    {
        ProcessArrayProperty(root, "newOpportunities", element =>
        {
            Opportunity opportunity = ParseOpportunity(element);
            if (opportunity != null)
            {
                result.NewOpportunities.Add(opportunity);
            }
        });
    }

    #endregion

    #region Entity Parsers

    private LocationSpot ParseLocationSpot(JsonElement element)
    {
        return SafeParseEntity("location spot", () =>
        {
            LocationSpot spot = new LocationSpot
            {
                Name = GetStringProperty(element, "name", "Unnamed Spot"),
                Description = GetStringProperty(element, "description", "No description available."),
                InteractionType = GetStringProperty(element, "interactionType", "Feature"),
                ActionTemplates = new List<string>()
            };

            // Process actions for this spot
            ProcessArrayProperty(element, "actions", actionElement =>
            {
                ActionTemplate actionTemplate = ParseAction(actionElement);
                ActionRepository.GetOrCreateAction(
                    actionTemplate.Name,
                    actionTemplate.Goal,
                    actionTemplate.Complication,
                    actionTemplate.ActionType,
                    actionTemplate.EncounterTemplateName
                );

                if (actionTemplate != null)
                {
                    spot.ActionTemplates.Add(actionTemplate.Name);
                }
            });

            return spot;
        });
    }

    private NewAction ParseNewAction(JsonElement element)
    {
        return SafeParseEntity("new action", () => new NewAction
        {
            SpotName = GetStringProperty(element, "spotName", "Unknown Spot"),
            Name = GetStringProperty(element, "name", "Unnamed Action"),
            Description = GetStringProperty(element, "description", "No description available."),
            Goal = GetStringProperty(element, "goal", "Unknown goal"),
            Complication = GetStringProperty(element, "complication", "Unknown complication"),
            ActionType = GetStringProperty(element, "actionType", "Unkown Action Type")
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

            // Parse ConnectedTo property (handle both array and string)
            ParseConnectedLocations(element, location);

            // Parse environmental properties
            ParseEnvironmentalProperties(element, location);

            // Parse location spots
            ProcessArrayProperty(element, "spots", spotElement =>
            {
                LocationSpot spot = ParseLocationSpot(spotElement);
                if (spot != null)
                {
                    location.Spots.Add(spot);
                }
            });

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

            // Create action template and convert to implementation
            ActionTemplate template = new ActionTemplateBuilder()
                .WithName(ParseActionName(actionName))
                .WithGoal(goal)
                .WithComplication(complication)
                .WithActionType(actionType)
                .Build();

            return template;
        });
    }

    #endregion

    #region Helper Methods

    private WorldEvolutionResponse InitializeEmptyResponse()
    {
        return new WorldEvolutionResponse
        {
            NewLocationSpots = new List<LocationSpot>(),
            NewActions = new List<NewAction>(),
            NewCharacters = new List<Character>(),
            NewLocations = new List<Location>(),
            NewOpportunities = new List<Opportunity>()
        };
    }

    private void ProcessArrayProperty(JsonElement element, string propertyName, Action<JsonElement> processor)
    {
        if (element.TryGetProperty(propertyName, out JsonElement arrayElement) &&
            arrayElement.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement item in arrayElement.EnumerateArray())
            {
                processor(item);
            }
        }
    }

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

    private void ParseConnectedLocations(JsonElement element, Location location)
    {
        if (element.TryGetProperty("connectedTo", out JsonElement connectedToElement))
        {
            if (connectedToElement.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement conn in connectedToElement.EnumerateArray())
                {
                    if (conn.ValueKind == JsonValueKind.String)
                    {
                        location.ConnectedTo.Add(conn.GetString() ?? "Unknown Connection");
                    }
                }
            }
            else if (connectedToElement.ValueKind == JsonValueKind.String)
            {
                string connectedLocation = connectedToElement.GetString() ?? "Unknown Connection";
                location.ConnectedTo.Add(connectedLocation);
            }
        }
    }

    private void ParseEnvironmentalProperties(JsonElement element, Location location)
    {
        ProcessArrayProperty(element, "environmentalProperties", prop =>
        {
            if (prop.ValueKind == JsonValueKind.String)
            {
                IEnvironmentalProperty envProp = ParseEnvironmentalProperty(prop.GetString() ?? "");
                if (envProp != null)
                {
                    location.EnvironmentalProperties.Add(envProp);
                }
            }
        });
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

    private void ProcessCoinChange(JsonElement root, WorldEvolutionResponse result)
    {
        if (root.TryGetProperty("coinChange", out JsonElement coinChangeElement) &&
            coinChangeElement.ValueKind == JsonValueKind.Number)
        {
            result.CoinChange = coinChangeElement.GetInt32();
        }
    }

    private void LogError(string message, Exception ex)
    {
        Console.WriteLine($"{message}: {ex.Message}");
    }

    #endregion

    #region Type Conversion Methods

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
        if (propertyMap.TryGetValue(propertyString, out IEnvironmentalProperty? property))
        {
            return property;
        }

        // Default fallback
        return Illumination.Bright; // Provide a default rather than null
    }

    private ActionNames ParseActionName(string actionName)
    {
        // Try direct enum parse first
        if (Enum.TryParse<ActionNames>(actionName.Replace(" ", ""), true, out ActionNames result))
        {
            return result;
        }

        // Create a dictionary to map keywords to ActionNames
        Dictionary<string, ActionNames> keywordMap = new Dictionary<string, ActionNames>
        {
            { "Trade", ActionNames.TradeGoods },
            { "Gather", ActionNames.VillageGathering },
            { "Travel", ActionNames.ForestTravel },
            { "Path", ActionNames.ForestTravel },
            { "Meet", ActionNames.SecretMeeting },
            { "Deal", ActionNames.SecretDeal },
            { "Bargain", ActionNames.SecretDeal },
            { "Rest", ActionNames.RentRoom },
            { "Room", ActionNames.RentRoom },
            { "Sleep", ActionNames.RentRoom },
            { "Quest", ActionNames.FindQuests },
            { "Job", ActionNames.FindQuests }
        };

        // Check for keyword matches
        foreach (string keyword in keywordMap.Keys)
        {
            if (actionName.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                return keywordMap[keyword];
            }
        }

        // Default fallback
        return ActionNames.VillageGathering;
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

    #endregion
}