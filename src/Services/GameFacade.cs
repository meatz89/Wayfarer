using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Wayfarer.GameState;
using Wayfarer.GameState.Constants;
using Wayfarer.Services;
using Wayfarer.ViewModels;

/// <summary>
/// GameFacade - THE single entry point for all UI-Backend communication.
/// This class delegates to existing UIServices and managers to maintain clean separation.
/// </summary>
public class GameFacade : ILetterQueueOperations
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
    private readonly LetterQueueManager _letterQueueManager;
    private readonly RouteDiscoveryManager _routeDiscoveryManager;

    // Domain services
    private readonly ConversationFactory _conversationFactory;
    private readonly NPCRepository _npcRepository;
    private readonly LocationRepository _locationRepository;
    private readonly RouteRepository _routeRepository;
    private readonly FlagService _flagService;
    private readonly ItemRepository _itemRepository;
    private readonly ConversationStateManager _conversationStateManager;
    private readonly TokenMechanicsManager _connectionTokenManager;
    private readonly NPCLetterOfferService _letterOfferService;
    private readonly GameConfiguration _gameConfiguration;
    private readonly InformationDiscoveryManager _informationDiscoveryManager;
    private readonly StandingObligationManager _standingObligationManager;
    private readonly StandingObligationRepository _standingObligationRepository;
    private readonly MarketManager _marketManager;
    private readonly LetterCategoryService _letterCategoryService;
    private readonly SpecialLetterGenerationService _specialLetterService;
    private readonly MorningActivitiesManager _morningActivitiesManager;
    private readonly DeliveryConversationService _deliveryConversationService;
    private readonly EndorsementManager _endorsementManager;
    private readonly NoticeBoardService _noticeBoardService;
    private readonly LetterTemplateRepository _letterTemplateRepository;
    private readonly InformationRevealService _informationRevealService;
    private readonly ContextTagCalculator _contextTagCalculator;
    private readonly NPCStateResolver _npcStateResolver;
    private readonly EnvironmentalHintSystem _environmentalHintSystem;
    private readonly ObservationSystem _observationSystem;
    private readonly ActionGenerator _actionGenerator;
    private readonly ActionBeatGenerator _actionBeatGenerator;
    private readonly BindingObligationSystem _bindingObligationSystem;
    private readonly AtmosphereCalculator _atmosphereCalculator;
    private readonly TimeBlockAttentionManager _timeBlockAttentionManager;
    private readonly NPCDeckFactory _deckFactory;
    private readonly WorldMemorySystem _worldMemorySystem;
    private readonly AmbientDialogueSystem _ambientDialogueSystem;

    public GameFacade(
        GameWorld gameWorld,
        ITimeManager timeManager,
        MessageSystem messageSystem,
        TravelManager travelManager,
        LetterQueueManager letterQueueManager,
        RouteDiscoveryManager routeDiscoveryManager,
        ConversationFactory conversationFactory,
        NPCRepository npcRepository,
        LocationRepository locationRepository,
        RouteRepository routeRepository,
        FlagService flagService,
        ItemRepository itemRepository,
        ConversationStateManager conversationStateManager,
        TokenMechanicsManager connectionTokenManager,
        NPCLetterOfferService letterOfferService,
        GameConfiguration gameConfiguration,
        InformationDiscoveryManager informationDiscoveryManager,
        StandingObligationManager standingObligationManager,
        StandingObligationRepository standingObligationRepository,
        MarketManager marketManager,
        RestManager restManager,
        IGameRuleEngine ruleEngine,
        LetterCategoryService letterCategoryService,
        SpecialLetterGenerationService specialLetterService,
        MorningActivitiesManager morningActivitiesManager,
        DeliveryConversationService deliveryConversationService,
        EndorsementManager endorsementManager,
        NoticeBoardService noticeBoardService,
        LetterTemplateRepository letterTemplateRepository,
        InformationRevealService informationRevealService,
        ContextTagCalculator contextTagCalculator,
        NPCStateResolver npcStateResolver,
        ActionGenerator actionGenerator,
        ActionBeatGenerator actionBeatGenerator,
        EnvironmentalHintSystem environmentalHintSystem,
        ObservationSystem observationSystem,
        BindingObligationSystem bindingObligationSystem,
        AtmosphereCalculator atmosphereCalculator,
        WorldMemorySystem worldMemorySystem,
        AmbientDialogueSystem ambientDialogueSystem,
        TimeBlockAttentionManager timeBlockAttentionManager,
        NPCDeckFactory deckFactory
)
    {
        _gameWorld = gameWorld;
        _timeManager = timeManager;
        _messageSystem = messageSystem;
        _travelManager = travelManager;
        _letterQueueManager = letterQueueManager;
        _routeDiscoveryManager = routeDiscoveryManager;
        _conversationFactory = conversationFactory;
        _npcRepository = npcRepository;
        _locationRepository = locationRepository;
        _routeRepository = routeRepository;
        _flagService = flagService;
        _itemRepository = itemRepository;
        _conversationStateManager = conversationStateManager;
        _connectionTokenManager = connectionTokenManager;
        _letterOfferService = letterOfferService;
        _gameConfiguration = gameConfiguration;
        _informationDiscoveryManager = informationDiscoveryManager;
        _standingObligationManager = standingObligationManager;
        _standingObligationRepository = standingObligationRepository;
        _marketManager = marketManager;
        _restManager = restManager;
        _ruleEngine = ruleEngine;
        _letterCategoryService = letterCategoryService;
        _specialLetterService = specialLetterService;
        _morningActivitiesManager = morningActivitiesManager;
        _deliveryConversationService = deliveryConversationService;
        _endorsementManager = endorsementManager;
        _noticeBoardService = noticeBoardService;
        _letterTemplateRepository = letterTemplateRepository;
        _informationRevealService = informationRevealService;
        _contextTagCalculator = contextTagCalculator;
        _npcStateResolver = npcStateResolver;
        _actionGenerator = actionGenerator;
        _actionBeatGenerator = actionBeatGenerator;
        _environmentalHintSystem = environmentalHintSystem;
        _observationSystem = observationSystem;
        _bindingObligationSystem = bindingObligationSystem;
        _atmosphereCalculator = atmosphereCalculator;
        _worldMemorySystem = worldMemorySystem;
        _ambientDialogueSystem = ambientDialogueSystem;

        // Use injected TimeBlockAttentionManager (shared with ConversationFactory)
        _timeBlockAttentionManager = timeBlockAttentionManager;
        _deckFactory = deckFactory;
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
        _informationRevealService?.ProcessCarriedInformation();
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
        _informationRevealService?.ProcessCarriedInformation();
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

        // Add letter weights
        foreach (Letter letter in player.LetterQueue)
        {
            if (letter != null && letter.State == LetterState.Collected)
            {
                totalWeight += letter.CarryWeight;
            }
        }

        return totalWeight;
    }

    // ========== GAME STATE QUERIES ==========

    public GameWorldSnapshot GetGameSnapshot()
    {
        return new GameWorldSnapshot(_gameWorld, _conversationStateManager);
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
                AcceptLetterOfferIntent offer => await ExecuteAcceptOffer(offer),
                TravelIntent travel => await ExecuteTravel(travel),
                DiscoverRouteIntent discover => await ExecuteDiscoverRoute(discover),
                ConvertEndorsementsIntent convert => await ExecuteConvertEndorsements(convert),
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
            // Create conversation context
            Location location = _locationRepository.GetCurrentLocation();
            LocationSpot? spot = player.CurrentLocationSpot;
            SceneContext context = SceneContext.Standard(_gameWorld, player, npc, location, spot);

            // Create conversation - this should always succeed with fallback content
            ConversationManager conversation = await _conversationFactory.CreateConversation(context, player);
            if (conversation == null)
            {
                // This should never happen with proper fallback, but handle it anyway
                _messageSystem.AddSystemMessage($"Failed to create conversation with {npc.Name}", SystemMessageTypes.Warning);
                return false;
            }

            // Set conversation state
            _conversationStateManager.SetCurrentConversation(conversation);

            // Verify the conversation was set
            if (!_conversationStateManager.ConversationPending || _conversationStateManager.PendingConversationManager == null)
            {
                _messageSystem.AddSystemMessage($"Failed to start conversation with {npc.Name} - state not set", SystemMessageTypes.Danger);
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
                if (slot.IsOccupied && slot.Letter?.DeadlineInHours <= 0)
                {
                    _messageSystem.AddSystemMessage($"Letter to {slot.Letter.RecipientName} has expired!", SystemMessageTypes.Danger);
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
        Letter letterToDeliver = null;
        int letterPosition = -1;

        for (int i = 0; i < player.LetterQueue.Length; i++)
        {
            if (player.LetterQueue[i]?.Id == intent.LetterId)
            {
                letterToDeliver = player.LetterQueue[i];
                letterPosition = i + 1; // Queue positions are 1-indexed
                break;
            }
        }

        if (letterToDeliver == null)
        {
            _messageSystem.AddSystemMessage("Letter not found in your queue", SystemMessageTypes.Warning);
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

        // Generate delivery conversation context
        DeliveryConversationContext conversationContext = _deliveryConversationService.AnalyzeDeliveryContext(letterToDeliver, recipient);
        List<ConversationChoice> choices = _deliveryConversationService.GenerateDeliveryChoices(conversationContext);

        // For now, execute standard delivery (first choice)
        ConversationChoice? standardChoice = choices.FirstOrDefault();
        if (standardChoice == null)
        {
            _messageSystem.AddSystemMessage("No delivery options available", SystemMessageTypes.Warning);
            return false;
        }

        DeliveryOutcome outcome = standardChoice.DeliveryOutcome;

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

        // Handle special letter types
        if (letterToDeliver.SpecialType == LetterSpecialType.Endorsement && _endorsementManager != null)
        {
            // For now, just log endorsement delivery
            _messageSystem.AddSystemMessage("Endorsement successfully delivered!", SystemMessageTypes.Success);
        }

        // Process patron leverage
        if (outcome.ReducesLeverage > 0)
        {
            player.PatronLeverage = Math.Max(0, player.PatronLeverage - outcome.ReducesLeverage);
            _messageSystem.AddSystemMessage($"Patron leverage reduced by {outcome.ReducesLeverage}", SystemMessageTypes.Success);
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
        Player player = _gameWorld.GetPlayer();

        // Check if queue has space
        if (_letterQueueManager.GetLetterCount() >= _gameConfiguration.LetterQueue.MaxQueueSize)
        {
            _messageSystem.AddSystemMessage("Your letter queue is full!", SystemMessageTypes.Warning);
            return false;
        }

        // For now, use the notice board service to generate a letter
        // In the future, this could work with pre-generated offers
        Letter? letter = _noticeBoardService?.UseNoticeBoard(NoticeBoardService.NoticeBoardOption.AnythingHeading);

        if (letter == null)
        {
            _messageSystem.AddSystemMessage("No letters available on the board", SystemMessageTypes.Warning);
            return false;
        }

        // Add letter to queue
        int position = _letterQueueManager.AddLetter(letter);
        if (position > 0)
        {
            _messageSystem.AddSystemMessage($"Collected letter from {letter.SenderName} to {letter.RecipientName}", SystemMessageTypes.Success);
            _messageSystem.AddSystemMessage($"Letter added to queue position {position}", SystemMessageTypes.Info);
            return true;
        }
        else
        {
            _messageSystem.AddSystemMessage("Failed to add letter to queue", SystemMessageTypes.Warning);
            return false;
        }
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
        Player player = _gameWorld.GetPlayer();

        // Check if player has a patron
        if (!player.HasPatron)
        {
            _messageSystem.AddSystemMessage("You don't have a patron to request funds from", SystemMessageTypes.Warning);
            return false;
        }

        // Check cooldown (7 days between requests)
        int daysSinceLastRequest = _gameWorld.CurrentDay - player.LastPatronFundDay;
        const int PATRON_FUND_COOLDOWN = 7;

        if (daysSinceLastRequest < PATRON_FUND_COOLDOWN)
        {
            int daysRemaining = PATRON_FUND_COOLDOWN - daysSinceLastRequest;
            _messageSystem.AddSystemMessage($"Your patron won't provide funds again for {daysRemaining} more days", SystemMessageTypes.Warning);
            return false;
        }

        // Grant funds (10 coins as emergency support)
        const int PATRON_FUND_AMOUNT = 10;
        player.ModifyCoins(PATRON_FUND_AMOUNT);
        player.LastPatronFundDay = _gameWorld.CurrentDay;

        // Increase patron leverage (they're helping you, so you owe them more)
        const int LEVERAGE_INCREASE = 3;
        player.PatronLeverage += LEVERAGE_INCREASE;

        _messageSystem.AddSystemMessage($"Your patron provides {PATRON_FUND_AMOUNT} coins of emergency funding", SystemMessageTypes.Success);
        _messageSystem.AddSystemMessage($"Their leverage over you increases by {LEVERAGE_INCREASE}", SystemMessageTypes.Warning);
        _messageSystem.AddSystemMessage($"Patron leverage is now {player.PatronLeverage}", SystemMessageTypes.Info);

        // If patron leverage is high, warn the player
        if (player.PatronLeverage >= 10)
        {
            _messageSystem.AddSystemMessage("Your patron's grip tightens. Their letters will demand even higher priority", SystemMessageTypes.Danger);
        }

        return true;
    }

    private async Task<bool> ExecuteAcceptOffer(AcceptLetterOfferIntent intent)
    {
        Player player = _gameWorld.GetPlayer();

        // Check if queue has space
        if (_letterQueueManager.GetLetterCount() >= _gameConfiguration.LetterQueue.MaxQueueSize)
        {
            _messageSystem.AddSystemMessage("Your letter queue is full!", SystemMessageTypes.Warning);
            return false;
        }

        // Parse offer ID to get NPC ID and offer details
        // Offer ID format: "npc_id|offer_guid"
        string[] parts = intent.OfferId.Split('|');
        if (parts.Length != 2)
        {
            _messageSystem.AddSystemMessage("Invalid offer ID", SystemMessageTypes.Warning);
            return false;
        }

        string npcId = parts[0];
        string offerId = parts[1];

        // Get the NPC
        NPC npc = _npcRepository.GetById(npcId);
        if (npc == null)
        {
            _messageSystem.AddSystemMessage("NPC not found", SystemMessageTypes.Warning);
            return false;
        }

        // Get pending offers for this NPC
        List<LetterOffer> offers = _letterOfferService.GetPendingOffersForNPC(npcId);
        LetterOffer? offer = offers.FirstOrDefault(o => o.Id == offerId);

        if (offer == null)
        {
            _messageSystem.AddSystemMessage("Letter offer no longer available", SystemMessageTypes.Warning);
            return false;
        }

        // Find a suitable recipient
        List<NPC> possibleRecipients = _npcRepository.GetAllNPCs()
            .Where(n => n.ID != npcId && n.SpotId != player.CurrentLocationSpot?.SpotID)
            .ToList();

        if (!possibleRecipients.Any())
        {
            _messageSystem.AddSystemMessage("No suitable recipients available for this letter", SystemMessageTypes.Warning);
            return false;
        }

        Random random = new Random();
        NPC recipient = possibleRecipients[random.Next(possibleRecipients.Count)];

        // Create letter from offer
        Letter letter = new Letter
        {
            Id = Guid.NewGuid().ToString(),
            SenderName = offer.NPCName,
            SenderId = npcId,
            RecipientName = recipient.Name,
            RecipientId = recipient.ID,
            Payment = offer.Payment,
            DeadlineInHours = offer.DeadlineInHours,
            DaysInQueue = 0,
            QueuePosition = 0,
            TokenType = offer.LetterType,
            Description = $"Letter from {offer.NPCName} to {recipient.Name}"
        };

        // Add letter to queue
        int position = _letterQueueManager.AddLetter(letter);
        if (position > 0)
        {
            _messageSystem.AddSystemMessage($"Accepted letter from {offer.NPCName}", SystemMessageTypes.Success);
            _messageSystem.AddSystemMessage($"Deliver to {letter.RecipientName}", SystemMessageTypes.Info);
            _messageSystem.AddSystemMessage($"Payment: {letter.Payment} coins", SystemMessageTypes.Info);

            // Remove the accepted offer
            // Note: NPCLetterOfferService would need a method to remove offers
            // For now, the offer will expire naturally

            return true;
        }
        else
        {
            _messageSystem.AddSystemMessage("Failed to add letter to queue", SystemMessageTypes.Warning);
            return false;
        }
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

    private async Task<bool> ExecuteConvertEndorsements(ConvertEndorsementsIntent intent)
    {
        Player player = _gameWorld.GetPlayer();
        LocationSpot currentLocation = player.CurrentLocationSpot;

        if (currentLocation == null || currentLocation.LocationId != intent.LocationId)
        {
            _messageSystem.AddSystemMessage("You must be at the guild to convert endorsements", SystemMessageTypes.Warning);
            return false;
        }

        // Check if this is actually a guild location by ID
        string[] validGuildLocations = new[] { "merchant_guild", "messenger_guild", "scholar_guild" };
        if (!validGuildLocations.Contains(intent.LocationId))
        {
            _messageSystem.AddSystemMessage("This isn't a guild location", SystemMessageTypes.Warning);
            return false;
        }

        // Parse target tier
        if (!Enum.TryParse<SealTier>(intent.TargetTier, out SealTier targetTier))
        {
            _messageSystem.AddSystemMessage("Invalid seal tier specified", SystemMessageTypes.Warning);
            return false;
        }

        // Get available conversions at this guild
        List<SealConversionOption> conversions = _endorsementManager.GetAvailableSealConversions(intent.LocationId);
        SealConversionOption? targetConversion = conversions.FirstOrDefault(c => c.TargetTier == targetTier);

        if (targetConversion == null)
        {
            _messageSystem.AddSystemMessage("That seal tier is not available for conversion", SystemMessageTypes.Warning);
            return false;
        }

        if (!targetConversion.CanConvert)
        {
            _messageSystem.AddSystemMessage($"You need {targetConversion.RequiredEndorsements} endorsements. You have {targetConversion.CurrentEndorsements}.", SystemMessageTypes.Warning);
            return false;
        }

        // Show conversion dialog
        _messageSystem.AddSystemMessage($"🏛️ Presenting your endorsements to the {targetConversion.GuildName}...", SystemMessageTypes.Info);

        // Perform the conversion
        bool success = _endorsementManager.ConvertEndorsementsToSeal(intent.LocationId, targetTier);

        if (success)
        {
            // Add narrative flavor based on seal type and tier
            SealType sealType = _endorsementManager.GetSealTypeForGuild(intent.LocationId);
            string narrative = GetSealConversionNarrative(sealType, targetTier);
            _messageSystem.AddSystemMessage(narrative, SystemMessageTypes.Info);

            // Time passes during ceremony
            ProcessTimeAdvancement(1);

            // Check for new opportunities
            CheckNewSealOpportunities(sealType, targetTier);
        }

        return success;
    }

    private string GetSealConversionNarrative(SealType type, SealTier tier)
    {
        return (type, tier) switch
        {
            (SealType.Commerce, SealTier.Apprentice) => "The guild master reviews your endorsements and nods approvingly. 'Welcome to the Merchant's Guild, apprentice.'",
            (SealType.Commerce, SealTier.Journeyman) => "After careful examination of your record, the guild council promotes you. 'Your business acumen has proven itself.'",
            (SealType.Commerce, SealTier.Master) => "The grand ceremony concludes with thunderous applause. 'Rise, Master Merchant, and take your place among the elite.'",
            (SealType.Status, SealTier.Apprentice) => "The scholars examine your credentials carefully. 'Your intellectual pursuits have earned recognition.'",
            (SealType.Status, SealTier.Journeyman) => "The academic council deliberates briefly. 'Your contributions to knowledge are noted. Welcome, Scholar.'",
            (SealType.Status, SealTier.Master) => "In a solemn ceremony, you receive the highest academic honor. 'May your wisdom guide others.'",
            (SealType.Shadow, SealTier.Apprentice) => "A hooded figure accepts your tokens. 'The shadows acknowledge your service.'",
            (SealType.Shadow, SealTier.Journeyman) => "Without ceremony, a new seal appears in your possession. 'Your discretion has been noted.'",
            (SealType.Shadow, SealTier.Master) => "In absolute silence, you are inducted into the inner circle. No words are needed.",
            _ => "The guild acknowledges your achievements with a new seal."
        };
    }

    private void CheckNewSealOpportunities(SealType type, SealTier tier)
    {
        // Check for newly available routes/locations/NPCs based on seal
        if (tier >= SealTier.Journeyman)
        {
            _messageSystem.AddSystemMessage("🔓 Your new seal may grant access to restricted areas and exclusive opportunities.", SystemMessageTypes.Info);
        }

        if (tier == SealTier.Master)
        {
            _messageSystem.AddSystemMessage("👑 As a Master, you now have significant influence in guild matters.", SystemMessageTypes.Info);
        }
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
                TravelTimeHours = route.TravelTimeHours,
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
                MinimumTime = dest.AvailableRoutes.Where(r => !r.IsBlocked).Select(r => r.TravelTimeHours).DefaultIfEmpty(0).Min()
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
                TimeCost = route.TravelTimeHours,
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
            TimeCost = route.TravelTimeHours,
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
        // Card-based conversation system handles all conversation types

        // Get NPC and create context
        NPC npc = _npcRepository.GetById(npcId);
        Player player = _gameWorld.GetPlayer();
        Location location = _locationRepository.GetCurrentLocation();
        LocationSpot spot = player.CurrentLocationSpot;

        // NPCs should have letters to offer when starting conversations
        // This creates the core tension: accepting letters fills your queue
        if (npc != null)
        {
            npc.HasLetterToOffer = true; // NPCs always have something to ask of you
        }

        SceneContext context = SceneContext.Standard(_gameWorld, player, npc, location, spot);

        // CRITICAL: Use persistent attention from time block manager
        TimeBlocks currentTimeBlock = _timeManager.GetCurrentTimeBlock();
        context.AttentionManager = _timeBlockAttentionManager.GetCurrentAttention(currentTimeBlock);
        Console.WriteLine($"[StartConversationAsync] Using time-block attention for {currentTimeBlock}: {context.AttentionManager.GetAvailableAttention()}/{context.AttentionManager.GetMaxAttention()}");

        // Start conversation directly
        ConversationManager conversation = await _conversationFactory.CreateConversation(context, player);
        if (conversation != null)
        {
            _conversationStateManager.SetCurrentConversation(conversation);
        }
        ConversationManager conversationManager = _conversationStateManager.PendingConversationManager;
        if (conversationManager == null) return null;

        return CreateConversationViewModel(conversationManager);
    }

    public async Task<ConversationViewModel> ContinueConversationAsync(string choiceId)
    {
        // Card-based conversation system handles all response types

        ConversationManager currentConversation = _conversationStateManager.PendingConversationManager;
        if (currentConversation == null || !_conversationStateManager.ConversationPending) return null;

        ConversationChoice? choice = currentConversation.Choices?.FirstOrDefault(c => c.ChoiceID == choiceId);
        if (choice == null) return null;

        // Process the choice
        ConversationBeatOutcome outcome = await currentConversation.ProcessPlayerChoice(choice);

        // If conversation is complete, clear it from state manager
        if (outcome.IsConversationComplete)
        {
            _conversationStateManager.ClearPendingConversation();
        }
        else
        {
            // Generate new choices for the next beat
            await currentConversation.ProcessNextBeat();
        }

        return CreateConversationViewModel(currentConversation);
    }


    public ConversationViewModel GetCurrentConversation()
    {
        Console.WriteLine($"[GameFacade.GetCurrentConversation] Called");
        Console.WriteLine($"[GameFacade.GetCurrentConversation] ConversationPending: {_conversationStateManager.ConversationPending}");
        Console.WriteLine($"[GameFacade.GetCurrentConversation] PendingConversationManager null? {_conversationStateManager.PendingConversationManager == null}");

        ConversationManager? currentConversation = _conversationStateManager.PendingConversationManager;
        if (currentConversation == null || !_conversationStateManager.ConversationPending)
        {
            Console.WriteLine($"[GameFacade.GetCurrentConversation] Returning null - no pending conversation");
            return null;
        }

        ConversationViewModel? viewModel = CreateConversationViewModel(currentConversation);
        Console.WriteLine($"[GameFacade.GetCurrentConversation] Created ViewModel with NpcId: {viewModel?.NpcId}, Text: {viewModel?.CurrentText?.Substring(0, Math.Min(50, viewModel?.CurrentText?.Length ?? 0))}");
        return viewModel;
    }

    public ConversationManager GetCurrentConversationManager()
    {
        return _conversationStateManager.PendingConversationManager;
    }

    public async Task<ConversationViewModel> ProcessConversationChoice(string choiceId)
    {
        Console.WriteLine($"[GameFacade.ProcessConversationChoice] Processing choice: {choiceId}");

        ConversationManager? currentConversation = _conversationStateManager.PendingConversationManager;
        if (currentConversation == null || !_conversationStateManager.ConversationPending)
        {
            Console.WriteLine($"[GameFacade.ProcessConversationChoice] No active conversation");
            return null;
        }

        // Find the selected choice
        ConversationChoice? selectedChoice = currentConversation.Choices?.FirstOrDefault(c => c.ChoiceID == choiceId);
        if (selectedChoice == null)
        {
            Console.WriteLine($"[GameFacade.ProcessConversationChoice] Choice not found: {choiceId}");
            return GetCurrentConversation();
        }

        Console.WriteLine($"[GameFacade.ProcessConversationChoice] Found choice: {selectedChoice.NarrativeText}, PatienceCost: {selectedChoice.PatienceCost}");

        // Track time before processing to handle any ConversationTimeEffects
        int hoursBefore = _timeManager.GetCurrentTimeHours();
        int minutesBefore = _timeManager.GetCurrentMinutes();
        int dayBefore = _timeManager.GetCurrentDay();

        // Process the choice and get the outcome
        ConversationBeatOutcome outcome = await currentConversation.ProcessPlayerChoice(selectedChoice);

        // Check if time advanced during choice processing (from ConversationTimeEffect)
        int hoursAfter = _timeManager.GetCurrentTimeHours();
        int minutesAfter = _timeManager.GetCurrentMinutes();
        int dayAfter = _timeManager.GetCurrentDay();

        // Calculate time difference and process letter deadlines if time advanced
        int totalMinutesAdvanced = 0;
        if (dayAfter > dayBefore)
        {
            // Crossed day boundary
            totalMinutesAdvanced = ((dayAfter - dayBefore) * 24 * 60) + ((hoursAfter - hoursBefore) * 60) + (minutesAfter - minutesBefore);
        }
        else
        {
            totalMinutesAdvanced = ((hoursAfter - hoursBefore) * 60) + (minutesAfter - minutesBefore);
        }

        if (totalMinutesAdvanced > 0)
        {
            // ConversationTimeEffect advanced time - process letter deadlines
            int hoursAdvanced = totalMinutesAdvanced / 60;
            if (hoursAdvanced > 0)
            {
                _letterQueueManager.ProcessHourlyDeadlines(hoursAdvanced);
            }
        }

        Console.WriteLine($"[GameFacade.ProcessConversationChoice] Choice processed. Conversation complete: {outcome.IsConversationComplete}");

        // If conversation is complete, check comfort thresholds and generate letters
        if (outcome.IsConversationComplete)
        {
            await ProcessConversationCompletion(currentConversation);
            _conversationStateManager.ClearPendingConversation();
            Console.WriteLine($"[GameFacade.ProcessConversationChoice] Conversation completed and cleared");
            return null;
        }

        // Generate new choices for the next beat
        await currentConversation.ProcessNextBeat();

        // Return updated view model
        return CreateConversationViewModel(currentConversation);
    }

    private ConversationViewModel CreateConversationViewModel(ConversationManager conversation)
    {
        SceneContext? context = conversation.Context;
        NPC? npc = conversation.Context?.TargetNPC;

        // Use ContextTagCalculator to populate tags if available
        if (_contextTagCalculator != null && context != null)
        {
            _contextTagCalculator.PopulateContextTags(context);
        }

        // Calculate NPC emotional state from letter queue
        NPCEmotionalState npcState = NPCEmotionalState.WITHDRAWN;
        if (_npcStateResolver != null && npc != null)
        {
            npcState = _npcStateResolver.CalculateState(npc);
        }

        // Get attention information
        int currentAttention = context?.AttentionManager?.Current ?? 3;
        int maxAttention = context?.AttentionManager?.Max ?? 3;

        // Generate attention narrative based on remaining points
        string attentionNarrative = currentAttention switch
        {
            3 => "Your mind is clear and focused, ready to absorb every detail.",
            2 => "You remain attentive, though some of your focus has been spent.",
            1 => "Your concentration wavers. You must choose your focus carefully.",
            0 => "Mental fatigue clouds your thoughts. You can only respond simply.",
            _ => ""
        };

        // Get the most urgent letter for this NPC (used in multiple places)
        Letter urgentLetter = null;
        if (npc != null)
        {
            urgentLetter = _letterQueueManager.GetActiveLetters()
                .Where(l => l.SenderId == npc.ID || l.SenderName == npc.Name)
                .OrderBy(l => l.DeadlineInHours)
                .FirstOrDefault();
        }

        // Generate body language description based on NPC emotional state
        string bodyLanguage = "";
        if (_npcStateResolver != null && npc != null)
        {
            StakeType stakes = urgentLetter?.Stakes ?? StakeType.REPUTATION;
            bodyLanguage = _npcStateResolver.GenerateBodyLanguage(npcState, stakes);
        }
        else
        {
            bodyLanguage = GenerateBodyLanguageFromTags(context);
        }

        // Generate peripheral observations
        List<string> peripheralObservations = GeneratePeripheralObservations(context);

        // Use EnvironmentalHintSystem for deadline pressure
        string deadlinePressure = _environmentalHintSystem?.GetDeadlinePressure() ?? "";

        // Use EnvironmentalHintSystem for environmental hints
        List<string> environmentalHints = new List<string>();
        if (!string.IsNullOrEmpty(context?.LocationName))
        {
            string? hint = _environmentalHintSystem?.GetEnvironmentalHint(context.LocationName.ToLower().Replace(" ", "_"));
            if (!string.IsNullOrEmpty(hint))
            {
                environmentalHints.Add(hint);
            }
        }

        // Get location atmosphere
        string locationAtmosphere = "";
        if (!string.IsNullOrEmpty(context?.LocationSpotName))
        {
            locationAtmosphere = context.LocationSpotName.Contains("Kettle")
                ? "Warm hearth-light, nervous energy in the air"
                : "The usual bustle of activity";
        }

        // Get the current narrative text, with fallback
        string currentText = conversation.State?.CurrentNarrative;
        if (string.IsNullOrWhiteSpace(currentText))
        {
            // Fallback narrative if none was generated
            currentText = $"{conversation.Context.TargetNPC.Name} looks at you expectantly, waiting for you to speak.";
        }

        return new ConversationViewModel
        {
            NpcName = conversation.Context.TargetNPC.Name,
            NpcId = conversation.Context.TargetNPC.ID,
            CurrentText = currentText,

            // Emotional State (from NPCStateResolver)
            EmotionalState = npcState,
            CurrentStakes = urgentLetter?.Stakes,
            HoursToDeadline = urgentLetter?.DeadlineInHours,

            Choices = conversation.Choices?.Select(c => new ConversationChoiceViewModel
            {
                Id = c.ChoiceID,
                Text = c.NarrativeText,
                IsAvailable = c.IsAffordable,
                UnavailableReason = !c.IsAffordable ? $"Requires {c.PatienceCost} attention" : null,
                PatienceCost = c.PatienceCost,
                PatienceDisplay = GetAttentionDisplayString(c.PatienceCost),
                PatienceDescription = GetAttentionDescription(c.PatienceCost),
                IsInternalThought = c.NarrativeText.StartsWith("*") || c.TemplatePurpose?.Contains("INTERNAL") == true,
                EmotionalTone = DetermineEmotionalTone(c),
                IsLocked = c.IsLocked,
                // Convert mechanical effects to display mechanics
                Mechanics = ConvertMechanicalEffectsToDisplay(c.MechanicalEffects)
            }).ToList() ?? new List<ConversationChoiceViewModel>(),
            IsComplete = conversation.State?.IsConversationComplete ?? false,
            ConversationTopic = conversation.Context.ConversationTopic,

            // Literary UI properties
            CurrentAttention = currentAttention,
            MaxAttention = maxAttention,
            AttentionNarrative = attentionNarrative,

            // Context tags
            PressureTags = context?.PressureTags?.Select(t => t.ToString()).ToList() ?? new(),
            RelationshipTags = context?.RelationshipTags?.Select(t => t.ToString()).ToList() ?? new(),
            FeelingTags = context?.FeelingTags?.Select(t => t.ToString()).ToList() ?? new(),
            DiscoveryTags = context?.DiscoveryTags?.Select(t => t.ToString()).ToList() ?? new(),
            ResourceTags = context?.ResourceTags?.Select(t => t.ToString()).ToList() ?? new(),

            // Scene pressure metrics
            MinutesUntilDeadline = context?.MinutesUntilDeadline ?? 0,
            LetterQueueSize = context?.LetterQueueSize ?? 0,

            // Body language and peripheral awareness
            BodyLanguageDescription = bodyLanguage,
            PeripheralObservations = peripheralObservations,

            // Literary UI elements
            DeadlinePressure = deadlinePressure,
            EnvironmentalHints = environmentalHints,
            LocationName = context?.LocationSpotName ?? "Unknown Location",
            LocationAtmosphere = locationAtmosphere,
            CharacterState = bodyLanguage,
            CharacterAction = GenerateCharacterAction(npcState, urgentLetter, conversation),
            RelationshipStatus = GetRelationshipStatusDisplay(npc),

            // Internal monologue (generated based on pressure)
            InternalMonologue = GenerateInternalMonologue(context),

            // Binding obligations (promises and debts)
            BindingObligations = _bindingObligationSystem?.GetActiveObligations() ?? new List<BindingObligationViewModel>()
        };
    }

    private string GenerateCharacterAction(
        NPCEmotionalState npcState,
        Letter urgentLetter,
        ConversationManager conversation)
    {
        if (_actionBeatGenerator == null) return "";

        // Determine conversation turn (simplified - could track actual turns)
        int conversationTurn = 1; // Default to first turn

        // Check if urgent
        bool isUrgent = urgentLetter?.DeadlineInHours <= 2;

        // Generate the action beat with NPC ID for determinism
        string? npcId = conversation?.Context?.TargetNPC?.ID;
        return _actionBeatGenerator.GenerateActionBeat(
            npcState,
            urgentLetter?.Stakes,
            conversationTurn,
            isUrgent,
            npcId
        );
    }

    private string GenerateBodyLanguageFromTags(SceneContext context)
    {
        if (context?.RelationshipTags == null || !context.RelationshipTags.Any())
            return "Their posture is neutral, giving little away.";

        List<string> bodyLanguage = new List<string>();

        if (context.RelationshipTags.Contains(RelationshipTag.TRUST_HIGH))
            bodyLanguage.Add("leaning forward with open gestures");
        if (context.RelationshipTags.Contains(RelationshipTag.TRUST_NEGATIVE))
            bodyLanguage.Add("arms crossed defensively");
        if (context.RelationshipTags.Contains(RelationshipTag.STATUS_RECOGNIZED))
            bodyLanguage.Add("maintaining formal bearing");
        if (context.RelationshipTags.Contains(RelationshipTag.SHADOW_COMPLICIT))
            bodyLanguage.Add("glancing around nervously");
        if (context.RelationshipTags.Contains(RelationshipTag.STRANGER))
            bodyLanguage.Add("maintaining polite distance");

        return bodyLanguage.Any() ? string.Join(", ", bodyLanguage) : "Their expression is unreadable.";
    }

    private List<string> GeneratePeripheralObservations(SceneContext context)
    {
        List<string> observations = new List<string>();

        if (context?.PressureTags?.Contains(PressureTag.DEADLINE_IMMINENT) == true)
            observations.Add("Time pressure weighs heavily...");
        if (context?.FeelingTags?.Contains(FeelingTag.DANGER_LURKS) == true)
            observations.Add("Something feels off about this place...");
        if (context?.DiscoveryTags?.Contains(DiscoveryTag.SECRET_PRESENT) == true)
            observations.Add("They seem to be holding something back...");

        return observations;
    }

    private string GenerateInternalMonologue(SceneContext context)
    {
        if (context?.PressureTags?.Contains(PressureTag.DEADLINE_IMMINENT) == true)
            return "Every minute here costs me dearly...";
        if (context?.PressureTags?.Contains(PressureTag.DEBT_CRITICAL) == true)
            return "I can't afford to make enemies now...";
        if (context?.ResourceTags?.Contains(ResourceTag.STAMINA_EXHAUSTED) == true)
            return "I can barely focus through the exhaustion...";

        return null;
    }

    private string GetAttentionDescription(int cost)
    {
        return cost switch
        {
            0 => "A simple response",
            1 => "Requires focus to pursue",
            2 => "Demands significant mental effort",
            3 => "Exhaustive investigation",
            _ => ""
        };
    }

    private string DetermineEmotionalTone(ConversationChoice choice)
    {
        // Determine emotional tone based on choice properties
        if (choice.OfferTokenType == ConnectionType.Trust)
            return "warm";
        if (choice.OfferTokenType == ConnectionType.Status)
            return "formal";
        if (choice.OfferTokenType == ConnectionType.Shadow)
            return "mysterious";
        if (choice.OfferTokenType == ConnectionType.Commerce)
            return "confident";
        if (choice.PatienceCost >= 2)
            return "anxious";

        return "neutral";
    }

    private List<MechanicEffectViewModel> ConvertMechanicalEffectsToDisplay(List<IMechanicalEffect> effects)
    {
        List<MechanicEffectViewModel> mechanics = new List<MechanicEffectViewModel>();

        if (effects == null || !effects.Any())
            return mechanics;

        foreach (IMechanicalEffect effect in effects)
        {
            // Get the description for player display
            string description = effect.GetDescriptionsForPlayer().FirstOrDefault()?.Text ?? "";
            if (string.IsNullOrEmpty(description))
                continue;

            // Determine the effect type based on the description
            MechanicEffectType effectType = DetermineEffectType(description);

            // Convert to view model
            mechanics.Add(new MechanicEffectViewModel
            {
                Description = description,
                Type = effectType
            });
        }

        return mechanics;
    }

    private string GetAttentionDisplayString(int cost)
    {
        return cost switch
        {
            0 => "Free",
            1 => "◆ 1",
            2 => "◆◆ 2",
            3 => "◆◆◆ 3",
            _ => $"◆ {cost}"
        };
    }

    private MechanicEffectType DetermineEffectType(string description)
    {
        // Determine type based on description content
        if (description.Contains("✓") || description.Contains("Gain") || description.Contains("Unlock"))
            return MechanicEffectType.Positive;
        if (description.Contains("⚠") || description.Contains("Spend") || description.Contains("Burn") || description.Contains("Must"))
            return MechanicEffectType.Negative;
        return MechanicEffectType.Neutral;
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
            Status = GetQueueStatus(),
            Actions = GetQueueActions()
        };

        // Build queue slots
        for (int position = 1; position <= 8; position++)
        {
            Letter? letter = _letterQueueManager.GetLetterAt(position);
            bool canSkip = position > 1 && letter != null && _letterQueueManager.GetLetterAt(1) == null;

            // Calculate skip action details
            SkipActionViewModel? skipAction = null;
            if (canSkip)
            {
                int baseCost = position - 1;
                int multiplier = _standingObligationManager.CalculateSkipCostMultiplier(letter);
                int tokenCost = baseCost * multiplier;
                int availableTokens = _connectionTokenManager.GetTokenCount(letter.TokenType);

                // Build detailed multiplier reason
                string multiplierReason = null;
                if (multiplier > 1)
                {
                    List<StandingObligation> activeObligations = _standingObligationManager.GetActiveObligations()
                        .Where(o => o.HasEffect(ObligationEffect.TrustSkipDoubleCost) && o.AppliesTo(letter.TokenType))
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
                    TokenType = letter.TokenType.ToString(),
                    AvailableTokens = availableTokens,
                    HasEnoughTokens = availableTokens >= tokenCost,
                    MultiplierReason = multiplierReason
                };
            }

            QueueSlotViewModel slot = new QueueSlotViewModel
            {
                Position = position,
                IsOccupied = letter != null,
                Letter = letter != null ? ConvertToLetterViewModel(letter) : null,
                CanDeliver = position == 1 && letter?.State == LetterState.Collected,
                CanSkip = canSkip,
                SkipAction = skipAction
            };

            viewModel.QueueSlots.Add(slot);
        }

        return viewModel;
    }

    public async Task<bool> ExecuteLetterActionAsync(string actionType, string letterId)
    {
        // Parse action type and letter position
        if (!int.TryParse(letterId, out int position))
            return false;

        switch (actionType.ToLower())
        {
            case "deliver":
                Letter letter = _letterQueueManager.GetLetterAt(position);
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
                    _messageSystem.AddSystemMessage("Letter delivered successfully", SystemMessageTypes.Success);
                }
                return deliverSuccess;
            case "skip":
                _letterQueueManager.PrepareSkipAction(position);
                return true;
            case "priority":
                bool prioritySuccess = _letterQueueManager.TryPriorityMove(position);
                if (prioritySuccess)
                {
                    _messageSystem.AddSystemMessage("Letter moved to priority position", SystemMessageTypes.Success);
                }
                return prioritySuccess;
            case "extend":
                bool extendSuccess = _letterQueueManager.TryExtendDeadline(position);
                if (extendSuccess)
                {
                    _messageSystem.AddSystemMessage("Letter deadline extended", SystemMessageTypes.Success);
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
        Letter letter = _letterQueueManager.GetLetterAt(1);
        if (letter == null) return false;

        bool success = _letterQueueManager.DeliverFromPosition1();
        if (success)
        {
            // Delivery takes 1 hour
            ProcessTimeAdvancement(1);
            _messageSystem.AddSystemMessage("Letter delivered successfully", SystemMessageTypes.Success);
        }
        return success;
    }

    public async Task<bool> SkipLetterAsync(int position)
    {
        // Validate position is valid for skipping (positions 2-8 when position 1 is empty)
        if (position < 2 || position > 8)
            return false;

        _letterQueueManager.PrepareSkipAction(position);
        return true;
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

        // Trigger conversation
        _letterQueueManager.PreparePurgeAction();
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
                UnavailableReason = "Letter board is only available at Dawn",
                Offers = new List<LetterOfferViewModel>(),
                CurrentTime = currentTime
            };
        }

        // Get offers from the letter queue manager
        Player player = _gameWorld.GetPlayer();
        LetterQueueViewModel queueViewModel = GetLetterQueue();
        List<LetterOffer> offers = new List<LetterOffer>();

        // Letter board offers are now retrieved from LetterQueueManager
        List<LetterOfferViewModel> offerViewModels = offers.Select(offer => new LetterOfferViewModel
        {
            Id = offer.Id,
            SenderName = offer.NPCName,
            RecipientName = "Various Recipients", // offer.RecipientName removed
            Description = offer.Message,
            Payment = offer.Payment,
            DeadlineDays = offer.DeadlineInHours / 24,
            CanAccept = true, // CanAcceptMore method removed
            CannotAcceptReason = null,
            TokenTypes = new List<string> { offer.LetterType.ToString() }
        }).ToList();

        return new LetterBoardViewModel
        {
            IsAvailable = true,
            Offers = offerViewModels,
            CurrentTime = currentTime
        };
    }

    public async Task<bool> AcceptLetterOfferAsync(string offerId)
    {
        // Letter offer acceptance now handled through LetterQueueManager
        // AcceptLetterOffer method removed - needs reimplementation
        return await Task.FromResult(false);
    }

    // ========== ILetterQueueOperations Implementation ==========

    public Letter[] GetQueueSnapshot()
    {
        // Return defensive copy of queue
        Player player = _gameWorld.GetPlayer();
        if (player?.LetterQueue == null) return new Letter[8];
        return player.LetterQueue.ToArray();
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

        return new QueueOperationCost(
            costs,
            operation == QueueOperationType.MorningSwap,
            operation == QueueOperationType.Deliver,
            errors.ToArray()
        );
    }

    public bool CanPerformOperation(QueueOperationType operation, int position1, int? position2 = null)
    {
        QueueOperationCost cost = GetOperationCost(operation, position1, position2);

        // Check for validation errors
        if (cost.ValidationErrors.Length > 0)
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
            if (cost.ValidationErrors.Length > 0)
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
            _messageSystem.AddSystemMessage("Letter delivered successfully!", SystemMessageTypes.Success);
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
            LetterQueue queue = _letterQueueManager.GetLetterQueue();
            if (queue == null)
                return new QueueOperationResult(false, "Queue not available", null, GetQueueSnapshot());

            // Use the LetterQueue's Reorder method
            bool success = queue.Reorder(fromPosition, toPosition);
            if (!success)
                return new QueueOperationResult(false, "Failed to reorder queue", null, GetQueueSnapshot());

            _messageSystem.AddSystemMessage($"Reordered: moved position {fromPosition} to {toPosition}", SystemMessageTypes.Success);
            return new QueueOperationResult(true, null, new Dictionary<ConnectionType, int>(), GetQueueSnapshot());
        }
        finally
        {
            _queueOperationLock.Release();
        }
    }

    // ========== NAVIGATION SUPPORT ==========

    public async Task<bool> EndConversationAsync()
    {
        // Check comfort thresholds before ending conversation
        ConversationManager currentConversation = _conversationStateManager.PendingConversationManager;
        if (currentConversation != null)
        {
            await ProcessConversationCompletion(currentConversation);
        }

        // End any active conversation
        _conversationStateManager.ClearPendingConversation();
        _messageSystem.AddSystemMessage("Conversation ended", SystemMessageTypes.Info);
        return await Task.FromResult(true);
    }

    /// <summary>
    /// Process conversation completion - check comfort thresholds and generate letters
    /// </summary>
    private async Task ProcessConversationCompletion(ConversationManager conversation)
    {
        ConversationState state = conversation.State;
        NPC npc = conversation.Context.TargetNPC;

        if (state == null || npc == null)
        {
            Console.WriteLine("[GameFacade] Cannot process conversation completion - missing state or NPC");
            return;
        }

        int totalComfort = state.TotalComfort;
        int startingPatience = state.StartingPatience;

        Console.WriteLine($"[GameFacade] Processing conversation completion with {npc.Name} - Comfort: {totalComfort}, Starting Patience: {startingPatience}");

        // Check if minimum threshold for relationship maintenance was reached
        if (!state.HasReachedMaintainThreshold())
        {
            _messageSystem.AddSystemMessage($"Conversation with {npc.Name} ended awkwardly", SystemMessageTypes.Warning);
            Console.WriteLine($"[GameFacade] Conversation did not reach maintain threshold ({startingPatience * GameRules.COMFORT_MAINTAIN_THRESHOLD})");
        }
        else
        {
            _messageSystem.AddSystemMessage($"Good conversation with {npc.Name}", SystemMessageTypes.Success);
            
            // CRITICAL: Grant tokens when comfort thresholds are reached
            // This is what enables NPCLetterOfferService to find templates
            if (npc.LetterTokenTypes != null && npc.LetterTokenTypes.Any())
            {
                // Grant 1 token of the NPC's primary type for reaching maintain threshold
                ConnectionType tokenType = npc.LetterTokenTypes.First();
                _connectionTokenManager.AddTokensToNPC(tokenType, 1, npc.ID);
                Console.WriteLine($"[GameFacade] Granted 1 {tokenType} token to player for reaching maintain threshold with {npc.Name}");
                
                // Grant bonus token for reaching perfect threshold
                if (state.HasReachedPerfectThreshold())
                {
                    _connectionTokenManager.AddTokensToNPC(tokenType, 1, npc.ID);
                    Console.WriteLine($"[GameFacade] Granted bonus {tokenType} token for perfect conversation with {npc.Name}");
                }
            }
            else
            {
                Console.WriteLine($"[GameFacade] NPC {npc.Name} has no letter token types - no tokens granted");
            }
        }

        // Check for letter generation threshold
        if (state.HasReachedLetterThreshold())
        {
            Console.WriteLine($"[GameFacade] Letter threshold reached! Generating letters from conversation");

            try
            {
                List<Letter>? generatedLetters = _letterOfferService.GenerateFromConversation(npc.ID, totalComfort, startingPatience);

                Console.WriteLine($"[GameFacade] Generated {generatedLetters.Count} letters from conversation");
                Console.WriteLine($"[GameFacade] generatedLetters.Any(): {generatedLetters.Any()}");
                Console.WriteLine($"[GameFacade] generatedLetters is null: {generatedLetters == null}");
                if (generatedLetters != null && generatedLetters.Count > 0)
                {
                    for (int i = 0; i < generatedLetters.Count; i++)
                    {
                        Letter letter = generatedLetters[i];
                        Console.WriteLine($"[GameFacade] Letter {i}: {(letter == null ? "NULL" : $"Valid - {letter.Id}")}");
                    }
                }

                if (generatedLetters.Count > 0)
                {
                    Console.WriteLine($"[GameFacade] Entering foreach loop for {generatedLetters.Count} letters");
                    foreach (Letter letter in generatedLetters)
                    {
                        if (letter == null)
                        {
                            Console.WriteLine($"[GameFacade] Skipping null letter in collection");
                            continue;
                        }

                        Console.WriteLine($"[GameFacade] Processing letter: {letter.Id} - {letter.SenderName} to {letter.RecipientName}");
                        // Add letters to queue using existing positioning logic
                        int position = _letterQueueManager.AddLetterWithObligationEffects(letter);
                        Console.WriteLine($"[GameFacade] Letter added to queue at position: {position}");

                        string perfectBonus = state.HasReachedPerfectThreshold() ? " (Perfect Conversation!)" : "";
                        _messageSystem.AddSystemMessage(
                            $"✉️ {npc.Name} trusts you with a {letter.TokenType} letter{perfectBonus}",
                            SystemMessageTypes.Success
                        );
                        _messageSystem.AddSystemMessage(
                            $"💰 {letter.Payment} coins • ⏰ {letter.DeadlineInHours / 24} days deadline",
                            SystemMessageTypes.Info
                        );
                    }

                    // Show queue impact
                    Letter[] queueSnapshot = GetQueueSnapshot();
                    int filledSlots = queueSnapshot.Count(letter => letter != null);
                    _messageSystem.AddSystemMessage(
                        $"📋 Queue: {filledSlots}/{queueSnapshot.Length} slots",
                        SystemMessageTypes.Info
                    );
                }
                else
                {
                    Console.WriteLine($"[GameFacade] Letter threshold reached but no letters generated");
                    _messageSystem.AddSystemMessage($"{npc.Name} considered offering a letter but has none available right now", SystemMessageTypes.Info);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GameFacade] Error generating letters from conversation: {ex.Message}");
                _messageSystem.AddSystemMessage("Something went wrong with letter generation", SystemMessageTypes.Danger);
            }
        }
        else
        {
            Console.WriteLine($"[GameFacade] Letter threshold not reached (need {startingPatience * GameRules.COMFORT_LETTER_THRESHOLD}, got {totalComfort})");
        }

        // Perfect conversation bonus feedback
        if (state.HasReachedPerfectThreshold())
        {
            _messageSystem.AddSystemMessage($"🌟 Perfect conversation with {npc.Name}! Your bond has deepened significantly.", SystemMessageTypes.Success);

            // Award bonus relationship tokens for perfect conversations
            ConnectionType highestTokenType = GetHighestRelationshipType(npc.ID);
            _connectionTokenManager.AddTokensToNPC(highestTokenType, 1, npc.ID);

            Console.WriteLine($"[GameFacade] Perfect conversation bonus: +1 {highestTokenType} token with {npc.Name}");
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Get the highest relationship type with an NPC for bonus rewards
    /// </summary>
    private ConnectionType GetHighestRelationshipType(string npcId)
    {
        Dictionary<ConnectionType, int> tokens = _connectionTokenManager.GetTokensWithNPC(npcId);
        if (!tokens.Any() || tokens.Values.All(v => v == 0))
        {
            return ConnectionType.Trust; // Default to Trust for new relationships
        }

        return tokens.OrderByDescending(kvp => kvp.Value).First().Key;
    }

    public void RefreshLocationState()
    {
        // Refresh the current location state
        // This ensures NPCs are in correct positions for current time
        Player player = _gameWorld.GetPlayer();
        if (player?.CurrentLocationSpot != null)
        {
            // Update NPC positions for current time
            TimeBlocks currentTime = _timeManager.GetCurrentTimeBlock();
            Console.WriteLine($"[GameFacade.RefreshLocationState] Refreshing location for time: {currentTime}");
        }
    }

    // ========== MARKET ==========

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

    public async Task<bool> BuyItemAsync(string itemId, string traderId)
    {
        return await ExecuteMarketTradeAsync(itemId, "buy", traderId);
    }

    public async Task<bool> SellItemAsync(string itemId, string traderId)
    {
        return await ExecuteMarketTradeAsync(itemId, "sell", traderId);
    }

    private async Task<bool> ExecuteMarketTradeAsync(string itemId, string action, string locationId)
    {
        if (_ruleEngine == null || _itemRepository == null)
        {
            _messageSystem.AddSystemMessage("Market trading not available", SystemMessageTypes.Danger);
            return false;
        }

        MarketManager.TradeActionResult result;
        if (action.ToLower() == "buy")
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
                    CanRead = item.IsReadable()
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

        if (item.IsReadable())
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

    public async Task<MorningActivityResult> AdvanceToNextDayAsync()
    {
        // Advance time to next day
        _gameWorld.AdvanceToNextDay();

        // Run morning activities
        if (_morningActivitiesManager != null)
        {
            return _morningActivitiesManager.ProcessMorningActivities();
        }

        return new MorningActivityResult();
    }

    public MorningActivityResult GetMorningActivities()
    {
        if (_morningActivitiesManager != null)
        {
            return _morningActivitiesManager.GetLastActivityResult();
        }

        return new MorningActivityResult();
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
            PendingOfferCount = _letterOfferService.GetPendingOffersForNPC(npc.ID).Count,
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
        if (player.HasPatron && player.PatronLeverage > 0)
        {
            obligations.Add(new ObligationViewModel
            {
                Name = "Patron Employment",
                Description = $"Patron has {player.PatronLeverage} leverage over you",
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
                DaysUntilForcedLetter = 0
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

            // Check for forced letter warnings
            if (obligation.HasEffect(ObligationEffect.ShadowForced))
            {
                activeVm.HasForcedLetterWarning = obligation.DaysSinceLastForcedLetter >= 2;
                activeVm.DaysUntilForcedLetter = Math.Max(0, 3 - obligation.DaysSinceLastForcedLetter);
            }
            else if (obligation.HasEffect(ObligationEffect.PatronMonthly))
            {
                activeVm.HasForcedLetterWarning = obligation.DaysSinceLastForcedLetter >= 28;
                activeVm.DaysUntilForcedLetter = Math.Max(0, 30 - obligation.DaysSinceLastForcedLetter);
            }

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

    private Dictionary<string, string> GetRelationshipStatusDisplay(NPC npc)
    {
        Dictionary<string, string> status = new Dictionary<string, string>();

        if (npc == null)
            return status;

        // Get conversation-specific context instead of token balances
        // Focus on NPC's current emotional state and conversation disposition

        // Get NPC's current emotional state for conversation context
        NPCEmotionalState currentState = _npcStateResolver?.CalculateState(npc) ?? NPCEmotionalState.WITHDRAWN;
        PersonalityType personalityType = npc.PersonalityType;

        // Add emotional state context
        string emotionalContext = currentState switch
        {
            NPCEmotionalState.DESPERATE => "Seeking urgent help",
            NPCEmotionalState.ANXIOUS => "Worried about something",
            NPCEmotionalState.CALCULATING => "Considering options carefully",
            NPCEmotionalState.WITHDRAWN => "Keeping thoughts private",
            NPCEmotionalState.HOSTILE => "Angry about past events",
            _ => "Ready to talk"
        };

        status["Current Mood"] = emotionalContext;

        // Add personality-based conversation style
        string conversationStyle = personalityType switch
        {
            PersonalityType.DEVOTED => "Values emotional connection",
            PersonalityType.MERCANTILE => "Focuses on practical matters",
            PersonalityType.PROUD => "Expects respect and formality",
            PersonalityType.CUNNING => "Speaks in subtle meanings",
            PersonalityType.STEADFAST => "Prefers direct, honest talk",
            _ => "Open to various approaches"
        };

        status["Conversation Style"] = conversationStyle;

        // Add context about what they might want to discuss
        string topicHint = GetConversationTopicHint(npc, currentState);
        if (!string.IsNullOrEmpty(topicHint))
        {
            status["Main Concern"] = topicHint;
        }

        return status;
    }

    private string GetConversationTopicHint(NPC npc, NPCEmotionalState currentState)
    {
        // Provide hints about what the NPC might want to talk about
        // Based on their current state and any pressing matters

        if (currentState == NPCEmotionalState.DESPERATE || currentState == NPCEmotionalState.ANXIOUS)
        {
            // Check if they have urgent letters or deadlines
            Letter[] playerQueue = GetPlayer().LetterQueue;
            Letter? urgentLetter = playerQueue?.FirstOrDefault(l => l != null &&
                l.SenderName == npc.Name && l.DeadlineInHours <= 6);

            if (urgentLetter != null)
            {
                return $"Urgent letter deadline approaching ({urgentLetter.DeadlineInHours}h)";
            }

            return "Has urgent personal matters to discuss";
        }

        if (currentState == NPCEmotionalState.HOSTILE)
        {
            return "Upset about previous interactions";
        }

        if (npc.PersonalityType == PersonalityType.MERCANTILE)
        {
            return "May discuss business opportunities";
        }

        if (npc.PersonalityType == PersonalityType.CUNNING)
        {
            return "Might share information or secrets";
        }

        return "Open to general conversation";
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

    // ========== SEAL MANAGEMENT ==========

    public SealProgressionViewModel GetSealProgression()
    {
        SealProgressionViewModel viewModel = new SealProgressionViewModel();
        Player player = _gameWorld.GetPlayer();

        // Get all owned seals
        foreach (Seal seal in player.OwnedSeals)
        {
            OwnedSeal ownedSeal = new OwnedSeal
            {
                Id = seal.Id,
                Name = seal.GetFullName(),
                Type = seal.Type,
                Tier = seal.Tier,
                Description = seal.Description,
                Material = seal.Material,
                Insignia = seal.Insignia,
                DayIssued = seal.DayIssued,
                IssuingGuild = GetGuildNameFromId(seal.IssuingGuildId),
                IsWorn = player.WornSeals.Contains(seal),
                Benefits = GetSealBenefits(seal)
            };
            viewModel.OwnedSeals.Add(ownedSeal);
        }

        // Get worn seals with slot numbers
        int slotNumber = 1;
        foreach (Seal seal in player.WornSeals)
        {
            viewModel.WornSeals.Add(new WornSeal
            {
                Id = seal.Id,
                Name = seal.GetFullName(),
                Type = seal.Type,
                Tier = seal.Tier,
                SlotNumber = slotNumber++
            });
        }

        // Get endorsement progress for each seal type
        foreach (SealType sealType in Enum.GetValues<SealType>())
        {
            EndorsementProgress progress = new EndorsementProgress
            {
                Type = sealType,
                TypeName = sealType.ToString()
            };

            // Get current endorsement count
            string endorsementKey = $"endorsements_delivered_{sealType}";
            MemoryFlag memory = player.GetMemory(endorsementKey);
            progress.CurrentEndorsements = memory?.Importance ?? 0;

            // Get current seal tier
            Seal? currentSeal = player.OwnedSeals.FirstOrDefault(s => s.Type == sealType);
            progress.CurrentTier = currentSeal?.Tier ?? (SealTier)0; // 0 means no seal

            // Determine next tier and requirements
            if (progress.CurrentTier < SealTier.Master)
            {
                progress.NextTier = progress.CurrentTier == 0 ? SealTier.Apprentice : progress.CurrentTier + 1;
                progress.EndorsementsToNext = GetEndorsementsRequired(progress.NextTier.Value) - progress.CurrentEndorsements;
                progress.NextTierBenefits = GetTierBenefits(sealType, progress.NextTier.Value);
            }

            // Get guild locations for this seal type
            progress.GuildLocations = GetGuildLocationsForSealType(sealType);

            viewModel.EndorsementTracking.Add(progress);
        }

        return viewModel;
    }

    public async Task<bool> EquipSealAsync(string sealId)
    {
        Player player = _gameWorld.GetPlayer();
        Seal? seal = player.OwnedSeals.FirstOrDefault(s => s.Id == sealId);

        if (seal == null)
        {
            _messageSystem.AddSystemMessage("You don't own this seal.", SystemMessageTypes.Danger);
            return false;
        }

        if (player.WornSeals.Contains(seal))
        {
            _messageSystem.AddSystemMessage("This seal is already equipped.", SystemMessageTypes.Warning);
            return false;
        }

        if (player.WornSeals.Count >= 3)
        {
            _messageSystem.AddSystemMessage("You can only wear 3 seals at once. Unequip one first.", SystemMessageTypes.Warning);
            return false;
        }

        bool success = player.EquipSeal(seal);
        if (success)
        {
            _messageSystem.AddSystemMessage($"Equipped {seal.GetFullName()}.", SystemMessageTypes.Success);
        }

        return success;
    }

    public async Task<bool> UnequipSealAsync(string sealId)
    {
        Player player = _gameWorld.GetPlayer();
        Seal? seal = player.WornSeals.FirstOrDefault(s => s.Id == sealId);

        if (seal == null)
        {
            _messageSystem.AddSystemMessage("This seal is not equipped.", SystemMessageTypes.Warning);
            return false;
        }

        bool success = player.UnequipSeal(seal);
        if (success)
        {
            _messageSystem.AddSystemMessage($"Unequipped {seal.GetFullName()}.", SystemMessageTypes.Success);
        }

        return success;
    }

    private List<string> GetSealBenefits(Seal seal)
    {
        List<string> benefits = new List<string>();

        // Base benefits by type
        switch (seal.Type)
        {
            case SealType.Commerce:
                benefits.Add("Better prices at markets");
                benefits.Add("Access to merchant guild services");
                if (seal.Tier >= SealTier.Journeyman)
                    benefits.Add("Priority trading with guild members");
                if (seal.Tier >= SealTier.Master)
                    benefits.Add("Exclusive trade route information");
                break;

            case SealType.Status:
                benefits.Add("Recognition by nobles and scholars");
                benefits.Add("Access to restricted libraries");
                if (seal.Tier >= SealTier.Journeyman)
                    benefits.Add("Invitation to exclusive events");
                if (seal.Tier >= SealTier.Master)
                    benefits.Add("Authority to issue recommendations");
                break;

            case SealType.Shadow:
                benefits.Add("Safe passage through dangerous areas");
                benefits.Add("Access to black market contacts");
                if (seal.Tier >= SealTier.Journeyman)
                    benefits.Add("Protection from rival gangs");
                if (seal.Tier >= SealTier.Master)
                    benefits.Add("Control over underground networks");
                break;
        }

        return benefits;
    }

    private string GetTierBenefits(SealType type, SealTier tier)
    {
        return (type, tier) switch
        {
            (SealType.Commerce, SealTier.Apprentice) => "Basic trading privileges and guild recognition",
            (SealType.Commerce, SealTier.Journeyman) => "Full merchant rights and priority trading",
            (SealType.Commerce, SealTier.Master) => "Elite status with exclusive trade routes",
            (SealType.Status, SealTier.Apprentice) => "Recognition among educated classes",
            (SealType.Status, SealTier.Journeyman) => "Library access and event invitations",
            (SealType.Status, SealTier.Master) => "Academic authority and teaching rights",
            (SealType.Shadow, SealTier.Apprentice) => "Safe passage and underground contacts",
            (SealType.Shadow, SealTier.Journeyman) => "Gang protection and safe house access",
            (SealType.Shadow, SealTier.Master) => "Network control and criminal authority",
            _ => "Guild recognition and privileges"
        };
    }

    private int GetEndorsementsRequired(SealTier tier)
    {
        return tier switch
        {
            SealTier.Apprentice => 3,
            SealTier.Journeyman => 5,
            SealTier.Master => 7,
            _ => 3
        };
    }

    private List<string> GetGuildLocationsForSealType(SealType type)
    {
        return type switch
        {
            SealType.Commerce => new List<string> { "Merchant Guild", "Courier's Guild" },
            SealType.Status => new List<string> { "Scholar's Atheneum" },
            SealType.Shadow => new List<string> { "Underground Guild Halls" },
            _ => new List<string>()
        };
    }

    private string GetGuildNameFromId(string guildId)
    {
        return guildId switch
        {
            "merchant_guild" => "Merchant Guild",
            "messenger_guild" => "Courier's Guild",
            "scholar_guild" => "Scholar's Atheneum",
            _ => "Unknown Guild"
        };
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
            if (!player.Seals.Any(s => s.Id == sealId))
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

        // Check for Letter Board availability
        if (_timeManager.GetCurrentTimeBlock() != TimeBlocks.Dawn)
        {
            ActionOptionViewModel letterBoardInfo = new ActionOptionViewModel
            {
                Id = "letter_board_closed",
                Description = "Visit Letter Board",
                IsAvailable = false,
                IsServiceClosed = true,
                NextAvailableTime = GetNextAvailableTime(TimeBlocks.Dawn),
                ServiceSchedule = "Available only at Dawn",
                UnavailableReasons = new List<string> { "Letter Board is closed. Only available during Dawn hours." }
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

    public List<SpecialLetterOption> GetAvailableSpecialLetters(string npcId)
    {
        return _specialLetterService?.GetAvailableSpecialLetters(npcId) ?? new List<SpecialLetterOption>();
    }

    public bool RequestSpecialLetter(string npcId, ConnectionType tokenType)
    {
        return _specialLetterService?.RequestSpecialLetter(npcId, tokenType) ?? false;
    }

    public LetterCategory GetAvailableCategory(string npcId, ConnectionType tokenType)
    {
        return _letterCategoryService?.GetAvailableCategory(npcId, tokenType) ?? LetterCategory.Basic;
    }

    public int GetTokensToNextCategory(string npcId, ConnectionType tokenType)
    {
        return _letterCategoryService?.GetTokensToNextCategory(npcId, tokenType) ?? 0;
    }

    public (int min, int max) GetCategoryPaymentRange(LetterCategory category)
    {
        return _letterCategoryService?.GetCategoryPaymentRange(category) ?? (0, 0);
    }

    // ========== ITEM MANAGEMENT ==========

    public Item GetItemById(string itemId)
    {
        return _itemRepository?.GetItemById(itemId);
    }

    public async Task<bool> ReadLetterAsync(string itemId)
    {
        Item item = _itemRepository.GetItemById(itemId);
        if (item == null || !item.IsReadable())
        {
            return false;
        }

        // Execute read directly
        if (!string.IsNullOrEmpty(item.ReadFlagToSet))
        {
            _flagService.SetFlag(item.ReadFlagToSet);
        }

        // Show message about reading the letter
        _messageSystem.AddSystemMessage($"You carefully read the {item.Name}...", SystemMessageTypes.Info);

        // Show any special effects or notifications
        if (!string.IsNullOrEmpty(item.ReadFlagToSet))
        {
            _messageSystem.AddSystemMessage("The letter's contents give you pause. This could change everything...", SystemMessageTypes.Tutorial);
        }

        return true;

        return false;
    }

    public bool CanReadItem(string itemId)
    {
        Item item = _itemRepository.GetItemById(itemId);
        return item != null && item.IsReadable();
    }

    public ReadableLetterInfo GetLetterInfo(string itemId)
    {
        Item item = _itemRepository.GetItemById(itemId);
        if (item == null || !item.IsReadable())
        {
            return null;
        }

        return new ReadableLetterInfo
        {
            ItemId = item.Id,
            Name = item.Name,
            Description = item.Description,
            IsRead = !string.IsNullOrEmpty(item.ReadFlagToSet) && _flagService.HasFlag(item.ReadFlagToSet),
            HasSpecialEffect = !string.IsNullOrEmpty(item.ReadFlagToSet)
        };
    }

    // ========== LETTER QUEUE MANAGEMENT ==========

    public int GetLetterQueueCount()
    {
        Player player = _gameWorld.GetPlayer();
        if (player?.LetterQueue == null) return 0;
        return player.LetterQueue.Count(l => l != null && l.State == LetterState.Collected);
    }

    public bool IsLetterQueueFull()
    {
        int count = GetLetterQueueCount();
        return count >= 8; // MAX_LETTER_QUEUE_SIZE
    }

    public int AddLetterWithObligationEffects(Letter letter)
    {
        if (letter == null || IsLetterQueueFull())
            return -1;

        // Delegate to LetterQueueManager for proper queue management
        return _letterQueueManager.AddLetterWithObligationEffects(letter);
    }

    public bool IsActionForbidden(string actionType, Letter letter, out string reason)
    {
        reason = null;

        if (_standingObligationManager == null)
            return false;

        return _standingObligationManager.IsActionForbidden(actionType, letter, out reason);
    }

    // ========== NOTICE BOARD ==========

    public List<NoticeBoardOption> GetNoticeBoardOptions()
    {
        List<NoticeBoardOption> options = new List<NoticeBoardOption>();

        // Define the three standard notice board options
        options.Add(new NoticeBoardOption
        {
            Id = "anything_heading",
            Name = "Anything heading...",
            Description = "Ask about letters going to specific locations",
            TokenCost = 2,
            OptionType = NoticeBoardOptionType.AnythingHeading,
            RequiresDirection = true
        });

        options.Add(new NoticeBoardOption
        {
            Id = "looking_for_work",
            Name = "Looking for work...",
            Description = "Request letters of a specific type",
            TokenCost = 3,
            OptionType = NoticeBoardOptionType.LookingForWork,
            RequiresTokenType = true
        });

        options.Add(new NoticeBoardOption
        {
            Id = "urgent_deliveries",
            Name = "Urgent deliveries?",
            Description = "High-paying letters with tight deadlines",
            TokenCost = 5,
            OptionType = NoticeBoardOptionType.UrgentDeliveries
        });

        return options;
    }

    public bool CanAffordNoticeBoardOption(NoticeBoardOption option)
    {
        if (option == null)
            return false;

        Player player = _gameWorld.GetPlayer();
        int totalTokens = 0;

        // Count all tokens
        foreach (ConnectionType tokenType in Enum.GetValues<ConnectionType>())
        {
            totalTokens += _connectionTokenManager?.GetTokenCount(tokenType) ?? 0;
        }

        return totalTokens >= option.TokenCost;
    }

    public async Task<bool> ExecuteNoticeBoardOption(NoticeBoardOption option, string direction = null)
    {
        if (option == null || !CanAffordNoticeBoardOption(option))
            return false;

        Player player = _gameWorld.GetPlayer();

        // Deduct tokens (randomly from available types)
        List<(ConnectionType type, int count)> availableTokens = new List<(ConnectionType type, int count)>();
        foreach (ConnectionType tokenType in Enum.GetValues<ConnectionType>())
        {
            int count = _connectionTokenManager?.GetTokenCount(tokenType) ?? 0;
            if (count > 0)
                availableTokens.Add((tokenType, count));
        }

        if (!availableTokens.Any())
            return false;

        // Deduct tokens randomly
        Random random = new Random();
        int tokensToDeduct = option.TokenCost;

        while (tokensToDeduct > 0 && availableTokens.Any())
        {
            int index = random.Next(availableTokens.Count);
            (ConnectionType tokenType, int count) = availableTokens[index];

            // Find an NPC to deduct from
            List<NPC> allNpcs = _npcRepository?.GetAllNPCs() ?? new List<NPC>();
            foreach (NPC npc in allNpcs)
            {
                Dictionary<ConnectionType, int>? npcTokens = _connectionTokenManager?.GetTokensWithNPC(npc.ID);
                if (npcTokens != null && npcTokens.ContainsKey(tokenType) && npcTokens[tokenType] > 0)
                {
                    // Deduct token directly from player's balance with NPC
                    Dictionary<string, Dictionary<ConnectionType, int>> playerTokens = _gameWorld.GetPlayer().NPCTokens;
                    if (playerTokens.ContainsKey(npc.ID) && playerTokens[npc.ID].ContainsKey(tokenType))
                    {
                        playerTokens[npc.ID][tokenType]--;
                        if (playerTokens[npc.ID][tokenType] <= 0)
                        {
                            playerTokens[npc.ID].Remove(tokenType);
                            if (playerTokens[npc.ID].Count == 0)
                                playerTokens.Remove(npc.ID);
                        }
                    }
                    tokensToDeduct--;

                    availableTokens[index] = (tokenType, count - 1);
                    if (availableTokens[index].count == 0)
                        availableTokens.RemoveAt(index);

                    break;
                }
            }
        }

        // Generate a letter based on the option type
        Letter generatedLetter = null;

        switch (option.OptionType)
        {
            case NoticeBoardOptionType.AnythingHeading:
                // Generate letter heading to specified direction
                generatedLetter = GenerateDirectionalLetter(direction);
                break;

            case NoticeBoardOptionType.LookingForWork:
                // Generate letter of specific type (would need token type parameter)
                generatedLetter = GenerateTypedLetter(ConnectionType.Commerce); // Default for now
                break;

            case NoticeBoardOptionType.UrgentDeliveries:
                // Generate urgent high-paying letter
                generatedLetter = GenerateUrgentLetter();
                break;
        }

        if (generatedLetter != null)
        {
            int position = AddLetterWithObligationEffects(generatedLetter);
            if (position > 0)
            {
                _messageSystem?.AddSystemMessage(
                    $"📬 Letter added to your queue at position {position}!",
                    SystemMessageTypes.Success
                );
                return true;
            }
        }

        return false;
    }

    private Letter GenerateDirectionalLetter(string direction)
    {
        Letter letter = new Letter
        {
            Id = Guid.NewGuid().ToString(),
            SenderName = "Notice Board Client",
            RecipientName = $"Recipient in {direction}",
            Payment = 20 + new Random().Next(10),
            DeadlineInHours = (3 + new Random().Next(2)) * 24,
            TokenType = (ConnectionType)new Random().Next(4)
        };

        return letter;
    }

    private Letter GenerateTypedLetter(ConnectionType tokenType)
    {
        Letter letter = new Letter
        {
            Id = Guid.NewGuid().ToString(),
            SenderName = $"{tokenType} Contact",
            RecipientName = $"{tokenType} Recipient",
            Payment = tokenType == ConnectionType.Shadow ? 40 : 25,
            DeadlineInHours = 4 * 24,
            TokenType = tokenType
        };

        return letter;
    }

    private Letter GenerateUrgentLetter()
    {
        Letter letter = new Letter
        {
            Id = Guid.NewGuid().ToString(),
            SenderName = "Urgent Client",
            RecipientName = "Time-Sensitive Recipient",
            Payment = 50 + new Random().Next(20),
            DeadlineInHours = 2 * 24,
            TokenType = (ConnectionType)new Random().Next(4),
            PhysicalProperties = LetterPhysicalProperties.Fragile
        };

        return letter;
    }

    // ========== LETTER QUEUE HELPER METHODS ==========

    private LetterViewModel ConvertToLetterViewModel(Letter letter)
    {
        (bool HasLeverage, string Indicator, string Tooltip, int LeverageStrength, int TokenBalance) leverage = GetLetterLeverageInfo(letter);
        (int paymentBonus, string paymentBonusSource, int deadlineExtension, string deadlineExtensionSource, int positionModifier, string positionModifierSource, List<string> activeEffects) obligationEffects = GetLetterObligationEffects(letter);

        return new LetterViewModel
        {
            Id = letter.Id,
            SenderName = letter.SenderName,
            RecipientName = letter.RecipientName,
            DeadlineInHours = letter.DeadlineInHours,
            Payment = letter.Payment,
            TokenType = letter.TokenType.ToString(),
            TokenIcon = GetTokenIcon(letter.TokenType),
            Size = letter.Size.ToString(),
            SizeIcon = GetSizeIcon(letter.Size),
            Weight = letter.Weight,
            WeightDisplay = GetWeightDisplay(letter.Weight),
            IsPatronLetter = letter.IsFromPatron,
            IsCollected = letter.State == LetterState.Collected,
            PhysicalConstraints = letter.GetPhysicalConstraintsDescription(),
            PhysicalIcon = GetPhysicalIcon(letter.PhysicalProperties),
            IsSpecial = letter.IsSpecial,
            SpecialIcon = letter.GetSpecialIcon(),
            SpecialDescription = letter.GetSpecialDescription(),
            DeadlineClass = GetDeadlineClass(letter.DeadlineInHours),
            DeadlineIcon = GetDeadlineIcon(letter.DeadlineInHours),
            DeadlineDescription = GetDeadlineDescription(letter.DeadlineInHours),
            LeverageIndicator = leverage.Indicator,
            LeverageTooltip = leverage.Tooltip,
            HasLeverage = leverage.HasLeverage,
            LeverageStrength = leverage.LeverageStrength,
            TokenBalance = leverage.TokenBalance,
            LeverageClass = GetLeverageClass(leverage.LeverageStrength),
            OriginalPosition = letter.OriginalQueuePosition ?? 0,
            CurrentPosition = _letterQueueManager.GetLetterPosition(letter.Id) ?? 0,
            HasPaymentBonus = obligationEffects.paymentBonus > 0,
            PaymentBonusAmount = obligationEffects.paymentBonus,
            PaymentBonusSource = obligationEffects.paymentBonusSource,
            HasDeadlineExtension = obligationEffects.deadlineExtension > 0,
            DeadlineExtensionHours = obligationEffects.deadlineExtension,
            DeadlineExtensionSource = obligationEffects.deadlineExtensionSource,
            HasPositionModifier = obligationEffects.positionModifier != 0,
            PositionModifierAmount = obligationEffects.positionModifier,
            PositionModifierSource = obligationEffects.positionModifierSource,
            ActiveObligationEffects = obligationEffects.activeEffects
        };
    }

    private QueueStatusViewModel GetQueueStatus()
    {
        LetterQueue queue = _letterQueueManager.GetLetterQueue();
        int totalWeight = queue.GetTotalWeight();
        int maxWeight = queue.MaxWeight;

        return new QueueStatusViewModel
        {
            LetterCount = _letterQueueManager.GetLetterCount(),
            MaxCapacity = 8,
            ExpiredCount = _letterQueueManager.GetExpiringLetters(0).Length,
            UrgentCount = _letterQueueManager.GetExpiringLetters(1).Length,
            WarningCount = _letterQueueManager.GetExpiringLetters(2).Length,
            TotalWeight = totalWeight,
            MaxWeight = maxWeight,
            RemainingWeight = maxWeight - totalWeight,
            WeightDisplay = $"{totalWeight}/{maxWeight}"
        };
    }

    private QueueActionsViewModel GetQueueActions()
    {
        Player player = _gameWorld.GetPlayer();

        QueueActionsViewModel actions = new QueueActionsViewModel
        {
            CanMorningSwap = _timeManager.GetCurrentTimeBlock() == TimeBlocks.Dawn &&
                           player.LastMorningSwapDay != _gameWorld.CurrentDay,
            MorningSwapReason = GetMorningSwapReason(),
            HasBottomLetter = _letterQueueManager.GetLetterAt(8) != null,
            TotalAvailableTokens = GetTotalAvailableTokens(),
            PurgeTokenOptions = GetPurgeTokenOptions()
        };

        return actions;
    }

    private string GetMorningSwapReason()
    {
        Player player = _gameWorld.GetPlayer();

        if (_timeManager.GetCurrentTimeBlock() != TimeBlocks.Dawn)
            return "Only available at dawn";
        if (player.LastMorningSwapDay == _gameWorld.CurrentDay)
            return "Already used today";
        return "Once per day at dawn";
    }

    private List<TokenOptionViewModel> GetPurgeTokenOptions()
    {
        List<TokenOptionViewModel> options = new List<TokenOptionViewModel>();

        foreach (ConnectionType tokenType in Enum.GetValues<ConnectionType>())
        {
            int available = _connectionTokenManager.GetTokenCount(tokenType);
            if (available > 0)
            {
                options.Add(new TokenOptionViewModel
                {
                    TokenType = tokenType.ToString(),
                    TokenIcon = GetTokenIcon(tokenType),
                    Available = available
                });
            }
        }

        return options;
    }

    private int GetTotalAvailableTokens()
    {
        return Enum.GetValues<ConnectionType>()
            .Cast<ConnectionType>()
            .Sum(type => _connectionTokenManager.GetTokenCount(type));
    }

    private (bool HasLeverage, string Indicator, string Tooltip, int LeverageStrength, int TokenBalance) GetLetterLeverageInfo(Letter letter)
    {
        Dictionary<ConnectionType, int> npcTokens = _connectionTokenManager.GetTokensWithNPC(letter.SenderName);
        int tokenBalance = npcTokens.GetValueOrDefault(letter.TokenType, 0);

        if (tokenBalance < 0)
        {
            int leverageStrength = Math.Abs(tokenBalance);
            string indicator = $"💸 -{leverageStrength}";
            string tooltip = $"You owe {leverageStrength} {letter.TokenType} tokens to {letter.SenderName}. This debt gives them leverage to push their letters forward in your queue.";
            return (true, indicator, tooltip, leverageStrength, tokenBalance);
        }
        else if (tokenBalance >= 4)
        {
            string indicator = "🛡️";
            string tooltip = $"Your strong relationship ({tokenBalance} tokens) reduces {letter.SenderName}'s leverage";
            return (false, indicator, tooltip, 0, tokenBalance);
        }

        return (false, "", "", 0, tokenBalance);
    }

    private (int paymentBonus, string paymentBonusSource, int deadlineExtension, string deadlineExtensionSource,
            int positionModifier, string positionModifierSource, List<string> activeEffects) GetLetterObligationEffects(Letter letter)
    {
        int paymentBonus = 0;
        string paymentBonusSource = "";
        int deadlineExtension = 0;
        string deadlineExtensionSource = "";
        int positionModifier = 0;
        string positionModifierSource = "";
        List<string> activeEffects = new();

        if (_standingObligationManager == null)
            return (0, "", 0, "", 0, "", activeEffects);

        // Calculate payment bonus
        int calculatedBonus = _standingObligationManager.CalculateTotalCoinBonus(letter);
        if (calculatedBonus > 0)
        {
            paymentBonus = calculatedBonus;
            List<StandingObligation> paymentObligations = _standingObligationManager.GetActiveObligations()
                .Where(o => o.AppliesTo(letter.TokenType) &&
                       (o.HasEffect(ObligationEffect.CommerceBonus) ||
                        o.HasEffect(ObligationEffect.CommerceBonusPlus3) ||
                        o.HasEffect(ObligationEffect.ShadowTriplePay) ||
                        o.HasEffect(ObligationEffect.DynamicPaymentBonus)))
                .ToList();

            if (paymentObligations.Any())
            {
                paymentBonusSource = string.Join(", ", paymentObligations.Select(o => o.Name));
                activeEffects.Add($"+{calculatedBonus} coins from {paymentBonusSource}");
            }
        }

        // Calculate deadline extensions
        List<StandingObligation> deadlineObligations = _standingObligationManager.GetActiveObligations()
            .Where(o => o.AppliesTo(letter.TokenType) &&
                   (o.HasEffect(ObligationEffect.DeadlinePlus2Days) ||
                    o.HasEffect(ObligationEffect.DynamicDeadlineBonus)))
            .ToList();

        foreach (StandingObligation? obligation in deadlineObligations)
        {
            if (obligation.HasEffect(ObligationEffect.DeadlinePlus2Days))
            {
                deadlineExtension += 2;
                deadlineExtensionSource = obligation.Name;
                activeEffects.Add($"+2 days deadline from {obligation.Name}");
            }
        }

        // Calculate position modifiers
        List<StandingObligation> positionObligations = _standingObligationManager.GetActiveObligations()
            .Where(o => o.AppliesTo(letter.TokenType) &&
                   (o.HasEffect(ObligationEffect.StatusPriority) ||
                    o.HasEffect(ObligationEffect.CommercePriority) ||
                    o.HasEffect(ObligationEffect.TrustPriority) ||
                    o.HasEffect(ObligationEffect.PatronLettersPosition1) ||
                    o.HasEffect(ObligationEffect.PatronLettersPosition3) ||
                    o.HasEffect(ObligationEffect.DynamicLeverageModifier)))
            .ToList();

        if (positionObligations.Any())
        {
            int bestPosition = _standingObligationManager.CalculateBestEntryPosition(letter, 8);
            if (bestPosition < 8)
            {
                positionModifier = bestPosition - 8; // Negative value means better position
                positionModifierSource = string.Join(", ", positionObligations.Select(o => o.Name));
                activeEffects.Add($"Position {bestPosition} from {positionModifierSource}");
            }
        }

        // Add restriction effects
        if (_standingObligationManager.IsActionForbidden("refuse", letter, out string refuseReason))
        {
            activeEffects.Add($"Cannot refuse: {refuseReason}");
        }

        if (_standingObligationManager.IsActionForbidden("purge", letter, out string purgeReason))
        {
            activeEffects.Add($"Cannot purge: {purgeReason}");
        }

        // Check for skip cost multipliers
        int skipMultiplier = _standingObligationManager.CalculateSkipCostMultiplier(letter);
        if (skipMultiplier > 1)
        {
            List<StandingObligation> skipObligations = _standingObligationManager.GetActiveObligations()
                .Where(o => o.HasEffect(ObligationEffect.TrustSkipDoubleCost) && o.AppliesTo(letter.TokenType))
                .ToList();
            if (skipObligations.Any())
            {
                activeEffects.Add($"Skip costs ×{skipMultiplier} from {string.Join(", ", skipObligations.Select(o => o.Name))}");
            }
        }

        return (paymentBonus, paymentBonusSource, deadlineExtension, deadlineExtensionSource,
                positionModifier, positionModifierSource, activeEffects);
    }

    private string GetSizeIcon(SizeCategory size)
    {
        return size switch
        {
            SizeCategory.Small => "📄",
            SizeCategory.Medium => "📦",
            SizeCategory.Large => "📦",
            _ => "📦"
        };
    }

    private string GetPhysicalIcon(LetterPhysicalProperties props)
    {
        if (props.HasFlag(LetterPhysicalProperties.Fragile))
            return "⚠️";
        if (props.HasFlag(LetterPhysicalProperties.Heavy))
            return "💪";
        if (props.HasFlag(LetterPhysicalProperties.Perishable))
            return "🕒";
        if (props.HasFlag(LetterPhysicalProperties.Valuable))
            return "💎";
        return "";
    }

    private string GetWeightDisplay(int weight)
    {
        return weight switch
        {
            1 => "■",
            2 => "■■",
            3 => "■■■",
            _ => "■"
        };
    }

    private string GetDeadlineClass(int deadlineHours)
    {
        if (deadlineHours <= 0) return "deadline-expired";
        if (deadlineHours <= 6) return "deadline-urgent";
        if (deadlineHours <= 24) return "deadline-warning";
        return "deadline-normal";
    }

    private string GetDeadlineIcon(int deadlineHours)
    {
        if (deadlineHours <= 0) return "💀";
        if (deadlineHours <= 6) return "🔥";
        if (deadlineHours <= 24) return "⚠️";
        return "📅";
    }

    private string GetDeadlineDescription(int deadlineHours)
    {
        if (deadlineHours <= 0) return "EXPIRED!";
        if (deadlineHours <= 6) return "Due within hours!";
        if (deadlineHours <= 24) return "Due today!";
        int days = deadlineHours / 24;
        int hours = deadlineHours % 24;
        if (hours == 0) return $"{days} days";
        return $"{days}d {hours}h";
    }

    private string GetLeverageClass(int leverageStrength)
    {
        if (leverageStrength >= 5) return "leverage-strong";
        if (leverageStrength >= 3) return "leverage-medium";
        if (leverageStrength >= 1) return "leverage-weak";
        return "";
    }

    // Missing methods for compilation
    public List<Location> GetPlayerKnownLocations()
    {
        Player player = _gameWorld.GetPlayer();
        return _locationRepository.GetAllLocations();
    }

    public async Task StartGame()
    {
        // Game is already initialized during startup
        await Task.CompletedTask;
    }

    public bool CanTravelRoute(string routeId)
    {
        return _travelManager.CanTravelTo(routeId);
    }

    public bool CanTravelTo(string locationId)
    {
        return _travelManager.CanTravelTo(locationId);
    }

    #region Literary UI Support

    /// <summary>
    /// Get literary conversation data for the mockup-style UI
    /// This starts a conversation if one isn't already active
    /// </summary>
    public async Task<ConversationViewModel> GetConversationAsync(string npcId)
    {
        // Check if there's already an active conversation with this NPC
        ConversationManager existing = _conversationStateManager.PendingConversationManager;
        if (existing != null && existing.Context?.TargetNPC?.ID == npcId)
        {
            return CreateConversationViewModel(existing);
        }

        // Otherwise, start a new conversation properly with async/await
        Player player = _gameWorld.GetPlayer();
        NPC npc = _npcRepository.GetById(npcId);
        Location? location = player.GetCurrentLocation(_locationRepository);
        LocationSpot spot = player.CurrentLocationSpot;

        if (npc == null) return null;

        // Create context for conversation with PERSISTENT attention from time block
        SceneContext context = SceneContext.Standard(_gameWorld, player, npc, location, spot);

        // CRITICAL: Pass the persistent attention manager for this time block
        TimeBlocks currentTimeBlock = _timeManager.GetCurrentTimeBlock();
        context.AttentionManager = _timeBlockAttentionManager.GetCurrentAttention(currentTimeBlock);
        Console.WriteLine($"[GameFacade] Providing attention for {currentTimeBlock}: {context.AttentionManager.GetAvailableAttention()}/{context.AttentionManager.GetMaxAttention()}");

        // Start the conversation to generate choices
        ConversationManager conversation = await _conversationFactory.CreateConversation(context, player);

        if (conversation != null)
        {
            _conversationStateManager.SetCurrentConversation(conversation);
            return CreateConversationViewModel(conversation);
        }

        // Fallback if conversation creation failed - create basic view model
        int baseAttention = 3;
        int attentionModifier = 0;
        string atmosphereDescription = "The space feels neutral";

        // Use AtmosphereCalculator if available
        if (_atmosphereCalculator != null && player.CurrentLocationSpot != null)
        {
            AtmosphereEffect atmosphere = _atmosphereCalculator.CalculateForLocation(player.CurrentLocationSpot.SpotID);
            attentionModifier = atmosphere.AttentionModifier;
            atmosphereDescription = atmosphere.Description;
        }

        int maxAttention = Math.Max(1, Math.Min(5, baseAttention + attentionModifier));
        int currentAttention = maxAttention; // Start with full attention

        string attentionNarrative = attentionModifier < 0
            ? "The distractions make it hard to focus"
            : attentionModifier > 0
                ? "The comfortable atmosphere helps you focus"
                : "You give your full attention";

        ConversationViewModel viewModel = new ConversationViewModel
        {
            // Attention System - dynamic based on environment
            CurrentAttention = currentAttention,
            MaxAttention = maxAttention,
            AttentionNarrative = attentionNarrative,

            // Location Context
            LocationName = (location?.Name ?? "Unknown Location") + (player.CurrentLocationSpot != null ? $" - {player.CurrentLocationSpot.Name}" : ""),
            LocationAtmosphere = GenerateLocationAtmosphere(location),
            LocationPath = GetLocationPath(player.CurrentLocationSpot),

            // Character Focus
            CharacterName = npc.Name,
            CharacterState = GetNPCBodyLanguage(npc),

            // Bottom Status
            CurrentLocation = player.CurrentLocationSpot?.Name ?? "Unknown",
            QueueStatus = $"{player.LetterQueue.Count(l => l != null)}/8",
            CoinStatus = $"{player.Coins}s",
            CurrentTime = FormatGameTime()
        };

        // Add relationship status
        Dictionary<ConnectionType, int> tokens = _connectionTokenManager.GetTokensWithNPC(npcId);
        foreach (KeyValuePair<ConnectionType, int> token in tokens)
        {
            string dots = new string('●', Math.Min(5, Math.Max(0, token.Value)));
            string empty = new string('○', 5 - dots.Length);
            viewModel.RelationshipStatus[token.Key.ToString()] = $"{token.Key} {dots}{empty}";
        }

        // Add deadline pressure
        Letter? urgentLetter = player.LetterQueue
            .Where(l => l != null && l.State == LetterState.Collected)
            .OrderBy(l => l.DeadlineInHours)
            .FirstOrDefault();

        if (urgentLetter != null && urgentLetter.DeadlineInHours <= 72)
        {
            viewModel.DeadlinePressure = $"⚡ {urgentLetter.SenderName}: {urgentLetter.DeadlineInHours / 24}d";
        }

        return viewModel;
    }

    /// <summary>
    /// Get location screen data for the mockup-style UI
    /// </summary>
    public LocationScreenViewModel GetLocationScreen()
    {
        Player player = _gameWorld.GetPlayer();
        Location? location = player.GetCurrentLocation(_locationRepository);
        LocationSpot? spot = player.CurrentLocationSpot;

        LocationScreenViewModel viewModel = new LocationScreenViewModel
        {
            CurrentTime = FormatGameTime(),
            DeadlineTimer = GetDeadlineTimer(),
            LocationPath = GetLocationPath(spot),
            LocationName = spot?.Name ?? "Unknown Location",
            AtmosphereText = GenerateLocationAtmosphere(location)
        };

        // Tags removed - atmosphere is now calculated from NPC presence instead

        // Generate actions using ActionGenerator
        if (_actionGenerator != null && location != null)
        {
            viewModel.QuickActions = _actionGenerator.GenerateActionsForLocation(location, spot);
        }

        // Ensure QuickActions is initialized
        if (viewModel.QuickActions == null)
        {
            viewModel.QuickActions = new List<LocationActionViewModel>();
        }

        // Add NPCs present
        List<NPC> npcs = _npcRepository.GetNPCsForLocationSpotAndTime(spot?.SpotID ?? "", _timeManager.GetCurrentTimeBlock());
        foreach (NPC npc in npcs)
        {
            NPCPresenceViewModel npcPresence = new NPCPresenceViewModel
            {
                Id = npc.ID,  // Add NPC ID for conversation initiation
                Name = npc.Name,
                MoodEmoji = GetNPCMoodEmoji(npc),
                Description = GetNPCDescription(npc)
            };

            npcPresence.Interactions.Add(new InteractionOptionViewModel
            {
                Text = "Start conversation",
                Cost = "10 min"
            });

            viewModel.NPCsPresent.Add(npcPresence);
        }

        // Calculate current attention for observation checks
        int baseAttention = 3;
        bool isCrowded = npcs.Count > 3 || location?.Population?.GetPropertyValue() == "Crowded";
        int attentionModifier = isCrowded ? -1 : 0;
        int currentAttention = Math.Max(1, baseAttention + attentionModifier);

        // Use ObservationSystem to get dynamic observations (only if attention >= 2)
        if (_observationSystem != null && location != null && currentAttention >= 2)
        {
            List<ObservableViewModel> systemObservations = _observationSystem.GetObservations(location.Id);
            foreach (ObservableViewModel obs in systemObservations)
            {
                viewModel.Observations.Add(new ObservationViewModel
                {
                    Icon = obs.Icon,
                    Text = obs.Text,
                    IsUnknown = !obs.IsKnown
                });
            }
        }
        else if (_observationSystem != null && location != null)
        {
            // Limited observations when distracted
            viewModel.Observations.Add(new ObservationViewModel
            {
                Icon = "💭",
                Text = "Too distracted to notice details",
                IsUnknown = false
            });
        }
        else
        {
            // Fallback observations if system not available
            if (npcs.Any(n => n.Name == "Marcus"))
            {
                viewModel.Observations.Add(new ObservationViewModel
                {
                    Icon = "🛒",
                    Text = "Marcus heading to his stall",
                    IsUnknown = false
                });
            }
        }

        // Add ambient dialogue from NPCs (environmental storytelling)
        List<string> ambientComments = GetLocationAmbience();
        foreach (string? comment in ambientComments.Take(2)) // Limit to 2 comments to avoid clutter
        {
            viewModel.Observations.Insert(0, new ObservationViewModel
            {
                Icon = "💬",
                Text = comment,
                IsUnknown = false
            });
        }

        // CRITICAL FIX: Always add travel button as first action so players can navigate
        // Insert at position 0 to ensure it's always visible
        viewModel.QuickActions.Insert(0, new LocationActionViewModel
        {
            ActionType = "travel",
            Title = "Travel",
            Icon = "🗺️",
            Cost = "FREE",
            Detail = "Go to another location"
        });

        // Add route options from current location
        // CRITICAL FIX: Use the actual location ID, not the spot ID
        // Copper Kettle is a spot within Market Square
        string currentLocationId = location?.Id ?? "market_square"; // Default to market_square
        List<Location> allLocations = _locationRepository.GetAllLocations();

        Console.WriteLine($"[GameFacade] Getting routes from location: {currentLocationId}");
        List<RouteOption> availableRoutes = new List<RouteOption>();

        // Get ALL routes (available and locked) to show tier requirements
        // player is already declared at the top of this method
        List<RouteOption> allRoutesFromLocation = _routeRepository.GetRoutesFromLocation(currentLocationId)
            .Where(r => r.IsDiscovered) // Only show discovered routes
            .ToList();

        Console.WriteLine($"[GameFacade] Total discovered routes from {currentLocationId}: {allRoutesFromLocation.Count}");

        // Group routes by destination and create view models
        IEnumerable<IGrouping<string, RouteOption>> destinations = allRoutesFromLocation
            .GroupBy(r => r.Destination)
            .Take(5); // Show up to 5 destinations

        foreach (IGrouping<string, RouteOption>? destGroup in destinations)
        {
            RouteOption cheapestRoute = destGroup.OrderBy(r => r.BaseStaminaCost).First();
            Location destination = _locationRepository.GetLocation(cheapestRoute.Destination);

            // Check if route is accessible
            bool isLocked = false;
            string lockReason = "";

            // Check tier requirement
            if (cheapestRoute.TierRequired > player.CurrentTier && !cheapestRoute.HasPermitUnlock)
            {
                isLocked = true;
                lockReason = cheapestRoute.TierRequired switch
                {
                    TierLevel.T2 => "Requires T2 (Associate)",
                    TierLevel.T3 => "Requires T3 (Confidant)",
                    _ => "Tier requirement not met"
                };
            }
            // Check access requirements
            else if (cheapestRoute.AccessRequirement != null && !cheapestRoute.AccessRequirement.HasReceivedPermit)
            {
                isLocked = true;
                lockReason = "Needs Transport Permit";
            }

            viewModel.Routes.Add(new RouteOptionViewModel
            {
                RouteId = cheapestRoute.Id,
                Destination = destination?.Name ?? cheapestRoute.Destination,
                TravelTime = $"{cheapestRoute.TravelTimeHours * 60} min",
                Detail = cheapestRoute.Description ?? "",
                IsLocked = isLocked,
                LockReason = lockReason,
                RequiredTier = cheapestRoute.TierRequired,
                CanUnlockWithPermit = cheapestRoute.TierRequired > player.CurrentTier
            });
        }

        return viewModel;
    }

    private string GenerateLocationAtmosphere(Location location)
    {
        if (location == null) return "A quiet place.";

        string timeOfDay = (_timeManager.GetCurrentTimeBlock() == TimeBlocks.Morning || _timeManager.GetCurrentTimeBlock() == TimeBlocks.Afternoon) ? "day" : "evening";
        string activity = location?.AvailableServices?.FirstOrDefault() switch
        {
            ServiceTypes.Trade => "Bustling crowds. Merchant calls.",
            ServiceTypes.Rest => "Warm firelight. Clinking mugs.",
            _ => "People moving with purpose."
        };

        return $"{activity} The {timeOfDay} continues.";
    }

    private List<string> GetLocationPath(LocationSpot spot)
    {
        List<string> path = new List<string>();

        if (spot != null)
        {
            Location location = _locationRepository.GetLocation(spot.LocationId);
            if (location != null)
            {
                path.Add(location.Name);
                if (spot.Name != location.Name)
                {
                    path.Add(spot.Name);
                }
            }
        }

        return path;
    }

    private string FormatGameTime()
    {
        // Use TimeManager's consistent formatting
        return _timeManager.GetFormattedTimeDisplay();
    }

    private string GetDeadlineTimer()
    {
        Player player = _gameWorld.GetPlayer();
        Letter? urgentLetter = player.LetterQueue
            .Where(l => l != null && l.State == LetterState.Collected)
            .OrderBy(l => l.DeadlineInHours)
            .FirstOrDefault();

        if (urgentLetter != null && urgentLetter.DeadlineInHours <= 48)
        {
            int hours = urgentLetter.DeadlineInHours;
            return $"⚡ {hours}h until {urgentLetter.SenderName}";
        }

        return "";
    }

    private string GetNPCBodyLanguage(NPC npc)
    {
        // Simplified body language based on tokens
        Dictionary<ConnectionType, int> tokens = _connectionTokenManager.GetTokensWithNPC(npc.ID);
        int trustTokens = tokens.TryGetValue(ConnectionType.Trust, out int trust) ? trust : 0;

        if (trustTokens < -2) return "arms crossed, looking away";
        else if (trustTokens < 1) return "watching you carefully";
        else if (trustTokens < 3) return "leaning forward with interest";
        else return "hands clasped together anxiously";
    }

    private string GetNPCMoodEmoji(NPC npc)
    {
        // Simplified mood calculation based on tokens
        Dictionary<ConnectionType, int> tokens = _connectionTokenManager.GetTokensWithNPC(npc.ID);
        int trustTokens = tokens.TryGetValue(ConnectionType.Trust, out int trust) ? trust : 0;

        NPCEmotionalState state;
        if (trustTokens < -2) state = NPCEmotionalState.HOSTILE;
        else if (trustTokens < 1) state = NPCEmotionalState.WITHDRAWN;
        else if (trustTokens < 3) state = NPCEmotionalState.CALCULATING;
        else state = NPCEmotionalState.DESPERATE;
        return state switch
        {
            NPCEmotionalState.DESPERATE => "😰",
            NPCEmotionalState.HOSTILE => "😠",
            NPCEmotionalState.CALCULATING => "🤔",
            NPCEmotionalState.WITHDRAWN => "😐",
            _ => "😊"
        };
    }

    private string GetNPCDescription(NPC npc)
    {
        // Simplified description based on tokens
        Dictionary<ConnectionType, int> tokens = _connectionTokenManager.GetTokensWithNPC(npc.ID);
        int trustTokens = tokens.TryGetValue(ConnectionType.Trust, out int trust) ? trust : 0;

        NPCEmotionalState state;
        if (trustTokens < -2) state = NPCEmotionalState.HOSTILE;
        else if (trustTokens < 1) state = NPCEmotionalState.WITHDRAWN;
        else if (trustTokens < 3) state = NPCEmotionalState.CALCULATING;
        else state = NPCEmotionalState.DESPERATE;
        return state switch
        {
            NPCEmotionalState.DESPERATE => $"Looking anxious, clearly needs help",
            NPCEmotionalState.HOSTILE => $"Avoiding eye contact, seems upset",
            NPCEmotionalState.CALCULATING => $"Thoughtful expression, considering options",
            NPCEmotionalState.WITHDRAWN => $"Going about their business",
            _ => $"Available to talk"
        };
    }

    /// <summary>
    /// Get conversation choices from NPC's card deck for frontend integration
    /// </summary>
    public async Task<List<ConversationChoice>> GetConversationChoicesFromDeckAsync(string npcId)
    {
        try
        {
            // Get the current conversation if available
            ConversationManager currentConversation = _conversationStateManager.PendingConversationManager;
            if (currentConversation != null && currentConversation.Context?.TargetNPC?.ID == npcId)
            {
                // Use existing conversation's choices if available
                if (currentConversation.Choices?.Any() == true)
                {
                    return currentConversation.Choices;
                }
            }

            // Create new choice generator to get card-based choices
            Player player = _gameWorld.GetPlayer();
            NPC npc = _npcRepository.GetById(npcId);
            if (npc == null)
            {
                Console.WriteLine($"[GameFacade] NPC not found: {npcId}");
                return new List<ConversationChoice>();
            }

            // Create context for choice generation
            Location location = player.GetCurrentLocation(_locationRepository);
            LocationSpot spot = player.CurrentLocationSpot;
            SceneContext context = SceneContext.Standard(_gameWorld, player, npc, location, spot);

            // Create choice generator
            ConversationChoiceGenerator choiceGenerator = new ConversationChoiceGenerator(
                _letterQueueManager,
                _connectionTokenManager,
                _npcStateResolver,
                _timeManager,
                player,
                _gameWorld,
                _timeBlockAttentionManager,
                _deckFactory);

            // Generate choices from card deck
            ConversationState state = new ConversationState(player, npc, _gameWorld, 3, 8); // Basic state for choice generation
            List<ConversationChoice> choices = choiceGenerator.GenerateChoices(context, state);

            Console.WriteLine($"[GameFacade] Generated {choices.Count} card-based choices for {npcId}");
            return choices;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GameFacade] Error generating card choices for {npcId}: {ex.Message}");
            return new List<ConversationChoice>();
        }
    }

    #endregion

}

public class ReadableLetterInfo
{
    public string ItemId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsRead { get; set; }
    public bool HasSpecialEffect { get; set; }
}