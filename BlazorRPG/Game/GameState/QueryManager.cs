
public class QueryManager
{
    private readonly GameState gameState;
    private readonly LocationSystem locationSystem;

    public QueryManager(GameState gameState, LocationSystem locationSystem)
    {
        this.gameState = gameState;
        this.locationSystem = locationSystem;
    }

    public List<PlayerAction> GetGlobalActions()
    {
        List<PlayerAction> actions = new List<PlayerAction>();
        actions.Add(new PlayerAction()
        {
            ActionType = BasicActionTypes.CheckStatus,
            Description = "[Player] Check Status"
        });
        actions.Add(new PlayerAction()
        {
            ActionType = BasicActionTypes.Travel,
            Description = "[Player] Travel"
        });

        return actions;
    }

    public List<PlayerAction> GetLocationActions()
    {
        LocationNames currentLocation = gameState.CurrentLocation;
        List<BasicActionTypes> locationActions = locationSystem.GetLocationActionsFor(currentLocation);

        List<PlayerAction> actions = new List<PlayerAction>();
        if (locationActions.Count > 0)
        {
            foreach (BasicActionTypes locationAction in locationActions)
            {
                actions.Add(new PlayerAction()
                {
                    ActionType = locationAction,
                    Description = $"[{currentLocation}] {locationAction}"
                });
            }
        }
        return actions;
    }

    public List<PlayerAction> GetCharacterActions()
    {
        List<PlayerAction> actions = new List<PlayerAction>();

        return actions;
    }


    public List<LocationNames> GetConnectedLocations()
    {
        Location location = FindLocation(gameState.CurrentLocation);

        List<LocationNames> loc = location.ConnectedLocations;
        return loc;
    }

    private Location FindLocation(LocationNames locationName)
    {
        return gameState.Locations.Where(x => x.Name == locationName).FirstOrDefault();
    }

    public LocationNames GetCurrentLocation()
    {
        return gameState.CurrentLocation;
    }
}
