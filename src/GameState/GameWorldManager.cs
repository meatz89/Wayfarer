using System.Threading.Tasks;

public class GameWorldManager
{
    private bool _useMemory;
    private bool _processStateChanges;
    private readonly GameWorld _gameWorld;

    public GameWorld GameWorld
    {
        get
        {
            return _gameWorld;
        }
    }

    private ItemRepository itemRepository;
    private LocationSystem locationSystem;
    private LocationRepository locationRepository;
    private TravelManager travelManager;
    private MarketManager marketManager;
    private TradeManager tradeManager;
    private RestManager restManager;
    private NPCRepository npcRepository;
    private LetterQueueManager letterQueueManager;
    private StandingObligationManager standingObligationManager;
    private MorningActivitiesManager morningActivitiesManager;
    private NPCLetterOfferService npcLetterOfferService;
    private ScenarioManager scenarioManager;
    private ConversationFactory conversationFactory;
    private LocationActionManager locationActionManager;

    private bool isAiAvailable = true;

    private readonly ILogger<GameWorldManager> logger;

    public GameWorldManager(GameWorld gameWorld,
                       ItemRepository itemRepository,
                       LocationSystem locationSystem,
                       LocationRepository locationRepository, TravelManager travelManager,
                       MarketManager marketManager, TradeManager tradeManager,
                       RestManager restManager,
                       NPCRepository npcRepository,
                       LetterQueueManager letterQueueManager, StandingObligationManager standingObligationManager,
                       MorningActivitiesManager morningActivitiesManager, NPCLetterOfferService npcLetterOfferService,
                       ScenarioManager scenarioManager, ConversationFactory conversationFactory,
                       LocationActionManager locationActionManager,
                       IConfiguration configuration, ILogger<GameWorldManager> logger)
    {
        _gameWorld = gameWorld;
        this.itemRepository = itemRepository;
        this.locationSystem = locationSystem;
        this.locationRepository = locationRepository;
        this.travelManager = travelManager;
        this.marketManager = marketManager;
        this.tradeManager = tradeManager;
        this.restManager = restManager;
        this.npcRepository = npcRepository;
        this.letterQueueManager = letterQueueManager;
        this.standingObligationManager = standingObligationManager;
        this.morningActivitiesManager = morningActivitiesManager;
        this.npcLetterOfferService = npcLetterOfferService;
        this.scenarioManager = scenarioManager;
        this.conversationFactory = conversationFactory;
        this.locationActionManager = locationActionManager;
        this.logger = logger;
        _useMemory = configuration.GetValue<bool>("useMemory");
        _processStateChanges = configuration.GetValue<bool>("processStateChanges");
    }

    public async Task StartGame()
    {
        _gameWorld.GetPlayer().HealFully();

        // Initialize time management - set to start of first day
        _gameWorld.TimeManager.SetNewTime(TimeManager.TimeDayStart);  // 6:00 AM
        _gameWorld.CurrentDay = 1; // Start on day 1

        Location startingLocation = await locationSystem.Initialize();
        locationRepository.RecordLocationVisit(startingLocation.Id);
        travelManager.StartLocationTravel(startingLocation.Id);

        // Preserve the location spot that was set by LocationSystem.Initialize()
        LocationSpot currentSpot = _gameWorld.GetPlayer().CurrentLocationSpot;
        locationRepository.SetCurrentLocation(startingLocation, currentSpot);

        Location currentLocation = locationRepository.GetCurrentLocation();
        if (locationRepository.GetCurrentLocationSpot() == null && locationSystem.GetLocationSpots(currentLocation.Id).Any() == true)
        {
            Console.WriteLine("Current location spot is null despite spots existing - manually setting");
            locationRepository.SetCurrentLocation(startingLocation, locationSystem.GetLocationSpots(currentLocation.Id).First());
        }

        Location? currentLoc = currentLocation;
        Console.WriteLine($"Game started at: {currentLoc?.Id}, Current spot: {locationRepository.GetCurrentLocationSpot()?.SpotID}");

        // Location actions are now handled by LocationActionManager
    }


    // Encounter system adapted for conversations - use ConversationSystem for NPC interactions

    // Action system removed - LocationActionManager handles location actions
    // ConversationSystem handles NPC interactions
    
