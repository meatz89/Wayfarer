/// <summary>
/// Defines categorical spatial constraint for placing dependent locations/NPCs.
/// Scaffolding class: stored temporarily during parsing, used during placement, then cleared.
/// NOT persisted in game state - purely procedural generation metadata.
///
/// DISTINCTION FROM PlacementFilter:
/// - PlacementFilter: Defines WHAT to select (categorical entity properties)
/// - ProximityConstraint: Defines WHERE to place it (spatial relationship to reference)
/// </summary>
public class ProximityConstraint
{
    /// <summary>
    /// Categorical proximity relationship to reference location.
    /// Determines spatial constraint during placement.
    /// </summary>
    public PlacementProximity Proximity { get; set; } = PlacementProximity.Anywhere;

    /// <summary>
    /// Key identifying reference location in activation context.
    /// Common values: "current" (activation location), "origin" (scene origin), "player" (player's location).
    /// </summary>
    public string ReferenceLocationKey { get; set; } = "current";
}
