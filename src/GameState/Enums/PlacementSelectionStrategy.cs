/// <summary>
/// Strategy for selecting ONE entity from multiple matching candidates during placement
/// Used by PlacementFilter to control HOW the final placement is chosen when multiple entities match the filter
/// </summary>
public enum PlacementSelectionStrategy
{
    /// <summary>
    /// Select randomly from all matching candidates (default)
    /// Uses RNG for unpredictable variety
    /// Good for general-purpose procedural content
    /// </summary>
    WeightedRandom,

    /// <summary>
    /// Select first matching entity from filtered results
    /// Deterministic selection (always same result for same filter)
    /// Good for tutorial/story-critical bindings
    /// </summary>
    First,

    /// <summary>
    /// Select spatially closest entity to player's current position
    /// Uses hex grid distance calculation
    /// Good for "nearby encounter" scenarios
    /// Requires entities to have hex positions
    /// </summary>
    Closest,

    /// <summary>
    /// Select NPC with highest bond strength (NPC placement only)
    /// Good for "trusted ally" or "close friend" scenarios
    /// Falls back to Random if no bond differences
    /// </summary>
    HighestBond,

    /// <summary>
    /// Select entity least recently interacted with
    /// Good for variety and rotating encounters
    /// Requires interaction history tracking
    /// Falls back to Random if no interaction history
    /// </summary>
    LeastRecent
}
