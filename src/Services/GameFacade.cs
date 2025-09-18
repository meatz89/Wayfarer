using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wayfarer.Subsystems.ExchangeSubsystem;
using Wayfarer.Subsystems.LocationSubsystem;
using Wayfarer.Subsystems.NarrativeSubsystem;
using Wayfarer.Subsystems.ObligationSubsystem;
using Wayfarer.Subsystems.ResourceSubsystem;
using Wayfarer.Subsystems.TimeSubsystem;
using Wayfarer.Subsystems.TokenSubsystem;
using Wayfarer.Subsystems.TravelSubsystem;

/// <summary>
/// GameFacade - Pure orchestrator for UI-Backend communication.
/// Delegates ALL business logic to specialized facades.
/// Coordinates cross-facade operations and handles UI-specific orchestration.
/// </summary>
public class GameFacade
{
    private readonly GameWorld _gameWorld;
    private readonly MessageSystem _messageSystem;
    private readonly ConversationFacade _conversationFacade;
    private readonly LocationFacade _locationFacade;
    private readonly ObligationFacade _obligationFacade;
    private readonly ResourceFacade _resourceFacade;
    private readonly TimeFacade _timeFacade;
    private readonly TravelFacade _travelFacade;
    private readonly TokenFacade _tokenFacade;
    private readonly NarrativeFacade _narrativeFacade;
    private readonly Wayfarer.Subsystems.ExchangeSubsystem.ExchangeFacade _exchangeFacade;

