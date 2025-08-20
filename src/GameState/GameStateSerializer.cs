using System.Text.Json;

// IMPORTANT: For testing purposes, save/load functionality is DISABLED
// This serializer exists but is NOT used for persisting game state
// The game ALWAYS starts fresh - do not implement save/load functionality
public static class GameWorldSerializer
{
    private static JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        WriteIndented = true
    };

    private static int GetHoursFromTimeBlock(TimeBlocks timeBlock)
    {
        return timeBlock switch
        {
            TimeBlocks.Dawn => 6,
            TimeBlocks.Morning => 10,
            TimeBlocks.Afternoon => 14,
            TimeBlocks.Evening => 18,
            TimeBlocks.Night => 22,
            _ => 10 // Default to morning
        };
    }

    public static string SerializeGameWorld(GameWorld gameWorld, FlagService flagService = null, ConversationRepository conversationRepository = null)
    {
        SerializableGameWorld serialized = new SerializableGameWorld
        {
            CurrentLocationId = gameWorld.GetPlayer().CurrentLocationSpot?.LocationId,
            CurrentLocationSpotId = gameWorld.GetPlayer().CurrentLocationSpot?.SpotID,
            CurrentDay = gameWorld.CurrentDay,
            CurrentTimeHours = GetHoursFromTimeBlock(gameWorld.CurrentTimeBlock),

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

                // Letter Queue System
                LetterQueue = SerializeLetterQueue(gameWorld.GetPlayer().LetterQueue),
                ConnectionTokens = SerializeConnectionTokens(gameWorld.GetPlayer().ConnectionTokens),
                NPCTokens = SerializeNPCTokens(gameWorld.GetPlayer().NPCTokens),

                // Physical Letter Carrying
                CarriedLetters = gameWorld.GetPlayer().CarriedLetters.Select(SerializeLetter).ToList(),

                // Queue manipulation tracking
                LastMorningSwapDay = gameWorld.GetPlayer().LastMorningSwapDay,
                LastLetterBoardDay = gameWorld.GetPlayer().LastLetterBoardDay,
                DailyBoardLetters = gameWorld.GetPlayer().DailyBoardLetters.Select(SerializeLetter).ToList(),

                // Letter history tracking
                NPCLetterHistory = SerializeLetterHistory(gameWorld.GetPlayer().NPCLetterHistory),

                // Standing Obligations System
                StandingObligations = gameWorld.GetPlayer().StandingObligations.Select(SerializeStandingObligation).ToList(),

                // Token Favor System
                PurchasedFavors = new List<string>(gameWorld.GetPlayer().PurchasedFavors),
                UnlockedLocationIds = new List<string>(gameWorld.GetPlayer().UnlockedLocationIds),
                UnlockedServices = new List<string>(gameWorld.GetPlayer().UnlockedServices),

                // Scenario tracking
                DeliveredLetters = gameWorld.GetPlayer().DeliveredLetters.Select(SerializeLetter).ToList(),
                TotalLettersDelivered = gameWorld.GetPlayer().TotalLettersDelivered,
                TotalLettersExpired = gameWorld.GetPlayer().TotalLettersExpired,
                TotalTokensSpent = gameWorld.GetPlayer().TotalTokensSpent,

                // Patron tracking
                PatronLeverage = gameWorld.GetPlayer().PatronLeverage
            },

            // Narrative state
            FlagServiceState = flagService?.GetState()
        };

        return JsonSerializer.Serialize(serialized, _jsonOptions);
    }

    // Helper methods for serialization
    private static List<SerializableLetter> SerializeLetterQueue(Letter[] letterQueue)
    {
        List<SerializableLetter> result = new List<SerializableLetter>();
        for (int i = 0; i < letterQueue.Length; i++)
        {
            if (letterQueue[i] != null)
            {
                SerializableLetter serialized = SerializeLetter(letterQueue[i]);
                serialized.QueuePosition = i + 1; // 1-based position
                result.Add(serialized);
            }
        }
        return result;
    }

    private static SerializableLetter SerializeLetter(Letter letter)
    {
        if (letter == null) return null;

        return new SerializableLetter
        {
            Id = letter.Id,
            SenderName = letter.SenderName,
            RecipientName = letter.RecipientName,
            DeadlineInHours = letter.DeadlineInHours,
            Payment = letter.Payment,
            TokenType = letter.TokenType.ToString(),
            State = letter.State.ToString(),
            QueuePosition = letter.QueuePosition,
            Size = letter.Size.ToString(),
            IsFromPatron = letter.IsFromPatron,
            PhysicalProperties = letter.PhysicalProperties.ToString(),
            RequiredEquipment = letter.RequiredEquipment?.ToString(),
            SenderId = letter.SenderId,
            RecipientId = letter.RecipientId,
            DaysInQueue = letter.DaysInQueue,
            Description = letter.Description,
            IsGenerated = letter.IsGenerated,
            GenerationReason = letter.GenerationReason,
            UnlocksLetterIds = new List<string>(letter.UnlocksLetterIds),
            ParentLetterId = letter.ParentLetterId,
            IsChainLetter = letter.IsChainLetter,
            Message = letter.Message
        };
    }

    private static Dictionary<string, int> SerializeConnectionTokens(Dictionary<ConnectionType, int> tokens)
    {
        Dictionary<string, int> result = new Dictionary<string, int>();
        foreach (KeyValuePair<ConnectionType, int> kvp in tokens)
        {
            result[kvp.Key.ToString()] = kvp.Value;
        }
        return result;
    }

    private static Dictionary<string, Dictionary<string, int>> SerializeNPCTokens(Dictionary<string, Dictionary<ConnectionType, int>> npcTokens)
    {
        Dictionary<string, Dictionary<string, int>> result = new Dictionary<string, Dictionary<string, int>>();
        foreach (KeyValuePair<string, Dictionary<ConnectionType, int>> npc in npcTokens)
        {
            result[npc.Key] = SerializeConnectionTokens(npc.Value);
        }
        return result;
    }

    private static Dictionary<string, SerializableLetterHistory> SerializeLetterHistory(Dictionary<string, LetterHistory> history)
    {
        Dictionary<string, SerializableLetterHistory> result = new Dictionary<string, SerializableLetterHistory>();
        foreach (KeyValuePair<string, LetterHistory> kvp in history)
        {
            result[kvp.Key] = new SerializableLetterHistory
            {
                DeliveredCount = kvp.Value.DeliveredCount,
                SkippedCount = kvp.Value.SkippedCount,
                ExpiredCount = kvp.Value.ExpiredCount,
                LastInteraction = kvp.Value.LastInteraction
            };
        }
        return result;
    }

    private static SerializableStandingObligation SerializeStandingObligation(StandingObligation obligation)
    {
        return new SerializableStandingObligation
        {
            ID = obligation.ID,
            Name = obligation.Name,
            Description = obligation.Description,
            Source = obligation.Source,
            BenefitEffects = obligation.BenefitEffects.Select(e => e.ToString()).ToList(),
            ConstraintEffects = obligation.ConstraintEffects.Select(e => e.ToString()).ToList(),
            RelatedTokenType = obligation.RelatedTokenType?.ToString(),
            DateAccepted = obligation.DayAccepted,
            IsActive = obligation.IsActive,
            DaysSinceLastForcedLetter = obligation.DaysSinceLastForcedLetter
        };
    }

    // Helper methods for deserialization
    private static void DeserializeLetterQueue(List<SerializableLetter> letterQueue, Player player)
    {
        // Clear the letter queue
        for (int i = 0; i < player.LetterQueue.Length; i++)
        {
            player.LetterQueue[i] = null;
        }

        // Restore letters to their positions
        foreach (SerializableLetter letterData in letterQueue)
        {
            Letter letter = DeserializeLetter(letterData);
            if (letter != null && letterData.QueuePosition > 0 && letterData.QueuePosition <= player.LetterQueue.Length)
            {
                player.LetterQueue[letterData.QueuePosition - 1] = letter; // Convert to 0-based index
            }
        }
    }

    private static Letter DeserializeLetter(SerializableLetter data)
    {
        if (data == null) return null;

        Letter letter = new Letter
        {
            Id = data.Id,
            SenderName = data.SenderName,
            RecipientName = data.RecipientName,
            DeadlineInHours = data.DeadlineInHours,
            Payment = data.Payment,
            QueuePosition = data.QueuePosition,
            IsFromPatron = data.IsFromPatron,
            SenderId = data.SenderId,
            RecipientId = data.RecipientId,
            DaysInQueue = data.DaysInQueue,
            Description = data.Description,
            IsGenerated = data.IsGenerated,
            GenerationReason = data.GenerationReason,
            UnlocksLetterIds = new List<string>(data.UnlocksLetterIds ?? new List<string>()),
            ParentLetterId = data.ParentLetterId,
            IsChainLetter = data.IsChainLetter,
            Message = data.Message
        };

        // Parse enums
        if (EnumParser.TryParse<ConnectionType>(data.TokenType, out ConnectionType tokenType))
            letter.TokenType = tokenType;
        if (EnumParser.TryParse<LetterState>(data.State, out LetterState state))
            letter.State = state;
        if (EnumParser.TryParse<SizeCategory>(data.Size, out SizeCategory size))
            letter.Size = size;
        if (EnumParser.TryParse<LetterPhysicalProperties>(data.PhysicalProperties, out LetterPhysicalProperties physicalProps))
            letter.PhysicalProperties = physicalProps;
        if (!string.IsNullOrEmpty(data.RequiredEquipment) && EnumParser.TryParse<ItemCategory>(data.RequiredEquipment, out ItemCategory equipment))
            letter.RequiredEquipment = equipment;

        return letter;
    }

    private static void DeserializeConnectionTokens(Dictionary<string, int> tokens, Player player)
    {
        player.ConnectionTokens.Clear();
        foreach (KeyValuePair<string, int> kvp in tokens)
        {
            if (EnumParser.TryParse<ConnectionType>(kvp.Key, out ConnectionType tokenType))
            {
                player.ConnectionTokens[tokenType] = kvp.Value;
            }
        }
    }

    private static void DeserializeNPCTokens(Dictionary<string, Dictionary<string, int>> npcTokens, Player player)
    {
        player.NPCTokens.Clear();
        foreach (KeyValuePair<string, Dictionary<string, int>> npc in npcTokens)
        {
            Dictionary<ConnectionType, int> tokenDict = new Dictionary<ConnectionType, int>();
            foreach (KeyValuePair<string, int> token in npc.Value)
            {
                if (EnumParser.TryParse<ConnectionType>(token.Key, out ConnectionType tokenType))
                {
                    tokenDict[tokenType] = token.Value;
                }
            }
            player.NPCTokens[npc.Key] = tokenDict;
        }
    }

    private static void DeserializeLetterHistory(Dictionary<string, SerializableLetterHistory> history, Player player)
    {
        player.NPCLetterHistory.Clear();
        foreach (KeyValuePair<string, SerializableLetterHistory> kvp in history)
        {
            player.NPCLetterHistory[kvp.Key] = new LetterHistory
            {
                DeliveredCount = kvp.Value.DeliveredCount,
                SkippedCount = kvp.Value.SkippedCount,
                ExpiredCount = kvp.Value.ExpiredCount,
                LastInteraction = kvp.Value.LastInteraction
            };
        }
    }

    private static StandingObligation DeserializeStandingObligation(SerializableStandingObligation data)
    {
        StandingObligation obligation = new StandingObligation
        {
            ID = data.ID,
            Name = data.Name,
            Description = data.Description,
            Source = data.Source,
            DayAccepted = data.DateAccepted,
            IsActive = data.IsActive,
            DaysSinceLastForcedLetter = data.DaysSinceLastForcedLetter
        };

        // Parse benefit effects
        foreach (string effect in data.BenefitEffects ?? new List<string>())
        {
            if (EnumParser.TryParse<ObligationEffect>(effect, out ObligationEffect effectEnum))
            {
                obligation.BenefitEffects.Add(effectEnum);
            }
        }

        // Parse constraint effects
        foreach (string effect in data.ConstraintEffects ?? new List<string>())
        {
            if (EnumParser.TryParse<ObligationEffect>(effect, out ObligationEffect effectEnum))
            {
                obligation.ConstraintEffects.Add(effectEnum);
            }
        }

        // Parse related token type
        if (!string.IsNullOrEmpty(data.RelatedTokenType) && EnumParser.TryParse<ConnectionType>(data.RelatedTokenType, out ConnectionType tokenType))
        {
            obligation.RelatedTokenType = tokenType;
        }

        return obligation;
    }

    public static GameWorld DeserializeGameWorld(string json, List<Location> locations, List<LocationSpot> spots, FlagService flagService = null, ConversationRepository conversationRepository = null)
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
        // Time is restored through TimeManager
        // Calculate hours to advance from start of day to reach saved time
        int savedHours = serialized.CurrentTimeHours;
        int savedDay = serialized.CurrentDay;

        // Restore time state directly to GameWorld
        gameWorld.CurrentDay = savedDay;
        // Convert hours to time block (6=Dawn, 10=Morning, 14=Afternoon, 18=Evening, 22=Night)
        gameWorld.CurrentTimeBlock = savedHours switch
        {
            >= 6 and < 10 => TimeBlocks.Dawn,
            >= 10 and < 14 => TimeBlocks.Morning,
            >= 14 and < 18 => TimeBlocks.Afternoon,
            >= 18 and < 22 => TimeBlocks.Evening,
            _ => TimeBlocks.Night
        };

        // Apply player state if character exists
        if (!string.IsNullOrEmpty(serialized.Player.Name))
        {
            if (EnumParser.TryParse<Genders>(serialized.Player.Gender, out Genders gender) &&
                EnumParser.TryParse<Professions>(serialized.Player.Archetype, out Professions archetype))
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

            // Apply letter queue system
            DeserializeLetterQueue(serialized.Player.LetterQueue, gameWorld.GetPlayer());
            DeserializeConnectionTokens(serialized.Player.ConnectionTokens, gameWorld.GetPlayer());
            DeserializeNPCTokens(serialized.Player.NPCTokens, gameWorld.GetPlayer());

            // Apply physical letters
            gameWorld.GetPlayer().CarriedLetters.Clear();
            foreach (SerializableLetter letterData in serialized.Player.CarriedLetters)
            {
                Letter letter = DeserializeLetter(letterData);
                if (letter != null)
                    gameWorld.GetPlayer().CarriedLetters.Add(letter);
            }

            // Apply queue manipulation tracking
            gameWorld.GetPlayer().LastMorningSwapDay = serialized.Player.LastMorningSwapDay;
            gameWorld.GetPlayer().LastLetterBoardDay = serialized.Player.LastLetterBoardDay;
            gameWorld.GetPlayer().DailyBoardLetters.Clear();
            foreach (SerializableLetter letterData in serialized.Player.DailyBoardLetters)
            {
                Letter letter = DeserializeLetter(letterData);
                if (letter != null)
                    gameWorld.GetPlayer().DailyBoardLetters.Add(letter);
            }

            // Apply letter history
            DeserializeLetterHistory(serialized.Player.NPCLetterHistory, gameWorld.GetPlayer());

            // Apply standing obligations
            gameWorld.GetPlayer().StandingObligations.Clear();
            foreach (SerializableStandingObligation obligationData in serialized.Player.StandingObligations)
            {
                StandingObligation obligation = DeserializeStandingObligation(obligationData);
                if (obligation != null)
                    gameWorld.GetPlayer().StandingObligations.Add(obligation);
            }

            // Apply token favor system
            gameWorld.GetPlayer().PurchasedFavors = new List<string>(serialized.Player.PurchasedFavors);
            gameWorld.GetPlayer().UnlockedLocationIds = new List<string>(serialized.Player.UnlockedLocationIds);
            gameWorld.GetPlayer().UnlockedServices = new List<string>(serialized.Player.UnlockedServices);

            // Apply scenario tracking
            gameWorld.GetPlayer().DeliveredLetters.Clear();
            foreach (SerializableLetter letterData in serialized.Player.DeliveredLetters)
            {
                Letter letter = DeserializeLetter(letterData);
                if (letter != null)
                    gameWorld.GetPlayer().DeliveredLetters.Add(letter);
            }
            gameWorld.GetPlayer().TotalLettersDelivered = serialized.Player.TotalLettersDelivered;
            gameWorld.GetPlayer().TotalLettersExpired = serialized.Player.TotalLettersExpired;
            gameWorld.GetPlayer().TotalTokensSpent = serialized.Player.TotalTokensSpent;

            // Apply patron tracking
            gameWorld.GetPlayer().PatronLeverage = serialized.Player.PatronLeverage;
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
                    gameWorld.GetPlayer().CurrentLocationSpot = currentSpot;
                }
            }
        }

        // Restore narrative state
        if (flagService != null && serialized.FlagServiceState != null)
        {
            flagService.RestoreState(serialized.FlagServiceState);
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
            travelTimeHours = route.TravelTimeMinutes,
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
