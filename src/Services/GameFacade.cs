using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// GameFacade - Pure orchestrator for UI-Backend communication.
/// Delegates ALL business logic to specialized facades.
/// Coordinates cross-facade operations and handles UI-specific orchestration.
/// </summary>
public class GameFacade
{
    private readonly GameWorld _gameWorld;
    private readonly MessageSystem _messageSystem;
    private readonly SocialFacade _conversationFacade;
    private readonly LocationFacade _locationFacade;
    private readonly ResourceFacade _resourceFacade;
    private readonly TimeFacade _timeFacade;
    private readonly TravelFacade _travelFacade;
    private readonly TokenFacade _tokenFacade;
    private readonly NarrativeFacade _narrativeFacade;
    private readonly ExchangeFacade _exchangeFacade;
    private readonly MentalFacade _mentalFacade;
    private readonly PhysicalFacade _physicalFacade;
    private readonly InvestigationActivity _investigationActivity;
    private readonly InvestigationDiscoveryEvaluator _investigationDiscoveryEvaluator;
    private readonly KnowledgeService _knowledgeService;

    public GameFacade(
        GameWorld gameWorld,
        MessageSystem messageSystem,
        SocialFacade conversationFacade,
        LocationFacade locationFacade,
        ResourceFacade resourceFacade,
        TimeFacade timeFacade,
        TravelFacade travelFacade,
        TokenFacade tokenFacade,
        NarrativeFacade narrativeFacade,
        ExchangeFacade exchangeFacade,
        MentalFacade mentalFacade,
        PhysicalFacade physicalFacade,
        InvestigationActivity investigationActivity,
        InvestigationDiscoveryEvaluator investigationDiscoveryEvaluator,
        KnowledgeService knowledgeService)
    {
        _gameWorld = gameWorld;
        _messageSystem = messageSystem;
        _conversationFacade = conversationFacade;
        _locationFacade = locationFacade;
        _resourceFacade = resourceFacade;
        _timeFacade = timeFacade;
        _travelFacade = travelFacade;
        _tokenFacade = tokenFacade;
        _narrativeFacade = narrativeFacade;
        _exchangeFacade = exchangeFacade;
        _mentalFacade = mentalFacade;
        _physicalFacade = physicalFacade;
        _investigationActivity = investigationActivity;
        _investigationDiscoveryEvaluator = investigationDiscoveryEvaluator;
        _knowledgeService = knowledgeService;
    }

    // ========== CORE GAME STATE ==========

    public Player GetPlayer()
    {
        return _gameWorld.GetPlayer();
    }

    public List<SystemMessage> GetSystemMessages()
    {
        return _messageSystem.GetMessages();
    }

    public void ClearSystemMessages()
    {
        _messageSystem.ClearMessages();
    }

    public MessageSystem GetMessageSystem()
    {
        return _messageSystem;
    }


    // ========== PLAYER STATS OPERATIONS ==========

    public PlayerStats GetPlayerStats()
    {
        return _gameWorld.GetPlayer().Stats;
    }

    public List<NPC> GetAvailableStrangers(string locationId)
    {
        TimeBlocks currentTime = _timeFacade.GetCurrentTimeBlock();
        return _gameWorld.GetAvailableStrangers(locationId, currentTime);
    }

    public SocialChallengeContext StartStrangerConversation(string strangerId)
    {
        // Find the stranger
        NPC stranger = _gameWorld.GetStrangerById(strangerId);

        if (stranger == null || !stranger.HasAvailableRequests())
        {
            return null;
        }

        // Get the first available request (strangers have single request)
        GoalCard request = stranger.GetAvailableRequests().FirstOrDefault();
        if (request == null || string.IsNullOrEmpty(request.ChallengeTypeId))
        {
            return null;
        }

        // THREE PARALLEL SYSTEMS: Get Social engagement type
        if (!_gameWorld.SocialChallengeTypes.TryGetValue(request.ChallengeTypeId, out SocialChallengeType challengeType))
        {
            return null;
        }

        // Mark stranger as encountered
        stranger.MarkAsEncountered();

        // Create conversation context (goal cards now handle domain logic)
        SocialChallengeContext context = new SocialChallengeContext
        {
            IsValid = true,
            Npc = stranger,
            NpcId = stranger.ID,
            ConversationTypeId = challengeType.Id,  // Using SocialChallengeType
            RequestId = request.Id,
            RequestText = request.Description,
            InitialState = ConnectionState.DISCONNECTED, // Strangers always start disconnected
        };

        return context;
    }

    public bool CanAffordStrangerConversation(string requestId)
    {
        // Find stranger with this request
        foreach (NPC stranger in _gameWorld.GetAllStrangers())
        {
            GoalCard request = stranger.GetRequestById(requestId);
            if (request != null && !string.IsNullOrEmpty(request.ChallengeTypeId))
            {
                // THREE PARALLEL SYSTEMS: Check if Social engagement type exists
                if (_gameWorld.SocialChallengeTypes.ContainsKey(request.ChallengeTypeId))
                {
                    return true; // No attention cost check needed
                }
            }
        }
        return false;
    }


