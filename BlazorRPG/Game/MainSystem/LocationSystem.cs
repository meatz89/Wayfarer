using System.Text;

public class LocationSystem
{
    private readonly GameState gameState;
    private readonly WorldState worldState;
    public bool IsInitialized = false;

    public LocationSystem(
    GameState gameState,
    GameContentProvider contentProvider)
    {
        this.gameState = gameState;
        this.worldState = gameState.WorldState;
        List<Location> allLocations = contentProvider.GetLocations();
    }

    public async Task<Location> Initialize(LocationNames startingLocation)
    {
        // Create the starting locations
        List<Location> locations = new List<Location>
        {
            WorldLocationsContent.Forest,
            WorldLocationsContent.Village,
        };

        gameState.WorldState.AddLocations(locations);

        string startingLocationName = startingLocation.ToString();
        InitializeLocationDepths(startingLocationName);

        IsInitialized = true;

        return GetLocation(startingLocationName);
    }

    private void InitializeLocationDepths(string startingLocation)
    {
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
    }

    public List<Location> GetAllLocations()
    {
        return gameState.WorldState.GetLocations();
    }

    internal List<Location> GetConnectedLocations()
    {
        List<Location> connectedLocations = new();
        List<string> locs = worldState.CurrentLocation.ConnectedTo;
        foreach (string conLoc in locs)
        {
            connectedLocations.Add(GetLocation(conLoc));
        }

        return connectedLocations;
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

        return location.LocationSpots;
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

    public void AddSpot(string locationName, LocationSpot spot)
    {
        Location location = gameState.WorldState.GetLocation(locationName);
        location.AddSpot(spot);
    }

    public string FormatLocations(List<Location> locations)
    {
        StringBuilder sb = new StringBuilder();

        if (locations == null || !locations.Any())
            return "None";

        foreach (Location loc in locations)
        {
            sb.AppendLine($"- {loc.Name}: {loc.Description} (Depth: {gameState.WorldState.GetLocationDepth(loc.Name)})");
        }

        return sb.ToString();
    }

    public string FormatLocationSpots(Location location)
    {
        StringBuilder sb = new StringBuilder();

        if (location == null || location.LocationSpots == null || !location.LocationSpots.Any())
            return "None";

        foreach (LocationSpot spot in location.LocationSpots)
        {
            sb.AppendLine($"- {spot.Name}: {spot.Description}");
        }

        return sb.ToString();
    }

    internal void ConnectLocations(Location location, Location currentLocation)
    {
        worldState.CurrentLocation.ConnectedTo.Add(location.Name);
        location.ConnectedTo.Add(worldState.CurrentLocation.Name);
    }
}
