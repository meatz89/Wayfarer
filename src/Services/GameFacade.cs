using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static DailyActivitiesManager;

/// <summary>
/// GameFacade - THE single entry point for all UI-Backend communication.
/// This class delegates to existing UIServices and managers to maintain clean separation.
/// </summary>
public class GameFacade
{
    // Queue operation lock to ensure atomicity
    private readonly SemaphoreSlim _queueOperationLock = new(1, 1);
    // Core dependencies
    private readonly GameWorld _gameWorld;
    private readonly ITimeManager _timeManager;
    private readonly MessageSystem _messageSystem;
    private readonly IGameRuleEngine _ruleEngine;


    // Managers
    private readonly TravelManager _travelManager;
    private readonly RestManager _restManager;
    private readonly ObligationQueueManager _letterQueueManager;
    private readonly RouteDiscoveryManager _routeDiscoveryManager;

    // Domain services
    private readonly NPCRepository _npcRepository;
    private readonly LocationRepository _locationRepository;
    private readonly LocationSpotRepository _locationSpotRepository;
    private readonly RouteRepository _routeRepository;
    private readonly FlagService _flagService;
    private readonly ItemRepository _itemRepository;
    private readonly TokenMechanicsManager _connectionTokenManager;
    private readonly ConversationManager _conversationManager;
    private readonly GameConfiguration _gameConfiguration;
    private readonly InformationDiscoveryManager _informationDiscoveryManager;
    private readonly StandingObligationManager _standingObligationManager;
    private readonly StandingObligationRepository _standingObligationRepository;
    private readonly MarketManager _marketManager;
    private readonly DailyActivitiesManager _dailyActivitiesManager;
    private readonly ContextTagCalculator _contextTagCalculator;
    private readonly EnvironmentalHintSystem _environmentalHintSystem;
    private readonly ObservationSystem _observationSystem;
    private readonly ActionGenerator _actionGenerator;
    private readonly BindingObligationSystem _bindingObligationSystem;
    private readonly AtmosphereCalculator _atmosphereCalculator;
    private readonly TimeBlockAttentionManager _timeBlockAttentionManager;
    private readonly NPCDeckFactory _deckFactory;
    private readonly WorldMemorySystem _worldMemorySystem;
    private readonly AmbientDialogueSystem _ambientDialogueSystem;
    private readonly EndingGenerator _endingGenerator;

    public GameFacade(
        GameWorld gameWorld,
        ITimeManager timeManager,
        MessageSystem messageSystem,
        TravelManager travelManager,
        ObligationQueueManager letterQueueManager,
        RouteDiscoveryManager routeDiscoveryManager,
        NPCRepository npcRepository,
        LocationRepository locationRepository,
        LocationSpotRepository locationSpotRepository,
        RouteRepository routeRepository,
        FlagService flagService,
        ItemRepository itemRepository,
        TokenMechanicsManager connectionTokenManager,
        ConversationManager conversationManager,
        GameConfiguration gameConfiguration,
        InformationDiscoveryManager informationDiscoveryManager,
        StandingObligationManager standingObligationManager,
        StandingObligationRepository standingObligationRepository,
        MarketManager marketManager,
        RestManager restManager,
        IGameRuleEngine ruleEngine,
        DailyActivitiesManager dailyActivitiesManager,
        ContextTagCalculator contextTagCalculator,
        ActionGenerator actionGenerator,
        EnvironmentalHintSystem environmentalHintSystem,
        ObservationSystem observationSystem,
        BindingObligationSystem bindingObligationSystem,
        AtmosphereCalculator atmosphereCalculator,
        WorldMemorySystem worldMemorySystem,
        AmbientDialogueSystem ambientDialogueSystem,
        TimeBlockAttentionManager timeBlockAttentionManager,
        NPCDeckFactory deckFactory,
        EndingGenerator endingGenerator
)
    {
        _gameWorld = gameWorld;
        _timeManager = timeManager;
        _messageSystem = messageSystem;
        _travelManager = travelManager;
        _letterQueueManager = letterQueueManager;
        _routeDiscoveryManager = routeDiscoveryManager;
        _npcRepository = npcRepository;
        _locationRepository = locationRepository;
        _locationSpotRepository = locationSpotRepository;
        _routeRepository = routeRepository;
        _flagService = flagService;
        _itemRepository = itemRepository;
        _connectionTokenManager = connectionTokenManager;
        _conversationManager = conversationManager;
        _gameConfiguration = gameConfiguration;
        _informationDiscoveryManager = informationDiscoveryManager;
        _standingObligationManager = standingObligationManager;
        _standingObligationRepository = standingObligationRepository;
        _marketManager = marketManager;
        _restManager = restManager;
        _ruleEngine = ruleEngine;
        _dailyActivitiesManager = dailyActivitiesManager;
        _contextTagCalculator = contextTagCalculator;
        _actionGenerator = actionGenerator;
        _environmentalHintSystem = environmentalHintSystem;
        _observationSystem = observationSystem;
        _bindingObligationSystem = bindingObligationSystem;
        _atmosphereCalculator = atmosphereCalculator;
        _worldMemorySystem = worldMemorySystem;
        _ambientDialogueSystem = ambientDialogueSystem;

        // Use injected TimeBlockAttentionManager
        _timeBlockAttentionManager = timeBlockAttentionManager;
        _deckFactory = deckFactory;
        _endingGenerator = endingGenerator;
    }

    // ========== ATTENTION STATE ACCESS ==========

    /// <summary>
    /// Get current attention state for UI display
    /// This is the single source of truth for attention across all screens
    /// </summary>
    public (int Current, int Max, TimeBlocks TimeBlock) GetCurrentAttentionState()
    {
        TimeBlocks currentTimeBlock = _timeManager.GetCurrentTimeBlock();
        AttentionManager attention = _timeBlockAttentionManager.GetCurrentAttention(currentTimeBlock);

        return (
            attention.GetAvailableAttention(),
            attention.GetMaxAttention(),
            currentTimeBlock
        );
    }

    // ========== EPIC 9: ATTENTION REFRESH SYSTEM ==========

    /// <summary>
    /// Epic 9: Check if attention refresh is available at current location
    /// </summary>
    public bool CanRefreshAttentionAtCurrentLocation()
    {
        Player player = _gameWorld.GetPlayer();
        if (player.CurrentLocationSpot == null) return false;

        // Check if any NPCs at current location provide appropriate services
        List<NPC> npcsHere = _npcRepository.GetNPCsForLocationSpotAndTime(
            player.CurrentLocationSpot.SpotID, 
            _timeManager.GetCurrentTimeBlock()
        );

        // Must have Rest, Lodging, or FoodProduction services for attention refresh
        bool hasAppropriateServices = npcsHere.Any(npc => 
            npc.ProvidedServices.Contains(ServiceTypes.Rest) ||
            npc.ProvidedServices.Contains(ServiceTypes.Lodging) ||
            npc.ProvidedServices.Contains(ServiceTypes.FoodProduction)
        );

        // For testing: temporarily allow refresh at any location with Trade services (Marcus's stall)
        if (!hasAppropriateServices)
        {
            hasAppropriateServices = npcsHere.Any(npc => npc.ProvidedServices.Contains(ServiceTypes.Trade));
        }

        return hasAppropriateServices && _timeBlockAttentionManager.CanRefreshAttention();
    }

    /// <summary>
    /// Epic 9: Get attention refresh status for UI display
    /// </summary>
    public AttentionRefreshStatus GetAttentionRefreshStatus()
    {
        return _timeBlockAttentionManager.GetRefreshStatus();
    }

    /// <summary>
    /// Epic 9: Attempt to refresh attention with quick drink (1 coin = +1 attention)
    /// </summary>
    public AttentionRefreshResult TryRefreshWithQuickDrink()
    {
        Player player = _gameWorld.GetPlayer();
        
        if (!CanRefreshAttentionAtCurrentLocation())
        {
            return new AttentionRefreshResult
            {
                Success = false,
                Message = "No tavern or inn services available here."
            };
        }

        if (player.Coins < GameRules.ATTENTION_REFRESH_QUICK_DRINK_COST)
        {
            return new AttentionRefreshResult
            {
                Success = false,
                Message = $"Need {GameRules.ATTENTION_REFRESH_QUICK_DRINK_COST} coin for a quick drink.",
                RemainingAttention = _timeBlockAttentionManager.GetAttentionState().current
            };
        }

        AttentionRefreshResult result = _timeBlockAttentionManager.TryRefreshWithCoins(
            GameRules.ATTENTION_REFRESH_QUICK_DRINK_POINTS,
            GameRules.ATTENTION_REFRESH_QUICK_DRINK_COST
        );

        if (result.Success)
        {
            player.ModifyCoins(-GameRules.ATTENTION_REFRESH_QUICK_DRINK_COST);
            _messageSystem.AddSystemMessage(
                "☕ A quick drink sharpens your focus.", 
                SystemMessageTypes.Success
            );
        }

        return result;
    }

    /// <summary>
    /// Epic 9: Attempt to refresh attention with full meal (3 coins = +2 attention)
    /// </summary>
    public AttentionRefreshResult TryRefreshWithFullMeal()
    {
        Player player = _gameWorld.GetPlayer();
        
        if (!CanRefreshAttentionAtCurrentLocation())
        {
            return new AttentionRefreshResult
            {
                Success = false,
                Message = "No tavern or inn services available here."
            };
        }

        if (player.Coins < GameRules.ATTENTION_REFRESH_FULL_MEAL_COST)
        {
            return new AttentionRefreshResult
            {
                Success = false,
                Message = $"Need {GameRules.ATTENTION_REFRESH_FULL_MEAL_COST} coins for a full meal.",
                RemainingAttention = _timeBlockAttentionManager.GetAttentionState().current
            };
        }

        AttentionRefreshResult result = _timeBlockAttentionManager.TryRefreshWithCoins(
            GameRules.ATTENTION_REFRESH_FULL_MEAL_POINTS,
            GameRules.ATTENTION_REFRESH_FULL_MEAL_COST
        );

        if (result.Success)
        {
            player.ModifyCoins(-GameRules.ATTENTION_REFRESH_FULL_MEAL_COST);
            _messageSystem.AddSystemMessage(
                "🍽️ A hearty meal restores your energy and focus.", 
                SystemMessageTypes.Success
            );
        }

        return result;
    }

    /// <summary>
    /// Check if player has any attention remaining
    /// </summary>
    public bool HasAttentionRemaining()
    {
        return _timeBlockAttentionManager.HasAttentionRemaining();
    }

    // ========== TIME ADVANCEMENT ==========

    /// <summary>
    /// Public method for advancing game time. Ensures all time-dependent systems are updated.
    /// This should be used by ALL managers instead of calling TimeManager directly.
    /// </summary>
    public void AdvanceGameTime(int hours)
    {
        if (hours <= 0) return;
        ProcessTimeAdvancement(hours);
    }

    /// <summary>
    /// Public method for advancing game time by minutes. Ensures all time-dependent systems are updated.
    /// This should be used by ALL managers instead of calling TimeManager directly.
    /// </summary>
    public void AdvanceGameTimeMinutes(int minutes)
    {
        if (minutes <= 0) return;
        ProcessTimeAdvancementMinutes(minutes);
    }

    // ========== HELPER METHODS ==========

    private void ProcessTimeAdvancement(int hours)
    {
        TimeBlocks oldTimeBlock = _timeManager.GetCurrentTimeBlock();
        _timeManager.AdvanceTime(hours);
        TimeBlocks newTimeBlock = _timeManager.GetCurrentTimeBlock();

        // Check if we've moved to a new time block - this triggers attention refresh
        if (oldTimeBlock != newTimeBlock)
        {
            Console.WriteLine($"[GameFacade] Time block changed from {oldTimeBlock} to {newTimeBlock} - attention will refresh on next use");
        }

        // Update letter deadlines when time advances
        _letterQueueManager.ProcessHourlyDeadlines(hours);

        // Process carried information letters after time change
        // Information revelation handled through other systems now
    }

    private void ProcessTimeAdvancementMinutes(int minutes)
    {
        // Convert to hours for deadline processing
        int hours = minutes / 60;

        _timeManager.AdvanceTimeMinutes(minutes);

        // Update letter deadlines when time advances (even partial hours)
        if (hours > 0)
        {
            _letterQueueManager.ProcessHourlyDeadlines(hours);
        }

        // Process carried information letters after time change
        // Information revelation handled through other systems now
    }

    private string GetMarketAvailabilityStatus(string locationId)
    {
        List<NPC> traders = GetTradingNPCs(locationId);
        List<NPC> availableTraders = traders.Where(npc => npc.IsAvailable(_timeManager.GetCurrentTimeBlock())).ToList();

        if (!availableTraders.Any())
        {
            return "Market closed - no traders available";
        }

        return $"Market open - {availableTraders.Count} trader(s) available";
    }

    private List<NPC> GetTradingNPCs(string locationId)
    {
        return _npcRepository.GetNPCsForLocation(locationId)
            .Where(npc => npc.ProvidedServices.Contains(ServiceTypes.Trade))
            .ToList();
    }

    private List<MarketItem> GetAvailableMarketItems(string locationId)
    {
        if (_marketManager == null) return new List<MarketItem>();

        return _marketManager.GetMarketItems(locationId)
            .Where(item => item.Stock > 0)
            .ToList();
    }

    private bool CanBuyMarketItem(string itemId, string locationId)
    {
        if (_marketManager == null) return false;

        MarketItem item = _marketManager.GetMarketItem(locationId, itemId);
        if (item == null || item.Stock <= 0) return false;

        Player player = _gameWorld.GetPlayer();
        return player.Coins >= item.Price;
    }

    public int CalculateTotalWeight()
    {
        Player player = _gameWorld.GetPlayer();
        int totalWeight = 0;

        // Add item weights
        foreach (string itemName in player.Inventory.ItemSlots)
        {
            if (!string.IsNullOrEmpty(itemName))
            {
                Item item = _itemRepository.GetItemById(itemName);
                if (item != null)
                {
                    totalWeight += item.Weight;
                }
            }
        }

        // Add letter sizes from physical satchel
        foreach (Letter letter in player.CarriedLetters)
        {
            if (letter != null)
            {
                totalWeight += letter.Size;
            }
        }

        return totalWeight;
    }

    // ========== GAME STATE QUERIES ==========

    public GameWorldSnapshot GetGameSnapshot()
    {
        return new GameWorldSnapshot(_gameWorld);
    }

    public Player GetPlayer()
    {
        return _gameWorld.GetPlayer();
    }

    public (Location location, LocationSpot spot) GetCurrentLocation()
    {
        Player player = _gameWorld.GetPlayer();
        Location? location = player.CurrentLocationSpot != null
            ? _locationRepository.GetLocation(player.CurrentLocationSpot.LocationId)
            : null;
        return (location, player.CurrentLocationSpot);
    }

    public (TimeBlocks timeBlock, int hoursRemaining, int currentDay) GetTimeInfo()
    {
        return (_timeManager.GetCurrentTimeBlock(),
                _timeManager.HoursRemaining,
                _gameWorld.CurrentDay);
    }

    public int GetCurrentDay()
    {
        return _gameWorld.CurrentDay;
    }

    /// <summary>
    /// Record a significant event for environmental storytelling
    /// </summary>
    public void RecordWorldEvent(WorldEventType type, string actorId, string targetId = null, string locationId = null)
    {
        _worldMemorySystem?.RecordEvent(type, actorId, targetId, locationId);
    }

    /// <summary>
    /// Get ambient dialogue for NPCs at current location
    /// </summary>
    public List<string> GetLocationAmbience()
    {
        if (_ambientDialogueSystem == null) return new List<string>();

        Location currentLocation = _locationRepository.GetCurrentLocation();
        if (currentLocation == null) return new List<string>();

        return _ambientDialogueSystem.GetLocationAmbience(currentLocation.Id);
    }

    /// <summary>
    /// Gets the current hour of the day (0-23)
    /// </summary>
    public int GetCurrentHour()
    {
        return _timeManager.GetCurrentTimeHours();
    }

    /// <summary>
    /// Gets formatted time display with day name and time.
    /// Returns format like "MON 3:30 PM"
    /// </summary>
    public string GetFormattedTimeDisplay()
    {
        return _timeManager.GetFormattedTimeDisplay();
    }

    // ========== LOCATION ACTIONS ==========

    public LocationScreenViewModel GetLocationScreen()
    {
        Player player = _gameWorld.GetPlayer();
        (Location location, LocationSpot spot) = GetCurrentLocation();
        
        var viewModel = new LocationScreenViewModel
        {
            CurrentTime = _timeManager.GetFormattedTimeDisplay(),
            DeadlineTimer = GetNextDeadlineDisplay(),
            LocationPath = BuildLocationPath(location, spot),
            LocationName = location?.Name ?? "Unknown Location",
            CurrentSpotName = spot?.Name,
            LocationTraits = GetLocationTraits(location, spot),
            AtmosphereText = spot?.Description ?? location?.Description ?? "A quiet place.",
            QuickActions = new List<LocationActionViewModel>(),
            NPCsPresent = new List<NPCPresenceViewModel>(),
            Observations = new List<ObservationViewModel>(),
            AreasWithinLocation = new List<AreaWithinLocationViewModel>(),
            Routes = new List<RouteOptionViewModel>()
        };
        
        if (location != null && spot != null)
        {
            // Add location-specific actions
            viewModel.QuickActions = GetLocationActions(location, spot);
            
            // Add NPCs with emotional states
            TimeBlocks currentTime = _timeManager.GetCurrentTimeBlock();
            var npcs = _npcRepository.GetNPCsForLocationSpotAndTime(spot.SpotID, currentTime);
            
            foreach (var npc in npcs)
            {
                var emotionalState = GetNPCEmotionalState(npc);
                viewModel.NPCsPresent.Add(new NPCPresenceViewModel
                {
                    Id = npc.ID,
                    Name = npc.Name,
                    MoodEmoji = GetEmotionalStateEmoji(emotionalState),
                    EmotionalStateName = emotionalState.ToString(),
                    Description = GetNPCDescription(npc, emotionalState),
                    Interactions = new List<InteractionOptionViewModel>
                    {
                        new InteractionOptionViewModel
                        {
                            Text = emotionalState == EmotionalState.DESPERATE 
                                ? "Approach Her Table" 
                                : $"Begin Conversation",
                            Cost = "1 attention"
                        }
                    }
                });
            }
            
            // Add observations
            viewModel.Observations = GetLocationObservations(location.Id);
            
            // Add areas within location
            viewModel.AreasWithinLocation = GetAreasWithinLocation(location, spot);
            
            // Add routes to other locations
            viewModel.Routes = GetRoutesFromLocation(location);
        }
        
        return viewModel;
    }
    
    private List<string> BuildLocationPath(Location location, LocationSpot spot)
    {
        var path = new List<string>();
        if (location != null)
        {
            // Build path from location hierarchy
            // For now, use location name as the primary path element
            // TODO: When location hierarchy is properly defined in JSON, 
            // use connectedTo relationships to build full path
            path.Add(location.Name);
        }
        if (spot != null && spot.Name != location?.Name)
        {
            path.Add(spot.Name);
        }
        return path;
    }
    
    private List<string> GetLocationTraits(Location location, LocationSpot spot)
    {
        // Use LocationTraitsParser for systematic trait generation from JSON data
        TimeBlocks currentTime = _timeManager.GetCurrentTimeBlock();
        return LocationTraitsParser.ParseLocationTraits(location, currentTime);
    }
    
    private List<LocationActionViewModel> GetLocationActions(Location location, LocationSpot spot)
    {
        // Use ActionGenerator for systematic action generation
        return _actionGenerator.GenerateActionsForLocation(location, spot);
    }
    
    private EmotionalState GetNPCEmotionalState(NPC npc)
    {
        // Determine emotional state from letter deadlines
        var obligations = _letterQueueManager?.GetActiveObligations() ?? new DeliveryObligation[0];
        var npcLetters = obligations.Where(o => o.SenderId == npc.ID || o.SenderName == npc.Name);
        var mostUrgent = npcLetters.OrderBy(o => o.DeadlineInMinutes).FirstOrDefault();
        
        if (mostUrgent == null) return EmotionalState.NEUTRAL;
        
        // Apply conversation-system.md rules
        if (mostUrgent.Stakes == StakeType.SAFETY && mostUrgent.DeadlineInMinutes < 360)
            return EmotionalState.DESPERATE;
        if (mostUrgent.DeadlineInMinutes < 720)
            return EmotionalState.TENSE;
            
        // Check personality-based states
        if (npc.PersonalityType == PersonalityType.MERCANTILE)
            return EmotionalState.NEUTRAL; // Calculating maps to NEUTRAL with business focus
            
        return EmotionalState.NEUTRAL;
    }
    
