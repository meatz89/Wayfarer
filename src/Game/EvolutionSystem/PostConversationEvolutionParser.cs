using System.Text.Json;

public class PostConversationEvolutionParser
{
    private ILogger<PostConversationEvolutionParser> _logger;

    public PostConversationEvolutionParser(
        ILogger<PostConversationEvolutionParser> logger = null)
    {
        _logger = logger;
    }

    public async Task<PostConversationEvolutionResult> ParsePostConversationEvolutionResponseAsync(string response)
    {
        FlatPostConversationEvolutionResponse flatResponse = ParseFlatResponse(response);
        PostConversationEvolutionResult postConversationEvolutionResult = await BuildNestedResponseAsync(flatResponse);
        return postConversationEvolutionResult;
    }

    private FlatPostConversationEvolutionResponse ParseFlatResponse(string response)
    {
        FlatPostConversationEvolutionResponse result = new FlatPostConversationEvolutionResponse();
        response = response.Replace("```json", "");
        response = response.Replace("```", "");

        try
        {
            using JsonDocument doc = JsonDocument.Parse(response);
            JsonElement root = doc.RootElement;

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
                element =>
                {
                    return ParseRelationshipChange(element);
                });

            // Parse locations
            result.Locations = GetArrayOfType(
                root,
                "locations",
                element =>
                {
                    return ParseLocationDefinition(element);
                });

            // Parse location spots
            result.LocationSpots = GetArrayOfType(
                root,
                "locationSpots",
                element =>
                {
                    return ParseLocationSpotDefinition(element);
                });

            // Parse action definitions
            result.ActionDefinitions = GetArrayOfType(
                root,
                "actionDefinitions",
                element =>
                {
                    return ParseActionDefinition(element);
                });

            // Parse characters
            result.Characters = GetArrayOfType(
                root,
                "characters",
                element =>
                {
                    return ParseCharacter(element);
                });

        }
        catch (Exception ex)
        {
            LogError("Error parsing flat response", ex);
        }

