using System.Text;

/// <summary>
/// GameOrchestrator - THE single entry point for UI-Backend communication.
/// FACADE ISOLATION: Only this class can coordinate between facades.
/// Facades may NEVER reference other facades directly - orchestration happens here.
/// </summary>
public class GameOrchestrator
{
    private readonly GameWorld _gameWorld;
    private readonly MessageSystem _messageSystem;

    // ==================== PUBLIC ACCESSORS ====================

    /// <summary>
    /// Direct access to GameWorld for debugging/testing
    /// Used by DebugPanel to access game state
    /// </summary>
    public GameWorld GameWorld => _gameWorld;
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
    private readonly CubeFacade _cubeFacade;
    private readonly ObligationActivity _obligationActivity;
    private readonly ObligationDiscoveryEvaluator _obligationDiscoveryEvaluator;
    private readonly ConversationTreeFacade _conversationTreeFacade;
    private readonly ObservationFacade _observationFacade;
    private readonly EmergencyFacade _emergencyFacade;
    private readonly SituationFacade _situationFacade;
    private readonly LocationActionExecutor _locationActionExecutor;
    private readonly SituationChoiceExecutor _situationChoiceExecutor;
    private readonly ConsequenceFacade _consequenceFacade;
    private readonly RewardApplicationService _rewardApplicationService;
    private readonly SceneFacade _sceneFacade;
    private readonly SituationCompletionHandler _situationCompletionHandler;
    private readonly SceneInstantiator _sceneInstantiator;
    private readonly PackageLoader _packageLoader;
    private readonly HexRouteGenerator _hexRouteGenerator;
    private readonly ContentGenerationFacade _contentGenerationFacade;
    private readonly ProceduralContentTracer _proceduralTracer;

    // Composed services (extracted via COMPOSITION OVER INHERITANCE)
    // Only DebugCommandHandler remains - it's compliant (uses RewardApplicationService, not facades)
    private readonly DebugCommandHandler _debugCommandHandler;

