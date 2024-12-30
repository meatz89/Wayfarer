
public class LocationSystem
{
    private readonly GameState gameState;
    private readonly List<LocationProperties> allLocationProperties;

    public LocationSystem(GameState gameState, GameContentProvider contentProvider)
    {
        this.gameState = gameState;
        this.allLocationProperties = contentProvider.GetLocationProperties();
    }

    public List<BasicAction> GetActionsForLocation(LocationNames location)
    {
        LocationProperties locationProperties = allLocationProperties.FirstOrDefault(x => x.Location == location);
        if (locationProperties == null) return null;

        List<BasicAction> actions = locationProperties.Actions;
        return actions;
    }

}
