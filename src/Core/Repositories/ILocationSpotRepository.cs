using System.Collections.Generic;
/// <summary>
/// Repository interface for LocationSpot entities
/// </summary>
public interface ILocationSpotRepository : IRepository<LocationSpot>
{
    /// <summary>
    /// Get spots for a specific location
    /// </summary>
    IEnumerable<LocationSpot> GetSpotsForLocation(string locationId);

    /// <summary>
    /// Get a spot by its SpotID
    /// </summary>
    LocationSpot GetBySpotId(string spotId);

    /// <summary>
    /// Get spots that have NPCs
    /// </summary>
    IEnumerable<LocationSpot> GetSpotsWithNPCs();
}