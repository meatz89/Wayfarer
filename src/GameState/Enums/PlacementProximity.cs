/// <summary>
/// Categorical spatial relationships defining placement constraints relative to reference location.
/// Used by PlacementFilter to constrain dependent location/NPC placement during procedural generation.
/// </summary>
public enum PlacementProximity
{
    /// <summary>No spatial constraint - place anywhere using standard categorical placement</summary>
    Anywhere = 0,

    /// <summary>Place at exact same location as reference (co-located entities)</summary>
    SameLocation = 1,

    /// <summary>Place at hex-adjacent location (distance = 1)</summary>
    AdjacentLocation = 2,

    /// <summary>Place within same venue (7-hex cluster)</summary>
    SameVenue = 3,

    /// <summary>Place within same district boundary</summary>
    SameDistrict = 4,

    /// <summary>Place within same region boundary</summary>
    SameRegion = 5
}
