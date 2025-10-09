using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// Provides observation data for locations from GameWorld
/// This is a pure data provider with no external file dependencies
/// </summary>
public class ObservationSystem
{
    private readonly GameWorld _gameWorld;
    private readonly Dictionary<string, Dictionary<string, List<Observation>>> _observationsByLocationAndSpot;
    private readonly HashSet<string> _revealedObservations;

    public ObservationSystem(GameWorld gameWorld)
    {
        Console.WriteLine("[ObservationSystem] Constructor called");
        Console.WriteLine($"[ObservationSystem] GameWorld null? {gameWorld == null}");

        _gameWorld = gameWorld;
        _revealedObservations = new HashSet<string>();
        _observationsByLocationAndSpot = LoadObservationsFromJson();

        Console.WriteLine("[ObservationSystem] Constructor completed");
    }

    private Dictionary<string, Dictionary<string, List<Observation>>> LoadObservationsFromJson()
    {
        Dictionary<string, Dictionary<string, List<Observation>>> observationsByLocationAndSpot = new Dictionary<string, Dictionary<string, List<Observation>>>();

        try
        {
            Console.WriteLine($"[ObservationSystem] Loading observations from GameWorld...");

            if (_gameWorld.Observations != null && _gameWorld.Observations.Count > 0)
            {
                Console.WriteLine($"[ObservationSystem] Found {_gameWorld.Observations.Count} observations in GameWorld");

                // Group observations by location, then by location
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
                        string LocationId = obs.LocationId ?? "default";
                        if (!spotObservations.ContainsKey(LocationId))
                        {
                            spotObservations[LocationId] = new List<Observation>();
                        }
                        spotObservations[LocationId].Add(obs);
                    }

                    observationsByLocationAndSpot[venueId] = spotObservations;
                    Console.WriteLine($"[ObservationSystem] Grouped observations for Venue {venueId}: {string.Join(", ", spotObservations.Select(s => $"{s.Key}({s.Value.Count})"))}");
                }
            }
            else
            {
                Console.WriteLine("[ObservationSystem] No observations found in GameWorld");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ObservationSystem] Error loading observations from GameWorld: {ex.Message}");
        }

        Console.WriteLine($"[ObservationSystem] Total locations with observations: {observationsByLocationAndSpot.Count}");
        return observationsByLocationAndSpot;
    }

    /// <summary>
    /// Get observations for a specific Venue and location
    /// </summary>
    public List<Observation> GetObservationsForLocationSpot(string venueId, string LocationId)
    {
        Console.WriteLine($"[ObservationSystem] Looking for observations at {venueId}/{LocationId}");

        if (_observationsByLocationAndSpot.TryGetValue(venueId, out Dictionary<string, List<Observation>>? spotMap))
        {
            Console.WriteLine($"[ObservationSystem] Found Venue {venueId}, available Locations: {string.Join(", ", spotMap.Keys)}");

            if (spotMap.TryGetValue(LocationId, out List<Observation>? observations))
            {
                Console.WriteLine($"[ObservationSystem] Found {observations.Count} observations for {venueId}/{LocationId}");
                return observations;
            }
        }

        Console.WriteLine($"[ObservationSystem] No observations found for {venueId}/{LocationId}");
        return new List<Observation>();
    }

    /// <summary>
    /// Get all observations for a Venue (all Locations combined)
    /// </summary>
    public List<Observation> GetAllObservationsForLocation(string venueId)
    {
        Console.WriteLine($"[ObservationSystem] Getting all observations for location: {venueId}");

        if (_observationsByLocationAndSpot.TryGetValue(venueId, out Dictionary<string, List<Observation>>? spotMap))
        {
            List<Observation> allObservations = spotMap.Values.SelectMany(list => list).ToList();
            Console.WriteLine($"[ObservationSystem] Found {allObservations.Count} total observations for {venueId}");
            return allObservations;
        }

        Console.WriteLine($"[ObservationSystem] No observations found for {venueId}");
        return new List<Observation>();
    }

    /// <summary>
    /// Mark an observation as revealed
    /// </summary>
    public void MarkObservationRevealed(string observationId)
    {
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