    private string GetEmotionalStateEmoji(EmotionalState state)
    {
        return state switch
        {
            EmotionalState.DESPERATE => "😰",
            EmotionalState.TENSE => "😟",
            EmotionalState.HOSTILE => "😠",
            EmotionalState.GUARDED => "🤨",
            EmotionalState.OPEN => "😊",
            EmotionalState.CONNECTED => "🤝",
            EmotionalState.EAGER => "😃",
            EmotionalState.OVERWHELMED => "😵",
            _ => "😐"
        };
    }
    
    private string GetNPCDescription(NPC npc, EmotionalState state)
    {
        // Generate categorical description based on profession and state
        var baseActivity = npc.Profession switch
        {
            Professions.Scribe => "Hunched over documents",
            Professions.Merchant => "Arranging goods with practiced efficiency",
            Professions.Innkeeper => "Polishing glasses behind the bar",
            _ => npc.Description ?? "Going about their business"
        };
        
        if (state == EmotionalState.DESPERATE)
        {
            return "Clutching a sealed letter with white knuckles, eyes darting to the door with each patron's entrance.";
        }
        
        return baseActivity;
    }
    
    private List<ObservationViewModel> GetLocationObservations(string locationId)
    {
        var observations = new List<ObservationViewModel>();
        
        // Get observations from ObservationSystem
        var locationObservations = _observationSystem?.GetObservationsForLocation(locationId);
        
        if (locationObservations != null)
        {
            foreach (var obs in locationObservations)
            {
                observations.Add(new ObservationViewModel
                {
                    Text = obs.Text,
                    Icon = obs.Type == ObservationType.Important ? "⚠️" : "👁️",
                    AttentionCost = obs.AttentionCost,
                    Relevance = BuildRelevanceString(obs),
                    IsObserved = _observationSystem.IsObserved(obs.Id)
                });
            }
        }
        
        return observations;
    }
    
    private string BuildRelevanceString(Observation obs)
    {
        if (obs.RelevantNPCs?.Any() == true)
        {
            var npcs = string.Join(", ", obs.RelevantNPCs.Select(id => 
                _npcRepository.GetById(id)?.Name ?? id));
            
            if (obs.CreatesState.HasValue)
                return $"→ {npcs} ({obs.CreatesState.Value})";
            else
                return $"→ {npcs}";
        }
        return "";
    }
    
    private List<AreaWithinLocationViewModel> GetAreasWithinLocation(Location location, LocationSpot currentSpot)
    {
        var areas = new List<AreaWithinLocationViewModel>();
        
        // Get all spots in the same location
        var spots = _locationSpotRepository.GetSpotsForLocation(location.Id);
        
        foreach (var spot in spots)
        {
            areas.Add(new AreaWithinLocationViewModel
            {
                Name = spot.Name,
                Detail = GetSpotDetail(spot),
                SpotId = spot.SpotID,
                IsCurrent = spot.SpotID == currentSpot?.SpotID
            });
        }
        
        return areas;
    }
    
    private string GetSpotDetail(LocationSpot spot)
    {
        // Generate detail based on spot ID
        return spot.SpotID switch
        {
            "marcus_stall" => "Cloth merchant's stall",
            "central_fountain" => "Gathering place",
            "north_entrance" => "To Noble District",
            "main_hall" => "Common room",
            "bar_counter" => "Bertram's domain",
            "corner_table" => "Private conversations",
            _ => spot.Description ?? ""
        };
    }
    
    private List<RouteOptionViewModel> GetRoutesFromLocation(Location location)
    {
        var routes = new List<RouteOptionViewModel>();
        
        // Get available routes
        var availableRoutes = _routeRepository.GetRoutesFromLocation(location.Id);
        
        foreach (var route in availableRoutes)
        {
            var destination = _locationRepository.GetLocation(route.Destination);
            if (destination != null)
            {
                // Each route is ONE specific transport method - no multiple options
                routes.Add(new RouteOptionViewModel
                {
                    RouteId = route.Id,
                    Destination = destination.Name,
                    TravelTime = $"{route.TravelTimeMinutes} min",
                    Detail = route.Description ?? route.Name,
                    IsLocked = !route.IsDiscovered,
                    LockReason = !route.IsDiscovered ? "Route not yet discovered" : null,
                    RequiredTier = route.TierRequired,
                    
                    // Each route IS a specific transport method
                    TransportMethod = route.Method.ToString().ToLower(),
                    
                    // These are NOT used - each route is ONE method
                    SupportsCart = false,
                    SupportsCarriage = false,
                    
                    // Additional route info
                    Familiarity = route.IsDiscovered ? null : "Unknown route"
                });
            }
        }
        
        return routes;
    }
    
    private string GetNextDeadlineDisplay()
    {
        var queue = GetLetterQueue();
        if (queue?.QueueSlots == null) return "";
        
        var mostUrgent = queue.QueueSlots
            .Where(s => s.IsOccupied && s.DeliveryObligation != null)
            .OrderBy(s => s.DeliveryObligation.DeadlineInHours)
            .FirstOrDefault();
            
        if (mostUrgent?.DeliveryObligation == null) return "";
        
        var deadline = mostUrgent.DeliveryObligation;
        if (deadline.DeadlineInHours <= 3)
            return $"⚡ {deadline.RecipientName}: {deadline.DeadlineInHours}h";
        else if (deadline.DeadlineInHours <= 24)
            return $"📜 Next: {deadline.RecipientName} in {deadline.DeadlineInHours}h";
        else
            return "";
    }
    
    private string GetNPCMoodEmoji(NPC npc)
    {
        // Simple mood display based on NPC state
        return "😐";
    }

    public LocationActionsViewModel GetLocationActions()
    {
        Player player = _gameWorld.GetPlayer();
        LocationSpot currentSpot = player.CurrentLocationSpot;

        if (currentSpot == null)
        {
            return new LocationActionsViewModel
            {
                LocationName = "Unknown Location",
                CurrentTimeBlock = _timeManager.GetCurrentTimeBlock().ToString(),
                HoursRemaining = _timeManager.HoursRemaining,
                PlayerStamina = player.Stamina,
                PlayerCoins = player.Coins,
                ActionGroups = new List<ActionGroupViewModel>()
            };
        }

        LocationActionsViewModel viewModel = new LocationActionsViewModel
        {
            LocationName = currentSpot.Name,
            CurrentTimeBlock = _timeManager.GetCurrentTimeBlock().ToString(),
            HoursRemaining = _timeManager.HoursRemaining,
            PlayerStamina = player.Stamina,
            PlayerCoins = player.Coins,
            ActionGroups = new List<ActionGroupViewModel>()
        };

        // Generate actions based on current context
        List<ActionOptionViewModel> actions = new List<ActionOptionViewModel>();

        // Add NPC actions
        List<NPC> npcsHere = _npcRepository.GetNPCsForLocationSpotAndTime(currentSpot.SpotID, _timeManager.GetCurrentTimeBlock());
        foreach (NPC npc in npcsHere)
        {
            // All NPCs are visible in the new architecture
            {
                actions.Add(new ActionOptionViewModel
                {
                    Id = $"talk_{npc.ID}",
                    Description = $"Talk with {npc.Name}",
                    NPCName = npc.Name,
                    TimeCost = 1,
                    StaminaCost = 0,
                    CoinCost = 0,
                    HasEnoughTime = _timeManager.HoursRemaining >= 1,
                    HasEnoughStamina = true,
                    HasEnoughCoins = true,
                    RewardsDescription = "Information and relationship building"
                });
            }
        }

        // Add rest actions
        if (!_flagService.HasFlag("tutorial_active") || actions.Count < 3)
        {
            actions.Add(new ActionOptionViewModel
            {
                Id = "rest_1",
                Description = "Rest for 1 hour",
                TimeCost = 1,
                StaminaCost = 0,
                CoinCost = 0,
                HasEnoughTime = _timeManager.HoursRemaining >= 1,
                HasEnoughStamina = true,
                HasEnoughCoins = true,
                RewardsDescription = "+2 stamina"
            });

            if (!_flagService.HasFlag("tutorial_active"))
            {
                actions.Add(new ActionOptionViewModel
                {
                    Id = "rest_2",
                    Description = "Rest for 2 hours",
                    TimeCost = 2,
                    StaminaCost = 0,
                    CoinCost = 0,
                    HasEnoughTime = _timeManager.HoursRemaining >= 2,
                    HasEnoughStamina = true,
                    HasEnoughCoins = true,
                    RewardsDescription = "+4 stamina"
                });

                actions.Add(new ActionOptionViewModel
                {
                    Id = "rest_4",
                    Description = "Rest for 4 hours",
                    TimeCost = 4,
                    StaminaCost = 0,
                    CoinCost = 0,
                    HasEnoughTime = _timeManager.HoursRemaining >= 4,
                    HasEnoughStamina = true,
                    HasEnoughCoins = true,
                    RewardsDescription = "+10 stamina"
                });
            }
        }

        // Add observe action
        actions.Add(new ActionOptionViewModel
        {
            Id = "observe",
            Description = "Observe location",
            TimeCost = 1,
            StaminaCost = 0,
            CoinCost = 0,
            HasEnoughTime = _timeManager.HoursRemaining >= 1,
            HasEnoughStamina = true,
            HasEnoughCoins = true,
            RewardsDescription = "Study your surroundings for opportunities"
        });

        // Epic 9: Add attention refresh actions at appropriate locations
        if (CanRefreshAttentionAtCurrentLocation())
        {
            AttentionRefreshStatus refreshStatus = GetAttentionRefreshStatus();
            
            if (refreshStatus.CanRefresh)
            {
                actions.Add(new ActionOptionViewModel
                {
                    Id = "attention_quick_drink",
                    Description = $"Quick drink ({GameRules.ATTENTION_REFRESH_QUICK_DRINK_COST} coin)",
                    TimeCost = 0,
                    StaminaCost = 0,
                    CoinCost = GameRules.ATTENTION_REFRESH_QUICK_DRINK_COST,
                    HasEnoughTime = true,
                    HasEnoughStamina = true,
                    HasEnoughCoins = player.Coins >= GameRules.ATTENTION_REFRESH_QUICK_DRINK_COST,
                    RewardsDescription = $"+{GameRules.ATTENTION_REFRESH_QUICK_DRINK_POINTS} attention"
                });

                actions.Add(new ActionOptionViewModel
                {
                    Id = "attention_full_meal",
                    Description = $"Full meal ({GameRules.ATTENTION_REFRESH_FULL_MEAL_COST} coins)",
                    TimeCost = 0,
                    StaminaCost = 0,
                    CoinCost = GameRules.ATTENTION_REFRESH_FULL_MEAL_COST,
                    HasEnoughTime = true,
                    HasEnoughStamina = true,
                    HasEnoughCoins = player.Coins >= GameRules.ATTENTION_REFRESH_FULL_MEAL_COST,
                    RewardsDescription = $"+{GameRules.ATTENTION_REFRESH_FULL_MEAL_POINTS} attention"
                });
            }
        }

        // Group actions by category
        List<ActionOptionViewModel> socialActions = actions.Where(a => a.Id.StartsWith("talk_")).ToList();
        List<ActionOptionViewModel> restActions = actions.Where(a => a.Id.StartsWith("rest_")).ToList();
        List<ActionOptionViewModel> specialActions = actions.Where(a => !a.Id.StartsWith("talk_") && !a.Id.StartsWith("rest_")).ToList();

        if (socialActions.Any())
        {
            viewModel.ActionGroups.Add(new ActionGroupViewModel
            {
                ActionType = "Social",
                Actions = socialActions
            });
        }

        if (restActions.Any())
        {
            viewModel.ActionGroups.Add(new ActionGroupViewModel
            {
                ActionType = "Rest",
                Actions = restActions
            });
        }

        if (specialActions.Any())
        {
            viewModel.ActionGroups.Add(new ActionGroupViewModel
            {
                ActionType = "Special",
                Actions = specialActions
            });
        }

        // Add closed services information
        AddClosedServicesInfo(viewModel, currentSpot);

        return viewModel;
    }

    public async Task<bool> ExecuteLocationActionAsync(string actionId)
    {
        Console.WriteLine($"[GameFacade.ExecuteLocationActionAsync] Starting execution for action: {actionId}");

        // Convert action ID to intent
        PlayerIntent intent = null;

        if (actionId.StartsWith("talk_"))
        {
            string npcId = actionId.Substring(5);
            intent = new TalkIntent(npcId);
        }
        else if (actionId.StartsWith("rest_"))
        {
            int hours = int.Parse(actionId.Substring(5));
            intent = new RestIntent(hours);
        }
        else if (actionId == "observe")
        {
            intent = new ObserveLocationIntent();
        }
        else if (actionId == "attention_quick_drink")
        {
            // Epic 9: Handle quick drink refresh
            AttentionRefreshResult result = TryRefreshWithQuickDrink();
            if (!result.Success)
            {
                _messageSystem.AddSystemMessage(result.Message, SystemMessageTypes.Warning);
            }
            return result.Success;
        }
        else if (actionId == "attention_full_meal")
        {
            // Epic 9: Handle full meal refresh
            AttentionRefreshResult result = TryRefreshWithFullMeal();
            if (!result.Success)
            {
                _messageSystem.AddSystemMessage(result.Message, SystemMessageTypes.Warning);
            }
            return result.Success;
        }
        else if (actionId.StartsWith("move_"))
        {
            string spotId = actionId.Substring(5);
            intent = new MoveIntent(spotId);
        }
        else
        {
            Console.WriteLine($"[GameFacade.ExecuteLocationActionAsync] Unknown action ID format: {actionId}");
            _messageSystem.AddSystemMessage($"Unknown action: {actionId}", SystemMessageTypes.Danger);
            return false;
        }

        // Execute the intent
        return await ExecuteIntent(intent);
    }

    // ========== INTENT-BASED EXECUTION ==========

    /// <summary>
    /// Execute a player intent using GameWorld as the single source of truth
    /// </summary>
    public async Task<bool> ExecuteIntent(PlayerIntent intent)
    {
        Console.WriteLine($"[GameFacade.ExecuteIntent] Executing {intent.GetType().Name}");

        try
        {
            return intent switch
            {
                MoveIntent move => await ExecuteMove(move),
                TalkIntent talk => await ExecuteTalk(talk),
                RestIntent rest => await ExecuteRest(rest),
                WaitIntent wait => await ExecuteWait(wait),
                DeliverLetterIntent deliver => await ExecuteDeliverLetter(deliver),
                CollectLetterIntent collect => await ExecuteCollectLetter(collect),
                ObserveLocationIntent observe => await ExecuteObserve(observe),
                ExploreAreaIntent explore => await ExecuteExplore(explore),
                RequestPatronFundsIntent patron => await ExecutePatronFunds(patron),
                AcceptLetterOfferIntent offer => await ExecuteAcceptOffer(offer), // Legacy - returns false
                TravelIntent travel => await ExecuteTravel(travel),
                DiscoverRouteIntent discover => await ExecuteDiscoverRoute(discover),
                _ => throw new NotSupportedException($"Unknown intent type: {intent.GetType()}")
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GameFacade.ExecuteIntent] Error: {ex.Message}");
            _messageSystem.AddSystemMessage($"Failed to execute action: {ex.Message}", SystemMessageTypes.Danger);
            return false;
        }
    }

    private async Task<bool> ExecuteMove(MoveIntent intent)
    {
        // Get context from GameWorld
        Player player = _gameWorld.GetPlayer();
        LocationSpot currentSpot = player.CurrentLocationSpot;

        if (currentSpot == null)
        {
            _messageSystem.AddSystemMessage("Cannot determine current location", SystemMessageTypes.Danger);
            return false;
        }

        // Get target spot from repositories (should ideally come from GameWorld)
        Location currentLocation = _locationRepository.GetLocation(currentSpot.LocationId);
        if (currentLocation == null)
        {
            _messageSystem.AddSystemMessage("Cannot determine current location", SystemMessageTypes.Danger);
            return false;
        }

        List<LocationSpot> spotsInLocation = _locationRepository.GetSpotsForLocation(currentLocation.Id);
        LocationSpot? targetSpot = spotsInLocation.FirstOrDefault(s => s.SpotID == intent.TargetSpotId);

        if (targetSpot == null)
        {
            _messageSystem.AddSystemMessage("Target location does not exist", SystemMessageTypes.Danger);
            return false;
        }

        if (targetSpot.IsClosed)
        {
            _messageSystem.AddSystemMessage($"{targetSpot.Name} is closed", SystemMessageTypes.Warning);
            return false;
        }

        // Check if in same location
        if (targetSpot.LocationId != currentSpot.LocationId)
        {
            _messageSystem.AddSystemMessage("Target is in a different location. Use travel instead.", SystemMessageTypes.Warning);
            return false;
        }

        // Check stamina
        if (player.Stamina < 1)
        {
            _messageSystem.AddSystemMessage("Not enough stamina to move", SystemMessageTypes.Warning);
            return false;
        }

        // Execute movement
        player.SpendStamina(1);
        _locationRepository.SetCurrentLocation(currentLocation, targetSpot);

        _messageSystem.AddSystemMessage($"Moved to {targetSpot.Name}", SystemMessageTypes.Success);
        return true;
    }

