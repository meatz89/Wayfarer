using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        InvestigationDiscoveryEvaluator investigationDiscoveryEvaluator)
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

    public List<NPC> GetAvailableStrangers(string venueId)
    {
        TimeBlocks currentTime = _timeFacade.GetCurrentTimeBlock();
        return _gameWorld.GetAvailableStrangers(venueId, currentTime);
    }

    public List<InvestigationApproach> GetAvailableInvestigationApproaches()
    {
        Player player = _gameWorld.GetPlayer();
        return _locationFacade.GetAvailableApproaches(player);
    }

    // ========== Venue OPERATIONS ==========

    public Venue GetCurrentLocation()
    {
        return _locationFacade.GetCurrentLocation();
    }

    public Location GetLocationSpot(string LocationId)
    {
        return _gameWorld.GetLocation(LocationId);
    }

    public Location GetCurrentLocationSpot()
    {
        return _locationFacade.GetCurrentLocationSpot();
    }

    public Venue GetLocationById(string venueId)
    {
        return _locationFacade.GetLocationById(venueId);
    }

    public bool MoveToSpot(string spotName)
    {
        bool success = _locationFacade.MoveToSpot(spotName);

        // Movement to new Venue may unlock investigation discovery (ImmediateVisibility, EnvironmentalObservation triggers)
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
        // NPCs no longer have inline conversation options - goals are location-based in GameWorld.Goals
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
                Location? destSpot = _gameWorld.GetLocation(actualRoute.DestinationLocationSpot);

                if (destSpot != null)
                {
                    player.CurrentLocation = destSpot;
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
            Location? finalDestSpot = _gameWorld.GetLocation(targetRoute.DestinationLocationSpot);

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
    public MentalSession StartMentalSession(string deckId, string locationSpotId, string goalId, string investigationId)
    {
        if (_mentalFacade.IsSessionActive())
            throw new InvalidOperationException("Mental session already active");

        MentalChallengeDeck challengeDeck = _gameWorld.MentalChallengeDecks.FirstOrDefault(d => d.Id == deckId);
        if (challengeDeck == null)
            throw new InvalidOperationException($"MentalChallengeDeck {deckId} not found");

        Player player = _gameWorld.GetPlayer();

        // Build deck with signature deck knowledge cards in starting hand
        (List<CardInstance> deck, List<CardInstance> startingHand) = _mentalFacade.GetDeckBuilder()
            .BuildDeckWithStartingHand(challengeDeck, player);

        return _mentalFacade.StartSession(challengeDeck, deck, startingHand, goalId, investigationId);
    }

    /// <summary>
    /// Execute observe action in current Mental investigation
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
    public PhysicalSession StartPhysicalSession(string deckId, string locationSpotId, string goalId, string investigationId)
    {
        if (_physicalFacade.IsSessionActive())
            throw new InvalidOperationException("Physical session already active");

        PhysicalChallengeDeck challengeDeck = _gameWorld.PhysicalChallengeDecks.FirstOrDefault(d => d.Id == deckId);
        if (challengeDeck == null)
            throw new InvalidOperationException($"PhysicalChallengeDeck {deckId} not found");

        Player player = _gameWorld.GetPlayer();

        // Build deck with signature deck knowledge cards in starting hand
        (List<CardInstance> deck, List<CardInstance> startingHand) = _physicalFacade.GetDeckBuilder()
            .BuildDeckWithStartingHand(challengeDeck, player);

        return _physicalFacade.StartSession(challengeDeck, deck, startingHand, goalId, investigationId);
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

        // Get current location
        Venue? currentLocation = GetCurrentLocation();
        Location? currentSpot = GetCurrentLocationSpot();

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
                VenueId = currentSpot.Id,
                Name = currentLocation.Name,
                Description = currentLocation.Description
            },
            CurrentTimeBlock = timeBlock,
            PlayerResources = playerResources,
            PlayerTokens = npcTokens,
            PlayerInventory = GetPlayerInventoryAsDictionary(),
            Session = new ExchangeSession
            {
                NpcId = npcId,
                VenueId = currentSpot.Id,
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
        Player player = _gameWorld.GetPlayer();
        string startingSpotId = _gameWorld.InitialLocationSpotId;
        Location? startingSpot = _gameWorld.Locations.FirstOrDefault(s => s.Id == startingSpotId);
        if (startingSpot != null)
        {
            player.CurrentLocation = startingSpot;
            Venue? startingLocation = _gameWorld.Venues.FirstOrDefault(l => l.Id == startingSpot.VenueId);
        }
        else
        { }

        // Initialize player resources from GameWorld initial player config
        if (_gameWorld.InitialPlayerConfig == null)
        {
            throw new InvalidOperationException(
                "InitialPlayerConfig not loaded from package content. " +
                "Ensure gameplay.json contains playerInitialConfig section.");
        }

        if (!_gameWorld.InitialPlayerConfig.Coins.HasValue)
            throw new InvalidOperationException("InitialPlayerConfig missing required field 'coins'");
        if (!_gameWorld.InitialPlayerConfig.Health.HasValue)
            throw new InvalidOperationException("InitialPlayerConfig missing required field 'health'");
        if (!_gameWorld.InitialPlayerConfig.MaxHealth.HasValue)
            throw new InvalidOperationException("InitialPlayerConfig missing required field 'maxHealth'");
        if (!_gameWorld.InitialPlayerConfig.Hunger.HasValue)
            throw new InvalidOperationException("InitialPlayerConfig missing required field 'hunger'");
        if (!_gameWorld.InitialPlayerConfig.StaminaPoints.HasValue)
            throw new InvalidOperationException("InitialPlayerConfig missing required field 'staminaPoints'");
        if (!_gameWorld.InitialPlayerConfig.MaxStamina.HasValue)
            throw new InvalidOperationException("InitialPlayerConfig missing required field 'maxStamina'");

        player.Coins = _gameWorld.InitialPlayerConfig.Coins.Value;
        player.Health = _gameWorld.InitialPlayerConfig.Health.Value;
        player.MaxHealth = _gameWorld.InitialPlayerConfig.MaxHealth.Value;
        player.Hunger = _gameWorld.InitialPlayerConfig.Hunger.Value;
        player.Stamina = _gameWorld.InitialPlayerConfig.StaminaPoints.Value;
        player.MaxStamina = _gameWorld.InitialPlayerConfig.MaxStamina.Value;// Initialize exchange inventories
        _exchangeFacade.InitializeNPCExchanges();// Mark game as started
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
            player.Health = Math.Clamp(player.Health + health, 0, 100);
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

        Venue? venue = _gameWorld.Venues.FirstOrDefault(l => l.Id == venueId);
        if (venue == null)
        {
            _messageSystem.AddSystemMessage($"Location '{venueId}' not found", SystemMessageTypes.Warning);
            return;
        }

        player.CurrentLocation = new Location(LocationId, location.Name) { VenueId = venueId };
        player.AddKnownLocation(venueId);

        _messageSystem.AddSystemMessage($"Teleported to {venue.Name} - {location.Name}", SystemMessageTypes.Success);
    }

    // ========== INVESTIGATION SYSTEM ==========

    /// <summary>
    /// Evaluate and trigger investigation discovery based on current player state
    /// Called when player moves to new location/location, gains knowledge, items, or accepts obligations
    /// Discovered investigations will have their intro goals added to the appropriate location
    /// and discovery modals will be triggered via pending results
    /// </summary>
    public void EvaluateInvestigationDiscovery()
    {
        Player player = _gameWorld.GetPlayer();// Evaluate which investigations can be discovered
        List<Investigation> discoverable = _investigationDiscoveryEvaluator.EvaluateDiscoverableInvestigations(player);// For each discovered investigation, trigger discovery flow
        foreach (Investigation investigation in discoverable)
        {// DiscoverInvestigation moves Potential→Discovered and spawns intro goal at location
            // No return value - goal is added directly to Location.ActiveGoals
            _investigationActivity.DiscoverInvestigation(investigation.Id);

            // Pending discovery result is now set in InvestigationActivity
            // GameScreen will check for it and display the modal

            // Only handle one discovery at a time for POC
            break;
        }
    }

    /// <summary>
    /// Complete investigation intro action - activates investigation and spawns Phase 1
    /// RPG quest acceptance pattern: Player clicks button → Investigation activates immediately
    /// </summary>
    public void CompleteInvestigationIntro(string investigationId)
    {
        _investigationActivity.CompleteIntroAction(investigationId);
    }

    /// <summary>
    /// Set pending intro action - prepares quest acceptance modal
    /// RPG quest acceptance: Button → Modal → "Begin" → Activate
    /// </summary>
    public void SetPendingIntroAction(string investigationId)
    {
        _investigationActivity.SetPendingIntroAction(investigationId);
    }

    /// <summary>
    /// Get pending intro result for modal display
    /// </summary>
    public InvestigationIntroResult GetPendingIntroResult()
    {
        return _investigationActivity.GetAndClearPendingIntroResult();
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
