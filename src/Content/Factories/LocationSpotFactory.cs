using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Factory for creating location spots with guaranteed valid references.
/// LocationSpots reference their parent Location by ID.
/// </summary>
public class LocationSpotFactory
{
    public LocationSpotFactory()
    {
        // No dependencies in constructor
    }

    /// <summary>
    /// Create a minimal location spot with just an ID.
    /// Used for dummy/placeholder creation when references are missing.
    /// </summary>
    public LocationSpot CreateMinimalSpot(string spotId, string locationId)
    {
        if (string.IsNullOrEmpty(spotId))
            throw new ArgumentException("Spot ID cannot be empty", nameof(spotId));
        if (string.IsNullOrEmpty(locationId))
            throw new ArgumentException("Location ID cannot be empty", nameof(locationId));

        string name = FormatIdAsName(spotId);

        return new LocationSpot(spotId, name)
        {
            LocationId = locationId,
            InitialState = "A quiet spot.",
            CurrentTimeBlocks = new List<TimeBlocks>
            {
                TimeBlocks.Morning,
                TimeBlocks.Afternoon,
                TimeBlocks.Evening
            },
            DomainTags = new List<string> { "GENERIC" }
        };
    }

    private string FormatIdAsName(string id)
    {
        // Convert snake_case or kebab-case to Title Case
        return string.Join(" ",
            id.Replace('_', ' ').Replace('-', ' ')
              .Split(' ')
              .Select(word => string.IsNullOrEmpty(word) ? "" :
                  char.ToUpper(word[0]) + word.Substring(1).ToLower()));
    }

    /// <summary>
    /// Create a location spot with validated location reference
    /// </summary>
    public LocationSpot CreateLocationSpot(
        string spotId,
        string name,
        Location parentLocation,  // Not string - actual Location object
        string initialState = null,
        List<TimeBlocks> availableTimeBlocks = null,
        List<string> domainTags = null)
    {
        if (string.IsNullOrEmpty(spotId))
            throw new ArgumentException("Spot ID cannot be empty", nameof(spotId));
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Spot name cannot be empty", nameof(name));
        if (parentLocation == null)
            throw new ArgumentNullException(nameof(parentLocation), "Parent location cannot be null");

        LocationSpot spot = new LocationSpot(spotId, name)
        {
            LocationId = parentLocation.Id,  // Extract ID from validated object
            InitialState = initialState ?? $"The {name} is in its usual state.",
            CurrentTimeBlocks = availableTimeBlocks ?? new List<TimeBlocks>(),
            DomainTags = domainTags ?? new List<string>()
        };

        return spot;
    }

    /// <summary>
    /// Create a location spot from string IDs with validation
    /// </summary>
    public LocationSpot CreateLocationSpotFromIds(
        string spotId,
        string name,
        string locationId,
        IEnumerable<Location> availableLocations,
        string initialState = null,
        List<TimeBlocks> availableTimeBlocks = null,
        List<string> domainTags = null)
    {
        // Resolve location
        Location? location = availableLocations.FirstOrDefault(l => l.Id == locationId);
        if (location == null)
            throw new InvalidOperationException($"Cannot create location spot: parent location '{locationId}' not found");

        return CreateLocationSpot(spotId, name, location, initialState, availableTimeBlocks, domainTags);
    }

    /// <summary>
    /// Connect an NPC to a location spot.
    /// This should be called after NPCs are loaded.
    /// </summary>
    public void ConnectNPCToSpot(LocationSpot spot, NPC npc)
    {
        if (spot == null)
            throw new ArgumentNullException(nameof(spot));
        if (npc == null)
            throw new ArgumentNullException(nameof(npc));

        // Validate NPC is in the same location as the spot
        if (npc.Location != spot.LocationId)
        {
            Console.WriteLine($"WARNING: NPC '{npc.Name}' (location: {npc.Location}) cannot be connected to spot '{spot.SpotID}' (location: {spot.LocationId})");
            return;
        }

        spot.PrimaryNPC = npc;
    }
}