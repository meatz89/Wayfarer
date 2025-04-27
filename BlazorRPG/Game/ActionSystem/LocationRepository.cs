/// <summary>
/// Repository for locations, backed by the GameObjectRegistry.
/// </summary>
public class LocationRepository
{
    private readonly GameContentRegistry _registry;

    public LocationRepository(GameContentRegistry registry)
    {
        _registry = registry;
    }

    /// <summary>
    /// Get a location by name.
    /// </summary>
    public Location GetLocation(string locationName)
    {
        if (_registry.TryGetLocation(locationName, out Location location))
            return location;

        throw new KeyNotFoundException($"Location '{locationName}' not found.");
    }

    /// <summary>
    /// Get all registered locations.
    /// </summary>
    public List<Location> GetAllLocations()
    {
        return _registry.GetAllLocations();
    }

    /// <summary>
    /// Get all location spots for a specific location.
    /// </summary>
    public List<LocationSpot> GetSpotsForLocation(string locationName)
    {
        return _registry.GetLocationSpotsForLocation(locationName);
    }

    /// <summary>
    /// Get a specific location spot by location name and spot name.
    /// </summary>
    public LocationSpot GetSpot(string locationName, string spotName)
    {
        List<LocationSpot> spots = GetSpotsForLocation(locationName);

        LocationSpot spot = spots.FirstOrDefault(s =>
            s.Name.Equals(spotName, StringComparison.OrdinalIgnoreCase));

        if (spot == null)
            throw new KeyNotFoundException($"Spot '{spotName}' not found in '{locationName}'.");

        return spot;
    }

    /// <summary>
    /// Get all locations connected to a specific location.
    /// </summary>
    public List<Location> GetConnectedLocations(string currentLocation)
    {
        List<Location> allLocations = _registry.GetAllLocations();

        return allLocations
            .Where(l => l.ConnectedTo.Contains(currentLocation))
            .ToList();
    }

    // Spot-specific progression methods
    public bool CanLocationSpotLevelUp(string locationName, string spotName)
    {
        LocationSpot spot = GetSpot(locationName, spotName);
        return spot.CurrentSpotXP >= spot.XPToNextLevel;
    }

    public void ApplyLocationSpotLevelUp(string locationName, string spotName)
    {
        LocationSpot spot = GetSpot(locationName, spotName);
        spot.CurrentLevel++;
        spot.CurrentSpotXP = 0;
        // Handle level-specific action changes
    }

    public List<string> GetAvailableActionsForLocationSpot(string locationName, string spotName)
    {
        LocationSpot spot = GetSpot(locationName, spotName);

        return spot.GetActionsForLevel(spot.CurrentLevel);
    }

    // Spot query methods
    public List<LocationSpot> GetSpotsReadyToLevelUp()
    {
        return _registry.GetAllLocationSpots()
            .Where(spot => spot.CurrentSpotXP >= spot.XPToNextLevel)
            .ToList();
    }
}