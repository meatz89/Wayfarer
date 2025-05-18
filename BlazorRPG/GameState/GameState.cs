public class GameState
{
    public PlayerState PlayerState { get; set; }
    public ActionStateTracker ActionStateTracker { get; }
    public WorldState WorldState { get; }
    public TimeManager TimeManager { get; set; }

    public GameState()
    {
        PlayerState = new PlayerState();
        ActionStateTracker = new ActionStateTracker();
        WorldState = new WorldState();
        TimeManager = new TimeManager(PlayerState, WorldState);
    }

    public void SetCurrentLocation(Location location, LocationSpot locationSpot)
    {
        WorldState.SetCurrentLocation(location, locationSpot);
        WorldState.SetCurrentLocationSpot(locationSpot);
        PlayerState.CurrentLocation = location;
        PlayerState.CurrentLocationSpot = locationSpot;
    }
}