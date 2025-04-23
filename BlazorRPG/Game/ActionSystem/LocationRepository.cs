public class LocationRepository
{
    private readonly ContentRegistry _contentRegistry;
    public LocationRepository(ContentRegistry contentRegistry) => _contentRegistry = contentRegistry;

    public Location GetLocation(string locationName)
        => _contentRegistry.TryResolve<Location>(locationName, out Location? loc)
            ? loc
            : throw new KeyNotFoundException($"Location '{locationName}' not found.");

    public List<Location> GetAllLocations()
        => _contentRegistry.GetAllOfType<Location>();

    public List<LocationSpot> GetSpotsForLocation(string locationName)
        => _contentRegistry.GetAllOfType<LocationSpot>()
                   .Where(s => string.Equals(s.LocationName, locationName, StringComparison.OrdinalIgnoreCase)).ToList();

    public LocationSpot GetSpot(string locationName, string spotName)
        => GetSpotsForLocation(locationName)
            .FirstOrDefault(s => s.Name.Equals(spotName, StringComparison.OrdinalIgnoreCase))
            ?? throw new KeyNotFoundException($"Spot '{spotName}' not found in '{locationName}'.");

    internal List<Location> GetConnectedLocations(string currentLocation)
    {
        return _contentRegistry.GetAllOfType<Location>().Where(l => l.ConnectedTo.Contains(currentLocation)).ToList();
    }
}
