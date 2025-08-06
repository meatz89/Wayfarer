using System;

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

    public AIResponse CurrentAIResponse { get; set; }
    public bool IsAwaitingAIResponse { get; set; }
    public int DeadlineDay { get; set; }
    public string DeadlineReason { get; set; }
    public Guid GameInstanceId { get; set; }
    public RouteOption CurrentRouteOption { get; internal set; }

    // System Messages State
    public List<SystemMessage> SystemMessages { get; set; } = new List<SystemMessage>();
    // Event Log - Permanent record of all messages
    public List<SystemMessage> EventLog { get; set; } = new List<SystemMessage>();

    // Note: Pending command system has been removed in favor of intent-based architecture

    // Temporary metadata for conversation context
    private Dictionary<string, string> _metadata = new Dictionary<string, string>();

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

    // Metadata management for conversation context
    public void SetMetadata(string key, string value)
    {
        _metadata[key] = value;
    }

    public string GetMetadata(string key)
    {
        return _metadata.TryGetValue(key, out string? value) ? value : null;
    }

    public void ClearMetadata(string key)
    {
        _metadata.Remove(key);
    }
    
    // Time management methods
    public void AdvanceToNextDay()
    {
        CurrentDay++;
        CurrentTimeBlock = TimeBlocks.Dawn;
    }

}