    public async Task<ConversationManager> StartConversation(string npcId)
    {
        var npc = npcRepository.GetNPCById(npcId);
        if (npc == null) return null;
        
        var location = locationRepository.GetCurrentLocation();
        var locationSpot = locationRepository.GetCurrentLocationSpot();
        
        // Create conversation context
        var context = new ConversationContext
        {
            GameWorld = _gameWorld,
            Player = _gameWorld.GetPlayer(),
            LocationName = location?.Name ?? "Unknown",
            LocationSpotName = locationSpot?.Name ?? "Unknown",
            LocationProperties = locationSpot?.GetCurrentProperties() ?? new List<string>(),
            TargetNPC = npc,
            ConversationTopic = "General conversation",
            StartingFocusPoints = 10
        };
        
        // Create conversation using factory
        var conversationManager = await conversationFactory.CreateConversation(context, _gameWorld.GetPlayer());
        await conversationManager.InitializeConversation();
        
        return conversationManager;
    }


    public async Task Travel(RouteOption route)
    {
        // Check if player can travel this route
        Player player = _gameWorld.GetPlayer();
        int totalWeight = CalculateTotalWeight();
        if (!route.CanTravel(itemRepository, player, totalWeight))
        {
            return;
        }

        // Check if we have enough time in the day
        if (GameWorld.TimeManager.CurrentTimeHours + route.TravelTimeHours > 24)
        {
            throw new InvalidOperationException($"Cannot travel: Not enough time remaining in the day. Route requires {route.TravelTimeHours} hours.");
        }

        // Apply costs
        int timeCost = route.GetActualTimeCost();
        int staminaCost = travelManager.CalculateStaminaCost(route);

        player.SpendStamina(staminaCost);
        player.ModifyCoins(route.BaseCoinCost);

        // Travel takes time
        GameWorld.TimeManager.AdvanceTime(route.TravelTimeHours);
        
        // Check for periodic letter offers after time change
        CheckForPeriodicLetterOffers();

        // Encounter system removed - travel encounters will use conversation system
        // int seed = _gameWorld.TimeManager.GetCurrentDay() + player.GetHashCode();
        // EncounterContext encounterContext = route.GetEncounter(seed);

        // if (encounterContext != null)
        // {
        //     // Start a travel encounter
        //     // This would use the encounter system
        // }
        // else
        // {
        //     // Arrived safely
        Location destination = locationRepository.GetLocation(route.Destination);

        // Find the first available location spot for the destination
        LocationSpot? destinationSpot = locationSystem.GetLocationSpots(destination.Id).FirstOrDefault();

        locationRepository.SetCurrentLocation(destination, destinationSpot);

        // Also update the Player's location to keep them in sync
        player.CurrentLocation = destination;
        player.CurrentLocationSpot = destinationSpot;


        // Update handled automatically
        // }
    }

    public async Task TravelWithTransport(RouteOption route, TravelMethods transport)
    {
        // ARCHITECTURAL FIX: Routes define their transport method, no separate transport selection needed
        // Verify that the requested transport matches the route's defined transport method
        if (route.Method != transport)
        {
            throw new InvalidOperationException($"Route '{route.Name}' uses {route.Method} transport, cannot use {transport}");
        }

        // Use the standard Travel method - route contains all transport information
        await Travel(route);
    }


    // Encounter choice preview removed - use ConversationSystem for NPC interactions

    public async Task MoveToLocationSpot(string locationSpotName)
    {
        Location location = locationRepository.GetCurrentLocation();

        LocationSpot locationSpot = locationSystem.GetLocationSpot(location.Id, locationSpotName);
        locationRepository.SetCurrentLocation(location, locationSpot);

    }

    public List<Location> GetPlayerKnownLocations()
    {
        return locationSystem.GetAllLocations();
    }

    public bool CanMoveToSpot(string locationSpotId)
    {
        Location location = locationRepository.GetCurrentLocation();
        LocationSpot locationSpot = locationRepository.GetSpot(location.Id, locationSpotId);
        if (locationSpot == null)
        {
            return false;
        }
        bool canMove = !locationSpot.IsClosed;

        return canMove;
    }

    public string GetTimeBlocks(int totalHours)
    {
        if (totalHours >= 5 && totalHours < 12) return "Morning";
        if (totalHours >= 12 && totalHours < 17) return "Afternoon";
        if (totalHours >= 17 && totalHours < 21) return "Evening";
        return "Night";
    }


    // Encounter and action methods removed - use LocationActionManager and ConversationSystem

    public async Task StartNewDay()
    {
        //SaveGame();

        // Process daily letter queue activities
        ProcessDailyLetterQueue();
        
        // Mark that morning activities should be shown
        if (morningActivitiesManager != null)
        {
            // Process and store the morning summary
            _lastMorningResult = morningActivitiesManager.ProcessMorningActivities();
        }
        
        // Check scenario progress if one is active
        if (scenarioManager != null)
        {
            scenarioManager.CheckScenarioProgress();
        }
        
    }
    
    private MorningActivityResult _lastMorningResult;

