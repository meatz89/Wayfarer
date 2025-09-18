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
    public List<ResourceEntry> InitialItems { get; set; }
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
    public List<LocationSpotEntry> Spots { get; set; } = new List<LocationSpotEntry>();
    public List<NPC> NPCs { get; set; } = new List<NPC>();
    public List<LocationAction> LocationActions { get; set; } = new List<LocationAction>();

    // TimeBlock tracking for stranger refresh
    private TimeBlocks _lastTimeBlock = TimeBlocks.Dawn;

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

    public List<PersonalityMappingEntry> PersonalityMappings { get; set; } = new List<PersonalityMappingEntry>();
    public List<TokenUnlockEntry> TokenUnlocks { get; set; } = new List<TokenUnlockEntry>();

    // DECK ARCHITECTURE - Single source of truth for all deck configurations
    // All cards are ConversationCard type (no LetterCard, ExchangeCard, etc.)
    public List<CardDefinitionEntry> AllCardDefinitions { get; set; } = new List<CardDefinitionEntry>();
    // Conversation type definitions and card decks - fully extensible via JSON
    public List<ConversationTypeEntry> ConversationTypes { get; set; } = new List<ConversationTypeEntry>();
    public List<CardDeckDefinitionEntry> CardDecks { get; set; } = new List<CardDeckDefinitionEntry>();
    // Exchange cards are now completely separate from conversation cards
    public List<NPCExchangeCardEntry> NPCExchangeCards { get; set; } = new List<NPCExchangeCardEntry>();
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
    public List<SkeletonRegistryEntry> SkeletonRegistry { get; set; } = new List<SkeletonRegistryEntry>();

    // Track if game has been started to prevent duplicate initialization
    public bool IsGameStarted { get; set; } = false;

    // PATH CARD SYSTEM - Travel path card discovery mechanics
    // Path cards are now stored in collections (AllPathCollections)

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
        return Spots.FindById(spotId)?.Spot;
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
        stranger.Location = locationId;
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
            if (npc.IsStranger && npc.Location == locationId && npc.IsAvailableAtTime(currentTimeBlock))
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

    /// <summary>
    /// Convert TimeBlocks enum (no longer needed as we unified to TimeBlocks)
    /// </summary>
    private TimeBlocks ConvertTimeBlocks(TimeBlocks timeBlocks)
    {
        return timeBlocks switch
        {
            TimeBlocks.Dawn => TimeBlocks.Dawn,
            TimeBlocks.Morning => TimeBlocks.Morning,
            TimeBlocks.Midday => TimeBlocks.Midday,
            TimeBlocks.Afternoon => TimeBlocks.Afternoon,
            TimeBlocks.Evening => TimeBlocks.Evening,
            TimeBlocks.Night => TimeBlocks.Night,
            _ => TimeBlocks.Morning
        };
    }

    /// <summary>
    /// Get current time block as TimeBlock enum
    /// </summary>
    public TimeBlocks GetCurrentTimeBlock()
    {
        return ConvertTimeBlocks(CurrentTimeBlock);
    }

}
