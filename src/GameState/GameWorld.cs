using System;
using System.Collections.Generic;

// Player initial configuration data
public class PlayerInitialConfig
{
    public int? Coins { get; set; }
    public int? StaminaPoints { get; set; }
    public int? MaxStamina { get; set; }
    public int? Health { get; set; }
    public int? MaxHealth { get; set; }
    public int? Food { get; set; }
    public int? MaxFood { get; set; }
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
public int PlayerCoins { get; set; } = 2;
public int PlayerStamina { get; set; } = 5;
public Inventory PlayerInventory { get; private set; }
public List<Location> Locations { get; set; } = new List<Location>();
public List<NPC> NPCs { get; set; } = new List<NPC>();

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
public Dictionary<string, List<string>> NPCConversationDeckMappings { get; set; } = new Dictionary<string, List<string>>();
public Dictionary<string, List<ConversationCard>> NPCGoalDecks { get; set; } = new Dictionary<string, List<ConversationCard>>();
public Dictionary<string, List<ConversationCard>> NPCExchangeDecks { get; set; } = new Dictionary<string, List<ConversationCard>>();
public List<ConversationCard> PlayerObservationCards { get; set; } = new List<ConversationCard>();
public List<TravelCard> TravelCards { get; set; } = new List<TravelCard>();

// Initialization data - stored in GameWorld, not passed between phases
// This eliminates the need for SharedData dictionary
public string InitialLocationId { get; set; }
public string InitialLocationSpotId { get; set; }
public PlayerInitialConfig InitialPlayerConfig { get; set; }

// Note: Pending command system has been removed in favor of intent-based architecture

// Strongly typed pending queue state (replaces unsafe metadata dictionary)
public PendingQueueState PendingQueueState { get; private set; } = new PendingQueueState();

// Endless mode flag for post-30 day gameplay
public bool EndlessMode { get; set; } = false;

public GameWorld()
{
    if (GameInstanceId == Guid.Empty) GameInstanceId = Guid.NewGuid();

    Player = new Player();
    WorldState = new WorldState();

    // GameWorld has NO dependencies and creates NO managers

    StreamingContentState = new StreamingContentState();

    // FlagService and NarrativeManager are created by DI, not GameWorld
}

public Player GetPlayer()
{
    return Player;
}

public PlayerResourceState GetPlayerResourceState()
{
    // Create a resource state from current player values
    return new PlayerResourceState(
        coins: PlayerCoins,
        stamina: PlayerStamina,
        health: 10, // Default health for now
        concentration: 10 // Default concentration for now
    );
}

public Guid GetGameInstanceId()
{
    return GameInstanceId;
}


// Time management methods
public void AdvanceToNextDay()
{
    CurrentDay++;
    CurrentTimeBlock = TimeBlocks.Dawn;
}

// Endless mode management
public void SetEndlessMode(bool enabled)
{
    EndlessMode = enabled;
}

public bool IsEndlessModeActive()
{
    return EndlessMode;
}

/// <summary>
/// Get domain tags from the current location spot
/// </summary>
public List<string> GetCurrentSpotDomainTags()
{
    var player = GetPlayer();
    if (player?.CurrentLocationSpot?.DomainTags == null)
        return new List<string>();
    
    return new List<string>(player.CurrentLocationSpot.DomainTags);
}

}
