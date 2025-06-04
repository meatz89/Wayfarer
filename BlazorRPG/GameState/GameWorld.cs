public class GameWorld
{
    // Existing properties
    public Player Player { get; private set; }
    public StreamingContentState StreamingContentState { get; private set; }

    // New journey-related properties
    public WorldMap Map { get; private set; }
    public Location CurrentLocation { get; private set; }
    public int GlobalTime { get; private set; }
    public List<Location> DiscoveredLocations { get; private set; }
    public List<Route> DiscoveredRoutes { get; private set; }

    // New resource properties
    public int Money { get; set; }
    public int Condition { get; set; }
    public Inventory PlayerInventory { get; private set; }

    public static int CurrentDay { get; }
    public static TimeOfDay CurrentTimeOfDay { get; private set; }
    public static List<Opportunity> AllOpportunities { get; set; } = new List<Opportunity>();

    public AIResponse CurrentAIResponse { get; set; }
    public bool IsAwaitingAIResponse { get; set; }
    public List<EncounterChoice> AvailableChoices { get; set; } = new List<EncounterChoice>();

    public ActionStateTracker ActionStateTracker { get; }
    public WorldState WorldState { get; }
    public TimeManager TimeManager { get; set; }
    public int DeadlineDay { get; set; }
    public string DeadlineReason { get; set; }

    public GameWorld()
    {
        Player = new Player();
        ActionStateTracker = new ActionStateTracker();
        WorldState = new WorldState();
        TimeManager = new TimeManager(Player, WorldState);

        StreamingContentState = new StreamingContentState();
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

    public static void AdvanceTime(TimeSpan timeSpan)
    {
        CurrentTimeOfDay = CurrentTimeOfDay.Advance(timeSpan);
    }

    public bool IsDeadlineReached()
    {
        return CurrentDay >= DeadlineDay;
    }

    public Player GetPlayer()
    {
        return Player;
    }

    public void ModifyRelationship(object targetNPC, object magnitude)
    {
        throw new NotImplementedException();
    }

    public void RevealLocation(object locationID)
    {
        throw new NotImplementedException();
    }

    public NPC GetCharacter(object targetID)
    {
        throw new NotImplementedException();
    }

    public List<Goal> GetGoalsByType(object core)
    {
        return new List<Goal>();
    }

    public List<TravelRoute> GetRoutesFromCurrentLocation()
    {
        string currentLocationName = CurrentLocation.Name;

        if (Player.KnownRoutes.ContainsKey(currentLocationName))
        {
            return Player.KnownRoutes[currentLocationName];
        }

        return new List<TravelRoute>();
    }

}