using System.Text.Json;

public static class GameStateSerializer
{
    private static JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        WriteIndented = true
    };

    public static string SerializeGameState(GameWorld gameState)
    {
        SerializableGameState serialized = new SerializableGameState
        {
            CurrentLocationId = gameState.WorldState.CurrentLocation?.Id,
            CurrentLocationSpotId = gameState.WorldState.CurrentLocationSpot?.SpotID,
            CurrentDay = gameState.WorldState.CurrentDay,
            CurrentTimeHours = gameState.WorldState.CurrentTimeHours,

            Player = new SerializablePlayerState
            {
                Name = gameState.Player.Name,
                Gender = gameState.Player.IsInitialized ? gameState.Player.Gender.ToString() : null,
                Archetype = gameState.Player.IsInitialized ? gameState.Player.Archetype.ToString() : null,
                Coins = gameState.Player.Money,
                MaxActionPoints = gameState.Player.MaxActionPoints,
                ActionPoints = gameState.Player.ActionPoints,
                MaxEnergy = gameState.Player.MaxEnergy,
                Energy = gameState.Player.Energy,
                MaxHealth = gameState.Player.MaxHealth,
                Health = gameState.Player.Health,
                Level = gameState.Player.Level,
                CurrentXP = gameState.Player.CurrentXP,
                InventoryItems = gameState.Player.Inventory.GetAllItems()
                    .Select(item => item.ToString())
                    .ToList(),
                // Add serialization for player's cards if needed
                SelectedCards = gameState.Player.AvailableCards?.Select(c => c.Id).ToList() ?? new List<string>()
            }
        };

        return JsonSerializer.Serialize(serialized, _jsonOptions);
    }

    public static GameWorld DeserializeGameState(string json, List<Location> locations, List<LocationSpot> spots,
            List<ActionDefinition> actions, List<OpportunityDefinition> Opportunities, List<SkillCard> cards)
    {
        SerializableGameState serialized = JsonSerializer.Deserialize<SerializableGameState>(json, _jsonOptions);
        if (serialized == null)
        {
            throw new JsonException("Failed to deserialize game state");
        }

        GameWorld gameState = new GameWorld();

        // Apply loaded content to world state
        gameState.WorldState.locations.Clear();
        gameState.WorldState.locations.AddRange(locations);

        gameState.WorldState.locationSpots.Clear();
        gameState.WorldState.locationSpots.AddRange(spots);

        gameState.WorldState.actions.Clear();
        gameState.WorldState.actions.AddRange(actions);

        gameState.WorldState.Opportunities.Clear();
        gameState.WorldState.Opportunities.AddRange(Opportunities);

        // Add cards to world state if applicable
        if (gameState.WorldState.AllCards != null)
        {
            gameState.WorldState.AllCards.Clear();
            gameState.WorldState.AllCards.AddRange(cards);
        }

        // Apply basic state data
        gameState.WorldState.CurrentDay = serialized.CurrentDay;
        gameState.TimeManager.SetNewTime(serialized.CurrentTimeHours);

        // Apply player state if character exists
        if (!string.IsNullOrEmpty(serialized.Player.Name))
        {
            if (Enum.TryParse<Genders>(serialized.Player.Gender, out Genders gender) &&
                Enum.TryParse<Professions>(serialized.Player.Archetype, out Professions archetype))
            {
                // Initialize player
                gameState.Player.Initialize(serialized.Player.Name, archetype, gender);
            }

            // Apply resources
            gameState.Player.Money = serialized.Player.Coins;
            gameState.Player.MaxActionPoints = serialized.Player.MaxActionPoints;
            gameState.Player.ActionPoints = serialized.Player.ActionPoints;
            gameState.Player.MaxEnergy = serialized.Player.MaxEnergy;
            gameState.Player.Energy = serialized.Player.Energy;
            gameState.Player.MaxHealth = serialized.Player.MaxHealth;
            gameState.Player.Health = serialized.Player.Health;

            // Apply progression
            gameState.Player.Level = serialized.Player.Level;
            gameState.Player.CurrentXP = serialized.Player.CurrentXP;

            // Apply inventory
            gameState.Player.Inventory.Clear();
            foreach (string itemName in serialized.Player.InventoryItems)
            {
                if (Enum.TryParse<ItemTypes>(itemName, out ItemTypes itemType))
                {
                    gameState.Player.Inventory.AddItem(itemType);
                }
            }

            // Apply selected cards if available
            if (serialized.Player.SelectedCards != null && cards != null)
            {
                gameState.Player.AvailableCards = new List<SkillCard>();
                foreach (string cardId in serialized.Player.SelectedCards)
                {
                    SkillCard card = cards.FirstOrDefault(c => c.Id == cardId);
                    if (card != null)
                    {
                        gameState.Player.AvailableCards.Add(card);
                    }
                }
            }
        }

        // Set current location and spot
        if (!string.IsNullOrEmpty(serialized.CurrentLocationId))
        {
            Location currentLocation = locations.FirstOrDefault(l => l.Id == serialized.CurrentLocationId);

            if (currentLocation != null)
            {
                LocationSpot currentSpot = null;
                if (!string.IsNullOrEmpty(serialized.CurrentLocationSpotId))
                {
                    currentSpot = spots.FirstOrDefault(s =>
                        s.LocationId == serialized.CurrentLocationId &&
                        s.SpotID == serialized.CurrentLocationSpotId);
                }

                if (currentSpot == null)
                {
                    currentSpot = spots.FirstOrDefault(s => s.LocationId == serialized.CurrentLocationId);
                }

                if (currentSpot != null)
                {
                    gameState.SetCurrentLocation(currentLocation, currentSpot);
                }
            }
        }

        return gameState;
    }

    public static string SerializeLocations(List<Location> locations)
    {
        if (locations.Count == 0)
        {
            return "[]";
        }

        // Cast to List<object> explicitly with updated structure
        List<object> serializableLocations = locations.Select(loc => (object)new
        {
            id = loc.Id,
            name = loc.Name,
            description = loc.Description,
            environmentalProperties = new
            {
                morning = loc.MorningProperties,
                afternoon = loc.AfternoonProperties,
                evening = loc.EveningProperties,
                night = loc.NightProperties
            },
            locationSpots = loc.LocationSpotIds,
            connectedTo = loc.ConnectedTo
        }).ToList();

        return JsonSerializer.Serialize(serializableLocations, _jsonOptions);
    }

    public static List<Location> DeserializeLocations(string json)
    {
        List<Location> locations = new List<Location>();

        if (string.IsNullOrWhiteSpace(json) || json == "[]")
        {
            return locations;
        }

        using (JsonDocument doc = JsonDocument.Parse(json))
        {
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
        List<object> serializableSpots = spots.Select(spot => (object)new
        {
            id = spot.SpotID,
            name = spot.Name,
            type = spot.Type,
            description = spot.Description,
            locationId = spot.LocationId,
            currentLevel = spot.CurrentLevel,
            currentXP = spot.CurrentSpotXP,
            xpToNextLevel = spot.XPToNextLevel
        }).ToList();

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
        // Updated to handle the new action structure with approaches
        List<object> serializableActions = actions.Select(action => (object)new
        {
            id = action.Id,
            name = action.Name,
            description = action.Description,
            spotId = action.LocationSpotId,

            // Time windows (string list)
            timeWindows = action.TimeWindows?.Select(tw => tw.ToString()).ToList(),

            moveToLocation = action.MoveToLocation,
            moveToLocationSpot = action.MoveToLocationSpot
        }).ToList();

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

    public static List<OpportunityDefinition> DeserializeOpportunities(string json)
    {
        List<OpportunityDefinition> Opportunities = new List<OpportunityDefinition>();

        using (JsonDocument doc = JsonDocument.Parse(json))
        {
            foreach (JsonElement opportunityElement in doc.RootElement.EnumerateArray())
            {
                Opportunities.Add(OpportunityParser.ParseOpportunity(opportunityElement.GetRawText()));
            }
        }

        return Opportunities;
    }
}
