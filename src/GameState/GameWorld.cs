using System;
using System.Collections.Generic;
using System.Linq;

public class GameWorld
{
    // Game mode determines content loading and tutorial state
    public GameMode GameMode { get; set; } = GameMode.MainGame;

    // Time is now tracked in WorldState, not through external dependencies
    public int CurrentDay { get; set; } = 1;
    public TimeBlocks CurrentTimeBlock { get; set; } = TimeBlocks.Morning;
    public List<Venue> Venues { get; set; } = new List<Venue>();
    public List<LocationEntry> Locations { get; set; } = new List<LocationEntry>();
    public List<NPC> NPCs { get; set; } = new List<NPC>();
    public List<LocationAction> LocationActions { get; set; } = new List<LocationAction>();

    // TimeBlock tracking for stranger refresh
    private TimeBlocks _lastTimeBlock = TimeBlocks.Morning;

    // Player stats system for character progression
    public List<PlayerStatDefinition> PlayerStatDefinitions { get; set; } = new List<PlayerStatDefinition>();
    public StatProgression StatProgression { get; set; }

    private Player Player;
    public WorldState WorldState { get; private set; }
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
    public List<SocialCard> PlayerObservationCards { get; set; } = new List<SocialCard>();
    // Exchange definitions loaded from JSON for lookup
    public List<ExchangeDTO> ExchangeDefinitions { get; set; } = new List<ExchangeDTO>();
    // Mental cards for investigation system
    public Dictionary<string, Goal> Goals { get; private set; } = new Dictionary<string, Goal>();
    public List<SocialCard> SocialCards { get; set; } = new List<SocialCard>();
    public List<MentalCard> MentalCards { get; set; } = new List<MentalCard>();
    // Physical cards for physical challenge system
    public List<PhysicalCard> PhysicalCards { get; set; } = new List<PhysicalCard>();

    // THREE PARALLEL TACTICAL SYSTEMS - Decks only (no Types, they're redundant)
    // Decks contain all necessary configuration for tactical engagements
    public Dictionary<string, SocialChallengeDeck> SocialChallengeDecks { get; private set; } = new Dictionary<string, SocialChallengeDeck>();
    public Dictionary<string, MentalChallengeDeck> MentalChallengeDecks { get; private set; } = new Dictionary<string, MentalChallengeDeck>();
    public Dictionary<string, PhysicalChallengeDeck> PhysicalChallengeDecks { get; private set; } = new Dictionary<string, PhysicalChallengeDeck>();

    // Observations from packages
    public List<Observation> Observations { get; set; } = new List<Observation>();

    // Dialogue templates from packages
    public DialogueTemplates DialogueTemplates { get; set; }
    public List<Investigation> Investigations { get; private set; } = new List<Investigation>();
    public InvestigationJournal InvestigationJournal { get; private set; } = new InvestigationJournal();

    // Travel System
    public Dictionary<string, List<RouteImprovement>> RouteImprovements { get; private set; } = new Dictionary<string, List<RouteImprovement>>();
    public List<TravelObstacle> TravelObstacles { get; private set; } = new List<TravelObstacle>();