    public List<InvestigationApproach> GetAvailableInvestigationApproaches()
    {
        Player player = _gameWorld.GetPlayer();
        return _locationFacade.GetAvailableApproaches(player);
    }


    // ========== LOCATION OPERATIONS ==========

    public Location GetCurrentLocation()
    {
        return _locationFacade.GetCurrentLocation();
    }

    public LocationSpot GetLocationSpot(string spotId)
    {
        return _gameWorld.GetSpot(spotId);
    }

    public LocationSpot GetCurrentLocationSpot()
    {
        return _locationFacade.GetCurrentLocationSpot();
    }

    public Location GetLocationById(string locationId)
    {
        return _locationFacade.GetLocationById(locationId);
    }

    public bool MoveToSpot(string spotName)
    {
        bool success = _locationFacade.MoveToSpot(spotName);

        // Movement to new location may unlock investigation discovery (ImmediateVisibility, EnvironmentalObservation triggers)
        if (success)
        {
            EvaluateInvestigationDiscovery();
        }

        return success;
    }

    public LocationFacade GetLocationFacade()
    {
        return _locationFacade;
    }

    public NPC GetNPCById(string npcId)
    {
        return _locationFacade.GetNPCById(npcId);
    }

    public List<NPC> GetNPCsAtLocation(string locationId)
    {
        return _locationFacade.GetNPCsAtLocation(locationId);
    }

    public List<NPC> GetNPCsAtCurrentSpot()
    {
        return _locationFacade.GetNPCsAtCurrentSpot();
    }

    public LocationScreenViewModel GetLocationScreen()
    {
        List<NPCConversationOptions> npcConversationOptions = GetNPCConversationOptionsForCurrentLocation();
        return _locationFacade.GetLocationScreen(npcConversationOptions);
    }

    private List<NPCConversationOptions> GetNPCConversationOptionsForCurrentLocation()
    {
        List<NPC> npcs = _locationFacade.GetNPCsAtCurrentSpot();
        List<NPCConversationOptions> options = new List<NPCConversationOptions>();

        foreach (NPC npc in npcs)
        {
            List<SocialChallengeOption> conversationOptions = _conversationFacade.GetAvailableConversationOptions(npc);
            List<string> conversationTypeIds = conversationOptions.Select(opt => opt.ChallengeTypeId).Distinct().ToList();

            options.Add(new NPCConversationOptions
            {
                NpcId = npc.ID,
                NpcName = npc.Description,
                AvailableTypes = conversationTypeIds,
                CanAfford = true
            });
        }

        return options;
    }

    // ========== TIME OPERATIONS ==========

    public TimeInfo GetTimeInfo()
    {
        return _timeFacade.GetTimeInfo();
    }


    public string GetFormattedTimeDisplay()
    {
        return _timeFacade.GetFormattedTimeDisplay();
    }

    public int GetSegmentsInCurrentPeriod()
    {
        return _timeFacade.GetSegmentsInCurrentPeriod();
    }


    // ========== TRAVEL OPERATIONS ==========

    public List<TravelDestinationViewModel> GetTravelDestinations()
    {
        return _travelFacade.GetTravelDestinations();
    }

    public List<TravelDestinationViewModel> GetTravelDestinationsWithRoutes()
    {
        return GetTravelDestinations();
    }