    public GameFacade(
        GameWorld gameWorld,
        MessageSystem messageSystem,
        ConversationFacade conversationFacade,
        LocationFacade locationFacade,
        ObligationFacade obligationFacade,
        ResourceFacade resourceFacade,
        TimeFacade timeFacade,
        TravelFacade travelFacade,
        TokenFacade tokenFacade,
        NarrativeFacade narrativeFacade,
        Wayfarer.Subsystems.ExchangeSubsystem.ExchangeFacade exchangeFacade)
    {
        _gameWorld = gameWorld;
        _messageSystem = messageSystem;
        _conversationFacade = conversationFacade;
        _locationFacade = locationFacade;
        _obligationFacade = obligationFacade;
        _resourceFacade = resourceFacade;
        _timeFacade = timeFacade;
        _travelFacade = travelFacade;
        _tokenFacade = tokenFacade;
        _narrativeFacade = narrativeFacade;
        _exchangeFacade = exchangeFacade;
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

    public AttentionStateInfo GetCurrentAttentionState()
    {
        TimeBlocks timeBlock = _timeFacade.GetCurrentTimeBlock();
        AttentionInfo attention = _resourceFacade.GetAttention(timeBlock);
        return new AttentionStateInfo(attention.Current, attention.Max, timeBlock);
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

    public ConversationContext StartStrangerConversation(string strangerId)
    {
        // Find the stranger
        NPC stranger = _gameWorld.GetStrangerById(strangerId);

        if (stranger == null || !stranger.HasAvailableRequests())
        {
            return null;
        }

        // Get the first available request (strangers have single request)
        NPCRequest request = stranger.GetAvailableRequests().FirstOrDefault();
        if (request == null || string.IsNullOrEmpty(request.ConversationTypeId))
        {
            return null;
        }

        // Get conversation type
        if (!_gameWorld.ConversationTypes.TryGetValue(request.ConversationTypeId, out ConversationTypeDefinition? conversationType))
        {
            return null;
        }

        // Mark stranger as encountered
        stranger.MarkAsEncountered();

        // Create conversation context
        ConversationContext context = new ConversationContext
        {
            IsValid = true,
            Npc = stranger,
            NpcId = stranger.ID,
            ConversationTypeId = conversationType.Id,
            RequestId = request.Id,
            RequestText = request.Description,
            InitialState = ConnectionState.DISCONNECTED, // Strangers always start disconnected
            AttentionSpent = conversationType.AttentionCost
        };

        return context;
    }

    public bool CanAffordStrangerConversation(string requestId)
    {
        // Find stranger with this request
        foreach (NPC stranger in _gameWorld.GetAllStrangers())
        {
            NPCRequest request = stranger.GetRequestById(requestId);
            if (request != null && !string.IsNullOrEmpty(request.ConversationTypeId))
            {
                if (_gameWorld.ConversationTypes.TryGetValue(request.ConversationTypeId, out ConversationTypeDefinition? conversationType))
                {
                    AttentionStateInfo attentionState = GetCurrentAttentionState();
                    return attentionState.Current >= conversationType.AttentionCost;
                }
            }
        }
        return false;
    }

    public int GetStrangerConversationAttentionCost(string requestId)
    {
        // Find stranger with this request
        foreach (NPC stranger in _gameWorld.GetAllStrangers())
        {
            NPCRequest request = stranger.GetRequestById(requestId);
            if (request != null && !string.IsNullOrEmpty(request.ConversationTypeId))
            {
                if (_gameWorld.ConversationTypes.TryGetValue(request.ConversationTypeId, out ConversationTypeDefinition? conversationType))
                {
                    return conversationType.AttentionCost;
                }
            }
        }
        return 1; // Default cost
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
        return _locationFacade.MoveToSpot(spotName);
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
            List<ConversationOption> conversationOptions = _conversationFacade.GetAvailableConversationOptions(npc);
            List<string> conversationTypeIds = conversationOptions.Select(opt => opt.ConversationTypeId).Distinct().ToList();

            // Get attention cost for default conversation type (friendly_chat)
            int attentionCost = _conversationFacade.GetAttentionCost("friendly_chat");
            AttentionInfo attentionInfo = _resourceFacade.GetAttention(_timeFacade.GetCurrentTimeBlock());
            int currentAttention = attentionInfo.Current;

            options.Add(new NPCConversationOptions
            {
                NpcId = npc.ID,
                NpcName = npc.Description,
                AvailableTypes = conversationTypeIds,
                AttentionCost = attentionCost,
                CanAfford = currentAttention >= attentionCost
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
        int itemCount = player.Inventory.ItemSlots.Count(i => !string.IsNullOrEmpty(i));
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

    // ========== OBLIGATION OPERATIONS ==========

    public LetterQueueViewModel GetLetterQueue()
    {
        return _obligationFacade.GetLetterQueue();
    }

    public QueueDisplacementPreview GetDisplacementPreview(string obligationId, int targetPosition)
    {
        return _obligationFacade.GetDisplacementPreview(obligationId, targetPosition);
    }

    public async Task<bool> DisplaceObligation(string obligationId, int targetPosition)
    {
        return _obligationFacade.DisplaceObligation(obligationId, targetPosition);
    }

    public int GetLetterQueueCount()
    {
        return _obligationFacade.GetLetterQueueCount();
    }

    public ObligationFacade GetObligationQueueManager()
    {
        return _obligationFacade;
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

    // ========== TOKEN OPERATIONS ==========

    public NPCTokenBalance GetTokensWithNPC(string npcId)
    {
        Dictionary<ConnectionType, int> tokens = _tokenFacade.GetTokensWithNPC(npcId);
        return new NPCTokenBalance
        {
            Balances = tokens.Select(kvp => new TokenBalance
            {
                TokenType = kvp.Key,
                Amount = kvp.Value
            }).ToList()
        };
    }


    // ========== CONVERSATION OPERATIONS ==========

    public ConversationFacade GetConversationFacade()
    {
        return _conversationFacade;
    }

    public async Task<ConversationContextBase> CreateConversationContext(string npcId, string requestId)
    {
        return await _conversationFacade.CreateConversationContext(npcId, requestId);
    }

    public List<ConversationOption> GetAvailableConversationOptions(string npcId)
    {
        NPC? npc = _gameWorld.NPCs.FirstOrDefault(n => n.ID == npcId);
        if (npc == null)
            return new List<ConversationOption>();

        return _conversationFacade.GetAvailableConversationOptions(npc);
    }

    /// <summary>
    /// Play a conversation card - proper architectural flow through GameFacade
    /// </summary>
    public async Task<ConversationTurnResult> PlayConversationCard(CardInstance card)
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
    public async Task<ConversationTurnResult> ExecuteListen()
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
    public bool CanPlayCard(CardInstance card, ConversationSession session)
    {
        if (_conversationFacade == null) return false;
        return _conversationFacade.CanPlayCard(card, session);
    }

    /// <summary>
    /// End the current conversation and return the outcome
    /// </summary>
    public ConversationOutcome EndConversation()
    {
        if (_conversationFacade == null) return null;
        return _conversationFacade.EndConversation();
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

        // Get current time block and attention
        TimeBlocks timeBlock = _timeFacade.GetCurrentTimeBlock();
        AttentionInfo attentionInfo = _resourceFacade.GetAttention(timeBlock);

        // Get player resources and tokens
        PlayerResourceState playerResources = _gameWorld.GetPlayerResourceState();

        // Get player's tokens with this specific NPC
        Dictionary<ConnectionType, int> npcTokens = _tokenFacade.GetTokensWithNPC(npcId);

        // Get relationship tier with this NPC
        RelationshipTier relationshipTier = _tokenFacade.GetRelationshipTier(npcId);

        // Get available exchanges from ExchangeFacade - now orchestrating properly
        List<ExchangeOption> availableExchanges = _exchangeFacade.GetAvailableExchanges(npcId, playerResources, attentionInfo, npcTokens, relationshipTier);

        if (!availableExchanges.Any())
        {
            _messageSystem.AddSystemMessage($"{npc.Name} has no exchanges available", SystemMessageTypes.Info);
            return null;
        }

        // Convert ExchangeOptions to ExchangeCards
        List<ExchangeCard> exchangeCards = availableExchanges.Select(option => ConvertToExchangeCard(option)).ToList();

        // Create exchange session through ExchangeFacade
        Wayfarer.Subsystems.ExchangeSubsystem.ExchangeSession session = _exchangeFacade.CreateExchangeSession(npcId);
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
                Portrait = "",  // NPCs don't have portraits stored
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
            CurrentAttention = attentionInfo.Current,
            PlayerTokens = npcTokens,
            Session = new ExchangeSession
            {
                NpcId = npcId,
                LocationId = currentSpot?.SpotID ?? "",
                AvailableExchanges = exchangeCards
            }
        };

        return await Task.FromResult(context);
    }

    // ========== NARRATIVE OPERATIONS ==========

    public async Task<bool> TakeObservationAsync(string observationId)
    {
        Location currentLocation = _locationFacade.GetCurrentLocation();
        LocationSpot currentSpot = _locationFacade.GetCurrentLocationSpot();

        // Check if this is an old-style observation from JSON
        Observation? observation = _narrativeFacade.GetLocationObservations(
            currentLocation?.Id,
            currentSpot?.SpotID)
            .FirstOrDefault(o => o.Id == observationId);

        if (observation != null && _resourceFacade.SpendAttention(observation.AttentionCost))
        {
            return _narrativeFacade.TakeObservation(observationId);
        }

        // Check if this is a new-style observation reward
        List<ObservationReward> availableRewards = _narrativeFacade.GetAvailableObservationRewards(currentLocation?.Id);
        ObservationReward? reward = availableRewards.FirstOrDefault(r => r.ObservationCard.Id == observationId);

        if (reward != null)
        {
            // New system: costs 0 attention, goes to NPC observation deck
            return _narrativeFacade.CompleteObservationReward(currentLocation?.Id, reward);
        }

        return false;
    }

    public List<ObservationReward> GetAvailableObservationRewards()
    {
        Location currentLocation = _locationFacade.GetCurrentLocation();
        if (currentLocation == null) return new List<ObservationReward>();

        return _narrativeFacade.GetAvailableObservationRewards(currentLocation.Id);
    }

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

        // Initialize player at starting location
        Player player = _gameWorld.GetPlayer();
        Location? startingLocation = _gameWorld.WorldState.locations.FirstOrDefault(l => l.Id == "market_square");
        if (startingLocation != null)
        {
            LocationSpot? startingSpot = _gameWorld.Spots.Values.FirstOrDefault(s => s.LocationId == "market_square" && s.SpotID == "central_fountain");
            if (startingSpot != null)
            {
                player.CurrentLocationSpot = startingSpot;
                Console.WriteLine($"[GameFacade.StartGameAsync] Player initialized at {startingLocation.Name} - {startingSpot.Name}");
            }
        }

        // Initialize player resources for testing
        player.Coins = 50;  // Starting coins for testing
        player.Health = 10; // Starting health for testing
        player.Hunger = 5;   // Starting food for testing

        Console.WriteLine($"[GameFacade.StartGameAsync] Player resources initialized - Coins: {player.Coins}, Health: {player.Health}, Hunger: {player.Hunger}");

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
            DeliverLetterIntent deliver => _obligationFacade.DeliverObligation(deliver.LetterId).Success,
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

    public DailyActivityResult GetDailyActivities()
    {
        return new DailyActivityResult();
    }

    public List<RouteOption> GetRoutesToDestination(string destinationId)
    {
        return new List<RouteOption>();
    }

    public void AddLetterWithObligationEffects(object letterData)
    {
        if (letterData is DeliveryObligation obligation)
        {
            _obligationFacade.AddLetterWithObligationEffects(obligation);
        }
        else
        {
            Console.WriteLine($"Warning: AddLetterWithObligationEffects called with invalid data type: {letterData?.GetType()?.Name ?? "null"}");
        }
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
            _obligationFacade.ProcessSegmentDeadlines(result.SegmentsAdvanced);
            _narrativeFacade.RefreshObservationsForNewTimeBlock();
        }
        else if (result.SegmentsAdvanced > 0)
        {
            _obligationFacade.ProcessSegmentDeadlines(result.SegmentsAdvanced);
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

    /// <summary>
    /// Converts an ExchangeOption from the ExchangeFacade to an ExchangeCard for the UI
    /// </summary>
    private ExchangeCard ConvertToExchangeCard(ExchangeOption option)
    {
        ExchangeData? exchangeData = option.ExchangeData;

        // Build token requirements from ExchangeData
        Dictionary<ConnectionType, int> tokenRequirements = new Dictionary<ConnectionType, int>();
        if (exchangeData?.RequiredTokenType != null && exchangeData.MinimumTokensRequired > 0)
        {
            tokenRequirements[exchangeData.RequiredTokenType.Value] = exchangeData.MinimumTokensRequired;
        }

        return new ExchangeCard
        {
            Id = option.ExchangeId,
            Name = option.Name,
            Description = option.Description,
            ExchangeType = ExchangeType.Purchase,  // Default type since ExchangeData doesn't have this
            NpcId = "",  // Will be set by the calling context
            Cost = new ExchangeCostStructure
            {
                Resources = exchangeData?.Costs ?? new List<ResourceAmount>(),
                TokenRequirements = tokenRequirements,
                RequiredItemIds = exchangeData?.RequiredItems ?? new List<string>()
            },
            Reward = new ExchangeRewardStructure
            {
                Resources = exchangeData?.Rewards ?? new List<ResourceAmount>(),
                ItemIds = new List<string>(),  // ExchangeData doesn't have RewardItems
                Tokens = new Dictionary<ConnectionType, int>()  // ExchangeData doesn't have TokenRewards
            },
            SingleUse = exchangeData?.IsUnique ?? false,
            IsCompleted = false,
            SuccessRate = 100,  // ExchangeData doesn't have SuccessRate
            FailurePenalty = null,  // ExchangeData doesn't have FailurePenalty
            RequiredLocationId = null,  // ExchangeData doesn't have RequiredLocation
            AvailableTimeBlocks = exchangeData?.TimeRestrictions ?? new List<TimeBlocks>()
        };
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

    // ========== EXCHANGE ORCHESTRATION ==========

    /// <summary>
    /// Execute an exchange using proper facade orchestration
    /// </summary>
    public async Task<ExchangeResult> ExecuteExchange(string npcId, string exchangeId)
    {
        // Collect all data needed for exchange
        PlayerResourceState playerResources = _gameWorld.GetPlayerResourceState();
        TimeBlocks timeBlock = _timeFacade.GetCurrentTimeBlock();
        AttentionInfo attentionInfo = _resourceFacade.GetAttention(timeBlock);
        Dictionary<ConnectionType, int> npcTokens = _tokenFacade.GetTokensWithNPC(npcId);
        RelationshipTier relationshipTier = _tokenFacade.GetRelationshipTier(npcId);

        // Execute exchange through facade (this returns operation data)
        ExchangeResult result = await _exchangeFacade.ExecuteExchange(npcId, exchangeId, playerResources, attentionInfo, npcTokens, relationshipTier);

        if (!result.Success || result.OperationData == null)
        {
            return result;
        }

        // Now orchestrate the actual execution using the operation data
        ExchangeOperationData operation = result.OperationData;

        // Apply costs through appropriate facades
        foreach (ResourceAmount cost in operation.Costs)
        {
            switch (cost.Type)
            {
                case ResourceType.Coins:
                    if (!_resourceFacade.SpendCoins(cost.Amount, $"Exchange with {npcId}"))
                    {
                        result.Success = false;
                        result.Message = "Failed to spend coins";
                        return result;
                    }
                    break;

                case ResourceType.Health:
                    _resourceFacade.TakeDamage(cost.Amount, $"Exchange with {npcId}");
                    break;

                case ResourceType.Attention:
                    if (!_resourceFacade.SpendAttention(cost.Amount, timeBlock, $"Exchange with {npcId}"))
                    {
                        result.Success = false;
                        result.Message = "Failed to spend attention";
                        return result;
                    }
                    break;

                case ResourceType.TrustToken:
                case ResourceType.CommerceToken:
                case ResourceType.StatusToken:
                case ResourceType.ShadowToken:
                    ConnectionType tokenType = cost.Type switch
                    {
                        ResourceType.TrustToken => ConnectionType.Trust,
                        ResourceType.CommerceToken => ConnectionType.Commerce,
                        ResourceType.StatusToken => ConnectionType.Status,
                        ResourceType.ShadowToken => ConnectionType.Shadow,
                        _ => ConnectionType.Trust
                    };
                    if (!_tokenFacade.SpendTokensWithNPC(tokenType, cost.Amount, npcId))
                    {
                        result.Success = false;
                        result.Message = $"Failed to spend {tokenType} tokens";
                        return result;
                    }
                    break;

                case ResourceType.Item:
                    if (!string.IsNullOrEmpty(cost.ItemId))
                    {
                        if (!_resourceFacade.RemoveItem(cost.ItemId))
                        {
                            result.Success = false;
                            result.Message = $"Failed to remove item {cost.ItemId}";
                            return result;
                        }
                    }
                    break;
            }
        }

        // Apply rewards through appropriate facades
        foreach (ResourceAmount reward in operation.Rewards)
        {
            switch (reward.Type)
            {
                case ResourceType.Coins:
                    _resourceFacade.AddCoins(reward.Amount, $"Exchange with {npcId}");
                    break;

                case ResourceType.Health:
                    _resourceFacade.Heal(reward.Amount, $"Exchange with {npcId}");
                    break;

                case ResourceType.Hunger:
                    _resourceFacade.DecreaseHunger(reward.Amount, $"Exchange with {npcId}");
                    break;

                case ResourceType.TrustToken:
                case ResourceType.CommerceToken:
                case ResourceType.StatusToken:
                case ResourceType.ShadowToken:
                    ConnectionType tokenType = reward.Type switch
                    {
                        ResourceType.TrustToken => ConnectionType.Trust,
                        ResourceType.CommerceToken => ConnectionType.Commerce,
                        ResourceType.StatusToken => ConnectionType.Status,
                        ResourceType.ShadowToken => ConnectionType.Shadow,
                        _ => ConnectionType.Trust
                    };
                    _tokenFacade.AddTokensToNPC(tokenType, reward.Amount, npcId);
                    break;
            }
        }

        // Apply item rewards
        foreach (string itemId in operation.ItemRewards)
        {
            _resourceFacade.AddItem(itemId);
        }

        // Handle side effects
        if (operation.AdvancesTime)
        {
            _timeFacade.AdvanceSegments(operation.TimeAdvancementHours);
            _messageSystem.AddSystemMessage($"Time passes... ({operation.TimeAdvancementHours} segment(s))", SystemMessageTypes.Info);
        }

        if (operation.AffectsRelationship)
        {
            NPC? npc = _gameWorld.NPCs.FirstOrDefault(n => n.ID == npcId);
            if (npc != null)
            {
                npc.RelationshipFlow += operation.FlowModifier;
                _messageSystem.AddSystemMessage($"Relationship with {npc.Name} {(operation.FlowModifier > 0 ? "improved" : "worsened")}", SystemMessageTypes.Info);
            }
        }

        if (operation.ConsumesPatience)
        {
            NPC? npc = _gameWorld.NPCs.FirstOrDefault(n => n.ID == npcId);
            if (npc != null)
            {
                npc.SpendPatience(operation.PatienceCost);
            }
        }

        // Mark exchange as used if unique
        if (operation.IsUnique)
        {
            _exchangeFacade.RemoveExchangeFromNPC(operation.NPCId, operation.ExchangeId);
        }

        // Update result with what was actually applied
        result.Success = true;
        result.Message = $"Exchange completed successfully";
        _messageSystem.AddSystemMessage(result.Message, SystemMessageTypes.Success);

        return result;
    }

}