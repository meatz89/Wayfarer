public class LocationRepository
{
    // ✅ ARCHITECTURAL COMPLIANCE: Stateless repository with GameWorld DI
    private readonly GameWorld _gameWorld;

    public LocationRepository(GameWorld gameWorld)
    {
        _gameWorld = gameWorld;
    }

    public Location GetCurrentLocation()
    {
        return _gameWorld.WorldState.CurrentLocation;
    }

    public LocationSpot GetCurrentLocationSpot()
    {
        return _gameWorld.WorldState.CurrentLocationSpot;
    }

    public Location GetLocation(string locationId)
    {
        Location location = _gameWorld.WorldState.locations.FirstOrDefault(l =>
        {
            return l.Id.Equals(locationId, StringComparison.OrdinalIgnoreCase);
        });

        if (location != null)
            return location;

        return null;
    }

    public Location GetLocationByName(string locationName)
    {
        Location location = _gameWorld.WorldState.locations.FirstOrDefault(l =>
        {
            return l.Name.Equals(locationName, StringComparison.OrdinalIgnoreCase);
        });

        if (location != null)
            return location;

        return null;
    }

    public List<Location> GetAllLocations()
    {
        return _gameWorld.WorldState.locations;
    }

    public List<LocationSpot> GetSpotsForLocation(string locationId)
    {
        Location location = GetLocation(locationId);
        return location.AvailableSpots;
    }

    public LocationSpot GetSpot(string locationId, string spotId)
    {
        Location location = GetLocation(locationId);
        if (location == null) location = _gameWorld.WorldState.CurrentLocation;
        LocationSpot spot = location.AvailableSpots.FirstOrDefault(s => s.SpotID == spotId);

        return spot;
    }

    public List<Location> GetConnectedLocations(string currentLocation)
    {
        return _gameWorld.WorldState.locations
            .Where(l =>
            {
                return l.ConnectedLocationIds.Contains(currentLocation);
            })
            .ToList();
    }

    // Additional methods remain largely unchanged, just using __gameWorld.WorldState instead
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
        return _gameWorld.WorldState.locationSpots
            .Where(spot =>
            {
                return spot.CurrentSpotXP >= spot.XPToNextLevel;
            })
            .ToList();
    }

    // Add a location to the world
    public void AddLocation(Location location)
    {
        if (_gameWorld.WorldState.locations.Any(l =>
        {
            return l.Id.Equals(location.Id, StringComparison.OrdinalIgnoreCase);
        }))
            throw new InvalidOperationException($"Location '{location.Id}' already exists.");

        _gameWorld.WorldState.locations.Add(location);
    }

    // Add a location spot to the world
    public void AddLocationSpot(LocationSpot spot)
    {
        if (_gameWorld.WorldState.locationSpots.Any((Func<LocationSpot, bool>)(s =>
        {
            return (bool)(s.LocationId.Equals(spot.LocationId, StringComparison.OrdinalIgnoreCase) &&
                        s.SpotID.Equals((string)spot.SpotID, StringComparison.OrdinalIgnoreCase));
        })))
            throw new InvalidOperationException($"Spot '{spot.SpotID}' already exists in '{spot.LocationId}'.");

        _gameWorld.WorldState.locationSpots.Add(spot);
    }
}