    // Initialization data - stored in GameWorld, not passed between phases
    public string InitialLocationSpotId { get; set; }
    public PlayerInitialConfig InitialPlayerConfig { get; set; }

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
        WorldState = new WorldState();

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
        return NPCs ?? new List<NPC>();
    }

    /// Get a Venue location by ID from primary storage
    /// </summary>
    public Location GetLocation(string LocationId)
    {
        return Locations.FindById(LocationId)?.location;
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
    public void AddStrangerToLocation(string venueId, NPC stranger)
    {
        if (stranger == null) return;
        stranger.Venue = venueId;
        stranger.IsStranger = true;
        NPCs.Add(stranger);
    }

    /// <summary>
    /// Get available strangers at a Venue for the current time block
    /// </summary>
    public List<NPC> GetAvailableStrangers(string venueId, TimeBlocks currentTimeBlock)
    {
        List<NPC> availableStrangers = new List<NPC>();
        foreach (NPC npc in NPCs)
        {
            if (npc.IsStranger && npc.Venue == venueId && npc.IsAvailableAtTime(currentTimeBlock))
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
        stranger?.MarkAsEncountered();
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
    /// Activate an investigation obligation - mark as active and start deadline tracking
    /// NPCCommissioned: Sets absolute deadline segment and tracks in active obligations
    /// SelfDiscovered: No deadline tracking (freedom-based exploration)
    /// </summary>
    public void ActivateObligation(string investigationId, TimeManager timeManager)
    {
        Investigation investigation = Investigations.FirstOrDefault(i => i.Id == investigationId);
        if (investigation == null) return;

        if (!Player.ActiveObligationIds.Contains(investigationId))
        {
            Player.ActiveObligationIds.Add(investigationId);
        }

        if (investigation.ObligationType == InvestigationObligationType.NPCCommissioned)
        {
            int currentSegment = timeManager.CurrentSegment;
            int deadlineDuration = investigation.DeadlineSegment ?? 0;
            investigation.DeadlineSegment = currentSegment + deadlineDuration;
        }
    }

    /// <summary>
    /// Complete an investigation obligation - apply rewards, chain spawned obligations, clear deadline
    /// Applies coins, items, XP, and increases relationship with patron (NPCCommissioned only)
    /// Chains spawned obligations by activating each spawned investigation
    /// </summary>
    public void CompleteObligation(string investigationId, TimeManager timeManager)
    {
        Investigation investigation = Investigations.FirstOrDefault(i => i.Id == investigationId);
        if (investigation == null) return;

        Player.ModifyCoins(investigation.CompletionRewardCoins);

        foreach (string itemId in investigation.CompletionRewardItems)
        {
            Player.Inventory.AddItem(itemId);
        }

        foreach (StatXPReward xpReward in investigation.CompletionRewardXP)
        {
            Player.Stats.AddXP(xpReward.Stat, xpReward.XPAmount);
        }

        foreach (string spawnedId in investigation.SpawnedObligationIds)
        {
            ActivateObligation(spawnedId, timeManager);
        }

        if (Player.ActiveObligationIds.Contains(investigationId))
        {
            Player.ActiveObligationIds.Remove(investigationId);
        }

        if (investigation.ObligationType == InvestigationObligationType.NPCCommissioned &&
            !string.IsNullOrEmpty(investigation.PatronNpcId))
        {
            NPC patron = NPCs.FirstOrDefault(n => n.ID == investigation.PatronNpcId);
            if (patron != null)
            {
                patron.RelationshipFlow = Math.Min(24, patron.RelationshipFlow + 2);
            }
        }
    }

    /// <summary>
    /// Check for expired obligations by comparing current segment to deadline
    /// Returns list of investigation IDs that have exceeded their deadline
    /// </summary>
    public List<string> CheckDeadlines(int currentSegment)
    {
        List<string> expiredObligations = new List<string>();

        foreach (string obligationId in Player.ActiveObligationIds)
        {
            Investigation investigation = Investigations.FirstOrDefault(i => i.Id == obligationId);
            if (investigation != null &&
                investigation.ObligationType == InvestigationObligationType.NPCCommissioned &&
                investigation.DeadlineSegment.HasValue &&
                currentSegment >= investigation.DeadlineSegment.Value)
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
    public void ApplyDeadlineConsequences(string investigationId)
    {
        Investigation investigation = Investigations.FirstOrDefault(i => i.Id == investigationId);
        if (investigation == null) return;

        if (investigation.ObligationType == InvestigationObligationType.NPCCommissioned &&
            !string.IsNullOrEmpty(investigation.PatronNpcId))
        {
            NPC patron = NPCs.FirstOrDefault(n => n.ID == investigation.PatronNpcId);
            if (patron != null)
            {
                patron.RelationshipFlow = Math.Max(0, patron.RelationshipFlow - 3);

                int cubeReduction = Math.Min(2, patron.StoryCubes);
                patron.StoryCubes = Math.Max(0, patron.StoryCubes - cubeReduction);
            }
        }

        if (Player.ActiveObligationIds.Contains(investigationId))
        {
            Player.ActiveObligationIds.Remove(investigationId);
        }

        investigation.IsFailed = true;
    }

    /// <summary>
    /// Get all active obligations for player
    /// </summary>
    public List<Investigation> GetActiveObligations()
    {
        List<Investigation> activeObligations = new List<Investigation>();
        foreach (string obligationId in Player.ActiveObligationIds)
        {
            Investigation investigation = Investigations.FirstOrDefault(i => i.Id == obligationId);
            if (investigation != null)
            {
                activeObligations.Add(investigation);
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
        return location?.InvestigationCubes ?? 0;
    }

    /// <summary>
    /// Get StoryCubes for an NPC (0-10 scale)
    /// </summary>
    public int GetNPCCubes(string npcId)
    {
        NPC npc = NPCs.FirstOrDefault(n => n.ID == npcId);
        return npc?.StoryCubes ?? 0;
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
