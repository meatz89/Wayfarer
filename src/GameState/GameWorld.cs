using System;
using System.Collections.Generic;


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

    CurrentAIResponse = null;
    IsAwaitingAIResponse = false;
}

public Player GetPlayer()
{
    return Player;
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

}
