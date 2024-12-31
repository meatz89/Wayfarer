
public class LocationSystem
{
    private readonly GameState gameState;
    private readonly List<Location> allLocationProperties;

    public LocationSystem(GameState gameState, GameContentProvider contentProvider)
    {
        this.gameState = gameState;
        this.allLocationProperties = contentProvider.GetLocationProperties();
    }

    public List<BasicAction> GetActionsForLocation(LocationNames location)
    {
        Location locationProperties = allLocationProperties.FirstOrDefault(x => x.Name == location);
        if (locationProperties == null) return null;

        List<BasicAction> actions = locationProperties.AvailableActions;
        return actions;
    }

    public List<Location> GetLocations()
    {
        return allLocationProperties;
    }

    public List<LocationNames> GetLocationConnections(LocationNames currentLocation)
    {
        Location location = GetLocation(currentLocation);
        return location.ConnectedLocations;
    }

    public Location GetLocation(LocationNames locationName)
    {
        Location location = allLocationProperties.FirstOrDefault(x => x.Name == locationName);
        return location;
    }

}
