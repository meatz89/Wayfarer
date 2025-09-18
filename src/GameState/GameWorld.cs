using System;
using System.Collections.Generic;
using System.Linq;

// Player initial configuration data
public class PlayerInitialConfig
{
    public int? Coins { get; set; }
    public int? StaminaPoints { get; set; }
    public int? MaxStamina { get; set; }
    public int? Health { get; set; }
    public int? MaxHealth { get; set; }
    public int? Hunger { get; set; }
    public int? MaxHunger { get; set; }
    public string Personality { get; set; }
    public string Archetype { get; set; }
    public Dictionary<string, int> InitialItems { get; set; }
}

public class GameWorld
{
    // Game mode determines content loading and tutorial state
    public GameMode GameMode { get; set; } = GameMode.MainGame;

    // Time is now tracked in WorldState, not through external dependencies
    public int CurrentDay { get; set; } = 1;
    public TimeBlocks CurrentTimeBlock { get; set; } = TimeBlocks.Morning;
    public WeatherCondition CurrentWeather
    {
        get
        {
            return WorldState.CurrentWeather;
        }

        set
        {
            WorldState.CurrentWeather = value;
        }
    }
    // Player state is accessed through Player object, not duplicated here
    public Inventory PlayerInventory { get; private set; }
    public List<Location> Locations { get; set; } = new List<Location>();
    public Dictionary<string, LocationSpot> Spots { get; set; } = new Dictionary<string, LocationSpot>();
    public List<NPC> NPCs { get; set; } = new List<NPC>();
    public List<LocationAction> LocationActions { get; set; } = new List<LocationAction>();

    // Stranger NPC system for practice conversations
    public Dictionary<string, List<StrangerNPC>> LocationStrangers { get; set; } = new Dictionary<string, List<StrangerNPC>>();
    private TimeBlock _lastTimeBlock = TimeBlock.Dawn;

    // Player stats system for character progression
    public List<PlayerStatDefinition> PlayerStatDefinitions { get; set; } = new List<PlayerStatDefinition>();
    public StatProgression StatProgression { get; set; }

    private Player Player;
    public WorldState WorldState { get; private set; }
    public StreamingContentState StreamingContentState { get; private set; }

    public int DeadlineDay { get; set; }
    public string DeadlineReason { get; set; }
    public Guid GameInstanceId { get; set; }
    public RouteOption CurrentRouteOption { get; internal set; }

    // System Messages State
    public List<SystemMessage> SystemMessages { get; set; } = new List<SystemMessage>();
    // Event Log - Permanent record of all messages
    public List<SystemMessage> EventLog { get; set; } = new List<SystemMessage>();
    // DeliveryObligation positioning messages for UI translation
    public List<LetterPositioningMessage> LetterPositioningMessages { get; set; } = new List<LetterPositioningMessage>();
    // Special letter events for UI translation
    public List<SpecialLetterEvent> SpecialLetterEvents { get; set; } = new List<SpecialLetterEvent>();

    public Dictionary<PersonalityType, PersonalityCardMapping> PersonalityMappings { get; set; } = new Dictionary<PersonalityType, PersonalityCardMapping>();
    public Dictionary<int, List<string>> TokenUnlocks { get; set; } = new Dictionary<int, List<string>>();

    // DECK ARCHITECTURE - Single source of truth for all deck configurations
    // All cards are ConversationCard type (no LetterCard, ExchangeCard, etc.)
    public Dictionary<string, ConversationCard> AllCardDefinitions { get; set; } = new Dictionary<string, ConversationCard>();
    // Conversation type definitions and card decks - fully extensible via JSON
    public Dictionary<string, ConversationTypeDefinition> ConversationTypes { get; set; } = new Dictionary<string, ConversationTypeDefinition>();
    public Dictionary<string, CardDeckDefinition> CardDecks { get; set; } = new Dictionary<string, CardDeckDefinition>();
    // Exchange cards are now completely separate from conversation cards
    public Dictionary<string, List<ExchangeCard>> NPCExchangeCards { get; set; } = new Dictionary<string, List<ExchangeCard>>();
    public List<ConversationCard> PlayerObservationCards { get; set; } = new List<ConversationCard>();
    // Exchange definitions loaded from JSON for lookup
    public List<ExchangeDTO> ExchangeDefinitions { get; set; } = new List<ExchangeDTO>();

    // Observations from packages
    public List<Observation> Observations { get; set; } = new List<Observation>();

    // Dialogue templates from packages
    public DialogueTemplates DialogueTemplates { get; set; }

    // Initialization data - stored in GameWorld, not passed between phases
    // This eliminates the need for SharedData dictionary
    public string InitialLocationSpotId { get; set; }
    public PlayerInitialConfig InitialPlayerConfig { get; set; }

    // Note: Pending command system has been removed in favor of intent-based architecture

    // Strongly typed pending queue state (replaces unsafe metadata dictionary)
    public PendingQueueState PendingQueueState { get; private set; } = new PendingQueueState();

    // Endless mode flag for post-30 day gameplay
    public bool EndlessMode { get; set; } = false;

    // Skeleton tracking for lazy content resolution
    public Dictionary<string, string> SkeletonRegistry { get; set; } = new Dictionary<string, string>();

    // Track if game has been started to prevent duplicate initialization
    public bool IsGameStarted { get; set; } = false;

