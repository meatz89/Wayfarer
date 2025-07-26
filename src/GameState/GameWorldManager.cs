using System.Threading.Tasks;

public class GameWorldManager
{
    private bool _useMemory;
    private bool _processStateChanges;
    private readonly GameWorld _gameWorld;

    public GameWorld GameWorld => _gameWorld;

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
    private ConversationFactory conversationFactory;
    private CommandExecutor commandExecutor;
    private CommandDiscoveryService commandDiscovery;
    private MessageSystem _messageSystem;
    private readonly ITimeManager _timeManager;
    private readonly ConversationStateManager _conversationStateManager;
    private readonly NarrativeManager _narrativeManager;
    private readonly FlagService _flagService;
    private readonly CollapseManager _collapseManager;

    private bool isAiAvailable = true;

    private readonly ILogger<GameWorldManager> logger;
    private readonly DebugLogger _debugLogger;

    public GameWorldManager(GameWorld gameWorld,
                       ItemRepository itemRepository,
                       LocationSystem locationSystem,
                       LocationRepository locationRepository, TravelManager travelManager,
                       MarketManager marketManager, TradeManager tradeManager,
                       RestManager restManager,
                       NPCRepository npcRepository,
                       LetterQueueManager letterQueueManager, StandingObligationManager standingObligationManager,
                       MorningActivitiesManager morningActivitiesManager, NPCLetterOfferService npcLetterOfferService,
                       ConversationFactory conversationFactory,
                       CommandExecutor commandExecutor,
                       CommandDiscoveryService commandDiscovery,
                       MessageSystem messageSystem,
                       DebugLogger debugLogger,
                       ITimeManager timeManager,
                       ConversationStateManager conversationStateManager,
                       NarrativeManager narrativeManager,
                       FlagService flagService,
                       CollapseManager collapseManager,
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
        this.conversationFactory = conversationFactory;
        this.commandExecutor = commandExecutor;
        this.commandDiscovery = commandDiscovery;
        _messageSystem = messageSystem;
        this.logger = logger;
        _debugLogger = debugLogger;
        _timeManager = timeManager;
        _conversationStateManager = conversationStateManager;
        _narrativeManager = narrativeManager;
        _flagService = flagService;
        _collapseManager = collapseManager;
        _useMemory = configuration.GetValue<bool>("useMemory");
        _processStateChanges = configuration.GetValue<bool>("processStateChanges");
    }

    public async Task StartGame()
    {
        _gameWorld.GetPlayer().HealFully();
        
        // Register collapse callback
        _gameWorld.GetPlayer().OnStaminaExhausted = () => CheckForCollapse();

        // Initialize time management - set to start of first day
        bool spendHours = _timeManager.SpendHours(6);  // 6:00 AM
        // CurrentDay is readonly - managed by TimeManager

        Location startingLocation = await locationSystem.Initialize();
        locationRepository.RecordLocationVisit(startingLocation.Id);
        RouteOption selectedRoute = travelManager.StartLocationTravel(startingLocation.Id);

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

        // Initialize tutorial if this is a new game
        InitializeTutorialIfNeeded();

        // Location actions are now handled by CommandDiscoveryService

        // Don't apply patron obligation at start - it's added during tutorial Day 10
        // when player accepts patronage
    }


    // Encounter system adapted for conversations - use ConversationSystem for NPC interactions

    // Action system removed - CommandDiscoveryService discovers commands
    // ConversationFactory handles NPC interactions

