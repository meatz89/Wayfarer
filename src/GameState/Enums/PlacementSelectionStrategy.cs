/// <summary>
/// Strategy for selecting ONE entity from multiple matching candidates during placement.
/// Used by PlacementFilter to control HOW the final placement is chosen when multiple entities match the filter.
/// DDR-007: All strategies are DETERMINISTIC - no Random in strategic layer.
/// </summary>
public enum PlacementSelectionStrategy
{
    /// <summary>
    /// Select first from Name-sorted candidates (default).
    /// DDR-007: Deterministic - same filter always produces same result.
    /// Provides variety through natural entity ordering, not randomness.
    /// </summary>
    First,

    /// <summary>
    /// Select spatially closest entity to player's current position.
    /// Uses hex grid distance calculation.
    /// Good for "nearby encounter" scenarios.
    /// Requires entities to have hex positions.
    /// </summary>
    Closest,

    /// <summary>
    /// Select NPC with highest bond strength (NPC placement only).
    /// Good for "trusted ally" or "close friend" scenarios.
    /// Falls back to First if no bond differences.
    /// </summary>
    HighestBond,

    /// <summary>
    /// Select entity least recently interacted with.
    /// Good for variety and rotating encounters.
    /// Requires interaction history tracking.
    /// Falls back to First if no interaction history.
    /// </summary>
    LeastRecent
}
