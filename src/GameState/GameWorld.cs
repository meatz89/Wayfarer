using System;
using System.Collections.Generic;
using System.Linq;

public class GameWorld
{
    // Game mode determines content loading and tutorial state
    public GameMode GameMode { get; set; } = GameMode.MainGame;

    public int CurrentDay { get; set; } = 1;
    public TimeBlocks CurrentTimeBlock { get; set; } = TimeBlocks.Morning;
    public List<Venue> Venues { get; set; } = new List<Venue>();
    public List<Location> Locations { get; set; } = new List<Location>();
    public List<NPC> NPCs { get; set; } = new List<NPC>();
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
    // Mental cards for obligation system
    public List<Goal> Goals { get; set; } = new List<Goal>();
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

    // Dialogue templates from packages
    public DialogueTemplates DialogueTemplates { get; set; }
    public List<Obligation> Obligations { get; private set; } = new List<Obligation>();
    public ObligationJournal ObligationJournal { get; private set; } = new ObligationJournal();

    // Travel System
    public List<RouteImprovement> RouteImprovements { get; set; } = new List<RouteImprovement>();
    public List<TravelObstacle> TravelObstacles { get; private set; } = new List<TravelObstacle>();

    // Initialization data - stored in GameWorld, not passed between phases
    public string InitialLocationSpotId { get; set; }
    public PlayerInitialConfig InitialPlayerConfig { get; set; }

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

    // Session context (obligation tracking for Mental/Physical)
    public string CurrentMentalGoalId { get; set; }
    public string CurrentMentalObligationId { get; set; }
    public string CurrentPhysicalGoalId { get; set; }
    public string CurrentPhysicalObligationId { get; set; }

    // Last outcomes (UI display after session ends)
    public SocialChallengeOutcome LastSocialOutcome { get; set; }
    public MentalOutcome LastMentalOutcome { get; set; }
    public PhysicalOutcome LastPhysicalOutcome { get; set; }

    // PATH SYSTEM - For FixedPath segments that always show the same cards
    // Path card collections for FixedPath route segments (collections contain the actual cards)
    public List<PathCollectionEntry> AllPathCollections { get; set; } = new List<PathCollectionEntry>();

    // EVENT SYSTEM - For Event segments that randomly select from a pool
    // Travel events containing narrative and card references
    public List<TravelEventEntry> AllTravelEvents { get; set; } = new List<TravelEventEntry>();

    // Event collections for Event route segments (containing eventIds, not pathCardIds)
    public List<PathCollectionEntry> AllEventCollections { get; set; } = new List<PathCollectionEntry>();

    // OBSTACLE SYSTEM - Single source of truth for all obstacles
    // Obstacles are location-agnostic, referenced by Location.ObstacleIds and NPC.ObstacleIds
    public List<Obstacle> Obstacles { get; set; } = new List<Obstacle>();

    // Hierarchical world organization
    public List<Region> Regions { get; set; } = new();
    public List<District> Districts { get; set; } = new();

    // Core data collections
    public List<StandingObligation> StandingObligationTemplates { get; set; } = new();

    public List<LocationVisitCount> LocationVisitCounts { get; set; } = new List<LocationVisitCount>();
    public List<string> CompletedConversations { get; } = new List<string>();

    // Weather conditions (no seasons - game timeframe is only days/weeks)
    public WeatherCondition CurrentWeather { get; set; } = WeatherCondition.Clear;

    // Route blocking system
    public List<TemporaryRouteBlock> TemporaryRouteBlocks { get; set; } = new List<TemporaryRouteBlock>();

    // New properties
    public List<Item> Items { get; set; } = new List<Item>();
    public List<RouteOption> Routes { get; set; } = new List<RouteOption>();

    // Card system removed - using conversation and Venue action systems

    // Progression tracking
    public List<RouteDiscovery> RouteDiscoveries { get; set; } = new List<RouteDiscovery>();

    public void RecordLocationVisit(string venueId)
    {
        LocationVisitCount visitCount = LocationVisitCounts.FirstOrDefault(lvc => lvc.LocationId == venueId);
        if (visitCount == null)
        {
            visitCount = new LocationVisitCount { LocationId = venueId, Count = 0 };
            LocationVisitCounts.Add(visitCount);
        }

        visitCount.Count++;
    }

