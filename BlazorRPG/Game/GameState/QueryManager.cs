

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
            Action = new BasicAction() { ActionType = BasicActionTypes.CheckStatus },
            Description = "(System) Check Status"
        });
        actions.Add(new PlayerAction()
        {
            Action = new BasicAction() { ActionType = BasicActionTypes.Travel },
            Description = "(System) Travel"
        });
        actions.Add(new PlayerAction()
        {
            Action = new BasicAction() { ActionType = BasicActionTypes.Wait },
            Description = "(System) Wait"
        });

        return actions;
    }

    public List<PlayerAction> GetLocationActions()
    {
        LocationNames currentLocation = gameState.CurrentLocation;

        // Add Location Action
        List<BasicAction> locationActions = locationSystem.GetActionsForLocation(currentLocation);

        List<PlayerAction> actions = new List<PlayerAction>();
        if (locationActions.Count > 0)
        {
            foreach (BasicAction locationAction in locationActions)
            {
                actions.Add(new PlayerAction()
                {
                    Action = locationAction,
                    Description = $"[{locationAction.ActionType.ToString()}] {locationAction.Description}"
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

    public TimeWindows GetCurrentTime()
    {
        return gameState.CurrentTime;
    }
}
