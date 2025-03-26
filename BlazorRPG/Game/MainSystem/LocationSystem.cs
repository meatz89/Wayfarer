public class LocationSystem
{
    private readonly GameState gameState;
    private readonly LocationGenerator initialLocationGenerator;
    public bool IsInitialized = false;

    public LocationSystem(
    GameState gameState,
    GameContentProvider contentProvider,
    LocationGenerator initialLocationGenerator)
    {
        this.gameState = gameState;
        this.initialLocationGenerator = initialLocationGenerator;
        List<Location> allLocations = contentProvider.GetLocations();
    }

    public async Task Initialize()
    {
        Location location = await initialLocationGenerator.GenerateNewLocationAsync("village");
        gameState.WorldState.AddLocation(location);

        IsInitialized = true;
    }

    internal void SetCurrentLocation(string locationName)
    {
        Location location = GetLocation(locationName);
        gameState.WorldState.SetCurrentLocation(location);
    }

    public List<Location> GetAllLocations()
    {
        return gameState.WorldState.GetLocations();
    }

    public List<string> GetTravelLocations(string currentLocation)
    {
        Location location = GetLocation(currentLocation);
        return location.ConnectedTo;
    }

    public Location GetLocation(string locationName)
    {
        List<Location> locations = GetAllLocations();
        Location location = locations.FirstOrDefault(x => x.Name == locationName);
        return location;
    }

    public List<LocationSpot> GetLocationSpots(Location location)
    {
        if (location == null) return new List<LocationSpot>();

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

    internal void AddSpot(string locationName, LocationSpot spot)
    {
        Location location = gameState.WorldState.GetLocation(locationName);
        location.AddSpot(spot);
    }

}
