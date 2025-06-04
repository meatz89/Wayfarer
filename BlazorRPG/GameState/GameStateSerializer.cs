using System.Text.Json;

public static class GameWorldSerializer
{
    private static JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        WriteIndented = true
    };

    public static string SerializeGameWorld(GameWorld gameWorld)
    {
        SerializableGameWorld serialized = new SerializableGameWorld
        {
            CurrentLocationId = gameWorld.WorldState.CurrentLocation?.Id,
            CurrentLocationSpotId = gameWorld.WorldState.CurrentLocationSpot?.SpotID,
            CurrentDay = gameWorld.WorldState.CurrentDay,
            CurrentTimeHours = gameWorld.WorldState.CurrentTimeHours,

            Player = new SerializablePlayerState
            {
                Name = gameWorld.Player.Name,
                Gender = gameWorld.Player.IsInitialized ? gameWorld.Player.Gender.ToString() : null,
                Archetype = gameWorld.Player.IsInitialized ? gameWorld.Player.Archetype.ToString() : null,
                Coins = gameWorld.Player.Money,
                MaxActionPoints = gameWorld.Player.MaxActionPoints,
                ActionPoints = gameWorld.Player.ActionPoints,
                MaxEnergy = gameWorld.Player.MaxEnergy,
                Energy = gameWorld.Player.Energy,
                MaxHealth = gameWorld.Player.MaxHealth,
                Health = gameWorld.Player.Health,
                Level = gameWorld.Player.Level,
                CurrentXP = gameWorld.Player.CurrentXP,
                InventoryItems = gameWorld.Player.Inventory.GetAllItems()
                    .Select(item => item.ToString())
                    .ToList(),
                // Add serialization for player's cards if needed
                SelectedCards = gameWorld.Player.AvailableCards?.Select(c => c.Id).ToList() ?? new List<string>()
            }
        };

        return JsonSerializer.Serialize(serialized, _jsonOptions);
    }

    public static GameWorld DeserializeGameWorld(string json, List<Location> locations, List<LocationSpot> spots,
            List<ActionDefinition> actions, List<OpportunityDefinition> Opportunities, List<SkillCard> cards)
    {
        SerializableGameWorld serialized = JsonSerializer.Deserialize<SerializableGameWorld>(json, _jsonOptions);
        if (serialized == null)
        {
            throw new JsonException("Failed to deserialize game state");
        }

        GameWorld gameWorld = new GameWorld();

        // Apply loaded content to world state
        gameWorld.WorldState.locations.Clear();
        gameWorld.WorldState.locations.AddRange(locations);

        gameWorld.WorldState.locationSpots.Clear();
        gameWorld.WorldState.locationSpots.AddRange(spots);

        gameWorld.WorldState.actions.Clear();
        gameWorld.WorldState.actions.AddRange(actions);

        gameWorld.WorldState.Opportunities.Clear();
        gameWorld.WorldState.Opportunities.AddRange(Opportunities);

        // Add cards to world state if applicable
        if (gameWorld.WorldState.AllCards != null)
        {
            gameWorld.WorldState.AllCards.Clear();
            gameWorld.WorldState.AllCards.AddRange(cards);
        }

        // Apply basic state data
        gameWorld.WorldState.CurrentDay = serialized.CurrentDay;
        gameWorld.TimeManager.SetNewTime(serialized.CurrentTimeHours);

        // Apply player state if character exists
        if (!string.IsNullOrEmpty(serialized.Player.Name))
        {
            if (Enum.TryParse<Genders>(serialized.Player.Gender, out Genders gender) &&
                Enum.TryParse<Professions>(serialized.Player.Archetype, out Professions archetype))
            {
                // Initialize player
                gameWorld.Player.Initialize(serialized.Player.Name, archetype, gender);
            }

            // Apply resources
            gameWorld.Player.Money = serialized.Player.Coins;
            gameWorld.Player.MaxActionPoints = serialized.Player.MaxActionPoints;
            gameWorld.Player.ActionPoints = serialized.Player.ActionPoints;
            gameWorld.Player.MaxEnergy = serialized.Player.MaxEnergy;
            gameWorld.Player.Energy = serialized.Player.Energy;
            gameWorld.Player.MaxHealth = serialized.Player.MaxHealth;
            gameWorld.Player.Health = serialized.Player.Health;

            // Apply progression
            gameWorld.Player.Level = serialized.Player.Level;
            gameWorld.Player.CurrentXP = serialized.Player.CurrentXP;

            // Apply inventory
            gameWorld.Player.Inventory.Clear();
            foreach (string itemName in serialized.Player.InventoryItems)
            {
                if (Enum.TryParse<ItemTypes>(itemName, out ItemTypes itemType))
                {
                    gameWorld.Player.Inventory.AddItem(itemType);
                }
            }

            // Apply selected cards if available
            if (serialized.Player.SelectedCards != null && cards != null)
            {
                gameWorld.Player.AvailableCards = new List<SkillCard>();
                foreach (string cardId in serialized.Player.SelectedCards)
                {
                    SkillCard card = cards.FirstOrDefault(c => c.Id == cardId);
                    if (card != null)
                    {
                        gameWorld.Player.AvailableCards.Add(card);
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
                    gameWorld.SetCurrentLocation(currentLocation, currentSpot);
                }
            }
        }

        return gameWorld;
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
