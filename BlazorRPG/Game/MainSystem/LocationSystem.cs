using System.Text;

public class LocationSystem
{
    private readonly LocationRepository locationRepository;
    private readonly GameState gameState;

    public LocationSystem(GameState gameState, LocationRepository locationRepo)
    {
        this.gameState = gameState;
        this.locationRepository = locationRepo;
    }

    public async Task<Location> Initialize(string startingLocation)
    {
        List<Location> allLocations = locationRepository.GetAllLocations().ToList();
        Location startLoc = GetLocation(startingLocation);
        startLoc.LocationType = LocationTypes.Hub;

        foreach (Location? loc in allLocations.Where(l =>
        {
            return l.Name != startingLocation;
        }))
        {
            int depth = loc.ConnectedTo.Contains(startingLocation) ? 1 : 2;
        }

        foreach (Location? loc in allLocations.Where(l =>
        {
            return l.PlayerKnowledge;
        }))
        {
            gameState.PlayerState.AddKnownLocation(loc.Name);
            foreach (LocationSpot? spot in locationRepository.GetSpotsForLocation(loc.Name).Where(s =>
            {
                return s.PlayerKnowledge;
            }))
                gameState.PlayerState.AddKnownLocationSpot(spot.Name);
        }

        return GetLocation(startingLocation);
    }

    public Location GetLocation(string id)
    {
        Location location = locationRepository.GetLocationById(id);
        return location;
    }

    public List<Location> GetAllLocations()
    {
        List<Location> locations = locationRepository.GetAllLocations();
        return locations;
    }

    public List<LocationSpot> GetLocationSpots(string locationId)
    {
        List<LocationSpot> locationSpots = locationRepository.GetSpotsForLocation(locationId);
        return locationSpots;
    }

    public LocationSpot GetLocationSpot(string locationName, string spotName)
    {
        LocationSpot locationSpot = locationRepository.GetSpot(locationName, spotName);
        return locationSpot;
    }

    public List<Location> GetConnectedLocations(string location)
    {
        List<Location> locations = locationRepository.GetConnectedLocations(location);
        return locations;
    }

    public string FormatLocations(List<Location> locations)
    {
        StringBuilder sb = new StringBuilder();

        if (locations == null || !locations.Any())
            return "None";

        foreach (Location loc in locations)
        {
            sb.AppendLine($"- {loc.Name}: {loc.Description} (Depth: {loc.Depth})");
        }

        return sb.ToString();
    }

    public string FormatLocationSpots(Location location)
    {
        StringBuilder sb = new StringBuilder();

        List<LocationSpot> locationSpots = GetLocationSpots(location.Name);
        if (location == null || locationSpots == null || !locationSpots.Any())
            return "None";

        foreach (LocationSpot spot in locationSpots)
        {
            sb.AppendLine($"- {spot.Name}: {spot.Description}");
        }

        return sb.ToString();
    }

}