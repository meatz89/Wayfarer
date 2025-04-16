public class GameState
{
    public Modes GameMode = Modes.Tutorial;
    public PlayerState PlayerState { get; set; }
    public ActionStateTracker ActionStateTracker { get; }
    public WorldState WorldState { get; }
    public PendingTravel PendingTravel { get; set; }
    public TimeManager TimeManager { get; set; }

    public GameState()
    {
        PlayerState = new PlayerState();
        ActionStateTracker = new ActionStateTracker();
        WorldState = new WorldState();
        PendingTravel = new PendingTravel();
        TimeManager = new TimeManager(WorldState);
    }

    public List<UserActionOption> GetActions(LocationSpot locationSpot)
    {
        List<UserActionOption> locationActions =
            ActionStateTracker.LocationSpotActions
            .Where(x => x.Location == locationSpot.LocationName)
            .Where(x => x.LocationSpot == locationSpot.Name)
            .ToList();

        List<UserActionOption> actions = new List<UserActionOption>();
        actions.AddRange(locationActions);

        return actions;
    }
}
public enum Modes
{
    Tutorial,
    Live
}