using System.Text;

public class LocationSystem
{
    private LocationRepository locationRepository;
    private GameWorld gameWorld;

    public LocationSystem(GameWorld gameWorld, LocationRepository locationRepo)
    {
        this.gameWorld = gameWorld;
        this.locationRepository = locationRepo;
    }

    public async Task<Location> Initialize()
    {
        List<Location> allLocations = locationRepository.GetAllLocations().ToList();
        Location startLoc = allLocations.First();

        startLoc.LocationType = LocationTypes.Hub;

        foreach (Location? loc in allLocations.Where(l =>
        {
            return l.Id != startLoc.Id;
        }))
        {
            int depth = loc.ConnectedTo.Contains(startLoc.Id) ? 1 : 2;
        }

        foreach (Location? loc in allLocations.Where(l =>
        {
            return l.PlayerKnowledge;
        }))
        {
            gameWorld.GetPlayer().AddKnownLocation(loc.Id);
            foreach (LocationSpot? spot in locationRepository.GetSpotsForLocation(loc.Id).Where(s =>
            {
                return s.PlayerKnowledge;
            }))
                gameWorld.GetPlayer().AddKnownLocationSpot(spot.SpotID);
        }

        return GetLocation(startLoc.Id);
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
            sb.AppendLine($"- {loc.Id}: {loc.Description} (Depth: {loc.Depth})");
        }

        return sb.ToString();
    }

    public string FormatLocationSpots(Location location)
    {
        StringBuilder sb = new StringBuilder();

        List<LocationSpot> locationSpots = GetLocationSpots(location.Id);
        if (location == null || locationSpots == null || !locationSpots.Any())
            return "None";

        foreach (LocationSpot spot in locationSpots)
        {
            sb.AppendLine($"- {spot.SpotID}: {spot.Description}");
        }

        return sb.ToString();
    }



}