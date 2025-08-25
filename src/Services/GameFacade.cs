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
    // Core dependencies
    private readonly GameWorld _gameWorld;
    private readonly TimeManager _timeManager;
    private readonly MessageSystem _messageSystem;
    private readonly IGameRuleEngine _ruleEngine;


    // Managers
    private readonly TravelManager _travelManager;
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
    private readonly StandingObligationManager _standingObligationManager;
    private readonly StandingObligationRepository _standingObligationRepository;
    private readonly MarketManager _marketManager;
    private readonly DailyActivitiesManager _dailyActivitiesManager;
    private readonly ObservationSystem _observationSystem;
    private readonly ObservationManager _observationManager;
    private readonly ActionGenerator _actionGenerator;
    private readonly BindingObligationSystem _bindingObligationSystem;
    private readonly TimeBlockAttentionManager _timeBlockAttentionManager;
    private readonly NPCDeckFactory _deckFactory;
    private readonly DialogueGenerationService _dialogueGenerator;
    private readonly NarrativeRenderer _narrativeRenderer;
    private readonly AccessRequirementChecker _accessChecker;

    public GameFacade(
        GameWorld gameWorld,
        TimeManager timeManager,
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
        StandingObligationManager standingObligationManager,
        StandingObligationRepository standingObligationRepository,
        MarketManager marketManager,
        IGameRuleEngine ruleEngine,
        DailyActivitiesManager dailyActivitiesManager,
        ActionGenerator actionGenerator,
        ObservationSystem observationSystem,
        ObservationManager observationManager,
        BindingObligationSystem bindingObligationSystem,
        TimeBlockAttentionManager timeBlockAttentionManager,
        NPCDeckFactory deckFactory,
        DialogueGenerationService dialogueGenerator,
        NarrativeRenderer narrativeRenderer,
        AccessRequirementChecker accessChecker
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
        _standingObligationManager = standingObligationManager;
        _standingObligationRepository = standingObligationRepository;
        _marketManager = marketManager;
        _ruleEngine = ruleEngine;
        _dailyActivitiesManager = dailyActivitiesManager;
        _actionGenerator = actionGenerator;
        _observationSystem = observationSystem;
        _observationManager = observationManager;
        Console.WriteLine($"[GameFacade] Constructor - ObservationSystem null? {observationSystem == null}");
        _bindingObligationSystem = bindingObligationSystem;
        _timeBlockAttentionManager = timeBlockAttentionManager;
        _deckFactory = deckFactory;
        _dialogueGenerator = dialogueGenerator;
        _narrativeRenderer = narrativeRenderer;
        _accessChecker = accessChecker;
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
        Console.WriteLine($"[ProcessTimeAdvancementMinutes] Starting time advancement: {minutes} minutes");
        
        // Convert to hours for deadline processing
        int hours = minutes / 60;

        // Get time before advancement
        var timeBefore = _timeManager.GetFormattedTimeDisplay();
        
        _timeManager.AdvanceTimeMinutes(minutes);
        
        // Get time after advancement
        var timeAfter = _timeManager.GetFormattedTimeDisplay();
        Console.WriteLine($"[ProcessTimeAdvancementMinutes] Time changed from {timeBefore} to {timeAfter}");

        // Update letter deadlines when time advances (even partial hours)
        if (hours > 0)
        {
            _letterQueueManager.ProcessHourlyDeadlines(hours);
        }

        // Process carried information letters after time change
        // Information revelation handled through other systems now
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

    public ObligationQueueManager GetObligationQueueManager()
    {
        return _letterQueueManager;
    }

    public Location GetCurrentLocation()
    {
        Player player = _gameWorld.GetPlayer();
        if (player.CurrentLocationSpot == null) return null;
        return _locationRepository.GetLocation(player.CurrentLocationSpot.LocationId);
    }
    
    public LocationSpot GetCurrentLocationSpot()
    {
        return _gameWorld.GetPlayer().CurrentLocationSpot;
    }

    /// <summary>
    /// Move player to a different spot within the current location.
    /// Movement between spots within a location is FREE (no attention cost).
    /// </summary>
    /// <param name="spotName">The name or ID of the target spot</param>
    /// <returns>True if movement successful, false otherwise</returns>
    public bool MoveToSpot(string spotName)
    {
        // VALIDATION: Check inputs
        if (string.IsNullOrEmpty(spotName))
        {
            _messageSystem.AddSystemMessage("Invalid spot name", SystemMessageTypes.Warning);
            return false;
        }

        // STATE CHECK: Get current location and player
        Player player = _gameWorld.GetPlayer();
        if (player.CurrentLocationSpot == null)
        {
            _messageSystem.AddSystemMessage("Cannot determine current location", SystemMessageTypes.Danger);
            return false;
        }

        Location currentLocation = _locationRepository.GetLocation(player.CurrentLocationSpot.LocationId);
        if (currentLocation == null)
        {
            _messageSystem.AddSystemMessage("Current location not found", SystemMessageTypes.Danger);
            return false;
        }

        // EDGE CASE: Check if already at target spot
        if (player.CurrentLocationSpot.Name == spotName || player.CurrentLocationSpot.SpotID == spotName)
        {
            // Already at this spot - no-op success
            return true;
        }

        // SPOT RESOLUTION: Find target spot in current location
        LocationSpot targetSpot = null;
        
        // First check AvailableSpots in the Location object
        if (currentLocation.AvailableSpots != null)
        {
            targetSpot = currentLocation.AvailableSpots.FirstOrDefault(s => 
                s.Name == spotName || s.SpotID == spotName);
        }
        
        // If not found, check the location spot repository for spots in this location
        if (targetSpot == null)
        {
            List<LocationSpot> spotsInLocation = _locationSpotRepository.GetSpotsForLocation(currentLocation.Id);
            targetSpot = spotsInLocation?.FirstOrDefault(s => 
                s.Name == spotName || s.SpotID == spotName);
        }

        // VALIDATION: Target spot must exist and be in current location
        if (targetSpot == null)
        {
            _messageSystem.AddSystemMessage($"Spot '{spotName}' not found in {currentLocation.Name}", SystemMessageTypes.Warning);
            return false;
        }

        // VALIDATION: Verify spot belongs to current location
        if (targetSpot.LocationId != currentLocation.Id)
        {
            _messageSystem.AddSystemMessage($"Spot '{spotName}' is not in current location", SystemMessageTypes.Warning);
            return false;
        }

        // ACCESS CHECK: Check if spot has access requirements
        if (targetSpot.AccessRequirement != null)
        {
            AccessCheckResult accessCheck = _accessChecker.CheckSpotAccess(targetSpot);
            if (!accessCheck.IsAllowed)
            {
                _messageSystem.AddSystemMessage(accessCheck.BlockedMessage ?? "Cannot access this spot", SystemMessageTypes.Warning);
                return false;
            }
        }

        // STATE TRANSITION: Update player location
        player.CurrentLocationSpot = targetSpot;
        
        // NOTIFICATION: Inform player of successful movement
        _messageSystem.AddSystemMessage($"Moved to {targetSpot.Name}", SystemMessageTypes.Info);
        
        // DISCOVERY: Track that player has visited this spot
        player.AddKnownLocationSpot(targetSpot.SpotID);
        
        return true;
    }

    /// <summary>
    /// Perform work at a Commercial location spot.
    /// Costs 2 attention, rewards 8 coins.
    /// </summary>
    public async Task<WorkResult> PerformWork()
    {
        // CONSTANTS (should be in GameRules but using literals as per requirements)
        const int WORK_ATTENTION_COST = 2;
        const int WORK_COIN_REWARD = 8;
        
        // STATE CHECK: Get player and current spot
        Player player = _gameWorld.GetPlayer();
        LocationSpot currentSpot = player.CurrentLocationSpot;
        
        if (currentSpot == null)
        {
            return new WorkResult
            {
                Success = false,
                Message = "Cannot determine current location",
                RemainingAttention = 0
            };
        }
        
        // VALIDATION: Check if spot is Commercial
        bool isCommercial = currentSpot.Properties?.Contains("Commercial") ?? false;
        if (!isCommercial)
        {
            return new WorkResult
            {
                Success = false,
                Message = "You can only work at Commercial locations",
                RemainingAttention = _timeBlockAttentionManager.GetCurrentAttention(_timeManager.GetCurrentTimeBlock()).GetAvailableAttention()
            };
        }
        
        // RESOURCE CHECK: Get current attention
        TimeBlocks currentTimeBlock = _timeManager.GetCurrentTimeBlock();
        AttentionManager currentAttention = _timeBlockAttentionManager.GetCurrentAttention(currentTimeBlock);
        int availableAttention = currentAttention.GetAvailableAttention();
        
        if (availableAttention < WORK_ATTENTION_COST)
        {
            return new WorkResult
            {
                Success = false,
                Message = $"Not enough attention to work (need {WORK_ATTENTION_COST}, have {availableAttention})",
                RemainingAttention = availableAttention
            };
        }
        
        // TRANSACTION: Spend attention
        bool attentionSpent = currentAttention.TrySpend(WORK_ATTENTION_COST);
        if (!attentionSpent)
        {
            return new WorkResult
            {
                Success = false,
                Message = "Failed to spend attention",
                RemainingAttention = availableAttention
            };
        }
        
        // TRANSACTION: Award coins
        player.Coins += WORK_COIN_REWARD;
        
        // TIME ADVANCEMENT: Work takes time (1 period)
        _timeManager.AdvanceTime(1);
        
        // NOTIFICATION: Log the work action
        _messageSystem.AddSystemMessage($"You worked hard and earned {WORK_COIN_REWARD} coins", SystemMessageTypes.Success);
        
        // RETURN: Success result
        return new WorkResult
        {
            Success = true,
            Message = $"Earned {WORK_COIN_REWARD} coins from working",
            CoinsEarned = WORK_COIN_REWARD,
            AttentionSpent = WORK_ATTENTION_COST,
            RemainingAttention = currentAttention.GetAvailableAttention()
        };
    }

    public (TimeBlocks timeBlock, int hoursRemaining, int currentDay) GetTimeInfo()
    {
        return (_timeManager.GetCurrentTimeBlock(),
                _timeManager.HoursRemaining,
                _gameWorld.CurrentDay);
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
        Console.WriteLine("[GameFacade.GetLocationScreen] Starting...");
        Player player = _gameWorld.GetPlayer();
        Location location = GetCurrentLocation();
        LocationSpot spot = GetCurrentLocationSpot();
        Console.WriteLine($"[GameFacade.GetLocationScreen] Location: {location?.Name}, Spot: {spot?.Name} ({spot?.SpotID})");
        
        var viewModel = new LocationScreenViewModel
        {
            CurrentTime = _timeManager.GetFormattedTimeDisplay(),
            DeadlineTimer = GetNextDeadlineDisplay(),
            LocationPath = BuildLocationPath(location, spot),
            LocationName = location?.Name ?? "Unknown Location",
            CurrentSpotName = spot?.Name,
            LocationTraits = GetLocationTraits(location, spot),
            AtmosphereText = GenerateAtmosphereText(spot, location),
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
            Console.WriteLine($"[GameFacade.GetLocationScreen] Looking for NPCs at {spot.SpotID} during {currentTime}");
            var npcs = _npcRepository.GetNPCsForLocationSpotAndTime(spot.SpotID, currentTime);
            Console.WriteLine($"[GameFacade.GetLocationScreen] Found {npcs.Count} NPCs");
            
            foreach (var npc in npcs)
            {
                var emotionalState = GetNPCEmotionalState(npc);
                
                // Get available conversation types for this NPC
                var availableConversationTypes = _conversationManager.GetAvailableConversationTypes(npc);
                var interactions = new List<InteractionOptionViewModel>();
                
                // Generate interaction options based on available conversation types
                foreach (var conversationType in availableConversationTypes)
                {
                    var interaction = GenerateConversationInteraction(npc, conversationType, emotionalState);
                    if (interaction != null)
                    {
                        interactions.Add(interaction);
                    }
                }
                
                // If no interactions available (e.g., hostile NPC), show why
                if (!interactions.Any())
                {
                    interactions.Add(new InteractionOptionViewModel
                    {
                        Text = emotionalState == EmotionalState.HOSTILE ? "Too hostile to approach" : "No interactions available",
                        Cost = "—"
                    });
                }
                
                viewModel.NPCsPresent.Add(new NPCPresenceViewModel
                {
                    Id = npc.ID,
                    Name = npc.Name,
                    EmotionalStateName = emotionalState.ToString(),
                    Description = GetNPCDescription(npc, emotionalState),
                    Interactions = interactions
                });
            }
            
            // Add observations (filtered by current spot)
            viewModel.Observations = GetLocationObservations(location.Id, spot.SpotID);
            
            // Add areas within location
            viewModel.AreasWithinLocation = GetAreasWithinLocation(location, spot);
            
            // Add routes to other locations
            viewModel.Routes = GetRoutesFromLocation(location);
        }
        
        Console.WriteLine($"[GameFacade.GetLocationScreen] Returning viewModel with {viewModel.NPCsPresent.Count} NPCs");
        return viewModel;
    }
    
    private InteractionOptionViewModel GenerateConversationInteraction(NPC npc, ConversationType conversationType, EmotionalState emotionalState)
    {
        // Get attention cost for this conversation type
        int attentionCost = ConversationTypeConfig.GetAttentionCost(conversationType);
        
        // Check if player has enough attention
        var attentionState = _timeBlockAttentionManager.GetAttentionState();
        bool hasEnoughAttention = attentionState.current >= attentionCost;
        
        // Check if conversation type is locked due to crisis
        bool isLockedByCrisis = npc.HasCrisisCards() && conversationType != ConversationType.Crisis;
        
        // Generate the interaction option
        var interaction = new InteractionOptionViewModel();
        interaction.ConversationType = conversationType;
        
        switch (conversationType)
        {
            case ConversationType.QuickExchange:
                interaction.Text = "Quick Exchange";
                interaction.Cost = "Free";
                break;
                
            case ConversationType.Crisis:
                interaction.Text = "⚠ Address Crisis";
                interaction.Cost = "1 attention";
                break;
                
            case ConversationType.Standard:
                interaction.Text = "Have Conversation";
                interaction.Cost = "2 attention";
                break;
                
            case ConversationType.Deep:
                interaction.Text = "Deep Conversation";
                interaction.Cost = "3 attention";
                break;
        }
        
        // Mark as locked if crisis is active and this isn't the crisis conversation
        if (isLockedByCrisis)
        {
            interaction.Text = $"[LOCKED] {interaction.Text}";
            interaction.Cost = "Crisis must be resolved";
        }
        // Mark as unavailable if not enough attention
        else if (!hasEnoughAttention && attentionCost > 0)
        {
            interaction.Text = $"[TIRED] {interaction.Text}";
            interaction.Cost = $"Need {attentionCost} attention";
        }
        
        return interaction;
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
    
    public EmotionalState GetNPCEmotionalState(NPC npc)
    {
        // Use the same logic as the conversation system for consistency
        return ConversationRules.DetermineInitialState(npc, _letterQueueManager);
    }
    
    public EmotionalState GetNPCEmotionalState(string npcId)
    {
        var npc = _npcRepository.GetById(npcId);
        if (npc == null) return EmotionalState.NEUTRAL;
        return GetNPCEmotionalState(npc);
    }
    
    public List<SimpleRouteViewModel> GetAvailableRoutes()
    {
        var currentLocation = GetCurrentLocation();
        if (currentLocation == null) return new List<SimpleRouteViewModel>();
        
        // Get all routes from current location
        var routes = _routeRepository.GetRoutesFromLocation(currentLocation.Id);
        
        return routes.Select(r => new SimpleRouteViewModel
        {
            Id = r.Id,
            Destination = GetDestinationName(r.DestinationLocationSpot),
            TransportType = r.Method.ToString().ToLower(),
            TravelTimeInMinutes = r.TravelTimeMinutes,
            Cost = r.BaseCoinCost,
            FamiliarityLevel = r.IsDiscovered ? "Known" : "Unknown"
        }).ToList();
    }
    
    private string GetDestinationName(string spotId)
    {
        var spot = _gameWorld.WorldState.locationSpots.FirstOrDefault(s => s.SpotID == spotId);
        if (spot == null) return "Unknown";
        
        var location = _locationRepository.GetLocation(spot.LocationId);
        return location?.Name ?? "Unknown";
    }
    
    private bool IsRouteAvailable(RouteOption route)
    {
        var player = _gameWorld.GetPlayer();
        
        // Check tier requirement
        if (route.TierRequired > player.CurrentTier)
            return false;
            
        // Check token requirements if any
        var accessCheck = _accessChecker.CheckRouteAccess(route);
        if (!accessCheck.IsAllowed)
            return false;
        
        return true;
    }
    
    private string GetRouteLockReason(RouteOption route)
    {
        var player = _gameWorld.GetPlayer();
        
        if (route.TierRequired > player.CurrentTier)
            return $"Requires Tier {route.TierRequired}";
            
        var accessCheck = _accessChecker.CheckRouteAccess(route);
        if (!accessCheck.IsAllowed)
            return accessCheck.BlockedMessage ?? "Requirements not met";
        
        return "";
    }
    
    public async Task<bool> DisplaceLetterInQueue(string letterId)
    {
        // For now, displacement is a no-op as the system handles it automatically via leverage
        // Manual displacement would require implementing a swap or reorder system
        _messageSystem.AddSystemMessage(
            "Letter displacement is handled automatically based on leverage and debt relationships.",
            SystemMessageTypes.Info
        );
        return false;
    }
    
    private string GetNPCDescription(NPC npc, EmotionalState state)
    {
        // Check if NPC has urgent letter
        var obligations = _letterQueueManager.GetActiveObligations();
        var hasUrgentLetter = obligations.Any(o => 
            (o.SenderId == npc.ID || o.SenderName == npc.Name) && 
            o.DeadlineInMinutes < 360);
        
        // Generate categorical template
        var template = _dialogueGenerator.GenerateNPCDescription(npc, state, hasUrgentLetter);
        
        // Render to human-readable text
        return _narrativeRenderer.RenderTemplate(template);
    }
    
    private List<ObservationViewModel> GetLocationObservations(string locationId, string currentSpotId)
    {
        var observations = new List<ObservationViewModel>();
        
        Console.WriteLine($"[GetLocationObservations] Looking for observations at {locationId}, spot {currentSpotId}");
        Console.WriteLine($"[GetLocationObservations] ObservationSystem null? {_observationSystem == null}");
        
        // Get observations from ObservationSystem
        var locationObservations = _observationSystem?.GetObservationsForLocationSpot(locationId, currentSpotId);
        
        Console.WriteLine($"[GetLocationObservations] Got {locationObservations?.Count ?? 0} observations from ObservationSystem");
        
        if (locationObservations != null)
        {
            // Get current time for filtering
            var currentTimeBlock = _timeManager.GetCurrentTimeBlock();
            var currentHour = _timeManager.GetCurrentTimeHours();
            
            // Get NPCs at current spot
            var npcsAtCurrentSpot = _npcRepository.GetNPCsForLocationSpotAndTime(currentSpotId, currentTimeBlock);
            var npcIdsAtCurrentSpot = npcsAtCurrentSpot.Select(n => n.ID).ToHashSet();
            
            foreach (var obs in locationObservations)
            {
                // Apply time-based filtering for specific observations
                bool shouldDisplay = ShouldDisplayObservation(obs, currentTimeBlock, currentHour);
                if (!shouldDisplay)
                {
                    Console.WriteLine($"[GetLocationObservations] Skipping observation {obs.Id} - not visible at current time");
                    continue;
                }
                
                // Filter out observations about NPCs not at current spot
                if (obs.RelevantNPCs?.Any() == true)
                {
                    // Check if any of the relevant NPCs are at the current spot
                    bool hasNpcAtSpot = obs.RelevantNPCs.Any(npcId => npcIdsAtCurrentSpot.Contains(npcId));
                    if (!hasNpcAtSpot)
                    {
                        continue; // Skip observations about NPCs at other spots
                    }
                }
                
                observations.Add(new ObservationViewModel
                {
                    Id = obs.Id,  // Pass the observation ID
                    Text = obs.Text,
                    Icon = obs.Type == ObservationType.Important ? "⚠️" : "👁️",
                    AttentionCost = obs.AttentionCost,
                    Relevance = BuildRelevanceString(obs),
                    IsObserved = _observationManager.HasTakenObservation(obs.Id)
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
    
    private bool ShouldDisplayObservation(Observation obs, TimeBlocks currentTimeBlock, int currentHour)
    {
        // Special handling for "guards_blocking" observation - only show in Afternoon
        if (obs.Id == "guards_blocking" || obs.Id == "guards_north")
        {
            return currentTimeBlock == TimeBlocks.Afternoon;
        }
        
        // For other observations, always display them (unless we add more time-based logic)
        return true;
    }
    
    private List<AreaWithinLocationViewModel> GetAreasWithinLocation(Location location, LocationSpot currentSpot)
    {
        var areas = new List<AreaWithinLocationViewModel>();
        
        // Get all spots in the same location
        var spots = _locationSpotRepository.GetSpotsForLocation(location.Id);
        var currentTime = _timeManager.GetCurrentTimeBlock();
        
        foreach (var spot in spots)
        {
            // Skip the current spot - don't show it in the list
            if (spot.SpotID == currentSpot?.SpotID)
                continue;
                
            // Get NPCs at this spot
            var npcsAtSpot = _npcRepository.GetNPCsForLocationSpotAndTime(spot.SpotID, currentTime);
            var npcNames = npcsAtSpot.Select(n => n.Name).ToList();
            
            // Build detail string with NPCs if present
            var detail = GetSpotDetail(spot);
            if (npcNames.Any())
            {
                detail = $"{detail} • {string.Join(", ", npcNames)}";
            }
            
            areas.Add(new AreaWithinLocationViewModel
            {
                Name = spot.Name,
                Detail = detail,
                SpotId = spot.SpotID,
                IsCurrent = false, // Never current since we skip the current spot
                IsTravelHub = spot.SpotID == location.TravelHubSpotId || spot.DomainTags?.Contains("Crossroads") == true
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
            _ => GenerateSpotDetail(spot)
        };
    }
    
    private string GenerateSpotDetail(LocationSpot spot)
    {
        if (spot == null) return "";
        
        var descGenerator = new Wayfarer.Game.MainSystem.SpotDescriptionGenerator();
        var activeProperties = spot.GetActiveProperties(_timeManager.GetCurrentTimeBlock());
        return descGenerator.GenerateBriefDescription(activeProperties);
    }
    
    private string GenerateAtmosphereText(LocationSpot spot, Location location)
    {
        if (spot != null)
        {
            var descGenerator = new Wayfarer.Game.MainSystem.SpotDescriptionGenerator();
            var activeProperties = spot.GetActiveProperties(_timeManager.GetCurrentTimeBlock());
            var urgentObligations = _letterQueueManager.GetActiveObligations()
                .Count(o => o.DeadlineInMinutes < 360);
            var npcsPresent = _npcRepository.GetNPCsForLocationSpotAndTime(spot.SpotID, _timeManager.GetCurrentTimeBlock()).Count();
            
            // Debug log
            Console.WriteLine($"[GenerateAtmosphereText] Spot: {spot.SpotID}, Properties: {string.Join(", ", activeProperties)}, Time: {_timeManager.GetCurrentTimeBlock()}");
            
            return descGenerator.GenerateDescription(
                activeProperties,
                _timeManager.GetCurrentTimeBlock(),
                urgentObligations,
                npcsPresent
            );
        }
        else if (location != null)
        {
            return location.Description ?? "An undefined location.";
        }
        
        return "An undefined location.";
    }
    
    private List<RouteOptionViewModel> GetRoutesFromLocation(Location location)
    {
        var routes = new List<RouteOptionViewModel>();
        
        // Get available routes
        var availableRoutes = _routeRepository.GetRoutesFromLocation(location.Id);
        
        foreach (var route in availableRoutes)
        {
            // Get destination location from the spot
            var destSpot = _gameWorld.WorldState.locationSpots.FirstOrDefault(s => s.SpotID == route.DestinationLocationSpot);
            var destination = destSpot != null ? _locationRepository.GetLocation(destSpot.LocationId) : null;
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
            // Default to Standard conversation type for now (should be passed from UI)
            var session = _conversationManager.StartConversation(npc.ID, ConversationType.Standard, null);
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

        // Generate location description from properties
        var currentLocation = _locationRepository.GetLocation(currentSpot.LocationId);
        var atmosphereText = GenerateAtmosphereText(currentSpot, currentLocation);
        if (!string.IsNullOrEmpty(atmosphereText))
        {
            messages.Add(atmosphereText);
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
        Console.WriteLine($"[ExecuteTravel] Looking for route: {intent.RouteId}");
        RouteOption route = _routeRepository.GetRouteById(intent.RouteId);

        if (route == null)
        {
            Console.WriteLine($"[ExecuteTravel] Route {intent.RouteId} not found!");
            _messageSystem.AddSystemMessage("Route not found", SystemMessageTypes.Danger);
            return false;
        }
        Console.WriteLine($"[ExecuteTravel] Found route: {route.Id} from {route.OriginLocationSpot} to {route.DestinationLocationSpot}");
        // Check if route is discovered
        if (!route.IsDiscovered)
        {
            _messageSystem.AddSystemMessage("You haven't discovered this route yet", SystemMessageTypes.Warning);
            return false;
        }

        // Calculate costs
        int staminaCost = _travelManager.CalculateStaminaCost(route);
        int timeCost = route.GetActualTimeCost();
        Console.WriteLine($"[ExecuteTravel] Time cost for route {route.Id}: {timeCost} minutes (TravelTimeMinutes: {route.TravelTimeMinutes})");

        // Check resources
        if (player.Stamina < staminaCost)
        {
            _messageSystem.AddSystemMessage($"Not enough stamina (need {staminaCost})", SystemMessageTypes.Warning);
            return false;
        }

        if (_timeManager.HoursRemaining * 60 < timeCost) // Convert hours to minutes for comparison
        {
            _messageSystem.AddSystemMessage($"Not enough time (need {timeCost} minutes)", SystemMessageTypes.Warning);
            return false;
        }

        if (player.Coins < route.BaseCoinCost)
        {
            _messageSystem.AddSystemMessage($"Not enough coins (need {route.BaseCoinCost})", SystemMessageTypes.Warning);
            return false;
        }

        // Execute travel
        player.SpendStamina(staminaCost);
        player.SpendMoney(route.BaseCoinCost);
        Console.WriteLine($"[ExecuteTravel] About to advance time by {timeCost} minutes");
        ProcessTimeAdvancementMinutes(timeCost); // timeCost is in MINUTES from route.TravelTimeMinutes
        Console.WriteLine($"[ExecuteTravel] Time advancement complete");

        // Get the destination spot
        Console.WriteLine($"[ExecuteTravel] Looking for destination spot: {route.DestinationLocationSpot}");
        LocationSpot targetSpot = _gameWorld.WorldState.locationSpots.FirstOrDefault(s => s.SpotID == route.DestinationLocationSpot);
        if (targetSpot == null)
        {
            Console.WriteLine($"[ExecuteTravel] Destination spot '{route.DestinationLocationSpot}' not found!");
            Console.WriteLine($"[ExecuteTravel] Available spots: {string.Join(", ", _gameWorld.WorldState.locationSpots.Select(s => s.SpotID))}");
            _messageSystem.AddSystemMessage($"Destination spot '{route.DestinationLocationSpot}' not found", SystemMessageTypes.Danger);
            return false;
        }
        Console.WriteLine($"[ExecuteTravel] Found destination spot: {targetSpot.SpotID} in location {targetSpot.LocationId}");
        
        // Get the location from the spot
        Location destination = _locationRepository.GetLocation(targetSpot.LocationId);
        if (destination == null)
        {
            _messageSystem.AddSystemMessage($"Destination location '{targetSpot.LocationId}' not found", SystemMessageTypes.Danger);
            return false;
        }
        
        // NO FALLBACKS - use the exact spot the route specifies
        _locationRepository.SetCurrentLocation(destination, targetSpot);
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

    /// <summary>
    /// Take an observation and generate a conversation card for the player
    /// Costs attention and can only be done once per time block
    /// </summary>
    public async Task<bool> TakeObservationAsync(string observationId)
    {
        try
        {
            // Get current attention state
            TimeBlocks currentTimeBlock = _timeManager.GetCurrentTimeBlock();
            AttentionManager attention = _timeBlockAttentionManager.GetCurrentAttention(currentTimeBlock);
            
            // Find the observation
            var currentLocation = GetCurrentLocation();
            var currentSpot = GetCurrentLocationSpot();
            var availableObservations = _observationSystem.GetObservationsForLocationSpot(currentLocation.Id, currentSpot.SpotID);
            var observation = availableObservations.FirstOrDefault(obs => obs.Id == observationId);
            
            if (observation == null)
            {
                _messageSystem.AddSystemMessage("That observation is no longer available.", SystemMessageTypes.Warning);
                return false;
            }

            // Check if already taken this time block
            if (_observationManager.HasTakenObservation(observationId))
            {
                _messageSystem.AddSystemMessage("You have already made that observation this time block.", SystemMessageTypes.Info);
                return false;
            }

            // Check attention cost
            if (attention.GetAvailableAttention() < observation.AttentionCost)
            {
                _messageSystem.AddSystemMessage($"Not enough attention (need {observation.AttentionCost})", SystemMessageTypes.Warning);
                return false;
            }

            // Spend attention
            if (!attention.TrySpend(observation.AttentionCost))
            {
                _messageSystem.AddSystemMessage("Failed to spend attention.", SystemMessageTypes.Warning);
                return false;
            }
            Console.WriteLine($"[GameFacade.TakeObservation] Spent {observation.AttentionCost} attention for observation {observationId}");

            // Generate observation card
            var observationCard = _observationManager.TakeObservation(observation, _connectionTokenManager);
            
            if (observationCard != null)
            {
                // Add message about gaining the card
                _messageSystem.AddSystemMessage($"Observed: {observation.Text}", SystemMessageTypes.Success);
                _messageSystem.AddSystemMessage($"Gained conversation card: {observationCard.Template}", SystemMessageTypes.Info);
                
                Console.WriteLine($"[GameFacade.TakeObservation] Successfully generated observation card {observationCard.Id}");
                return true;
            }
            else
            {
                _messageSystem.AddSystemMessage("Failed to process observation.", SystemMessageTypes.Warning);
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GameFacade.TakeObservation] Error taking observation {observationId}: {ex.Message}");
            _messageSystem.AddSystemMessage("Failed to make observation.", SystemMessageTypes.Warning);
            return false;
        }
    }

    /// <summary>
    /// Get available observations for the current location
    /// Returns a view model suitable for UI display
    /// </summary>
    public ObservationsViewModel GetObservationsViewModel()
    {
        Console.WriteLine("[GameFacade.GetObservationsViewModel] Method called");
        
        var viewModel = new ObservationsViewModel
        {
            AvailableObservations = new List<ObservationSummaryViewModel>()
        };

        // Get current location and spot
        var location = GetCurrentLocation();
        var spot = GetCurrentLocationSpot();
        
        Console.WriteLine($"[GameFacade.GetObservationsViewModel] Location: {location?.Id ?? "null"}, Spot: {spot?.SpotID ?? "null"}");
        
        if (location == null || spot == null)
        {
            Console.WriteLine("[GameFacade.GetObservationsViewModel] Returning empty - no location/spot");
            return viewModel;
        }

        // Get observations for current location
        var observations = GetLocationObservations(location.Id, spot.SpotID);
        Console.WriteLine($"[GameFacade.GetObservationsViewModel] Got {observations.Count} observations from GetLocationObservations");
        
        // Transform to summary view model for UI
        foreach (var obs in observations)
        {
            // Skip already observed items
            if (obs.IsObserved)
            {
                continue;
            }

            viewModel.AvailableObservations.Add(new ObservationSummaryViewModel
            {
                Id = obs.Id,
                Title = obs.Text, // Use Text as Title for display
                Type = obs.Relevance ?? "Observation" // Use Relevance as Type, or default
            });
        }

        return viewModel;
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
                .Where(r => 
                {
                    var spot = _gameWorld.WorldState.locationSpots.FirstOrDefault(s => s.SpotID == r.DestinationLocationSpot);
                    return spot != null && spot.LocationId == location.Id;
                })
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

    // ========== CONVERSATIONS ==========

    public async Task<ConversationContext> CreateConversationContext(string npcId, ConversationType conversationType = ConversationType.Standard)
    {
        // Create conversation context with all data needed atomically
        // This prevents race conditions and ensures complete data before navigation
        
        NPC npc = _npcRepository.GetById(npcId);
        if (npc == null) 
        {
            Console.WriteLine($"[GameFacade] NPC {npcId} not found in repository");
            return new ConversationContext
            {
                IsValid = false,
                ErrorMessage = $"NPC {npcId} not found"
            };
        }
        
        Player player = _gameWorld.GetPlayer();
        Location location = _locationRepository.GetCurrentLocation();
        
        // Check if NPC exists in game world
        var worldNpc = _gameWorld.NPCs.FirstOrDefault(n => n.ID == npcId);
        if (worldNpc == null)
        {
            Console.WriteLine($"[GameFacade] NPC {npcId} not found in GameWorld.NPCs");
            return new ConversationContext
            {
                IsValid = false,
                ErrorMessage = $"NPC {npcId} not found in game world"
            };
        }
            
        // Debug logging
        Console.WriteLine($"[GameFacade] Player.CurrentLocationSpot: {player.CurrentLocationSpot?.SpotID ?? "null"}");
        Console.WriteLine($"[GameFacade] NPC.SpotId: {worldNpc.SpotId}");
        
        // Check if NPC is at current spot
        if (player.CurrentLocationSpot == null || worldNpc.SpotId != player.CurrentLocationSpot.SpotID)
        {
            Console.WriteLine($"[GameFacade] NPC spot mismatch - conversation blocked");
            return new ConversationContext
            {
                IsValid = false,
                ErrorMessage = "NPC is not at your current location"
            };
        }
        
        // Check and deduct attention cost BEFORE starting conversation
        int attentionCost = ConversationTypeConfig.GetAttentionCost(conversationType);
        var currentTimeBlock = _timeManager.GetCurrentTimeBlock();
        var currentAttentionManager = _timeBlockAttentionManager.GetCurrentAttention(currentTimeBlock);
        
        // Check if player has enough attention
        if (!currentAttentionManager.CanAfford(attentionCost))
        {
            Console.WriteLine($"[GameFacade] Not enough attention for {conversationType} conversation. Cost: {attentionCost}, Available: {currentAttentionManager.GetAvailableAttention()}");
            _messageSystem.AddSystemMessage($"You don't have enough attention for this conversation (need {attentionCost}, have {currentAttentionManager.GetAvailableAttention()})", SystemMessageTypes.Warning);
            return new ConversationContext
            {
                IsValid = false,
                ErrorMessage = $"Not enough attention (need {attentionCost}, have {currentAttentionManager.GetAvailableAttention()})"
            };
        }
        
        // Deduct the attention cost
        if (attentionCost > 0)
        {
            bool spendSuccess = currentAttentionManager.TrySpend(attentionCost);
            if (!spendSuccess)
            {
                Console.WriteLine($"[GameFacade] Failed to spend attention for conversation");
                return new ConversationContext
                {
                    IsValid = false,
                    ErrorMessage = "Failed to spend attention for conversation"
                };
            }
            Console.WriteLine($"[GameFacade] Spent {attentionCost} attention for {conversationType} conversation. Remaining: {currentAttentionManager.GetAvailableAttention()}");
        }
            
        // Get observation cards for this conversation
        var observationCards = _observationManager.GetObservationCards();
        Console.WriteLine($"[GameFacade.CreateConversationContext] Including {observationCards.Count} observation cards in conversation");

        // Start the conversation session with observation cards
        var conversationSession = _conversationManager.StartConversation(npcId, conversationType, observationCards);
        
        // Create complete conversation context atomically
        var context = new ConversationContext
        {
            NpcId = npc.ID,
            Npc = npc,
            Type = conversationType,
            Session = conversationSession,
            ObservationCards = observationCards,
            AttentionSpent = attentionCost,
            InitialState = ConversationRules.DetermineInitialState(worldNpc, _letterQueueManager),
            PlayerResources = _gameWorld.GetPlayerResourceState(),
            LocationName = location.Name,
            TimeDisplay = _timeManager.GetFormattedTimeDisplay(),
            IsValid = true,
            ErrorMessage = null
        };
        
        return context;
    }

    public async Task<bool> ExecuteExchange(string npcId, ExchangeCard exchange)
    {
        // Validate inputs
        if (string.IsNullOrEmpty(npcId) || exchange == null)
        {
            Console.WriteLine("[ExecuteExchange] Invalid parameters");
            return false;
        }
        
        // Get NPC
        var npc = _npcRepository.GetById(npcId);
        if (npc == null)
        {
            Console.WriteLine($"[ExecuteExchange] NPC {npcId} not found");
            return false;
        }
        
        // Get player resources
        var player = _gameWorld.GetPlayer();
        var playerResources = _gameWorld.GetPlayerResourceState();
        
        // Check if player can afford
        var currentTimeBlock = _timeManager.GetCurrentTimeBlock();
        var currentAttention = _timeBlockAttentionManager.GetCurrentAttention(currentTimeBlock);
        if (!exchange.CanAfford(playerResources, _connectionTokenManager, currentAttention))
        {
            Console.WriteLine("[ExecuteExchange] Player cannot afford exchange");
            _messageSystem.AddSystemMessage("You don't have enough resources for this exchange", SystemMessageTypes.Warning);
            return false;
        }
        
        // Apply costs
        foreach (var cost in exchange.Cost)
        {
            switch (cost.Type)
            {
                case ResourceType.Coins:
                    player.Coins -= cost.Amount;
                    break;
                case ResourceType.Health:
                    player.Health -= cost.Amount;
                    break;
                case ResourceType.Attention:
                    // Attention is managed by TimeBlockAttentionManager
                    var costTimeBlock = _timeManager.GetCurrentTimeBlock();
                    var attentionMgr = _timeBlockAttentionManager.GetCurrentAttention(costTimeBlock);
                    if (!attentionMgr.TrySpend(cost.Amount))
                    {
                        Console.WriteLine($"[ExecuteExchange] Failed to spend {cost.Amount} attention");
                        return false;
                    }
                    break;
                case ResourceType.TrustToken:
                    _connectionTokenManager.SpendTokens(ConnectionType.Trust, cost.Amount);
                    break;
                case ResourceType.CommerceToken:
                    _connectionTokenManager.SpendTokens(ConnectionType.Commerce, cost.Amount);
                    break;
                case ResourceType.StatusToken:
                    _connectionTokenManager.SpendTokens(ConnectionType.Status, cost.Amount);
                    break;
                case ResourceType.ShadowToken:
                    _connectionTokenManager.SpendTokens(ConnectionType.Shadow, cost.Amount);
                    break;
            }
        }
        
        // Apply rewards
        foreach (var reward in exchange.Reward)
        {
            switch (reward.Type)
            {
                case ResourceType.Coins:
                    player.Coins += reward.Amount;
                    break;
                case ResourceType.Health:
                    if (reward.IsAbsolute)
                        player.Health = reward.Amount;
                    else
                        player.Health = Math.Min(100, player.Health + reward.Amount);
                    break;
                case ResourceType.Attention:
                    // Attention rewards add to current time block's attention
                    var rewardTimeBlock = _timeManager.GetCurrentTimeBlock();
                    var rewardAttentionMgr = _timeBlockAttentionManager.GetCurrentAttention(rewardTimeBlock);
                    if (reward.IsAbsolute)
                    {
                        // Set attention to specific value (for lodging/rest)
                        rewardAttentionMgr.SetAttention(reward.Amount);
                    }
                    else
                    {
                        // Add to current attention
                        rewardAttentionMgr.AddAttention(reward.Amount);
                    }
                    break;
                case ResourceType.Hunger:
                    // Hunger maps to Food (0 = not hungry, 100 = very hungry)
                    // So setting Hunger to 0 means setting Food to max
                    if (reward.IsAbsolute)
                        player.Food = reward.Amount == 0 ? 100 : (100 - reward.Amount);
                    else
                        player.Food = Math.Max(0, Math.Min(100, player.Food - reward.Amount));
                    break;
                case ResourceType.TrustToken:
                    _connectionTokenManager.AddTokensToNPC(ConnectionType.Trust, reward.Amount, npcId);
                    break;
                case ResourceType.CommerceToken:
                    _connectionTokenManager.AddTokensToNPC(ConnectionType.Commerce, reward.Amount, npcId);
                    break;
                case ResourceType.StatusToken:
                    _connectionTokenManager.AddTokensToNPC(ConnectionType.Status, reward.Amount, npcId);
                    break;
                case ResourceType.ShadowToken:
                    _connectionTokenManager.AddTokensToNPC(ConnectionType.Shadow, reward.Amount, npcId);
                    break;
                case ResourceType.Item:
                    // Add item to player inventory
                    if (!string.IsNullOrEmpty(reward.ItemId))
                    {
                        // First check if item exists or needs to be created
                        var item = _itemRepository.GetItemById(reward.ItemId);
                        if (item == null && reward.ItemId == "fine_silk")
                        {
                            // Create Fine Silk item on-demand if it doesn't exist
                            item = new Item
                            {
                                Id = "fine_silk",
                                Name = "Fine Silk",
                                Weight = 1,
                                BuyPrice = 15,
                                SellPrice = 10,
                                InventorySlots = 1,
                                Size = SizeCategory.Small,
                                Categories = new List<ItemCategory> { ItemCategory.Luxury_Items, ItemCategory.Trade_Goods },
                                Description = "A bolt of exquisite silk fabric, highly valued by nobles and traders"
                            };
                            _itemRepository.AddItem(item);
                        }
                        
                        if (item != null)
                        {
                            // Add item to player inventory
                            for (int i = 0; i < reward.Amount; i++)
                            {
                                if (!player.Inventory.AddItem(reward.ItemId))
                                {
                                    _messageSystem.AddSystemMessage($"Your inventory is full! Could not receive {item.Name}.", SystemMessageTypes.Warning);
                                    break;
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine($"[ExecuteExchange] Warning: Item '{reward.ItemId}' not found");
                        }
                    }
                    break;
            }
        }
        
        // Generate narrative message
        var narrativeContext = exchange.GetNarrativeContext();
        _messageSystem.AddSystemMessage($"You {narrativeContext} with {npc.Name}", SystemMessageTypes.Success);
        
        // Log for debugging
        Console.WriteLine($"[ExecuteExchange] Completed exchange {exchange.Id} with {npc.Name}");
        
        return true;
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

    // Market action enum for categorical mapping
    private enum MarketAction
    {
        Buy,
        Sell
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


    // ========== NARRATIVE/TUTORIAL ==========

    public bool IsTutorialActive()
    {
        return _flagService.HasFlag("tutorial_active");
    }


    // ========== GAME FLOW ==========

    public async Task StartGameAsync()
    {
        // Game already started during initialization
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


    // ========== NPC & LOCATION QUERIES ==========

    public NPC GetNPCById(string npcId)
    {
        return _npcRepository?.GetById(npcId);
    }

    public List<NPC> GetNPCsAtCurrentSpot()
    {
        var player = _gameWorld.GetPlayer();
        if (player?.CurrentLocationSpot == null) return new List<NPC>();
        
        var currentTime = _timeManager.GetCurrentTimeBlock();
        return _npcRepository.GetNPCsForLocationSpotAndTime(player.CurrentLocationSpot.SpotID, currentTime);
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
