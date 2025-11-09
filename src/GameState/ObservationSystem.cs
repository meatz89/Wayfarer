/// <summary>
/// Provides observation data for locations from GameWorld
/// This is a pure data provider with no external file dependencies
/// </summary>
public class ObservationSystem
{
private readonly GameWorld _gameWorld;
private readonly Dictionary<string, Dictionary<string, List<Observation>>> _observationsByLocationAndSpot;
private readonly List<string> _revealedObservations;

public ObservationSystem(GameWorld gameWorld)
{
    _gameWorld = gameWorld;
    _revealedObservations = new List<string>();
    _observationsByLocationAndSpot = LoadObservationsFromJson();
}

private Dictionary<string, Dictionary<string, List<Observation>>> LoadObservationsFromJson()
{
    Dictionary<string, Dictionary<string, List<Observation>>> observationsByLocationAndSpot = new Dictionary<string, Dictionary<string, List<Observation>>>();

    if (_gameWorld.Observations != null && _gameWorld.Observations.Count > 0)
    {// Group observations by location, then by location
        IEnumerable<IGrouping<string, Observation>> locationGroups = _gameWorld.Observations.GroupBy(obs =>
        {
            // Since observations don't have venueId directly, we'll use a default grouping
            // In the future, observations should be enhanced to have Venue context
            return "default_location";
        });

        foreach (IGrouping<string, Observation> locationGroup in locationGroups)
        {
            string venueId = locationGroup.Key;
            Dictionary<string, List<Observation>> spotObservations = new Dictionary<string, List<Observation>>();

            // Group observations by location within this location
            foreach (Observation obs in locationGroup)
            {
                string LocationId = string.IsNullOrEmpty(obs.LocationId) ? "default" : obs.LocationId;
                if (!spotObservations.ContainsKey(LocationId))
                {
                    spotObservations[LocationId] = new List<Observation>();
                }
                spotObservations[LocationId].Add(obs);
            }

            observationsByLocationAndSpot[venueId] = spotObservations;
        }
    }
    else
    { }

    return observationsByLocationAndSpot;
}

/// <summary>
/// Get observations for a specific Venue and location
/// </summary>
public List<Observation> GetObservationsForLocation(string venueId, string LocationId)
{
    if (_observationsByLocationAndSpot.TryGetValue(venueId, out Dictionary<string, List<Observation>>? spotMap))
    {
        if (spotMap.TryGetValue(LocationId, out List<Observation>? observations))
        {
            return observations;
        }
    }
    return new List<Observation>();
}

/// <summary>
/// Get all observations for a Venue (all Locations combined)
/// </summary>
public List<Observation> GetAllObservationsForLocation(string venueId)
{
    if (_observationsByLocationAndSpot.TryGetValue(venueId, out Dictionary<string, List<Observation>> spotMap))
    {
        List<Observation> allObservations = spotMap.Values.SelectMany(list => list).ToList();
        return allObservations;
    }
    return new List<Observation>();
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