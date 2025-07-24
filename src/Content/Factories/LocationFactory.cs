using System;
using System.Collections.Generic;
using System.Linq;
/// <summary>
/// Factory for creating locations with guaranteed valid references.
/// Locations are base entities, but their ConnectedLocationIds and LocationSpotIds
/// need to be validated after all locations and spots are loaded.
/// </summary>
public class LocationFactory
{
    public LocationFactory()
    {
        // No dependencies needed - locations are base entities
    }

    /// <summary>
    /// Create a location from validated data.
    /// Note: ConnectedLocationIds and LocationSpotIds are stored as strings
    /// and must be validated after all entities are loaded.
    /// </summary>
    public Location CreateLocation(
        string id,
        string name,
        string description,
        List<string> connectedLocationIds,
        List<string> locationSpotIds,
        List<string> domainTags,
        Dictionary<TimeBlocks, List<string>> environmentalProperties = null,
        Dictionary<TimeBlocks, List<Professions>> availableProfessionsByTime = null)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("Location ID cannot be empty", nameof(id));
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Location name cannot be empty", nameof(name));

        Location location = new Location(id, name)
        {
            Description = description ?? "",
            ConnectedLocationIds = connectedLocationIds ?? new List<string>(),
            LocationSpotIds = locationSpotIds ?? new List<string>(),
            DomainTags = domainTags ?? new List<string>()
        };

        // Set environmental properties
        if (environmentalProperties != null)
        {
            foreach ((TimeBlocks timeBlock, List<string> properties) in environmentalProperties)
            {
                switch (timeBlock)
                {
                    case TimeBlocks.Morning:
                        location.MorningProperties = properties;
                        break;
                    case TimeBlocks.Afternoon:
                        location.AfternoonProperties = properties;
                        break;
                    case TimeBlocks.Evening:
                        location.EveningProperties = properties;
                        break;
                    case TimeBlocks.Night:
                        location.NightProperties = properties;
                        break;
                }
            }
        }

        // Set available professions by time
        if (availableProfessionsByTime != null)
        {
            location.AvailableProfessionsByTime = availableProfessionsByTime;
        }

        return location;
    }

    /// <summary>
    /// Validate that all connected location IDs actually exist
    /// This should be called after all locations are loaded
    /// </summary>
    public static void ValidateLocationConnections(Location location, IEnumerable<Location> allLocations)
    {
        HashSet<string> allLocationIds = allLocations.Select(l => l.Id).ToHashSet();

        List<string> invalidConnections = location.ConnectedLocationIds
            .Where(id => !allLocationIds.Contains(id))
            .ToList();

        if (invalidConnections.Any())
        {
            Console.WriteLine($"WARNING: Location '{location.Id}' has invalid connections: {string.Join(", ", invalidConnections)}");
            // Remove invalid connections
            location.ConnectedLocationIds = location.ConnectedLocationIds
                .Where(id => allLocationIds.Contains(id))
                .ToList();
        }
    }
}