public class LocationRepository
{
    private readonly ContentRegistry _contentRegistry;
    public LocationRepository(ContentRegistry contentRegistry)
    {
        _contentRegistry = contentRegistry;
    }

    public Location GetLocation(string locationName)
    {
        bool canResolve = _contentRegistry.TryResolve<Location>(locationName, out Location? loc);
        return canResolve
                ? loc
                : throw new KeyNotFoundException($"Location '{locationName}' not found.");
    }

    public List<Location> GetAllLocations()
    {
        List<Location> locations = _contentRegistry.GetAllOfType<Location>();

        return locations;
    }

    public List<LocationSpot> GetSpotsForLocation(string locationName)
    {
        List<LocationSpot> locationSpotsAll = _contentRegistry.GetAllOfType<LocationSpot>();
        List<LocationSpot> locationSpots = 
            locationSpotsAll.Where(s =>
            {
                return string.Equals(s.LocationName, locationName, StringComparison.OrdinalIgnoreCase);
            })
            .ToList();
        return locationSpots;
    }

    public LocationSpot GetSpot(string locationName, string spotName)
    {
        LocationSpot locationSpot = GetSpotsForLocation(locationName)
            .FirstOrDefault(s =>
            {
                return s.Name.Equals(spotName, StringComparison.OrdinalIgnoreCase);
            })
            ?? throw new KeyNotFoundException($"Spot '{spotName}' not found in '{locationName}'.");
        return locationSpot;
    }

    internal List<Location> GetConnectedLocations(string currentLocation)
    {
        List<Location> allLocations = _contentRegistry.GetAllOfType<Location>();
        List<Location> locations = allLocations
            .Where(l =>
                {
                    return l.ConnectedTo.Contains(currentLocation);
                })
            .ToList();
        return locations;
    }
}
