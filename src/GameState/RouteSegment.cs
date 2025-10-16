using System.Collections.Generic;

/// <summary>
/// Type of route segment
/// </summary>
public enum SegmentType
{
    FixedPath,  // Walking - predetermined path cards
    Event       // Caravan - random event from event collection
}

/// <summary>
/// Represents a segment of a travel route containing path card collection reference
/// </summary>
public class RouteSegment
{
    public int SegmentNumber { get; set; }
    public SegmentType Type { get; set; } = SegmentType.FixedPath;  // Determines selection behavior

    // For FixedPath segments
    public string PathCollectionId { get; set; }  // References AllPathCollections

    // For Event segments
    public string EventCollectionId { get; set; }  // References AllEventCollections

    // Core Loop: Path choices within this segment (1-3 paths with different trade-offs)
    // References RoutePath entities for time/stamina/obstacle combinations
    public List<RoutePath> AvailablePaths { get; set; } = new List<RoutePath>();

    // Core Loop: Narrative description of this segment location
    // Example: "Forest Approach" or "Creek Crossing"
    public string NarrativeDescription { get; set; }
}