    public async Task<bool> TravelToDestinationAsync(string routeId)
    {
        // Get all routes and find the one with matching ID
        List<RouteOption> allRoutes = _travelFacade.GetAvailableRoutesFromCurrentLocation();
        RouteOption? targetRoute = allRoutes.FirstOrDefault(r => r.Id == routeId);

        if (targetRoute == null)
        {
            _narrativeFacade.AddSystemMessage($"Route {routeId} not found", SystemMessageTypes.Danger);
            return false;
        }

        Player player = _gameWorld.GetPlayer();
        TimeBlocks currentTimeBlock = _timeFacade.GetCurrentTimeBlock();

        // Travel does NOT cost attention - only time, coins, and hunger from the route

        // 1. SIMPLIFIED ACCESS CHECK - just use CanTravel which combines all checks
        // Note: This is simplified for now - full implementation would check access requirements separately
        // The RouteOption.CanTravel method already checks terrain requirements internally

        // 2. CALCULATE ACTUAL HUNGER COST
        // Base hunger cost from route plus any load penalties
        int itemCount = player.Inventory.GetAllItems().Count(i => !string.IsNullOrEmpty(i));
        int hungerCost = targetRoute.BaseStaminaCost; // This is actually the hunger cost in the data

        // Add load penalties if carrying many items
        if (itemCount > 3) // Light load threshold
        {
            hungerCost += (itemCount - 3); // +1 hunger per item over light load
        }

        // 5. CHECK HUNGER CAPACITY
        if (player.Hunger + hungerCost > player.MaxHunger)
        {
            _narrativeFacade.AddSystemMessage(
                $"Too exhausted to travel. Travel costs {hungerCost} hunger, but you have {player.Hunger}/{player.MaxHunger} hunger",
                SystemMessageTypes.Warning);
            return false;
        }

        // 6. CHECK COIN COST
        int coinCost = targetRoute.BaseCoinCost;
        if (coinCost > 0 && player.Coins < coinCost)
        {
            _narrativeFacade.AddSystemMessage($"Not enough coins. Need {coinCost}, have {player.Coins}", SystemMessageTypes.Warning);
            return false;
        }

        // === ALL CHECKS PASSED - APPLY COSTS AND EXECUTE TRAVEL ===

        // 7. APPLY ALL COSTS
        if (coinCost > 0)
        {
            _resourceFacade.SpendCoins(coinCost);
        }
        _resourceFacade.IncreaseHunger(hungerCost, "Travel fatigue");
        // Travel does NOT cost attention - removed incorrect attention spending

        // Create travel result for processing
        int travelTime = targetRoute.TravelTimeSegments;
        TravelResult travelResult = new TravelResult
        {
            Success = true,
            TravelTimeSegments = travelTime,
            CoinCost = coinCost,
            RouteId = routeId,
            TransportMethod = targetRoute.Method
        };

        if (travelResult.Success)
        {

            // Get the actual destination spot from the route
            RouteOption? actualRoute = _travelFacade.GetAvailableRoutesFromCurrentLocation()
                .FirstOrDefault(r => r.Id == routeId);

            if (actualRoute != null)
            {
                // Find the destination spot by its ID from GameWorld's Spots dictionary
                LocationSpot? destSpot = _gameWorld.GetSpot(actualRoute.DestinationLocationSpot);

                if (destSpot != null)
                {
                    player.CurrentLocationSpot = destSpot;
                    Console.WriteLine($"[GameFacade] Player moved to spot: {destSpot.SpotID} at location: {destSpot.LocationId}");
                }
            }

            TimeBlocks oldTimeBlock = _timeFacade.GetCurrentTimeBlock();
            TimeBlocks newTimeBlock = _timeFacade.AdvanceSegments(travelResult.SegmentCost);

            ProcessTimeAdvancement(new TimeAdvancementResult
            {
                OldTimeBlock = oldTimeBlock,
                NewTimeBlock = newTimeBlock,
                CrossedTimeBlock = oldTimeBlock != newTimeBlock,
                SegmentsAdvanced = travelResult.SegmentCost
            });

            // Get destination location name for the message
            LocationSpot? finalDestSpot = _gameWorld.GetSpot(targetRoute.DestinationLocationSpot);

            string destinationName = "Unknown";
            if (finalDestSpot != null)
            {
                Location? destLocation = _gameWorld.WorldState.locations
                    ?.FirstOrDefault(l => l.Id == finalDestSpot.LocationId);
                destinationName = destLocation?.Name ?? finalDestSpot.Name;
            }

            _narrativeFacade.AddSystemMessage($"Traveled to {destinationName}", SystemMessageTypes.Info);
        }

        return travelResult.Success;
    }

    // ========== RESOURCE OPERATIONS ==========

    public InventoryViewModel GetInventory()
    {
        return _resourceFacade.GetInventoryViewModel();
    }

    public async Task<WorkResult> PerformWork()
    {
        WorkResult result = _resourceFacade.PerformWork();
        if (result.Success)
        {
            TimeBlocks oldTimeBlock = _timeFacade.GetCurrentTimeBlock();
            TimeBlocks newTimeBlock = _timeFacade.JumpToNextPeriod(); // Work takes 4 segments (full period)

            ProcessTimeAdvancement(new TimeAdvancementResult
            {
                OldTimeBlock = oldTimeBlock,
                NewTimeBlock = newTimeBlock,
                CrossedTimeBlock = oldTimeBlock != newTimeBlock,
                SegmentsAdvanced = 4
            });
        }
        return result;
    }

    // ========== CONVERSATION OPERATIONS ==========

    public SocialFacade GetConversationFacade()
    {
        return _conversationFacade;
    }

    public async Task<SocialChallengeContext> CreateConversationContext(string npcId, string requestId)
    {
        return await _conversationFacade.CreateConversationContext(npcId, requestId);
    }

    public List<SocialChallengeOption> GetAvailableConversationOptions(string npcId)
    {
        NPC? npc = _gameWorld.NPCs.FirstOrDefault(n => n.ID == npcId);
        if (npc == null)
            return new List<SocialChallengeOption>();

        return _conversationFacade.GetAvailableConversationOptions(npc);
    }

    /// <summary>
    /// Play a conversation card - proper architectural flow through GameFacade
    /// </summary>
    public async Task<SocialTurnResult> PlayConversationCard(CardInstance card)
    {
        if (_conversationFacade == null)
            throw new InvalidOperationException("ConversationFacade not available");

        if (!_conversationFacade.IsConversationActive())
            throw new InvalidOperationException("No active conversation");

        return await _conversationFacade.ExecuteSpeakSingleCard(card);
    }

