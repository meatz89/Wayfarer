public class LocationSystem
{
    private readonly GameState gameState;
    private readonly List<Location> allLocations;

    public LocationSystem(GameState gameState, GameContentProvider contentProvider)
    {
        this.gameState = gameState;
        this.allLocations = contentProvider.GetLocations();

        foreach (Location location in allLocations)
        {
            gameState.WorldState.AddLocation(location.Name, location);
        }
    }

    public List<Location> GetAllLocations()
    {
        return allLocations;
    }

    public List<string> GetTravelLocations(string currentLocation)
    {
        Location location = GetLocation(currentLocation);
        return location.ConnectedLocationIds;
    }

    public Location GetLocation(string locationName)
    {
        Location location = allLocations.FirstOrDefault(x => x.Name == locationName);
        return location;
    }

    public List<LocationSpot> GetLocationSpots(Location location)
    {
        return location.Spots;
    }

    public LocationSpot GetLocationSpotForLocation(string locationName, string locationSpotName)
    {
        Location location = GetLocation(locationName);
        List<LocationSpot> spots = GetLocationSpots(location);
        LocationSpot? locationSpot = spots.FirstOrDefault(x => x.Name == locationSpotName);
        return locationSpot;
    }

    public List<StrategicTag> GetEnvironmentalProperties(string locationName, string locationSpotName)
    {
        Location location = GetLocation(locationName);
        LocationSpot locationSpot = GetLocationSpotForLocation(locationName, locationSpotName);

        return new List<StrategicTag>();
    }

}
