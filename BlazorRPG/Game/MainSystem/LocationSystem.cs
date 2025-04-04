public class LocationSystem
{
    private readonly GameState gameState;
    private readonly WorldState worldState;
    private readonly LocationGenerator initialLocationGenerator;
    public bool IsInitialized = false;

    public LocationSystem(
    GameState gameState,
    GameContentProvider contentProvider,
    LocationGenerator initialLocationGenerator)
    {
        this.gameState = gameState;
        this.worldState = gameState.WorldState;
        this.initialLocationGenerator = initialLocationGenerator;
        List<Location> allLocations = contentProvider.GetLocations();
    }

    public async Task Initialize()
    {
        // Create the starting locations
        List<Location> locations = new List<Location>
    {
        WorldLocationsContent.Forest,
        WorldLocationsContent.Village,
    };

        gameState.WorldState.AddLocations(locations);
        InitializeLocationDepths();

        IsInitialized = true;
    }

    private void InitializeLocationDepths()
    {
        // Get the starting location
        string startingLocation = GameRules.StandardRuleset.StartingLocation.ToString();

        // Set the starting location to depth 0
        worldState.SetLocationDepth(startingLocation, 0);

        // Initialize it as a hub
        Location startLoc = GetLocation(startingLocation);
        if (startLoc != null)
        {
            startLoc.LocationType = LocationTypes.Hub;
            worldState.LastHubLocationId = startingLocation;
            worldState.LastHubDepth = 0;
        }

        // Set depths for connected locations
        foreach (Location location in GetAllLocations())
        {
            if (location.Name != startingLocation)
            {
                bool isDirectlyConnected = location.ConnectedTo?.Contains(startingLocation) ?? false;
                int depth = isDirectlyConnected ? 1 : 2;

                worldState.SetLocationDepth(location.Name, depth);
            }
        }
    }

    public void SetCurrentLocation(Location location)
    {
        worldState.SetCurrentLocation(location);
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
