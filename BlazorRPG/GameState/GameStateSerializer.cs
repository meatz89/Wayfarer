using System.Text.Json;

public static class GameStateSerializer
{
    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        WriteIndented = true
    };

    public static string SerializeGameState(GameState gameState)
    {
        SerializableGameState serialized = new SerializableGameState
        {
            CurrentLocationId = gameState.WorldState.CurrentLocation?.Name,
            CurrentLocationSpotId = gameState.WorldState.CurrentLocationSpot?.Name,
            CurrentDay = gameState.WorldState.CurrentDay,
            CurrentTimeHours = gameState.WorldState.CurrentTimeHours,

            Player = new SerializablePlayerState
            {
                Name = gameState.PlayerState.Name,
                Gender = gameState.PlayerState.Gender.ToString(),
                Archetype = gameState.PlayerState.Archetype.ToString(),
                Coins = gameState.PlayerState.Coins,
                Food = gameState.PlayerState.Food,
                MedicinalHerbs = gameState.PlayerState.MedicinalHerbs,
                Health = gameState.PlayerState.Health,
                Energy = gameState.PlayerState.Energy,
                Concentration = gameState.PlayerState.Concentration,
                Confidence = gameState.PlayerState.Confidence,
                Level = gameState.PlayerState.Level,
                CurrentXP = gameState.PlayerState.CurrentXP,
                InventoryItems = gameState.PlayerState.Inventory.GetAllItems()
                    .Select(item =>
                    {
                        return item.ToString();
                    })
                    .ToList()
            }
        };

        return JsonSerializer.Serialize(serialized, _jsonOptions);
    }

    public static GameState DeserializeGameState(string json, List<Location> locations, List<LocationSpot> spots, List<ActionDefinition> actions)
    {
        SerializableGameState serialized = JsonSerializer.Deserialize<SerializableGameState>(json, _jsonOptions);
        if (serialized == null)
        {
            throw new JsonException("Failed to deserialize game state");
        }

        GameState gameState = new GameState();

        // Apply loaded content to world state
        gameState.WorldState.locations.Clear();
        gameState.WorldState.locations.AddRange(locations);

        gameState.WorldState.locationSpots.Clear();
        gameState.WorldState.locationSpots.AddRange(spots);

        gameState.WorldState.actions.Clear();
        gameState.WorldState.actions.AddRange(actions);

        // Apply basic state data
        gameState.WorldState.CurrentDay = serialized.CurrentDay;
        gameState.WorldState.CurrentTimeHours = serialized.CurrentTimeHours;

        // Apply player state if character exists
        if (!string.IsNullOrEmpty(serialized.Player.Name))
        {
            if (Enum.TryParse<Genders>(serialized.Player.Gender, out Genders gender) &&
                Enum.TryParse<ArchetypeTypes>(serialized.Player.Archetype, out ArchetypeTypes archetype))
            {
                // Initialize player
                gameState.PlayerState.Initialize(serialized.Player.Name, archetype, gender);
            }

            // Apply resources
            gameState.PlayerState.Coins = serialized.Player.Coins;
            gameState.PlayerState.Food = serialized.Player.Food;
            gameState.PlayerState.MedicinalHerbs = serialized.Player.MedicinalHerbs;
            gameState.PlayerState.Health = serialized.Player.Health;
            gameState.PlayerState.Energy = serialized.Player.Energy;
            gameState.PlayerState.Concentration = serialized.Player.Concentration;
            gameState.PlayerState.Confidence = serialized.Player.Confidence;

            // Apply progression
            gameState.PlayerState.Level = serialized.Player.Level;
            gameState.PlayerState.CurrentXP = serialized.Player.CurrentXP;

            // Apply inventory
            gameState.PlayerState.Inventory.Clear();
            foreach (string itemName in serialized.Player.InventoryItems)
            {
                if (Enum.TryParse<ItemTypes>(itemName, out ItemTypes itemType))
                {
                    gameState.PlayerState.Inventory.AddItem(itemType);
                }
            }
        }

        // Set current location and spot
        if (!string.IsNullOrEmpty(serialized.CurrentLocationId))
        {
            Location currentLocation = locations.FirstOrDefault(l =>
            {
                return l.Name == serialized.CurrentLocationId;
            });
            if (currentLocation != null)
            {
                LocationSpot currentSpot = null;
                if (!string.IsNullOrEmpty(serialized.CurrentLocationSpotId))
                {
                    currentSpot = spots.FirstOrDefault(s =>
                    {
                        return s.LocationName == serialized.CurrentLocationId &&
                                                s.Name == serialized.CurrentLocationSpotId;
                    });
                }

                if (currentSpot == null)
                {
                    currentSpot = spots.FirstOrDefault(s =>
                    {
                        return s.LocationName == serialized.CurrentLocationId;
                    });
                }

                if (currentSpot != null)
                {
                    gameState.WorldState.SetCurrentLocation(currentLocation, currentSpot);
                }
            }
        }

        return gameState;
    }

    public static string SerializeLocations(List<Location> locations)
    {
        // Make sure we're serializing a list/array, not a single object
        if (locations.Count == 0)
        {
            // Return empty array if no locations
            return "[]";
        }

        // Create array of anonymous objects for serialization
        var serializableLocations = locations.Select(loc =>
        {
            return new
            {
                id = loc.Name,
                name = loc.Name,
                description = loc.Description,
                detailedDescription = loc.DetailedDescription,
                connectedTo = loc.ConnectedTo
            };
        }).ToList();

        // Serialize the list as a JSON array
        return JsonSerializer.Serialize(serializableLocations, _jsonOptions);
    }

    public static List<Location> DeserializeLocations(string json)
    {
        List<Location> locations = new List<Location>();

        // Handle empty arrays or null
        if (string.IsNullOrWhiteSpace(json) || json == "[]")
        {
            return locations;
        }

        using (JsonDocument doc = JsonDocument.Parse(json))
        {
            // Ensure we're dealing with an array
            if (doc.RootElement.ValueKind != JsonValueKind.Array)
            {
                throw new FormatException("Location JSON must be an array of location objects");
            }

            foreach (JsonElement locationElement in doc.RootElement.EnumerateArray())
            {
                locations.Add(LocationParser.ParseLocation(locationElement.GetRawText()));
            }
        }

        return locations;
    }

    public static string SerializeLocationSpots(List<LocationSpot> spots)
    {
        List<object> serializableSpots = spots.Select(spot =>
        {
            return new
            {
                name = spot.Name,
                locationId = spot.LocationName,
                description = spot.Description,
                currentLevel = spot.CurrentLevel,
                currentXP = spot.CurrentSpotXP,
                xpToNextLevel = spot.XPToNextLevel,
                levels = spot.LevelData.Select(level =>
                {
                    return new
                    {
                        level = level.Level,
                        description = level.Description,
                        actionIds = level.AddedActionIds,
                        removedActionIds = level.RemovedActionIds,
                        levelUpEncounter = new
                        {
                            id = level.EncounterActionId
                        }
                    };
                }).ToList()
            };
        }).ToList<object>();

        return JsonSerializer.Serialize(serializableSpots, _jsonOptions);
    }

    public static List<LocationSpot> DeserializeLocationSpots(string json)
    {
        List<LocationSpot> spots = new List<LocationSpot>();

        using (JsonDocument doc = JsonDocument.Parse(json))
        {
            foreach (JsonElement spotElement in doc.RootElement.EnumerateArray())
            {
                spots.Add(LocationParser.ParseLocationSpot(spotElement.GetRawText()));
            }
        }

        return spots;
    }

    public static string SerializeActions(List<ActionDefinition> actions)
    {
        List<object> serializableActions = actions.Select(action =>
        {
            return new
            {
                id = action.Id,
                name = action.Name,
                description = action.Description,
                actionType = action.ActionType.ToString(),
                timeWindows = action.TimeWindows?.Select(tw =>
                {
                    return tw.ToString();
                }).ToList(),
                costs = new
                {
                    energy = action.EnergyCost,
                    concentration = action.ConcentrationCost,
                    confidence = action.ConfidenceCost,
                    health = action.HealthCost,
                    coin = action.CoinCost,
                    time = action.TimeCost
                },
                yields = new
                {
                    energy = action.RestoresEnergy,
                    concentration = action.RestoresConcentration,
                    confidence = action.RestoresConfidence,
                    health = action.RestoresHealth,
                    coin = action.CoinGain,
                    relationships = action.RelationshipChanges?.Select(r =>
                    {
                        return new
                        {
                            characterName = r.CharacterName,
                            amount = r.ChangeAmount
                        };
                    }).ToList(),
                    spotXp = action.SpotXp
                },
                encounterDetails = new
                {
                    goal = action.Goal,
                    complication = action.Complication,
                    isOneTimeEncounter = action.IsOneTimeEncounter,
                    encounterType = action.EncounterType.ToString(),
                    difficulty = action.Difficulty
                },
                moveToLocation = action.MoveToLocation,
                moveToLocationSpot = action.MoveToLocationSpot
            };
        }).ToList<object>();

        return JsonSerializer.Serialize(serializableActions, _jsonOptions);
    }

    public static List<ActionDefinition> DeserializeActions(string json)
    {
        List<ActionDefinition> actions = new List<ActionDefinition>();

        using (JsonDocument doc = JsonDocument.Parse(json))
        {
            foreach (JsonElement actionElement in doc.RootElement.EnumerateArray())
            {
                actions.Add(ActionParser.ParseAction(actionElement.GetRawText()));
            }
        }

        return actions;
    }
}