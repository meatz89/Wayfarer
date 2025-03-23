public class GameState
{
    public Modes GameMode = Modes.Debug;

    public PlayerState PlayerState { get; set; }
    public ActionState Actions { get; }
    public WorldState WorldState { get; }

    public GameState()
    {
        PlayerState = new PlayerState();
        Actions = new ActionState();
        WorldState = new WorldState();
    }

    public List<UserActionOption> GetActions(LocationSpot locationSpot)
    {
        List<UserActionOption> locationActions =
            Actions.LocationSpotActions
            .Where(x => x.Location == locationSpot.LocationName)
            .Where(x => x.LocationSpot == locationSpot.Name)
            .ToList();

        List<UserActionOption> characterActions =
            Actions.CharacterActions
            .Where(x => x.Location == locationSpot.LocationName)
            .Where(x => x.LocationSpot == locationSpot.Name)
            .ToList();

        List<UserActionOption> questActions =
            Actions.QuestActions
            .Where(x => x.Location == locationSpot.LocationName)
            .Where(x => x.LocationSpot == locationSpot.Name)
            .ToList();

        List<UserActionOption> actions = new List<UserActionOption>();
        actions.AddRange(locationActions);
        actions.AddRange(characterActions);
        actions.AddRange(questActions);

        return actions;
    }
}
public enum Modes
{
    Debug,
    Live
}