    /// <summary>
    /// Execute listen action in current conversation - proper architectural flow through GameFacade
    /// </summary>
    public async Task<SocialTurnResult> ExecuteListen()
    {
        if (_conversationFacade == null)
            throw new InvalidOperationException("ConversationFacade not available");

        if (!_conversationFacade.IsConversationActive())
            throw new InvalidOperationException("No active conversation");

        return await _conversationFacade.ExecuteListen();
    }

    /// <summary>
    /// Check if a conversation is currently active
    /// </summary>
    public bool IsConversationActive()
    {
        return _conversationFacade?.IsConversationActive() ?? false;
    }

    /// <summary>
    /// Check if a card can be played in the current conversation
    /// </summary>
    public bool CanPlayCard(CardInstance card, SocialSession session)
    {
        if (_conversationFacade == null) return false;
        return _conversationFacade.CanPlayCard(card, session);
    }

    /// <summary>
    /// End the current conversation and return the outcome
    /// </summary>
    public SocialChallengeOutcome EndConversation()
    {
        if (_conversationFacade == null) return null;
        return _conversationFacade.EndConversation();
    }

    // ========== MENTAL TACTICAL SYSTEM OPERATIONS ==========

    public MentalFacade GetMentalFacade()
    {
        return _mentalFacade;
    }

    /// <summary>
    /// Start a new Mental tactical session with specified engagement type
    /// Strategic-Tactical Integration Point
    /// </summary>
    public MentalSession StartMentalSession(string challengeTypeId)
    {
        if (_mentalFacade == null)
            throw new InvalidOperationException("MentalFacade not available");

        if (_mentalFacade.IsSessionActive())
            throw new InvalidOperationException("Mental session already active");

        if (!_gameWorld.MentalChallengeTypes.TryGetValue(challengeTypeId, out MentalChallengeType challengeType))
            throw new InvalidOperationException($"MentalChallengeType {challengeTypeId} not found");

        Player player = _gameWorld.GetPlayer();
        string currentLocationId = player.CurrentLocationSpot?.LocationId;

        // Build deck with signature deck knowledge cards in starting hand
        (List<CardInstance> deck, List<CardInstance> startingHand) = _mentalFacade.GetDeckBuilder()
            .BuildDeckWithStartingHand(challengeType, currentLocationId, player);

        return _mentalFacade.StartSession(challengeType, deck, startingHand, currentLocationId, goalId, investigationId);
    }

    /// <summary>
    /// Execute observe action in current Mental investigation
    /// </summary>
    public async Task<MentalTurnResult> ExecuteObserve(CardInstance card)
    {
        if (_mentalFacade == null)
            throw new InvalidOperationException("MentalFacade not available");

        if (!_mentalFacade.IsSessionActive())
            throw new InvalidOperationException("No active mental session");

        return await _mentalFacade.ExecuteObserve(card);
    }

    /// <summary>
    /// Execute act action in current Mental investigation
    /// </summary>
    public async Task<MentalTurnResult> ExecuteAct(CardInstance card)
    {
        if (_mentalFacade == null)
            throw new InvalidOperationException("MentalFacade not available");

        if (!_mentalFacade.IsSessionActive())
            throw new InvalidOperationException("No active mental session");

        return await _mentalFacade.ExecuteAct(card);
    }

    /// <summary>
    /// End the current mental investigation and return the outcome
    /// </summary>
    public MentalOutcome EndMentalSession()
    {
        if (_mentalFacade == null) return null;
        return _mentalFacade.EndSession();
    }

    /// <summary>
    /// Check if a mental session is currently active
    /// </summary>
    public bool IsMentalSessionActive()
    {
        return _mentalFacade?.IsSessionActive() ?? false;
    }

    // ========== PHYSICAL TACTICAL SYSTEM OPERATIONS ==========

    public PhysicalFacade GetPhysicalFacade()
    {
        return _physicalFacade;
    }

    /// <summary>
    /// Start a new Physical tactical session with specified engagement type
    /// Strategic-Tactical Integration Point
    /// </summary>
    public PhysicalSession StartPhysicalSession(string challengeTypeId)
    {
        if (_physicalFacade == null)
            throw new InvalidOperationException("PhysicalFacade not available");

        if (_physicalFacade.IsSessionActive())
            throw new InvalidOperationException("Physical session already active");

        if (!_gameWorld.PhysicalChallengeTypes.TryGetValue(challengeTypeId, out PhysicalChallengeType challengeType))
            throw new InvalidOperationException($"PhysicalChallengeType {challengeTypeId} not found");

        Player player = _gameWorld.GetPlayer();
        string currentLocationId = player.CurrentLocationSpot?.LocationId;

        // Build deck with signature deck knowledge cards in starting hand
        (List<CardInstance> deck, List<CardInstance> startingHand) = _physicalFacade.GetDeckBuilder()
            .BuildDeckWithStartingHand(challengeType, currentLocationId, player);

        return _physicalFacade.StartSession(challengeType, deck, startingHand, currentLocationId, goalId, investigationId);
    }

