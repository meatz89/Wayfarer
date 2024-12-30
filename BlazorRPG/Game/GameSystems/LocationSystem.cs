
public class LocationSystem
{
    private readonly GameState gameState;
    private readonly List<LocationProperties> allLocationProperties;
    private readonly List<BasicActionDefinition> basicActionDefinitions;

    public LocationSystem(GameState gameState, GameContentProvider contentProvider)
    {
        this.gameState = gameState;
        this.allLocationProperties = contentProvider.GetLocationProperties();
        this.basicActionDefinitions = contentProvider.GetBasicActionDefinitions();
    }

    public List<BasicActionDefinition> GetActionsForLocation(LocationNames location)
    {
        LocationProperties locationProperties = allLocationProperties.FirstOrDefault(x => x.Location == location);
        if (locationProperties == null) return null;

        return locationProperties.Actions;
    }

}