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

        // FOR  POC: Make all locations known by default
        // In full game, only locations with PlayerKnowledge=true would be known
        foreach (Location loc in allLocations)
        {
            // Check PlayerKnowledge OR default to true for  POC
            bool shouldKnow = loc.PlayerKnowledge || true; //  POC: always true

            if (shouldKnow)
            {
                gameWorld.GetPlayer().AddKnownLocation(loc.Id);

                // Add all spots for known locations in  POC
                foreach (LocationSpot spot in locationRepository.GetSpotsForLocation(loc.Id))
                {
                    // Check spot PlayerKnowledge OR default to true for  POC
                    bool shouldKnowSpot = spot.PlayerKnowledge || true; //  POC: always true

                    if (shouldKnowSpot)
                    {
                        gameWorld.GetPlayer().AddKnownLocationSpot(spot.SpotID);
                    }
                }
            }
        }

        // CRITICAL: Ensure player always has valid current location and spot
        // Systems depend on these never being null
        Player player = gameWorld.GetPlayer();
        if (player.CurrentLocation == null)
        {
            player.CurrentLocation = startLoc;
            Console.WriteLine($"Set player CurrentLocation to: {startLoc.Id}");
        }

        if (player.CurrentLocationSpot == null)
        {
            List<LocationSpot> startLocationSpots = GetLocationSpots(startLoc.Id);
            if (startLocationSpots.Any())
            {
                player.CurrentLocationSpot = startLocationSpots.First();
                Console.WriteLine($"Set player CurrentLocationSpot to: {player.CurrentLocationSpot.SpotID}");
            }
        }

        return GetLocation(startLoc.Id);
    }

    public Location GetLocation(string id)
    {
        Location location = locationRepository.GetLocation(id);
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