        return result;
    }

    private async Task<PostConversationEvolutionResult> BuildNestedResponseAsync(FlatPostConversationEvolutionResponse flatResponse)
    {
        PostConversationEvolutionResult result = new PostConversationEvolutionResult
        {
            ResourceChanges = flatResponse.ResourceChanges ?? new ResourceChanges(),
            RelationshipChanges = flatResponse.RelationshipChanges ?? new List<RelationshipChange>(),
            CoinChange = flatResponse.ResourceChanges?.CoinChange ?? 0,
            NewLocationSpots = new List<LocationSpot>(),
            NewActions = new List<NewAction>(),
            NewCharacters = flatResponse.Characters ?? new List<NPC>(),
            NewLocations = new List<Location>(),
            // Contract system removed - using letter system instead
        };

        // Process action definitions
        foreach (EvolutionActionTemplate actionDef in flatResponse.ActionDefinitions)
        {
            try
            {
                // Add to the NewActions collection
                result.NewActions.Add(new NewAction
                {
                    SpotName = actionDef.SpotName,
                    LocationName = actionDef.LocationName,
                    Name = actionDef.Name,
                    Description = actionDef.Description,
                    Goal = actionDef.Description,
                    ActionType = actionDef.ActionType,
                    IsRepeatable = actionDef.IsRepeatable,
                    StaminaCost = actionDef.StaminaCost
                });
            }
            catch (Exception ex)
            {
                LogError($"Failed to generate action for {actionDef.Name}", ex);
            }
        }

        // Create location spots
        Dictionary<string, List<LocationSpot>> spotsByLocation = new Dictionary<string, List<LocationSpot>>();

        foreach (LocationSpotDefinition spotDef in flatResponse.LocationSpots)
        {
            LocationSpot spot = new LocationSpot(spotDef.Name, spotDef.LocationId)
            {
                Description = spotDef.Description,
            };

            // Add to spots by location
            if (!spotsByLocation.ContainsKey(spotDef.LocationId))
            {
                spotsByLocation[spotDef.LocationId] = new List<LocationSpot>();
            }
            spotsByLocation[spotDef.LocationId].Add(spot);

            // Also add to NewLocationSpots if it's not part of a new location
            if (!flatResponse.Locations.Any(l =>
            {
                return l.Name == spotDef.Name;
            }))
            {
                result.NewLocationSpots.Add(spot);
            }
        }

        // Create locations with their spots
        foreach (LocationDefinition locDef in flatResponse.Locations)
        {
            string locationId = locDef.Name.Replace(" ", "_").ToLowerInvariant();

            Location location = new Location(locationId, locDef.Name)
            {
                Description = locDef.Description,
                Difficulty = locDef.Difficulty,
                Depth = locDef.Depth,
                LocationType = ParseLocationType(locDef.LocationType),
                AvailableServices = ParseServices(locDef.AvailableServices),
                // Discovery bonuses removed - emergent opportunities instead
                HasBeenVisited = false,
                VisitCount = 0,
                ConnectedLocationIds = locDef.ConnectedTo
            };

            result.NewLocations.Add(location);
        }

        return result;
    }

    private LocationTypes ParseLocationType(string locationTypeStr)
    {
        if (Enum.TryParse<LocationTypes>(locationTypeStr, true, out LocationTypes locationType))
        {
            return locationType;
        }
        return LocationTypes.Connective; // Default fallback
    }

    private List<ServiceTypes> ParseServices(List<string> serviceStrings)
    {
        List<ServiceTypes> services = new List<ServiceTypes>();

        foreach (string serviceStr in serviceStrings)
        {
            if (Enum.TryParse<ServiceTypes>(serviceStr, true, out ServiceTypes service))
            {
                services.Add(service);
            }
        }

        return services;
    }

    private RelationshipChange ParseRelationshipChange(JsonElement element)
    {
        return SafeParseEntity("relationship change", () =>
        {
            return new RelationshipChange
            {
                CharacterName = GetStringProperty(element, "characterName", "Unknown Character"),
                ChangeAmount = GetIntProperty(element, "changeAmount", 0),
                Reason = GetStringProperty(element, "reason", "Unspecified reason")
            };
        });
    }

    private LocationDefinition ParseLocationDefinition(JsonElement element)
    {
        return SafeParseEntity("location", () =>
        {
            return new LocationDefinition
            {
                Name = GetStringProperty(element, "name", "Unnamed Location"),
                Description = GetStringProperty(element, "description", "No description available."),
                Difficulty = GetIntProperty(element, "difficulty", 1),
                Depth = GetIntProperty(element, "depth", 0),
                LocationType = GetStringProperty(element, "locationType", "Connective"),
                AvailableServices = GetStringArray(element, "availableServices"),
                DiscoveryBonusXP = GetIntProperty(element, "discoveryBonusXP", 10),
                DiscoveryBonusCoins = GetIntProperty(element, "discoveryBonusCoins", 5),
                ConnectedTo = GetStringArrayOrSingle(element, "connectedTo"),
                EnvironmentalProperties = GetStringArray(element, "environmentalProperties")
            };
        });
    }

    private LocationSpotDefinition ParseLocationSpotDefinition(JsonElement element)
    {
        return SafeParseEntity("location spot", () =>
        {
            return new LocationSpotDefinition
            {
                Name = GetStringProperty(element, "name", "Unnamed Spot"),
                Description = GetStringProperty(element, "description", "No description available."),
                LocationId = GetStringProperty(element, "locationName", "Unknown Location")
            };
        });
    }

    private EvolutionActionTemplate ParseActionDefinition(JsonElement element)
    {
        return SafeParseEntity("action definition", () =>
        {
            return new EvolutionActionTemplate
            {
                Name = GetStringProperty(element, "name", "Unnamed Action"),
                Description = GetStringProperty(element, "description", "No description available."),
                Goal = GetStringProperty(element, "goal", "Unknown goal"),
                ActionType = GetStringProperty(element, "actionType", "Discuss"),
                SpotName = GetStringProperty(element, "spotName", "Unknown Spot"),
                LocationName = GetStringProperty(element, "locationName", "Unknown Location"),
                IsRepeatable = GetBoolProperty(element, "isRepeatable", false),
                StaminaCost = GetIntProperty(element, "staminaCost", 1)
            };
        });
    }

    private NPC ParseCharacter(JsonElement element)
    {
        return SafeParseEntity("character", () =>
        {
            return new NPC
            {
                Name = GetStringProperty(element, "name", "Unnamed Character"),
                Role = GetStringProperty(element, "role", "Unknown Role"),
                Description = GetStringProperty(element, "description", "No description available."),
                Location = GetStringProperty(element, "location", "Unknown Location")
            };
        });
    }

    private ILocationProperty ParseEnvironmentalProperty(string propertyString)
    {
        Dictionary<string, ILocationProperty> propertyMap = new Dictionary<string, ILocationProperty>(StringComparer.OrdinalIgnoreCase)
        {
            // Illumination properties
            { "Bright", Illumination.Bright },
            { "Thiefy", Illumination.Thiefy },
            { "Dark", Illumination.Dark },
            
            // Population properties
            { "Crowded", Population.Crowded },
            { "Quiet", Population.Quiet },
            { "Isolated", Population.Scholarly },
            
            // Physical properties
            { "Confined", Physical.Confined },
            { "Expansive", Physical.Expansive },
            { "Hazardous", Physical.Hazardous },
            
            // Atmosphere properties
            { "Tense", Atmosphere.Rough },
            { "Formal", Atmosphere.Tense },
            { "Chaotic", Atmosphere.Chaotic }
        };

        if (propertyMap.TryGetValue(propertyString, out ILocationProperty property))
        {
            return property;
        }

        return Illumination.Bright; // Default fallback
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


}
