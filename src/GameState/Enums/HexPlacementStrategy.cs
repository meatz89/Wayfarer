/// <summary>
/// Strategy for placing dependent locations on the hex grid relative to base location
/// Used when scenes create new locations dynamically through package generation
/// </summary>
public enum HexPlacementStrategy
{
/// <summary>
/// Place in any available adjacent hex (one of 6 neighbors)
/// Most common strategy for connected rooms or nearby areas
/// </summary>
Adjacent,

/// <summary>
/// Place in the same venue (same VenueId)
/// No hex placement needed - intra-venue navigation is instant
/// Used for rooms within the same building
/// </summary>
SameVenue,

/// <summary>
/// Place at a specific distance from base location
/// Used for distant but related locations
/// </summary>
Distance,

/// <summary>
/// Place in a random hex within specified radius
/// Used for hidden or exploration-based content
/// </summary>
Random
}
