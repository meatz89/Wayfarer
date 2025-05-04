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
}