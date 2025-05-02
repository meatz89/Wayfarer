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
            return l.Name.Equals(locationName, StringComparison.OrdinalIgnoreCase);
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
        return _worldState.locationSpots
            .Where(spot =>
            {
                return spot.LocationId.Equals(locationId, StringComparison.OrdinalIgnoreCase);
            })
            .ToList();
    }

    public LocationSpot GetSpot(string locationId, string spotName)
    {
        List<LocationSpot> spots = _worldState.locationSpots.Where(s =>
        {
            return s.LocationId == locationId;
        }).ToList();
        LocationSpot? spot = spots.FirstOrDefault(s =>
        {
            return s.Name.Equals(spotName, StringComparison.OrdinalIgnoreCase);
        });

        if (spot == null)
            throw new KeyNotFoundException($"Spot '{spotName}' not found in '{locationId}'.");

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

    public List<string> GetAvailableActionsForLocationSpot(string locationName, string spotName)
    {
        LocationSpot spot = GetSpot(locationName, spotName);
        return spot.GetActionsForLevel(spot.CurrentLevel);
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
            return l.Name.Equals(location.Name, StringComparison.OrdinalIgnoreCase);
        }))
            throw new InvalidOperationException($"Location '{location.Name}' already exists.");

        _worldState.locations.Add(location);
    }

    // New method to add a location spot to the world
    public void AddLocationSpot(LocationSpot spot)
    {
        if (_worldState.locationSpots.Any(s =>
        {
            return s.LocationId.Equals(spot.LocationId, StringComparison.OrdinalIgnoreCase) &&
                        s.Name.Equals(spot.Name, StringComparison.OrdinalIgnoreCase);
        }))
            throw new InvalidOperationException($"Spot '{spot.Name}' already exists in '{spot.LocationId}'.");

        _worldState.locationSpots.Add(spot);
    }
}