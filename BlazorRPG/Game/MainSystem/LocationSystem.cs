using System.Text;

public class LocationSystem
{
    private readonly GameState gameState;
    private readonly PlayerState playerState;
    private readonly WorldState worldState;
    public bool IsInitialized = false;

    public string StartingLocation { get { return WorldLocationsContent.StartingLocation; } }

    public LocationSystem(
    GameState gameState,
    GameContentProvider contentProvider)
    {
        this.gameState = gameState;
        this.worldState = gameState.WorldState;
        this.playerState = gameState.PlayerState;
    }

    public async Task<Location> Initialize(string startingLocation)
    {
        List<Location> locations = WorldLocationsContent.AllLocations;

        gameState.WorldState.AddLocations(locations);

        string startingLocationName = startingLocation.ToString();
        InitializeLocationDepths(startingLocationName);

        List<Location> knownLocations = worldState.Locations.Where(x => x.PlayerKnowledge).ToList();
        foreach (Location location in knownLocations)
        {
            playerState.AddKnownLocation(location.Name);

            List<LocationSpot> knownLocationSpots = location.LocationSpots.Where(x => x.PlayerKnowledge).ToList();
            foreach (LocationSpot locationSpot in knownLocationSpots)
            {
                playerState.AddKnownLocationSpot(locationSpot.Name);
            }
        }

        IsInitialized = true;

        return GetLocation(startingLocationName);
    }

    public List<Location> GetKnownLocations()
    {
        List<Location> knownLocations = new List<Location>();

        List<string> known = gameState.PlayerState.KnownLocations;

        foreach (Location locationSpot in GetAllLocations())
        {
            if (known.Contains(locationSpot.Name))
            {
                knownLocations.Add(locationSpot);
            }
        }

        return knownLocations;
    }

    public List<LocationSpot> GetKnownLocationSpots(Location location)
    {
        List<LocationSpot> knownLocationSpots = new List<LocationSpot>();

        List<string> known = gameState.PlayerState.KnownLocationSpots;

        foreach (LocationSpot locationSpot in location.LocationSpots)
        {
            if (known.Contains(locationSpot.Name))
            {
                knownLocationSpots.Add(locationSpot);
            }
        }

        return knownLocationSpots;
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

    private List<Location> GetAllLocations()
    {
        return gameState.WorldState.GetLocations();
    }

    public List<Location> GetConnectedLocations()
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

    public LocationSpot GetLocationSpotForLocation(string locationName, string locationSpotName)
    {
        Location location = GetLocation(locationName);
        List<LocationSpot> spots = GetKnownLocationSpots(location);
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

    public void ConnectLocations(Location targetLocation, Location currentLocation)
    {
        NewConnection(currentLocation, targetLocation.Name);
        NewConnection(targetLocation, currentLocation.Name);
    }

    private void NewConnection(Location currentLocation, string targetLocation)
    {
        if (currentLocation.ConnectedTo.Contains(targetLocation)) return;
        currentLocation.ConnectedTo.Add(targetLocation);
    }
}
