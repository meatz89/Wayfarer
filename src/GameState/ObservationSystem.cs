/// <summary>
/// Provides observation data for locations from GameWorld
/// This is a pure data provider with no external file dependencies
/// </summary>
public class ObservationSystem
{
    private readonly GameWorld _gameWorld;
    private readonly List<string> _revealedObservations;

    public ObservationSystem(GameWorld gameWorld)
    {
        _gameWorld = gameWorld;
        _revealedObservations = new List<string>();
    }

    /// <summary>
    /// Get observations for a specific Venue and location
    /// </summary>
    public List<Observation> GetObservationsForLocation(string venueId, string locationId)
    {
        return _gameWorld.Observations
            .Where(obs => obs.LocationId == locationId)
            .ToList();
    }

    /// <summary>
    /// Get all observations for a Venue (all Locations combined)
    /// </summary>
    public List<Observation> GetAllObservationsForLocation(string venueId)
    {
        // ADR-007: Use Venue.Name and Name instead of deleted VenueId/Id
        List<string> locationIds = _gameWorld.Locations
            .Where(loc => loc.Venue.Name == venueId)
            .Select(loc => loc.Name)
            .ToList();

        return _gameWorld.Observations
            .Where(obs => locationIds.Contains(obs.LocationId))
            .ToList();
    }

    /// <summary>
    /// Mark an observation as revealed
    /// </summary>
    public void MarkObservationRevealed(string observationId)
    {
        if (!_revealedObservations.Contains(observationId))
            _revealedObservations.Add(observationId);
    }

    /// <summary>
    /// Check if an observation has been revealed
    /// </summary>
    public bool IsObservationRevealed(string observationId)
    {
        return _revealedObservations.Contains(observationId);
    }

}