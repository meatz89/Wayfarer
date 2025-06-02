public class GameWorld
{
    public static int CurrentDay { get; }
    public static TimeOfDay CurrentTimeOfDay { get; private set; }
    public static List<Opportunity> AllOpportunities { get; set; }

    public Player Player { get; set; }
    public ActionStateTracker ActionStateTracker { get; }
    public WorldState WorldState { get; }
    public TimeManager TimeManager { get; set; }
    public Location CurrentLocation { get; set; }
    public int DeadlineDay { get; set; }
    public string DeadlineReason { get; set; }
    public EncounterManager CurrentEncounterContext { get; set; }

    public GameWorld()
    {
        Player = new Player();
        ActionStateTracker = new ActionStateTracker();
        WorldState = new WorldState();
        TimeManager = new TimeManager(Player, WorldState);
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
        throw new NotImplementedException();
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