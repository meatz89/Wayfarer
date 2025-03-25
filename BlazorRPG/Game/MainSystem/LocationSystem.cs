public class LocationSystem
{
    private readonly GameState gameState;
    private readonly NarrativeService narrativeService;

    public bool IsInitialized = false;

    public LocationSystem(
    GameState gameState,
    GameContentProvider contentProvider,
    NarrativeService narrativeService)
    {
        this.gameState = gameState;
        this.narrativeService = narrativeService;

        List<Location> allLocations = contentProvider.GetLocations();
    }

    public async Task Initialize()
    {
        LocationGenerator initialLocationGenerator = new LocationGenerator();

        Location location = await initialLocationGenerator.GenerateNewLocationAsync("village", narrativeService);
        gameState.WorldState.AddLocation(location.Name, location);

        IsInitialized = true;
    }

    public List<Location> GetAllLocations()
    {
        return gameState.WorldState.GetLocations();
    }

    public List<string> GetTravelLocations(string currentLocation)
    {
        Location location = GetLocation(currentLocation);
        return location.ConnectedLocationIds;
    }

    public Location GetLocation(string locationName)
    {
        Location location = GetAllLocations().FirstOrDefault(x => x.Name == locationName);
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
}
