namespace Wayfarer.GameState.Enums;

/// <summary>
/// Placement relation types for Scene spawning
/// Defines where a new Scene spawns relative to the context where Choice was made
/// </summary>
public enum PlacementRelation
{
    /// <summary>
    /// Spawn at same location where Choice was executed
    /// Context-relative placement
    /// </summary>
    SameLocation,

    /// <summary>
    /// Spawn at same NPC where Choice was executed
    /// Context-relative placement
    /// </summary>
    SameNPC,

    /// <summary>
    /// Spawn on same route where Choice was executed
    /// Context-relative placement
    /// </summary>
    SameRoute,

    /// <summary>
    /// Spawn at specific location (ID provided in SpecificPlacementId)
    /// Absolute placement
    /// </summary>
    SpecificLocation,

    /// <summary>
    /// Spawn at specific NPC (ID provided in SpecificPlacementId)
    /// Absolute placement
    /// </summary>
    SpecificNPC,

    /// <summary>
    /// Spawn on specific route (ID provided in SpecificPlacementId)
    /// Absolute placement
    /// </summary>
    SpecificRoute
}