    private async Task<bool> ExecuteTalk(TalkIntent intent)
    {
        Player player = _gameWorld.GetPlayer();
        NPC npc = _npcRepository.GetById(intent.NpcId);

        if (npc == null)
        {
            _messageSystem.AddSystemMessage("NPC not found", SystemMessageTypes.Danger);
            return false;
        }

        // Check if NPC is at current location
        if (!npc.IsAvailableAtLocation(player.CurrentLocationSpot?.SpotID))
        {
            _messageSystem.AddSystemMessage($"{npc.Name} is not here", SystemMessageTypes.Warning);
            return false;
        }

        try
        {
            // Start conversation with the new card-based system
            var session = _conversationManager.StartConversation(npc.ID);
            if (session == null)
            {
                _messageSystem.AddSystemMessage($"Cannot talk to {npc.Name} right now", SystemMessageTypes.Warning);
                return false;
            }

            _messageSystem.AddSystemMessage($"Started conversation with {npc.Name}", SystemMessageTypes.Success);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ExecuteTalk] Exception: {ex}");
            _messageSystem.AddSystemMessage($"Error starting conversation: {ex.Message}", SystemMessageTypes.Danger);
            return false;
        }
    }

    private async Task<bool> ExecuteWait(WaitIntent intent)
    {
        // Calculate time to next period
        TimeBlocks currentTime = _timeManager.GetCurrentTimeBlock();
        int currentHour = _timeManager.GetCurrentTimeHours();

        // Determine next time block and hours to advance
        int hoursToAdvance = currentTime switch
        {
            TimeBlocks.Dawn => 8 - currentHour,      // Dawn (6-8) -> Morning (8)
            TimeBlocks.Morning => 12 - currentHour,   // Morning (8-12) -> Afternoon (12)
            TimeBlocks.Afternoon => 17 - currentHour, // Afternoon (12-17) -> Evening (17)
            TimeBlocks.Evening => 20 - currentHour,   // Evening (17-20) -> Night (20)
            TimeBlocks.Night => 22 - currentHour,     // Night (20-22) -> Late Night (22)
            TimeBlocks.LateNight => 30 - currentHour, // Late Night (22-6) -> Next Dawn (+6)
            _ => 2 // Default advance 2 hours
        };

        // Make sure we advance at least 1 hour
        if (hoursToAdvance <= 0) hoursToAdvance = 1;

        // Advance time (using ProcessTimeAdvancement to ensure deadlines are updated)
        ProcessTimeAdvancement(hoursToAdvance);

        // The TimeBlockAttentionManager will automatically refresh attention for the new time block

        // Add narrative message about waiting
        TimeBlocks newTime = _timeManager.GetCurrentTimeBlock();
        string message = newTime switch
        {
            TimeBlocks.Dawn => "You wait as the first light breaks over the horizon.",
            TimeBlocks.Morning => "The morning sun climbs higher as time passes.",
            TimeBlocks.Afternoon => "The day wears on toward afternoon.",
            TimeBlocks.Evening => "Shadows lengthen as evening approaches.",
            TimeBlocks.Night => "Darkness falls across the town.",
            TimeBlocks.LateNight => "The deep of night settles in.",
            _ => "Time passes quietly."
        };

        _messageSystem.AddSystemMessage(message, SystemMessageTypes.Info);

        // Check for missed deadlines from letter queue
        LetterQueueViewModel letterQueue = GetLetterQueue();
        if (letterQueue?.QueueSlots != null)
        {
            foreach (QueueSlotViewModel slot in letterQueue.QueueSlots)
            {
                if (slot.IsOccupied && slot.DeliveryObligation?.DeadlineInHours <= 0)
                {
                    _messageSystem.AddSystemMessage($"DeliveryObligation to {slot.DeliveryObligation.RecipientName} has expired!", SystemMessageTypes.Danger);
                }
            }
        }

        return true;
    }

    private async Task<bool> ExecuteRest(RestIntent intent)
    {
        Player player = _gameWorld.GetPlayer();

        // Check if player needs rest
        if (player.Stamina >= player.MaxStamina)
        {
            _messageSystem.AddSystemMessage("Already at maximum stamina", SystemMessageTypes.Warning);
            return false;
        }

        // Check if enough time remaining
        if (_timeManager.HoursRemaining < intent.Hours)
        {
            _messageSystem.AddSystemMessage($"Not enough time remaining (need {intent.Hours} hours)", SystemMessageTypes.Warning);
            return false;
        }

        // Calculate stamina recovery based on hours
        int staminaRecovery = intent.Hours switch
        {
            1 => 2,
            2 => 4,
            4 => 10,
            _ => intent.Hours * 2 // Default formula
        };

        // Rest and recover stamina
        ProcessTimeAdvancement(intent.Hours);
        int actualRecovery = Math.Min(staminaRecovery, player.MaxStamina - player.Stamina);
        player.ModifyStamina(actualRecovery);

        _messageSystem.AddSystemMessage(
            $"Rested for {intent.Hours} hour(s) and recovered {actualRecovery} stamina",
            SystemMessageTypes.Success
        );

        return true;
    }

    private async Task<bool> ExecuteDeliverLetter(DeliverLetterIntent intent)
    {
        Player player = _gameWorld.GetPlayer();

        // Find the letter in the player's queue
        DeliveryObligation letterToDeliver = null;
        int letterPosition = -1;

        for (int i = 0; i < player.ObligationQueue.Length; i++)
        {
            if (player.ObligationQueue[i]?.Id == intent.LetterId)
            {
                letterToDeliver = player.ObligationQueue[i];
                letterPosition = i + 1; // Queue positions are 1-indexed
                break;
            }
        }

        if (letterToDeliver == null)
        {
            _messageSystem.AddSystemMessage("DeliveryObligation not found in your queue", SystemMessageTypes.Warning);
            return false;
        }

        // Find the recipient NPC by name
        NPC? recipient = _npcRepository.GetAllNPCs()
            .FirstOrDefault(npc => npc.Name.Equals(letterToDeliver.RecipientName, StringComparison.OrdinalIgnoreCase));
        if (recipient == null)
        {
            _messageSystem.AddSystemMessage($"Cannot find {letterToDeliver.RecipientName} here", SystemMessageTypes.Warning);
            return false;
        }

        // Check if recipient is available
        if (!recipient.IsAvailable(_timeManager.GetCurrentTimeBlock()))
        {
            _messageSystem.AddSystemMessage($"{recipient.Name} is not available right now", SystemMessageTypes.Warning);
            return false;
        }

        // Delivery now handled through conversation cards
        // Simple delivery for now - will be expanded through conversation system
        DeliveryOutcome outcome = new DeliveryOutcome 
        { 
            BasePayment = letterToDeliver.Payment,
            BonusPayment = 0,
            TokenType = ConnectionType.Trust,
            TokenAmount = 1
        };

        // Process payment
        player.ModifyCoins(outcome.BasePayment + outcome.BonusPayment);
        _messageSystem.AddSystemMessage($"Received {outcome.BasePayment + outcome.BonusPayment} coins for delivery", SystemMessageTypes.Success);

        // Process token rewards/penalties
        if (outcome.TokenReward && outcome.TokenType != default(ConnectionType))
        {
            _connectionTokenManager.AddTokensToNPC(outcome.TokenType, outcome.TokenAmount, recipient.ID);
            _messageSystem.AddSystemMessage($"Gained {outcome.TokenAmount} {outcome.TokenType} token with {recipient.Name}", SystemMessageTypes.Success);
        }
        else if (outcome.TokenPenalty && outcome.TokenType != default(ConnectionType))
        {
            _connectionTokenManager.SpendTokens(outcome.TokenType, Math.Abs(outcome.TokenAmount), recipient.ID);
            _messageSystem.AddSystemMessage($"Lost {Math.Abs(outcome.TokenAmount)} {outcome.TokenType} token with {recipient.Name}", SystemMessageTypes.Warning);
        }

        // Handle special letter types - Endorsement letters removed from game

        // Process patron leverage
        if (outcome.ReducesLeverage)
        {
            _messageSystem.AddSystemMessage("Patron leverage reduced", SystemMessageTypes.Success);
        }

        // Track delivery
        player.DeliveredLetters.Add(letterToDeliver);
        player.TotalLettersDelivered++;

        // Remove letter from queue
        _letterQueueManager.RemoveLetterFromQueue(letterPosition);

        // Final message
        _messageSystem.AddSystemMessage($"Successfully delivered letter to {recipient.Name}!", SystemMessageTypes.Success);

        // Additional effects
        if (!string.IsNullOrEmpty(outcome.AdditionalEffect))
        {
            _messageSystem.AddSystemMessage(outcome.AdditionalEffect, SystemMessageTypes.Info);
        }

        return true;
    }

    private async Task<bool> ExecuteCollectLetter(CollectLetterIntent intent)
    {
        // Notice boards have been removed - letters only come from conversations
        _messageSystem.AddSystemMessage("Letters are now obtained through conversations with NPCs", SystemMessageTypes.Info);
        return false;
    }

    private async Task<bool> ExecuteObserve(ObserveLocationIntent intent)
    {
        Player player = _gameWorld.GetPlayer();
        LocationSpot currentSpot = player.CurrentLocationSpot;

        if (currentSpot == null)
        {
            _messageSystem.AddSystemMessage("Cannot determine current location", SystemMessageTypes.Danger);
            return false;
        }

        // Check time
        if (_timeManager.HoursRemaining < 1)
        {
            _messageSystem.AddSystemMessage("Not enough time to observe", SystemMessageTypes.Warning);
            return false;
        }

        // Spend time
        ProcessTimeAdvancement(1);

        // Build observation message
        List<string> messages = new List<string>();
        messages.Add($"You carefully observe {currentSpot.Name}.");

        // List NPCs
        List<NPC> npcsHere = _npcRepository.GetNPCsForLocationSpotAndTime(currentSpot.SpotID, _timeManager.GetCurrentTimeBlock());
        if (npcsHere.Any())
        {
            string npcNames = string.Join(", ", npcsHere.Select(n => n.Name));
            messages.Add($"People here: {npcNames}");
        }
        else
        {
            messages.Add("No one else is here right now.");
        }

        // Location properties
        if (!string.IsNullOrEmpty(currentSpot.Description))
        {
            messages.Add(currentSpot.Description);
        }

        // Display all messages
        foreach (string msg in messages)
        {
            _messageSystem.AddSystemMessage(msg, SystemMessageTypes.Info);
        }

        return true;
    }

    private async Task<bool> ExecuteExplore(ExploreAreaIntent intent)
    {
        Player player = _gameWorld.GetPlayer();
        LocationSpot currentLocation = player.CurrentLocationSpot;

        if (currentLocation == null)
        {
            _messageSystem.AddSystemMessage("You need to be at a location to explore", SystemMessageTypes.Warning);
            return false;
        }

        // Check stamina requirement (exploration is tiring)
        const int STAMINA_COST = 2;
        if (player.Stamina < STAMINA_COST)
        {
            _messageSystem.AddSystemMessage($"Not enough stamina to explore. Need {STAMINA_COST} stamina.", SystemMessageTypes.Warning);
            return false;
        }

        // Determine time cost based on location tier (2-4 hours)
        Location? location = _locationRepository.GetLocation(currentLocation.LocationId);
        int timeCost = location?.Tier switch
        {
            1 => 2,  // Small locations
            2 => 3,  // Medium locations  
            3 => 3,  // Medium locations
            4 => 4,  // Large locations
            5 => 4,  // Large locations
            _ => 3
        };

        // Spend resources
        player.Stamina -= STAMINA_COST;
        ProcessTimeAdvancement(timeCost);

        // Show exploration message
        _messageSystem.AddSystemMessage($"🔍 You spend {timeCost} hours exploring {location?.Name ?? "the area"}...", SystemMessageTypes.Info);

        // Check for route discoveries
        List<RouteDiscoveryOption> discoveries = _routeDiscoveryManager.GetAvailableDiscoveries(currentLocation.LocationId);
        List<RouteDiscoveryOption> undiscoveredRoutes = discoveries.Where(d => !d.Route.IsDiscovered).ToList();

        if (undiscoveredRoutes.Any())
        {
            // Randomly discover 1-2 routes
            List<RouteDiscoveryOption> toDiscover = undiscoveredRoutes.OrderBy(x => Guid.NewGuid()).Take(Math.Min(2, undiscoveredRoutes.Count)).ToList();

            foreach (RouteDiscoveryOption? discovery in toDiscover)
            {
                // Mark route as discovered but not necessarily accessible
                discovery.Route.IsDiscovered = true;

                _messageSystem.AddSystemMessage($"✨ Discovered route: {discovery.Route.Name}", SystemMessageTypes.Success);

                // Show requirements if any
                if (!discovery.MeetsRequirements.MeetsAllRequirements)
                {
                    if (!discovery.MeetsRequirements.HasEnoughTrust)
                    {
                        _messageSystem.AddSystemMessage($"   Requires trust with locals to access", SystemMessageTypes.Info);
                    }
                    if (!discovery.MeetsRequirements.HasRequiredEquipment)
                    {
                        _messageSystem.AddSystemMessage($"   Requires special equipment: {string.Join(", ", discovery.MeetsRequirements.MissingEquipment)}", SystemMessageTypes.Info);
                    }
                }
            }

            _messageSystem.AddSystemMessage($"Your exploration revealed {toDiscover.Count} new route{(toDiscover.Count > 1 ? "s" : "")}!", SystemMessageTypes.Success);
        }
        else
        {
            _messageSystem.AddSystemMessage("You thoroughly explore the area but find no new routes.", SystemMessageTypes.Info);
        }

        // Small chance to find items or information
        Random random = new Random();
        if (random.Next(100) < 20) // 20% chance
        {
            _messageSystem.AddSystemMessage("💡 You notice something interesting and make a mental note.", SystemMessageTypes.Info);
            player.AddMemory($"exploration_{currentLocation.LocationId}_{_gameWorld.CurrentDay}",
                           $"Found something interesting while exploring {location?.Name}",
                           _gameWorld.CurrentDay, 2);
        }

        return true;
    }

    private async Task<bool> ExecutePatronFunds(RequestPatronFundsIntent intent)
    {
        // Patron system has been completely removed
        _messageSystem.AddSystemMessage("The patron system no longer exists. Earn coins through deliveries and trade.", SystemMessageTypes.Info);
        return false;
    }

    private async Task<bool> ExecuteAcceptOffer(AcceptLetterOfferIntent intent)
    {
        // REMOVED - Letters are now ONLY created through conversation choices
        // This legacy automatic offer system violates our architectural principles
        _messageSystem.AddSystemMessage(
            "Letter offers are now handled through conversations. Talk to NPCs to request letters!",
            SystemMessageTypes.Info
        );
        return false;
    }

    private async Task<bool> ExecuteTravel(TravelIntent intent)
    {
        Player player = _gameWorld.GetPlayer();
        RouteOption route = _routeRepository.GetRouteById(intent.RouteId);

        if (route == null)
        {
            _messageSystem.AddSystemMessage("Route not found", SystemMessageTypes.Danger);
            return false;
        }

        // Check if route is discovered
        if (!route.IsDiscovered)
        {
            _messageSystem.AddSystemMessage("You haven't discovered this route yet", SystemMessageTypes.Warning);
            return false;
        }

        // Calculate costs
        int staminaCost = _travelManager.CalculateStaminaCost(route);
        int timeCost = route.GetActualTimeCost();

        // Check resources
        if (player.Stamina < staminaCost)
        {
            _messageSystem.AddSystemMessage($"Not enough stamina (need {staminaCost})", SystemMessageTypes.Warning);
            return false;
        }

        if (_timeManager.HoursRemaining < timeCost)
        {
            _messageSystem.AddSystemMessage($"Not enough time (need {timeCost} hours)", SystemMessageTypes.Warning);
            return false;
        }

        if (player.Coins < route.CoinCost)
        {
            _messageSystem.AddSystemMessage($"Not enough coins (need {route.CoinCost})", SystemMessageTypes.Warning);
            return false;
        }

        // Execute travel
        player.SpendStamina(staminaCost);
        player.SpendMoney(route.CoinCost);
        ProcessTimeAdvancement(timeCost);

        // Move to destination
        Location destination = _locationRepository.GetLocation(route.Destination);
        if (destination == null)
        {
            _messageSystem.AddSystemMessage($"Destination '{route.Destination}' not found", SystemMessageTypes.Danger);
            return false;
        }

        List<LocationSpot> destinationSpots = _locationRepository.GetSpotsForLocation(destination.Id);
        if (!destinationSpots.Any())
        {
            _messageSystem.AddSystemMessage($"No spots found at destination '{destination.Name}'", SystemMessageTypes.Danger);
            return false;
        }

        _locationRepository.SetCurrentLocation(destination, destinationSpots.First());
        _messageSystem.AddSystemMessage($"Traveled to {destination.Name}", SystemMessageTypes.Success);

        // Record the visit
        _locationRepository.RecordLocationVisit(destination.Id);

        return true;
    }

    private async Task<bool> ExecuteDiscoverRoute(DiscoverRouteIntent intent)
    {
        Player player = _gameWorld.GetPlayer();
        LocationSpot currentLocation = player.CurrentLocationSpot;

        if (currentLocation == null)
        {
            _messageSystem.AddSystemMessage("You need to be at a location to discover routes", SystemMessageTypes.Warning);
            return false;
        }

        // Get the NPC
        NPC npc = _npcRepository.GetById(intent.NpcId);
        if (npc == null || npc.Location != currentLocation.LocationId)
        {
            _messageSystem.AddSystemMessage("That person isn't here", SystemMessageTypes.Warning);
            return false;
        }

        // Get the route discovery option
        List<RouteDiscoveryOption> discoveries = _routeDiscoveryManager.GetDiscoveriesFromNPC(npc);
        RouteDiscoveryOption? discovery = discoveries.FirstOrDefault(d => d.Discovery.RouteId == intent.RouteId);

        if (discovery == null)
        {
            _messageSystem.AddSystemMessage($"{npc.Name} doesn't know about that route", SystemMessageTypes.Warning);
            return false;
        }

        // Check if already discovered
        if (discovery.Route.IsDiscovered)
        {
            _messageSystem.AddSystemMessage("You already know that route", SystemMessageTypes.Info);
            return false;
        }

        // Check requirements
        if (!discovery.MeetsRequirements.MeetsAllRequirements)
        {
            if (!discovery.MeetsRequirements.HasEnoughTrust)
            {
                _messageSystem.AddSystemMessage($"{npc.Name} doesn't trust you enough to share this route ({discovery.PlayerTokensWithNPC}/{discovery.Discovery.RequiredTokensWithNPC} tokens)", SystemMessageTypes.Warning);
            }
            if (!discovery.MeetsRequirements.HasRequiredEquipment)
            {
                _messageSystem.AddSystemMessage($"You need special equipment: {string.Join(", ", discovery.MeetsRequirements.MissingEquipment)}", SystemMessageTypes.Warning);
            }
            return false;
        }

        // Check if player can afford the token cost
        if (!discovery.CanAfford)
        {
            _messageSystem.AddSystemMessage($"You need {discovery.Discovery.RequiredTokensWithNPC} tokens with {npc.Name} to learn this route", SystemMessageTypes.Warning);
            return false;
        }

        // Determine token type and spend tokens
        ConnectionType tokenType = _routeDiscoveryManager.DetermineTokenTypeForRoute(discovery.Route, discovery.Discovery, npc);
        Dictionary<ConnectionType, int> tokensWithNpc = _connectionTokenManager.GetTokensWithNPC(npc.ID);

        // Spend tokens from the appropriate type (prefer the determined type)
        int tokensToSpend = discovery.Discovery.RequiredTokensWithNPC;
        bool spent = false;

        if (tokensWithNpc.ContainsKey(tokenType) && tokensWithNpc[tokenType] >= tokensToSpend)
        {
            spent = _connectionTokenManager.SpendTokens(tokenType, tokensToSpend, npc.ID);
        }
        else
        {
            // Try to spend from any available token type
            foreach (KeyValuePair<ConnectionType, int> kvp in tokensWithNpc.Where(t => t.Value >= tokensToSpend))
            {
                spent = _connectionTokenManager.SpendTokens(kvp.Key, tokensToSpend, npc.ID);
                if (spent) break;
            }
        }

        if (!spent)
        {
            _messageSystem.AddSystemMessage("Failed to spend tokens for route discovery", SystemMessageTypes.Danger);
            return false;
        }

        // Discover the route
        bool success = _routeDiscoveryManager.TryDiscoverRoute(intent.RouteId);

        if (success)
        {
            // Add narrative flavor
            _messageSystem.AddSystemMessage($"💬 {npc.Name} shares their knowledge with you...", SystemMessageTypes.Info);

            // NPC-specific dialogue
            RouteDiscoveryContext? routeContext = discovery.Discovery.DiscoveryContexts.GetValueOrDefault(npc.ID);
            if (routeContext != null && !string.IsNullOrEmpty(routeContext.Narrative))
            {
                _messageSystem.AddSystemMessage($"\"{routeContext.Narrative}\"", SystemMessageTypes.Info);
            }
            else
            {
                // Generic discovery text
                _messageSystem.AddSystemMessage($"\"{discovery.Route.Name}? Yes, I know that route well. Let me tell you how to navigate it safely...\"", SystemMessageTypes.Info);
            }

            // Time passes during conversation
            ProcessTimeAdvancement(1);
        }

        return success;
    }

    // Seal conversion system completely removed - endorsements and seals no longer exist

    // ========== TRAVEL ==========

    private TravelViewModel GetTravelViewModel()
    {
        Player player = _gameWorld.GetPlayer();
        Location currentLocation = player.GetCurrentLocation(_locationRepository);

        TravelViewModel viewModel = new TravelViewModel
        {
            CurrentLocationId = currentLocation.Id,
            CurrentLocationName = currentLocation.Name,
            Status = GetTravelStatus(player),
            Destinations = GetDestinations(currentLocation)
        };

        return viewModel;
    }

    private TravelStatusViewModel GetTravelStatus(Player player)
    {
        int totalWeight = CalculateTotalWeight();
        string weightClass = totalWeight <= GameConstants.LoadWeight.LIGHT_LOAD_MAX ? "" : (totalWeight <= GameConstants.LoadWeight.MEDIUM_LOAD_MAX ? "warning" : "danger");
        string weightStatus = totalWeight <= GameConstants.LoadWeight.LIGHT_LOAD_MAX ? "Normal load" :
                          (totalWeight <= GameConstants.LoadWeight.MEDIUM_LOAD_MAX ? "Medium load (+1 stamina)" : "Heavy load (+2 stamina)");
        int baseStaminaCost = totalWeight <= GameConstants.LoadWeight.LIGHT_LOAD_MAX ? GameConstants.LoadWeight.LIGHT_LOAD_STAMINA_PENALTY :
                             (totalWeight <= GameConstants.LoadWeight.MEDIUM_LOAD_MAX ? GameConstants.LoadWeight.MEDIUM_LOAD_STAMINA_PENALTY : GameConstants.LoadWeight.HEAVY_LOAD_STAMINA_PENALTY);

        List<Letter> carriedLetters = player.CarriedLetters ?? new List<Letter>();
        bool hasHeavyLetters = carriedLetters.Any(l => l.PhysicalProperties.HasFlag(LetterPhysicalProperties.Heavy));
        bool hasFragileLetters = carriedLetters.Any(l => l.PhysicalProperties.HasFlag(LetterPhysicalProperties.Fragile));
        bool hasValuableLetters = carriedLetters.Any(l => l.PhysicalProperties.HasFlag(LetterPhysicalProperties.Valuable));

        List<string> warnings = new List<string>();
        if (hasHeavyLetters) warnings.Add("Heavy letters (+1 stamina on all routes)");
        if (hasFragileLetters) warnings.Add("Fragile letters (avoid rough terrain)");
        if (hasValuableLetters) warnings.Add("Valuable letters (beware of thieves)");

        return new TravelStatusViewModel
        {
            TotalWeight = totalWeight,
            WeightClass = weightClass,
            WeightStatus = weightStatus,
            BaseStaminaCost = baseStaminaCost,
            CurrentStamina = player.Stamina,
            CurrentEquipment = GetEquipmentCategories(player),
            CarriedLetterCount = carriedLetters.Count,
            HasHeavyLetters = hasHeavyLetters,
            HasFragileLetters = hasFragileLetters,
            HasValuableLetters = hasValuableLetters,
            LetterWarnings = warnings
        };
    }

    private List<DestinationViewModel> GetDestinations(Location currentLocation)
    {
        List<DestinationViewModel> destinations = new List<DestinationViewModel>();
        List<Location> allLocations = _locationRepository.GetAllLocations();

        foreach (Location location in allLocations)
        {
            List<RouteOption> availableRoutes = _travelManager.GetAvailableRoutes(currentLocation.Id, location.Id);
            // Get all routes from connections
            LocationConnection? connection = currentLocation.Connections?.FirstOrDefault(c => c.DestinationLocationId == location.Id);
            List<RouteOption> allRoutes = connection?.RouteOptions ?? new List<RouteOption>();
            List<RouteOption> lockedRoutes = allRoutes.Where(r => !r.IsDiscovered).ToList();

            if (!availableRoutes.Any() && !lockedRoutes.Any())
                continue;

            DestinationViewModel destination = new DestinationViewModel
            {
                LocationId = location.Id,
                LocationName = location.Name,
                IsCurrent = location.Id == currentLocation.Id,
                AvailableRoutes = ConvertRoutes(availableRoutes),
                LockedRoutes = ConvertLockedRoutes(lockedRoutes, currentLocation.Id)
            };

            destinations.Add(destination);
        }

        return destinations.OrderBy(d => d.IsCurrent ? 0 : 1).ThenBy(d => d.LocationName).ToList();
    }

    private List<RouteViewModel> ConvertRoutes(List<RouteOption> routes)
    {
        Player player = _gameWorld.GetPlayer();
        List<Letter> carriedLetters = player.CarriedLetters ?? new List<Letter>();
        bool hasHeavyLetters = carriedLetters.Any(l => l.PhysicalProperties.HasFlag(LetterPhysicalProperties.Heavy));

        return routes.Select(route =>
        {
            int coinCost = _travelManager.CalculateCoinCost(route);
            int routeStaminaCost = _travelManager.CalculateStaminaCost(route);
            int letterStaminaPenalty = hasHeavyLetters ? 1 : 0;
            int totalStaminaCost = routeStaminaCost + letterStaminaPenalty;
            RouteAccessResult accessInfo = _travelManager.GetRouteAccessInfo(route);

            return new RouteViewModel
            {
                RouteId = route.Id,
                TerrainType = "Standard", // Terrain not needed for POC
                CoinCost = coinCost,
                StaminaCost = routeStaminaCost,
                TravelTimeMinutes = route.TravelTimeMinutes,
                TransportRequirement = route.Method.ToString(),
                CanAffordCoins = player.Coins >= coinCost,
                CanAffordStamina = player.Stamina >= totalStaminaCost,
                IsBlocked = !accessInfo.IsAllowed,
                BlockedReason = accessInfo.BlockingReason,
                LetterStaminaPenalty = letterStaminaPenalty,
                TotalStaminaCost = totalStaminaCost
            };
        }).ToList();
    }

    private List<LockedRouteViewModel> ConvertLockedRoutes(List<RouteOption> routes, string currentLocationId)
    {
        return routes.Select(route =>
        {
            List<RouteDiscoveryOption> discoveries = _routeDiscoveryManager.GetAvailableDiscoveries(currentLocationId)
                .Where(d => d.Route.Id == route.Id)
                .ToList();

            return new LockedRouteViewModel
            {
                RouteId = route.Id,
                TerrainType = "Standard", // Terrain not needed for POC
                DiscoveryOptions = ConvertDiscoveryOptions(discoveries)
            };
        }).ToList();
    }

    private List<RouteDiscoveryViewModel> ConvertDiscoveryOptions(List<RouteDiscoveryOption> discoveries)
    {
        Player player = _gameWorld.GetPlayer();

        return discoveries.Select(discoveryOption =>
        {
            RouteDiscovery discovery = discoveryOption.Discovery;
            NPC teachingNPC = discoveryOption.TeachingNPC;

            // Check for equipment requirements first
            string requiredEquipment = null;
            bool hasRequiredEquipment = true;
            string description = $"Learn from {teachingNPC?.Name ?? "Unknown"}";

            if (discovery.DiscoveryContexts.ContainsKey(teachingNPC.ID))
            {
                RouteDiscoveryContext context = discovery.DiscoveryContexts[teachingNPC.ID];
                if (context.RequiredEquipment?.Any() == true)
                {
                    requiredEquipment = string.Join(", ", context.RequiredEquipment);
                    hasRequiredEquipment = context.RequiredEquipment.All(item => player.Inventory.HasItem(item));
                    description += $" (requires {requiredEquipment})";
                }
            }

            // All discoveries in the new system are through NPC relationships and tokens
            DiscoveryMethodViewModel method = new DiscoveryMethodViewModel
            {
                MethodType = "NPC Teaching",
                Description = description,
                NPCName = teachingNPC?.Name ?? "Unknown",
                TokenType = "Total Tokens",
                TokenCost = discovery.RequiredTokensWithNPC,
                AvailableTokens = discoveryOption.PlayerTokensWithNPC,
                RequiredItem = requiredEquipment,
                HasItem = hasRequiredEquipment
            };

            return new RouteDiscoveryViewModel
            {
                DiscoveryId = discovery.RouteId, // Use RouteId as the discovery identifier
                Method = method,
                CanAfford = discoveryOption.CanAfford
            };
        }).ToList();
    }

    private List<string> GetEquipmentCategories(Player player)
    {
        List<string> categories = new List<string>();

        foreach (string? itemName in player.Inventory.ItemSlots.Where(s => !string.IsNullOrEmpty(s)))
        {
            // This would ideally come from item repository
            // For now, return generic categories
            if (itemName.Contains("torch", StringComparison.OrdinalIgnoreCase))
                categories.Add("Light_Equipment");
            else if (itemName.Contains("rope", StringComparison.OrdinalIgnoreCase))
                categories.Add("Climbing_Equipment");
        }

        return categories.Distinct().ToList();
    }

    public List<TravelDestinationViewModel> GetTravelDestinations()
    {
        TravelViewModel travelViewModel = GetTravelViewModel();
        List<TravelDestinationViewModel> destinations = new List<TravelDestinationViewModel>();

        foreach (DestinationViewModel dest in travelViewModel.Destinations)
        {
            Location location = _locationRepository.GetLocation(dest.LocationId);
            bool canTravel = dest.AvailableRoutes.Any(r => !r.IsBlocked);

            destinations.Add(new TravelDestinationViewModel
            {
                LocationId = dest.LocationId,
                LocationName = dest.LocationName,
                Description = location?.Description ?? "",
                CanTravel = canTravel,
                CannotTravelReason = !canTravel ? "No available routes" : null,
                MinimumCost = dest.AvailableRoutes.Where(r => !r.IsBlocked).Select(r => r.CoinCost).DefaultIfEmpty(0).Min(),
                MinimumTime = dest.AvailableRoutes.Where(r => !r.IsBlocked).Select(r => r.TravelTimeMinutes).DefaultIfEmpty(0).Min()
            });
        }

        return destinations;
    }

    public List<TravelRouteViewModel> GetRoutesToDestination(string destinationId)
    {
        TravelViewModel travelViewModel = GetTravelViewModel();
        DestinationViewModel? destination = travelViewModel.Destinations.FirstOrDefault(d => d.LocationId == destinationId);

        if (destination == null)
            return new List<TravelRouteViewModel>();

        List<TravelRouteViewModel> routes = new List<TravelRouteViewModel>();

        foreach (RouteViewModel route in destination.AvailableRoutes)
        {
            routes.Add(new TravelRouteViewModel
            {
                RouteId = route.RouteId,
                RouteName = route.TerrainType,
                Description = route.TransportRequirement ?? "Standard route",
                TransportMethod = route.TransportRequirement == "Carriage" ? TravelMethods.Carriage : TravelMethods.Walking,
                TimeCost = route.TravelTimeMinutes,
                TotalStaminaCost = route.TotalStaminaCost,
                CoinCost = route.CoinCost,
                CanTravel = !route.IsBlocked,
                CannotTravelReason = route.BlockedReason
            });
        }

        return routes;
    }

    public async Task<bool> ExecuteWaitAction()
    {
        // Create and execute a wait intent
        WaitIntent intent = new WaitIntent();
        return await ExecuteIntent(intent);
    }

    public async Task<bool> ExecuteRestAction(string actionType, string cost)
    {
        // Parse cost to determine payment required (e.g., "2c" = 2 coins, "FREE" = 0)
        int coinCost = 0;
        if (!string.IsNullOrEmpty(cost) && cost != "FREE")
        {
            string numericPart = System.Text.RegularExpressions.Regex.Match(cost, @"\d+").Value;
            if (int.TryParse(numericPart, out int parsed))
            {
                coinCost = parsed;
            }
        }

        // Check if player can afford it
        Player player = _gameWorld.GetPlayer();
        if (coinCost > 0 && player.Coins < coinCost)
        {
            _messageSystem.AddSystemMessage($"Not enough coins (need {coinCost})", SystemMessageTypes.Warning);
            return false;
        }

        // Deduct cost
        if (coinCost > 0)
        {
            player.ModifyCoins(-coinCost);
            _messageSystem.AddSystemMessage($"Paid {coinCost} coins for rest", SystemMessageTypes.Info);
        }

        // For now, basic rest is 1 hour (will be extended for inn rooms)
        RestIntent intent = new RestIntent(1);
        return await ExecuteIntent(intent);
    }

    public async Task<bool> TravelToDestinationAsync(string destinationId, string routeId)
    {
        // Find the route
        RouteOption route = _routeRepository.GetRouteById(routeId);
        if (route == null) return false;

        RouteOption routeOption = new RouteOption
        {
            Id = routeId,
            Method = route.Method
        };

        TravelIntent intent = new TravelIntent(routeId);
        return await ExecuteIntent(intent);
    }

    public async Task<bool> TravelAsync(string routeId)
    {
        // Use the proper ExecuteIntent flow to ensure time advancement is handled correctly
        TravelIntent intent = new TravelIntent(routeId);
        return await ExecuteIntent(intent);
    }

    public async Task<bool> UnlockRouteAsync(string discoveryId)
    {
        List<RouteDiscoveryOption> discoveries = _routeDiscoveryManager.GetAvailableDiscoveries(_gameWorld.GetPlayer().CurrentLocationSpot?.LocationId);
        RouteDiscoveryOption? discovery = discoveries.FirstOrDefault(d => d.Discovery.RouteId == discoveryId);

        if (discovery == null) return false;

        // Execute discovery directly
        bool success = _routeDiscoveryManager.TryDiscoverRoute(discovery.Discovery.RouteId);
        if (success)
        {
            _messageSystem.AddSystemMessage($"Discovered route: {discovery.Discovery.RouteId}", SystemMessageTypes.Success);
        }
        return success;
    }

    public TravelContextViewModel GetTravelContext()
    {
        Player player = _gameWorld.GetPlayer();
        int totalWeight = CalculateTotalWeight();
        List<Letter>? carriedLetters = player.CarriedLetters;

        // Calculate weight status
        string weightStatus;
        string weightClass;
        int baseStaminaPenalty;

        if (totalWeight <= GameConstants.LoadWeight.LIGHT_LOAD_MAX)
        {
            weightStatus = "Light load";
            weightClass = "";
            baseStaminaPenalty = 0;
        }
        else if (totalWeight <= GameConstants.LoadWeight.MEDIUM_LOAD_MAX)
        {
            weightStatus = "Medium load (+1 stamina)";
            weightClass = "warning";
            baseStaminaPenalty = GameConstants.LoadWeight.MEDIUM_LOAD_STAMINA_PENALTY;
        }
        else
        {
            weightStatus = "Heavy load (+2 stamina)";
            weightClass = "danger";
            baseStaminaPenalty = GameConstants.LoadWeight.HEAVY_LOAD_STAMINA_PENALTY;
        }

        // Check letter properties
        bool hasHeavyLetters = carriedLetters?.Any(l => l.PhysicalProperties.HasFlag(LetterPhysicalProperties.Heavy)) ?? false;
        bool hasFragileLetters = carriedLetters?.Any(l => l.PhysicalProperties.HasFlag(LetterPhysicalProperties.Fragile)) ?? false;
        bool hasBulkyLetters = carriedLetters?.Any(l => l.PhysicalProperties.HasFlag(LetterPhysicalProperties.Bulky)) ?? false;
        bool hasPerishableLetters = carriedLetters?.Any(l => l.PhysicalProperties.HasFlag(LetterPhysicalProperties.Perishable)) ?? false;

        // Determine letter warning
        string letterWarning = "";
        if (hasHeavyLetters)
            letterWarning = "Heavy letters +1 stamina cost";
        else if (hasBulkyLetters)
            letterWarning = "Bulky letters restrict movement";
        else if (hasPerishableLetters)
            letterWarning = "Perishable letters - time sensitive";
        else if (hasFragileLetters)
            letterWarning = "Fragile letters need careful handling";

        // Get equipment categories
        List<ItemCategory> equipmentCategories = new List<ItemCategory>();
        foreach (string itemName in player.Inventory.ItemSlots)
        {
            if (!string.IsNullOrEmpty(itemName))
            {
                Item item = _itemRepository.GetItemByName(itemName);
                if (item != null)
                {
                    equipmentCategories.AddRange(item.Categories);
                }
            }
        }

        return new TravelContextViewModel
        {
            CurrentStamina = player.Stamina,
            TotalWeight = totalWeight,
            WeightStatus = weightStatus,
            WeightClass = weightClass,
            BaseStaminaPenalty = baseStaminaPenalty,
            CarriedLetterCount = carriedLetters?.Count ?? 0,
            HasHeavyLetters = hasHeavyLetters,
            HasFragileLetters = hasFragileLetters,
            HasBulkyLetters = hasBulkyLetters,
            HasPerishableLetters = hasPerishableLetters,
            LetterWarning = letterWarning,
            CurrentEquipmentCategories = equipmentCategories.Distinct().ToList(),
            CurrentWeather = _gameWorld.CurrentWeather,
            WeatherIcon = GetWeatherIcon(_gameWorld.CurrentWeather)
        };
    }

    public List<TravelDestinationViewModel> GetTravelDestinationsWithRoutes()
    {
        Location currentLocation = _locationRepository.GetCurrentLocation();
        List<TravelDestinationViewModel> destinations = new List<TravelDestinationViewModel>();
        TravelContextViewModel travelContext = GetTravelContext();

        // Get all locations that can be traveled to
        List<Location> allLocations = _locationRepository.GetAllLocations();

        foreach (Location location in allLocations)
        {
            if (location.Id == currentLocation.Id) continue;

            // Get all routes to this destination
            List<RouteOption> allRoutes = _routeRepository.GetRoutesFromLocation(currentLocation.Id)
                .Where(r => r.Destination == location.Id)
                .ToList();

            if (!allRoutes.Any()) continue;

            List<TravelRouteViewModel> routeViewModels = new List<TravelRouteViewModel>();

            foreach (RouteOption? route in allRoutes)
            {
                routeViewModels.Add(CreateTravelRouteViewModel(route, travelContext));
            }

            bool canTravel = routeViewModels.Any(r => r.CanTravel);

            destinations.Add(new TravelDestinationViewModel
            {
                LocationId = location.Id,
                LocationName = location.Name,
                Description = location.Description,
                CanTravel = canTravel,
                CannotTravelReason = !canTravel ? "No available routes" : null,
                MinimumCost = routeViewModels.Where(r => r.CanTravel).Select(r => r.CoinCost).DefaultIfEmpty(0).Min(),
                MinimumTime = routeViewModels.Where(r => r.CanTravel).Select(r => r.TimeCost).DefaultIfEmpty(0).Min(),
                IsCurrent = false,
                Routes = routeViewModels
            });
        }

        return destinations;
    }

    private TravelRouteViewModel CreateTravelRouteViewModel(RouteOption route, TravelContextViewModel travelContext)
    {
        Player player = _gameWorld.GetPlayer();

        // Calculate costs
        int coinCost = _travelManager.CalculateCoinCost(route);
        int baseStaminaCost = _travelManager.CalculateStaminaCost(route);
        int letterStaminaPenalty = travelContext.HasHeavyLetters ? 1 : 0;
        int totalStaminaCost = baseStaminaCost + letterStaminaPenalty;

        // Check if can travel
        bool canAffordStamina = player.Stamina >= totalStaminaCost;
        bool canAffordCoins = player.Coins >= coinCost;
        RouteAccessResult accessInfo = _travelManager.GetRouteAccessInfo(route);
        AccessCheckResult tokenAccessInfo = _travelManager.GetTokenAccessInfo(route);

        bool canTravel = canAffordStamina && canAffordCoins && accessInfo.IsAllowed && tokenAccessInfo.IsAllowed;

        // Build token requirements
        Dictionary<string, RouteTokenRequirementViewModel> tokenRequirements = new Dictionary<string, RouteTokenRequirementViewModel>();

        if (route.AccessRequirement != null)
        {
            // Type-based requirements
            foreach (TokenTypeRequirement typeReq in route.AccessRequirement.RequiredTokensPerType)
            {
                int currentCount = _connectionTokenManager.GetTotalTokensOfType(typeReq.TokenType);
                string key = $"type_{typeReq.TokenType}";
                tokenRequirements[key] = new RouteTokenRequirementViewModel
                {
                    RequirementKey = key,
                    RequiredAmount = typeReq.MinimumCount,
                    CurrentAmount = currentCount,
                    DisplayName = $"{typeReq.TokenType} tokens (any NPC)",
                    Icon = GetTokenIcon(typeReq.TokenType),
                    IsMet = currentCount >= typeReq.MinimumCount
                };
            }

            // NPC-specific requirements
            foreach (TokenRequirement tokenReq in route.AccessRequirement.RequiredTokensPerNPC)
            {
                Dictionary<ConnectionType, int> npcTokens = _connectionTokenManager.GetTokensWithNPC(tokenReq.NPCId);
                int currentCount = npcTokens.Values.Sum();
                NPC npc = _npcRepository.GetById(tokenReq.NPCId);
                string key = $"npc_{tokenReq.NPCId}";
                tokenRequirements[key] = new RouteTokenRequirementViewModel
                {
                    RequirementKey = key,
                    RequiredAmount = tokenReq.MinimumCount,
                    CurrentAmount = currentCount,
                    DisplayName = $"tokens with {npc?.Name ?? tokenReq.NPCId}",
                    Icon = "🎭",
                    IsMet = currentCount >= tokenReq.MinimumCount
                };
            }
        }

        // Build warnings
        List<string> warnings = new List<string>(accessInfo.Warnings);

        // Add fragile letter warnings
        if (travelContext.HasFragileLetters &&
            (route.TerrainCategories.Contains(TerrainCategory.Requires_Climbing) ||
             route.TerrainCategories.Contains(TerrainCategory.Wilderness_Terrain)))
        {
            warnings.Insert(0, "Fragile letters at risk on this route!");
        }

        // Get discovery options if route is locked
        List<RouteDiscoveryOptionViewModel> discoveryOptions = new List<RouteDiscoveryOptionViewModel>();
        if (!route.IsDiscovered)
        {
            // Note: RouteDiscoveryManager would need to be injected for this to work fully
            // For now, leaving empty as it would require more refactoring
        }

        // Determine cannot travel reason
        string cannotTravelReason = null;
        if (!canTravel)
        {
            if (!accessInfo.IsAllowed)
                cannotTravelReason = accessInfo.BlockingReason;
            else if (!tokenAccessInfo.IsAllowed)
                cannotTravelReason = tokenAccessInfo.BlockedMessage;
            else if (!canAffordCoins)
                cannotTravelReason = "Not enough coins";
            else if (!canAffordStamina)
                cannotTravelReason = "Not enough stamina";
        }

        return new TravelRouteViewModel
        {
            RouteId = route.Id,
            RouteName = route.Name,
            Description = route.Description,
            TransportMethod = route.Method,
            TimeCost = route.TravelTimeMinutes,
            BaseStaminaCost = route.BaseStaminaCost,
            TotalStaminaCost = totalStaminaCost,
            CoinCost = coinCost,
            CanTravel = canTravel,
            CannotTravelReason = cannotTravelReason,
            IsDiscovered = route.IsDiscovered,
            IsBlocked = !accessInfo.IsAllowed || !tokenAccessInfo.IsAllowed,
            BlockingReason = !tokenAccessInfo.IsAllowed ? tokenAccessInfo.BlockedMessage : accessInfo.BlockingReason,
            HintMessage = tokenAccessInfo.HintMessage,
            Warnings = warnings,
            TerrainCategories = route.TerrainCategories,
            DepartureTime = route.DepartureTime,
            TokenRequirements = tokenRequirements,
            DiscoveryOptions = discoveryOptions
        };
    }


    private string GetWeatherIcon(WeatherCondition weather)
    {
        return weather switch
        {
            WeatherCondition.Clear => "☀️",
            WeatherCondition.Rain => "🌧️",
            WeatherCondition.Snow => "❄️",
            WeatherCondition.Fog => "🌫️",
            _ => "❓"
        };
    }

    private string GetTokenIcon(ConnectionType tokenType)
    {
        return tokenType switch
        {
            ConnectionType.Trust => "💝",
            ConnectionType.Commerce => "🤝",
            ConnectionType.Status => "👑",
            ConnectionType.Shadow => "🌑",
            _ => "🎭"
        };
    }

    // ========== REST ==========

    public RestOptionsViewModel GetRestOptions()
    {
        return new RestOptionsViewModel
        {
            RestOptions = GetRestOptionsList(),
            LocationActions = GetRestLocationActions(),
            WaitOptions = GetWaitOptionsList()
        };
    }

    public async Task<bool> ExecuteRestAsync(string restOptionId)
    {
        if (_restManager == null)
        {
            _messageSystem.AddSystemMessage("Rest manager not available", SystemMessageTypes.Danger);
            return false;
        }

        RestOption? option = _restManager.GetAvailableRestOptions().FirstOrDefault(o => o.Id == restOptionId);
        if (option == null)
        {
            _messageSystem.AddSystemMessage("Rest option not found", SystemMessageTypes.Danger);
            return false;
        }

        // Apply costs if any
        if (option.CoinCost > 0)
        {
            Player player = _gameWorld.GetPlayer();
            if (player.Coins < option.CoinCost)
            {
                return false;
            }
            player.SpendMoney(option.CoinCost);
            _messageSystem.AddSystemMessage($"Spent {option.CoinCost} coins for rest at {option.Name}", SystemMessageTypes.Info);
        }

        // Execute rest directly
        _restManager.Rest(option);
        return true;
    }

    private List<RestOptionViewModel> GetRestOptionsList()
    {
        if (_restManager == null)
            return new List<RestOptionViewModel>();

        Player player = _gameWorld.GetPlayer();
        List<RestOption> restOptions = _restManager.GetAvailableRestOptions()
            .Where(o => o.StaminaRecovery > 0 && o.RestTimeHours >= 1)
            .ToList();

        return restOptions.Select(option =>
        {
            bool hasRequiredItem = string.IsNullOrEmpty(option.RequiredItem) ||
                                 player.Inventory.HasItem(option.RequiredItem);
            bool canAffordCoins = player.Coins >= option.CoinCost;
            bool isAvailable = canAffordCoins && hasRequiredItem;

            string unavailableReason = null;
            if (!canAffordCoins)
                unavailableReason = $"Need {option.CoinCost} coins";
            else if (!hasRequiredItem)
                unavailableReason = $"Need {option.RequiredItem}";

            return new RestOptionViewModel
            {
                Id = option.Id,
                Name = option.Name,
                Description = option.Name, // RestOption doesn't have Description, use Name
                StaminaRecovery = option.StaminaRecovery,
                CoinCost = option.CoinCost,
                TimeHours = option.RestTimeHours,
                RequiredItem = option.RequiredItem,
                CanAffordCoins = canAffordCoins,
                HasRequiredItem = hasRequiredItem,
                IsAvailable = isAvailable,
                UnavailableReason = unavailableReason
            };
        }).ToList();
    }

    private List<RestLocationActionViewModel> GetRestLocationActions()
    {
        Player player = _gameWorld.GetPlayer();
        List<RestLocationActionViewModel> actions = new List<RestLocationActionViewModel>();

        // Add rest options
        (int, int)[] restOptions = new[] { (1, 2), (2, 4), (4, 10) };

        foreach ((int hours, int stamina) in restOptions)
        {
            actions.Add(new RestLocationActionViewModel
            {
                Id = $"rest_{hours}",
                Description = $"Rest for {hours} hour{(hours > 1 ? "s" : "")}",
                NPCName = null,
                NPCProfession = null,
                TimeCost = hours,
                StaminaCost = 0,
                CoinCost = 0,
                StaminaReward = stamina,
                IsAvailable = _timeManager.HoursRemaining >= hours && player.Stamina < player.MaxStamina,
                UnavailableReason = _timeManager.HoursRemaining < hours ? "Not enough time" :
                                   player.Stamina >= player.MaxStamina ? "Already at max stamina" : null,
                CanBeRemedied = false,
                RemediationHint = null
            });
        }

        return actions;
    }

    private List<WaitOptionViewModel> GetWaitOptionsList()
    {
        // Simple wait options - could be expanded
        return new List<WaitOptionViewModel>
        {
            new WaitOptionViewModel { Hours = 1, Description = "Wait 1 hour" },
            new WaitOptionViewModel { Hours = GameConstants.UI.WAIT_OPTION_SHORT_HOURS, Description = $"Wait {GameConstants.UI.WAIT_OPTION_SHORT_HOURS} hours" },
            new WaitOptionViewModel { Hours = GameConstants.UI.WAIT_OPTION_LONG_HOURS, Description = $"Wait {GameConstants.UI.WAIT_OPTION_LONG_HOURS} hours" }
        };
    }

    private int ExtractStaminaReward(string rewardDescription)
    {
        if (string.IsNullOrEmpty(rewardDescription))
            return 0;

        // Simple extraction from "+X stamina" format
        if (rewardDescription.Contains("stamina"))
        {
            string[] parts = rewardDescription.Split(' ');
            for (int i = 0; i < parts.Length - 1; i++)
            {
                if (parts[i].StartsWith("+") && parts[i + 1].StartsWith("stamina"))
                {
                    if (int.TryParse(parts[i].Substring(1), out int stamina))
                    {
                        return stamina;
                    }
                }
            }
        }
        return 0;
    }

    // ========== CONVERSATIONS ==========


    public async Task<ConversationViewModel> StartConversationAsync(string npcId)
    {
        // New card-based conversation system
        // Conversations are now handled directly by ConversationScreen component
        // This method validates the NPC exists and creates the view model
        
        NPC npc = _npcRepository.GetById(npcId);
        if (npc == null) 
        {
            Console.WriteLine($"[GameFacade] NPC {npcId} not found in repository");
            return null;
        }
        
        Player player = _gameWorld.GetPlayer();
        Location location = _locationRepository.GetCurrentLocation();
        
        // Check if NPC exists in game world
        var worldNpc = _gameWorld.NPCs.FirstOrDefault(n => n.ID == npcId);
        if (worldNpc == null)
        {
            Console.WriteLine($"[GameFacade] NPC {npcId} not found in GameWorld.NPCs");
            return null;
        }
            
        // Debug logging
        Console.WriteLine($"[GameFacade] Player.CurrentLocationSpot: {player.CurrentLocationSpot?.SpotID ?? "null"}");
        Console.WriteLine($"[GameFacade] NPC.SpotId: {worldNpc.SpotId}");
        
        // Check if NPC is at current spot
        if (player.CurrentLocationSpot == null || worldNpc.SpotId != player.CurrentLocationSpot.SpotID)
        {
            Console.WriteLine($"[GameFacade] NPC spot mismatch - conversation blocked");
            return null;
        }
            
        // Create conversation view model
        var viewModel = new ConversationViewModel
        {
            NpcName = npc.Name,
            NpcId = npc.ID,
            LocationName = location.Name,
            CurrentAttention = _timeBlockAttentionManager.GetAttentionState().current,
            MaxAttention = _timeBlockAttentionManager.GetAttentionState().max,
            CurrentTime = _timeManager.GetFormattedTimeDisplay(),
            QueueStatus = $"{player.ObligationQueue.Count(o => o != null)}/8",
            CoinStatus = $"{player.Coins}s"
        };
        
        return viewModel;
    }

    // Card-based conversation system integration
    
    /// <summary>
    /// Get current conversation session
    /// </summary>
    public ConversationSession GetCurrentConversation()
    {
        return _conversationManager.CurrentSession;
    }
    
    /// <summary>
    /// Execute LISTEN action in current conversation
    /// </summary>
    public void ExecuteConversationListen()
    {
        if (!_conversationManager.IsConversationActive)
        {
            throw new InvalidOperationException("No active conversation");
        }
        
        _conversationManager.ExecuteListen();
    }
    
    /// <summary>
    /// Execute SPEAK action with selected cards
    /// </summary>
    public CardPlayResult ExecuteConversationSpeak(HashSet<ConversationCard> selectedCards)
    {
        if (!_conversationManager.IsConversationActive)
        {
            throw new InvalidOperationException("No active conversation");
        }
        
        return _conversationManager.ExecuteSpeak(selectedCards);
    }
    
    /// <summary>
    /// End the current conversation
    /// </summary>
    public void EndConversation()
    {
        _conversationManager.EndConversation();
    }

    // ========== LETTER QUEUE ==========

    public LetterQueueViewModel GetLetterQueue()
    {
        Player player = _gameWorld.GetPlayer();

        LetterQueueViewModel viewModel = new LetterQueueViewModel
        {
            CurrentTimeBlock = _timeManager.GetCurrentTimeBlock(),
            CurrentDay = _gameWorld.CurrentDay,
            LastMorningSwapDay = player.LastMorningSwapDay,
            QueueSlots = new List<QueueSlotViewModel>(),
            Status = new QueueStatusViewModel
            {
                LetterCount = player.ObligationQueue.Count(o => o != null),
                MaxCapacity = 8,
                ExpiredCount = player.ObligationQueue.Where(o => o != null && o.DeadlineInMinutes <= 0).Count(),
                UrgentCount = player.ObligationQueue.Where(o => o != null && o.DeadlineInMinutes > 0 && o.DeadlineInMinutes <= 24).Count(),
                WarningCount = player.ObligationQueue.Where(o => o != null && o.DeadlineInMinutes > 24 && o.DeadlineInMinutes <= 48).Count(),
                TotalSize = player.ObligationQueue.Where(o => o != null).Count(), // Each obligation takes 1 slot
                MaxSize = 12, // Satchel size capacity
                RemainingSize = 12 - player.ObligationQueue.Where(o => o != null).Count(),
                SizeDisplay = $"{player.ObligationQueue.Where(o => o != null).Count()}/12"
            },
            Actions = new QueueActionsViewModel
            {
                CanMorningSwap = _timeManager.GetCurrentTimeBlock() == TimeBlocks.Morning && player.LastMorningSwapDay < _gameWorld.CurrentDay,
                MorningSwapReason = _timeManager.GetCurrentTimeBlock() != TimeBlocks.Morning ? "Only available in morning" : 
                                   player.LastMorningSwapDay >= _gameWorld.CurrentDay ? "Already swapped today" : 
                                   "Available",
                HasBottomDeliveryObligation = player.ObligationQueue[7] != null,
                TotalAvailableTokens = 0, // Would need to calculate total tokens across all NPCs
                PurgeTokenOptions = new List<TokenOptionViewModel>()
            }
        };

        // Build queue slots
        for (int position = 1; position <= 8; position++)
        {
            DeliveryObligation? obligation = _letterQueueManager.GetLetterAt(position);
            bool canSkip = position > 1 && obligation != null && _letterQueueManager.GetLetterAt(1) == null;

            // Calculate skip action details
            SkipActionViewModel? skipAction = null;
            if (canSkip)
            {
                int baseCost = position - 1;
                int multiplier = _standingObligationManager.CalculateSkipCostMultiplier(obligation);
                int tokenCost = baseCost * multiplier;
                int availableTokens = _connectionTokenManager.GetTokenCount(obligation.TokenType);

                // Build detailed multiplier reason
                string multiplierReason = null;
                if (multiplier > 1)
                {
                    List<StandingObligation> activeObligations = _standingObligationManager.GetActiveObligations()
                        .Where(o => o.HasEffect(ObligationEffect.TrustSkipDoubleCost) && o.AppliesTo(obligation.TokenType))
                        .ToList();

                    if (activeObligations.Any())
                    {
                        IEnumerable<string> obligationNames = activeObligations.Select(o => o.Name);
                        multiplierReason = $"×{multiplier} from: {string.Join(", ", obligationNames)}";
                    }
                    else
                    {
                        multiplierReason = $"×{multiplier} from active obligations";
                    }
                }

                skipAction = new SkipActionViewModel
                {
                    BaseCost = baseCost,
                    Multiplier = multiplier,
                    TotalCost = tokenCost,
                    TokenType = obligation.TokenType.ToString(),
                    AvailableTokens = availableTokens,
                    HasEnoughTokens = availableTokens >= tokenCost,
                    MultiplierReason = multiplierReason
                };
            }

            QueueSlotViewModel slot = new QueueSlotViewModel
            {
                Position = position,
                IsOccupied = obligation != null,
                DeliveryObligation = obligation != null ? ConvertToLetterViewModel(obligation) : null,
                CanDeliver = position == 1 && obligation != null,
                CanSkip = canSkip,
                SkipAction = skipAction
            };

            viewModel.QueueSlots.Add(slot);
        }

        return viewModel;
    }
    
    private LetterViewModel ConvertToLetterViewModel(DeliveryObligation obligation)
    {
        if (obligation == null) return null;
        
        // Calculate deadline urgency
        string deadlineClass = obligation.DeadlineInMinutes <= 24 ? "danger" : 
                              obligation.DeadlineInMinutes <= 48 ? "warning" : "normal";
        string deadlineIcon = obligation.DeadlineInMinutes <= 24 ? "⚠️" : "⏰";
        
        return new LetterViewModel
        {
            Id = obligation.Id,
            SenderName = obligation.SenderName,
            RecipientName = obligation.RecipientName,
            DeadlineInHours = obligation.DeadlineInMinutes / 60,
            Payment = obligation.Payment,
            TokenType = obligation.TokenType.ToString(),
            TokenIcon = GetTokenIcon(obligation.TokenType),
            Size = 1, // Obligations don't have size - that's a physical letter property
            SizeIcon = "📜",
            SizeDisplay = "■",
            IsPatronDeliveryObligation = false,
            IsCollected = false,
            PhysicalConstraints = "",
            PhysicalIcon = "",
            IsSpecial = false,
            SpecialIcon = "",
            SpecialDescription = "",
            DeadlineClass = deadlineClass,
            DeadlineIcon = deadlineIcon,
            DeadlineDescription = $"{obligation.DeadlineInMinutes / 60}h deadline"
        };
    }

    public async Task<bool> ExecuteLetterActionAsync(string actionType, string letterId)
    {
        // Parse action type and letter position
        if (!int.TryParse(letterId, out int position))
            return false;

        switch (actionType.ToLower())
        {
            case "deliver":
                DeliveryObligation letter = _letterQueueManager.GetLetterAt(position);
                if (letter == null) return false;
                bool deliverSuccess = _letterQueueManager.DeliverFromPosition1();
                if (deliverSuccess)
                {
                    // Record successful delivery for environmental storytelling
                    RecordWorldEvent(WorldEventType.LetterDelivered,
                        letter.SenderName,
                        letter.RecipientName,
                        _locationRepository.GetCurrentLocation()?.Id);

                    // Delivery takes 1 hour
                    ProcessTimeAdvancement(1);
                    _messageSystem.AddSystemMessage("DeliveryObligation delivered successfully", SystemMessageTypes.Success);
                }
                return deliverSuccess;
            case "skip":
                DeliveryObligation letterToSkip = _letterQueueManager.GetLetterAt(position);
                if (letterToSkip != null)
                {
                    _letterQueueManager.PrepareSkipAction(letterToSkip.Id);
                }
                return true;
            case "priority":
                bool prioritySuccess = _letterQueueManager.TryPriorityMove(position);
                if (prioritySuccess)
                {
                    _messageSystem.AddSystemMessage("DeliveryObligation moved to priority position", SystemMessageTypes.Success);
                }
                return prioritySuccess;
            case "extend":
                bool extendSuccess = _letterQueueManager.TryExtendDeadline(position);
                if (extendSuccess)
                {
                    _messageSystem.AddSystemMessage("DeliveryObligation deadline extended", SystemMessageTypes.Success);
                }
                return extendSuccess;
            default:
                return false;
        }
    }

    public async Task<bool> DeliverLetterAsync(string letterId)
    {
        // For now, we only support delivering from position 1
        // The letterId parameter is kept for future flexibility if we need to deliver by ID
        DeliveryObligation letter = _letterQueueManager.GetLetterAt(1);
        if (letter == null) return false;

        bool success = _letterQueueManager.DeliverFromPosition1();
        if (success)
        {
            // Delivery takes 1 hour
            ProcessTimeAdvancement(1);
            _messageSystem.AddSystemMessage("DeliveryObligation delivered successfully", SystemMessageTypes.Success);
        }
        return success;
    }

    public async Task<bool> SkipLetterAsync(int position)
    {
        // Validate position is valid for skipping (positions 2-8 when position 1 is empty)
        if (position < 2 || position > 8)
            return false;

        // Get the obligation at this position
        var obligations = _letterQueueManager.GetActiveObligations();
        if (obligations == null || position > obligations.Length)
            return false;
            
        var obligation = obligations[position - 1];
        if (obligation == null)
            return false;

        return _letterQueueManager.PrepareSkipAction(obligation.Id);
    }

    public async Task<bool> LetterQueueMorningSwapAsync(int position1, int position2)
    {
        // Validate positions are within range
        if (position1 < 1 || position1 > 8 || position2 < 1 || position2 > 8)
            return false;

        if (position1 == position2)
            return false;

        bool success = _letterQueueManager.TryMorningSwap(position1, position2);
        if (success)
        {
            _messageSystem.AddSystemMessage($"Swapped letters at positions {position1} and {position2}", SystemMessageTypes.Success);
        }
        return success;
    }

    public async Task<bool> LetterQueuePriorityMoveAsync(int fromPosition)
    {
        // Validate position is within range and not already at position 1
        if (fromPosition < 2 || fromPosition > 8)
            return false;

        bool success = _letterQueueManager.TryPriorityMove(fromPosition);
        if (success)
        {
            _messageSystem.AddSystemMessage($"Moved letter from position {fromPosition} to priority", SystemMessageTypes.Success);
        }
        return success;
    }

    public async Task<bool> LetterQueueExtendDeadlineAsync(int position)
    {
        // Validate position is within range
        if (position < 1 || position > 8)
            return false;

        bool success = _letterQueueManager.TryExtendDeadline(position);
        if (success)
        {
            _messageSystem.AddSystemMessage($"Extended deadline for letter at position {position}", SystemMessageTypes.Success);
        }
        return success;
    }

    public async Task<bool> LetterQueuePurgeAsync(List<TokenSelection> tokenSelections)
    {
        // Validate token selection
        if (tokenSelections == null || tokenSelections.Count == 0)
            return false;

        // Convert to dictionary for serialization (legacy interface)
        Dictionary<ConnectionType, int> enumSelection = new Dictionary<ConnectionType, int>();
        foreach (TokenSelection selection in tokenSelections)
        {
            enumSelection[selection.TokenType] = selection.Count;
        }

        // Store for later use in conversation - strongly typed
        foreach (KeyValuePair<ConnectionType, int> selection in enumSelection)
        {
            _gameWorld.PendingQueueState.PendingPurgeTokens.SetTokenCount(selection.Key, selection.Value);
        }

        // For purge, we need to specify which obligation to purge
        // Typically this would be position 1 or specified elsewhere
        // Get the first obligation as default target
        var obligations = _letterQueueManager.GetActiveObligations();
        if (obligations == null || obligations.Length == 0)
            return false;
            
        var targetObligation = obligations[0]; // Default to first obligation
        if (targetObligation == null)
            return false;
            
        // Trigger conversation
        _letterQueueManager.PreparePurgeAction(targetObligation.Id);
        return true;
    }

    public LetterBoardViewModel GetLetterBoard()
    {
        // Check if it's dawn
        TimeBlocks currentTime = _timeManager.GetCurrentTimeBlock();
        if (currentTime != TimeBlocks.Dawn)
        {
            return new LetterBoardViewModel
            {
                IsAvailable = false,
                UnavailableReason = "DeliveryObligation board is only available at Dawn",
                Offers = new List<LetterOfferViewModel>(),
                CurrentTime = currentTime
            };
        }

        // Letter board no longer exists - all letters come from conversations
        return new LetterBoardViewModel
        {
            IsAvailable = false,
            Offers = new List<LetterOfferViewModel>(),
            CurrentTime = currentTime
        };
    }

    public async Task<bool> AcceptLetterOfferAsync(string offerId)
    {
        // DeliveryObligation offer acceptance now handled through ObligationQueueManager
        // AcceptLetterOffer method removed - needs reimplementation
        return await Task.FromResult(false);
    }

    // ========== ILetterQueueOperations Implementation ==========

    public DeliveryObligation[] GetQueueSnapshot()
    {
        // Return defensive copy of queue
        Player player = _gameWorld.GetPlayer();
        if (player?.ObligationQueue == null) return new DeliveryObligation[8];
        return player.ObligationQueue.ToArray();
    }

    public QueueOperationCost GetOperationCost(QueueOperationType operation, int position1, int? position2 = null)
    {
        List<string> errors = new List<string>();
        Dictionary<ConnectionType, int> costs = new Dictionary<ConnectionType, int>();

        // Validate positions
        if (position1 < 1 || position1 > 8)
            errors.Add($"Invalid position: {position1}");
        if (position2.HasValue && (position2.Value < 1 || position2.Value > 8))
            errors.Add($"Invalid position: {position2}");

        switch (operation)
        {
            case QueueOperationType.MorningSwap:
                if (_timeManager.GetCurrentTimeHours() >= 10)
                    errors.Add("Morning swaps only available before 10:00");
                if (position1 == position2)
                    errors.Add("Cannot swap same position");
                // Morning swap is free during morning hours
                break;

            case QueueOperationType.PriorityMove:
                // Cost increases with distance moved
                int distance = position1 - 1;
                if (distance > 0)
                {
                    costs[ConnectionType.Commerce] = distance * 2;
                    costs[ConnectionType.Trust] = distance;
                }
                break;

            case QueueOperationType.ExtendDeadline:
                // Fixed cost for deadline extension
                costs[ConnectionType.Status] = 3;
                break;

            case QueueOperationType.Deliver:
                if (position1 != 1)
                    errors.Add("Can only deliver from position 1");
                // Delivery itself is free
                break;

            case QueueOperationType.SkipDeliver:
                // High cost for skipping delivery obligation
                costs[ConnectionType.Trust] = 5;
                costs[ConnectionType.Status] = 2;
                break;

            case QueueOperationType.Reorder:
                // Basic reorder cost
                if (position2.HasValue && position1 != position2.Value)
                {
                    costs[ConnectionType.Commerce] = 1;
                }
                break;
        }

        return new QueueOperationCost
        {
            TokenCosts = costs,
            ValidationErrors = errors,
            IsAffordable = errors.Count == 0
        };
    }

    public bool CanPerformOperation(QueueOperationType operation, int position1, int? position2 = null)
    {
        QueueOperationCost cost = GetOperationCost(operation, position1, position2);

        // Check for validation errors
        if (cost.ValidationErrors.Count > 0)
            return false;

        // Check token affordability
        foreach (KeyValuePair<ConnectionType, int> tokenCost in cost.TokenCosts)
        {
            int available = _connectionTokenManager.GetTotalTokensOfType(tokenCost.Key);
            if (available < tokenCost.Value)
                return false;
        }

        // Check specific operation requirements
        switch (operation)
        {
            case QueueOperationType.Deliver:
                return _letterQueueManager.CanDeliverFromPosition1();

            case QueueOperationType.MorningSwap:
                return _timeManager.GetCurrentTimeHours() < 10;

            default:
                return true;
        }
    }

    public async Task<QueueOperationResult> TryMorningSwapAsync(int position1, int position2)
    {
        await _queueOperationLock.WaitAsync();
        try
        {
            // Validate operation
            QueueOperationCost cost = GetOperationCost(QueueOperationType.MorningSwap, position1, position2);
            if (cost.ValidationErrors.Count > 0)
                return new QueueOperationResult(false, string.Join(", ", cost.ValidationErrors), null, GetQueueSnapshot());

            // Execute swap
            bool success = _letterQueueManager.TryMorningSwap(position1, position2);
            if (!success)
                return new QueueOperationResult(false, "Failed to swap letters", null, GetQueueSnapshot());

            _messageSystem.AddSystemMessage($"Swapped positions {position1} and {position2}", SystemMessageTypes.Success);
            return new QueueOperationResult(true, null, new Dictionary<ConnectionType, int>(), GetQueueSnapshot());
        }
        finally
        {
            _queueOperationLock.Release();
        }
    }

    public async Task<QueueOperationResult> TryPriorityMoveAsync(int fromPosition, Dictionary<ConnectionType, int> payment)
    {
        await _queueOperationLock.WaitAsync();
        try
        {
            // Validate tokens
            foreach (KeyValuePair<ConnectionType, int> token in payment)
            {
                if (_connectionTokenManager.GetTotalTokensOfType(token.Key) < token.Value)
                    return new QueueOperationResult(false, $"Insufficient {token.Key} tokens", null, GetQueueSnapshot());
            }

            // Execute move
            bool success = _letterQueueManager.TryPriorityMove(fromPosition);
            if (!success)
                return new QueueOperationResult(false, "Failed to move letter to priority", null, GetQueueSnapshot());

            // Deduct tokens
            foreach (KeyValuePair<ConnectionType, int> token in payment)
            {
                _connectionTokenManager.SpendTokensOfType(token.Key, token.Value);
            }

            _messageSystem.AddSystemMessage($"Moved letter to priority position", SystemMessageTypes.Success);
            return new QueueOperationResult(true, null, payment, GetQueueSnapshot());
        }
        finally
        {
            _queueOperationLock.Release();
        }
    }

    public async Task<QueueOperationResult> TryExtendDeadlineAsync(int position, Dictionary<ConnectionType, int> payment)
    {
        await _queueOperationLock.WaitAsync();
        try
        {
            // Validate tokens
            foreach (KeyValuePair<ConnectionType, int> token in payment)
            {
                if (_connectionTokenManager.GetTotalTokensOfType(token.Key) < token.Value)
                    return new QueueOperationResult(false, $"Insufficient {token.Key} tokens", null, GetQueueSnapshot());
            }

            // Execute extension
            bool success = _letterQueueManager.TryExtendDeadline(position);
            if (!success)
                return new QueueOperationResult(false, "Failed to extend deadline", null, GetQueueSnapshot());

            // Deduct tokens
            foreach (KeyValuePair<ConnectionType, int> token in payment)
            {
                _connectionTokenManager.SpendTokensOfType(token.Key, token.Value);
            }

            _messageSystem.AddSystemMessage($"Extended deadline for letter at position {position}", SystemMessageTypes.Success);
            return new QueueOperationResult(true, null, payment, GetQueueSnapshot());
        }
        finally
        {
            _queueOperationLock.Release();
        }
    }

    public async Task<QueueOperationResult> DeliverFromPosition1Async()
    {
        await _queueOperationLock.WaitAsync();
        try
        {
            if (!_letterQueueManager.CanDeliverFromPosition1())
                return new QueueOperationResult(false, "Cannot deliver: not at recipient location or position 1 is empty", null, GetQueueSnapshot());

            bool success = _letterQueueManager.DeliverFromPosition1();
            if (!success)
                return new QueueOperationResult(false, "Failed to deliver letter", null, GetQueueSnapshot());

            // Delivery takes 1 hour
            ProcessTimeAdvancement(1);
            _messageSystem.AddSystemMessage("DeliveryObligation delivered successfully!", SystemMessageTypes.Success);
            return new QueueOperationResult(true, null, new Dictionary<ConnectionType, int>(), GetQueueSnapshot());
        }
        finally
        {
            _queueOperationLock.Release();
        }
    }

    public async Task<QueueOperationResult> TrySkipDeliverAsync(int position, Dictionary<ConnectionType, int> payment)
    {
        await _queueOperationLock.WaitAsync();
        try
        {
            // Validate tokens
            foreach (KeyValuePair<ConnectionType, int> token in payment)
            {
                if (_connectionTokenManager.GetTotalTokensOfType(token.Key) < token.Value)
                    return new QueueOperationResult(false, $"Insufficient {token.Key} tokens", null, GetQueueSnapshot());
            }

            // Execute skip
            bool success = _letterQueueManager.TrySkipDeliver(position);
            if (!success)
                return new QueueOperationResult(false, "Failed to skip letter", null, GetQueueSnapshot());

            // Deduct tokens
            foreach (KeyValuePair<ConnectionType, int> token in payment)
            {
                _connectionTokenManager.SpendTokensOfType(token.Key, token.Value);
            }

            _messageSystem.AddSystemMessage($"Skipped letter at position {position}", SystemMessageTypes.Warning);
            return new QueueOperationResult(true, null, payment, GetQueueSnapshot());
        }
        finally
        {
            _queueOperationLock.Release();
        }
    }

    public async Task<QueueOperationResult> TryReorderAsync(int fromPosition, int toPosition)
    {
        await _queueOperationLock.WaitAsync();
        try
        {
            // Get the queue object directly
            Player player = _gameWorld.GetPlayer();
            DeliveryObligation[] queue = _letterQueueManager.GetPlayerQueue();
            if (queue == null)
                return new QueueOperationResult(false, "Queue not available", null, GetQueueSnapshot());

            // Get the obligation at fromPosition
            DeliveryObligation obligationToMove = _letterQueueManager.GetLetterAt(fromPosition);
            if (obligationToMove == null)
                return new QueueOperationResult(false, $"No obligation at position {fromPosition}", null, GetQueueSnapshot());
            
            // Move it to toPosition
            _letterQueueManager.MoveObligationToPosition(obligationToMove, toPosition);
            bool success = true;

            _messageSystem.AddSystemMessage($"Reordered: moved position {fromPosition} to {toPosition}", SystemMessageTypes.Success);
            return new QueueOperationResult(true, null, new Dictionary<ConnectionType, int>(), GetQueueSnapshot());
        }
        finally
        {
            _queueOperationLock.Release();
        }
    }

    // ========== NAVIGATION SUPPORT ==========
    
    public void RefreshLocationState()
    {
        // Refresh current location state
        var player = _gameWorld.GetPlayer();
        if (player.CurrentLocationSpot != null)
        {
            _messageSystem.AddSystemMessage("Location refreshed", SystemMessageTypes.Info);
        }
    }

    public async Task<bool> EndConversationAsync()
    {
        // End any active conversation
        if (_conversationManager.IsConversationActive)
        {
            _conversationManager.EndConversation();
            _messageSystem.AddSystemMessage("Conversation ended", SystemMessageTypes.Info);
        }
        return await Task.FromResult(true);
    }

    /// <summary>
    /// Process conversation completion - check comfort thresholds and generate letters
    /// </summary>

    public MarketViewModel GetMarket()
    {
        Player player = _gameWorld.GetPlayer();
        LocationSpot locationSpot = player.CurrentLocationSpot;
        if (locationSpot == null)
            return null;

        Location location = _locationRepository.GetLocation(locationSpot.LocationId);
        if (location == null)
            return null;

        string marketStatus = GetMarketAvailabilityStatus(location.Id);
        List<NPC> traders = GetTradingNPCs(location.Id)
            .Where(npc => npc.IsAvailable(_timeManager.GetCurrentTimeBlock()))
            .ToList();

        // Get all available items
        List<MarketItem> marketItems = GetAvailableMarketItems(location.Id);

        // Convert items to view models
        HashSet<string> allCategories = new HashSet<string> { "All" };
        List<MarketItemViewModel> itemViewModels = new List<MarketItemViewModel>();

        foreach (MarketItem item in marketItems)
        {
            if (item == null) continue;

            bool canBuy = CanBuyMarketItem(item.Id ?? item.Name, location.Id);
            bool canSell = player.Inventory.HasItem(item.Name);

            itemViewModels.Add(new MarketItemViewModel
            {
                ItemId = item.Id ?? item.Name,
                Name = item.Name,
                BuyPrice = item.BuyPrice,
                SellPrice = item.SellPrice,
                CanBuy = canBuy,
                CanSell = canSell,
                TraderId = location.Id, // Use location as trader ID for now since items are location-based
                Categories = item.Categories.Select(c => c.ToString()).ToList(),
                Item = item.Item
            });

            // Collect categories
            foreach (ItemCategory category in item.Categories)
            {
                allCategories.Add(category.ToString());
            }
        }

        // Build view model
        return new MarketViewModel
        {
            LocationName = location.Name,
            MarketStatus = marketStatus,
            IsOpen = marketStatus.Contains("Open"),
            TraderCount = traders.Count,
            PlayerCoins = player.Coins,
            InventoryUsed = player.Inventory.UsedCapacity,
            InventoryCapacity = player.Inventory.Size,
            Items = itemViewModels,
            AvailableCategories = allCategories.OrderBy(c => c).ToList()
        };
    }

    // Market action enum for categorical mapping
    private enum MarketAction
    {
        Buy,
        Sell
    }

    public async Task<bool> BuyItemAsync(string itemId, string traderId)
    {
        return await ExecuteMarketTradeAsync(itemId, MarketAction.Buy, traderId);
    }

    public async Task<bool> SellItemAsync(string itemId, string traderId)
    {
        return await ExecuteMarketTradeAsync(itemId, MarketAction.Sell, traderId);
    }

    private async Task<bool> ExecuteMarketTradeAsync(string itemId, MarketAction action, string locationId)
    {
        if (_ruleEngine == null || _itemRepository == null)
        {
            _messageSystem.AddSystemMessage("Market trading not available", SystemMessageTypes.Danger);
            return false;
        }

        MarketManager.TradeActionResult result;
        if (action == MarketAction.Buy)
        {
            result = _marketManager.TryBuyItem(itemId, locationId);
            if (result.Success)
            {
                // Trading takes 1 hour
                ProcessTimeAdvancement(1);
                _messageSystem.AddSystemMessage($"Successfully purchased item", SystemMessageTypes.Success);
            }
            else
            {
                _messageSystem.AddSystemMessage("Failed to purchase item", SystemMessageTypes.Danger);
            }
        }
        else
        {
            result = _marketManager.TrySellItem(itemId, locationId);
            if (result.Success)
            {
                // Trading takes 1 hour
                ProcessTimeAdvancement(1);
                _messageSystem.AddSystemMessage($"Successfully sold item", SystemMessageTypes.Success);
            }
            else
            {
                _messageSystem.AddSystemMessage("Failed to sell item", SystemMessageTypes.Danger);
            }
        }

        return result.Success;
    }

    // ========== INVENTORY ==========

    public InventoryViewModel GetInventory()
    {
        Player player = _gameWorld.GetPlayer();
        List<InventoryItemViewModel> items = new List<InventoryItemViewModel>();

        foreach (string? itemId in player.Inventory.ItemSlots.Where(s => !string.IsNullOrEmpty(s)))
        {
            Item item = _itemRepository.GetItemById(itemId);
            if (item != null)
            {
                items.Add(new InventoryItemViewModel
                {
                    ItemId = item.Id,
                    Name = item.Name,
                    Description = item.Description,
                    Weight = item.Weight,
                    Value = item.SellPrice,
                });
            }
        }

        return new InventoryViewModel
        {
            Items = items,
            TotalWeight = items.Sum(i => i.Weight),
            MaxSlots = player.Inventory.ItemSlots.Length,
            UsedSlots = items.Count,
            Coins = player.Coins
        };
    }

    public async Task<bool> UseItemAsync(string itemId)
    {
        Item item = _itemRepository.GetItemById(itemId);
        if (item == null) return false;

        {
            return await ReadLetterAsync(itemId);
        }

        // Other item uses can be implemented here
        return false;
    }

    // ========== NARRATIVE/TUTORIAL ==========

    public NarrativeStateViewModel GetNarrativeState()
    {
        return new NarrativeStateViewModel
        {
            ActiveNarratives = new List<NarrativeViewModel>(),
            IsTutorialActive = _flagService.HasFlag("tutorial_active"),
            TutorialComplete = _flagService.HasFlag(FlagService.TUTORIAL_COMPLETE)
        };
    }

    public bool IsTutorialActive()
    {
        return _flagService.HasFlag("tutorial_active");
    }

    public TutorialGuidanceViewModel GetTutorialGuidance()
    {
        if (!_flagService.HasFlag("tutorial_active"))
        {
            return new TutorialGuidanceViewModel { IsActive = false };
        }

        return new TutorialGuidanceViewModel
        {
            IsActive = true,
            CurrentStep = 1,
            TotalSteps = 1,
            StepTitle = "Getting Started",
            GuidanceText = "Welcome to Wayfarer! Start by exploring your current location.",
            AllowedActions = new List<string>()
        };
    }

    // ========== GAME FLOW ==========

    public async Task StartGameAsync()
    {
        // Game already started during initialization
    }

    public async Task<DailyActivityResult> AdvanceToNextDayAsync()
    {
        // Advance time to next day
        _gameWorld.AdvanceToNextDay();

        // Run morning activities
        if (_dailyActivitiesManager != null)
        {
            return _dailyActivitiesManager.ProcessDailyActivities();
        }

        return new DailyActivityResult();
    }

    public DailyActivityResult GetDailyActivities()
    {
        if (_dailyActivitiesManager != null)
        {
            return _dailyActivitiesManager.GetLastActivityResult();
        }

        return new DailyActivityResult();
    }

    // ========== SYSTEM MESSAGES ==========

    public List<SystemMessage> GetSystemMessages()
    {
        // MessageSystem doesn't store messages, they're in GameWorld
        return _gameWorld.SystemMessages ?? new List<SystemMessage>();
    }

    public void ClearSystemMessages()
    {
        // Clear messages from GameWorld
        if (_gameWorld.SystemMessages != null)
            _gameWorld.SystemMessages.Clear();
    }

    // ========== NPC & RELATIONSHIPS ==========

    public List<TimeBlockServiceViewModel> GetTimeBlockServicePlan()
    {
        Player player = _gameWorld.GetPlayer();
        if (player.CurrentLocationSpot == null) return new List<TimeBlockServiceViewModel>();

        Location location = _locationRepository.GetLocation(player.CurrentLocationSpot.LocationId);
        List<TimeBlockServiceInfo> timeBlockServicePlan = _npcRepository.GetTimeBlockServicePlan(location.Id);
        TimeBlocks currentTimeBlock = _timeManager.GetCurrentTimeBlock();

        return timeBlockServicePlan.Select(plan => new TimeBlockServiceViewModel
        {
            TimeBlock = plan.TimeBlock,
            IsCurrentTimeBlock = plan.TimeBlock == currentTimeBlock,
            AvailableServices = plan.AvailableServices,
            AvailableNPCs = plan.AvailableNPCs?.Select(npc => npc.Name).ToList() ?? new List<string>()
        }).ToList();
    }

    public List<NPCWithOffersViewModel> GetNPCsWithOffers()
    {
        Player player = _gameWorld.GetPlayer();
        if (player.CurrentLocationSpot == null) return new List<NPCWithOffersViewModel>();

        TimeBlocks currentTime = _timeManager.GetCurrentTimeBlock();
        Location location = _locationRepository.GetLocation(player.CurrentLocationSpot.LocationId);
        List<NPC> currentNPCs = _npcRepository.GetNPCsForLocationAndTime(location.Id, currentTime);

        return currentNPCs.Select(npc => new NPCWithOffersViewModel
        {
            NPCId = npc.ID,
            NPCName = npc.Name,
            Role = npc.Role,
            HasDirectOfferAvailable = _connectionTokenManager.HasEnoughTokensForDirectOffer(npc.ID),
            PendingOfferCount = 0, // Legacy offer system removed
            IsAvailable = npc.IsAvailable(currentTime)
        })
        .Where(npc => npc.HasDirectOfferAvailable || npc.PendingOfferCount > 0)
        .ToList();
    }

    public List<NPCRelationshipViewModel> GetNPCRelationships()
    {
        Player player = _gameWorld.GetPlayer();
        List<NPCRelationshipViewModel> relationships = new List<NPCRelationshipViewModel>();

        // Get all NPCs from all locations
        List<Location> allLocations = _locationRepository.GetAllLocations();

        foreach (Location location in allLocations)
        {
            List<NPC> npcs = _npcRepository.GetNPCsForLocation(location.Id);
            foreach (NPC npc in npcs)
            {
                // Get tokens with this NPC
                Dictionary<ConnectionType, int> npcTokens = _connectionTokenManager.GetTokensWithNPC(npc.ID);
                int totalTokens = npcTokens.Values.Sum();

                if (totalTokens > 0)
                {
                    relationships.Add(new NPCRelationshipViewModel
                    {
                        NPCId = npc.ID,
                        NPCName = npc.Name,
                        Role = npc.Role,
                        LocationId = location.Id,
                        LocationName = location.Name,
                        ConnectionTokens = totalTokens,
                        CanMakeDirectOffer = _connectionTokenManager.HasEnoughTokensForDirectOffer(npc.ID),
                        TokensByType = npcTokens
                    });
                }
            }
        }

        return relationships.OrderByDescending(r => r.ConnectionTokens).ToList();
    }

    public List<ObligationViewModel> GetStandingObligations()
    {
        List<ObligationViewModel> obligations = new List<ObligationViewModel>();

        // Get active standing obligations from player
        Player player = _gameWorld.GetPlayer();
        foreach (StandingObligation? obligation in player.StandingObligations.Where(o => o.IsActive))
        {
            obligations.Add(new ObligationViewModel
            {
                Name = obligation.Name,
                Description = obligation.GetEffectsSummary(),
                Type = obligation.Source,
                Priority = 1
            });
        }

        // Check for patron obligations based on leverage
        {
            obligations.Add(new ObligationViewModel
            {
                Name = "Patron Employment",
                Type = "Patron",
                Priority = 2
            });
        }

        // Check for token debts (negative tokens)
        List<NPC> allNpcs = _npcRepository.GetAllNPCs();
        foreach (NPC npc in allNpcs)
        {
            Dictionary<ConnectionType, int> tokens = _connectionTokenManager.GetTokensWithNPC(npc.ID);
            foreach (KeyValuePair<ConnectionType, int> tokenType in tokens.Where(t => t.Value < 0))
            {
                obligations.Add(new ObligationViewModel
                {
                    Name = $"Debt to {npc.Name}",
                    Description = $"{Math.Abs(tokenType.Value)} {tokenType.Key} tokens owed",
                    Type = "Debt",
                    Priority = 3
                });
            }
        }

        return obligations.OrderBy(o => o.Priority).ToList();
    }

    public DetailedObligationsViewModel GetDetailedObligations()
    {
        DetailedObligationsViewModel vm = new DetailedObligationsViewModel();
        Player player = _gameWorld.GetPlayer();

        if (_standingObligationManager == null || _standingObligationRepository == null)
        {
            return vm; // Return empty view model if managers not available
        }

        // Get active obligations
        foreach (StandingObligation obligation in _standingObligationManager.GetActiveObligations())
        {
            ActiveObligationViewModel activeVm = new ActiveObligationViewModel
            {
                ID = obligation.ID,
                Name = obligation.Name,
                Description = obligation.Description,
                Source = obligation.Source,
                DaysSinceAccepted = obligation.DaysSinceAccepted,
                RelatedTokenType = obligation.RelatedTokenType,
                TokenCount = 0,
                HasForcedLetterWarning = false,
                DaysUntilForcedDeliveryObligation = 0
            };

            // Add benefit descriptions
            foreach (ObligationEffect effect in obligation.BenefitEffects)
            {
                activeVm.BenefitDescriptions.Add(GetEffectDescription(effect));
            }

            // Add constraint descriptions
            foreach (ObligationEffect effect in obligation.ConstraintEffects)
            {
                activeVm.ConstraintDescriptions.Add(GetEffectDescription(effect));
            }

            // Get token count if applicable
            if (obligation.RelatedTokenType.HasValue)
            {
                activeVm.TokenCount = _connectionTokenManager.GetTokenCount(obligation.RelatedTokenType.Value);
            }

            // Forced letter system removed - all letters come from conversations

            // Check for conflicts
            activeVm.HasConflicts = CheckForConflicts(obligation, _standingObligationManager.GetActiveObligations());

            vm.ActiveObligations.Add(activeVm);
        }

        // Get debt obligations
        foreach (ActiveObligationViewModel? obligation in vm.ActiveObligations.Where(o => o.ID.StartsWith("debt_")))
        {
            string[] parts = obligation.ID.Split('_');
            if (parts.Length >= 3)
            {
                string npcId = parts[1];
                NPC npc = _npcRepository.GetById(npcId);
                if (npc != null && obligation.RelatedTokenType.HasValue)
                {
                    Dictionary<ConnectionType, int> tokens = _connectionTokenManager.GetTokensWithNPC(npcId);
                    int debtAmount = tokens[obligation.RelatedTokenType.Value];

                    DebtObligationViewModel debtVm = new DebtObligationViewModel
                    {
                        ID = obligation.ID,
                        Name = obligation.Name,
                        Description = obligation.Description,
                        LeverageHolderName = npc.Name,
                        LeverageAmount = Math.Abs(debtAmount)
                    };

                    // Add effect descriptions
                    debtVm.EffectDescriptions.Add($"Letters get {Math.Abs(debtAmount)} position priority");
                    if (obligation.ConstraintDescriptions.Any(c => c.Contains("Skip costs doubled")))
                        debtVm.EffectDescriptions.Add("Skip costs doubled");
                    if (obligation.ConstraintDescriptions.Any(c => c.Contains("Cannot refuse")))
                        debtVm.EffectDescriptions.Add("Cannot refuse letters");

                    vm.DebtObligations.Add(debtVm);
                }
            }
        }

        // Check threshold warnings
        List<NPC> allNpcs = _npcRepository.GetAllNPCs();
        foreach (NPC npc in allNpcs)
        {
            Dictionary<ConnectionType, int> tokens = _connectionTokenManager.GetTokensWithNPC(npc.ID);

            foreach ((ConnectionType tokenType, int balance) in tokens)
            {
                // Warning for approaching debt
                if (balance == -1 || balance == -2)
                {
                    vm.ThresholdWarnings.Add(new ThresholdWarningViewModel
                    {
                        WarningType = "Debt",
                        Message = $"At -3 {tokenType} tokens, '{GetDebtObligationName(tokenType)}' will activate",
                        NPCName = npc.Name,
                        CurrentValue = balance,
                        ThresholdValue = -3,
                        TokenType = tokenType
                    });
                }

                // Warning for extreme debt
                if (balance == -4)
                {
                    vm.ThresholdWarnings.Add(new ThresholdWarningViewModel
                    {
                        WarningType = "Debt",
                        Message = $"At -5 {tokenType} tokens, extreme leverage effects will apply",
                        NPCName = npc.Name,
                        CurrentValue = balance,
                        ThresholdValue = -5,
                        TokenType = tokenType
                    });
                }

                // Warning for high positive tokens
                if (balance == 4)
                {
                    vm.ThresholdWarnings.Add(new ThresholdWarningViewModel
                    {
                        WarningType = "HighTokens",
                        Message = $"At 5 {tokenType} tokens, new benefits may unlock",
                        NPCName = npc.Name,
                        CurrentValue = balance,
                        ThresholdValue = 5,
                        TokenType = tokenType
                    });
                }

                if (balance == 9)
                {
                    vm.ThresholdWarnings.Add(new ThresholdWarningViewModel
                    {
                        WarningType = "HighTokens",
                        Message = $"At 10 {tokenType} tokens, powerful benefits may activate",
                        NPCName = npc.Name,
                        CurrentValue = balance,
                        ThresholdValue = 10,
                        TokenType = tokenType
                    });
                }
            }
        }

        // Sort warnings by proximity to threshold
        vm.ThresholdWarnings = vm.ThresholdWarnings
            .OrderBy(w => Math.Abs(w.CurrentValue - w.ThresholdValue))
            .Take(5) // Show top 5 most relevant
            .ToList();

        // Get available templates
        List<StandingObligation> allTemplates = _standingObligationRepository.GetAllObligationTemplates();
        foreach (StandingObligation? template in allTemplates.Where(t => !HasObligation(t.ID, vm.ActiveObligations)))
        {
            ObligationTemplateViewModel templateVm = new ObligationTemplateViewModel
            {
                ID = template.ID,
                Name = template.Name,
                Description = template.Description,
                TriggerDescription = template.Source,
                RequirementDescription = "Available through special letters",
                CanAccept = true
            };

            // Check for conflicts
            List<StandingObligation> conflicts = _standingObligationManager.CheckObligationConflicts(template);
            if (conflicts.Any())
            {
                templateVm.CanAccept = false;
                templateVm.CannotAcceptReason = "Conflicts with existing obligations";
                templateVm.ConflictingObligations = conflicts.Select(c => c.Name).ToList();
            }

            vm.AvailableTemplates.Add(templateVm);
        }

        return vm;
    }

    private bool CheckForConflicts(StandingObligation obligation, List<StandingObligation> allObligations)
    {
        return allObligations
            .Where(other => other.ID != obligation.ID && other.RelatedTokenType == obligation.RelatedTokenType)
            .Any(other =>
                (obligation.BenefitEffects.Any(b => other.ConstraintEffects.Contains(b)) ||
                 obligation.ConstraintEffects.Any(c => other.BenefitEffects.Contains(c))));
    }

    private bool HasObligation(string obligationId, List<ActiveObligationViewModel> activeObligations)
    {
        return activeObligations.Any(o => o.ID.Equals(obligationId, StringComparison.OrdinalIgnoreCase));
    }

    private string GetEffectDescription(ObligationEffect effect)
    {
        return effect switch
        {
            ObligationEffect.StatusPriority => "Status letters enter at slot 3",
            ObligationEffect.CommercePriority => "Commerce letters enter at slot 5",
            ObligationEffect.TrustPriority => "Trust letters enter at slot 7",
            ObligationEffect.PatronJumpToTop => "Patron letters jump to top slots",
            ObligationEffect.CommerceBonus => "Commerce letters +10 coins",
            ObligationEffect.CommerceBonusPlus3 => "Commerce letters +3 coins bonus",
            ObligationEffect.ShadowTriplePay => "Shadow letters pay triple",
            ObligationEffect.TrustFreeExtend => "Trust letters extend deadline free",
            ObligationEffect.ShadowForced => "Forced shadow letter every 3 days",
            ObligationEffect.PatronMonthly => "Monthly patron resource package",
            ObligationEffect.NoStatusRefusal => "Cannot refuse status letters",
            ObligationEffect.NoCommercePurge => "Cannot purge commerce letters",
            ObligationEffect.TrustSkipDoubleCost => "Skipping trust letters costs double",
            ObligationEffect.PatronLettersPosition1 => "Patron letters locked to position 1",
            ObligationEffect.PatronLettersPosition3 => "Patron letters locked to position 3",
            ObligationEffect.DeadlinePlus2Days => "All letters get +2 days deadline",
            ObligationEffect.CannotRefuseLetters => "Cannot refuse any letters",
            ObligationEffect.ShadowEqualsStatus => "Shadow letters use Status position",
            ObligationEffect.MerchantRespect => "Commerce 5+ tokens: +1 position",
            ObligationEffect.PatronAbsolute => "Patron letters push everything down",
            ObligationEffect.DebtSpiral => "All debts get extra leverage",
            ObligationEffect.DynamicLeverageModifier => "Leverage scales with tokens",
            ObligationEffect.DynamicPaymentBonus => "Payment scales with tokens",
            ObligationEffect.DynamicDeadlineBonus => "Deadline bonus scales with tokens",
            _ => effect.ToString()
        };
    }

    private string GetDebtObligationName(ConnectionType tokenType)
    {
        return tokenType switch
        {
            ConnectionType.Trust => "Personal Betrayal",
            ConnectionType.Commerce => "Outstanding Payment",
            ConnectionType.Status => "Social Disgrace",
            ConnectionType.Shadow => "Dangerous Enemy",
            _ => "Debt Obligation"
        };
    }


    private List<MechanicEffectViewModel> ParseMechanicalDescription(string mechanicalDescription)
    {
        List<MechanicEffectViewModel> mechanics = new List<MechanicEffectViewModel>();

        if (string.IsNullOrEmpty(mechanicalDescription))
            return mechanics;

        // Split by common delimiters
        string[] parts = mechanicalDescription.Split('|');

        foreach (string part in parts)
        {
            string trimmed = part.Trim();
            if (string.IsNullOrEmpty(trimmed))
                continue;

            // Determine type based on content
            MechanicEffectType type = MechanicEffectType.Neutral;
            string description = trimmed;

            // Check for specific patterns - NO ICON GENERATION, just clean up and categorize
            if (trimmed.StartsWith("✓") || trimmed.StartsWith("+") || trimmed.Contains("Opens") || trimmed.Contains("+"))
            {
                type = MechanicEffectType.Positive;
                description = trimmed.TrimStart('✓', '+').Trim();
            }
            else if (trimmed.StartsWith("⚠") || trimmed.StartsWith("!") || trimmed.Contains("Must") || trimmed.Contains("burn"))
            {
                type = MechanicEffectType.Negative;
                description = trimmed.TrimStart('⚠', '!').Trim();
            }
            else if (trimmed.StartsWith("ℹ") || trimmed.StartsWith("i") || trimmed.Contains("Gain") || trimmed.Contains("Learn"))
            {
                type = MechanicEffectType.Positive;
                description = trimmed.TrimStart('ℹ', 'i').Trim();
            }
            else if (trimmed.StartsWith("♥") || trimmed.StartsWith("<3") || trimmed.Contains("Trust"))
            {
                type = MechanicEffectType.Positive;
                description = trimmed.TrimStart('♥').Trim();
                if (description.StartsWith("<3"))
                    description = description.Substring(2).Trim();
            }
            else if (trimmed.StartsWith("⛓") || trimmed.StartsWith("[]") || trimmed.Contains("Obligation") || trimmed.Contains("Binding"))
            {
                type = MechanicEffectType.Negative;
                description = trimmed.TrimStart('⛓').Trim();
                if (description.StartsWith("[]"))
                    description = description.Substring(2).Trim();
            }
            else if (trimmed.StartsWith("⏱") || trimmed.StartsWith("~") || trimmed.Contains("minutes") || trimmed.Contains("time"))
            {
                type = MechanicEffectType.Neutral;
                description = trimmed.TrimStart('⏱', '~').Trim();
            }
            else if (trimmed.StartsWith("→") || trimmed.StartsWith("->"))
            {
                description = trimmed.TrimStart('→').Trim();
                if (description.StartsWith("->"))
                    description = description.Substring(2).Trim();
            }
            else if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
            {
                // Locked/unavailable option
                type = MechanicEffectType.Neutral;
                description = trimmed.Trim('[', ']');
            }

            mechanics.Add(new MechanicEffectViewModel
            {
                Description = description,
                Type = type
            });
        }

        // If no delimiters found, treat the whole string as one mechanic
        if (mechanics.Count == 0 && !string.IsNullOrWhiteSpace(mechanicalDescription))
        {
            mechanics.Add(new MechanicEffectViewModel
            {
                Description = mechanicalDescription,
                Type = MechanicEffectType.Neutral
            });
        }

        return mechanics;
    }

    // ========== LEVERAGE SYSTEM ==========

    /// <summary>
    /// Get leverage data for a specific NPC and token type.
    /// Used to display power dynamics in UI.
    /// </summary>
    public LeverageViewModel GetNPCLeverage(string npcId, ConnectionType tokenType)
    {
        if (string.IsNullOrEmpty(npcId))
        {
            return new LeverageViewModel();
        }

        NPC npc = _npcRepository.GetById(npcId);
        if (npc == null)
        {
            return new LeverageViewModel();
        }

        // Get simple leverage from TokenMechanicsManager
        int leverage = _connectionTokenManager.GetLeverage(npcId, tokenType);
        Dictionary<ConnectionType, int> tokens = _connectionTokenManager.GetTokensWithNPC(npcId);
        int tokenBalance = tokens.GetValueOrDefault(tokenType, 0);

        // Calculate target queue position based on leverage
        int basePosition = tokenType switch
        {
            ConnectionType.Status => 3,
            ConnectionType.Commerce => 5,
            ConnectionType.Trust => 6,
            ConnectionType.Shadow => 7,
            _ => 8
        };

        int targetPosition = basePosition;
        if (leverage > 0)
        {
            int leverageBoost = leverage / 2;
            targetPosition = Math.Max(1, basePosition - leverageBoost);

            if (leverage >= 10)
                targetPosition = 1;
            else if (leverage >= 5)
                targetPosition = Math.Min(2, targetPosition);
        }

        // Simple displacement cost (leverage + base cost)
        int displacementCost = Math.Max(2, leverage + 2);

        // Determine leverage level
        string level = leverage switch
        {
            >= 10 => "Extreme",
            >= 5 => "High",
            >= 3 => "Moderate",
            >= 1 => "Low",
            _ => "None"
        };

        // Generate narrative
        string narrative = leverage > 0
            ? $"You owe {npc.Name} {leverage} {tokenType} tokens, giving them {level.ToLower()} leverage over you."
            : $"No leverage with {npc.Name}";

        return new LeverageViewModel
        {
            NPCId = npcId,
            NPCName = npc.Name,
            TokenType = tokenType,
            TotalLeverage = leverage,
            TokenDebtLeverage = leverage,
            ObligationLeverage = 0,  // Simplified - no separate obligation leverage
            FailureLeverage = 0,     // Simplified - incorporated into token debt
            TargetQueuePosition = targetPosition,
            DisplacementCost = displacementCost,
            Level = level,
            Narrative = narrative
        };
    }

    /// <summary>
    /// Get leverage data for all NPCs with active relationships.
    /// Used for relationship overview screens.
    /// </summary>
    public List<LeverageViewModel> GetAllNPCLeverage()
    {
        List<LeverageViewModel> leverageList = new List<LeverageViewModel>();
        Player player = _gameWorld.GetPlayer();

        // Get all NPCs with token relationships
        foreach (KeyValuePair<string, Dictionary<ConnectionType, int>> kvp in player.NPCTokens)
        {
            string npcId = kvp.Key;
            NPC npc = _npcRepository.GetById(npcId);
            if (npc == null) continue;

            // Calculate leverage for each token type the NPC uses
            foreach (ConnectionType tokenType in npc.LetterTokenTypes)
            {
                int leverage = _connectionTokenManager.GetLeverage(npcId, tokenType);

                // Only include if there's actual leverage
                if (leverage > 0)
                {
                    LeverageViewModel leverageViewModel = GetNPCLeverage(npcId, tokenType);
                    leverageList.Add(leverageViewModel);
                }
            }
        }

        return leverageList.OrderByDescending(l => l.TotalLeverage).ToList();
    }

    /// <summary>
    /// Clear leverage cache at turn boundaries.
    /// </summary>
    public void ClearLeverageCache()
    {
        // No cache to clear with simplified leverage system
    }


    // ========== INFORMATION DISCOVERY ==========

    public InformationDiscoveryViewModel GetDiscoveredInformation()
    {
        InformationDiscoveryViewModel vm = new InformationDiscoveryViewModel();

        if (_informationDiscoveryManager == null)
        {
            // Information discovery not yet implemented
            return vm;
        }

        List<Information> allInfo = _informationDiscoveryManager.GetDiscoveredInformation();
        Player player = _gameWorld.GetPlayer();

        foreach (Information info in allInfo)
        {
            DiscoveredInfoViewModel infoVm = new DiscoveredInfoViewModel
            {
                Id = info.Id,
                Name = info.Name,
                Description = info.Description,
                Type = info.Type,
                Tier = info.Tier,
                IsAccessUnlocked = info.IsAccessUnlocked,
                DayDiscovered = info.DayDiscovered,
                TokenRequirements = info.TokenRequirements,
                SealRequirements = info.SealRequirements,
                EquipmentRequirements = info.EquipmentRequirements,
                CoinCost = info.CoinCost
            };

            // Check if player can afford to unlock
            infoVm.CanAfford = CanAffordInformationAccess(info);

            // Categorize by type
            switch (info.Type)
            {
                case InformationType.RouteExistence:
                    vm.RouteInformation.Add(infoVm);
                    break;
                case InformationType.LocationExistence:
                    vm.LocationInformation.Add(infoVm);
                    break;
                case InformationType.NPCExistence:
                    vm.NPCInformation.Add(infoVm);
                    break;
                case InformationType.ServiceAvailability:
                    vm.ServiceInformation.Add(infoVm);
                    break;
                case InformationType.SecretKnowledge:
                    vm.SecretInformation.Add(infoVm);
                    break;
            }
        }

        vm.TotalDiscovered = allInfo.Count;
        vm.TotalUnlocked = allInfo.Count(i => i.IsAccessUnlocked);

        return vm;
    }

    public async Task<bool> UnlockInformationAccessAsync(string informationId)
    {
        if (_informationDiscoveryManager == null)
            return false;

        bool result = _informationDiscoveryManager.TryUnlockAccess(informationId);

        // Save will be handled by the command system

        return result;
    }

    // Leverage system removed - Information letters are for unlocking NPCs/routes only

    private bool CanAffordInformationAccess(Information info)
    {
        Player player = _gameWorld.GetPlayer();

        // Check tokens
        foreach (KeyValuePair<ConnectionType, int> tokenReq in info.TokenRequirements)
        {
            if (!_connectionTokenManager.HasTokens(tokenReq.Key, tokenReq.Value))
                return false;
        }

        // Check seals
        foreach (string sealId in info.SealRequirements)
        {
                return false;
        }

        // Check equipment
        foreach (string equipmentId in info.EquipmentRequirements)
        {
            if (!player.Inventory.HasItem(equipmentId))
                return false;
        }

        // Check coins
        if (player.Coins < info.CoinCost)
            return false;

        return true;
    }

    // ========== LOCATION ACTIONS HELPER METHODS ==========

    // Legacy command conversion methods removed - using intent-based architecture

    private void AddClosedServicesInfo(LocationActionsViewModel viewModel, LocationSpot currentSpot)
    {
        // Skip adding closed service info during tutorial
        bool isInTutorial = _flagService.HasFlag("tutorial_active");
        if (isInTutorial)
        {
            return;
        }

        // Check for DeliveryObligation Board availability
        if (_timeManager.GetCurrentTimeBlock() != TimeBlocks.Dawn)
        {
            ActionOptionViewModel letterBoardInfo = new ActionOptionViewModel
            {
                Id = "letter_board_closed",
                Description = "Visit DeliveryObligation Board",
                IsAvailable = false,
                IsServiceClosed = true,
                NextAvailableTime = GetNextAvailableTime(TimeBlocks.Dawn),
                ServiceSchedule = "Available only at Dawn",
                UnavailableReasons = new List<string> { "DeliveryObligation Board is closed. Only available during Dawn hours." }
            };

            // Add to Special category
            ActionGroupViewModel specialGroup = viewModel.ActionGroups.FirstOrDefault(g => g.ActionType == "Special");
            if (specialGroup == null)
            {
                specialGroup = new ActionGroupViewModel
                {
                    ActionType = "Special",
                    Actions = new List<ActionOptionViewModel>()
                };
                viewModel.ActionGroups.Add(specialGroup);
            }
            specialGroup.Actions.Add(letterBoardInfo);
        }

        // Check for Market availability
        if (_marketManager != null)
        {
            string marketStatus = _marketManager.GetMarketAvailabilityStatus(currentSpot.LocationId);
            if (!marketStatus.Contains("Market Open"))
            {
                List<NPC> allTraders = _marketManager.GetAllTraders(currentSpot.LocationId);
                if (allTraders.Any())
                {
                    string schedule = GetTradersSchedule(allTraders);
                    string nextAvailable = GetNextMarketAvailable(currentSpot.LocationId, allTraders);

                    ActionOptionViewModel marketInfo = new ActionOptionViewModel
                    {
                        Id = "market_closed",
                        Description = "Browse Market",
                        IsAvailable = false,
                        IsServiceClosed = true,
                        NextAvailableTime = nextAvailable,
                        ServiceSchedule = schedule,
                        UnavailableReasons = new List<string> { marketStatus }
                    };

                    // Add to Economic category
                    ActionGroupViewModel economicGroup = viewModel.ActionGroups.FirstOrDefault(g => g.ActionType == "Economic");
                    if (economicGroup == null)
                    {
                        economicGroup = new ActionGroupViewModel
                        {
                            ActionType = "Economic",
                            Actions = new List<ActionOptionViewModel>()
                        };
                        viewModel.ActionGroups.Add(economicGroup);
                    }
                    economicGroup.Actions.Add(marketInfo);
                }
            }
        }

        // Check for missing NPCs and their schedules
        List<NPC> allNPCs = _npcRepository.GetNPCsForLocation(currentSpot.LocationId);
        List<NPC> currentNPCs = _npcRepository.GetNPCsForLocationSpotAndTime(currentSpot.SpotID, _timeManager.GetCurrentTimeBlock());
        List<NPC> missingNPCs = allNPCs.Where(npc => !currentNPCs.Any(c => c.ID == npc.ID)).ToList();

        foreach (NPC missingNPC in missingNPCs)
        {
            string schedule = GetNPCSchedule(missingNPC);
            string nextAvailable = GetNextNPCAvailable(missingNPC);

            // Add info about when this NPC will be available
            ActionOptionViewModel npcInfo = new ActionOptionViewModel
            {
                Id = $"npc_unavailable_{missingNPC.ID}",
                Description = $"Wait for {missingNPC.Name}",
                NPCName = missingNPC.Name,
                NPCProfession = missingNPC.Profession.ToString(),
                IsAvailable = false,
                IsServiceClosed = true,
                NextAvailableTime = nextAvailable,
                ServiceSchedule = schedule,
                UnavailableReasons = new List<string> { $"{missingNPC.Name} is not here right now." }
            };

            // Add to Social category
            ActionGroupViewModel socialGroup = viewModel.ActionGroups.FirstOrDefault(g => g.ActionType == "Social");
            if (socialGroup == null)
            {
                socialGroup = new ActionGroupViewModel
                {
                    ActionType = "Social",
                    Actions = new List<ActionOptionViewModel>()
                };
                viewModel.ActionGroups.Add(socialGroup);
            }
            socialGroup.Actions.Add(npcInfo);
        }
    }

    private string GetNextAvailableTime(TimeBlocks targetTime)
    {
        TimeBlocks currentTime = _timeManager.GetCurrentTimeBlock();
        int currentHour = _timeManager.GetCurrentTimeHours();

        // Calculate hours until target time
        int hoursUntilTarget = CalculateHoursUntilTimeBlock(currentTime, targetTime, currentHour);

        if (hoursUntilTarget <= 0)
        {
            // It's available next day
            return "Available tomorrow at " + GetTimeBlockDisplayName(targetTime);
        }
        else if (hoursUntilTarget == 1)
        {
            return "Available in 1 hour";
        }
        else if (hoursUntilTarget < 12)
        {
            return $"Available in {hoursUntilTarget} hours";
        }
        else
        {
            return "Available tomorrow at " + GetTimeBlockDisplayName(targetTime);
        }
    }

    private string GetTradersSchedule(List<NPC> traders)
    {
        List<string> schedules = new List<string>();
        foreach (NPC trader in traders)
        {
            List<TimeBlocks> availableTimes = GetNPCAvailableTimes(trader);
            if (availableTimes.Any())
            {
                string timeList = string.Join(", ", availableTimes.Select(t => GetTimeBlockDisplayName(t)));
                schedules.Add($"{trader.Name}: {timeList}");
            }
        }
        return string.Join("; ", schedules);
    }

    private string GetNPCSchedule(NPC npc)
    {
        List<TimeBlocks> availableTimes = GetNPCAvailableTimes(npc);
        if (!availableTimes.Any())
        {
            return "Schedule unknown";
        }

        string timeList = string.Join(", ", availableTimes.Select(t => GetTimeBlockDisplayName(t)));
        return $"Available: {timeList}";
    }

    private List<TimeBlocks> GetNPCAvailableTimes(NPC npc)
    {
        List<TimeBlocks> availableTimes = new List<TimeBlocks>();
        TimeBlocks[] allTimes = new[] { TimeBlocks.Dawn, TimeBlocks.Morning, TimeBlocks.Afternoon, TimeBlocks.Evening, TimeBlocks.Night };

        foreach (TimeBlocks time in allTimes)
        {
            if (npc.IsAvailable(time))
            {
                availableTimes.Add(time);
            }
        }

        return availableTimes;
    }

    private string GetNextNPCAvailable(NPC npc)
    {
        TimeBlocks currentTime = _timeManager.GetCurrentTimeBlock();
        List<TimeBlocks> availableTimes = GetNPCAvailableTimes(npc);

        if (!availableTimes.Any())
        {
            return "Availability unknown";
        }

        // Find next available time
        TimeBlocks? nextTime = GetNextAvailableTimeBlock(currentTime, availableTimes);
        if (nextTime.HasValue)
        {
            return GetNextAvailableTime(nextTime.Value);
        }

        return "Available tomorrow";
    }

    private string GetNextMarketAvailable(string locationId, List<NPC> traders)
    {
        TimeBlocks currentTime = _timeManager.GetCurrentTimeBlock();

        // Find all times when at least one trader is available
        HashSet<TimeBlocks> marketTimes = new HashSet<TimeBlocks>();
        foreach (NPC trader in traders)
        {
            List<TimeBlocks> traderTimes = GetNPCAvailableTimes(trader);
            foreach (TimeBlocks time in traderTimes)
            {
                marketTimes.Add(time);
            }
        }

        if (!marketTimes.Any())
        {
            return "Market schedule unknown";
        }

        TimeBlocks? nextTime = GetNextAvailableTimeBlock(currentTime, marketTimes.ToList());
        if (nextTime.HasValue)
        {
            return GetNextAvailableTime(nextTime.Value);
        }

        return "Available tomorrow";
    }

    private TimeBlocks? GetNextAvailableTimeBlock(TimeBlocks current, List<TimeBlocks> availableTimes)
    {
        TimeBlocks[] timeOrder = new[] { TimeBlocks.Dawn, TimeBlocks.Morning, TimeBlocks.Afternoon, TimeBlocks.Evening, TimeBlocks.Night };
        int currentIndex = Array.IndexOf(timeOrder, current);

        // Check remaining times today
        for (int i = currentIndex + 1; i < timeOrder.Length; i++)
        {
            if (availableTimes.Contains(timeOrder[i]))
            {
                return timeOrder[i];
            }
        }

        // Check times tomorrow (starting from Dawn)
        for (int i = 0; i <= currentIndex; i++)
        {
            if (availableTimes.Contains(timeOrder[i]))
            {
                return timeOrder[i];
            }
        }

        return null;
    }

    private int CalculateHoursUntilTimeBlock(TimeBlocks current, TimeBlocks target, int currentHour)
    {
        // Map time blocks to hour ranges
        int targetStartHour = target switch
        {
            TimeBlocks.Dawn => 4,
            TimeBlocks.Morning => 8,
            TimeBlocks.Afternoon => 12,
            TimeBlocks.Evening => 17,
            TimeBlocks.Night => 20,
            _ => 0
        };

        if (targetStartHour > currentHour)
        {
            return targetStartHour - currentHour;
        }
        else
        {
            // Next day
            return (24 - currentHour) + targetStartHour;
        }
    }

    private string GetTimeBlockDisplayName(TimeBlocks timeBlock)
    {
        return timeBlock switch
        {
            TimeBlocks.Dawn => "Dawn (4-8 AM)",
            TimeBlocks.Morning => "Morning (8 AM-12 PM)",
            TimeBlocks.Afternoon => "Afternoon (12-5 PM)",
            TimeBlocks.Evening => "Evening (5-8 PM)",
            TimeBlocks.Night => "Night (8 PM-4 AM)",
            _ => timeBlock.ToString()
        };
    }

    // ========== TOKEN MANAGEMENT ==========

    public NPCTokenBalance GetTokensWithNPC(string npcId)
    {
        Dictionary<ConnectionType, int> tokenDict = _connectionTokenManager?.GetTokensWithNPC(npcId) ?? new Dictionary<ConnectionType, int>();

        NPCTokenBalance balance = new NPCTokenBalance();

        foreach (KeyValuePair<ConnectionType, int> kvp in tokenDict)
        {
            balance.Balances.Add(new TokenBalance
            {
                TokenType = kvp.Key,
                Amount = kvp.Value
            });
        }

        return balance;
    }

    public int GetTokenCount(ConnectionType tokenType)
    {
        return _connectionTokenManager?.GetTokenCount(tokenType) ?? 0;
    }

    // ========== NPC & LOCATION QUERIES ==========

    public List<NPC> GetAllNPCs()
    {
        return _npcRepository?.GetAllNPCs() ?? new List<NPC>();
    }

    public NPC GetNPCById(string npcId)
    {
        return _npcRepository?.GetById(npcId);
    }

    public NPC GetNPCByName(string name)
    {
        return _npcRepository?.GetByName(name);
    }

    public Location GetLocation(string locationId)
    {
        return _locationRepository?.GetLocation(locationId);
    }

    // ========== LETTER SERVICES ==========

    public bool CanNPCOfferLetters(string npcId)
    {
        // Check if NPC has tokens to offer letters
        NPC? npc = _npcRepository?.GetById(npcId);
        if (npc == null) return false;

        // Check if NPC can offer letters based on tokens
        Dictionary<ConnectionType, int>? tokens = _connectionTokenManager?.GetTokensWithNPC(npcId);
        return tokens != null && tokens.Any(t => t.Value > 0);
    }


    public bool RequestSpecialLetter(string npcId, ConnectionType tokenType)
    {
        // Map token type to special letter type
        LetterSpecialType specialType = tokenType switch
        {
            ConnectionType.Trust => LetterSpecialType.Introduction,
            ConnectionType.Commerce => LetterSpecialType.AccessPermit,
            _ => LetterSpecialType.None
        };
        
        // For now, special letters not implemented in new system
        return false;
    }

    public LetterCategory GetAvailableCategory(string npcId, ConnectionType tokenType)
    {
        // Determine category based on token count
        var tokens = _connectionTokenManager.GetTokensWithNPC(npcId);
        int tokenCount = tokens.TryGetValue(tokenType, out int count) ? count : 0;
        
        if (tokenCount >= GameRules.TOKENS_PREMIUM_THRESHOLD)
            return LetterCategory.Premium;
        if (tokenCount >= GameRules.TOKENS_QUALITY_THRESHOLD)
            return LetterCategory.Quality;
        return LetterCategory.Basic;
    }

    public int GetTokensToNextCategory(string npcId, ConnectionType tokenType)
    {
        // Calculate tokens needed for next category threshold
        Dictionary<ConnectionType, int> tokens = _connectionTokenManager.GetTokensWithNPC(npcId);
        int currentTokens = tokens.GetValueOrDefault(tokenType, 0);
        
        if (currentTokens < GameRules.TOKENS_QUALITY_THRESHOLD)
            return GameRules.TOKENS_QUALITY_THRESHOLD - currentTokens;
        if (currentTokens < GameRules.TOKENS_PREMIUM_THRESHOLD)
            return GameRules.TOKENS_PREMIUM_THRESHOLD - currentTokens;
        
        return 0; // Already at premium
    }

    public (int min, int max) GetCategoryPaymentRange(LetterCategory category)
    {
        // Payment ranges based on category
        return category switch
        {
            LetterCategory.Basic => (4, 8),
            LetterCategory.Quality => (8, 12),
            LetterCategory.Premium => (12, 20),
            _ => (4, 8)
        };
    }

    // ========== ITEM MANAGEMENT ==========

    public Item GetItemById(string itemId)
    {
        return _itemRepository?.GetItemById(itemId);
    }

    public async Task<bool> ReadLetterAsync(string itemId)
    {
        Item item = _itemRepository.GetItemById(itemId);
        {
            return false;
        }

        // Execute read directly
        {
        }

        // Show message about reading the letter
        _messageSystem.AddSystemMessage($"You carefully read the {item.Name}...", SystemMessageTypes.Info);

        // Show any special effects or notifications
        {
            _messageSystem.AddSystemMessage("The letter's contents give you pause. This could change everything...", SystemMessageTypes.Tutorial);
        }

        return true;

        return false;
    }

    public bool CanReadItem(string itemId)
    {
        // Letters are not items - they're separate entities
        // This method is for legacy compatibility
        return false;
    }


    // ========== LETTER QUEUE MANAGEMENT ==========

    public int GetLetterQueueCount()
    {
        Player player = _gameWorld.GetPlayer();
        if (player?.ObligationQueue == null) return 0;
        return player.ObligationQueue.Count(o => o != null);
    }

    public bool IsLetterQueueFull()
    {
        int count = GetLetterQueueCount();
        return count >= 8; // MAX_LETTER_QUEUE_SIZE
    }

    public int AddLetterWithObligationEffects(DeliveryObligation letter)
    {
        if (letter == null || IsLetterQueueFull())
            return -1;

        // Delegate to ObligationQueueManager for proper queue management
        return _letterQueueManager.AddLetterWithObligationEffects(letter);
    }

    public bool IsActionForbidden(string actionType, DeliveryObligation letter, out string reason)
    {
        reason = null;

        if (_standingObligationManager == null)
            return false;

        return _standingObligationManager.IsActionForbidden(actionType, letter, out reason);
    }

}


// Queue operation types moved from deleted interface
public enum QueueOperationType
{
    MorningSwap,
    PriorityMove,
    ExtendDeadline,
    SkipDelivery,
    Deliver,
    SkipDeliver,
    Reorder
}

public class QueueOperationCost
{
    public Dictionary<ConnectionType, int> TokenCosts { get; set; } = new Dictionary<ConnectionType, int>();
    public List<string> ValidationErrors { get; set; } = new List<string>();
    public bool IsAffordable { get; set; }
}

public class QueueOperationResult
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
    public Dictionary<ConnectionType, int> TokensSpent { get; set; }
    public DeliveryObligation[] UpdatedQueue { get; set; }

    public QueueOperationResult(bool success, string errorMessage, Dictionary<ConnectionType, int> tokensSpent, DeliveryObligation[] updatedQueue)
    {
        Success = success;
        ErrorMessage = errorMessage;
        TokensSpent = tokensSpent ?? new Dictionary<ConnectionType, int>();
        UpdatedQueue = updatedQueue;
    }
}
