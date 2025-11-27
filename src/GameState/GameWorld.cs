public class GameWorld
{
    // Game mode determines content loading and tutorial state
    public GameMode GameMode { get; set; } = GameMode.MainGame;

    public int CurrentDay { get; set; } = 1;
    public TimeBlocks CurrentTimeBlock { get; set; } = TimeBlocks.Morning;
    public List<Venue> Venues { get; set; } = new List<Venue>();
    public List<Location> Locations { get; set; } = new List<Location>();
    public List<NPC> NPCs { get; set; } = new List<NPC>();

    // HEX-BASED TRAVEL SYSTEM - Spatial scaffolding for procedural generation
    // World hex grid with terrain, danger levels, and location placement
    // Source of truth for spatial positioning and hex-based pathfinding
    // HIGHLANDER: GameWorld.WorldHexGrid owns all Hex entities
    public HexMap WorldHexGrid { get; set; } = new HexMap();

    // ATMOSPHERIC ACTION LAYER: Static gameplay actions (Travel, Work, Rest, Intra-Venue Movement)
    // Generated once at parse-time from LocationActionCatalog
    // Always available regardless of scene state - prevents dead ends and soft-locks
    // DISTINCT from scene-spawned ephemeral actions (created fresh from ChoiceTemplates)
    public List<LocationAction> LocationActions { get; set; } = new List<LocationAction>();

    public List<PlayerAction> PlayerActions { get; set; } = new List<PlayerAction>();

    // TimeBlock tracking for stranger refresh
    private TimeBlocks _lastTimeBlock = TimeBlocks.Morning;

    // Player stats system for character progression
    public List<PlayerStatDefinition> PlayerStatDefinitions { get; set; } = new List<PlayerStatDefinition>();
    public StatProgression StatProgression { get; set; }

    private Player Player;
    public StreamingContentState StreamingContentState { get; private set; }

    public Guid GameInstanceId { get; set; }
    public RouteOption CurrentRouteOption { get; internal set; }

    // System Messages State
    public List<SystemMessage> SystemMessages { get; set; } = new List<SystemMessage>();
    // Event Log - Permanent record of all messages
    public List<SystemMessage> EventLog { get; set; } = new List<SystemMessage>();

    // DECK ARCHITECTURE - Single source of truth for all deck configurations
    // All cards are ConversationCard type (no LetterCard, ExchangeCard, etc.)
    // Exchange cards are now completely separate from conversation cards
    public List<NPCExchangeCardEntry> NPCExchangeCards { get; set; } = new List<NPCExchangeCardEntry>();
    public List<ExchangeHistoryEntry> ExchangeHistory { get; set; } = new List<ExchangeHistoryEntry>();
    public List<SocialCard> PlayerObservationCards { get; set; } = new List<SocialCard>();
    // Exchange definitions loaded from JSON for lookup
    public List<ExchangeDTO> ExchangeDefinitions { get; set; } = new List<ExchangeDTO>();
    public List<SocialCard> SocialCards { get; set; } = new List<SocialCard>();
    public List<MentalCard> MentalCards { get; set; } = new List<MentalCard>();
    // Physical cards for physical challenge system
    public List<PhysicalCard> PhysicalCards { get; set; } = new List<PhysicalCard>();

    // THREE PARALLEL TACTICAL SYSTEMS - Decks only (no Types, they're redundant)
    // Decks contain all necessary configuration for tactical engagements
    public List<SocialChallengeDeck> SocialChallengeDecks { get; set; } = new List<SocialChallengeDeck>();
    public List<MentalChallengeDeck> MentalChallengeDecks { get; set; } = new List<MentalChallengeDeck>();
    public List<PhysicalChallengeDeck> PhysicalChallengeDecks { get; set; } = new List<PhysicalChallengeDeck>();

    // Observations from packages
    public List<Observation> Observations { get; set; } = new List<Observation>();

    // ObservationScenes - Mental challenge system for scene investigation
    public List<ObservationScene> ObservationScenes { get; set; } = new List<ObservationScene>();

    // ConversationTrees - Simple dialogue without tactical challenge
    public List<ConversationTree> ConversationTrees { get; set; } = new List<ConversationTree>();
    // EmergencySituations - Urgent situations demanding immediate response
    public List<EmergencySituation> EmergencySituations { get; set; } = new List<EmergencySituation>();
    // ActiveEmergency - Currently triggering emergency that interrupts gameplay (set at sync points)
    public EmergencySituation ActiveEmergency { get; set; }

    // ADR-007: PendingForcedSceneId DELETED - replaced with PendingForcedScene object reference
    // Modal scene that should auto-trigger on location entry
    // Set by movement methods after checking for forced scenes
    // Checked by UI layer (LocationContent) after movement completes
    // HIGHLANDER: Single pending forced scene at any time
    public Scene PendingForcedScene { get; set; }

    // Dialogue templates from packages
    public DialogueTemplates DialogueTemplates { get; set; }
    public List<Obligation> Obligations { get; private set; } = new List<Obligation>();
    public ObligationJournal ObligationJournal { get; private set; } = new ObligationJournal();

    // Travel System
    public List<RouteImprovement> RouteImprovements { get; set; } = new List<RouteImprovement>();

    // ADR-007: InitialLocationId DELETED (dead code - never read)
    // Initialization data - stored in GameWorld, not passed between phases
    public PlayerInitialConfig InitialPlayerConfig { get; set; }

    // HIGHLANDER: Starting location as object reference (set by PackageLoader, used by GameFacade.StartGameAsync)
    // Player.CurrentPosition initialized from StartingLocation.HexPosition at game start
    public Location StartingLocation { get; set; }

    // Time initialization (applied to TimeModel after DI initialization)
    public int? InitialDay { get; set; }
    public TimeBlocks? InitialTimeBlock { get; set; }
    public int? InitialSegment { get; set; }

    // Skeleton tracking for lazy content resolution
    public List<SkeletonRegistryEntry> SkeletonRegistry { get; set; } = new List<SkeletonRegistryEntry>();

    // Track if game has been started to prevent duplicate initialization
    public bool IsGameStarted { get; set; } = false;

    // Persistent discovery states
    public List<PathCardDiscoveryEntry> PathCardDiscoveries { get; set; } = new List<PathCardDiscoveryEntry>();

    // Track one-time rewards
    public List<PathCardDiscoveryEntry> PathCardRewardsClaimed { get; set; } = new List<PathCardDiscoveryEntry>();

    // Track event deck positions for deterministic draws
    public List<EventDeckPositionEntry> EventDeckPositions { get; set; } = new List<EventDeckPositionEntry>();

    // Active travel session
    public TravelSession CurrentTravelSession { get; set; }

    // THREE PARALLEL TACTICAL SYSTEMS - Active Session State
    public SocialSession CurrentSocialSession { get; set; }
    public MentalSession CurrentMentalSession { get; set; }
    public PhysicalSession CurrentPhysicalSession { get; set; }

    // PROCEDURAL CONTENT TRACING SYSTEM - Debugging tool for tracking spawn graph
    // Captures scene spawning, situation creation, choice execution
    // Null-safe: Check IsEnabled before using
    public ProceduralContentTracer ProceduralTracer { get; set; }

    // ADR-007: Session context IDs DELETED - MentalChallengeContext/PhysicalChallengeContext hold objects
    // Session contexts already contain SituationCard and Obligation object references
    // No need for redundant ID storage in GameWorld

    // Last outcomes (UI display after session ends)
    public SocialChallengeOutcome LastSocialOutcome { get; set; }
    public MentalOutcome LastMentalOutcome { get; set; }
    public PhysicalOutcome LastPhysicalOutcome { get; set; }

    // Pending challenge contexts (for reward application after challenge completion)
    // Set when challenge starts from Choice with ActionType = StartChallenge
    // Contains CompletionReward to apply when challenge succeeds
    // Cleared after challenge ends (success or failure)
    // Tutorial system uses this: store reward when Elena social challenge starts, apply when succeeds
    public SocialChallengeContext PendingSocialContext { get; set; }
    public MentalChallengeContext PendingMentalContext { get; set; }
    public PhysicalChallengeContext PendingPhysicalContext { get; set; }

    // PATH SYSTEM - For FixedPath segments that always show the same cards
    // Path card collections for FixedPath route segments (collections contain the actual cards)
    public List<PathCollectionEntry> AllPathCollections { get; set; } = new List<PathCollectionEntry>();

    // EVENT SYSTEM - For Event segments that randomly select from a pool
    // Travel events containing narrative and card references
    public List<TravelEventEntry> AllTravelEvents { get; set; } = new List<TravelEventEntry>();

    // Event collections for Event route segments (containing eventIds, not pathCardIds)
    public List<PathCollectionEntry> AllEventCollections { get; set; } = new List<PathCollectionEntry>();

    // SCENE-SITUATION TEMPLATE SYSTEM - Templates for procedural Scene generation
    // SceneTemplates define Scene archetypes with categorical filters
    // At spawn time, SceneInstantiator queries GameWorld for matching entities and creates Scene instances
    public List<SceneTemplate> SceneTemplates { get; set; } = new List<SceneTemplate>();

    // SCENE SYSTEM - Persistent Scene instances spawned from SceneTemplates
    // PHASE 1.4: Unified collection for all scenes (Active, Provisional, Completed)
    // Scenes stored here, queried by PlacementType and PlacementId
    // Contains embedded Situations with 2-4 Choices (Sir Brante pattern)
    public List<Scene> Scenes { get; set; } = new List<Scene>();

    // SCENE-SITUATION ARCHITECTURE - Sir Brante integration
    // State definitions (metadata about temporary player conditions)
    public List<State> States { get; set; } = new List<State>();
    // Achievement definitions (milestone templates)
    public List<Achievement> Achievements { get; set; } = new List<Achievement>();

    // Hierarchical world organization
    public List<Region> Regions { get; set; } = new();
    public List<District> Districts { get; set; } = new();

    // Core data collections
    public List<StandingObligation> StandingObligationTemplates { get; set; } = new();

    public List<string> CompletedConversations { get; } = new List<string>();

    // Weather conditions (no seasons - game timeframe is only days/weeks)
    public WeatherCondition CurrentWeather { get; set; } = WeatherCondition.Clear;

    // Route blocking system
    public List<TemporaryRouteBlock> TemporaryRouteBlocks { get; set; } = new List<TemporaryRouteBlock>();

    // New properties
    public List<Item> Items { get; set; } = new List<Item>();

    // Market price override system - enables testing and special market conditions
    // Used for quest rewards, merchant relationships, government price controls, etc.
    // HIGHLANDER: GameWorld owns all price modifiers, single source of truth
    // SENTINEL: Null override = use calculated price, explicit value = use that price
    public List<MarketPriceModifier> MarketPriceModifiers { get; set; } = new List<MarketPriceModifier>();

    public List<RouteOption> Routes { get; set; } = new List<RouteOption>();

    // Delivery Job System - Core game loop (Phase 3)
    // Jobs generated procedurally at parse time from routes by DeliveryJobCatalog
    // Players can accept ONE active job at a time (tracked in Player.ActiveDeliveryJob)
    public List<DeliveryJob> AvailableDeliveryJobs { get; set; } = new();

    // Card system removed - using conversation and Venue action systems

    // Progression tracking
    public List<RouteDiscovery> RouteDiscoveries { get; set; } = new List<RouteDiscovery>();

    public bool IsConversationCompleted(string actionId)
    {
        return CompletedConversations.Contains(actionId);
    }

    public void MarkConversationCompleted(string actionId)
    {
        CompletedConversations.Add(actionId);
    }

    public void AddCharacter(NPC character)
    {
        NPCs.Add(character);
    }

    public List<NPC> GetCharacters()
    {
        return NPCs;
    }

    /// <summary>
    /// Add a temporary route block that expires after specified days
    /// HIGHLANDER: Accepts RouteOption object, uses route.Name as natural key
    /// ZERO NULL TOLERANCE: route must never be null
    /// </summary>
    public void AddTemporaryRouteBlock(RouteOption route, int daysBlocked, int currentDay)
    {
        // HIGHLANDER: Compare Route objects directly, not route.Name strings
        TemporaryRouteBlock block = TemporaryRouteBlocks.FirstOrDefault(trb => trb.Route == route);
        if (block == null)
        {
            // HIGHLANDER: Store Route object reference, not RouteName string
            block = new TemporaryRouteBlock { Route = route };
            TemporaryRouteBlocks.Add(block);
        }
        block.UnblockDay = currentDay + daysBlocked;
    }

    /// <summary>
    /// Check if a route is temporarily blocked
    /// HIGHLANDER: Accepts RouteOption object, uses route.Name as natural key
    /// ZERO NULL TOLERANCE: route must never be null
    /// </summary>
    public bool IsRouteBlocked(RouteOption route, int currentDay)
    {
        // HIGHLANDER: Compare Route objects directly, not route.Name strings
        TemporaryRouteBlock block = TemporaryRouteBlocks.FirstOrDefault(trb => trb.Route == route);
        if (block != null)
        {
            if (currentDay >= block.UnblockDay)
            {
                TemporaryRouteBlocks.Remove(block);
                return false;
            }
            return true;
        }
        return false;
    }

    // Hierarchy lookup methods
    public District GetDistrictForLocation(Venue venue)
    {
        // ZERO NULL TOLERANCE: venue must never be null
        return venue.District;
    }

    // HIGHLANDER: Accept District object, use object reference (not string lookup)
    public Region GetRegionForDistrict(District district)
    {
        // ZERO NULL TOLERANCE: district must never be null
        return district.Region;
    }

    public string GetFullLocationPath(string venueName)
    {
        Venue venue = Venues.FirstOrDefault(l => l.Name == venueName);
        // FAIL-FAST: If venue name is invalid, this is data error
        if (venue == null)
        {
            throw new InvalidOperationException($"Venue '{venueName}' not found in GameWorld");
        }

        // HIGHLANDER: GetDistrictForLocation accepts Venue, not string - need to pass venue object
        District district = GetDistrictForLocation(venue);

        // HIGHLANDER: Pass District object, not string
        Region region = GetRegionForDistrict(district);

        return $"{venue.Name}, {district.Name}, {region.Name}";
    }

    /// <summary>
    /// Get a report of all skeletons that need to be populated
    /// </summary>
    public List<string> GetSkeletonReport()
    {
        List<string> report = new List<string>();
        foreach (SkeletonRegistryEntry entry in SkeletonRegistry)
        {
            report.Add($"{entry.ContentType}: {entry.SkeletonKey}");
        }
        return report;
    }

    public GameWorld()
    {
        if (GameInstanceId == Guid.Empty) GameInstanceId = Guid.NewGuid();

        Player = new Player();

        // GameWorld has NO dependencies and creates NO managers

        StreamingContentState = new StreamingContentState();

    }

    public Player GetPlayer()
    {
        return Player;
    }

    public PlayerResourceState GetPlayerResourceState()
    {
        // Get resource state from the actual Player object with proper 1:1 mapping
        return new PlayerResourceState(
            coins: Player.Coins,
            stamina: Player.Stamina,
            health: Player.Health,
            hunger: Player.Hunger,
            maxStamina: Player.MaxStamina,
            maxHealth: Player.MaxHealth,
            maxHunger: Player.MaxHunger
        );
    }

    /// <summary>
    /// Get all NPCs in the game world
    /// </summary>
    public List<NPC> GetAllNPCs()
    {
        return NPCs;
    }

    // HIGHLANDER: GetLocation(string) DELETED - use Locations.FirstOrDefault(l => l.Name == name) directly

    /// <summary>
    /// Get player's current location via hex-first architecture
    /// HEX-FIRST PATTERN: player.CurrentPosition → hex → Location object
    /// Returns null if player position has no location
    /// HIGHLANDER: Direct object access (not locationId lookup)
    /// NOTE: Returns null during initialization (before IsGameStarted), which is expected
    /// Only logs errors after game has started to avoid false positives during init
    /// </summary>
    public Location GetPlayerCurrentLocation()
    {
        Player player = GetPlayer();
        if (player == null)
        {
            if (IsGameStarted)
                Console.WriteLine("[GameWorld.GetPlayerCurrentLocation] ERROR: Player is null");
            return null;
        }

        if (WorldHexGrid == null)
        {
            if (IsGameStarted)
                Console.WriteLine("[GameWorld.GetPlayerCurrentLocation] ERROR: WorldHexGrid is null");
            return null;
        }

        Hex currentHex = WorldHexGrid.GetHex(player.CurrentPosition);
        if (currentHex == null)
        {
            if (IsGameStarted)
                Console.WriteLine($"[GameWorld.GetPlayerCurrentLocation] ERROR: No hex found at position ({player.CurrentPosition.Q}, {player.CurrentPosition.R})");
            return null;
        }

        // HIGHLANDER: Query Location.HexPosition (source of truth), not derived lookup
        Location primaryLocation = GetPrimaryLocationAtHex(player.CurrentPosition.Q, player.CurrentPosition.R);
        if (primaryLocation == null)
        {
            if (IsGameStarted)
                Console.WriteLine($"[GameWorld.GetPlayerCurrentLocation] ERROR: No location found at hex ({player.CurrentPosition.Q}, {player.CurrentPosition.R})");
            return null;
        }

        return primaryLocation;
    }

    /// <summary>
    /// Get all locations at a specific hex position.
    /// HIGHLANDER: Queries Location.HexPosition (source of truth) instead of derived lookup.
    /// Multiple locations can exist at the same hex (e.g., Common Room + Private Room).
    /// </summary>
    public List<Location> GetLocationsAtHex(int q, int r)
    {
        return Locations.Where(l =>
            l.HexPosition.HasValue &&
            l.HexPosition.Value.Q == q &&
            l.HexPosition.Value.R == r
        ).ToList();
    }

    /// <summary>
    /// Get the primary location at a hex position.
    /// Authored locations take precedence over scene-created locations.
    /// Returns null if no location exists at that hex.
    /// </summary>
    public Location GetPrimaryLocationAtHex(int q, int r)
    {
        return GetLocationsAtHex(q, r)
            .OrderBy(l => l.Origin == LocationOrigin.Authored ? 0 : 1)
            .FirstOrDefault();
    }

    /// <summary>
    /// Get a SocialChallengeDeck by name
    /// </summary>
    public SocialChallengeDeck GetSocialDeckByName(string name)
    {
        return SocialChallengeDecks.FirstOrDefault(d => d.Name == name);
    }

    /// <summary>
    /// Get a MentalChallengeDeck by name
    /// </summary>
    public MentalChallengeDeck GetMentalDeckByName(string name)
    {
        return MentalChallengeDecks.FirstOrDefault(d => d.Name == name);
    }

    /// <summary>
    /// Get a PhysicalChallengeDeck by name
    /// </summary>
    public PhysicalChallengeDeck GetPhysicalDeckByName(string name)
    {
        return PhysicalChallengeDecks.FirstOrDefault(d => d.Name == name);
    }

    /// <summary>
    /// Apply initial player configuration after package loading
    /// ZERO NULL TOLERANCE NOTE: InitialPlayerConfig can be null (optional configuration)
    /// </summary>
    public void ApplyInitialPlayerConfiguration()
    {
        if (InitialPlayerConfig != null)
        {
            Player.ApplyInitialConfiguration(InitialPlayerConfig, this);
        }
    }

    // HIGHLANDER: AddStrangerToLocation(string) DELETED - no callers, violated object parameter principle

    /// <summary>
    /// Get available strangers at a venue for the current time block
    /// HIGHLANDER: Accepts Venue object, uses object equality (not .Name comparison)
    /// </summary>
    public List<NPC> GetAvailableStrangers(Venue venue, TimeBlocks currentTimeBlock)
    {
        List<NPC> availableStrangers = new List<NPC>();
        foreach (NPC npc in NPCs)
        {
            if (npc.IsStranger && npc.Location?.Venue == venue && npc.IsAvailableAtTime(currentTimeBlock))
            {
                availableStrangers.Add(npc);
            }
        }
        return availableStrangers;
    }

    /// <summary>
    /// Get stranger by name across all locations
    /// </summary>
    public NPC GetStrangerByName(string strangerName)
    {
        foreach (NPC npc in NPCs)
        {
            if (npc.IsStranger && npc.Name == strangerName)
            {
                return npc;
            }
        }
        return null;
    }

    /// <summary>
    /// Refresh all strangers when time block changes
    /// </summary>
    public void RefreshStrangersForTimeBlock(TimeBlocks newTimeBlock)
    {
        if (_lastTimeBlock != newTimeBlock)
        {
            foreach (NPC npc in NPCs)
            {
                if (npc.IsStranger)
                {
                    npc.RefreshForNewTimeBlock();
                }
            }
            _lastTimeBlock = newTimeBlock;
        }
    }

    /// <summary>
    /// Mark a stranger as talked to for current time block
    /// </summary>
    public void MarkStrangerAsTalkedTo(string strangerName)
    {
        NPC stranger = GetStrangerByName(strangerName);
        if (stranger != null)
            stranger.MarkAsEncountered();
    }

    /// <summary>
    /// Get all strangers across all locations (for debugging/admin)
    /// </summary>
    public List<NPC> GetAllStrangers()
    {
        List<NPC> allStrangers = new List<NPC>();
        foreach (NPC npc in NPCs)
        {
            if (npc.IsStranger)
            {
                allStrangers.Add(npc);
            }
        }
        return allStrangers;
    }

    // ============================================
    // OBLIGATION MANAGEMENT (Core Loop design)
    // ============================================

    /// <summary>
    /// Activate an obligation - mark as active and start deadline tracking
    /// NPCCommissioned: Sets absolute deadline segment and tracks in active obligations
    /// SelfDiscovered: No deadline tracking (freedom-based exploration)
    /// </summary>
    public void ActivateObligation(string obligationName, TimeManager timeManager)
    {
        Obligation obligation = Obligations.FirstOrDefault(i => i.Name == obligationName);
        if (obligation == null) return;

        if (!Player.ActiveObligations.Contains(obligation))
        {
            Player.ActiveObligations.Add(obligation);
        }

        if (obligation.ObligationType == ObligationObligationType.NPCCommissioned)
        {
            int currentSegment = timeManager.CurrentSegment;
            if (!obligation.DeadlineSegment.HasValue)
                throw new System.InvalidOperationException($"Obligation {obligationName} is NPCCommissioned but has no DeadlineSegment configured");
            int deadlineDuration = obligation.DeadlineSegment.Value;
            obligation.DeadlineSegment = currentSegment + deadlineDuration;
        }
    }

    /// <summary>
    /// Complete an obligation - apply rewards, chain spawned obligations, clear deadline
    /// Applies coins, items, XP, and increases relationship with patron (NPCCommissioned only)
    /// Chains spawned obligations by activating each spawned obligation
    /// </summary>
    public void CompleteObligation(string obligationName, TimeManager timeManager)
    {
        Obligation obligation = Obligations.FirstOrDefault(i => i.Name == obligationName);
        if (obligation == null) return;

        Player.ModifyCoins(obligation.CompletionRewardCoins);

        foreach (Item item in obligation.CompletionRewardItems)
        {
            Player.Inventory.Add(item);
        }

        // Stats are now simple integers - no XP system
        // CompletionRewardXP deleted as part of XP system removal

        foreach (Obligation spawnedObligation in obligation.SpawnedObligations)
        {
            ActivateObligation(spawnedObligation.Name, timeManager);
        }

        if (Player.ActiveObligations.Contains(obligation))
        {
            Player.ActiveObligations.Remove(obligation);
        }

        if (obligation.ObligationType == ObligationObligationType.NPCCommissioned &&
            obligation.PatronNpc != null)
        {
            NPC patron = obligation.PatronNpc;
            patron.RelationshipFlow = Math.Min(24, patron.RelationshipFlow + 2);
        }
    }

    // ============================================
    // DELIVERY JOB MANAGEMENT (Core Loop - Phase 3)
    // ============================================

    /// <summary>
    /// Get all available delivery jobs at a specific location for current time
    /// Used by LocationActionManager to show job board actions
    /// </summary>
    public List<DeliveryJob> GetJobsAvailableAt(Location location, TimeBlocks currentTime)
    {
        return AvailableDeliveryJobs
            .Where(job => job.OriginLocation == location)
            .Where(job => job.IsAvailable)
            .Where(job => job.AvailableAt.Count == 0 || job.AvailableAt.Contains(currentTime))
            .ToList();
    }

    // HIGHLANDER: GetJobByLocations(string, string) DELETED - no callers, violated object parameter principle

    /// <summary>
    /// Check for expired obligations by comparing current segment to deadline
    /// Returns list of obligation names that have exceeded their deadline
    /// </summary>
    public List<string> CheckDeadlines(int currentSegment)
    {
        List<string> expiredObligations = new List<string>();

        foreach (Obligation obligation in Player.ActiveObligations)
        {
            if (obligation.ObligationType == ObligationObligationType.NPCCommissioned &&
                obligation.DeadlineSegment.HasValue &&
                currentSegment >= obligation.DeadlineSegment.Value)
            {
                expiredObligations.Add(obligation.Name);
            }
        }

        return expiredObligations;
    }

    /// <summary>
    /// Apply deadline consequences for missed obligation
    /// Penalizes relationship with patron NPC, removes StoryCubes, and removes from active obligations
    /// </summary>
    public void ApplyDeadlineConsequences(string obligationName)
    {
        Obligation obligation = Obligations.FirstOrDefault(i => i.Name == obligationName);
        if (obligation == null) return;

        if (obligation.ObligationType == ObligationObligationType.NPCCommissioned &&
            obligation.PatronNpc != null)
        {
            NPC patron = obligation.PatronNpc;
            patron.RelationshipFlow = Math.Max(0, patron.RelationshipFlow - 3);

            int cubeReduction = Math.Min(2, patron.StoryCubes);
            patron.StoryCubes = Math.Max(0, patron.StoryCubes - cubeReduction);
        }

        if (Player.ActiveObligations.Contains(obligation))
        {
            Player.ActiveObligations.Remove(obligation);
        }

        obligation.IsFailed = true;
    }

    /// <summary>
    /// Get all active obligations for player
    /// </summary>
    public List<Obligation> GetActiveObligations()
    {
        return Player.ActiveObligations;
    }

    // ============================================
    // CUBE MANAGEMENT (Localized Mastery)
    // ============================================

    /// <summary>
    /// Get InvestigationCubes for a location (0-10 scale)
    /// HIGHLANDER: Accept Location object, not string name
    /// </summary>
    public int GetLocationCubes(Location location)
    {
        if (location == null)
            throw new ArgumentNullException(nameof(location));

        return location.InvestigationCubes;
    }

    /// <summary>
    /// Get StoryCubes for an NPC (0-10 scale)
    /// HIGHLANDER: Accept NPC object, not string name
    /// </summary>
    public int GetNPCCubes(NPC npc)
    {
        if (npc == null)
            throw new ArgumentNullException(nameof(npc));

        return npc.StoryCubes;
    }

    /// <summary>
    /// Get ExplorationCubes for a route (0-10 scale)
    /// HIGHLANDER: Accept RouteOption object, not string name
    /// </summary>
    public int GetRouteCubes(RouteOption route)
    {
        if (route == null)
            throw new ArgumentNullException(nameof(route));

        return route.ExplorationCubes;
    }

    /// <summary>
    /// Grant InvestigationCubes to a location (max 10)
    /// HIGHLANDER: Accept Location object, not string name
    /// </summary>
    public void GrantLocationCubes(Location location, int amount)
    {
        if (location == null)
            throw new ArgumentNullException(nameof(location));

        location.InvestigationCubes = Math.Min(10, location.InvestigationCubes + amount);
    }

    /// <summary>
    /// Grant StoryCubes to an NPC (max 10)
    /// HIGHLANDER: Accept NPC object, not string name
    /// </summary>
    public void GrantNPCCubes(NPC npc, int amount)
    {
        if (npc == null)
            throw new ArgumentNullException(nameof(npc));

        npc.StoryCubes = Math.Min(10, npc.StoryCubes + amount);
    }

    /// <summary>
    /// Grant ExplorationCubes to a route (max 10)
    /// HIGHLANDER: Accept RouteOption object, not string name
    /// </summary>
    public void GrantRouteCubes(RouteOption route, int amount)
    {
        if (route == null)
            throw new ArgumentNullException(nameof(route));

        route.ExplorationCubes = Math.Min(10, route.ExplorationCubes + amount);
    }

    // ============================================
    // SCENE-SITUATION ARCHITECTURE (Sir Brante Integration)
    // ============================================

    /// <summary>
    /// Get a State definition by type
    /// Returns metadata about a state (blocked actions, clear conditions, etc.)
    /// </summary>
    public State GetStateDefinition(StateType stateType)
    {
        return States.FirstOrDefault(s => s.Type == stateType);
    }

    /// <summary>
    /// Get an Achievement definition by name
    /// </summary>
    public Achievement GetAchievementByName(string achievementName)
    {
        return Achievements.FirstOrDefault(a => a.Name == achievementName);
    }

    /// <summary>
    /// Check if player has earned a specific achievement
    /// HIGHLANDER: Accepts Achievement object, not string ID
    /// </summary>
    public bool HasAchievement(Achievement achievement)
    {
        return Player.EarnedAchievements.Any(pa => pa.Achievement == achievement);
    }

    /// <summary>
    /// Grant achievement to player with current time tracking
    /// HIGHLANDER: Accepts Achievement object, not string ID
    /// </summary>
    public void GrantAchievement(Achievement achievement, int currentDay, TimeBlocks currentTimeBlock, int currentSegment)
    {
        if (HasAchievement(achievement))
            return; // Already earned, don't grant again

        PlayerAchievement playerAchievement = new PlayerAchievement
        {
            Achievement = achievement,
            EarnedDay = currentDay,
            EarnedTimeBlock = currentTimeBlock,
            EarnedSegment = currentSegment
        };

        Player.EarnedAchievements.Add(playerAchievement);
    }

    /// <summary>
    /// Apply a state to the player
    /// </summary>
    public void ApplyState(StateType stateType, int currentDay, TimeBlocks currentTimeBlock, int currentSegment)
    {
        // Check if player already has this state active
        if (Player.ActiveStates.Any(s => s.Type == stateType))
            return; // Already active, don't apply again

        State stateDefinition = GetStateDefinition(stateType);
        if (stateDefinition == null)
            throw new InvalidOperationException($"State definition not found for type: {stateType}");

        ActiveState activeState = new ActiveState
        {
            Type = stateType,
            Category = stateDefinition.Category,
            AppliedDay = currentDay,
            AppliedTimeBlock = currentTimeBlock,
            AppliedSegment = currentSegment,
            DurationSegments = stateDefinition.Duration
        };

        Player.ActiveStates.Add(activeState);
    }

    /// <summary>
    /// Clear a state from the player
    /// </summary>
    public void ClearState(StateType stateType)
    {
        ActiveState activeState = Player.ActiveStates.FirstOrDefault(s => s.Type == stateType);
        if (activeState != null)
        {
            Player.ActiveStates.Remove(activeState);
        }
    }

    /// <summary>
    /// Check and auto-clear expired states based on duration
    /// </summary>
    public void ProcessExpiredStates(int currentDay, TimeBlocks currentTimeBlock, int currentSegment)
    {
        List<ActiveState> expiredStates = new List<ActiveState>();

        foreach (ActiveState state in Player.ActiveStates)
        {
            if (state.ShouldAutoClear(currentDay, currentTimeBlock, currentSegment))
            {
                expiredStates.Add(state);
            }
        }

        foreach (ActiveState expiredState in expiredStates)
        {
            Player.ActiveStates.Remove(expiredState);
        }
    }

    // ============================================
    // PATH COLLECTION MANAGEMENT (Travel system)
    // ============================================

    /// <summary>
    /// Get path collection by ID
    /// </summary>
    public PathCardCollectionDTO GetPathCollection(string collectionId)
    {
        // ADR-007: Use Collection.Id (object property) instead of deleted CollectionId
        PathCollectionEntry entry = AllPathCollections.FirstOrDefault(c => c.Collection.Id == collectionId);
        if (entry == null)
            throw new InvalidOperationException($"No collection entry found for collection '{collectionId}' - ensure collection exists before accessing");
        return entry.Collection;
    }

    /// <summary>
    /// Add or update path collection
    /// </summary>
    public void AddOrUpdatePathCollection(string collectionId, PathCardCollectionDTO collection)
    {
        // ADR-007: Use Collection.Id (object property) instead of deleted CollectionId
        PathCollectionEntry existing = AllPathCollections.FirstOrDefault(c => c.Collection.Id == collectionId);
        if (existing != null)
        {
            existing.Collection = collection;
        }
        else
        {
            AllPathCollections.Add(new PathCollectionEntry { Collection = collection });
        }
    }

    // ============================================
    // TRAVEL EVENT MANAGEMENT (Travel system)
    // ============================================

    /// <summary>
    /// Get travel event by ID
    /// </summary>
    public TravelEventDTO GetTravelEvent(string eventId)
    {
        // ADR-007: Use TravelEvent.Id (object property) instead of deleted EventId
        TravelEventEntry entry = AllTravelEvents.FirstOrDefault(e => e.TravelEvent.Id == eventId);
        if (entry == null)
            throw new InvalidOperationException($"No event entry found for event '{eventId}' - ensure event exists before accessing");
        return entry.TravelEvent;
    }

    // ============================================
    // SKELETON REGISTRY MANAGEMENT (Lazy loading)
    // ============================================

    /// <summary>
    /// Add skeleton to registry for lazy resolution
    /// </summary>
    public void AddSkeleton(string key, string contentType)
    {
        if (!SkeletonRegistry.Any(r => r.SkeletonKey == key))
        {
            SkeletonRegistry.Add(new SkeletonRegistryEntry { SkeletonKey = key, ContentType = contentType });
        }
    }

    // ============================================
    // EVENT DECK POSITION MANAGEMENT (Deterministic draws)
    // ============================================

    /// <summary>
    /// Set event deck position
    /// </summary>
    public void SetEventDeckPosition(string deckId, int position)
    {
        EventDeckPositionEntry existing = EventDeckPositions.FirstOrDefault(p => p.DeckId == deckId);
        if (existing != null)
        {
            existing.Position = position;
        }
        else
        {
            EventDeckPositions.Add(new EventDeckPositionEntry { DeckId = deckId, Position = position });
        }
    }

    // ============================================
    // PATH CARD DISCOVERY MANAGEMENT (Progression tracking)
    // ============================================

    /// <summary>
    /// Check if path card is discovered
    /// HIGHLANDER: Accept PathCardDTO object, extract Id internally for state tracking
    /// Returns false for procedurally generated cards without discovery entries (face-down by default)
    /// </summary>
    public bool IsPathCardDiscovered(PathCardDTO card)
    {
        if (card == null)
            throw new ArgumentNullException(nameof(card));

        PathCardDiscoveryEntry entry = PathCardDiscoveries.FirstOrDefault(d => d.CardId == card.Id);
        if (entry == null)
        {
            return card.StartsRevealed;
        }
        return entry.IsDiscovered;
    }

    /// <summary>
    /// Set path card discovery status
    /// HIGHLANDER: Accept PathCardDTO object, extract Id internally for state tracking
    /// </summary>
    public void SetPathCardDiscovered(PathCardDTO card, bool discovered)
    {
        if (card == null)
            throw new ArgumentNullException(nameof(card));

        PathCardDiscoveryEntry existing = PathCardDiscoveries.FirstOrDefault(d => d.CardId == card.Id);
        if (existing != null)
        {
            existing.IsDiscovered = discovered;
        }
        else
        {
            PathCardDiscoveries.Add(new PathCardDiscoveryEntry { CardId = card.Id, IsDiscovered = discovered });
        }
    }

    // ============================================
    // LOCATION MANAGEMENT (GameWorld owns locations)
    // ============================================

    /// <summary>
    /// Add or update location spot
    /// CRITICAL: Maintains unidirectional Location → Venue relationship
    /// Updates existing location IN-PLACE (no removal - entities persist forever)
    /// </summary>
    public void AddOrUpdateLocation(string locationName, Location location)
    {
        Location existing = Locations.FirstOrDefault(l => l.Name == locationName);
        if (existing != null)
        {
            // UPDATE EXISTING IN-PLACE - Never remove entities
            // Venue relationship is unidirectional (Location → Venue), no bidirectional sync needed

            // Copy all properties from new location to existing (preserve object identity)
            existing.Name = location.Name;
            existing.AssignVenue(location.Venue);
            existing.HexPosition = location.HexPosition;
            // Orthogonal categorical properties
            existing.Environment = location.Environment;
            existing.Setting = location.Setting;
            existing.Role = location.Role;
            existing.Purpose = location.Purpose;
            existing.Privacy = location.Privacy;
            existing.Safety = location.Safety;
            existing.Activity = location.Activity;
            existing.IsSkeleton = false; // Mark as no longer skeleton
            existing.Tier = location.Tier;
            // Provenance intentionally NOT copied - preserve creation metadata
        }
        else
        {
            // New location - add to collection
            Locations.Add(location);
        }
    }

    // ========== VENUE CAPACITY HELPERS ==========

    /// <summary>
    /// Count how many locations are in the specified venue.
    /// HIGHLANDER: Accepts Venue object, uses object equality (not .Name comparison)
    /// </summary>
    public int GetLocationCountInVenue(Venue venue)
    {
        return Locations.Count(loc => loc.Venue == venue);
    }

    /// <summary>
    /// Check if venue can accept more locations within its capacity budget.
    /// HIGHLANDER: Passes Venue object directly (no string extraction)
    /// </summary>
    public bool CanVenueAddMoreLocations(Venue venue)
    {
        int currentCount = GetLocationCountInVenue(venue);
        return currentCount < venue.MaxLocations;
    }

}
