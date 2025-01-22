public class GameState
{
    public PlayerState Player { get; set; }
    public ActionState Actions { get; }
    public WorldState World { get; }
    public Modes Mode = Modes.Debug;

    public GameState()
    {
        Player = new PlayerState();
        Actions = new ActionState();
        World = new WorldState();
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