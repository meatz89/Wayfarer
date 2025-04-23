using System.Text;

public class LocationSystem
{
    private readonly LocationRepository locationRepo;
    private readonly GameState gameState;

    public LocationSystem(GameState gameState, LocationRepository locationRepo)
    {
        this.gameState = gameState;
        this.locationRepo = locationRepo;
    }

    public async Task<Location> Initialize(string startingLocation)
    {
        List<Location> allLocations = locationRepo.GetAllLocations().ToList();
        gameState.WorldState.AddLocations(allLocations);

        gameState.WorldState.SetLocationDepth(startingLocation, 0);
        Location startLoc = GetLocation(startingLocation);
        startLoc.LocationType = LocationTypes.Hub;
        gameState.WorldState.LastHubLocationId = startingLocation;
        gameState.WorldState.LastHubDepth = 0;

        foreach (Location? loc in allLocations.Where(l => l.Name != startingLocation))
        {
            int depth = loc.ConnectedTo.Contains(startingLocation) ? 1 : 2;
            gameState.WorldState.SetLocationDepth(loc.Name, depth);
        }

        foreach (Location? loc in allLocations.Where(l => l.PlayerKnowledge))
        {
            gameState.PlayerState.AddKnownLocation(loc.Name);
            foreach (LocationSpot? spot in locationRepo.GetSpotsForLocation(loc.Name).Where(s => s.PlayerKnowledge))
                gameState.PlayerState.AddKnownLocationSpot(spot.Name);
        }

        return GetLocation(startingLocation);
    }

    public Location GetLocation(string name)
        => locationRepo.GetLocation(name);

    public List<Location> GetAllLocations()
        => locationRepo.GetAllLocations();

    public List<LocationSpot> GetLocationSpots(string locationName)
        => locationRepo.GetSpotsForLocation(locationName);

    public LocationSpot GetLocationSpot(string locationName, string spotName)
        => locationRepo.GetSpot(locationName, spotName);

    internal List<Location> GetConnectedLocations(string location)
        => locationRepo.GetConnectedLocations(location);

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

}