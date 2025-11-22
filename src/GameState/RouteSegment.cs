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
/// HIGHLANDER: Object references ONLY - resolved at parse-time, no runtime lookups
/// </summary>
public class RouteSegment
{
    public int SegmentNumber { get; set; }
    public SegmentType Type { get; set; } = SegmentType.FixedPath;  // Determines selection behavior

    // For FixedPath segments - HIGHLANDER: Object reference, not PathCollectionId string
    public PathCardCollectionDTO PathCollection { get; set; }

    // For Event segments - HIGHLANDER: Object reference, not EventCollectionId string
    public PathCardCollectionDTO EventCollection { get; set; }

    // For Encounter segments - mandatory scene template that must be spawned
    // HIGHLANDER: Object reference to template, not MandatorySceneId string
    // Runtime spawns Scene instance from this template
    public SceneTemplate MandatorySceneTemplate { get; set; }

    // Narrative description of this segment location
    // Example: "Forest Approach" or "Creek Crossing"
    public string NarrativeDescription { get; set; }
}