    public void ProcessDailyLetterQueue()
    {
        // Process letter deadline countdown daily
        letterQueueManager.ProcessDailyDeadlines();
        
        // Process standing obligations and generate forced letters
        var forcedLetters = standingObligationManager.ProcessDailyObligations(_gameWorld.CurrentDay);
        
        // Add forced letters to the queue with obligation effects
        foreach (var letter in forcedLetters)
        {
            letterQueueManager.AddLetterWithObligationEffects(letter);
        }
        
        // Advance obligation time tracking
        standingObligationManager.AdvanceDailyTime();
        
        // Generate new letters for the day
        letterQueueManager.GenerateDailyLetters();
    }
    
    public MorningActivityResult GetMorningActivitySummary()
    {
        // Return the last morning result if available
        var result = _lastMorningResult;
        _lastMorningResult = null; // Clear after reading
        return result;
    }
    
    public bool HasPendingMorningActivities()
    {
        return _lastMorningResult != null && _lastMorningResult.HasEvents;
    }

    public bool CanTravelTo(string locationId)
    {
        return travelManager.CanTravelTo(locationId);
    }


    public GameWorldSnapshot GetGameSnapshot()
    {
        return new GameWorldSnapshot(_gameWorld);
    }

    private void GameOver()
    {
        // Game over logic to be implemented
    }

    private bool IsGameOver(Player player)
    {
        return false;
    }

    // ACTION GATEWAY METHODS

    /// <summary>
    /// Execute trading actions (buy/sell items at markets)
    /// </summary>
    public void ExecuteTradeAction(string itemId, string action, string locationId)
    {
        if (action == "buy")
        {
            tradeManager.BuyItem(itemId, locationId);
        }
        else if (action == "sell")
        {
            tradeManager.SellItem(itemId, locationId);
        }

    }

    /// <summary>
    /// Execute travel actions between locations
    /// </summary>
    public void ExecuteTravelAction(string destinationId, string routeId = null)
    {
        travelManager.StartLocationTravel(destinationId);
    }


    /// <summary>
    /// Execute rest actions for stamina recovery
    /// </summary>
    public void ExecuteRestAction(string restOptionId)
    {
        List<RestOption> restOptions = restManager.GetAvailableRestOptions();
        RestOption? option = restOptions.FirstOrDefault(r => r.Id == restOptionId);

        if (option != null)
        {
            // Validate time availability before attempting rest
            if (GameWorld.TimeManager.CurrentTimeHours + option.RestTimeHours > 24)
            {
                throw new InvalidOperationException($"Cannot rest: Not enough time remaining in the day. Rest requires {option.RestTimeHours} hours.");
            }

            restManager.Rest(option);
            
            // Check for periodic letter offers after time change
            CheckForPeriodicLetterOffers();
        }
    }

    /// <summary>
    /// Get market pricing information for UI display
    /// </summary>
    public List<Item> GetMarketData(string locationId)
    {
        return marketManager.GetAvailableItems(locationId);
    }

    /// <summary>
    /// Get available travel routes between locations (UI read-only access)
    /// </summary>
    public List<RouteOption> GetAvailableRoutes(string fromLocationId, string toLocationId)
    {
        return travelManager.GetAvailableRoutes(fromLocationId, toLocationId);
    }

    /// <summary>
    /// Check if player can travel on a specific route (UI validation)
    /// </summary>
    public bool CanTravelRoute(RouteOption route)
    {
        return travelManager.CanTravel(route);
    }

    /// <summary>
    /// Get available market items for UI display
    /// </summary>
    public List<Item> GetAvailableMarketItems(string locationId)
    {
        return marketManager.GetAvailableItems(locationId);
    }

    /// <summary>
    /// Check if player can buy item (UI validation)
    /// </summary>
    public bool CanBuyMarketItem(string itemId, string locationId)
    {
        return marketManager.CanBuyItem(itemId, locationId);
    }

    /// <summary>
    /// Get inventory status for market display
    /// </summary>
    public string GetInventoryStatusForMarket()
    {
        return marketManager.GetInventoryStatusForMarket();
    }

    /// <summary>
    /// Get inventory constraint message for an item
    /// </summary>
    public string GetInventoryConstraintMessage(string itemId)
    {
        return marketManager.GetInventoryConstraintMessage(itemId);
    }

    /// <summary>
    /// Get market availability status for display
    /// </summary>
    public string GetMarketAvailabilityStatus(string locationId)
    {
        return marketManager.GetMarketAvailabilityStatus(locationId);
    }

    /// <summary>
    /// Get detailed market status with trader information
    /// </summary>
    public string GetDetailedMarketStatus(string locationId)
    {
        return marketManager.GetDetailedMarketStatus(locationId);
    }

