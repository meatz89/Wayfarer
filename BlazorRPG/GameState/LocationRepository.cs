public class LocationRepository
{
    private WorldState worldState;

    public LocationRepository(GameWorld gameWorld)
    {
        worldState = gameWorld.WorldState;
    }

    public Location GetCurrentLocation()
    {
        return worldState.CurrentLocation;
    }

    public LocationSpot GetCurrentLocationSpot()
    {
        return worldState.CurrentLocationSpot;
    }

    public Location GetLocationById(string locationId)
    {
        Location location = worldState.locations.FirstOrDefault(l =>
        {
            return l.Id.Equals(locationId, StringComparison.OrdinalIgnoreCase);
        });

        if (location != null)
            return location;

        return null;
    }

    public Location GetLocationByName(string locationName)
    {
        Location location = worldState.locations.FirstOrDefault(l =>
        {
            return l.Id.Equals(locationName, StringComparison.OrdinalIgnoreCase);
        });

        if (location != null)
            return location;

        return null;
    }

    public List<Location> GetAllLocations()
    {
        return worldState.locations;
    }

    public List<LocationSpot> GetSpotsForLocation(string locationId)
    {
        Location location = GetLocationById(locationId);
        return location.AvailableSpots;
    }

    public LocationSpot GetSpot(string locationId, string spotId)
    {
        Location location = GetLocationById(locationId);
        if (location == null) location = worldState.CurrentLocation;
        LocationSpot spot = location.AvailableSpots.FirstOrDefault(s => s.SpotID == spotId);

        return spot;
    }

    public List<Location> GetConnectedLocations(string currentLocation)
    {
        return worldState.locations
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
        return worldState.locationSpots
            .Where(spot =>
            {
                return spot.CurrentSpotXP >= spot.XPToNextLevel;
            })
            .ToList();
    }

    // Add a location to the world
    public void AddLocation(Location location)
    {
        if (worldState.locations.Any(l =>
        {
            return l.Id.Equals(location.Id, StringComparison.OrdinalIgnoreCase);
        }))
            throw new InvalidOperationException($"Location '{location.Id}' already exists.");

        worldState.locations.Add(location);
    }

    // Add a location spot to the world
    public void AddLocationSpot(LocationSpot spot)
    {
        if (worldState.locationSpots.Any((Func<LocationSpot, bool>)(s =>
        {
            return (bool)(s.LocationId.Equals(spot.LocationId, StringComparison.OrdinalIgnoreCase) &&
                        s.SpotID.Equals((string)spot.SpotID, StringComparison.OrdinalIgnoreCase));
        })))
            throw new InvalidOperationException($"Spot '{spot.SpotID}' already exists in '{spot.LocationId}'.");

        worldState.locationSpots.Add(spot);
    }
}