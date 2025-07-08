public class GameWorld
{
    public int CurrentDay { get; set; } = 1;
    public TimeBlocks CurrentTimeBlock { get; set; } = TimeBlocks.Dawn;
    public int PlayerCoins { get; set; } = 2;
    public int PlayerStamina { get; set; } = 5;
    public Inventory PlayerInventory { get; private set; }
    public List<Contract> ActiveContracts { get; set; } = new List<Contract>();
    public List<Location> Locations { get; set; } = new List<Location>();

    public Location CurrentLocation { get; private set; }

    private Player Player;
    public WorldState WorldState { get; private set; }
    public ActionStateTracker ActionStateTracker { get; private set; }
    public StreamingContentState StreamingContentState { get; private set; }

    // New journey-related properties
    public WorldMap Map { get; private set; }
    public int GlobalTime { get; set; }
    public List<Location> DiscoveredLocations { get; set; }
    public List<RouteOption> DiscoveredRoutes { get; set; }

    // New resource properties
    public int Money { get; set; }
    public int Condition { get; set; }
    public static List<Contract> AllContracts { get; set; } = new List<Contract>();

    public AIResponse CurrentAIResponse { get; set; }
    public bool IsAwaitingAIResponse { get; set; }
    public List<EncounterChoice> AvailableChoices { get; set; } = new List<EncounterChoice>();
    
    public int DeadlineDay { get; set; }
    public string DeadlineReason { get; set; }
    public Guid GameInstanceId { get; set; }
    public RouteOption CurrentRouteOption { get; internal set; }
    public TimeManager TimeManager { get; internal set; }

    public GameWorld()
    {
        if(GameInstanceId == Guid.Empty) GameInstanceId = Guid.NewGuid();

        Player = new Player();
        WorldState = new WorldState();

        TimeManager = new TimeManager(Player, WorldState);

        ActionStateTracker = new ActionStateTracker();
        StreamingContentState = new StreamingContentState();

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
}



