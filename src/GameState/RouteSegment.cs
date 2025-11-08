/// <summary>
/// Type of route segment
/// </summary>
public enum SegmentType
{
    FixedPath,  // Walking - predetermined path cards, player chooses path
    Event,      // Caravan - random event from event collection, narrative responses
    Encounter   // Challenge - mandatory scene engagement, must resolve to proceed
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

    // For Encounter segments - mandatory scene that must be resolved
    public string MandatorySceneId { get; set; }  // References Scene that player MUST engage

    // Narrative description of this segment location
    // Example: "Forest Approach" or "Creek Crossing"
    public string NarrativeDescription { get; set; }
}