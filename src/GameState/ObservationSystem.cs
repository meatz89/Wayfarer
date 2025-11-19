/// <summary>
/// Provides observation data for locations from GameWorld
/// This is a pure data provider with no external file dependencies
/// HIGHLANDER: Accept Location objects, compare objects
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
    /// Get observations for a specific location
    /// HIGHLANDER: Accept Location object, compare objects directly
    /// </summary>
    public List<Observation> GetObservationsForLocation(Location location)
    {
        return _gameWorld.Observations
            .Where(obs => obs.Location == location)
            .ToList();
    }

    /// <summary>
    /// Get all observations for a Venue (all Locations combined)
    /// HIGHLANDER: Accept Venue object, compare objects
    /// </summary>
    public List<Observation> GetAllObservationsForVenue(Venue venue)
    {
        // Get all locations in venue
        List<Location> venueLocations = _gameWorld.Locations
            .Where(loc => loc.Venue == venue)
            .ToList();

        return _gameWorld.Observations
            .Where(obs => venueLocations.Contains(obs.Location))
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