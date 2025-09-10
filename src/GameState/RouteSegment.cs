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
/// Represents a segment of a travel route containing multiple path card options
/// </summary>
public class RouteSegment
{
    public int SegmentNumber { get; set; }
    public List<string> PathCardIds { get; set; } = new();
    public SegmentType Type { get; set; } = SegmentType.FixedPath;  // Default to FixedPath for compatibility
    public string EventCollectionId { get; set; }  // Used when Type is Event
}