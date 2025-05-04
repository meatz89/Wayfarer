public class LocationRepository
{
    private readonly WorldState _worldState;

    public LocationRepository(GameState gameState)
    {
        _worldState = gameState.WorldState;
    }

    public Location GetCurrentLocation()
    {
        return _worldState.CurrentLocation;
    }

    public LocationSpot GetCurrentLocationSpot()
    {
        return _worldState.CurrentLocationSpot;
    }

    public Location GetLocationById(string locationId)
    {
        Location location = _worldState.locations.FirstOrDefault(l =>
        {
            return l.Id.Equals(locationId, StringComparison.OrdinalIgnoreCase);
        });

        if (location != null)
            return location;

        throw new KeyNotFoundException($"Location '{locationId}' not found.");
    }

    public Location GetLocationByName(string locationName)
    {
        Location location = _worldState.locations.FirstOrDefault(l =>
        {
            return l.Id.Equals(locationName, StringComparison.OrdinalIgnoreCase);
        });

        if (location != null)
            return location;

        throw new KeyNotFoundException($"Location '{locationName}' not found.");
    }

    public List<Location> GetAllLocations()
    {
        return _worldState.locations;
    }

    public List<LocationSpot> GetSpotsForLocation(string locationId)
    {
        Location location = GetLocationById(locationId);
        return location.LocationSpots;
    }

    public LocationSpot GetSpot(string locationId, string spotId)
    {
        Location location = GetLocationById(locationId);

        LocationSpot spot = location.LocationSpots.FirstOrDefault(s => s.Id == spotId);

        if (spot == null)
            throw new KeyNotFoundException($"Spot '{spotId}' not found in '{locationId}'.");

        return spot;
    }

    public List<Location> GetConnectedLocations(string currentLocation)
    {
        return _worldState.locations
            .Where(l =>
            {
                return l.ConnectedTo.Contains(currentLocation);
            })
            .ToList();
    }

    // Additional methods remain largely unchanged, just using _worldState instead
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

    public List<LocationSpot> GetSpotsReadyToLevelUp()
    {
        return _worldState.locationSpots
            .Where(spot =>
            {
                return spot.CurrentSpotXP >= spot.XPToNextLevel;
            })
            .ToList();
    }

    // New method to add a location to the world
    public void AddLocation(Location location)
    {
        if (_worldState.locations.Any(l =>
        {
            return l.Id.Equals(location.Id, StringComparison.OrdinalIgnoreCase);
        }))
            throw new InvalidOperationException($"Location '{location.Id}' already exists.");

        _worldState.locations.Add(location);
    }

    // New method to add a location spot to the world
    public void AddLocationSpot(LocationSpot spot)
    {
        if (_worldState.locationSpots.Any((Func<LocationSpot, bool>)(s =>
        {
            return (bool)(s.LocationId.Equals(spot.LocationId, StringComparison.OrdinalIgnoreCase) &&
                        s.Id.Equals((string)spot.Id, StringComparison.OrdinalIgnoreCase));
        })))
            throw new InvalidOperationException($"Spot '{spot.Id}' already exists in '{spot.LocationId}'.");

        _worldState.locationSpots.Add(spot);
    }
}