    public async Task<ConversationManager> StartConversation(string npcId)
    {
        _debugLogger.LogConversation("Starting", $"NPC: {npcId}");

        NPC npc = npcRepository.GetById(npcId);
        if (npc == null)
        {
            _debugLogger.LogWarning("Conversation", $"NPC {npcId} not found");
            return null;
        }

        Location? location = locationRepository.GetCurrentLocation();
        LocationSpot? locationSpot = locationRepository.GetCurrentLocationSpot();

        _debugLogger.LogDebug($"Starting conversation with {npc.Name} at {location?.Name ?? "null"}/{locationSpot?.SpotID ?? "null"}");

        // Create conversation context
        ConversationContext context = new ConversationContext
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
        ConversationManager conversationManager = await conversationFactory.CreateConversation(context, _gameWorld.GetPlayer());
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
        if (_timeManager.GetCurrentTimeHours() + route.TravelTimeHours > 24)
        {
            throw new InvalidOperationException($"Cannot travel: Not enough time remaining in the day. Route requires {route.TravelTimeHours} hours.");
        }

        // Apply costs
        int timeCost = route.GetActualTimeCost();
        int staminaCost = travelManager.CalculateStaminaCost(route);

        player.SpendStamina(staminaCost);
        player.ModifyCoins(-route.BaseCoinCost); // Negative to deduct coins

        // Store pending travel completion
        _pendingTravelDestination = route.Destination;
        _pendingTravelTimeHours = route.TravelTimeHours;
        _pendingTravelRoute = route;

        // Travel encounters might be implemented later
        // For now, just complete the travel
        CompleteTravelToDestination(route.Destination, route.TravelTimeHours);
    }

    // Store pending travel state for after encounter
    private string _pendingTravelDestination;
    private int _pendingTravelTimeHours;
    private RouteOption _pendingTravelRoute;

    /// <summary>
    /// Complete travel to destination (called directly or after encounter)
    /// </summary>
    private void CompleteTravelToDestination(string destinationId, int travelTimeHours)
    {
        // Travel takes time
        _timeManager.AdvanceTime(travelTimeHours);

        // Check for periodic letter offers after time change
        CheckForPeriodicLetterOffers();

        Location destination = locationRepository.GetLocation(destinationId);

        // Find the first available location spot for the destination
        LocationSpot? destinationSpot = locationSystem.GetLocationSpots(destination.Id).FirstOrDefault();

        locationRepository.SetCurrentLocation(destination, destinationSpot);

        // Also update the Player's location to keep them in sync
        Player player = _gameWorld.GetPlayer();
        player.CurrentLocation = destination;
        player.CurrentLocationSpot = destinationSpot;

        // Clear pending travel state
        _pendingTravelDestination = null;
        _pendingTravelTimeHours = 0;
        _pendingTravelRoute = null;
    }

    /// <summary>
    /// Complete travel after encounter resolution
    /// </summary>
    public void CompleteTravelAfterEncounter()
    {
        if (!string.IsNullOrEmpty(_pendingTravelDestination))
        {
            CompleteTravelToDestination(_pendingTravelDestination, _pendingTravelTimeHours);
        }
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


    // Encounter and action methods removed - use CommandDiscoveryService and ConversationFactory

    public async Task AdvanceToNextDay()
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

    }

    private MorningActivityResult _lastMorningResult;

