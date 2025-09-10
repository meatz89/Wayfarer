using System.Collections.Generic;

/// <summary>
/// Represents a segment of a travel route containing multiple path card options
/// </summary>
public class RouteSegment
{
    public int SegmentNumber { get; set; }
    public List<string> PathCardIds { get; set; } = new();
}