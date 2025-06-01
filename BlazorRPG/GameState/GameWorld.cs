public class GameWorld
{
    public Player Player { get; set; }
    public ActionStateTracker ActionStateTracker { get; }
    public WorldState WorldState { get; }
    public TimeManager TimeManager { get; set; }

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
}