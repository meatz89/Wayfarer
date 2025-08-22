using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Repository for accessing location spot data from the game world.
/// Provides read-only access to location spots following the repository pattern.
/// </summary>
public class LocationSpotRepository
{
    private readonly GameWorld _gameWorld;

    public LocationSpotRepository(GameWorld gameWorld)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
    }


    /// <summary>
    /// Get all location spots for a specific location.
    /// </summary>
    public List<LocationSpot> GetSpotsForLocation(string locationId)
    {
        return _gameWorld.WorldState.locationSpots
            .Where(s => s.LocationId.Equals(locationId, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

}