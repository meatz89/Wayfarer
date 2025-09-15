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

                // Group observations by location, then by spot
                var locationGroups = _gameWorld.Observations.GroupBy(obs =>
                {
                    // Since observations don't have locationId directly, we'll use a default grouping
                    // In the future, observations should be enhanced to have location context
                    return "default_location";
                });

                foreach (var locationGroup in locationGroups)
                {
                    string locationId = locationGroup.Key;
                    Dictionary<string, List<Observation>> spotObservations = new Dictionary<string, List<Observation>>();

                    // Group observations by spot within this location
                    foreach (Observation obs in locationGroup)
                    {
                        string spotId = obs.SpotId ?? "default";
                        if (!spotObservations.ContainsKey(spotId))
                        {
                            spotObservations[spotId] = new List<Observation>();
                        }
                        spotObservations[spotId].Add(obs);
                    }

                    observationsByLocationAndSpot[locationId] = spotObservations;
                    Console.WriteLine($"[ObservationSystem] Grouped observations for location {locationId}: {string.Join(", ", spotObservations.Select(s => $"{s.Key}({s.Value.Count})"))}");
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
    /// Get observations for a specific location and spot
    /// </summary>
    public List<Observation> GetObservationsForLocationSpot(string locationId, string spotId)
    {
        Console.WriteLine($"[ObservationSystem] Looking for observations at {locationId}/{spotId}");

        if (_observationsByLocationAndSpot.TryGetValue(locationId, out Dictionary<string, List<Observation>>? spotMap))
        {
            Console.WriteLine($"[ObservationSystem] Found location {locationId}, available spots: {string.Join(", ", spotMap.Keys)}");

            if (spotMap.TryGetValue(spotId, out List<Observation>? observations))
            {
                Console.WriteLine($"[ObservationSystem] Found {observations.Count} observations for {locationId}/{spotId}");
                return observations;
            }
        }

        Console.WriteLine($"[ObservationSystem] No observations found for {locationId}/{spotId}");
        return new List<Observation>();
    }

    /// <summary>
    /// Get all observations for a location (all spots combined)
    /// </summary>
    public List<Observation> GetAllObservationsForLocation(string locationId)
    {
        Console.WriteLine($"[ObservationSystem] Getting all observations for location: {locationId}");

        if (_observationsByLocationAndSpot.TryGetValue(locationId, out Dictionary<string, List<Observation>>? spotMap))
        {
            List<Observation> allObservations = spotMap.Values.SelectMany(list => list).ToList();
            Console.WriteLine($"[ObservationSystem] Found {allObservations.Count} total observations for {locationId}");
            return allObservations;
        }

        Console.WriteLine($"[ObservationSystem] No observations found for {locationId}");
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