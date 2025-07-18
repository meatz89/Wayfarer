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
    /// Get the current location spot where the player is located.
    /// </summary>
    public LocationSpot GetCurrentLocationSpot()
    {
        return _gameWorld.WorldState.CurrentLocationSpot;
    }

    /// <summary>
    /// Get all location spots in the game world.
    /// </summary>
    public List<LocationSpot> GetAllLocationSpots()
    {
        return _gameWorld.WorldState.locationSpots;
    }

    /// <summary>
    /// Get a specific location spot by its ID and location ID.
    /// </summary>
    public LocationSpot GetLocationSpot(string locationId, string spotId)
    {
        return _gameWorld.WorldState.locationSpots.FirstOrDefault(s => 
            s.LocationId.Equals(locationId, StringComparison.OrdinalIgnoreCase) &&
            s.SpotID.Equals(spotId, StringComparison.OrdinalIgnoreCase));
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

    /// <summary>
    /// Get all location spots of a specific type.
    /// </summary>
    public List<LocationSpot> GetSpotsByType(LocationSpotTypes type)
    {
        return _gameWorld.WorldState.locationSpots
            .Where(s => s.Type == type)
            .ToList();
    }

    /// <summary>
    /// Get all location spots that are available at the specified time.
    /// </summary>
    public List<LocationSpot> GetAvailableSpots(TimeBlocks currentTime)
    {
        return _gameWorld.WorldState.locationSpots
            .Where(s => s.CurrentTimeBlocks.Contains(currentTime))
            .ToList();
    }

    /// <summary>
    /// Get all location spots with NPCs.
    /// </summary>
    public List<LocationSpot> GetSpotsWithNPCs()
    {
        return _gameWorld.WorldState.locationSpots
            .Where(s => s.PrimaryNPC != null)
            .ToList();
    }

    /// <summary>
    /// Check if a location spot can level up.
    /// </summary>
    public bool CanLocationSpotLevelUp(string locationId, string spotId)
    {
        LocationSpot spot = GetLocationSpot(locationId, spotId);
        return spot != null && spot.CurrentSpotXP >= spot.XPToNextLevel;
    }

    /// <summary>
    /// Get all location spots ready to level up.
    /// </summary>
    public List<LocationSpot> GetSpotsReadyToLevelUp()
    {
        return _gameWorld.WorldState.locationSpots
            .Where(spot => spot.CurrentSpotXP >= spot.XPToNextLevel)
            .ToList();
    }

    /// <summary>
    /// Add a location spot to the world.
    /// </summary>
    public void AddLocationSpot(LocationSpot spot)
    {
        if (spot == null)
            throw new ArgumentNullException(nameof(spot));

        if (_gameWorld.WorldState.locationSpots.Any(s =>
            s.LocationId.Equals(spot.LocationId, StringComparison.OrdinalIgnoreCase) &&
            s.SpotID.Equals(spot.SpotID, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"Location spot '{spot.SpotID}' already exists in location '{spot.LocationId}'.");
        }

        _gameWorld.WorldState.locationSpots.Add(spot);
    }

    /// <summary>
    /// Check if a location spot exists.
    /// </summary>
    public bool LocationSpotExists(string locationId, string spotId)
    {
        return GetLocationSpot(locationId, spotId) != null;
    }

    /// <summary>
    /// Get location spots by domain tag.
    /// </summary>
    public List<LocationSpot> GetSpotsByDomainTag(string domainTag)
    {
        return _gameWorld.WorldState.locationSpots
            .Where(s => s.DomainTags.Contains(domainTag, StringComparer.OrdinalIgnoreCase))
            .ToList();
    }
}