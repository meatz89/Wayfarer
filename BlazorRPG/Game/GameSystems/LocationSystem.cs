


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
        Location locationProperties = allLocationProperties.FirstOrDefault(x => x.LocationName == location);
        if (locationProperties == null) return null;

        List<BasicAction> actions = locationProperties.Actions;
        return actions;
    }

    public List<LocationNames> GetLocationConnections(LocationNames currentLocation)
    {
        Location location = GetLocation(currentLocation);
        return location.ConnectedLocations;
    }

    private Location GetLocation(LocationNames locationName)
    {
        Location location = allLocationProperties.FirstOrDefault(x => x.LocationName == locationName);
        return location;
    }

}