    /// <summary>
    /// Execute assess action in current Physical challenge
    /// </summary>
    public async Task<PhysicalTurnResult> ExecuteAssess(CardInstance card)
    {
        if (_physicalFacade == null)
            throw new InvalidOperationException("PhysicalFacade not available");

        if (!_physicalFacade.IsSessionActive())
            throw new InvalidOperationException("No active physical session");

        return await _physicalFacade.ExecuteAssess(card);
    }

    /// <summary>
    /// Execute execute action in current Physical challenge
    /// </summary>
    public async Task<PhysicalTurnResult> ExecuteExecute(CardInstance card)
    {
        if (_physicalFacade == null)
            throw new InvalidOperationException("PhysicalFacade not available");

        if (!_physicalFacade.IsSessionActive())
            throw new InvalidOperationException("No active physical session");

        return await _physicalFacade.ExecuteExecute(card);
    }

    /// <summary>
    /// End the current physical challenge and return the outcome
    /// </summary>
    public PhysicalOutcome EndPhysicalSession()
    {
        if (_physicalFacade == null) return null;
        return _physicalFacade.EndSession();
    }

    /// <summary>
    /// Check if a physical session is currently active
    /// </summary>
    public bool IsPhysicalSessionActive()
    {
        return _physicalFacade?.IsSessionActive() ?? false;
    }

    public async Task<ExchangeContext> CreateExchangeContext(string npcId)
    {
        // Get NPC
        NPC? npc = _gameWorld.NPCs.FirstOrDefault(n => n.ID == npcId);
        if (npc == null)
        {
            _messageSystem.AddSystemMessage($"NPC {npcId} not found", SystemMessageTypes.Danger);
            return null;
        }

        // Get current location
        Location? currentLocation = GetCurrentLocation();
        LocationSpot? currentSpot = GetCurrentLocationSpot();

        // Get current time block
        TimeBlocks timeBlock = _timeFacade.GetCurrentTimeBlock();

        // Get player resources and tokens
        PlayerResourceState playerResources = _gameWorld.GetPlayerResourceState();

        // Get player's tokens with this specific NPC
        Dictionary<ConnectionType, int> npcTokens = _tokenFacade.GetTokensWithNPC(npcId);

        // Get relationship tier with this NPC
        RelationshipTier relationshipTier = _tokenFacade.GetRelationshipTier(npcId);

        // Get available exchanges from ExchangeFacade - now orchestrating properly
        List<ExchangeOption> availableExchanges = _exchangeFacade.GetAvailableExchanges(npcId, playerResources, npcTokens, relationshipTier);

        if (!availableExchanges.Any())
        {
            _messageSystem.AddSystemMessage($"{npc.Name} has no exchanges available", SystemMessageTypes.Info);
            return null;
        }

        // Create exchange session through ExchangeFacade
        ExchangeSession session = _exchangeFacade.CreateExchangeSession(npcId);
        if (session == null)
        {
            _messageSystem.AddSystemMessage($"Could not create exchange session with {npc.Name}", SystemMessageTypes.Danger);
            return null;
        }

        // Build the context
        ExchangeContext context = new ExchangeContext
        {
            NpcInfo = new NpcInfo
            {
                NpcId = npc.ID,
                Name = npc.Name,
                TokenCounts = _tokenFacade.GetTokensWithNPC(npc.ID)
            },
            LocationInfo = new LocationInfo
            {
                LocationId = currentSpot?.SpotID ?? "",
                Name = currentLocation?.Name ?? "",
                Description = currentLocation?.Description ?? ""
            },
            CurrentTimeBlock = timeBlock,
            PlayerResources = playerResources,
            PlayerTokens = npcTokens,
            Session = new ExchangeSession
            {
                NpcId = npcId,
                LocationId = currentSpot?.SpotID ?? "",
                AvailableExchanges = availableExchanges
            }
        };

        return await Task.FromResult(context);
    }

    // ========== NARRATIVE OPERATIONS ==========

    public List<TakenObservation> GetTakenObservations()
    {
        // ARCHITECTURE: Observations are stored per-NPC, not globally
        // This method returns empty as there's no global observation list
        // Each NPC tracks their own observation cards in their ObservationDeck
        return new List<TakenObservation>();
    }

    // ========== GAME INITIALIZATION ==========

