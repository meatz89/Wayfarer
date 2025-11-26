/// <summary>
/// Template for procedural venue generation.
/// Defines categorical requirements for dynamically created venues.
/// Parallel to PlacementFilter for locations.
/// </summary>
public class VenueTemplate
{
    /// <summary>
    /// Name pattern with placeholder support.
    /// Example: "{DistrictName} {VenueType}" → "Lower Wards Tavern"
    /// </summary>
    public string NamePattern { get; set; }

    /// <summary>
    /// Description pattern with placeholder support.
    /// Example: "A {quality} {venueType} in the {districtName}."
    /// </summary>
    public string DescriptionPattern { get; set; }

    /// <summary>
    /// Venue type category (Tavern, Inn, Market, etc.)
    /// </summary>
    public VenueType Type { get; set; } = VenueType.Wilderness;

    /// <summary>
    /// Difficulty/progression tier (1-5)
    /// </summary>
    public int Tier { get; set; } = 1;

    /// <summary>
    /// District ID to place venue in.
    /// null = inherit from context or default to wilderness
    /// </summary>
    public string District { get; set; }

    /// <summary>
    /// Maximum total locations allowed in this venue.
    /// Small venues: 5-10 (constrained, intimate)
    /// Large venues: 50-100 (expansive, variety)
    /// </summary>
    public int MaxLocations { get; set; } = 20;

    /// <summary>
    /// Hex allocation strategy for venue placement.
    /// </summary>
    public HexAllocationStrategy HexAllocation { get; set; } = HexAllocationStrategy.ClusterOf7;
}

/// <summary>
/// Strategy for allocating hexes to generated venue.
/// </summary>
public enum HexAllocationStrategy
{
    /// <summary>
    /// Traditional venue: allocate 7-hex cluster (center + 6 neighbors)
    /// Intra-venue movement is instant/free between adjacent hexes
    /// </summary>
    ClusterOf7,

    /// <summary>
    /// Minimal venue: single hex only
    /// Used for temporary spaces, wilderness camps, small structures
    /// </summary>
    SingleHex
}