    public GameOrchestrator(
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
        CubeFacade cubeFacade,
        ObligationActivity obligationActivity,
        ObligationDiscoveryEvaluator obligationDiscoveryEvaluator,
        ConversationTreeFacade conversationTreeFacade,
        ObservationFacade observationFacade,
        EmergencyFacade emergencyFacade,
        SituationFacade situationFacade,
        LocationActionExecutor locationActionExecutor,
        SituationChoiceExecutor situationChoiceExecutor,
        ConsequenceFacade consequenceFacade,
        RewardApplicationService rewardApplicationService,
        SpawnConditionsEvaluator spawnConditionsEvaluator,
        SceneFacade sceneFacade,
        SituationCompletionHandler situationCompletionHandler,
        SceneInstantiator sceneInstantiator,
        PackageLoader packageLoader,
        HexRouteGenerator hexRouteGenerator,
        ContentGenerationFacade contentGenerationFacade,
        ProceduralContentTracer proceduralTracer,
        DebugCommandHandler debugCommandHandler)
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
        _cubeFacade = cubeFacade ?? throw new ArgumentNullException(nameof(cubeFacade));
        _obligationActivity = obligationActivity;
        _obligationDiscoveryEvaluator = obligationDiscoveryEvaluator;
        _conversationTreeFacade = conversationTreeFacade ?? throw new ArgumentNullException(nameof(conversationTreeFacade));
        _observationFacade = observationFacade ?? throw new ArgumentNullException(nameof(observationFacade));
        _emergencyFacade = emergencyFacade ?? throw new ArgumentNullException(nameof(emergencyFacade));
        _situationFacade = situationFacade ?? throw new ArgumentNullException(nameof(situationFacade));
        _locationActionExecutor = locationActionExecutor ?? throw new ArgumentNullException(nameof(locationActionExecutor));
        _situationChoiceExecutor = situationChoiceExecutor ?? throw new ArgumentNullException(nameof(situationChoiceExecutor));
        _consequenceFacade = consequenceFacade ?? throw new ArgumentNullException(nameof(consequenceFacade));
        _rewardApplicationService = rewardApplicationService ?? throw new ArgumentNullException(nameof(rewardApplicationService));
        _sceneFacade = sceneFacade ?? throw new ArgumentNullException(nameof(sceneFacade));
        _situationCompletionHandler = situationCompletionHandler ?? throw new ArgumentNullException(nameof(situationCompletionHandler));
        _sceneInstantiator = sceneInstantiator ?? throw new ArgumentNullException(nameof(sceneInstantiator));
        _packageLoader = packageLoader ?? throw new ArgumentNullException(nameof(packageLoader));
        _hexRouteGenerator = hexRouteGenerator ?? throw new ArgumentNullException(nameof(hexRouteGenerator));
        _contentGenerationFacade = contentGenerationFacade ?? throw new ArgumentNullException(nameof(contentGenerationFacade));
        _proceduralTracer = proceduralTracer ?? throw new ArgumentNullException(nameof(proceduralTracer));
        _debugCommandHandler = debugCommandHandler ?? throw new ArgumentNullException(nameof(debugCommandHandler));
    }

    // ========== CORE GAME STATE ==========

    public Player GetPlayer()
    {
        return _gameWorld.GetPlayer();
    }

    /// <summary>
    /// Get player's current location via hex-first architecture
    /// </summary>
    public Location GetPlayerCurrentLocation()
    {
        return _gameWorld.GetPlayerCurrentLocation();
    }

    /// <summary>
    /// Get player's active delivery job (if any)
    /// </summary>
    public DeliveryJob GetActiveDeliveryJob()
    {
        Player player = _gameWorld.GetPlayer();
        if (player != null && player.HasActiveDeliveryJob)
        {
            return player.ActiveDeliveryJob;
        }
        return null;
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
    // PlayerStats class deleted - stats are now simple integers on Player

    public List<NPC> GetAvailableStrangers(Venue venue)
    {
        TimeBlocks currentTime = _timeFacade.GetCurrentTimeBlock();
        return _gameWorld.GetAvailableStrangers(venue, currentTime);
    }

    public List<ObligationApproach> GetAvailableObligationApproaches()
    {
        Player player = _gameWorld.GetPlayer();
        return _locationFacade.GetAvailableApproaches(player);
    }

    // ========== LOCATION OPERATIONS ==========
    // Venue is ALWAYS derived from Location: location.Venue
    // HIGHLANDER: GetLocation(string) DELETED - use _gameWorld.Locations LINQ queries

    public Location GetCurrentLocation()
    {
        return _locationFacade.GetCurrentLocation();
    }

    public async Task<bool> MoveToSpot(Location location)
    {
        bool success = await _locationFacade.MoveToSpot(location);

        // Movement to new Venue may unlock obligation discovery (ImmediateVisibility, EnvironmentalObservation triggers)
        if (success)
        {
            await EvaluateObligationDiscovery();
        }

        return success;
    }

    public LocationFacade GetLocationFacade()
    {
        return _locationFacade;
    }

    public SituationFacade GetSituationFacade()
    {
        return _situationFacade;
    }


    public List<NPC> GetNPCsAtLocation(Location location)
    {
        return _locationFacade.GetNPCsAtLocation(location);
    }

    public List<NPC> GetNPCsAtCurrentSpot()
    {
        return _locationFacade.GetNPCsAtCurrentSpot();
    }

    public LocationScreenViewModel GetLocationScreen()
    {
        // NPCs no longer have inline conversation options - situations are embedded in Scene.Situations
        return _locationFacade.GetLocationScreen(new List<NPCConversationOptions>());
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

    public async Task<bool> TravelToDestinationAsync(RouteOption route)
    {
        // No lookup needed!
        if (route == null)
        {
            _narrativeFacade.AddSystemMessage("Route not found", SystemMessageTypes.Danger, null);
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
        int itemCount = player.Inventory.GetAllItems().Count;
        int hungerCost = route.BaseStaminaCost; // This is actually the hunger cost in the data

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
                SystemMessageTypes.Warning,
                null);
            return false;
        }

        // 6. CHECK COIN COST
        int coinCost = route.BaseCoinCost;
        if (coinCost > 0 && player.Coins < coinCost)
        {
            _narrativeFacade.AddSystemMessage($"Not enough coins. Need {coinCost}, have {player.Coins}", SystemMessageTypes.Warning, null);
            return false;
        }

        // === ALL CHECKS PASSED - APPLY COSTS AND EXECUTE TRAVEL ===

        // 7. APPLY ALL COSTS
        if (coinCost > 0)
        {
            await _resourceFacade.SpendCoins(coinCost);
        }
        await _resourceFacade.IncreaseHunger(hungerCost, "Travel fatigue");
        // Travel does NOT cost attention - removed incorrect attention spending

        // Create travel result for processing
        int travelTime = route.TravelTimeSegments;
        TravelResult travelResult = new TravelResult
        {
            Success = true,
            TravelTimeSegments = travelTime,
            CoinCost = coinCost,
            Route = route,  // HIGHLANDER: Object reference, not ID
            Destination = route.DestinationLocation,  // HIGHLANDER: Object reference, not ID
            TransportMethod = route.Method
        };

        if (travelResult.Success)
        {
            // HIGHLANDER: Use object reference directly, no string lookup
            Location destSpot = route.DestinationLocation;

            if (destSpot != null)
            {
                if (!destSpot.HexPosition.HasValue)
                    throw new InvalidOperationException($"Destination location '{destSpot.Name}' has no HexPosition - cannot move player");

                player.CurrentPosition = destSpot.HexPosition.Value;

                // TRIGGER POINT 1: Record location visit after successful travel
                RecordLocationVisit(destSpot);

                // TRIGGER POINT 3: Record route traversal after successful travel
                RecordRouteTraversal(route);
            }

            TimeBlocks oldTimeBlock = _timeFacade.GetCurrentTimeBlock();
            TimeBlocks newTimeBlock = _timeFacade.AdvanceSegments(travelResult.SegmentCost);

            await ProcessTimeAdvancement(new TimeAdvancementResult
            {
                OldTimeBlock = oldTimeBlock,
                NewTimeBlock = newTimeBlock,
                CrossedTimeBlock = oldTimeBlock != newTimeBlock,
                SegmentsAdvanced = travelResult.SegmentCost
            });

            // HIGHLANDER: Use object references directly, no string lookups
            Location finalDestSpot = route.DestinationLocation;

            string destinationName = "Unknown";
            if (finalDestSpot != null)
            {
                Venue destLocation = finalDestSpot.Venue;
                if (destLocation != null)
                {
                    destinationName = destLocation.Name;
                }
                else if (!string.IsNullOrEmpty(finalDestSpot.Name))
                {
                    destinationName = finalDestSpot.Name;
                }
            }

            _narrativeFacade.AddSystemMessage($"Traveled to {destinationName}", SystemMessageTypes.Info, null);
        }

        return travelResult.Success;
    }

    // ========== RESOURCE OPERATIONS ==========

    public InventoryViewModel GetInventory()
    {
        return _resourceFacade.GetInventoryViewModel();
    }

    public async Task<WorkResult> PerformWork(Consequence consequence)
    {
        WorkResult result = await _resourceFacade.PerformWork(consequence);
        if (result.Success)
        {
            TimeBlocks oldTimeBlock = _timeFacade.GetCurrentTimeBlock();
            TimeBlocks newTimeBlock = _timeFacade.JumpToNextPeriod(); // Work takes 4 segments (full period)

            await ProcessTimeAdvancement(new TimeAdvancementResult
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

    /// <summary>
    /// Create conversation context with cross-facade orchestration
    /// Note: Scene activation happens via LOCATION (CheckAndActivateDeferredScenes when player enters)
    /// NPCs are for display context only - choices display when player talks to situation's NPC
    /// </summary>
    public async Task<SocialChallengeContext> CreateConversationContext(NPC npc, Situation situation)
    {
        return await _conversationFacade.CreateConversationContext(npc, situation);
    }

    /// <summary>
    /// Play a conversation card - proper architectural flow through GameOrchestrator
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
    /// Execute listen action in current conversation - proper architectural flow through GameOrchestrator
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
        return _conversationFacade.IsConversationActive();
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
        return _conversationFacade.EndConversation();
    }

    // ========== MENTAL TACTICAL SYSTEM OPERATIONS ==========

    public MentalFacade GetMentalFacade()
    {
        return _mentalFacade;
    }

    /// <summary>
    /// Start a new Mental tactical session with specified deck
    /// Strategic-Tactical Integration Point
    /// </summary>
    public async Task<MentalSession> StartMentalSession(MentalChallengeDeck challengeDeck, Location location, Situation situation, Obligation obligation)
    {
        if (_mentalFacade.IsSessionActive())
            throw new InvalidOperationException("Mental session already active");

        if (challengeDeck == null)
            throw new InvalidOperationException($"MentalChallengeDeck is null");

        Player player = _gameWorld.GetPlayer();

        // Build deck with signature deck knowledge cards in starting hand
        MentalDeckBuildResult buildResult = _mentalFacade.GetDeckBuilder()
            .BuildDeckWithStartingHand(challengeDeck, player);

        return await _mentalFacade.StartSession(challengeDeck, buildResult.Deck, buildResult.StartingHand, situation?.TemplateId, obligation?.Name);
    }

    /// <summary>
    /// Execute observe action in current Mental obligation
    /// </summary>
    public async Task<MentalTurnResult> ExecuteObserve()
    {
        if (_mentalFacade == null)
            throw new InvalidOperationException("MentalFacade not available");

        if (!_mentalFacade.IsSessionActive())
            throw new InvalidOperationException("No active mental session");

        return await _mentalFacade.ExecuteObserve();
    }

    /// <summary>
    /// Execute act action in current Mental obligation
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
    /// End the current mental obligation and return the outcome
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
        return _mentalFacade.IsSessionActive();
    }

    // ========== PHYSICAL TACTICAL SYSTEM OPERATIONS ==========

    public PhysicalFacade GetPhysicalFacade()
    {
        return _physicalFacade;
    }

    /// <summary>
    /// Start a new Physical tactical session with specified deck
    /// Strategic-Tactical Integration Point
    /// </summary>
    public async Task<PhysicalSession> StartPhysicalSession(PhysicalChallengeDeck challengeDeck, Location location, Situation situation, Obligation obligation)
    {
        if (_physicalFacade.IsSessionActive())
            throw new InvalidOperationException("Physical session already active");

        if (challengeDeck == null)
            throw new InvalidOperationException($"PhysicalChallengeDeck is null");

        Player player = _gameWorld.GetPlayer();

        // Build deck with signature deck knowledge cards in starting hand
        PhysicalDeckBuildResult buildResult = _physicalFacade.GetDeckBuilder()
            .BuildDeckWithStartingHand(challengeDeck, player);

        // ADR-007: Pass Situation and Obligation objects (not IDs)
        return await _physicalFacade.StartSession(challengeDeck, buildResult.Deck, buildResult.StartingHand, situation, obligation);
    }

    /// <summary>
    /// Execute assess action in current Physical challenge
    /// </summary>
    public async Task<PhysicalTurnResult> ExecuteAssess()
    {
        if (_physicalFacade == null)
            throw new InvalidOperationException("PhysicalFacade not available");

        if (!_physicalFacade.IsSessionActive())
            throw new InvalidOperationException("No active physical session");

        return await _physicalFacade.ExecuteAssess();
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
    public async Task<PhysicalOutcome> EndPhysicalSession()
    {
        if (_physicalFacade == null) return null;
        return await _physicalFacade.EndSession();
    }

    /// <summary>
    /// Check if a physical session is currently active
    /// </summary>
    public bool IsPhysicalSessionActive()
    {
        return _physicalFacade.IsSessionActive();
    }

    public async Task<ExchangeContext> CreateExchangeContext(NPC npc)
    {
        // NPC passed as object - no lookup needed
        if (npc == null)
        {
            _messageSystem.AddSystemMessage($"NPC is null", SystemMessageTypes.Danger, null);
            return null;
        }

        // Get current location (Venue derived from Location)
        Location? currentLocation = GetCurrentLocation();
        Venue? venue = currentLocation?.Venue;

        // Get current time block
        TimeBlocks timeBlock = _timeFacade.GetCurrentTimeBlock();

        // Get player resources and tokens
        PlayerResourceState playerResources = _gameWorld.GetPlayerResourceState();

        // Get player's tokens with this specific NPC
        // DOMAIN COLLECTION: GetTokensWithNPC now returns List<TokenCount> directly
        List<TokenCount> npcTokens = _tokenFacade.GetTokensWithNPC(npc);

        // Get relationship tier with this NPC
        RelationshipTier relationshipTier = _tokenFacade.GetRelationshipTier(npc);

        // Get available exchanges from ExchangeFacade - now orchestrating properly
        List<ExchangeOption> availableExchanges = _exchangeFacade.GetAvailableExchanges(npc, playerResources, npcTokens, relationshipTier);

        if (!availableExchanges.Any())
        {
            _messageSystem.AddSystemMessage($"{npc.Name} has no exchanges available", SystemMessageTypes.Info, null);
            return null;
        }

        // Create exchange session through ExchangeFacade
        // HIGHLANDER: Pass NPC object, not string
        ExchangeSession session = _exchangeFacade.CreateExchangeSession(npc);
        if (session == null)
        {
            _messageSystem.AddSystemMessage($"Could not create exchange session with {npc.Name}", SystemMessageTypes.Danger, null);
            return null;
        }

        // ADR-007: Build context using session object (not creating redundant session)
        ExchangeContext context = new ExchangeContext
        {
            // ADR-007: Store NPC object reference (not NpcId string)
            Npc = npc,
            // ADR-007: Store Location object reference (not LocationId string)
            Location = currentLocation,
            CurrentTimeBlock = timeBlock,
            PlayerResources = playerResources,
            PlayerTokens = npcTokens,
            // HIGHLANDER: Access inventory through Player.Inventory, not redundant dictionary
            Player = _gameWorld.GetPlayer(),
            // ADR-007: Use existing session object (already has Npc and Location)
            Session = session
        };

        return await Task.FromResult(context);
    }

    // ========== NARRATIVE OPERATIONS ==========

    // ObservationDeck system eliminated - replaced by transparent resource competition

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
        Console.WriteLine("[StartGameAsync] Called");
        Console.WriteLine($"[StartGameAsync] IsGameStarted: {_gameWorld.IsGameStarted}");

        // Check if game is already started to prevent duplicate initialization
        // This is CRITICAL for ServerPrerendered mode compatibility
        if (_gameWorld.IsGameStarted)
        {
            Console.WriteLine("[StartGameAsync] Game already started - returning early");
            return;
        }

        // PROCEDURAL CONTENT TRACING: Clear trace data for new game
        _proceduralTracer.Clear();
        Console.WriteLine("[StartGameAsync] Procedural content trace cleared");

        // Initialize player at starting location from GameWorld initial conditions
        // HEX-FIRST ARCHITECTURE: Player position is hex coordinates
        // HIGHLANDER: Use StartingLocation object reference set by PackageLoader
        Player player = _gameWorld.GetPlayer();
        Location startingSpot = _gameWorld.StartingLocation;
        if (startingSpot == null)
            throw new InvalidOperationException("GameWorld.StartingLocation not set - PackageLoader should have initialized this");

        Console.WriteLine($"[StartGameAsync] Starting location name: {startingSpot.Name}");

        if (!startingSpot.HexPosition.HasValue)
            throw new InvalidOperationException($"Starting location '{startingSpot.Name}' has no HexPosition - cannot initialize player position");

        // ========== GAME INITIALIZATION FLOW ==========
        // Holistic initialization order ensures systems are ready before content spawns,
        // content exists before player moves, and game is marked started only after everything is complete.

        // PHASE 1: Initialize game systems
        // Player resources already applied by PackageLoader.ApplyInitialPlayerConfiguration()
        // No need to re-apply here - HIGHLANDER PRINCIPLE: initialization happens ONCE

        // Initialize time state from GameWorld initial conditions
        if (_gameWorld.InitialTimeBlock.HasValue && _gameWorld.InitialSegment.HasValue)
        {
            _timeFacade.SetInitialTimeState(
                _gameWorld.InitialDay ?? 1,
                _gameWorld.InitialTimeBlock.Value,
                _gameWorld.InitialSegment.Value);
        }

        // Initialize exchange inventories
        _exchangeFacade.InitializeNPCExchanges();

        // PHASE 2: Spawn starter scenes (creates DEFERRED scenes in GameWorld.Scenes)
        // Must happen BEFORE player movement so CheckAndActivateDeferredScenes has scenes to activate
        // Pass startingSpot directly since player hasn't moved yet
        await SpawnStarterScenes(startingSpot);

        // PHASE 3: Move player to starting location (triggers CheckAndActivateDeferredScenes)
        // INTEGRATED ACTIVATION: MoveToSpot() calls LocationFacade.CheckAndActivateDeferredScenes() which:
        //   1. Finds DEFERRED scenes matching player's location via LocationActivationFilter
        //   2. Calls ActivateScene() - INTEGRATED process that creates Situations AND resolves entities
        //   3. Entities found via EntityResolver.Find*() or created via PackageLoader.CreateSingle*()
        // This ensures A1 scene is activated and Elena shows the "Secure Lodging" conversation option
        Console.WriteLine($"[StartGameAsync] Moving player to starting location '{startingSpot.Name}' at hex ({startingSpot.HexPosition.Value.Q}, {startingSpot.HexPosition.Value.R})");

        bool moveSuccess = await _locationFacade.MoveToSpot(startingSpot);

        if (!moveSuccess)
        {
            throw new InvalidOperationException($"CRITICAL: Failed to move player to starting location '{startingSpot.Name}'. Game cannot start.");
        }

        Console.WriteLine($"[StartGameAsync] Validated player at '{startingSpot.Name}' - scene activation complete");

        // PHASE 4: Mark game as started (LAST - game is now fully initialized and ready)
        _gameWorld.IsGameStarted = true;
        _messageSystem.AddSystemMessage("Game started", SystemMessageTypes.Success, null);
    }

    // ========== INTENT PROCESSING ==========

    /// <summary>
    /// HIGHLANDER: Single execution point for ALL player intents.
    /// Backend authority: Determines all effects, navigation, and refresh requirements.
    /// Returns strongly-typed result for UI to interpret without making decisions.
    /// </summary>
    public async Task<IntentResult> ProcessIntent(PlayerIntent intent)
    {
        return intent switch
        {
            // Navigation intents
            OpenTravelScreenIntent => ProcessOpenTravelScreenIntent(),
            TravelIntent travel => await ProcessTravelIntentAsync(travel.Route),

            // Movement intents
            MoveIntent move => await ProcessMoveIntent(move.TargetSpot),

            // Player action intents
            WaitIntent => await ProcessWaitIntent(),
            SleepOutsideIntent => await ProcessSleepOutsideIntent(),
            LookAroundIntent => ProcessLookAroundIntent(),
            CheckBelongingsIntent => ProcessCheckBelongingsIntent(),

            // Location action intents
            RestAtLocationIntent => await ProcessRestAtLocationIntent(),
            SecureRoomIntent => await ProcessSecureRoomIntent(),
            WorkIntent => await ProcessWorkIntent(),
            InvestigateLocationIntent => ProcessInvestigateIntent(),

            // Delivery job intents (Core Loop Phase 3)
            ViewJobBoardIntent => ProcessViewJobBoardIntent(),
            AcceptDeliveryJobIntent accept => ProcessAcceptDeliveryJobIntent(accept.Job),
            CompleteDeliveryIntent => await ProcessCompleteDeliveryIntent(),

            // Conversation/NPC intents
            TalkIntent talk => await ProcessTalkIntent(talk.Npc),

            _ => IntentResult.Failed()
        };
    }

    // ========== NAVIGATION INTENT HANDLERS ==========

    private IntentResult ProcessOpenTravelScreenIntent()
    {
        // Backend authority: Navigate to Travel screen to view available routes
        return IntentResult.NavigateScreen(ScreenMode.Travel);
    }

    private async Task<IntentResult> ProcessTravelIntentAsync(RouteOption route)
    {
        // Backend authority: Navigate to Travel screen (screen-level navigation)
        bool success = await TravelToDestinationAsync(route);
        return success ? IntentResult.NavigateScreen(ScreenMode.Travel) : IntentResult.Failed();
    }

    // ========== MOVEMENT INTENT HANDLERS ==========

    private async Task<IntentResult> ProcessMoveIntent(Location targetLocation)
    {
        // HIGHLANDER: Accept Location object directly, no lookup needed
        if (targetLocation == null)
            return IntentResult.Failed();

        bool success = await MoveToSpot(targetLocation);

        if (success)
        {
            // TRIGGER POINT 1: Record location visit after successful movement
            RecordLocationVisit(targetLocation);
        }

        return success ? IntentResult.Executed(requiresRefresh: true) : IntentResult.Failed();
    }

    // ========== PLAYER ACTION INTENT HANDLERS ==========

    private async Task<IntentResult> ProcessWaitIntent()
    {
        // Fetch entity for data-driven execution
        PlayerAction action = _gameWorld.PlayerActions.FirstOrDefault(a => a.ActionType == PlayerActionType.Wait);
        if (action == null)
        {
            _messageSystem.AddSystemMessage("Wait action not found", SystemMessageTypes.Warning, null);
            return IntentResult.Failed();
        }

        // Execute with data from entity
        TimeBlocks oldTimeBlock = _timeFacade.GetCurrentTimeBlock();
        _timeFacade.AdvanceSegments(1); // ORCHESTRATION: GameOrchestrator controls time progression
        _resourceFacade.ExecuteWait(); // Resource effects only, no time progression
        TimeBlocks newTimeBlock = _timeFacade.GetCurrentTimeBlock();

        await ProcessTimeAdvancement(new TimeAdvancementResult
        {
            OldTimeBlock = oldTimeBlock,
            NewTimeBlock = newTimeBlock,
            CrossedTimeBlock = oldTimeBlock != newTimeBlock,
            SegmentsAdvanced = action.TimeRequired
        });

        return IntentResult.Executed(requiresRefresh: true);
    }

    private async Task<IntentResult> ProcessSleepOutsideIntent()
    {
        // Fetch entity for data-driven costs
        PlayerAction action = _gameWorld.PlayerActions.FirstOrDefault(a => a.ActionType == PlayerActionType.SleepOutside);
        if (action == null)
        {
            _messageSystem.AddSystemMessage("SleepOutside action not found", SystemMessageTypes.Warning, null);
            return IntentResult.Failed();
        }

        // HIGHLANDER: Apply health cost via Consequence
        // Negative Health in Consequence = cost, so extract the cost amount for message
        int healthCost = action.Consequence.Health < 0 ? -action.Consequence.Health : 0;
        await _rewardApplicationService.ApplyConsequence(action.Consequence, null);

        _messageSystem.AddSystemMessage(
            $"You sleep rough on a bench. Cold. Uncomfortable. You wake stiff and sore. (-{healthCost} Health)",
            SystemMessageTypes.Warning,
            null);

        return IntentResult.Executed(requiresRefresh: true);
    }

    private IntentResult ProcessLookAroundIntent()
    {
        // Backend authority: Navigate to LookingAround view showing NPCs, challenges, opportunities
        return IntentResult.NavigateView(LocationViewState.LookingAround);
    }

    private IntentResult ProcessCheckBelongingsIntent()
    {
        // Backend authority: Navigate to Inventory screen (full-screen meta view)
        // Displays player's belongings: inventory items + equipped items
        // Passive viewing only - no equip/unequip mechanics (out of scope)
        return IntentResult.NavigateScreen(ScreenMode.Inventory);
    }

    // ========== LOCATION ACTION INTENT HANDLERS ==========

    private async Task<IntentResult> ProcessRestAtLocationIntent()
    {
        // Fetch entity for data-driven rewards
        LocationAction action = _gameWorld.LocationActions.FirstOrDefault(a => a.ActionType == LocationActionType.Rest);
        if (action == null)
        {
            _messageSystem.AddSystemMessage("Rest action not found", SystemMessageTypes.Warning, null);
            return IntentResult.Failed();
        }

        // Execute with data from entity - HIGHLANDER: Consequence is unified
        TimeBlocks oldTimeBlock = _timeFacade.GetCurrentTimeBlock();
        _timeFacade.AdvanceSegments(1); // ORCHESTRATION: GameOrchestrator controls time progression
        await _resourceFacade.ExecuteRest(action.Consequence); // Resource effects only, no time progression
        TimeBlocks newTimeBlock = _timeFacade.GetCurrentTimeBlock();

        await ProcessTimeAdvancement(new TimeAdvancementResult
        {
            OldTimeBlock = oldTimeBlock,
            NewTimeBlock = newTimeBlock,
            CrossedTimeBlock = oldTimeBlock != newTimeBlock,
            SegmentsAdvanced = action.TimeRequired
        });

        await Task.CompletedTask;
        return IntentResult.Executed(requiresRefresh: true);
    }

    private async Task<IntentResult> ProcessSecureRoomIntent()
    {
        // Fetch entity for data-driven recovery
        LocationAction action = _gameWorld.LocationActions.FirstOrDefault(a => a.ActionType == LocationActionType.SecureRoom);
        if (action == null)
        {
            _messageSystem.AddSystemMessage("SecureRoom action not found", SystemMessageTypes.Warning, null);
            return IntentResult.Failed();
        }

        Player player = _gameWorld.GetPlayer();

        // Record recovery amounts for message
        int healthBefore = player.Health;
        int staminaBefore = player.Stamina;
        int hungerBefore = player.Hunger;
        int focusBefore = player.Focus;

        // HIGHLANDER: Use action.Consequence directly (unified costs/rewards)
        Consequence consequence = action.Consequence;

        // Use projection to calculate change amounts for messaging (before mutation)
        PlayerStateProjection projected = consequence.GetProjectedState(player);
        int healthRecovered = projected.Health - healthBefore;
        int staminaRecovered = projected.Stamina - staminaBefore;
        int hungerRecovered = hungerBefore - projected.Hunger;
        int focusRecovered = projected.Focus - focusBefore;

        // TWO PILLARS: Apply via RewardApplicationService (single source of mutation)
        await _rewardApplicationService.ApplyConsequence(consequence, null);

        // Generate recovery message
        string recoveryMessage = "You rest in a secure room through the night.";
        if (healthRecovered > 0) recoveryMessage += $" Health +{healthRecovered}";
        if (staminaRecovered > 0) recoveryMessage += $" Stamina +{staminaRecovered}";
        if (hungerRecovered > 0) recoveryMessage += $" Hunger -{hungerRecovered}";
        if (focusRecovered > 0) recoveryMessage += $" Focus +{focusRecovered}";
        if (consequence.FullRecovery) recoveryMessage += " (Fully recovered)";
        _messageSystem.AddSystemMessage(recoveryMessage, SystemMessageTypes.Success, null);

        // Advance to next day morning
        TimeAdvancementResult timeResult = _timeFacade.AdvanceToNextDay();
        await ProcessTimeAdvancement(timeResult);
        // NOTE: Time-based spawn trigger now fires inside ProcessTimeAdvancement (HIGHLANDER principle)
        // No need to call CheckAndSpawnEligibleScenes here - it's handled automatically

        return IntentResult.Executed(requiresRefresh: true);
    }

    private async Task<IntentResult> ProcessWorkIntent()
    {
        // Fetch entity for data-driven consequence
        LocationAction action = _gameWorld.LocationActions.FirstOrDefault(a => a.ActionType == LocationActionType.Work);
        if (action == null)
        {
            _messageSystem.AddSystemMessage("Work action not found", SystemMessageTypes.Warning, null);
            return IntentResult.Failed();
        }

        // Execute with data from entity - HIGHLANDER: Consequence is unified
        await PerformWork(action.Consequence);
        return IntentResult.Executed(requiresRefresh: true);
    }

    private IntentResult ProcessInvestigateIntent()
    {
        Location currentSpot = GetCurrentLocation();
        if (currentSpot == null)
        {
            _messageSystem.AddSystemMessage("No current location to investigate", SystemMessageTypes.Warning, null);
            return IntentResult.Failed();
        }

        _locationFacade.InvestigateLocation(currentSpot);
        _messageSystem.AddSystemMessage("Investigated location, gaining familiarity", SystemMessageTypes.Info, MessageCategory.Discovery);
        return IntentResult.Executed(requiresRefresh: true);
    }

    // ========== DELIVERY JOB INTENT HANDLERS ==========

    private IntentResult ProcessViewJobBoardIntent()
    {
        // Navigate to JobBoard view modal
        return IntentResult.NavigateView(LocationViewState.JobBoard);
    }

    private IntentResult ProcessAcceptDeliveryJobIntent(DeliveryJob job)
    {
        Player player = _gameWorld.GetPlayer();

        // Validation: Can only have one active job
        if (player.HasActiveDeliveryJob)
        {
            _messageSystem.AddSystemMessage("You already have an active delivery job", SystemMessageTypes.Warning, null);
            return IntentResult.Failed();
        }

        // Validate job is available
        if (job == null || !job.IsAvailable)
        {
            _messageSystem.AddSystemMessage("Job no longer available", SystemMessageTypes.Warning, null);
            return IntentResult.Failed();
        }

        // Accept job
        player.ActiveDeliveryJob = job;
        job.IsAvailable = false;

        _messageSystem.AddSystemMessage($"Accepted: {job.JobDescription}", SystemMessageTypes.Success, null);

        // Close modal and refresh location
        return IntentResult.Executed(requiresRefresh: true);
    }

    private async Task<IntentResult> ProcessCompleteDeliveryIntent()
    {
        Player player = _gameWorld.GetPlayer();

        // Validation: Must have active job
        if (!player.HasActiveDeliveryJob)
        {
            _messageSystem.AddSystemMessage("No active delivery job", SystemMessageTypes.Warning, null);
            return IntentResult.Failed();
        }

        DeliveryJob job = player.ActiveDeliveryJob;
        if (job == null)
        {
            _messageSystem.AddSystemMessage("Delivery job data not found", SystemMessageTypes.Warning, null);
            return IntentResult.Failed();
        }

        // TWO PILLARS: Pay player via Consequence + ApplyConsequence
        Consequence deliveryReward = new Consequence { Coins = job.Payment };
        await _rewardApplicationService.ApplyConsequence(deliveryReward, null);

        // Clear active job
        player.ActiveDeliveryJob = null;

        // Advance time (delivery takes time)
        TimeBlocks oldTimeBlock = _timeFacade.GetCurrentTimeBlock();
        TimeBlocks newTimeBlock = _timeFacade.AdvanceSegments(1);

        // HIGHLANDER: Process time advancement side effects (hunger, emergencies, time-based spawns)
        await ProcessTimeAdvancement(new TimeAdvancementResult
        {
            OldTimeBlock = oldTimeBlock,
            NewTimeBlock = newTimeBlock,
            CrossedTimeBlock = oldTimeBlock != newTimeBlock,
            SegmentsAdvanced = 1
        });

        _messageSystem.AddSystemMessage($"Delivery complete! Earned {job.Payment} coins", SystemMessageTypes.Success, null);

        return IntentResult.Executed(requiresRefresh: true);
    }

    // ========== NPC INTERACTION INTENT HANDLERS ==========

    private async Task<IntentResult> ProcessTalkIntent(NPC npc)
    {
        // HIGHLANDER: Accept NPC object directly, no lookup needed
        if (npc == null)
            return IntentResult.Failed();

        _messageSystem.AddSystemMessage($"Talking to NPC {npc.Name} not yet implemented", SystemMessageTypes.Info, null);
        await Task.CompletedTask;
        return IntentResult.Executed(requiresRefresh: false);
    }

    public ConnectionState GetNPCConnectionState(NPC npc)
    {
        return ConnectionState.NEUTRAL;
    }

    public List<RouteOption> GetAvailableRoutes()
    {
        return _travelFacade.GetAvailableRoutesFromCurrentLocation();
    }

    public List<RouteOption> GetRoutesToDestination(Location destination)
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

    /// <summary>
    /// HIGHLANDER: THE ONLY place for processing time advancement side effects.
    /// All time-based resource changes happen here (hunger, day transitions, emergency checking).
    /// Called after EVERY time advancement (Wait, Rest, Work, Travel, etc.).
    /// </summary>
    private async Task ProcessTimeAdvancement(TimeAdvancementResult result)
    {
        // HUNGER: +5 per segment (universal time cost)
        // This is THE ONLY place hunger increases due to time
        int hungerIncrease = result.SegmentsAdvanced * 5;
        await _resourceFacade.IncreaseHunger(hungerIncrease, "Time passes");

        // DAY TRANSITION: Process dawn effects (NPC decay)
        // Only when crossing into Morning (new day starts)
        if (result.CrossedDayBoundary && result.NewTimeBlock == TimeBlocks.Morning)
        {
            _resourceFacade.ProcessDayTransition();
        }

        // EMERGENCY CHECKING: Check for active emergencies at sync points
        // Emergencies interrupt normal gameplay and demand immediate response
        // HIGHLANDER: ActiveEmergencyState separates mutable state from immutable template
        ActiveEmergencyState activeEmergency = _emergencyFacade.CheckForActiveEmergency();
        if (activeEmergency != null)
        {
            _gameWorld.ActiveEmergency = activeEmergency;
            _messageSystem.AddSystemMessage(
                $"EMERGENCY: {activeEmergency.Template.Name}",
                SystemMessageTypes.Warning,
                null);
        }

        // SCENE EXPIRATION ENFORCEMENT (HIGHLANDER sync point for time-based state changes)
        // Check all active scenes for expiration based on current day
        // Scenes with ExpiresOnDay <= CurrentDay transition to Expired state
        // Expired scenes filtered out from SceneFacade queries (no longer visible to player)
        int currentDay = _gameWorld.CurrentDay;
        List<Scene> activeScenes = _gameWorld.Scenes
            .Where(s => s.State == SceneState.Active && s.ExpiresOnDay.HasValue)
            .ToList();

        foreach (Scene scene in activeScenes)
        {
            if (currentDay >= scene.ExpiresOnDay.Value)
            {
                scene.State = SceneState.Expired;

                // PROCEDURAL CONTENT TRACING: Update scene state to Expired
                _proceduralTracer.UpdateSceneState(scene, SceneState.Expired, DateTime.UtcNow);
            }
        }
    }


    /// <summary>
    /// Gets the district containing a location
    /// HIGHLANDER: Pass Venue object directly
    /// </summary>
    public District GetDistrictForLocation(Venue venue)
    {
        return _gameWorld.GetDistrictForLocation(venue);
    }

    /// <summary>
    /// Gets the region containing a district
    /// HIGHLANDER: Pass District object directly
    /// </summary>
    public Region GetRegionForDistrict(District district)
    {
        return _gameWorld.GetRegionForDistrict(district);
    }

    /// <summary>
    /// Gets all locations in GameWorld
    /// </summary>
    public List<Venue> GetAllLocations()
    {
        return _gameWorld.Venues;
    }

    // ============================================
    // DEBUG COMMANDS (delegated to DebugCommandHandler)
    // ============================================

    public async Task<bool> DebugSetStatLevel(PlayerStatType statType, int level)
        => await _debugCommandHandler.SetStatLevel(statType, level);

    public async Task<bool> DebugAddStatXP(PlayerStatType statType, int points)
        => await _debugCommandHandler.AddStatXP(statType, points);

    public async Task DebugSetAllStats(int level)
        => await _debugCommandHandler.SetAllStats(level);

    public string DebugGetStatInfo()
        => _debugCommandHandler.GetStatInfo();

    public async Task DebugGiveResources(int coins, int health, int hunger)
        => await _debugCommandHandler.GiveResources(coins, health, hunger);

    public void DebugTeleportToLocation(string venueName, string locationName)
        => _debugCommandHandler.TeleportToLocation(venueName, locationName);

    // ========== OBLIGATION SYSTEM ==========

    /// <summary>
    /// Evaluate and trigger obligation discovery based on current player state
    /// Called when player moves to new location/location, gains knowledge, items, or accepts obligations
    /// Discovered obligations will have their intro situations added to the appropriate location
    /// and discovery modals will be triggered via pending results
    /// </summary>
    public async Task EvaluateObligationDiscovery()
    {
        List<Obligation> discoverable = _obligationDiscoveryEvaluator.EvaluateDiscoverableObligations();
        // For each discovered obligation, trigger discovery flow
        foreach (Obligation obligation in discoverable)
        {// DiscoverObligation moves Potential→Discovered and spawns intro situation at location
         // No return value - situation is added directly to Location.ActiveSituations
            await _obligationActivity.DiscoverObligation(obligation);

            // Pending discovery result is now set in ObligationActivity
            // GameScreen will check for it and display the modal

            // Only handle one discovery at a time for POC
            break;
        }
    }

    /// <summary>
    /// Complete obligation intro action - activates obligation and spawns Phase 1
    /// RPG quest acceptance pattern: Player clicks button → Obligation activates immediately
    /// </summary>
    public async Task CompleteObligationIntro(Obligation obligation)
    {
        await _obligationActivity.CompleteIntroAction(obligation);
    }

    /// <summary>
    /// Set pending intro action - prepares quest acceptance modal
    /// RPG quest acceptance: Button → Modal → "Begin" → Activate
    /// </summary>
    public void SetPendingIntroAction(Obligation obligation)
    {
        _obligationActivity.SetPendingIntroAction(obligation);
    }

    /// <summary>
    /// Get pending intro result for modal display
    /// </summary>
    public ObligationIntroResult GetPendingIntroResult()
    {
        return _obligationActivity.GetAndClearPendingIntroResult();
    }


    // ========== CONVERSATION TREE OPERATIONS ==========

    /// <summary>
    /// Create context for conversation tree screen
    /// </summary>
    public ConversationTreeContext CreateConversationTreeContext(ConversationTree tree)
    {
        return _conversationTreeFacade.CreateContext(tree);
    }

    /// <summary>
    /// Select a dialogue response in conversation tree
    /// </summary>
    public async Task<ConversationTreeResult> SelectConversationResponse(ConversationTree tree, DialogueNode node, DialogueResponse response)
    {
        return await _conversationTreeFacade.SelectResponse(tree, node, response);
    }

    // ========== OBSERVATION SCENE OPERATIONS ==========

    /// <summary>
    /// Create context for observation scene screen
    /// </summary>
    public ObservationContext CreateObservationContext(ObservationScene scene)
    {
        return _observationFacade.CreateContext(scene);
    }

    /// <summary>
    /// Examine a point in an observation scene
    /// ADR-007: Passes objects directly (not IDs)
    /// </summary>
    public async Task<ObservationResult> ExaminePoint(ObservationScene scene, ExaminationPoint point)
    {
        return await _observationFacade.ExaminePoint(scene, point);
    }

    // ========== EMERGENCY SITUATION OPERATIONS ==========

    /// <summary>
    /// Check for active emergencies (called at sync points)
    /// HIGHLANDER: Returns ActiveEmergencyState with Template reference + mutable state.
    /// </summary>
    public ActiveEmergencyState CheckForActiveEmergency()
    {
        return _emergencyFacade.CheckForActiveEmergency();
    }

    /// <summary>
    /// Create context for emergency situation screen
    /// HIGHLANDER: Accepts ActiveEmergencyState, accesses Template for immutable data.
    /// </summary>
    public EmergencyContext CreateEmergencyContext(ActiveEmergencyState emergencyState)
    {
        return _emergencyFacade.CreateContext(emergencyState);
    }

    /// <summary>
    /// Select a response to an emergency situation
    /// HIGHLANDER: Accepts ActiveEmergencyState, mutates state not template.
    /// </summary>
    public EmergencyResult SelectEmergencyResponse(ActiveEmergencyState emergencyState, EmergencyResponse response)
    {
        return _emergencyFacade.SelectResponse(emergencyState, response);
    }

    /// <summary>
    /// Ignore an emergency situation (accept consequences)
    /// HIGHLANDER: Accepts ActiveEmergencyState, mutates state not template.
    /// </summary>
    public EmergencyResult IgnoreEmergency(ActiveEmergencyState emergencyState)
    {
        return _emergencyFacade.IgnoreEmergency(emergencyState);
    }

    // ========== LOCATION QUERY METHODS ==========

    /// <summary>
    /// Get all conversation trees available at a specific location
    /// </summary>
    public List<ConversationTree> GetAvailableConversationTreesAtLocation(Location location)
    {
        return _conversationTreeFacade.GetAvailableTreesAtLocation(location);
    }

    /// <summary>
    /// Get all observation scenes available at a specific location
    /// </summary>
    public List<ObservationScene> GetAvailableObservationScenesAtLocation(Location location)
    {
        return _observationFacade.GetAvailableScenesAtLocation(location);
    }

    /// <summary>
    /// Get all Situations available at a specific location.
    /// Returns ALL situations regardless of intensity - player state does NOT filter visibility.
    /// Learning comes from seeing choices they can't afford (greyed-out), not hidden situations.
    /// See gdd/06_balance.md §6.8 (Challenge and Consequence Philosophy).
    /// </summary>
    public List<Situation> GetAvailableSituationsAtLocation(Location location)
    {
        if (location == null)
            throw new InvalidOperationException($"Location is null - cannot query situations!");

        return _gameWorld.Scenes.SelectMany(s => s.Situations)
            .Where(sit => sit.Location == location)
            .ToList();
    }

    /// <summary>
    /// Get all Situations available for a specific NPC.
    /// Returns ALL situations regardless of intensity - player state does NOT filter visibility.
    /// Learning comes from seeing choices they can't afford (greyed-out), not hidden situations.
    /// See gdd/06_balance.md §6.8 (Challenge and Consequence Philosophy).
    /// </summary>
    public List<Situation> GetAvailableSituationsForNPC(NPC npc)
    {
        if (npc == null) return new List<Situation>();

        return _gameWorld.Scenes.SelectMany(s => s.Situations)
            .Where(sit => sit.Npc == npc)
            .ToList();
    }

    // ========== ACTIVE EMERGENCY OPERATIONS ==========

    /// <summary>
    /// Get the currently active emergency (if any)
    /// HIGHLANDER: Returns ActiveEmergencyState with Template reference + mutable state.
    /// </summary>
    public ActiveEmergencyState GetActiveEmergency()
    {
        return _gameWorld.ActiveEmergency;
    }

    /// <summary>
    /// Clear the active emergency (called after emergency screen resolves)
    /// </summary>
    public void ClearActiveEmergency()
    {
        _gameWorld.ActiveEmergency = null;
    }

    // ========== PROCEDURAL CONTENT GENERATION - MULTI-FACADE ORCHESTRATION ==========

    /// <summary>
    /// HIGHLANDER ORCHESTRATOR: Spawn scene with dynamic content generation
    /// GameOrchestrator is SOLE orchestrator for multi-facade operations
    /// LET IT CRASH: Pipeline succeeds completely or throws exception with stack trace
    /// </summary>
    public Task<Scene> SpawnSceneWithDynamicContent(
        SceneTemplate template,
        SceneSpawnReward spawnReward,
        SceneSpawnContext context)
    {
        // HIGHLANDER FLOW: Create Scene via SceneInstantiator → PackageLoader.CreateSingleScene(dto)
        Scene scene = _sceneInstantiator.CreateDeferredScene(template, spawnReward, context);

        if (scene == null)
        {
            Console.WriteLine($"[GameOrchestrator] Scene '{template.Id}' failed spawn conditions");
        }

        return Task.FromResult(scene);
    }

    /// <summary>
    /// TWO-PHASE SPAWNING - PHASE 1: Create deferred scene WITHOUT dependent resources
    /// Scene.State = Deferred (dependent resources not spawned yet)
    /// HIGHLANDER FLOW: SceneInstantiator → PackageLoader.CreateSingleScene(dto) → Scene entity
    /// </summary>
    private Task<Scene> CreateDeferredSceneWithDynamicContent(
        SceneTemplate template,
        SceneSpawnReward spawnReward,
        SceneSpawnContext context)
    {
        // PHASE 1: Create deferred scene via HIGHLANDER path (no JSON round-trip)
        Scene scene = _sceneInstantiator.CreateDeferredScene(template, spawnReward, context);

        if (scene == null)
        {
            // Scene not eligible (spawn conditions failed)
            return Task.FromResult<Scene>(null);
        }

        return Task.FromResult(scene);
    }

    /// <summary>
    /// Spawn initial scenes during game initialization
    /// HIGHLANDER: Called ONCE from StartGameAsync() after IsGameStarted = true
    /// Spawns MainStory scenes with LocationActivationFilter as deferred content
    /// TWO-PHASE SPAWNING: Creates scenes as Deferred (no dependent resources spawned)
    /// Activation happens when player enters location via LocationFacade
    /// </summary>
    /// <param name="startingLocation">The starting location (player hasn't moved yet, so GetPlayerCurrentLocation() would fail)</param>
    public async Task SpawnStarterScenes(Location startingLocation)
    {
        Player player = _gameWorld.GetPlayer();

        // Spawn only scenes marked as IsStarter=true as deferred at game start
        // A1 (isStarter: true) gets created here; A2/A3 get created when ScenesToSpawn rewards fire
        // They will activate when player enters a matching location
        List<SceneTemplate> initialTemplates = _gameWorld.SceneTemplates
            .Where(t => t.IsStarter)
            .ToList();

        foreach (SceneTemplate template in initialTemplates)
        {
            // 5-SYSTEM ARCHITECTURE: Initial scenes use template's PlacementFilter (no override)
            // EntityResolver will FindOrCreate entities from categorical specifications
            SceneSpawnReward spawnReward = new SceneSpawnReward
            {
                Template = template // Direct object reference, NO ID STRINGS
            };

            // Build context from player state
            // Use startingLocation directly since player hasn't been moved yet
            SceneSpawnContext spawnContext = new SceneSpawnContext
            {
                Player = player,
                CurrentLocation = startingLocation,
                CurrentSituation = null
            };

            // TWO-PHASE: Create deferred scene WITHOUT dependent resources
            // Phase 1: Scene + Situations created with State=Deferred
            // Phase 2: Activation (dependent resources) happens when player enters location
            Scene scene = await CreateDeferredSceneWithDynamicContent(template, spawnReward, spawnContext);

            if (scene == null)
            {
                Console.WriteLine($"[GameOrchestrator] Initial scene '{template.Id}' failed to spawn - skipping");
                continue;
            }

            Console.WriteLine($"[GameOrchestrator] Created deferred initial scene '{template.Id}' (State=Deferred, no dependent resources yet)");
        }
    }

    // ========== UNIFIED ACTION ARCHITECTURE - EXECUTE METHODS ==========

    /// <summary>
    /// Execute LocationAction through unified action architecture
    /// HIGHLANDER PATTERN: All location actions flow through this method
    /// FALLBACK SCENE ARCHITECTURE: Supports both atmospheric (fallback) and scene-based actions
    /// </summary>
    public async Task<IntentResult> ExecuteLocationAction(LocationAction action)
    {
        Player player = _gameWorld.GetPlayer();

        // PATTERN DISCRIMINATION: Determine if scene-based or atmospheric (fallback scene)
        bool isSceneBased = action.ChoiceTemplate != null;
        Situation situation = action.Situation;  // null for atmospheric actions

        // THREE-TIER TIMING MODEL: Action passed directly (ephemeral object, no lookup)

        // STEP 1: Validate and extract execution plan
        ActionExecutionPlan plan;
        if (isSceneBased)
        {
            // SCENE-BASED ACTION: Use SituationChoiceExecutor
            if (situation == null)
                return IntentResult.Failed();

            // TWO-PHASE SCALING: Pass pre-scaled values from action (populated by SceneFacade)
            // Perfect Information compliance: display = execution (arc42 §8.26)
            plan = _situationChoiceExecutor.ValidateAndExtract(
                action.ChoiceTemplate,
                action.Name,
                action.ScaledRequirement,    // Pre-scaled by SceneFacade
                action.ScaledConsequence);   // Pre-scaled by SceneFacade
        }
        else
        {
            // ATMOSPHERIC ACTION (FALLBACK SCENE): Use LocationActionExecutor
            plan = _locationActionExecutor.ValidateAndExtract(action);
        }

        if (!plan.IsValid)
        {
            _messageSystem.AddSystemMessage(plan.FailureReason, SystemMessageTypes.Warning, null);
            return IntentResult.Failed();
        }

        // STEP 2: Apply strategic costs (HIGHLANDER: single source of truth via Consequence class)
        // TWO PILLARS: All cost/reward application uses Consequence class
        Consequence costs = new Consequence
        {
            Coins = -plan.CoinsCost,
            Health = -plan.HealthCost,
            Stamina = -plan.StaminaCost,
            Focus = -plan.FocusCost,
            Resolve = -plan.ResolveCoins,
            Hunger = plan.HungerCost  // Hunger increase is positive (adding hunger is bad)
        };
        await _rewardApplicationService.ApplyConsequence(costs, situation);

        TimeBlocks oldTimeBlock = _timeFacade.GetCurrentTimeBlock();
        if (plan.TimeSegments > 0)
            _timeFacade.AdvanceSegments(plan.TimeSegments);
        TimeBlocks newTimeBlock = _timeFacade.GetCurrentTimeBlock();

        // STEP 3: Route based on ActionType
        if (plan.ActionType == ChoiceActionType.Instant)
        {
            // HIGHLANDER: Apply Consequence (used by BOTH atmospheric and scene-based actions)
            if (plan.Consequence != null)
            {
                await _rewardApplicationService.ApplyConsequence(plan.Consequence, situation);
            }

            // THREE-TIER TIMING MODEL: No cleanup needed (actions are ephemeral, not stored)

            // MULTI-SITUATION SCENE: Complete situation and advance scene
            // Scene.AdvanceToNextSituation() called by completion handler
            // Enables linear progression through multi-situation arcs
            if (situation != null)
            {
                await _situationCompletionHandler.CompleteSituation(situation);
            }

            // Process time advancement (HIGHLANDER: single sync point)
            await ProcessTimeAdvancement(new TimeAdvancementResult
            {
                OldTimeBlock = oldTimeBlock,
                NewTimeBlock = newTimeBlock,
                CrossedTimeBlock = oldTimeBlock != newTimeBlock,
                SegmentsAdvanced = plan.TimeSegments
            });

            await Task.CompletedTask;
            return IntentResult.Executed(requiresRefresh: true);
        }
        else if (plan.ActionType == ChoiceActionType.StartChallenge)
        {
            // Route to tactical system based on ChallengeType
            return RouteToTacticalChallenge(plan);
        }
        else if (plan.ActionType == ChoiceActionType.Navigate)
        {
            // Apply navigation payload
            if (plan.NavigationPayload != null)
            {
                return ApplyNavigationPayload(plan.NavigationPayload);
            }

            return IntentResult.Failed();
        }

        return IntentResult.Failed();
    }

    /// <summary>
    /// Execute NPCAction through unified action architecture
    /// HIGHLANDER PATTERN: All NPC actions flow through this method
    /// ALL NPC actions are scene-based (no atmospheric NPC actions exist)
    /// </summary>
    public async Task<IntentResult> ExecuteNPCAction(NPCAction action)
    {
        Player player = _gameWorld.GetPlayer();

        // Verify Situation exists (ALL NPC actions require Situation)
        Situation situation = action.Situation;
        if (situation == null)
            return IntentResult.Failed();

        // Verify ChoiceTemplate exists (ALL NPC actions are scene-based)
        if (action.ChoiceTemplate == null)
            return IntentResult.Failed();

        // THREE-TIER TIMING MODEL: Action passed directly (ephemeral object, no lookup)

        // TRIGGER POINT 2: Record NPC interaction when action execution starts
        // ARCHITECTURAL CHANGE: Direct property access (situation owns placement)
        // HIGHLANDER: Pass NPC object, not string name
        if (situation.Npc != null)
        {
            RecordNPCInteraction(situation.Npc);
        }

        // STEP 1: Validate and extract execution plan (all NPC actions use SituationChoiceExecutor)
        // TWO-PHASE SCALING: Pass pre-scaled values from action (populated by SceneFacade)
        ActionExecutionPlan plan = _situationChoiceExecutor.ValidateAndExtract(
            action.ChoiceTemplate,
            action.Name,
            action.ScaledRequirement,    // Pre-scaled by SceneFacade
            action.ScaledConsequence);   // Pre-scaled by SceneFacade

        if (!plan.IsValid)
        {
            _messageSystem.AddSystemMessage(plan.FailureReason, SystemMessageTypes.Warning, null);
            return IntentResult.Failed();
        }

        // STEP 2: Apply strategic costs (HIGHLANDER: single source of truth via Consequence class)
        // TWO PILLARS: All cost/reward application uses Consequence class
        Consequence npcCosts = new Consequence
        {
            Coins = -plan.CoinsCost,
            Health = -plan.HealthCost,
            Stamina = -plan.StaminaCost,
            Focus = -plan.FocusCost,
            Resolve = -plan.ResolveCoins,
            Hunger = plan.HungerCost  // Hunger increase is positive (adding hunger is bad)
        };
        await _rewardApplicationService.ApplyConsequence(npcCosts, situation);

        TimeBlocks oldTimeBlock = _timeFacade.GetCurrentTimeBlock();
        if (plan.TimeSegments > 0)
            _timeFacade.AdvanceSegments(plan.TimeSegments);
        TimeBlocks newTimeBlock = _timeFacade.GetCurrentTimeBlock();

        // STEP 3: Route based on ActionType
        if (plan.ActionType == ChoiceActionType.Instant)
        {
            // Apply consequences
            if (plan.Consequence != null)
            {
                await _rewardApplicationService.ApplyConsequence(plan.Consequence, situation);
            }

            // THREE-TIER TIMING MODEL: No cleanup needed (actions are ephemeral, not stored)

            // MULTI-SITUATION SCENE: Complete situation and advance scene
            // Scene.AdvanceToNextSituation() called by completion handler
            // Enables linear progression through multi-situation arcs
            if (situation != null)
            {
                await _situationCompletionHandler.CompleteSituation(situation);
            }

            // Process time advancement (HIGHLANDER: single sync point)
            await ProcessTimeAdvancement(new TimeAdvancementResult
            {
                OldTimeBlock = oldTimeBlock,
                NewTimeBlock = newTimeBlock,
                CrossedTimeBlock = oldTimeBlock != newTimeBlock,
                SegmentsAdvanced = plan.TimeSegments
            });

            await Task.CompletedTask;
            return IntentResult.Executed(requiresRefresh: true);
        }
        else if (plan.ActionType == ChoiceActionType.StartChallenge)
        {
            // Route to tactical system based on ChallengeType
            return RouteToTacticalChallenge(plan);
        }
        else if (plan.ActionType == ChoiceActionType.Navigate)
        {
            // Apply navigation payload
            if (plan.NavigationPayload != null)
            {
                return ApplyNavigationPayload(plan.NavigationPayload);
            }

            return IntentResult.Failed();
        }

        return IntentResult.Failed();
    }

    /// <summary>
    /// Execute PathCard through unified action architecture
    /// HIGHLANDER PATTERN: All path cards flow through this method
    /// FALLBACK SCENE ARCHITECTURE: Supports both atmospheric (fallback) and scene-based path cards
    /// </summary>
    public async Task<IntentResult> ExecutePathCard(PathCard card)
    {
        Player player = _gameWorld.GetPlayer();

        // PATTERN DISCRIMINATION: Determine if scene-based or atmospheric (fallback scene)
        bool isSceneBased = card.ChoiceTemplate != null;
        Situation situation = card.Situation;  // null for atmospheric PathCards

        // THREE-TIER TIMING MODEL: PathCard passed directly (ephemeral object, no lookup)

        // STEP 1: Validate and extract execution plan
        ActionExecutionPlan plan;
        if (isSceneBased)
        {
            // SCENE-BASED PATHCARD: Use SituationChoiceExecutor
            if (situation == null)
                return IntentResult.Failed();

            // TWO-PHASE SCALING: Pass pre-scaled values from PathCard (populated by SceneFacade)
            // Perfect Information compliance: display = execution (arc42 §8.26)
            plan = _situationChoiceExecutor.ValidateAndExtract(
                card.ChoiceTemplate,
                card.Name,
                card.ScaledRequirement,    // Pre-scaled by SceneFacade
                card.ScaledConsequence);   // Pre-scaled by SceneFacade
        }
        else
        {
            // ATMOSPHERIC PATHCARD (FALLBACK SCENE): Use LocationActionExecutor
            plan = _locationActionExecutor.ValidateAtmosphericPathCard(card);
        }

        if (!plan.IsValid)
        {
            _messageSystem.AddSystemMessage(plan.FailureReason, SystemMessageTypes.Warning, null);
            return IntentResult.Failed();
        }

        // STEP 2: Apply strategic costs (HIGHLANDER: single source of truth via Consequence class)
        // TWO PILLARS: All cost/reward application uses Consequence class
        Consequence pathCosts = new Consequence
        {
            Coins = -plan.CoinsCost,
            Health = -plan.HealthCost,
            Stamina = -plan.StaminaCost,
            Focus = -plan.FocusCost,
            Resolve = -plan.ResolveCoins,
            Hunger = plan.HungerCost  // Hunger increase is positive (adding hunger is bad)
        };
        await _rewardApplicationService.ApplyConsequence(pathCosts, situation);

        TimeBlocks oldTimeBlock = _timeFacade.GetCurrentTimeBlock();
        if (plan.TimeSegments > 0)
            _timeFacade.AdvanceSegments(plan.TimeSegments);
        TimeBlocks newTimeBlock = _timeFacade.GetCurrentTimeBlock();

        // STEP 3: Route based on ActionType
        if (plan.ActionType == ChoiceActionType.Instant)
        {
            // Apply rewards (scene-based Consequence OR atmospheric PathCard rewards)
            if (plan.IsAtmosphericAction)
            {
                // ATMOSPHERIC PATHCARD (FALLBACK SCENE): Apply PathCard-specific effects via Consequence
                // TWO PILLARS: All cost/reward application uses Consequence class
                Consequence pathCardEffects = new Consequence
                {
                    Coins = card.CoinReward,
                    Stamina = card.StaminaRestore,
                    Health = card.HealthEffect  // Negative = damage, Positive = recovery
                };
                await _rewardApplicationService.ApplyConsequence(pathCardEffects, situation);

                // Token gains (future implementation)
                // DOMAIN COLLECTION PRINCIPLE: Explicit properties for ConnectionType (fixed enum)
                if (card.HasAnyTokenGains())
                {
                    // Token system integration (future implementation)
                    // Access via card.TrustTokenGain, card.DiplomacyTokenGain, etc.
                }
            }
            else if (plan.Consequence != null)
            {
                // SCENE-BASED PATHCARD: Apply unified Consequence
                await _rewardApplicationService.ApplyConsequence(plan.Consequence, situation);
            }

            // THREE-TIER TIMING MODEL: No cleanup needed (actions are ephemeral, not stored)

            // MULTI-SITUATION SCENE: Complete situation and advance scene
            // Scene.AdvanceToNextSituation() called by completion handler
            // Enables linear progression through multi-situation arcs
            if (situation != null)
            {
                await _situationCompletionHandler.CompleteSituation(situation);
            }

            // Process time advancement (HIGHLANDER: single sync point)
            await ProcessTimeAdvancement(new TimeAdvancementResult
            {
                OldTimeBlock = oldTimeBlock,
                NewTimeBlock = newTimeBlock,
                CrossedTimeBlock = oldTimeBlock != newTimeBlock,
                SegmentsAdvanced = plan.TimeSegments
            });

            await Task.CompletedTask;
            return IntentResult.Executed(requiresRefresh: true);
        }
        else if (plan.ActionType == ChoiceActionType.StartChallenge)
        {
            // Route to tactical system based on ChallengeType
            return RouteToTacticalChallenge(plan);
        }

        return IntentResult.Failed();
    }

    // ========== HELPER METHODS ==========

    // ============================================
    // INTERACTION HISTORY
    // Records player interactions using update-in-place pattern (ONE record per entity)
    // ============================================

    /// <summary>
    /// Record location visit in player interaction history
    /// Update-in-place pattern: Find existing record or create new
    /// ONE record per location (replaces previous timestamp)
    /// </summary>
    private void RecordLocationVisit(Location location)
    {
        Player player = _gameWorld.GetPlayer();

        LocationVisitRecord existingRecord = player.LocationVisits
            .FirstOrDefault(record => record.Location == location);

        if (existingRecord != null)
        {
            existingRecord.LastVisitDay = _timeFacade.GetCurrentDay();
            existingRecord.LastVisitTimeBlock = _timeFacade.GetCurrentTimeBlock();
            existingRecord.LastVisitSegment = _timeFacade.GetCurrentSegment();
        }
        else
        {
            player.LocationVisits.Add(new LocationVisitRecord
            {
                Location = location,
                LastVisitDay = _timeFacade.GetCurrentDay(),
                LastVisitTimeBlock = _timeFacade.GetCurrentTimeBlock(),
                LastVisitSegment = _timeFacade.GetCurrentSegment()
            });
        }
    }

    /// <summary>
    /// Record NPC interaction in player interaction history
    /// Update-in-place pattern: Find existing record or create new
    /// ONE record per NPC (replaces previous timestamp)
    /// HIGHLANDER: Accept NPC object, use object equality
    /// </summary>
    private void RecordNPCInteraction(NPC npc)
    {
        Player player = _gameWorld.GetPlayer();

        NPCInteractionRecord existingRecord = player.NPCInteractions
            .FirstOrDefault(record => record.Npc == npc);

        if (existingRecord != null)
        {
            existingRecord.LastInteractionDay = _timeFacade.GetCurrentDay();
            existingRecord.LastInteractionTimeBlock = _timeFacade.GetCurrentTimeBlock();
            existingRecord.LastInteractionSegment = _timeFacade.GetCurrentSegment();
        }
        else
        {
            player.NPCInteractions.Add(new NPCInteractionRecord
            {
                Npc = npc,
                LastInteractionDay = _timeFacade.GetCurrentDay(),
                LastInteractionTimeBlock = _timeFacade.GetCurrentTimeBlock(),
                LastInteractionSegment = _timeFacade.GetCurrentSegment()
            });
        }
    }

    /// <summary>
    /// Record route traversal in player interaction history
    /// Update-in-place pattern: Find existing record or create new
    /// ONE record per route (replaces previous timestamp)
    /// </summary>
    private void RecordRouteTraversal(RouteOption route)
    {
        Player player = _gameWorld.GetPlayer();

        RouteTraversalRecord existingRecord = player.RouteTraversals
            .FirstOrDefault(record => record.Route == route);

        if (existingRecord != null)
        {
            existingRecord.LastTraversalDay = _timeFacade.GetCurrentDay();
            existingRecord.LastTraversalTimeBlock = _timeFacade.GetCurrentTimeBlock();
            existingRecord.LastTraversalSegment = _timeFacade.GetCurrentSegment();
        }
        else
        {
            player.RouteTraversals.Add(new RouteTraversalRecord
            {
                Route = route,
                LastTraversalDay = _timeFacade.GetCurrentDay(),
                LastTraversalTimeBlock = _timeFacade.GetCurrentTimeBlock(),
                LastTraversalSegment = _timeFacade.GetCurrentSegment()
            });
        }
    }

    private IntentResult RouteToTacticalChallenge(ActionExecutionPlan plan)
    {
        // Store CompletionReward in appropriate PendingContext
        // Consequence will be applied when challenge completes successfully
        if (plan.ChallengeType == TacticalSystemType.Social)
        {
            // Store Social context with CompletionReward
            _gameWorld.PendingSocialContext = new SocialChallengeContext
            {
                CompletionReward = plan.Consequence
            };
            // Navigate to social challenge screen
            return IntentResult.NavigateScreen(ScreenMode.SocialChallenge);
        }
        else if (plan.ChallengeType == TacticalSystemType.Mental)
        {
            // Store Mental context with CompletionReward
            _gameWorld.PendingMentalContext = new MentalChallengeContext
            {
                CompletionReward = plan.Consequence
            };
            // Navigate to mental challenge screen
            return IntentResult.NavigateScreen(ScreenMode.MentalChallenge);
        }
        else if (plan.ChallengeType == TacticalSystemType.Physical)
        {
            // Store Physical context with CompletionReward
            _gameWorld.PendingPhysicalContext = new PhysicalChallengeContext
            {
                CompletionReward = plan.Consequence
            };
            // Navigate to physical challenge screen
            return IntentResult.NavigateScreen(ScreenMode.PhysicalChallenge);
        }

        return IntentResult.Failed();
    }

    /// <summary>
    /// Process social challenge outcome - apply CompletionReward if successful, FailureReward if failed
    /// STRATEGIC LAYER: GameOrchestrator applies rewards after receiving tactical outcome
    /// </summary>
    public async Task ProcessSocialChallengeOutcome()
    {
        // PROCEDURAL CONTENT TRACING: Use stored ChoiceExecution for context
        ChoiceExecutionNode choiceNode = _gameWorld.PendingSocialContext?.ChoiceExecution;
        if (choiceNode != null)
        {
            _proceduralTracer.PushChoiceContext(choiceNode);
        }

        try
        {
            if (_gameWorld.LastSocialOutcome?.Success == true &&
                _gameWorld.PendingSocialContext?.CompletionReward != null)
            {
                Situation currentSituation = _gameWorld.PendingSocialContext.Situation;
                await _rewardApplicationService.ApplyConsequence(
                    _gameWorld.PendingSocialContext.CompletionReward,
                    currentSituation);
            }
            else if (_gameWorld.LastSocialOutcome?.Success == false &&
                     _gameWorld.PendingSocialContext?.FailureReward != null)
            {
                Situation currentSituation = _gameWorld.PendingSocialContext.Situation;
                await _rewardApplicationService.ApplyConsequence(
                    _gameWorld.PendingSocialContext.FailureReward,
                    currentSituation);
            }
        }
        finally
        {
            // ALWAYS pop context (even on exception)
            if (choiceNode != null)
            {
                _proceduralTracer.PopChoiceContext();
            }
            _gameWorld.PendingSocialContext = null;
        }
    }

    /// <summary>
    /// Process mental challenge outcome - apply CompletionReward if successful, FailureReward if failed
    /// STRATEGIC LAYER: GameOrchestrator applies rewards after receiving tactical outcome
    /// </summary>
    public async Task ProcessMentalChallengeOutcome()
    {
        // PROCEDURAL CONTENT TRACING: Use stored ChoiceExecution for context
        ChoiceExecutionNode choiceNode = _gameWorld.PendingMentalContext?.ChoiceExecution;
        if (choiceNode != null)
        {
            _proceduralTracer.PushChoiceContext(choiceNode);
        }

        try
        {
            if (_gameWorld.LastMentalOutcome?.Success == true &&
                _gameWorld.PendingMentalContext?.CompletionReward != null)
            {
                Situation currentSituation = _gameWorld.PendingMentalContext.Situation;
                await _rewardApplicationService.ApplyConsequence(
                    _gameWorld.PendingMentalContext.CompletionReward,
                    currentSituation);
            }
            else if (_gameWorld.LastMentalOutcome?.Success == false &&
                     _gameWorld.PendingMentalContext?.FailureReward != null)
            {
                Situation currentSituation = _gameWorld.PendingMentalContext.Situation;
                await _rewardApplicationService.ApplyConsequence(
                    _gameWorld.PendingMentalContext.FailureReward,
                    currentSituation);
            }
        }
        finally
        {
            // ALWAYS pop context (even on exception)
            if (choiceNode != null)
            {
                _proceduralTracer.PopChoiceContext();
            }
            _gameWorld.PendingMentalContext = null;
        }
    }

    /// <summary>
    /// Process physical challenge outcome - apply CompletionReward if successful, FailureReward if failed
    /// STRATEGIC LAYER: GameOrchestrator applies rewards after receiving tactical outcome
    /// </summary>
    public async Task ProcessPhysicalChallengeOutcome()
    {
        // PROCEDURAL CONTENT TRACING: Use stored ChoiceExecution for context
        ChoiceExecutionNode choiceNode = _gameWorld.PendingPhysicalContext?.ChoiceExecution;
        if (choiceNode != null)
        {
            _proceduralTracer.PushChoiceContext(choiceNode);
        }

        try
        {
            if (_gameWorld.LastPhysicalOutcome?.Success == true &&
                _gameWorld.PendingPhysicalContext?.CompletionReward != null)
            {
                Situation currentSituation = _gameWorld.PendingPhysicalContext.Situation;
                await _rewardApplicationService.ApplyConsequence(
                    _gameWorld.PendingPhysicalContext.CompletionReward,
                    currentSituation);
            }
            else if (_gameWorld.LastPhysicalOutcome?.Success == false &&
                     _gameWorld.PendingPhysicalContext?.FailureReward != null)
            {
                Situation currentSituation = _gameWorld.PendingPhysicalContext.Situation;
                await _rewardApplicationService.ApplyConsequence(
                    _gameWorld.PendingPhysicalContext.FailureReward,
                    currentSituation);
            }
        }
        finally
        {
            // ALWAYS pop context (even on exception)
            if (choiceNode != null)
            {
                _proceduralTracer.PopChoiceContext();
            }
            _gameWorld.PendingPhysicalContext = null;
        }
    }

    private IntentResult ApplyNavigationPayload(NavigationPayload payload)
    {
        // Apply navigation based on payload type
        // NavigationPayload structure from Consequence - contains target location/route
        // For now, return to location screen (navigation logic to be implemented)
        return IntentResult.Executed(requiresRefresh: true);
    }

}