    /// <summary>
    /// Initializes the game state. MUST be idempotent due to Blazor ServerPrerendered mode.
    /// 
    /// CRITICAL: Blazor ServerPrerendered causes ALL components to render TWICE:
    /// 1. During server-side prerendering (static HTML generation)
    /// 2. After establishing interactive SignalR connection
    /// 
    /// This means OnInitializedAsync() runs twice, so this method MUST:
    /// - Check IsGameStarted flag to prevent duplicate initialization
    /// - NOT perform any side effects that shouldn't happen twice
    /// - NOT add duplicate messages to the UI
    /// </summary>
    public async Task StartGameAsync()
    {
        // Check if game is already started to prevent duplicate initialization
        // This is CRITICAL for ServerPrerendered mode compatibility
        if (_gameWorld.IsGameStarted)
        {
            Console.WriteLine($"[GameFacade.StartGameAsync] Game already started, skipping initialization");
            return;
        }

        // Initialize player at starting location from GameWorld initial conditions
        Player player = _gameWorld.GetPlayer();
        string startingSpotId = _gameWorld.InitialLocationSpotId ?? "courtyard";
        LocationSpot? startingSpot = _gameWorld.Spots.GetAllSpots().FirstOrDefault(s => s.SpotID == startingSpotId);
        if (startingSpot != null)
        {
            player.CurrentLocationSpot = startingSpot;
            Location? startingLocation = _gameWorld.WorldState.locations.FirstOrDefault(l => l.Id == startingSpot.LocationId);
            Console.WriteLine($"[GameFacade.StartGameAsync] Player initialized at {startingLocation?.Name ?? "Unknown"} - {startingSpot.Name}");
        }
        else
        {
            Console.WriteLine($"[GameFacade.StartGameAsync] WARNING: Starting spot '{startingSpotId}' not found!");
        }

        // Initialize player resources from GameWorld initial player config
        if (_gameWorld.InitialPlayerConfig != null)
        {
            player.Coins = _gameWorld.InitialPlayerConfig.Coins ?? 20;
            player.Health = _gameWorld.InitialPlayerConfig.Health ?? 10;
            player.MaxHealth = _gameWorld.InitialPlayerConfig.MaxHealth ?? 10;
            player.Hunger = _gameWorld.InitialPlayerConfig.Hunger ?? 0;
            player.Stamina = _gameWorld.InitialPlayerConfig.StaminaPoints ?? 12;
            player.MaxStamina = _gameWorld.InitialPlayerConfig.MaxStamina ?? 12;
        }

        Console.WriteLine($"[GameFacade.StartGameAsync] Player resources initialized - Coins: {player.Coins}, Health: {player.Health}, Stamina: {player.Stamina}, Hunger: {player.Hunger}");

        // Initialize exchange inventories
        _exchangeFacade.InitializeNPCExchanges();
        Console.WriteLine($"[GameFacade.StartGameAsync] Exchange inventories initialized");

        // Mark game as started
        _gameWorld.IsGameStarted = true;

        _messageSystem.AddSystemMessage("Game started", SystemMessageTypes.Success);
    }

    // ========== INTENT PROCESSING ==========

    public async Task<bool> ProcessIntent(PlayerIntent intent)
    {
        return intent switch
        {
            TravelIntent travel => await TravelToDestinationAsync(travel.RouteId),
            MoveIntent move => MoveToSpot(move.TargetSpotId),
            WaitIntent => ProcessWaitIntent(),
            RestIntent rest => ProcessRestIntent(rest.Segments),
            _ => ProcessGenericIntent(intent)
        };
    }

    private bool ProcessWaitIntent()
    {
        TimeBlocks oldTimeBlock = _timeFacade.GetCurrentTimeBlock();
        TimeBlocks newTimeBlock = _timeFacade.AdvanceSegments(1); // Wait costs 1 segment

        ProcessTimeAdvancement(new TimeAdvancementResult
        {
            OldTimeBlock = oldTimeBlock,
            NewTimeBlock = newTimeBlock,
            CrossedTimeBlock = oldTimeBlock != newTimeBlock,
            SegmentsAdvanced = 1
        });

        _narrativeFacade.AddSystemMessage("You wait and time passes", SystemMessageTypes.Info);
        return true;
    }

    private bool ProcessRestIntent(int segments)
    {
        TimeBlocks oldTimeBlock = _timeFacade.GetCurrentTimeBlock();
        TimeBlocks newTimeBlock = _timeFacade.AdvanceSegments(segments);

        ProcessTimeAdvancement(new TimeAdvancementResult
        {
            OldTimeBlock = oldTimeBlock,
            NewTimeBlock = newTimeBlock,
            CrossedTimeBlock = oldTimeBlock != newTimeBlock,
            SegmentsAdvanced = segments
        });

        _narrativeFacade.AddSystemMessage($"You rest for {segments} segment(s)", SystemMessageTypes.Info);
        return true;
    }

    private bool ProcessGenericIntent(PlayerIntent intent)
    {
        _narrativeFacade.AddSystemMessage($"Intent type '{intent.GetType().Name}' not implemented", SystemMessageTypes.Warning);
        return false;
    }


    public ConnectionState GetNPCConnectionState(string npcId)
    {
        return ConnectionState.NEUTRAL;
    }

