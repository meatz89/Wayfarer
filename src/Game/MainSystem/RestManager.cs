public class RestManager
{
    private GameWorld gameWorld;
    private TimeManager timeManager;
    private LocationRepository locationRepository;
    private MessageSystem messageSystem;

    public RestManager(
        GameWorld gameWorld,
        LocationRepository locationRepository,
        MessageSystem messageSystem)
    {
        this.gameWorld = gameWorld;
        this.timeManager = gameWorld.TimeManager;
        this.locationRepository = locationRepository;
        this.messageSystem = messageSystem;
    }

    public List<RestOption> GetAvailableRestOptions()
    {
        Location currentLocation = gameWorld.WorldState.CurrentLocation;
        if (currentLocation == null) return new List<RestOption>();

        List<RestOption> restOptions = new List<RestOption>
        {
            // Standard rest options available everywhere
            new RestOption
            {
                Id = "camp",
                Name = "Camp Outside",
                CoinCost = 0,
                StaminaRecovery = 2,
                EnablesDawnDeparture = true
            }
        };

        // Location-specific rest options
        if (currentLocation.Id == "Millbrook")
        {
            restOptions.Add(new RestOption
            {
                Id = "inn",
                Name = "Inn Room",
                CoinCost = 2,
                StaminaRecovery = 4,
                ProvidesMarketRumors = true
            });
        }
        else if (currentLocation.Id == "Crossbridge")
        {
            restOptions.Add(new RestOption
            {
                Id = "inn",
                Name = "Inn Room",
                CoinCost = 3,
                StaminaRecovery = 4,
                ProvidesMarketRumors = true
            });

            restOptions.Add(new RestOption
            {
                Id = "church",
                Name = "Church Lodging",
                CoinCost = 0,
                StaminaRecovery = 3,
                CleansesContraband = true,
                RequiredItem = "Pilgrim Token"
            });
        }
        else if (currentLocation.Id == "Thornwood")
        {
            restOptions.Add(new RestOption
            {
                Id = "hunter_cabin",
                Name = "Hunter's Cabin",
                CoinCost = 1,
                StaminaRecovery = 3
            });
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

        // Validate time block availability
        if (!timeManager.ValidateTimeBlockAction(option.TimeBlockCost))
        {
            throw new InvalidOperationException($"Cannot rest: Not enough time blocks remaining. Rest requires {option.TimeBlockCost} blocks, but only {timeManager.RemainingTimeBlocks} available.");
        }

        // Deduct cost
        player.ModifyCoins(-option.CoinCost);

        // Consume time blocks
        timeManager.ConsumeTimeBlock(option.TimeBlockCost);

        // Recover stamina
        player.Stamina += option.StaminaRecovery;
        if (player.Stamina > player.MaxStamina)
        {
            player.Stamina = player.MaxStamina;
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
            gameWorld.TimeManager.StartNewDay();
            // StartNewDay() sets time to TimeDayStart (6 AM) which is Dawn
            gameWorld.WorldState.CurrentTimeBlock = TimeBlocks.Dawn;
        }
        else
        {
            gameWorld.TimeManager.StartNewDay();
            // Override to Morning for regular departure
            gameWorld.WorldState.CurrentTimeBlock = TimeBlocks.Morning;
        }

        // Refresh player cards
        foreach (SkillCard card in player.GetAllAvailableCards())
        {
            player.RefreshCard(card);
        }

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
        
        // Convert hours to time blocks (roughly 3.6 hours per block)
        int timeBlocks = (int)Math.Ceiling(hours / 3.6);
        
        // Validate time block availability
        if (!timeManager.ValidateTimeBlockAction(timeBlocks))
        {
            messageSystem.AddSystemMessage($"Cannot wait {hours} hours: insufficient time blocks remaining", SystemMessageTypes.Danger);
            return;
        }
        
        // Consume time blocks
        timeManager.ConsumeTimeBlock(timeBlocks);
        
        // Show feedback
        string timeDescription = hours == 1 ? "1 hour" : $"{hours} hours";
        messageSystem.AddSystemMessage($"Waited {timeDescription}. Time advanced to {timeManager.CurrentTimeHours}:00", SystemMessageTypes.Info);
    }
}