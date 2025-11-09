using System.Text;
using Wayfarer.Content;
using Wayfarer.GameState.Enums;
using Wayfarer.Services;

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
    private readonly CubeFacade _cubeFacade;
    private readonly ObligationActivity _obligationActivity;
    private readonly ObligationDiscoveryEvaluator _obligationDiscoveryEvaluator;
    private readonly ConversationTreeFacade _conversationTreeFacade;
    private readonly ObservationFacade _observationFacade;
    private readonly EmergencyFacade _emergencyFacade;
    private readonly SituationFacade _situationFacade;
    private readonly LocationActionExecutor _locationActionExecutor;
    private readonly NPCActionExecutor _npcActionExecutor;
    private readonly PathCardExecutor _pathCardExecutor;
    private readonly ConsequenceFacade _consequenceFacade;
    private readonly RewardApplicationService _rewardApplicationService;
    private readonly SpawnFacade _spawnFacade;
    private readonly SceneFacade _sceneFacade;
    private readonly SituationCompletionHandler _situationCompletionHandler;
    private readonly SceneInstanceFacade _sceneInstanceFacade;
    private readonly PackageLoaderFacade _packageLoaderFacade;
    private readonly HexRouteGenerator _hexRouteGenerator;
    private readonly ContentGenerationFacade _contentGenerationFacade;
    private readonly SceneInstantiator _sceneInstantiator;
    private readonly DependentResourceOrchestrationService _dependentResourceOrchestrationService;

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
        CubeFacade cubeFacade,
        ObligationActivity obligationActivity,
        ObligationDiscoveryEvaluator obligationDiscoveryEvaluator,
        ConversationTreeFacade conversationTreeFacade,
        ObservationFacade observationFacade,
        EmergencyFacade emergencyFacade,
        SituationFacade situationFacade,
        LocationActionExecutor locationActionExecutor,
        NPCActionExecutor npcActionExecutor,
        PathCardExecutor pathCardExecutor,
        ConsequenceFacade consequenceFacade,
        RewardApplicationService rewardApplicationService,
        SpawnConditionsEvaluator spawnConditionsEvaluator,
        SpawnFacade spawnFacade,
        SceneFacade sceneFacade,
        SituationCompletionHandler situationCompletionHandler,
        SceneInstanceFacade sceneInstanceFacade,
        PackageLoaderFacade packageLoaderFacade,
        HexRouteGenerator hexRouteGenerator,
        ContentGenerationFacade contentGenerationFacade,
        SceneInstantiator sceneInstantiator,
        DependentResourceOrchestrationService dependentResourceOrchestrationService)
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
        _npcActionExecutor = npcActionExecutor ?? throw new ArgumentNullException(nameof(npcActionExecutor));
        _pathCardExecutor = pathCardExecutor ?? throw new ArgumentNullException(nameof(pathCardExecutor));
        _consequenceFacade = consequenceFacade ?? throw new ArgumentNullException(nameof(consequenceFacade));
        _rewardApplicationService = rewardApplicationService ?? throw new ArgumentNullException(nameof(rewardApplicationService));
        _spawnFacade = spawnFacade ?? throw new ArgumentNullException(nameof(spawnFacade));
        _sceneFacade = sceneFacade ?? throw new ArgumentNullException(nameof(sceneFacade));
        _situationCompletionHandler = situationCompletionHandler ?? throw new ArgumentNullException(nameof(situationCompletionHandler));
        _sceneInstanceFacade = sceneInstanceFacade ?? throw new ArgumentNullException(nameof(sceneInstanceFacade));
        _packageLoaderFacade = packageLoaderFacade ?? throw new ArgumentNullException(nameof(packageLoaderFacade));
        _hexRouteGenerator = hexRouteGenerator ?? throw new ArgumentNullException(nameof(hexRouteGenerator));
        _contentGenerationFacade = contentGenerationFacade ?? throw new ArgumentNullException(nameof(contentGenerationFacade));
        _sceneInstantiator = sceneInstantiator ?? throw new ArgumentNullException(nameof(sceneInstantiator));
        _dependentResourceOrchestrationService = dependentResourceOrchestrationService ?? throw new ArgumentNullException(nameof(dependentResourceOrchestrationService));
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
            return _gameWorld.GetJobById(player.ActiveDeliveryJobId);
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

    public PlayerStats GetPlayerStats()
    {
        return _gameWorld.GetPlayer().Stats;
    }

    public List<NPC> GetAvailableStrangers(string venueId)
    {
        TimeBlocks currentTime = _timeFacade.GetCurrentTimeBlock();
        return _gameWorld.GetAvailableStrangers(venueId, currentTime);
    }

    public List<ObligationApproach> GetAvailableObligationApproaches()
    {
        Player player = _gameWorld.GetPlayer();
        return _locationFacade.GetAvailableApproaches(player);
    }

    // ========== LOCATION OPERATIONS ==========
    // Venue is ALWAYS derived from Location: location.Venue

    public Location GetLocation(string LocationId)
    {
        return _gameWorld.GetLocation(LocationId);
    }

    public Location GetCurrentLocation()
    {
        return _locationFacade.GetCurrentLocation();
    }

    public bool MoveToSpot(string locationId)
    {
        bool success = _locationFacade.MoveToSpot(locationId);

        // Movement to new Venue may unlock obligation discovery (ImmediateVisibility, EnvironmentalObservation triggers)
        if (success)
        {
            EvaluateObligationDiscovery();
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

    public NPC GetNPCById(string npcId)
    {
        return _locationFacade.GetNPCById(npcId);
    }

    public List<NPC> GetNPCsAtLocation(string venueId)
    {
        return _locationFacade.GetNPCsAtLocation(venueId);
    }

    public List<NPC> GetNPCsAtCurrentSpot()
    {
        return _locationFacade.GetNPCsAtCurrentSpot();
    }

    public LocationScreenViewModel GetLocationScreen()
    {
        // NPCs no longer have inline conversation options - situations are location-based in GameWorld.Situations
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

            // Get the actual destination location from the route
            RouteOption? actualRoute = _travelFacade.GetAvailableRoutesFromCurrentLocation()
                .FirstOrDefault(r => r.Id == routeId);

            if (actualRoute != null)
            {
                // Find the destination location by its ID from GameWorld's Locations dictionary
                Location? destSpot = _gameWorld.GetLocation(actualRoute.DestinationLocationId);

                if (destSpot != null)
                {
                    if (!destSpot.HexPosition.HasValue)
                        throw new InvalidOperationException($"Destination location '{destSpot.Id}' has no HexPosition - cannot move player");

                    player.CurrentPosition = destSpot.HexPosition.Value;

                    // TRIGGER POINT 1: Record location visit after successful travel
                    RecordLocationVisit(destSpot.Id);

                    // TRIGGER POINT 3: Record route traversal after successful travel
                    RecordRouteTraversal(routeId);

                    // AUTOMATIC SPAWNING ORCHESTRATION - Location trigger
                    // Check for procedural scenes that become eligible when entering this location
                    // Handoff implementation: Phase 4 (lines 254-260)
                    _spawnFacade.CheckAndSpawnEligibleScenes(SpawnTriggerType.Location, destSpot.Id);
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

            // Get destination Venue name for the message
            Location? finalDestSpot = _gameWorld.GetLocation(targetRoute.DestinationLocationId);

            string destinationName = "Unknown";
            if (finalDestSpot != null)
            {
                Venue? destLocation = _gameWorld.Venues
                    .FirstOrDefault(l => l.Id == finalDestSpot.VenueId);
                if (destLocation != null)
                {
                    destinationName = destLocation.Name;
                }
                else if (!string.IsNullOrEmpty(finalDestSpot.Name))
                {
                    destinationName = finalDestSpot.Name;
                }
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

    public async Task<WorkResult> PerformWork(ActionRewards rewards)
    {
        WorkResult result = _resourceFacade.PerformWork(rewards);
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

    // ========== DEPRECATED - ACTION EXECUTION REPLACED BY INTENT SYSTEM ==========
    // Old ExecutePlayerAction and ExecuteLocationAction methods deleted.
    // All actions now execute through ProcessIntent() for unified handling.

    // ========== CONVERSATION OPERATIONS ==========

    public SocialFacade GetConversationFacade()
    {
        return _conversationFacade;
    }

    public async Task<SocialChallengeContext> CreateConversationContext(string npcId, string requestId)
    {
        return await _conversationFacade.CreateConversationContext(npcId, requestId);
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
    public MentalSession StartMentalSession(string deckId, string locationId, string situationId, string obligationId)
    {
        if (_mentalFacade.IsSessionActive())
            throw new InvalidOperationException("Mental session already active");

        MentalChallengeDeck challengeDeck = _gameWorld.MentalChallengeDecks.FirstOrDefault(d => d.Id == deckId);
        if (challengeDeck == null)
            throw new InvalidOperationException($"MentalChallengeDeck {deckId} not found");

        Player player = _gameWorld.GetPlayer();

        // Build deck with signature deck knowledge cards in starting hand
        MentalDeckBuildResult buildResult = _mentalFacade.GetDeckBuilder()
            .BuildDeckWithStartingHand(challengeDeck, player);

        return _mentalFacade.StartSession(challengeDeck, buildResult.Deck, buildResult.StartingHand, situationId, obligationId);
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
    public PhysicalSession StartPhysicalSession(string deckId, string locationId, string situationId, string obligationId)
    {
        if (_physicalFacade.IsSessionActive())
            throw new InvalidOperationException("Physical session already active");

        PhysicalChallengeDeck challengeDeck = _gameWorld.PhysicalChallengeDecks.FirstOrDefault(d => d.Id == deckId);
        if (challengeDeck == null)
            throw new InvalidOperationException($"PhysicalChallengeDeck {deckId} not found");

        Player player = _gameWorld.GetPlayer();

        // Build deck with signature deck knowledge cards in starting hand
        PhysicalDeckBuildResult buildResult = _physicalFacade.GetDeckBuilder()
            .BuildDeckWithStartingHand(challengeDeck, player);

        return _physicalFacade.StartSession(challengeDeck, buildResult.Deck, buildResult.StartingHand, situationId, obligationId);
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
        return _physicalFacade.IsSessionActive();
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

        // Get current location (Venue derived from Location)
        Location? currentLocation = GetCurrentLocation();
        Venue? venue = currentLocation?.Venue;

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
                LocationId = currentLocation.Id,
                LocationName = currentLocation.Name,
                VenueId = currentLocation.VenueId,
                VenueName = venue?.Name,
                Description = venue?.Description
            },
            CurrentTimeBlock = timeBlock,
            PlayerResources = playerResources,
            PlayerTokens = npcTokens,
            PlayerInventory = GetPlayerInventoryAsDictionary(),
            Session = new ExchangeSession
            {
                NpcId = npcId,
                LocationId = currentLocation.Id,
                AvailableExchanges = availableExchanges
            }
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
        // Check if game is already started to prevent duplicate initialization
        // This is CRITICAL for ServerPrerendered mode compatibility
        if (_gameWorld.IsGameStarted)
        {
            return;
        }

        // Initialize player at starting Venue from GameWorld initial conditions
        // HEX-FIRST ARCHITECTURE: Player position is hex coordinates
        Player player = _gameWorld.GetPlayer();
        string startingSpotId = _gameWorld.InitialLocationId;
        Location? startingSpot = _gameWorld.Locations.FirstOrDefault(s => s.Id == startingSpotId);
        if (startingSpot == null)
            throw new InvalidOperationException($"Invalid InitialLocationId '{startingSpotId}' - no matching Location found in GameWorld.Locations");

        if (!startingSpot.HexPosition.HasValue)
            throw new InvalidOperationException($"Starting location '{startingSpotId}' has no HexPosition - cannot initialize player position");

        player.CurrentPosition = startingSpot.HexPosition.Value;
        Venue? startingLocation = _gameWorld.Venues.FirstOrDefault(l => l.Id == startingSpot.VenueId);

        // Player resources already applied by PackageLoader.ApplyInitialPlayerConfiguration()
        // No need to re-apply here - HIGHLANDER PRINCIPLE: initialization happens ONCE

        // Initialize time state from GameWorld initial conditions
        if (_gameWorld.InitialTimeBlock.HasValue && _gameWorld.InitialSegment.HasValue)
        {
            _timeFacade.SetInitialTimeState(
                _gameWorld.InitialDay ?? 1,
                _gameWorld.InitialTimeBlock.Value,
                _gameWorld.InitialSegment.Value);
        }// Initialize exchange inventories
        _exchangeFacade.InitializeNPCExchanges();// Mark game as started
        _gameWorld.IsGameStarted = true;

        // Spawn starter scenes (tutorial content, initial situations)
        SpawnStarterScenes();

        _messageSystem.AddSystemMessage("Game started", SystemMessageTypes.Success);
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
            TravelIntent travel => await ProcessTravelIntentAsync(travel.RouteId),

            // Movement intents
            MoveIntent move => ProcessMoveIntent(move.TargetSpotId),

            // Player action intents
            WaitIntent => ProcessWaitIntent(),
            SleepOutsideIntent => ProcessSleepOutsideIntent(),
            LookAroundIntent => ProcessLookAroundIntent(),
            CheckBelongingsIntent => ProcessCheckBelongingsIntent(),

            // Location action intents
            RestAtLocationIntent => await ProcessRestAtLocationIntent(),
            SecureRoomIntent => await ProcessSecureRoomIntent(),
            WorkIntent => await ProcessWorkIntent(),
            InvestigateLocationIntent => ProcessInvestigateIntent(),

            // Delivery job intents (Core Loop Phase 3)
            ViewJobBoardIntent => ProcessViewJobBoardIntent(),
            AcceptDeliveryJobIntent accept => ProcessAcceptDeliveryJobIntent(accept.JobId),
            CompleteDeliveryIntent => ProcessCompleteDeliveryIntent(),

            // Conversation/NPC intents
            TalkIntent talk => await ProcessTalkIntent(talk.NpcId),

            _ => IntentResult.Failed()
        };
    }

    // ========== NAVIGATION INTENT HANDLERS ==========

    private IntentResult ProcessOpenTravelScreenIntent()
    {
        // Backend authority: Navigate to Travel screen to view available routes
        return IntentResult.NavigateScreen(ScreenMode.Travel);
    }

    private async Task<IntentResult> ProcessTravelIntentAsync(string routeId)
    {
        // Backend authority: Navigate to Travel screen (screen-level navigation)
        bool success = await TravelToDestinationAsync(routeId);
        return success ? IntentResult.NavigateScreen(ScreenMode.Travel) : IntentResult.Failed();
    }

    // ========== MOVEMENT INTENT HANDLERS ==========

    private IntentResult ProcessMoveIntent(string targetSpotId)
    {
        bool success = MoveToSpot(targetSpotId);

        if (success)
        {
            // TRIGGER POINT 1: Record location visit after successful movement
            RecordLocationVisit(targetSpotId);
        }

        return success ? IntentResult.Executed(requiresRefresh: true) : IntentResult.Failed();
    }

    // ========== PLAYER ACTION INTENT HANDLERS ==========

    private IntentResult ProcessWaitIntent()
    {
        // Fetch entity for data-driven execution
        PlayerAction action = _gameWorld.PlayerActions.FirstOrDefault(a => a.ActionType == PlayerActionType.Wait);
        if (action == null)
        {
            _messageSystem.AddSystemMessage("Wait action not found", SystemMessageTypes.Warning);
            return IntentResult.Failed();
        }

        // Execute with data from entity
        TimeBlocks oldTimeBlock = _timeFacade.GetCurrentTimeBlock();
        _timeFacade.AdvanceSegments(1); // ORCHESTRATION: GameFacade controls time progression
        _resourceFacade.ExecuteWait(); // Resource effects only, no time progression
        TimeBlocks newTimeBlock = _timeFacade.GetCurrentTimeBlock();

        ProcessTimeAdvancement(new TimeAdvancementResult
        {
            OldTimeBlock = oldTimeBlock,
            NewTimeBlock = newTimeBlock,
            CrossedTimeBlock = oldTimeBlock != newTimeBlock,
            SegmentsAdvanced = action.TimeRequired
        });

        return IntentResult.Executed(requiresRefresh: true);
    }

    private IntentResult ProcessSleepOutsideIntent()
    {
        // Fetch entity for data-driven costs
        PlayerAction action = _gameWorld.PlayerActions.FirstOrDefault(a => a.ActionType == PlayerActionType.SleepOutside);
        if (action == null)
        {
            _messageSystem.AddSystemMessage("SleepOutside action not found", SystemMessageTypes.Warning);
            return IntentResult.Failed();
        }

        // Apply costs from entity (data-driven)
        Player player = _gameWorld.GetPlayer();
        int healthCost = action.Costs.Health;
        player.ModifyHealth(-healthCost);

        _messageSystem.AddSystemMessage(
            $"You sleep rough on a bench. Cold. Uncomfortable. You wake stiff and sore. (-{healthCost} Health)",
            SystemMessageTypes.Warning);

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
            _messageSystem.AddSystemMessage("Rest action not found", SystemMessageTypes.Warning);
            return IntentResult.Failed();
        }

        // Execute with data from entity
        TimeBlocks oldTimeBlock = _timeFacade.GetCurrentTimeBlock();
        _timeFacade.AdvanceSegments(1); // ORCHESTRATION: GameFacade controls time progression
        _resourceFacade.ExecuteRest(action.Rewards); // Resource effects only, no time progression
        TimeBlocks newTimeBlock = _timeFacade.GetCurrentTimeBlock();

        ProcessTimeAdvancement(new TimeAdvancementResult
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
            _messageSystem.AddSystemMessage("SecureRoom action not found", SystemMessageTypes.Warning);
            return IntentResult.Failed();
        }

        Player player = _gameWorld.GetPlayer();

        // Record recovery amounts for message
        int healthBefore = player.Health;
        int staminaBefore = player.Stamina;
        int hungerBefore = player.Hunger;
        int focusBefore = player.Focus;

        // Check if action grants full recovery (data-driven from JSON)
        if (action.Rewards.FullRecovery)
        {
            // Full resource recovery
            player.Health = player.MaxHealth;
            player.Stamina = player.MaxStamina;
            player.Hunger = 0; // Full recovery means no hunger
            player.Focus = player.MaxFocus;
        }
        else
        {
            // Partial recovery based on rewards
            player.Health = Math.Min(player.Health + action.Rewards.HealthRecovery, player.MaxHealth);
            player.Stamina = Math.Min(player.Stamina + action.Rewards.StaminaRecovery, player.MaxStamina);
            player.Focus = Math.Min(player.Focus + action.Rewards.FocusRecovery, player.MaxFocus);
        }

        int healthRecovered = player.Health - healthBefore;
        int staminaRecovered = player.Stamina - staminaBefore;
        int hungerRecovered = hungerBefore - player.Hunger;
        int focusRecovered = player.Focus - focusBefore;

        // Generate recovery message
        string recoveryMessage = "You rest in a secure room through the night.";
        if (healthRecovered > 0) recoveryMessage += $" Health +{healthRecovered}";
        if (staminaRecovered > 0) recoveryMessage += $" Stamina +{staminaRecovered}";
        if (hungerRecovered > 0) recoveryMessage += $" Hunger -{hungerRecovered}";
        if (focusRecovered > 0) recoveryMessage += $" Focus +{focusRecovered}";
        if (action.Rewards.FullRecovery) recoveryMessage += " (Fully recovered)";
        _messageSystem.AddSystemMessage(recoveryMessage, SystemMessageTypes.Success);

        // Advance to next day morning
        TimeAdvancementResult timeResult = _timeFacade.AdvanceToNextDay();
        ProcessTimeAdvancement(timeResult);
        // NOTE: Time-based spawn trigger now fires inside ProcessTimeAdvancement (HIGHLANDER principle)
        // No need to call CheckAndSpawnEligibleScenes here - it's handled automatically

        await Task.CompletedTask;
        return IntentResult.Executed(requiresRefresh: true);
    }

    private async Task<IntentResult> ProcessWorkIntent()
    {
        // Fetch entity for data-driven rewards
        LocationAction action = _gameWorld.LocationActions.FirstOrDefault(a => a.ActionType == LocationActionType.Work);
        if (action == null)
        {
            _messageSystem.AddSystemMessage("Work action not found", SystemMessageTypes.Warning);
            return IntentResult.Failed();
        }

        // Execute with data from entity
        await PerformWork(action.Rewards);
        return IntentResult.Executed(requiresRefresh: true);
    }

    private IntentResult ProcessInvestigateIntent()
    {
        Location currentSpot = GetCurrentLocation();
        if (currentSpot == null)
        {
            _messageSystem.AddSystemMessage("No current location to investigate", SystemMessageTypes.Warning);
            return IntentResult.Failed();
        }

        _locationFacade.InvestigateLocation(currentSpot.Id);
        _messageSystem.AddSystemMessage("🔍 Investigated location, gaining familiarity", SystemMessageTypes.Info);
        return IntentResult.Executed(requiresRefresh: true);
    }

    // ========== DELIVERY JOB INTENT HANDLERS ==========

    private IntentResult ProcessViewJobBoardIntent()
    {
        // Navigate to JobBoard view modal
        return IntentResult.NavigateView(LocationViewState.JobBoard);
    }

    private IntentResult ProcessAcceptDeliveryJobIntent(string jobId)
    {
        Player player = _gameWorld.GetPlayer();

        // Validation: Can only have one active job
        if (player.HasActiveDeliveryJob)
        {
            _messageSystem.AddSystemMessage("You already have an active delivery job", SystemMessageTypes.Warning);
            return IntentResult.Failed();
        }

        // Get job and validate
        DeliveryJob job = _gameWorld.GetJobById(jobId);
        if (job == null || !job.IsAvailable)
        {
            _messageSystem.AddSystemMessage("Job no longer available", SystemMessageTypes.Warning);
            return IntentResult.Failed();
        }

        // Accept job
        player.ActiveDeliveryJobId = jobId;
        job.IsAvailable = false;

        _messageSystem.AddSystemMessage($"Accepted: {job.JobDescription}", SystemMessageTypes.Success);

        // Close modal and refresh location
        return IntentResult.Executed(requiresRefresh: true);
    }

    private IntentResult ProcessCompleteDeliveryIntent()
    {
        Player player = _gameWorld.GetPlayer();

        // Validation: Must have active job
        if (!player.HasActiveDeliveryJob)
        {
            _messageSystem.AddSystemMessage("No active delivery job", SystemMessageTypes.Warning);
            return IntentResult.Failed();
        }

        DeliveryJob job = _gameWorld.GetJobById(player.ActiveDeliveryJobId);
        if (job == null)
        {
            _messageSystem.AddSystemMessage("Delivery job data not found", SystemMessageTypes.Warning);
            return IntentResult.Failed();
        }

        // Pay player
        player.ModifyCoins(job.Payment);

        // Clear active job
        player.ActiveDeliveryJobId = "";

        // Advance time (delivery takes time)
        TimeBlocks oldTimeBlock = _timeFacade.GetCurrentTimeBlock();
        TimeBlocks newTimeBlock = _timeFacade.AdvanceSegments(1);

        // HIGHLANDER: Process time advancement side effects (hunger, emergencies, time-based spawns)
        ProcessTimeAdvancement(new TimeAdvancementResult
        {
            OldTimeBlock = oldTimeBlock,
            NewTimeBlock = newTimeBlock,
            CrossedTimeBlock = oldTimeBlock != newTimeBlock,
            SegmentsAdvanced = 1
        });

        _messageSystem.AddSystemMessage($"Delivery complete! Earned {job.Payment} coins", SystemMessageTypes.Success);

        return IntentResult.Executed(requiresRefresh: true);
    }

    // ========== NPC INTERACTION INTENT HANDLERS ==========

    private async Task<IntentResult> ProcessTalkIntent(string npcId)
    {
        // AUTOMATIC SPAWNING ORCHESTRATION - NPC trigger
        // Check for procedural scenes that become eligible when interacting with this NPC
        // GameFacade orchestrates: Player talks to NPC, then SpawnFacade checks for eligible scenes
        _spawnFacade.CheckAndSpawnEligibleScenes(SpawnTriggerType.NPC, npcId);

        // TODO: Implement conversation initiation
        _messageSystem.AddSystemMessage($"Talking to NPC {npcId} not yet implemented", SystemMessageTypes.Info);
        await Task.CompletedTask;
        return IntentResult.Executed(requiresRefresh: false);
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

    /// <summary>
    /// HIGHLANDER: THE ONLY place for processing time advancement side effects.
    /// All time-based resource changes happen here (hunger, day transitions, emergency checking).
    /// Called after EVERY time advancement (Wait, Rest, Work, Travel, etc.).
    /// </summary>
    private void ProcessTimeAdvancement(TimeAdvancementResult result)
    {
        // HUNGER: +5 per segment (universal time cost)
        // This is THE ONLY place hunger increases due to time
        int hungerIncrease = result.SegmentsAdvanced * 5;
        _resourceFacade.IncreaseHunger(hungerIncrease, "Time passes");

        // DAY TRANSITION: Process dawn effects (NPC decay)
        // Only when crossing into Morning (new day starts)
        if (result.CrossedDayBoundary && result.NewTimeBlock == TimeBlocks.Morning)
        {
            _resourceFacade.ProcessDayTransition();
        }

        // EMERGENCY CHECKING: Check for active emergencies at sync points
        // Emergencies interrupt normal gameplay and demand immediate response
        EmergencySituation activeEmergency = CheckForActiveEmergency();
        if (activeEmergency != null)
        {
            _gameWorld.ActiveEmergency = activeEmergency;
            _messageSystem.AddSystemMessage(
                $"⚠️ EMERGENCY: {activeEmergency.Name}",
                SystemMessageTypes.Warning);
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
                // Optional: System message for player feedback (uncomment if desired)
                // _messageSystem.AddSystemMessage($"Opportunity expired: {scene.DisplayName}", SystemMessageTypes.Info);
            }
        }

        // AUTOMATIC SPAWNING ORCHESTRATION - Time trigger
        // Check for procedural scenes with time-based spawn conditions (morning, evening, day ranges)
        // HIGHLANDER: This ensures time-based spawns fire after EVERY time advancement
        // (Wait, Rest, Work, Travel, SecureRoom, Delivery, Action execution, etc.)
        _spawnFacade.CheckAndSpawnEligibleScenes(SpawnTriggerType.Time, contextId: null);
    }

    /// <summary>
    /// Converts player inventory to Dictionary format for ExchangeContext
    /// Key: ItemId, Value: Quantity
    /// </summary>
    private Dictionary<string, int> GetPlayerInventoryAsDictionary()
    {
        Player player = _gameWorld.GetPlayer();
        Dictionary<string, int> inventoryDict = new Dictionary<string, int>();

        List<string> allItems = player.Inventory.GetAllItems();
        List<string> uniqueItemIds = player.Inventory.GetItemIds();

        foreach (string itemId in uniqueItemIds)
        {
            int count = player.Inventory.GetItemCount(itemId);
            inventoryDict[itemId] = count;
        }

        return inventoryDict;
    }

    /// <summary>
    /// Gets the district containing a location
    /// </summary>
    public District GetDistrictForLocation(string venueId)
    {
        return _gameWorld.GetDistrictForLocation(venueId);
    }

    /// <summary>
    /// Gets the region containing a district
    /// </summary>
    public Region GetRegionForDistrict(string districtId)
    {
        return _gameWorld.GetRegionForDistrict(districtId);
    }

    /// <summary>
    /// Gets all locations in GameWorld
    /// </summary>
    public List<Venue> GetAllLocations()
    {
        return _gameWorld.Venues;
    }

    /// <summary>
    /// Gets a district by its ID
    /// </summary>
    public District GetDistrictById(string districtId)
    {
        return _gameWorld.Districts.FirstOrDefault(d => d.Id == districtId);
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
            player.Health = Math.Clamp(player.Health + health, 0, player.MaxHealth);
            _messageSystem.AddSystemMessage($"Health {(health > 0 ? "+" : "")}{health} (now {player.Health})", SystemMessageTypes.Success);
        }

        if (hunger != 0)
        {
            player.Hunger = Math.Clamp(player.Hunger + hunger, 0, 100);
            _messageSystem.AddSystemMessage($"Hunger {(hunger > 0 ? "+" : "")}{hunger} (now {player.Hunger})", SystemMessageTypes.Success);
        }
    }

    public void DebugTeleportToLocation(string venueId, string LocationId)
    {
        Player player = _gameWorld.GetPlayer();
        Location location = _gameWorld.GetLocation(LocationId);

        if (location == null)
        {
            _messageSystem.AddSystemMessage($"location '{LocationId}' not found", SystemMessageTypes.Warning);
            return;
        }

        if (!location.HexPosition.HasValue)
        {
            _messageSystem.AddSystemMessage($"location '{LocationId}' has no HexPosition - cannot teleport", SystemMessageTypes.Warning);
            return;
        }

        Venue? venue = _gameWorld.Venues.FirstOrDefault(l => l.Id == venueId);
        if (venue == null)
        {
            _messageSystem.AddSystemMessage($"Location '{venueId}' not found", SystemMessageTypes.Warning);
            return;
        }

        player.CurrentPosition = location.HexPosition.Value;

        _messageSystem.AddSystemMessage($"Teleported to {venue.Name} - {location.Name}", SystemMessageTypes.Success);
    }

    // ========== OBLIGATION SYSTEM ==========

    /// <summary>
    /// Evaluate and trigger obligation discovery based on current player state
    /// Called when player moves to new location/location, gains knowledge, items, or accepts obligations
    /// Discovered obligations will have their intro situations added to the appropriate location
    /// and discovery modals will be triggered via pending results
    /// </summary>
    public void EvaluateObligationDiscovery()
    {
        Player player = _gameWorld.GetPlayer();// Evaluate which obligations can be discovered
        List<Obligation> discoverable = _obligationDiscoveryEvaluator.EvaluateDiscoverableObligations(player);// For each discovered obligation, trigger discovery flow
        foreach (Obligation obligation in discoverable)
        {// DiscoverObligation moves Potential→Discovered and spawns intro situation at location
            // No return value - situation is added directly to Location.ActiveSituations
            _obligationActivity.DiscoverObligation(obligation.Id);

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
    public void CompleteObligationIntro(string obligationId)
    {
        _obligationActivity.CompleteIntroAction(obligationId);
    }

    /// <summary>
    /// Set pending intro action - prepares quest acceptance modal
    /// RPG quest acceptance: Button → Modal → "Begin" → Activate
    /// </summary>
    public void SetPendingIntroAction(string obligationId)
    {
        _obligationActivity.SetPendingIntroAction(obligationId);
    }

    /// <summary>
    /// Get pending intro result for modal display
    /// </summary>
    public ObligationIntroResult GetPendingIntroResult()
    {
        return _obligationActivity.GetAndClearPendingIntroResult();
    }

    /// <summary>
    /// Get route by ID (V2 Travel Integration)
    /// </summary>
    public RouteOption GetRouteById(string routeId)
    {
        return _travelFacade.GetAvailableRoutesFromCurrentLocation()
            .FirstOrDefault(r => r.Id == routeId);
    }

    // ========== CONVERSATION TREE OPERATIONS ==========

    /// <summary>
    /// Create context for conversation tree screen
    /// </summary>
    public ConversationTreeContext CreateConversationTreeContext(string treeId)
    {
        return _conversationTreeFacade.CreateContext(treeId);
    }

    /// <summary>
    /// Select a dialogue response in conversation tree
    /// </summary>
    public ConversationTreeResult SelectConversationResponse(string treeId, string nodeId, string responseId)
    {
        return _conversationTreeFacade.SelectResponse(treeId, nodeId, responseId);
    }

    // ========== OBSERVATION SCENE OPERATIONS ==========

    /// <summary>
    /// Create context for observation scene screen
    /// </summary>
    public ObservationContext CreateObservationContext(string sceneId)
    {
        return _observationFacade.CreateContext(sceneId);
    }

    /// <summary>
    /// Examine a point in an observation scene
    /// </summary>
    public ObservationResult ExaminePoint(string sceneId, string pointId)
    {
        return _observationFacade.ExaminePoint(sceneId, pointId);
    }

    // ========== EMERGENCY SITUATION OPERATIONS ==========

    /// <summary>
    /// Check for active emergencies (called at sync points)
    /// </summary>
    public EmergencySituation CheckForActiveEmergency()
    {
        return _emergencyFacade.CheckForActiveEmergency();
    }

    /// <summary>
    /// Create context for emergency screen
    /// </summary>
    public EmergencyContext CreateEmergencyContext(string emergencyId)
    {
        return _emergencyFacade.CreateContext(emergencyId);
    }

    /// <summary>
    /// Select a response to an emergency situation
    /// </summary>
    public EmergencyResult SelectEmergencyResponse(string emergencyId, string responseId)
    {
        return _emergencyFacade.SelectResponse(emergencyId, responseId);
    }

    /// <summary>
    /// Ignore an emergency situation
    /// </summary>
    public EmergencyResult IgnoreEmergency(string emergencyId)
    {
        return _emergencyFacade.IgnoreEmergency(emergencyId);
    }

    // ========== LOCATION QUERY METHODS ==========

    /// <summary>
    /// Get all conversation trees available at a specific location
    /// </summary>
    public List<ConversationTree> GetAvailableConversationTreesAtLocation(string locationId)
    {
        return _conversationTreeFacade.GetAvailableTreesAtLocation(locationId);
    }

    /// <summary>
    /// Get all observation scenes available at a specific location
    /// </summary>
    public List<ObservationScene> GetAvailableObservationScenesAtLocation(string locationId)
    {
        return _observationFacade.GetAvailableScenesAtLocation(locationId);
    }

    /// <summary>
    /// Get all Situations available at a specific location
    /// Includes both legacy standalone Situations and Scene-embedded Situations
    /// Scene-embedded Situations inherit placement from parent Scene
    /// PHASE 0.2: Query ParentScene for placement using GetPlacementId() helper
    /// </summary>
    public List<Situation> GetAvailableSituationsAtLocation(string locationId)
    {
        // PLAYABILITY VALIDATION: Location must exist
        Location location = _gameWorld.Locations.FirstOrDefault(l => l.Id == locationId);
        if (location == null)
            throw new InvalidOperationException($"Location '{locationId}' not found in GameWorld - cannot query situations!");

        // Query all Situations (both legacy and Scene-embedded) at this location
        return _gameWorld.Scenes.SelectMany(s => s.Situations)
            .Where(sit => sit.GetPlacementId(PlacementType.Location) == locationId)
            .ToList();
    }

    /// <summary>
    /// Get all Situations available for a specific NPC
    /// Includes both legacy standalone Situations and Scene-embedded Situations
    /// PHASE 0.2: Query ParentScene for placement using GetPlacementId() helper
    /// </summary>
    public List<Situation> GetAvailableSituationsForNPC(string npcId)
    {
        NPC npc = _gameWorld.NPCs.FirstOrDefault(n => n.ID == npcId);
        if (npc == null) return new List<Situation>();

        // Query all Situations (both legacy and Scene-embedded) for this NPC
        return _gameWorld.Scenes.SelectMany(s => s.Situations)
            .Where(sit => sit.GetPlacementId(PlacementType.NPC) == npcId)
            .ToList();
    }

    // ========== ACTIVE EMERGENCY OPERATIONS ==========

    /// <summary>
    /// Get the currently active emergency (if any)
    /// </summary>
    public EmergencySituation GetActiveEmergency()
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
    /// GameFacade is SOLE orchestrator for multi-facade operations
    /// LET IT CRASH: Pipeline succeeds completely or throws exception with stack trace
    /// </summary>
    public Scene SpawnSceneWithDynamicContent(
        SceneTemplate template,
        SceneSpawnReward spawnReward,
        SceneSpawnContext context)
    {
        Scene provisionalScene = _sceneInstanceFacade.CreateProvisionalScene(template, spawnReward, context);

        if (provisionalScene == null)
        {
            Console.WriteLine($"[GameFacade] Scene '{template.Id}' failed spawn conditions - cannot finalize");
            return null;
        }

        SceneFinalizationResult finalizationResult = _sceneInstanceFacade.FinalizeScene(provisionalScene.Id, context);
        Scene scene = finalizationResult.Scene;
        DependentResourceSpecs dependentSpecs = finalizationResult.DependentSpecs;

        _dependentResourceOrchestrationService.LoadDependentResources(scene, dependentSpecs, context.Player);

        return scene;
    }

    /// <summary>
    /// Spawn all starter scenes during game initialization
    /// HIGHLANDER: Called ONCE from StartGameAsync() after IsGameStarted = true
    /// Starter scenes provide initial gameplay content (tutorial, intro situations)
    /// </summary>
    public void SpawnStarterScenes()
    {
        Player player = _gameWorld.GetPlayer();

        List<SceneTemplate> starterTemplates = _gameWorld.SceneTemplates
            .Where(t => t.IsStarter)
            .ToList();

        foreach (SceneTemplate template in starterTemplates)
        {
            PlacementRelation placementRelation = DeterminePlacementRelation(template.PlacementFilter);

            string specificPlacementId = null;
            if (template.PlacementFilter != null)
            {
                if (!string.IsNullOrEmpty(template.PlacementFilter.NpcId))
                    specificPlacementId = template.PlacementFilter.NpcId;
                else if (!string.IsNullOrEmpty(template.PlacementFilter.LocationId))
                    specificPlacementId = template.PlacementFilter.LocationId;
            }

            SceneSpawnReward spawnReward = new SceneSpawnReward
            {
                SceneTemplateId = template.Id,
                PlacementRelation = placementRelation,
                SpecificPlacementId = specificPlacementId,
                DelayDays = 0
            };

            SceneSpawnContext spawnContext = SceneSpawnContextBuilder.BuildContext(
                _gameWorld,
                player,
                placementRelation,
                specificPlacementId,
                null);

            if (spawnContext == null)
                continue;

            Scene scene = SpawnSceneWithDynamicContent(template, spawnReward, spawnContext);

            if (scene == null)
            {
                Console.WriteLine($"[GameFacade] Starter scene '{template.Id}' failed to spawn - skipping");
                continue;
            }
        }
    }

    /// <summary>
    /// Determine PlacementRelation from PlacementFilter
    /// TWO PATTERNS:
    /// 1. CONCRETE BINDING: NpcId/LocationId present → SpecificNPC/SpecificLocation
    /// 2. CATEGORICAL SEARCH: Only categorical properties → Generic
    /// </summary>
    private PlacementRelation DeterminePlacementRelation(PlacementFilter filter)
    {
        if (filter == null)
            return PlacementRelation.SpecificLocation;

        if (!string.IsNullOrEmpty(filter.NpcId))
            return PlacementRelation.SpecificNPC;

        if (!string.IsNullOrEmpty(filter.LocationId))
            return PlacementRelation.SpecificLocation;

        return PlacementRelation.Generic;
    }

    // ========== UNIFIED ACTION ARCHITECTURE - EXECUTE METHODS ==========

    /// <summary>
    /// Execute LocationAction through unified action architecture
    /// HIGHLANDER PATTERN: All location actions flow through this method
    /// </summary>
    public async Task<IntentResult> ExecuteLocationAction(string situationId, string actionId)
    {
        Player player = _gameWorld.GetPlayer();

        // Verify Situation exists
        Situation situation = _gameWorld.Scenes.SelectMany(s => s.Situations).FirstOrDefault(sit => sit.Id == situationId);
        if (situation == null)
            return IntentResult.Failed();

        // Actions stored in FLAT GameWorld collections (QUERY-TIME INSTANTIATION)
        // Actions created by SceneFacade when Situation activated (Dormant → Active)
        LocationAction action = _gameWorld.LocationActions.FirstOrDefault(a => a.Id == actionId && a.SituationId == situationId);
        if (action == null)
            return IntentResult.Failed();

        // STEP 1: Validate and extract execution plan
        ActionExecutionPlan plan = _locationActionExecutor.ValidateAndExtract(action, player, _gameWorld);

        if (!plan.IsValid)
        {
            _messageSystem.AddSystemMessage(plan.FailureReason, SystemMessageTypes.Warning);
            return IntentResult.Failed();
        }

        // STEP 2: Apply strategic costs
        if (plan.ResolveCoins > 0)
            player.Resolve -= plan.ResolveCoins;

        if (plan.CoinsCost > 0)
            player.Coins -= plan.CoinsCost;

        // Apply new tutorial resource costs
        if (plan.HealthCost > 0)
            player.Health = Math.Max(0, player.Health - plan.HealthCost);

        if (plan.StaminaCost > 0)
            player.Stamina = Math.Max(0, player.Stamina - plan.StaminaCost);

        if (plan.FocusCost > 0)
            player.Focus = Math.Max(0, player.Focus - plan.FocusCost);

        if (plan.HungerCost > 0)
            player.Hunger = Math.Min(player.MaxHunger, player.Hunger + plan.HungerCost);

        TimeBlocks oldTimeBlock = _timeFacade.GetCurrentTimeBlock();
        if (plan.TimeSegments > 0)
            _timeFacade.AdvanceSegments(plan.TimeSegments);
        TimeBlocks newTimeBlock = _timeFacade.GetCurrentTimeBlock();

        // STEP 3: Route based on ActionType
        if (plan.ActionType == ChoiceActionType.Instant)
        {
            // Apply rewards
            if (plan.ChoiceReward != null)
            {
                _rewardApplicationService.ApplyChoiceReward(plan.ChoiceReward, situation);
            }
            else if (plan.LegacyRewards != null)
            {
                ApplyLegacyRewards(plan.LegacyRewards);
            }

            // CLEANUP: Delete all ephemeral actions for this Situation
            // Actions are query-time instances, not persistent data
            // After execution, remove them so they can be regenerated on next query
            CleanupActionsForSituation(situationId);

            // MULTI-SITUATION SCENE: Complete situation and advance scene
            // Scene.AdvanceToNextSituation() called by completion handler
            // Enables linear progression through multi-situation arcs
            if (situation != null)
            {
                _situationCompletionHandler.CompleteSituation(situation);
            }

            // Process time advancement (HIGHLANDER: single sync point)
            ProcessTimeAdvancement(new TimeAdvancementResult
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
    /// </summary>
    public async Task<IntentResult> ExecuteNPCAction(string situationId, string actionId)
    {
        Player player = _gameWorld.GetPlayer();

        // Verify Situation exists
        Situation situation = _gameWorld.Scenes.SelectMany(s => s.Situations).FirstOrDefault(sit => sit.Id == situationId);
        if (situation == null)
            return IntentResult.Failed();

        // Actions stored in FLAT GameWorld collections (QUERY-TIME INSTANTIATION)
        // Actions created by SceneFacade when Situation activated (Dormant → Active)
        NPCAction action = _gameWorld.NPCActions.FirstOrDefault(a => a.Id == actionId && a.SituationId == situationId);
        if (action == null)
            return IntentResult.Failed();

        // TRIGGER POINT 2: Record NPC interaction when action execution starts
        // Get NPC ID from Situation placement (Scene-embedded situations inherit placement from parent Scene)
        string npcId = situation.GetPlacementId(PlacementType.NPC);
        if (!string.IsNullOrEmpty(npcId))
        {
            RecordNPCInteraction(npcId);
        }

        // STEP 1: Validate and extract execution plan
        ActionExecutionPlan plan = _npcActionExecutor.ValidateAndExtract(action, player, _gameWorld);

        if (!plan.IsValid)
        {
            _messageSystem.AddSystemMessage(plan.FailureReason, SystemMessageTypes.Warning);
            return IntentResult.Failed();
        }

        // STEP 2: Apply strategic costs
        if (plan.ResolveCoins > 0)
            player.Resolve -= plan.ResolveCoins;

        if (plan.CoinsCost > 0)
            player.Coins -= plan.CoinsCost;

        // Apply new tutorial resource costs
        if (plan.HealthCost > 0)
            player.Health = Math.Max(0, player.Health - plan.HealthCost);

        if (plan.StaminaCost > 0)
            player.Stamina = Math.Max(0, player.Stamina - plan.StaminaCost);

        if (plan.FocusCost > 0)
            player.Focus = Math.Max(0, player.Focus - plan.FocusCost);

        if (plan.HungerCost > 0)
            player.Hunger = Math.Min(player.MaxHunger, player.Hunger + plan.HungerCost);

        TimeBlocks oldTimeBlock = _timeFacade.GetCurrentTimeBlock();
        if (plan.TimeSegments > 0)
            _timeFacade.AdvanceSegments(plan.TimeSegments);
        TimeBlocks newTimeBlock = _timeFacade.GetCurrentTimeBlock();

        // STEP 3: Route based on ActionType
        if (plan.ActionType == ChoiceActionType.Instant)
        {
            // Apply rewards
            if (plan.ChoiceReward != null)
            {
                _rewardApplicationService.ApplyChoiceReward(plan.ChoiceReward, situation);
            }
            else if (plan.LegacyRewards != null)
            {
                ApplyLegacyRewards(plan.LegacyRewards);
            }

            // CLEANUP: Delete all ephemeral actions for this Situation
            // Actions are query-time instances, not persistent data
            // After execution, remove them so they can be regenerated on next query
            CleanupActionsForSituation(situationId);

            // MULTI-SITUATION SCENE: Complete situation and advance scene
            // Scene.AdvanceToNextSituation() called by completion handler
            // Enables linear progression through multi-situation arcs
            if (situation != null)
            {
                _situationCompletionHandler.CompleteSituation(situation);
            }

            // Process time advancement (HIGHLANDER: single sync point)
            ProcessTimeAdvancement(new TimeAdvancementResult
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
    /// </summary>
    public async Task<IntentResult> ExecutePathCard(string situationId, string cardId)
    {
        Player player = _gameWorld.GetPlayer();

        // Verify Situation exists
        Situation situation = _gameWorld.Scenes.SelectMany(s => s.Situations).FirstOrDefault(sit => sit.Id == situationId);
        if (situation == null)
            return IntentResult.Failed();

        // Actions stored in FLAT GameWorld collections (QUERY-TIME INSTANTIATION)
        // PathCards created by SceneFacade when Situation activated (Dormant → Active)
        PathCard card = _gameWorld.PathCards.FirstOrDefault(c => c.Id == cardId && c.SituationId == situationId);
        if (card == null)
            return IntentResult.Failed();

        // STEP 1: Validate and extract execution plan
        ActionExecutionPlan plan = _pathCardExecutor.ValidateAndExtract(card, player, _gameWorld);

        if (!plan.IsValid)
        {
            _messageSystem.AddSystemMessage(plan.FailureReason, SystemMessageTypes.Warning);
            return IntentResult.Failed();
        }

        // STEP 2: Apply strategic costs
        if (plan.CoinsCost > 0)
            player.Coins -= plan.CoinsCost;

        TimeBlocks oldTimeBlock = _timeFacade.GetCurrentTimeBlock();
        if (plan.TimeSegments > 0)
            _timeFacade.AdvanceSegments(plan.TimeSegments);
        TimeBlocks newTimeBlock = _timeFacade.GetCurrentTimeBlock();

        // STEP 3: Route based on ActionType
        if (plan.ActionType == ChoiceActionType.Instant)
        {
            // Apply rewards
            if (plan.ChoiceReward != null)
            {
                _rewardApplicationService.ApplyChoiceReward(plan.ChoiceReward, situation);
            }
            else if (plan.IsLegacyAction)
            {
                // Apply legacy PathCard rewards (StaminaRestore, HealthEffect, CoinReward, etc.)
                ApplyLegacyPathCardRewards(card);
            }

            // CLEANUP: Delete all ephemeral actions for this Situation
            // Actions are query-time instances, not persistent data
            // After execution, remove them so they can be regenerated on next query
            CleanupActionsForSituation(situationId);

            // MULTI-SITUATION SCENE: Complete situation and advance scene
            // Scene.AdvanceToNextSituation() called by completion handler
            // Enables linear progression through multi-situation arcs
            if (situation != null)
            {
                _situationCompletionHandler.CompleteSituation(situation);
            }

            // Process time advancement (HIGHLANDER: single sync point)
            ProcessTimeAdvancement(new TimeAdvancementResult
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

    /// <summary>
    /// Record location visit in player interaction history
    /// Update-in-place pattern: Find existing record or create new
    /// ONE record per location (replaces previous timestamp)
    /// </summary>
    private void RecordLocationVisit(string locationId)
    {
        Player player = _gameWorld.GetPlayer();

        // Find existing record
        LocationVisitRecord existingRecord = player.LocationVisits
            .FirstOrDefault(record => record.LocationId == locationId);

        if (existingRecord != null)
        {
            // Update existing record with current timestamp
            existingRecord.LastVisitDay = _timeFacade.GetCurrentDay();
            existingRecord.LastVisitTimeBlock = _timeFacade.GetCurrentTimeBlock();
            existingRecord.LastVisitSegment = _timeFacade.GetCurrentSegment();
        }
        else
        {
            // Create new record
            player.LocationVisits.Add(new LocationVisitRecord
            {
                LocationId = locationId,
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
    /// </summary>
    private void RecordNPCInteraction(string npcId)
    {
        Player player = _gameWorld.GetPlayer();

        // Find existing record
        NPCInteractionRecord existingRecord = player.NPCInteractions
            .FirstOrDefault(record => record.NPCId == npcId);

        if (existingRecord != null)
        {
            // Update existing record with current timestamp
            existingRecord.LastInteractionDay = _timeFacade.GetCurrentDay();
            existingRecord.LastInteractionTimeBlock = _timeFacade.GetCurrentTimeBlock();
            existingRecord.LastInteractionSegment = _timeFacade.GetCurrentSegment();
        }
        else
        {
            // Create new record
            player.NPCInteractions.Add(new NPCInteractionRecord
            {
                NPCId = npcId,
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
    private void RecordRouteTraversal(string routeId)
    {
        Player player = _gameWorld.GetPlayer();

        // Find existing record
        RouteTraversalRecord existingRecord = player.RouteTraversals
            .FirstOrDefault(record => record.RouteId == routeId);

        if (existingRecord != null)
        {
            // Update existing record with current timestamp
            existingRecord.LastTraversalDay = _timeFacade.GetCurrentDay();
            existingRecord.LastTraversalTimeBlock = _timeFacade.GetCurrentTimeBlock();
            existingRecord.LastTraversalSegment = _timeFacade.GetCurrentSegment();
        }
        else
        {
            // Create new record
            player.RouteTraversals.Add(new RouteTraversalRecord
            {
                RouteId = routeId,
                LastTraversalDay = _timeFacade.GetCurrentDay(),
                LastTraversalTimeBlock = _timeFacade.GetCurrentTimeBlock(),
                LastTraversalSegment = _timeFacade.GetCurrentSegment()
            });
        }
    }

    /// <summary>
    /// Cleanup ephemeral actions for a Situation after execution
    /// Actions are query-time instances created by SceneFacade, NOT persistent data
    /// After action execution, delete all actions so they can be regenerated on next query
    /// This is CRITICAL for three-tier timing model (Query-time instantiation)
    /// </summary>
    private void CleanupActionsForSituation(string situationId)
    {
        // Remove all ephemeral LocationActions for this Situation
        _gameWorld.LocationActions.RemoveAll(a => a.SituationId == situationId);

        // Remove all ephemeral NPCActions for this Situation
        _gameWorld.NPCActions.RemoveAll(a => a.SituationId == situationId);

        // Remove all ephemeral PathCards for this Situation
        _gameWorld.PathCards.RemoveAll(pc => pc.SituationId == situationId);

        // STATE MACHINE: Reset Situation to Dormant for re-entry
        // Next time player enters context, SceneFacade will recreate actions fresh
        Situation situation = _gameWorld.Scenes.SelectMany(s => s.Situations).FirstOrDefault(sit => sit.Id == situationId);
        if (situation != null)
        {
            situation.InstantiationState = InstantiationState.Deferred;
        }
    }

    private void ApplyLegacyRewards(ActionRewards rewards)
    {
        Player player = _gameWorld.GetPlayer();

        if (rewards.FullRecovery)
        {
            player.Health = player.MaxHealth;
            player.Focus = player.MaxFocus;
            player.Stamina = player.MaxStamina;
            return;
        }

        if (rewards.CoinReward > 0)
            player.Coins += rewards.CoinReward;

        if (rewards.HealthRecovery > 0)
            player.Health = Math.Min(player.MaxHealth, player.Health + rewards.HealthRecovery);

        if (rewards.FocusRecovery > 0)
            player.Focus = Math.Min(player.MaxFocus, player.Focus + rewards.FocusRecovery);

        if (rewards.StaminaRecovery > 0)
            player.Stamina = Math.Min(player.MaxStamina, player.Stamina + rewards.StaminaRecovery);
    }

    private void ApplyLegacyPathCardRewards(PathCard card)
    {
        Player player = _gameWorld.GetPlayer();

        if (card.StaminaRestore > 0)
            player.Stamina = Math.Min(player.MaxStamina, player.Stamina + card.StaminaRestore);

        if (card.HealthEffect != 0)
        {
            if (card.HealthEffect > 0)
                player.Health = Math.Min(player.MaxHealth, player.Health + card.HealthEffect);
            else
                player.Health = Math.Max(0, player.Health + card.HealthEffect); // Negative effect
        }

        if (card.CoinReward > 0)
            player.Coins += card.CoinReward;

        // TokenGains, RevealsPaths, etc. would be handled by PathCard-specific logic
    }


    private IntentResult RouteToTacticalChallenge(ActionExecutionPlan plan)
    {
        // Store CompletionReward in appropriate PendingContext
        // Reward will be applied when challenge completes successfully
        if (plan.ChallengeType == TacticalSystemType.Social)
        {
            // Store Social context with CompletionReward
            _gameWorld.PendingSocialContext = new SocialChallengeContext
            {
                CompletionReward = plan.ChoiceReward
            };
            // Navigate to social challenge screen
            return IntentResult.NavigateScreen(ScreenMode.SocialChallenge);
        }
        else if (plan.ChallengeType == TacticalSystemType.Mental)
        {
            // Store Mental context with CompletionReward
            _gameWorld.PendingMentalContext = new MentalChallengeContext
            {
                CompletionReward = plan.ChoiceReward
            };
            // Navigate to mental challenge screen
            return IntentResult.NavigateScreen(ScreenMode.MentalChallenge);
        }
        else if (plan.ChallengeType == TacticalSystemType.Physical)
        {
            // Store Physical context with CompletionReward
            _gameWorld.PendingPhysicalContext = new PhysicalChallengeContext
            {
                CompletionReward = plan.ChoiceReward
            };
            // Navigate to physical challenge screen
            return IntentResult.NavigateScreen(ScreenMode.PhysicalChallenge);
        }

        return IntentResult.Failed();
    }

    /// <summary>
    /// Process social challenge outcome - apply CompletionReward if successful, FailureReward if failed
    /// STRATEGIC LAYER: GameFacade applies rewards after receiving tactical outcome
    /// </summary>
    public void ProcessSocialChallengeOutcome()
    {
        if (_gameWorld.LastSocialOutcome?.Success == true &&
            _gameWorld.PendingSocialContext?.CompletionReward != null)
        {
            Situation currentSituation = _gameWorld.Scenes.SelectMany(s => s.Situations)
                .FirstOrDefault(sit => sit.Id == _gameWorld.CurrentSocialSession?.RequestId);
            _rewardApplicationService.ApplyChoiceReward(
                _gameWorld.PendingSocialContext.CompletionReward,
                currentSituation);
        }
        else if (_gameWorld.LastSocialOutcome?.Success == false &&
                 _gameWorld.PendingSocialContext?.FailureReward != null)
        {
            Situation currentSituation = _gameWorld.Scenes.SelectMany(s => s.Situations)
                .FirstOrDefault(sit => sit.Id == _gameWorld.PendingSocialContext.SituationId);
            _rewardApplicationService.ApplyChoiceReward(
                _gameWorld.PendingSocialContext.FailureReward,
                currentSituation);
        }
        _gameWorld.PendingSocialContext = null;
    }

    /// <summary>
    /// Process mental challenge outcome - apply CompletionReward if successful, FailureReward if failed
    /// STRATEGIC LAYER: GameFacade applies rewards after receiving tactical outcome
    /// </summary>
    public void ProcessMentalChallengeOutcome()
    {
        if (_gameWorld.LastMentalOutcome?.Success == true &&
            _gameWorld.PendingMentalContext?.CompletionReward != null)
        {
            Situation currentSituation = _gameWorld.Scenes.SelectMany(s => s.Situations)
                .FirstOrDefault(sit => sit.Id == _gameWorld.CurrentMentalSituationId);
            _rewardApplicationService.ApplyChoiceReward(
                _gameWorld.PendingMentalContext.CompletionReward,
                currentSituation);
        }
        else if (_gameWorld.LastMentalOutcome?.Success == false &&
                 _gameWorld.PendingMentalContext?.FailureReward != null)
        {
            Situation currentSituation = _gameWorld.Scenes.SelectMany(s => s.Situations)
                .FirstOrDefault(sit => sit.Id == _gameWorld.PendingMentalContext.SituationId);
            _rewardApplicationService.ApplyChoiceReward(
                _gameWorld.PendingMentalContext.FailureReward,
                currentSituation);
        }
        _gameWorld.PendingMentalContext = null;
    }

    /// <summary>
    /// Process physical challenge outcome - apply CompletionReward if successful, FailureReward if failed
    /// STRATEGIC LAYER: GameFacade applies rewards after receiving tactical outcome
    /// </summary>
    public void ProcessPhysicalChallengeOutcome()
    {
        if (_gameWorld.LastPhysicalOutcome?.Success == true &&
            _gameWorld.PendingPhysicalContext?.CompletionReward != null)
        {
            Situation currentSituation = _gameWorld.Scenes.SelectMany(s => s.Situations)
                .FirstOrDefault(sit => sit.Id == _gameWorld.CurrentPhysicalSituationId);
            _rewardApplicationService.ApplyChoiceReward(
                _gameWorld.PendingPhysicalContext.CompletionReward,
                currentSituation);
        }
        else if (_gameWorld.LastPhysicalOutcome?.Success == false &&
                 _gameWorld.PendingPhysicalContext?.FailureReward != null)
        {
            Situation currentSituation = _gameWorld.Scenes.SelectMany(s => s.Situations)
                .FirstOrDefault(sit => sit.Id == _gameWorld.PendingPhysicalContext.SituationId);
            _rewardApplicationService.ApplyChoiceReward(
                _gameWorld.PendingPhysicalContext.FailureReward,
                currentSituation);
        }
        _gameWorld.PendingPhysicalContext = null;
    }

    private IntentResult ApplyNavigationPayload(NavigationPayload payload)
    {
        // Apply navigation based on payload type
        // NavigationPayload structure from ChoiceReward - contains target location/route
        // For now, return to location screen (navigation logic to be implemented)
        return IntentResult.Executed(requiresRefresh: true);
    }

}
