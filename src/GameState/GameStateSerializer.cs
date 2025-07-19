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
                // Card system removed - using letter queue system
            }
        };

        return JsonSerializer.Serialize(serialized, _jsonOptions);
    }

    public static GameWorld DeserializeGameWorld(string json, List<Location> locations, List<LocationSpot> spots)
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

        // Actions removed - using letter queue system


        // Card system removed - using letter queue system

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

            // Card system removed - using conversation and location action systems
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
                spots.Add(LocationSpotParser.ParseLocationSpot(spotElement.GetRawText()));
            }
        }

        return spots;
    }

    // Action system removed - using conversation and location action systems

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
            travelTimeHours = route.TravelTimeHours,
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

    // Contract serialization removed - using letter system only

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