    // PATH CARD SYSTEM - Travel path card discovery mechanics
    // Path cards are now stored in collections (AllPathCollections)

    // Persistent discovery states
    public Dictionary<string, bool> PathCardDiscoveries { get; set; } = new Dictionary<string, bool>();

    // Track one-time rewards
    public Dictionary<string, bool> PathCardRewardsClaimed { get; set; } = new Dictionary<string, bool>();

    // Track event deck positions for deterministic draws
    public Dictionary<string, int> EventDeckPositions { get; set; } = new Dictionary<string, int>();

    // Active travel session
    public TravelSession CurrentTravelSession { get; set; }

    // PATH SYSTEM - For FixedPath segments that always show the same cards
    // Path card collections for FixedPath route segments (collections contain the actual cards)
    public Dictionary<string, PathCardCollectionDTO> AllPathCollections { get; set; } = new Dictionary<string, PathCardCollectionDTO>();

    // EVENT SYSTEM - For Event segments that randomly select from a pool
    // Travel events containing narrative and card references
    public Dictionary<string, TravelEventDTO> AllTravelEvents { get; set; } = new Dictionary<string, TravelEventDTO>();

    // Event collections for Event route segments (containing eventIds, not pathCardIds)
    public Dictionary<string, PathCardCollectionDTO> AllEventCollections { get; set; } = new Dictionary<string, PathCardCollectionDTO>();

    /// <summary>
    /// Get a report of all skeletons that need to be populated
    /// </summary>
    public List<string> GetSkeletonReport()
    {
        List<string> report = new List<string>();
        foreach (KeyValuePair<string, string> kvp in SkeletonRegistry)
        {
            report.Add($"{kvp.Value}: {kvp.Key}");
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
            attention: Player.Attention,
            maxStamina: Player.MaxStamina,
            maxHealth: Player.MaxHealth,
            maxHunger: Player.MaxHunger,
            maxAttention: Player.MaxAttention
        );
    }


    /// <summary>
    /// Get all NPCs in the game world
    /// </summary>
    public List<NPC> GetAllNPCs()
    {
        return NPCs ?? new List<NPC>();
    }

    /// Get a location spot by ID from primary storage
    /// </summary>
    public LocationSpot GetSpot(string spotId)
    {
        return Spots.TryGetValue(spotId, out LocationSpot? spot) ? spot : null;
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
    public void AddStrangerToLocation(string locationId, StrangerNPC stranger)
    {
        if (!LocationStrangers.ContainsKey(locationId))
        {
            LocationStrangers[locationId] = new List<StrangerNPC>();
        }
        LocationStrangers[locationId].Add(stranger);
    }

    /// <summary>
    /// Get available strangers at a location for the current time block
    /// </summary>
    public List<StrangerNPC> GetAvailableStrangers(string locationId, TimeBlock currentTimeBlock)
    {
        if (!LocationStrangers.ContainsKey(locationId))
            return new List<StrangerNPC>();

        return LocationStrangers[locationId]
            .Where(s => s.IsAvailableAtTime(currentTimeBlock) && !s.HasBeenTalkedTo)
            .ToList();
    }

    /// <summary>
    /// Get stranger by ID across all locations
    /// </summary>
    public StrangerNPC GetStrangerById(string strangerId)
    {
        foreach (List<StrangerNPC> locationStrangers in LocationStrangers.Values)
        {
            StrangerNPC? stranger = locationStrangers.FirstOrDefault(s => s.Id == strangerId);
            if (stranger != null)
                return stranger;
        }
        return null;
    }

    /// <summary>
    /// Refresh all strangers when time block changes
    /// </summary>
    public void RefreshStrangersForTimeBlock(TimeBlock newTimeBlock)
    {
        if (_lastTimeBlock != newTimeBlock)
        {
            foreach (List<StrangerNPC> locationStrangers in LocationStrangers.Values)
            {
                foreach (StrangerNPC stranger in locationStrangers)
                {
                    stranger.RefreshForNewTimeBlock();
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
        StrangerNPC stranger = GetStrangerById(strangerId);
        stranger?.MarkAsTalkedTo();
    }

    /// <summary>
    /// Get all strangers across all locations (for debugging/admin)
    /// </summary>
    public List<StrangerNPC> GetAllStrangers()
    {
        List<StrangerNPC> allStrangers = new List<StrangerNPC>();
        foreach (List<StrangerNPC> locationStrangers in LocationStrangers.Values)
        {
            allStrangers.AddRange(locationStrangers);
        }
        return allStrangers;
    }

    /// <summary>
    /// Convert TimeBlocks enum to TimeBlock enum
    /// </summary>
    private TimeBlock ConvertTimeBlocks(TimeBlocks timeBlocks)
    {
        return timeBlocks switch
        {
            TimeBlocks.Dawn => TimeBlock.Dawn,
            TimeBlocks.Morning => TimeBlock.Morning,
            TimeBlocks.Midday => TimeBlock.Midday,
            TimeBlocks.Afternoon => TimeBlock.Afternoon,
            TimeBlocks.Evening => TimeBlock.Evening,
            TimeBlocks.Night => TimeBlock.Night,
            _ => TimeBlock.Morning
        };
    }

    /// <summary>
    /// Get current time block as TimeBlock enum
    /// </summary>
    public TimeBlock GetCurrentTimeBlock()
    {
        return ConvertTimeBlocks(CurrentTimeBlock);
    }

}
