
public class LocationSystem
{
    private readonly GameState gameState;
    private readonly List<Location> allLocations;

    public LocationSystem(GameState gameState, GameContentProvider contentProvider)
    {
        this.gameState = gameState;
        this.allLocations = contentProvider.GetLocationProperties();
    }

    public List<BasicAction> GetActionsForLocation(LocationNames location)
    {
        Location locationProperties = allLocations.FirstOrDefault(x => x.Name == location);
        if (locationProperties == null) return null;

        List<BasicAction> actions = locationProperties.CoreActions;
        return actions;
    }

    public List<Location> GetLocations()
    {
        return allLocations;
    }

    public List<LocationNames> GetLocationConnections(LocationNames currentLocation)
    {
        Location location = GetLocation(currentLocation);
        return location.ConnectedLocations;
    }

    public Location GetLocation(LocationNames locationName)
    {
        Location location = allLocations.FirstOrDefault(x => x.Name == locationName);
        return location;
    }

    public List<LocationSpot> GetLocationSpots(Location location)
    {
        return location.Spots;
    }

    public LocationSpot GetLocationSpotForLocation(LocationNames locationName, LocationSpotTypes locationSpotType)
    {
        Location location = GetLocation(locationName);
        List<LocationSpot> spots = GetLocationSpots(location);
        return spots.FirstOrDefault(x => x.Name == locationSpotType);
    }
}