    public List<RouteOption> GetAvailableRoutes()
    {
        return _travelFacade.GetAvailableRoutesFromCurrentLocation();
    }


    public List<RouteOption> GetRoutesToDestination(string destinationId)
    {
        return new List<RouteOption>();
    }


    /// <summary>
    /// Get the LocationActionManager for managing location-specific actions.
    /// </summary>
    public LocationActionManager GetLocationActionManager()
    {
        return _locationFacade.GetLocationActionManager();
    }

    // ========== PRIVATE HELPERS ==========

    private void ProcessTimeAdvancement(TimeAdvancementResult result)
    {
        if (result.CrossedTimeBlock)
        {
            _resourceFacade.ProcessTimeBlockTransition(result.OldTimeBlock, result.NewTimeBlock);
        }
    }

    /// <summary>
    /// Gets the district containing a location
    /// </summary>
    public District GetDistrictForLocation(string locationId)
    {
        return _gameWorld.WorldState.GetDistrictForLocation(locationId);
    }

    /// <summary>
    /// Gets the region containing a district
    /// </summary>
    public Region GetRegionForDistrict(string districtId)
    {
        return _gameWorld.WorldState.GetRegionForDistrict(districtId);
    }

    /// <summary>
    /// Gets all locations in WorldState
    /// </summary>
    public List<Location> GetAllLocations()
    {
        return _gameWorld.WorldState.locations;
    }

    /// <summary>
    /// Gets a district by its ID
    /// </summary>
    public District GetDistrictById(string districtId)
    {
        return _gameWorld.WorldState.Districts.FirstOrDefault(d => d.Id == districtId);
    }


    // ============================================
    // DEBUG COMMANDS
    // ============================================

    /// <summary>
    /// Debug: Set player stat to specific level
    /// </summary>
    public bool DebugSetStatLevel(PlayerStatType statType, int level)
    {
        if (level < 1 || level > 5)
        {
            _messageSystem.AddSystemMessage($"Invalid stat level {level}. Must be 1-5.", SystemMessageTypes.Danger);
            return false;
        }

        Player player = _gameWorld.GetPlayer();

        // Since there's no SetLevel method, we need to manipulate the internal state
        // We'll add XP to reach the desired level
        int currentLevel = player.Stats.GetLevel(statType);

        if (currentLevel < level)
        {
            // Calculate XP needed to reach target level
            int totalXPNeeded = 0;
            for (int lvl = 1; lvl < level; lvl++)
            {
                totalXPNeeded += lvl switch
                {
                    1 => 10,
                    2 => 25,
                    3 => 50,
                    4 => 100,
                    _ => 0
                };
            }

            // Add enough XP to reach the target level
            player.Stats.AddXP(statType, totalXPNeeded);
        }

        _messageSystem.AddSystemMessage($"Set {statType} to level {level}", SystemMessageTypes.Success);
        return true;
    }

    /// <summary>
    /// Debug: Add XP to a specific stat
    /// </summary>
    public bool DebugAddStatXP(PlayerStatType statType, int xp)
    {
        if (xp <= 0)
        {
            _messageSystem.AddSystemMessage($"Invalid XP amount {xp}. Must be positive.", SystemMessageTypes.Danger);
            return false;
        }

        Player player = _gameWorld.GetPlayer();
        int oldLevel = player.Stats.GetLevel(statType);
        player.Stats.AddXP(statType, xp);
        int newLevel = player.Stats.GetLevel(statType);

        if (newLevel > oldLevel)
        {
            _messageSystem.AddSystemMessage($"Added {xp} XP to {statType}. LEVEL UP! Now level {newLevel}.", SystemMessageTypes.Success);
        }
        else
        {
            int currentXP = player.Stats.GetXP(statType);
            int required = player.Stats.GetXPToNextLevel(statType);
            _messageSystem.AddSystemMessage($"Added {xp} XP to {statType}. Progress: {currentXP}/{required}", SystemMessageTypes.Success);
        }

        return true;
    }

    /// <summary>
    /// Debug: Set all stats to a specific level
    /// </summary>
    public void DebugSetAllStats(int level)
    {
        if (level < 1 || level > 5)
        {
            _messageSystem.AddSystemMessage($"Invalid stat level {level}. Must be 1-5.", SystemMessageTypes.Danger);
            return;
        }

        foreach (PlayerStatType statType in Enum.GetValues(typeof(PlayerStatType)))
        {
            DebugSetStatLevel(statType, level);
        }

        _messageSystem.AddSystemMessage($"All stats set to level {level}", SystemMessageTypes.Success);
    }

    /// <summary>
    /// Debug: Display current stat levels and XP
    /// </summary>
    public string DebugGetStatInfo()
    {
        Player player = _gameWorld.GetPlayer();
        StringBuilder statInfo = new System.Text.StringBuilder();

        statInfo.AppendLine("=== Player Stats ===");
        foreach (PlayerStatType statType in Enum.GetValues(typeof(PlayerStatType)))
        {
            int level = player.Stats.GetLevel(statType);
            int xp = player.Stats.GetXP(statType);
            int required = player.Stats.GetXPToNextLevel(statType);

            statInfo.AppendLine($"{statType}: Level {level} ({xp}/{required} XP)");
        }

        return statInfo.ToString();
    }