    public void ProcessDailyLetterQueue()
    {
        // Process letter deadline countdown daily
        letterQueueManager.ProcessDailyDeadlines();

        // Process standing obligations and generate forced letters
        List<Letter> forcedLetters = standingObligationManager.ProcessDailyObligations(_gameWorld.CurrentDay);

        // Add forced letters to the queue with obligation effects
        foreach (Letter letter in forcedLetters)
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
        MorningActivityResult result = _lastMorningResult;
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
        return new GameWorldSnapshot(_gameWorld, _conversationStateManager);
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
            if (_timeManager.GetCurrentTimeHours() + option.RestTimeHours > 24)
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
    /// Check if there's a pending travel encounter
    /// </summary>
    public bool HasPendingTravelEncounter()
    {
        return !string.IsNullOrEmpty(_pendingTravelDestination);
    }

    /// <summary>
    /// Get the pending travel route for encounter generation
    /// </summary>
    public RouteOption GetPendingTravelRoute()
    {
        return _pendingTravelRoute;
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
        TimeBlocks currentTime = _timeManager.GetCurrentTimeBlock();
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
    /// Get next available time for an NPC
    /// </summary>
    public string GetNextAvailableTime(NPC npc)
    {
        TimeBlocks currentTime = _timeManager.GetCurrentTimeBlock();

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
    /// Execute a command by ID - UI must use this
    /// </summary>
    public async Task<bool> ExecuteCommandAsync(string commandId)
    {
        // Discover commands to find the one to execute
        CommandDiscoveryResult discovery = commandDiscovery.DiscoverCommands(_gameWorld);
        DiscoveredCommand commandToExecute = discovery.AllCommands.FirstOrDefault(c => c.UniqueId == commandId);

        if (commandToExecute == null)
        {
            _messageSystem.AddSystemMessage("Command not found", SystemMessageTypes.Danger);
            return false;
        }

        CommandResult result = await commandExecutor.ExecuteAsync(commandToExecute.Command);
        return result.IsSuccess;
    }

    /// <summary>
    /// Get available commands at current location
    /// </summary>
    public CommandDiscoveryResult DiscoverAvailableCommands()
    {
        return commandDiscovery.DiscoverCommands(_gameWorld);
    }

    private ConversationChoice LastSelectedChoice { get; set; }

    public void SetLastSelectedChoice(ConversationChoice choice)
    {
        LastSelectedChoice = choice;
    }

    public ConversationChoice GetLastSelectedChoice()
    {
        return LastSelectedChoice;
    }

    /// <summary>
    /// Complete a pending command that was waiting for an event
    /// </summary>
    public void CompletePendingCommand(PendingCommand pendingCommand)
    {
        if (pendingCommand == null) return;

        // Execute any completion logic based on the command type
        // This is a stub - actual implementation would handle different command types
        _debugLogger.LogDebug($"Completing pending command: {pendingCommand.CommandType} - {pendingCommand.Description}");

        // Clear the pending command
        _gameWorld.PendingCommand = null;
    }

    /// <summary>
    /// Initialize tutorial for new players
    /// </summary>
    private void InitializeTutorialIfNeeded()
    {
        _debugLogger.LogDebug($"InitializeTutorialIfNeeded called. NarrativeManager: {_narrativeManager != null}, Tutorial Complete: {_flagService?.HasFlag(FlagService.TUTORIAL_COMPLETE) ?? false}");
        
        // ALWAYS start tutorial for new games - no player choice
        if (_narrativeManager != null && !_flagService.HasFlag(FlagService.TUTORIAL_COMPLETE))
        {
            _debugLogger.LogDebug("Starting Wayfarer tutorial for new game");
            
            // Set tutorial starting conditions
            var player = _gameWorld.GetPlayer();
            player.Coins = 2; // Tutorial starts with 2 coins
            player.Stamina = 4; // Tutorial starts with 4/10 stamina
            
            // Load tutorial narrative definitions
            NarrativeContentBuilder.BuildAllNarratives();
            _narrativeManager.LoadNarrativeDefinitions(NarrativeDefinitions.All);
            
            _debugLogger.LogDebug($"Narrative definitions loaded. Count: {NarrativeDefinitions.All.Count}");
            
            // Start the tutorial
            _narrativeManager.StartNarrative("wayfarer_tutorial");
            
            _debugLogger.LogDebug($"Tutorial narrative started. Active narratives: {string.Join(", ", _narrativeManager.GetActiveNarratives())}");
            
            _messageSystem.AddSystemMessage("Welcome to Wayfarer. Your journey begins in the Lower Ward...", SystemMessageTypes.Tutorial);
        }
        else
        {
            _debugLogger.LogDebug($"Tutorial not started. NarrativeManager null: {_narrativeManager == null}, Tutorial already complete: {_flagService?.HasFlag(FlagService.TUTORIAL_COMPLETE) ?? false}");
        }
    }

    /// <summary>
    /// Check for collapse after any stamina modification.
    /// Should be called by any system that modifies player stamina.
    /// </summary>
    public void CheckForCollapse()
    {
        if (_collapseManager != null)
        {
            bool collapsed = _collapseManager.CheckAndHandleCollapse();
            
            // Show low stamina warning if at risk
            if (!collapsed && _collapseManager.IsAtRiskOfCollapse())
            {
                string warning = _collapseManager.GetLowStaminaWarning();
                if (!string.IsNullOrEmpty(warning))
                {
                    _messageSystem.AddSystemMessage(warning, SystemMessageTypes.Warning);
                }
            }
        }
    }
}
