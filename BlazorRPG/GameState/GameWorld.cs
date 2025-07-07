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
    public int GlobalTime { get; private set; }
    public List<Location> DiscoveredLocations { get; private set; }
    public List<RouteOption> DiscoveredRoutes { get; private set; }

    // New resource properties
    public int Money { get; set; }
    public int Condition { get; set; }
    public static List<Contract> AllContracts { get; set; } = new List<Contract>();

    public AIResponse CurrentAIResponse { get; set; }
    public bool IsAwaitingAIResponse { get; set; }
    public List<EncounterChoice> AvailableChoices { get; set; } = new List<EncounterChoice>();

    public TimeManager TimeManager { get; set; }
    public int DeadlineDay { get; set; }
    public string DeadlineReason { get; set; }
    public Guid GameInstanceId { get; set; }
    public RouteOption CurrentRouteOption { get; internal set; }

    public GameWorld()
    {
        if(GameInstanceId == Guid.Empty) GameInstanceId = Guid.NewGuid();

        Player = new Player();
        WorldState = new WorldState();
        ActionStateTracker = new ActionStateTracker();
        StreamingContentState = new StreamingContentState();
        TimeManager = new TimeManager(Player, WorldState);

        CurrentAIResponse = null;
        IsAwaitingAIResponse = false;
    }

    public void StartEncounter(EncounterManager encounterManager)
    {
        ActionStateTracker.SetActiveEncounter(encounterManager);
    }

    public void EndEncounter()
    {
        ActionStateTracker.EndEncounter();
        CurrentAIResponse = null;
        IsAwaitingAIResponse = false;
    }

    public void SetCurrentLocation(Location location)
    {
        WorldState.SetCurrentLocation(location, null);
        Player.CurrentLocation = location;
        Player.CurrentLocationSpot = null;
        CurrentLocation = location;
    }

    public void SetCurrentLocation(Location location, LocationSpot locationSpot)
    {
        WorldState.SetCurrentLocation(location, locationSpot);
        WorldState.SetCurrentLocationSpot(locationSpot);
        Player.CurrentLocation = location;
        Player.CurrentLocationSpot = locationSpot;
    }

    public void AdvanceTime(int duration)
    {
        TimeManager.AdvanceTime(duration);
        WorldState.CurrentTimeWindow = TimeManager.GetCurrentTimeWindow();
        WorldState.CurrentTimeHours = TimeManager.GetCurrentHour();
    }

    public bool IsDeadlineReached()
    {
        return CurrentDay >= DeadlineDay;
    }

    public Player GetPlayer()
    {
        return Player;
    }

    public List<RouteOption> GetRoutesFromCurrentLocation()
    {
        string currentLocationName = CurrentLocation.Name;

        if (Player.KnownRoutes.ContainsKey(currentLocationName))
        {
            return Player.KnownRoutes[currentLocationName];
        }

        return new List<RouteOption>();
    }

    public Guid GetGameInstanceId()
    {
        return GameInstanceId;
    }

    public RouteOption GetRouteOption(string travelLocationName)
    {
        List<LocationConnection> connections = CurrentLocation.Connections;

        foreach (var connection in connections)
        {
            RouteOption? routeOption = connection.RouteOptions.FirstOrDefault(r =>
                r.Destination.Equals(travelLocationName, StringComparison.OrdinalIgnoreCase));
            if (routeOption != null)
            {
                return routeOption;
            }
        }
        return null;
    }
}



