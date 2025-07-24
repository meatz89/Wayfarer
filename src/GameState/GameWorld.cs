using System;

public class GameWorld
{
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

    public Location CurrentLocation
    {
        get
        {
            return Player?.CurrentLocation;
        }

        private set
        {
            if (Player != null)
                Player.CurrentLocation = value;
        }
    }
    public LocationSpot CurrentLocationSpot
    {
        get
        {
            return Player?.CurrentLocationSpot;
        }

        set
        {
            if (Player != null)
                Player.CurrentLocationSpot = value;
        }
    }

    private Player Player;
    public WorldState WorldState { get; private set; }
    public StreamingContentState StreamingContentState { get; private set; }

    // New journey-related properties
    public WorldMap Map { get; private set; }
    public int GlobalTime { get; set; }
    public List<Location> DiscoveredLocations { get; set; }
    public List<RouteOption> DiscoveredRoutes { get; set; }

    // New resource properties
    public int Money { get; set; }
    public int Condition { get; set; }

    public AIResponse CurrentAIResponse { get; set; }
    public bool IsAwaitingAIResponse { get; set; }
    public List<ConversationChoice> AvailableChoices { get; set; } = new List<ConversationChoice>();

    public int DeadlineDay { get; set; }
    public string DeadlineReason { get; set; }
    public Guid GameInstanceId { get; set; }
    public RouteOption CurrentRouteOption { get; internal set; }

    // System Messages State
    public List<SystemMessage> SystemMessages { get; set; } = new List<SystemMessage>();
    // Event Log - Permanent record of all messages
    public List<SystemMessage> EventLog { get; set; } = new List<SystemMessage>();

    // Pending command for any command that doesn't complete instantly
    public PendingCommand PendingCommand { get; set; }

    // Action-Conversation State
    public ConversationManager PendingConversationManager { get; set; }
    public bool ConversationPending { get; set; }

    // Narrative System
    public FlagService FlagService { get; set; }
    public NarrativeManager NarrativeManager { get; set; }

    // Injected by GameWorldInitializer
    public CommandDiscoveryService CommandDiscoveryService { get; internal set; }
    public IServiceProvider ServiceProvider { get; internal set; }

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

    public bool IsDeadlineReached()
    {
        return CurrentDay >= DeadlineDay;
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

}



