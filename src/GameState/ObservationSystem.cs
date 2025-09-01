using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// Provides observation data for locations from JSON templates
/// This is a pure data provider with no game state dependencies
/// </summary>
public class ObservationSystem
{
    private readonly IContentDirectory _contentDirectory;
    private readonly Dictionary<string, Dictionary<string, List<Observation>>> _observationsByLocationAndSpot;
    private readonly HashSet<string> _revealedObservations;

    public ObservationSystem(IContentDirectory contentDirectory)
    {
        Console.WriteLine("[ObservationSystem] Constructor called");
        Console.WriteLine($"[ObservationSystem] ContentDirectory null? {contentDirectory == null}");

        _contentDirectory = contentDirectory;
        _revealedObservations = new HashSet<string>();
        _observationsByLocationAndSpot = LoadObservationsFromJson();

        Console.WriteLine("[ObservationSystem] Constructor completed");
    }

    private Dictionary<string, Dictionary<string, List<Observation>>> LoadObservationsFromJson()
    {
        Dictionary<string, Dictionary<string, List<Observation>>> observationsByLocationAndSpot = new Dictionary<string, Dictionary<string, List<Observation>>>();

        try
        {
            string filePath = Path.Combine(_contentDirectory.Path, "Templates", "observations.json");
            Console.WriteLine($"[ObservationSystem] Looking for observations at: {filePath}");

            if (File.Exists(filePath))
            {
                Console.WriteLine($"[ObservationSystem] Found observations.json, loading...");
                string json = File.ReadAllText(filePath);
                ObservationsData data = ObservationParser.ParseObservations(json);

                if (data?.locations != null)
                {
                    foreach (KeyValuePair<string, List<Observation>> locationKvp in data.locations)
                    {
                        string locationId = locationKvp.Key;
                        Dictionary<string, List<Observation>> spotObservations = new Dictionary<string, List<Observation>>();

                        // Group observations by spot
                        foreach (Observation obs in locationKvp.Value)
                        {
                            string spotId = obs.SpotId ?? "default";
                            if (!spotObservations.ContainsKey(spotId))
                            {
                                spotObservations[spotId] = new List<Observation>();
                            }
                            spotObservations[spotId].Add(obs);
                        }

                        observationsByLocationAndSpot[locationId] = spotObservations;
                        Console.WriteLine($"[ObservationSystem] Loaded observations for location {locationId}: {string.Join(", ", spotObservations.Select(s => $"{s.Key}({s.Value.Count})"))}");
                    }
                }
                else
                {
                    Console.WriteLine("[ObservationSystem] No observations data found in JSON");
                }
            }
            else
            {
                Console.WriteLine($"[ObservationSystem] observations.json not found at {filePath}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ObservationSystem] Error loading observations.json: {ex.Message}");
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