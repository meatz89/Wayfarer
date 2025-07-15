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
                Name = gameWorld.GetPlayer().Name,
                Gender = gameWorld.GetPlayer().IsInitialized ? gameWorld.GetPlayer().Gender.ToString() : null,
                Archetype = gameWorld.GetPlayer().IsInitialized ? gameWorld.GetPlayer().Archetype.ToString() : null,
                Coins = gameWorld.GetPlayer().Coins,
                MaxStamina = gameWorld.GetPlayer().MaxStamina,
                Stamina = gameWorld.GetPlayer().Stamina,
                MaxHealth = gameWorld.GetPlayer().MaxHealth,
                Health = gameWorld.GetPlayer().Health,
                Level = gameWorld.GetPlayer().Level,
                CurrentXP = gameWorld.GetPlayer().CurrentXP,
                InventoryItems = gameWorld.GetPlayer().Inventory.GetAllItems()
                    .Select(item => item.ToString())
                    .ToList(),
                // Add serialization for player's cards if needed
                SelectedCards = gameWorld.GetPlayer().AvailableCards?.Select(c => c.Id).ToList() ?? new List<string>()
            }
        };

        return JsonSerializer.Serialize(serialized, _jsonOptions);
    }

    public static GameWorld DeserializeGameWorld(string json, List<Location> locations, List<LocationSpot> spots,
            List<ActionDefinition> actions, List<SkillCard> cards)
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

        gameWorld.WorldState.Contracts.Clear();

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
                gameWorld.GetPlayer().Initialize(serialized.Player.Name, archetype, gender);
            }

            // Apply resources
            gameWorld.GetPlayer().Coins = serialized.Player.Coins;
            gameWorld.GetPlayer().MaxStamina = serialized.Player.MaxStamina;
            gameWorld.GetPlayer().Stamina = serialized.Player.Stamina;
            gameWorld.GetPlayer().MaxHealth = serialized.Player.MaxHealth;
            gameWorld.GetPlayer().Health = serialized.Player.Health;

            // Apply progression
            gameWorld.GetPlayer().Level = serialized.Player.Level;
            gameWorld.GetPlayer().CurrentXP = serialized.Player.CurrentXP;

            // Apply inventory
            gameWorld.GetPlayer().Inventory.Clear();

            // Apply selected cards if available
            if (serialized.Player.SelectedCards != null && cards != null)
            {
                gameWorld.GetPlayer().AvailableCards = new List<SkillCard>();
                foreach (string cardId in serialized.Player.SelectedCards)
                {
                    SkillCard card = cards.FirstOrDefault(c => c.Id == cardId);
                    if (card != null)
                    {
                        gameWorld.GetPlayer().AvailableCards.Add(card);
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
                    gameWorld.WorldState.SetCurrentLocation(currentLocation, currentSpot);
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
            connectedTo = loc.Connections
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
            CurrentTimeBlocks = action.CurrentTimeBlocks?.Select(tw => tw.ToString()).ToList(),

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

    public static List<Contract> DeserializeContracts(string json)
    {
        List<Contract> Contracts = new List<Contract>();

        using (JsonDocument doc = JsonDocument.Parse(json))
        {
            foreach (JsonElement contractElement in doc.RootElement.EnumerateArray())
            {
                Contracts.Add(ContractParser.ParseContract(contractElement.GetRawText()));
            }
        }

        return Contracts;
    }
    public static string SerializeRouteOptions(List<RouteOption> routes)
    {
        List<object> serializableRoutes = routes.Select(route => (object)new
        {
            id = route.Id,
            name = route.Name,
            origin = route.Origin,
            destination = route.Destination,
            method = route.Method.ToString(),
            baseCoinCost = route.BaseCoinCost,
            baseStaminaCost = route.BaseStaminaCost,
            timeBlockCost = route.TimeBlockCost,
            departureTime = route.DepartureTime?.ToString(),
            isDiscovered = route.IsDiscovered,
            terrainCategories = route.TerrainCategories.Select(c => c.ToString()).ToList(),
            maxItemCapacity = route.MaxItemCapacity,
            description = route.Description
        }).ToList();

        return JsonSerializer.Serialize(serializableRoutes, _jsonOptions);
    }

    public static string SerializeItems(List<Item> items)
    {
        List<object> serializableItems = items.Select(item => (object)new
        {
            id = item.Id,
            name = item.Name,
            weight = item.Weight,
            buyPrice = item.BuyPrice,
            sellPrice = item.SellPrice,
            inventorySlots = item.InventorySlots,
            categories = item.Categories.Select(c => c.ToString()).ToList(),
            locationId = item.LocationId,
            spotId = item.SpotId,
            description = item.Description
        }).ToList();

        return JsonSerializer.Serialize(serializableItems, _jsonOptions);
    }

    public static string SerializeContracts(List<Contract> contracts)
    {
        List<object> serializableContracts = contracts.Select(contract => (object)new
        {
            id = contract.Id,
            description = contract.Description,
            startDay = contract.StartDay,
            dueDay = contract.DueDay,
            payment = contract.Payment,
            failurePenalty = contract.FailurePenalty,
            isCompleted = contract.IsCompleted,
            isFailed = contract.IsFailed,
            unlocksContractIds = contract.UnlocksContractIds,
            locksContractIds = contract.LocksContractIds
        }).ToList();

        return JsonSerializer.Serialize(serializableContracts, _jsonOptions);
    }

    public static List<RouteOption> DeserializeRouteOptions(string json)
    {
        List<RouteOption> routes = new List<RouteOption>();

        if (string.IsNullOrWhiteSpace(json) || json == "[]")
        {
            return routes;
        }

        using (JsonDocument doc = JsonDocument.Parse(json))
        {
            if (doc.RootElement.ValueKind != JsonValueKind.Array)
            {
                throw new FormatException("RouteOption JSON must be an array of route objects");
            }

            foreach (JsonElement routeElement in doc.RootElement.EnumerateArray())
            {
                routes.Add(RouteOptionParser.ParseRouteOption(routeElement.GetRawText()));
            }
        }

        return routes;
    }

    public static List<Item> DeserializeItems(string json)
    {
        List<Item> items = new List<Item>();

        if (string.IsNullOrWhiteSpace(json) || json == "[]")
        {
            return items;
        }

        using (JsonDocument doc = JsonDocument.Parse(json))
        {
            if (doc.RootElement.ValueKind != JsonValueKind.Array)
            {
                throw new FormatException("Item JSON must be an array of item objects");
            }

            foreach (JsonElement itemElement in doc.RootElement.EnumerateArray())
            {
                items.Add(ItemParser.ParseItem(itemElement.GetRawText()));
            }
        }

        return items;
    }
}
