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
            CurrentLocationId = gameState.WorldState.CurrentLocation?.Id,
            CurrentLocationSpotId = gameState.WorldState.CurrentLocationSpot?.Id,
            CurrentDay = gameState.WorldState.CurrentDay,
            CurrentTimeHours = gameState.WorldState.CurrentTimeHours,

            Player = new SerializablePlayerState
            {
                Name = gameState.PlayerState.Name,
                Gender = gameState.PlayerState.IsInitialized ? gameState.PlayerState.Gender.ToString() : null,
                Archetype = gameState.PlayerState.IsInitialized ? gameState.PlayerState.Archetype.ToString() : null,
                Coins = gameState.PlayerState.Silver,
                MaxActionPoints = gameState.PlayerState.MaxActionPoints,
                ActionPoints = gameState.PlayerState.ActionPoints,
                MaxEnergy = gameState.PlayerState.MaxEnergyPoints,
                Energy = gameState.PlayerState.EnergyPoints,
                MaxHealth = gameState.PlayerState.MaxHealth,
                Health = gameState.PlayerState.Health,
                Level = gameState.PlayerState.Level,
                CurrentXP = gameState.PlayerState.CurrentXP,
                InventoryItems = gameState.PlayerState.Inventory.GetAllItems()
                    .Select(item => item.ToString())
                    .ToList(),
                // Add serialization for player's cards if needed
                SelectedCards = gameState.PlayerState.PlayerHandCards?.Select(c => c.Id).ToList() ?? new List<string>()
            }
        };

        return JsonSerializer.Serialize(serialized, _jsonOptions);
    }

    public static GameState DeserializeGameState(string json, List<Location> locations, List<LocationSpot> spots,
            List<ActionDefinition> actions, List<CommissionDefinition> commissions, List<CardDefinition> cards)
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
        
        gameState.WorldState.commissions.Clear();
        gameState.WorldState.commissions.AddRange(commissions);

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
                gameState.PlayerState.Initialize(serialized.Player.Name, archetype, gender);
            }

            // Apply resources
            gameState.PlayerState.Silver = serialized.Player.Coins;
            gameState.PlayerState.MaxActionPoints = serialized.Player.MaxActionPoints;
            gameState.PlayerState.ActionPoints = serialized.Player.ActionPoints;
            gameState.PlayerState.MaxEnergyPoints = serialized.Player.MaxEnergy;
            gameState.PlayerState.EnergyPoints = serialized.Player.Energy;
            gameState.PlayerState.MaxHealth = serialized.Player.MaxHealth;
            gameState.PlayerState.Health = serialized.Player.Health;

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

            // Apply selected cards if available
            if (serialized.Player.SelectedCards != null && cards != null)
            {
                gameState.PlayerState.PlayerHandCards = new List<CardDefinition>();
                foreach (string cardId in serialized.Player.SelectedCards)
                {
                    CardDefinition card = cards.FirstOrDefault(c => c.Id == cardId);
                    if (card != null)
                    {
                        gameState.PlayerState.PlayerHandCards.Add(card);
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
                        s.Id == serialized.CurrentLocationSpotId);
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
            id = spot.Id,
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

    public static List<CommissionDefinition> DeserializeCommissions(string json)
    {
        List<CommissionDefinition> commissions = new List<CommissionDefinition>();

        using (JsonDocument doc = JsonDocument.Parse(json))
        {
            foreach (JsonElement commissionElement in doc.RootElement.EnumerateArray())
            {
                commissions.Add(CommissionParser.ParseCommission(commissionElement.GetRawText()));
            }
        }

        return commissions;
    }

    // New methods for card serialization
    public static string SerializeCards(List<CardDefinition> cards)
    {
        List<object> serializableCards = cards.Select(card => (object)new
        {
            id = card.Id,
            name = card.Name,
            type = card.Type.ToString(),
            skill = card.Skill.ToString(),
            level = card.Level,
            cost = card.EnergyCost,
            gain = card.SkillBonus,
            tags = card.Tags
        }).ToList();

        return JsonSerializer.Serialize(serializableCards, _jsonOptions);
    }

    public static List<CardDefinition> DeserializeCards(string json)
    {
        List<CardDefinition> cards = new List<CardDefinition>();

        using (JsonDocument doc = JsonDocument.Parse(json))
        {
            foreach (JsonElement cardElement in doc.RootElement.EnumerateArray())
            {
                cards.Add(CardParser.ParseCard(cardElement.GetRawText()));
            }
        }

        return cards;
    }
}