    /// <summary>
    /// Debug: Grant resources (coins, health, etc.)
    /// </summary>
    public void DebugGiveResources(int coins = 0, int health = 0, int hunger = 0)
    {
        Player player = _gameWorld.GetPlayer();

        if (coins != 0)
        {
            player.Coins = Math.Max(0, player.Coins + coins);
            _messageSystem.AddSystemMessage($"Coins {(coins > 0 ? "+" : "")}{coins} (now {player.Coins})", SystemMessageTypes.Success);
        }

        if (health != 0)
        {
            player.Health = Math.Clamp(player.Health + health, 0, 100);
            _messageSystem.AddSystemMessage($"Health {(health > 0 ? "+" : "")}{health} (now {player.Health})", SystemMessageTypes.Success);
        }

        if (hunger != 0)
        {
            player.Hunger = Math.Clamp(player.Hunger + hunger, 0, 100);
            _messageSystem.AddSystemMessage($"Hunger {(hunger > 0 ? "+" : "")}{hunger} (now {player.Hunger})", SystemMessageTypes.Success);
        }
    }

    public void DebugTeleportToLocation(string locationId, string spotId)
    {
        Player player = _gameWorld.GetPlayer();
        LocationSpot spot = _gameWorld.GetSpot(spotId);

        if (spot == null)
        {
            _messageSystem.AddSystemMessage($"Spot '{spotId}' not found", SystemMessageTypes.Warning);
            return;
        }

        Location? location = _gameWorld.Locations.FirstOrDefault(l => l.Id == locationId);
        if (location == null)
        {
            _messageSystem.AddSystemMessage($"Location '{locationId}' not found", SystemMessageTypes.Warning);
            return;
        }

        player.CurrentLocationSpot = new LocationSpot(spotId, spot.Name) { LocationId = locationId };
        player.AddKnownLocation(locationId);

        _messageSystem.AddSystemMessage($"Teleported to {location.Name} - {spot.Name}", SystemMessageTypes.Success);
    }

    // ========== INVESTIGATION SYSTEM ==========

    /// <summary>
    /// Evaluate and trigger investigation discovery based on current player state
    /// Called when player moves to new location/spot, gains knowledge, items, or accepts obligations
    /// Discovered investigations will have their intro goals added to the appropriate location
    /// and discovery modals will be triggered via pending results
    /// </summary>
    public void EvaluateInvestigationDiscovery()
    {
        Player player = _gameWorld.GetPlayer();
        Console.WriteLine($"[InvestigationDiscovery] Evaluating discovery - Potential investigations: {_gameWorld.InvestigationJournal.PotentialInvestigationIds.Count}, Player at spot: {player.CurrentLocationSpot?.SpotID ?? "NULL"}");

        // Evaluate which investigations can be discovered
        List<Investigation> discoverable = _investigationDiscoveryEvaluator.EvaluateDiscoverableInvestigations(player);
        Console.WriteLine($"[InvestigationDiscovery] Found {discoverable.Count} discoverable investigations");

        // For each discovered investigation, trigger discovery flow
        foreach (Investigation investigation in discoverable)
        {
            Console.WriteLine($"[InvestigationDiscovery] Discovering investigation '{investigation.Name}' (ID: {investigation.Id})");

            // DiscoverInvestigation moves Potential→Discovered and returns intro LocationGoal
            LocationGoal introGoal = _investigationActivity.DiscoverInvestigation(investigation.Id);

            // Add intro goal directly to the spot (Spots are the only entity that matters)
            LocationSpotEntry spotEntry = _gameWorld.Spots.FirstOrDefault(s => s.Spot.SpotID == investigation.IntroAction.SpotId);
            if (spotEntry != null)
            {
                if (spotEntry.Spot.Goals == null)
                    spotEntry.Spot.Goals = new List<LocationGoal>();
                spotEntry.Spot.Goals.Add(introGoal);
                Console.WriteLine($"[InvestigationDiscovery] Added intro goal to spot '{spotEntry.Spot.SpotID}' ({spotEntry.Spot.Name})");
            }
            else
            {
                Console.WriteLine($"[InvestigationDiscovery] ERROR: Could not find spot '{investigation.IntroAction.SpotId}' to add intro goal!");
            }

            // Pending discovery result is now set in InvestigationActivity
            // GameScreen will check for it and display the modal

            // Only handle one discovery at a time for POC
            break;
        }
    }

    /// <summary>
    /// Get route by ID (V2 Travel Integration)
    /// </summary>
    public RouteOption GetRouteById(string routeId)
    {
        return _travelFacade.GetAvailableRoutesFromCurrentLocation()
            .FirstOrDefault(r => r.Id == routeId);
    }

}