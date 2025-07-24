public class RestManager
{
    private GameWorld gameWorld;
    private ITimeManager timeManager;
    private LocationRepository locationRepository;
    private MessageSystem messageSystem;

    public RestManager(
        GameWorld gameWorld,
        ITimeManager timeManager,
        LocationRepository locationRepository,
        MessageSystem messageSystem)
    {
        this.gameWorld = gameWorld;
        this.timeManager = timeManager;
        this.locationRepository = locationRepository;
        this.messageSystem = messageSystem;
    }

    public List<RestOption> GetAvailableRestOptions()
    {
        Location currentLocation = gameWorld.GetPlayer().CurrentLocation;
        if (currentLocation == null) return new List<RestOption>();

        List<RestOption> restOptions = new List<RestOption>();

        TimeBlocks currentTime = timeManager.GetCurrentTimeBlock();

        // During day/evening - only short rest/wait options
        if (currentTime != TimeBlocks.Night)
        {
            restOptions.Add(new RestOption
            {
                Id = "wait",
                Name = "Wait an Hour",
                CoinCost = 0,
                StaminaRecovery = 0,
                RestTimeHours = 1,
                EnablesDawnDeparture = false
            });

            restOptions.Add(new RestOption
            {
                Id = "short_rest",
                Name = "Short Rest",
                CoinCost = 0,
                StaminaRecovery = 1,
                RestTimeHours = 1,
                EnablesDawnDeparture = false
            });
        }
        // At night - sleep options that trigger morning activities
        else
        {
            restOptions.Add(new RestOption
            {
                Id = "camp",
                Name = "Sleep Outside",
                CoinCost = 0,
                StaminaRecovery = 2,
                RestTimeHours = 1, // Consumes remaining night
                EnablesDawnDeparture = true
            });
        }

        // Location-specific rest options (only available evening/night)
        if (currentTime == TimeBlocks.Evening || currentTime == TimeBlocks.Night)
        {
            bool isNight = currentTime == TimeBlocks.Night;

            if (currentLocation.Id == "Millbrook")
            {
                restOptions.Add(new RestOption
                {
                    Id = "inn",
                    Name = isNight ? "Inn Room (Full Night)" : "Inn Room (Evening)",
                    CoinCost = 2,
                    StaminaRecovery = isNight ? 4 : 2,
                    ProvidesMarketRumors = true,
                    EnablesDawnDeparture = isNight
                });
            }
            else if (currentLocation.Id == "Crossbridge")
            {
                restOptions.Add(new RestOption
                {
                    Id = "inn",
                    Name = isNight ? "Inn Room (Full Night)" : "Inn Room (Evening)",
                    CoinCost = 3,
                    StaminaRecovery = isNight ? 4 : 2,
                    ProvidesMarketRumors = true,
                    EnablesDawnDeparture = isNight
                });

                restOptions.Add(new RestOption
                {
                    Id = "church",
                    Name = isNight ? "Church Lodging (Full Night)" : "Church Lodging (Evening)",
                    CoinCost = 0,
                    StaminaRecovery = isNight ? 3 : 1,
                    CleansesContraband = true,
                    RequiredItem = "Pilgrim Token",
                    EnablesDawnDeparture = isNight
                });
            }
            else if (currentLocation.Id == "Thornwood")
            {
                restOptions.Add(new RestOption
                {
                    Id = "hunter_cabin",
                    Name = isNight ? "Hunter's Cabin (Full Night)" : "Hunter's Cabin (Evening)",
                    CoinCost = 1,
                    StaminaRecovery = isNight ? 3 : 1,
                    EnablesDawnDeparture = isNight
                });
            }
        }

        // Filter by player requirements
        Player player = gameWorld.GetPlayer();
        return restOptions.Where(r =>
            r.RequiredItem == null ||
            Array.IndexOf(player.Inventory.ItemSlots, r.RequiredItem) != -1).ToList();
    }

    public void Rest(RestOption option)
    {
        Player player = gameWorld.GetPlayer();
        Location currentLocation = player.CurrentLocation;

        // Validate time block availability
        // Check if we have enough hours left in the day
        if (timeManager.GetCurrentTimeHours() + option.RestTimeHours > 24)
        {
            throw new InvalidOperationException($"Cannot rest: Not enough time remaining in the day. Rest requires {option.RestTimeHours} hours.");
        }

        // Show the rest beginning
        messageSystem.AddSystemMessage(
            $"🛌 {option.Name} at {currentLocation.Name}...",
            SystemMessageTypes.Info
        );

        // Deduct cost with narrative
        if (option.CoinCost > 0)
        {
            player.ModifyCoins(-option.CoinCost);
            messageSystem.AddSystemMessage(
                $"  • Paid {option.CoinCost} coins for accommodations",
                SystemMessageTypes.Info
            );
        }

        // Resting takes time
        timeManager.AdvanceTime(option.RestTimeHours);
        messageSystem.AddSystemMessage(
            $"  • {option.RestTimeHours} hours pass...",
            SystemMessageTypes.Info
        );

        // Recover stamina with narrative
        int oldStamina = player.Stamina;
        player.ModifyStamina(option.StaminaRecovery);

        int staminaGained = player.Stamina - oldStamina;
        messageSystem.AddSystemMessage(
            $"💪 Recovered {staminaGained} stamina (now {player.Stamina}/{player.MaxStamina})",
            SystemMessageTypes.Success
        );

        // Describe the rest quality
        if (option.StaminaRecovery >= 8)
        {
            messageSystem.AddSystemMessage(
                $"  • You feel completely refreshed after a good night's sleep.",
                SystemMessageTypes.Success
            );
        }
        else if (option.StaminaRecovery >= 4)
        {
            messageSystem.AddSystemMessage(
                $"  • The rest helps, though you could use more sleep.",
                SystemMessageTypes.Info
            );
        }
        else
        {
            messageSystem.AddSystemMessage(
                $"  • A brief rest, but exhaustion still lingers.",
                SystemMessageTypes.Warning
            );
        }

        // Cleanse contraband if applicable
        if (option.CleansesContraband)
        {
            for (int i = 0; i < player.Inventory.ItemSlots.Length; i++)
            {
                string itemName = player.Inventory.ItemSlots[i];
                if (itemName != null)
                {
                    // This would require tracking contraband status in the inventory
                    // For simplicity in the POC, we'll assume this is handled elsewhere
                }
            }
        }

        // Generate market rumors if applicable
        if (option.ProvidesMarketRumors)
        {
            GenerateMarketRumors();
        }

        // Advance time based on rest option using TimeManager
        if (option.EnablesDawnDeparture)
        {
            // Calculate hours until dawn (6 AM) of next day
            int currentHour = timeManager.GetCurrentTimeHours();
            int hoursUntilDawn = (24 - currentHour) + 6; // Hours to midnight + 6 hours to dawn
            timeManager.AdvanceTime(hoursUntilDawn);
        }
        else
        {
            // Advance to next day at 6 AM
            int currentHour = timeManager.GetCurrentTimeHours();
            int hoursUntilDawn = (24 - currentHour) + 6;
            timeManager.AdvanceTime(hoursUntilDawn);
            // Time advances to next day at dawn (6 AM)
            // If we need to start at a different time, we can advance additional hours
            timeManager.AdvanceTime(3); // Advance to 9 AM (Morning)
        }

        // Skill cards removed - using letter queue system

        // Generate exclusive contract if applicable
        if (option.OffersExclusiveContract)
        {
            GenerateExclusiveContract();
        }
    }

    private void GenerateMarketRumors()
    {
        // For POC, simply adjust prices at a random location
        Random random = new Random();
        List<Location> locations = locationRepository.GetAllLocations();
        Location targetLocation = locations[random.Next(locations.Count)];

        // Find a random item to create a price fluctuation for
        if (targetLocation.MarketItems != null && targetLocation.MarketItems.Any())
        {
            Item item = targetLocation.MarketItems[random.Next(targetLocation.MarketItems.Count)];

            // Create a temporary price change (lasts 1 day)
            bool isPriceIncrease = random.Next(2) == 0;
            int percentChange = random.Next(20, 101); // 20-100% change

            if (isPriceIncrease)
            {
                item.SellPrice = (int)(item.SellPrice * (1 + percentChange / 100.0));
                messageSystem.AddSystemMessage($"Rumor: {item.Name} prices have increased in {targetLocation.Name}!");
            }
            else
            {
                item.BuyPrice = (int)(item.BuyPrice * (1 - percentChange / 100.0));
                messageSystem.AddSystemMessage($"Rumor: {item.Name} prices have dropped in {targetLocation.Name}!");
            }
        }
    }

    private void GenerateExclusiveContract()
    {
        // Contract functionality has been removed - using letter system instead
        messageSystem.AddSystemMessage("Your connections have provided you with valuable information!");
    }

    public void Wait(int hours)
    {
        if (hours <= 0) return;

        // Validate we don't go past midnight
        if (timeManager.GetCurrentTimeHours() + hours > 24)
        {
            messageSystem.AddSystemMessage($"Cannot wait {hours} hours: would go past midnight", SystemMessageTypes.Danger);
            return;
        }

        // Advance time
        timeManager.AdvanceTime(hours);

        // Show feedback
        string timeDescription = hours == 1 ? "1 hour" : $"{hours} hours";
        messageSystem.AddSystemMessage($"Waited {timeDescription}. Time advanced to {timeManager.GetCurrentTimeHours()}:00", SystemMessageTypes.Info);
    }
}