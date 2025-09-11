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
    
    // Legacy properties for backwards compatibility
    public string CollectionId { get; set; }  // For FixedPath: single collection ID, For Event: selected from pool
    public List<string> CollectionPool { get; set; } = new();  // For Event segments: pool of collection IDs to randomly select from
}