    /// <summary>
    /// Get all NPCs who provide trading services at a location
    /// </summary>
    public List<NPC> GetTradingNPCs(string locationId)
    {
        return npcRepository.GetNPCsForLocation(locationId)
            .Where(npc => npc.ProvidedServices.Contains(ServiceTypes.Trade))
            .ToList();
    }

    /// <summary>
    /// Get currently available NPCs at a location
    /// </summary>
    public List<NPC> GetCurrentlyAvailableNPCs(string locationId)
    {
        TimeBlocks currentTime = _gameWorld.TimeManager.GetCurrentTimeBlock();
        return npcRepository.GetNPCsForLocationAndTime(locationId, currentTime);
    }

    /// <summary>
    /// Get NPCs providing a specific service
    /// </summary>
    public List<NPC> GetNPCsProvidingService(ServiceTypes service)
    {
        return npcRepository.GetNPCsProvidingService(service);
    }

    /// <summary>
    /// Get human-readable schedule description for an NPC
    /// </summary>
    public string GetNPCScheduleDescription(Schedule schedule)
    {
        return schedule switch
        {
            Schedule.Always => "Always available",
            Schedule.Market_Hours => "Morning, Afternoon",
            Schedule.Workshop_Hours => "Dawn, Morning, Afternoon",
            Schedule.Library_Hours => "Morning, Afternoon",
            Schedule.Business_Hours => "Morning, Afternoon",
            Schedule.Morning_Evening => "Morning, Evening",
            Schedule.Morning_Afternoon => "Morning, Afternoon",
            Schedule.Afternoon_Evening => "Afternoon, Evening",
            Schedule.Evening_Only => "Evening only",
            Schedule.Morning_Only => "Morning only",
            Schedule.Afternoon_Only => "Afternoon only",
            Schedule.Evening_Night => "Evening, Night",
            Schedule.Dawn_Only => "Dawn only",
            Schedule.Night_Only => "Night only",
            _ => "Unknown schedule"
        };
    }

    /// <summary>
    /// Get next available time for an NPC
    /// </summary>
    public string GetNextAvailableTime(NPC npc)
    {
        TimeBlocks currentTime = _gameWorld.TimeManager.GetCurrentTimeBlock();
        
        if (npc.IsAvailable(currentTime))
        {
            return "Available now";
        }

        // Check upcoming time blocks in order
        List<TimeBlocks> timeBlocks = new List<TimeBlocks> 
        { 
            TimeBlocks.Dawn, TimeBlocks.Morning, TimeBlocks.Afternoon, 
            TimeBlocks.Evening, TimeBlocks.Night 
        };

        // Start checking from the next time block
        int currentIndex = timeBlocks.IndexOf(currentTime);
        for (int i = 1; i <= timeBlocks.Count; i++)
        {
            int nextIndex = (currentIndex + i) % timeBlocks.Count;
            TimeBlocks nextTime = timeBlocks[nextIndex];
            
            if (npc.IsAvailable(nextTime))
            {
                return $"Next available: {nextTime.ToString().Replace('_', ' ')}";
            }
        }

        return "Never available";
    }



    /// <summary>
    /// Get item price at a specific location
    /// </summary>
    public int GetItemPrice(string locationId, string itemId, bool isBuyPrice)
    {
        return marketManager.GetItemPrice(locationId, itemId, isBuyPrice);
    }



    /// <summary>
    /// Calculate total inventory weight for UI display
    /// </summary>
    public int CalculateTotalWeight()
    {
        Player player = _gameWorld.GetPlayer();
        int totalWeight = 0;

        // Add item weights
        foreach (string itemName in player.Inventory.ItemSlots)
        {
            if (!string.IsNullOrEmpty(itemName))
            {
                Item item = itemRepository.GetItemById(itemName);
                if (item != null)
                {
                    totalWeight += item.Weight;
                }
            }
        }

        // Coins no longer have weight

        return totalWeight;
    }
    
    /// <summary>
    /// Check for periodic letter offers after time changes.
    /// NPCs with strong relationships may spontaneously offer letters.
    /// </summary>
    private void CheckForPeriodicLetterOffers()
    {
        // Generate periodic offers based on time changes
        npcLetterOfferService.GeneratePeriodicOffers();
    }
    
    /// <summary>
    /// Execute an action - UI must use this, not LocationActionManager directly
    /// </summary>
    public async Task<bool> ExecuteAction(ActionOption action)
    {
        return await locationActionManager.ExecuteAction(action);
    }
    
    /// <summary>
    /// Complete action after conversation - called by MainGameplayView
    /// </summary>
    public bool CompleteActionAfterConversation(ActionOption action)
    {
        return locationActionManager.CompleteActionAfterConversation(action);
    }
}
