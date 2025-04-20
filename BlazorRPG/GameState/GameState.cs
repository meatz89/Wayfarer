public class GameState
{
    public Modes GameMode = Modes.Live;
    public PlayerState PlayerState { get; set; }
    public ActionStateTracker ActionStateTracker { get; }
    public WorldState WorldState { get; }
    public TimeManager TimeManager { get; set; }

    public GameState()
    {
        PlayerState = new PlayerState();
        ActionStateTracker = new ActionStateTracker();
        WorldState = new WorldState();
        TimeManager = new TimeManager(WorldState);
    }

    public List<UserActionOption> GetActions()
    {
        return ActionStateTracker.LocationSpotActions;
    }
}
public enum Modes
{
    Live
}