    public int GetLocationVisitCount(string venueId)
    {
        LocationVisitCount visitCount = LocationVisitCounts.FirstOrDefault(lvc => lvc.LocationId == venueId);
        if (visitCount == null)
            return 0;
        return visitCount.Count;
    }

    public bool IsFirstVisit(string venueId)
    {
        return GetLocationVisitCount(venueId) == 0;
    }

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
    /// </summary>
    public void AddTemporaryRouteBlock(string routeId, int daysBlocked, int currentDay)
    {
        TemporaryRouteBlock block = TemporaryRouteBlocks.FirstOrDefault(trb => trb.RouteId == routeId);
        if (block == null)
        {
            block = new TemporaryRouteBlock { RouteId = routeId };
            TemporaryRouteBlocks.Add(block);
        }
        block.UnblockDay = currentDay + daysBlocked;
    }

    /// <summary>
    /// Check if a route is temporarily blocked
    /// </summary>
    public bool IsRouteBlocked(string routeId, int currentDay)
    {
        TemporaryRouteBlock block = TemporaryRouteBlocks.FirstOrDefault(trb => trb.RouteId == routeId);
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
    public District GetDistrictForLocation(string venueId)
    {
        Venue venue = Venues.FirstOrDefault(l => l.Id == venueId);
        if (venue == null || string.IsNullOrEmpty(venue.District))
            return null;

        return Districts.FirstOrDefault(d => d.Id == venue.District);
    }

    public Region GetRegionForDistrict(string districtId)
    {
        District district = Districts.FirstOrDefault(d => d.Id == districtId);
        if (district == null || string.IsNullOrEmpty(district.RegionId))
            return null;

        return Regions.FirstOrDefault(r => r.Id == district.RegionId);
    }

    public string GetFullLocationPath(string venueId)
    {
        Venue? venue = Venues.FirstOrDefault(l => l.Id == venueId);
        if (venue == null) return "";

        District district = GetDistrictForLocation(venueId);
        if (district == null) return venue.Name;

        Region region = GetRegionForDistrict(district.Id);
        if (region == null) return $"{venue.Name}, {district.Name}";

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

    /// Get a Venue location by ID from primary storage
    /// </summary>
    public Location GetLocation(string LocationId)
    {
        return Locations.FirstOrDefault(l => l.Id == LocationId);
    }

    /// <summary>
    /// Get a Goal by ID from centralized Goals list
    /// </summary>
    public Goal GetGoalById(string id)
    {
        return Goals.FirstOrDefault(g => g.Id == id);
    }

    /// <summary>
    /// Get a SocialChallengeDeck by ID
    /// </summary>
    public SocialChallengeDeck GetSocialDeckById(string id)
    {
        return SocialChallengeDecks.FirstOrDefault(d => d.Id == id);
    }

    /// <summary>
    /// Get a MentalChallengeDeck by ID
    /// </summary>
    public MentalChallengeDeck GetMentalDeckById(string id)
    {
        return MentalChallengeDecks.FirstOrDefault(d => d.Id == id);
    }

    /// <summary>
    /// Get a PhysicalChallengeDeck by ID
    /// </summary>
    public PhysicalChallengeDeck GetPhysicalDeckById(string id)
    {
        return PhysicalChallengeDecks.FirstOrDefault(d => d.Id == id);
    }

    /// <summary>
    /// Apply initial player configuration after package loading
    /// </summary>
    public void ApplyInitialPlayerConfiguration()
    {
        if (InitialPlayerConfig != null)
        {
            Player.ApplyInitialConfiguration(InitialPlayerConfig);
        }
    }

    /// <summary>
    /// Add a stranger NPC to a specific location
    /// </summary>
    public void AddStrangerToLocation(string locationId, NPC stranger)
    {
        if (stranger == null) return;
        stranger.LocationId = locationId;
        stranger.IsStranger = true;
        NPCs.Add(stranger);
    }

    /// <summary>
    /// Get available strangers at a location for the current time block
    /// </summary>
    public List<NPC> GetAvailableStrangers(string locationId, TimeBlocks currentTimeBlock)
    {
        List<NPC> availableStrangers = new List<NPC>();
        foreach (NPC npc in NPCs)
        {
            if (npc.IsStranger && npc.LocationId == locationId && npc.IsAvailableAtTime(currentTimeBlock))
            {
                availableStrangers.Add(npc);
            }
        }
        return availableStrangers;
    }

    /// <summary>
    /// Get stranger by ID across all locations
    /// </summary>
    public NPC GetStrangerById(string strangerId)
    {
        foreach (NPC npc in NPCs)
        {
            if (npc.IsStranger && npc.ID == strangerId)
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
    public void MarkStrangerAsTalkedTo(string strangerId)
    {
        NPC stranger = GetStrangerById(strangerId);
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
    public void ActivateObligation(string obligationId, TimeManager timeManager)
    {
        Obligation obligation = Obligations.FirstOrDefault(i => i.Id == obligationId);
        if (obligation == null) return;

        if (!Player.ActiveObligationIds.Contains(obligationId))
        {
            Player.ActiveObligationIds.Add(obligationId);
        }

        if (obligation.ObligationType == ObligationObligationType.NPCCommissioned)
        {
            int currentSegment = timeManager.CurrentSegment;
            if (!obligation.DeadlineSegment.HasValue)
                throw new System.InvalidOperationException($"Obligation {obligationId} is NPCCommissioned but has no DeadlineSegment configured");
            int deadlineDuration = obligation.DeadlineSegment.Value;
            obligation.DeadlineSegment = currentSegment + deadlineDuration;
        }
    }

    /// <summary>
    /// Complete an obligation - apply rewards, chain spawned obligations, clear deadline
    /// Applies coins, items, XP, and increases relationship with patron (NPCCommissioned only)
    /// Chains spawned obligations by activating each spawned obligation
    /// </summary>
    public void CompleteObligation(string obligationId, TimeManager timeManager)
    {
        Obligation obligation = Obligations.FirstOrDefault(i => i.Id == obligationId);
        if (obligation == null) return;

        Player.ModifyCoins(obligation.CompletionRewardCoins);

        foreach (string itemId in obligation.CompletionRewardItems)
        {
            Player.Inventory.AddItem(itemId);
        }

        foreach (StatXPReward xpReward in obligation.CompletionRewardXP)
        {
            Player.Stats.AddXP(xpReward.Stat, xpReward.XPAmount);
        }

        foreach (string spawnedId in obligation.SpawnedObligationIds)
        {
            ActivateObligation(spawnedId, timeManager);
        }

        if (Player.ActiveObligationIds.Contains(obligationId))
        {
            Player.ActiveObligationIds.Remove(obligationId);
        }

        if (obligation.ObligationType == ObligationObligationType.NPCCommissioned &&
            !string.IsNullOrEmpty(obligation.PatronNpcId))
        {
            NPC patron = NPCs.FirstOrDefault(n => n.ID == obligation.PatronNpcId);
            if (patron != null)
            {
                patron.RelationshipFlow = Math.Min(24, patron.RelationshipFlow + 2);
            }
        }
    }

    /// <summary>
    /// Check for expired obligations by comparing current segment to deadline
    /// Returns list of obligation IDs that have exceeded their deadline
    /// </summary>
    public List<string> CheckDeadlines(int currentSegment)
    {
        List<string> expiredObligations = new List<string>();

        foreach (string obligationId in Player.ActiveObligationIds)
        {
            Obligation obligation = Obligations.FirstOrDefault(i => i.Id == obligationId);
            if (obligation != null &&
                obligation.ObligationType == ObligationObligationType.NPCCommissioned &&
                obligation.DeadlineSegment.HasValue &&
                currentSegment >= obligation.DeadlineSegment.Value)
            {
                expiredObligations.Add(obligationId);
            }
        }

        return expiredObligations;
    }

    /// <summary>
    /// Apply deadline consequences for missed obligation
    /// Penalizes relationship with patron NPC, removes StoryCubes, and removes from active obligations
    /// </summary>
    public void ApplyDeadlineConsequences(string obligationId)
    {
        Obligation obligation = Obligations.FirstOrDefault(i => i.Id == obligationId);
        if (obligation == null) return;

        if (obligation.ObligationType == ObligationObligationType.NPCCommissioned &&
            !string.IsNullOrEmpty(obligation.PatronNpcId))
        {
            NPC patron = NPCs.FirstOrDefault(n => n.ID == obligation.PatronNpcId);
            if (patron != null)
            {
                patron.RelationshipFlow = Math.Max(0, patron.RelationshipFlow - 3);

                int cubeReduction = Math.Min(2, patron.StoryCubes);
                patron.StoryCubes = Math.Max(0, patron.StoryCubes - cubeReduction);
            }
        }

        if (Player.ActiveObligationIds.Contains(obligationId))
        {
            Player.ActiveObligationIds.Remove(obligationId);
        }

        obligation.IsFailed = true;
    }

    /// <summary>
    /// Get all active obligations for player
    /// </summary>
    public List<Obligation> GetActiveObligations()
    {
        List<Obligation> activeObligations = new List<Obligation>();
        foreach (string obligationId in Player.ActiveObligationIds)
        {
            Obligation obligation = Obligations.FirstOrDefault(i => i.Id == obligationId);
            if (obligation != null)
            {
                activeObligations.Add(obligation);
            }
        }
        return activeObligations;
    }

    // ============================================
    // EQUIPMENT MANAGEMENT (Core Loop design)
    // ============================================

    /// <summary>
    /// Purchase equipment and add to player inventory
    /// </summary>
    public bool PurchaseEquipment(string equipmentId, int cost)
    {
        if (Player.Coins < cost)
        {
            return false;
        }

        Player.ModifyCoins(-cost);
        Player.Inventory.AddItem(equipmentId);
        return true;
    }

    /// <summary>
    /// Sell equipment from inventory
    /// </summary>
    public bool SellEquipment(string equipmentId, int sellPrice)
    {
        if (!Player.Inventory.HasItem(equipmentId))
        {
            return false;
        }

        Player.Inventory.RemoveItem(equipmentId);
        Player.ModifyCoins(sellPrice);
        return true;
    }

    // ============================================
    // CUBE MANAGEMENT (Localized Mastery)
    // ============================================

    /// <summary>
    /// Get InvestigationCubes for a location (0-10 scale)
    /// </summary>
    public int GetLocationCubes(string locationId)
    {
        Location location = GetLocation(locationId);
        if (location == null)
            throw new InvalidOperationException($"Location not found: {locationId}");

        return location.InvestigationCubes;
    }

    /// <summary>
    /// Get StoryCubes for an NPC (0-10 scale)
    /// </summary>
    public int GetNPCCubes(string npcId)
    {
        NPC npc = NPCs.FirstOrDefault(n => n.ID == npcId);
        if (npc == null)
            throw new InvalidOperationException($"NPC not found: {npcId}");

        return npc.StoryCubes;
    }

    /// <summary>
    /// Get ExplorationCubes for a route (0-10 scale)
    /// </summary>
    public int GetRouteCubes(string routeId)
    {
        // Routes are stored as RouteOption - need to find through venue system
        // This method will be implemented when route storage is clarified
        return 0;
    }

    /// <summary>
    /// Grant InvestigationCubes to a location (max 10)
    /// </summary>
    public void GrantLocationCubes(string locationId, int amount)
    {
        Location location = GetLocation(locationId);
        if (location != null)
        {
            location.InvestigationCubes = Math.Min(10, location.InvestigationCubes + amount);
        }
    }

    /// <summary>
    /// Grant StoryCubes to an NPC (max 10)
    /// </summary>
    public void GrantNPCCubes(string npcId, int amount)
    {
        NPC npc = NPCs.FirstOrDefault(n => n.ID == npcId);
        if (npc != null)
        {
            npc.StoryCubes = Math.Min(10, npc.StoryCubes + amount);
        }
    }

    /// <summary>
    /// Grant ExplorationCubes to a route (max 10)
    /// </summary>
    public void GrantRouteCubes(string routeId, int amount)
    {
        // Routes are stored as RouteOption - need to find through venue system
        // This method will be implemented when route storage is clarified
    }

}
