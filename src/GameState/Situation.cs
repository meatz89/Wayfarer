using System.Collections.Generic;
using Wayfarer.GameState.Enums;

/// <summary>
/// Situation - approach to overcome obstacle (lives inside obstacle as child)
/// Universal across all three challenge types (Social/Mental/Physical)
/// PlacementLocationId/PlacementNpcId determines WHERE button appears (not ownership)
/// </summary>
public class Situation
{
    /// <summary>
    /// Unique identifier for the situation
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Display name for the situation
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Narrative description of the situation
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// THREE PARALLEL SYSTEMS - which tactical system this situation uses
    /// </summary>
    public TacticalSystemType SystemType { get; set; } = TacticalSystemType.Social;

    /// <summary>
    /// The deck ID this situation uses for challenge generation
    /// </summary>
    public string DeckId { get; set; }

    /// <summary>
    /// Location ID where this situation's button appears in UI (semantic: placement, not ownership)
    /// Used for Mental/Physical situations and distributed obstacle situations
    /// </summary>
    public string PlacementLocationId { get; set; }

    /// <summary>
    /// NPC ID where this situation's button appears in UI (semantic: placement, not ownership)
    /// Used for Social situations and distributed obstacle situations
    /// </summary>
    public string PlacementNpcId { get; set; }

    /// <summary>
    /// Route ID where this situation's button appears in UI (semantic: placement, not ownership)
    /// Used for route-based situations (scouting, pathfinding) that grant ExplorationCubes
    /// </summary>
    public string PlacementRouteId { get; set; }

    /// <summary>
    /// Obligation ID for UI grouping and label display
    /// </summary>
    public string ObligationId { get; set; }

    /// <summary>
    /// Object reference to placement location (for runtime navigation)
    /// </summary>
    public Location PlacementLocation { get; set; }

    /// <summary>
    /// Object reference to placement NPC (for runtime navigation)
    /// </summary>
    public NPC PlacementNpc { get; set; }

    /// <summary>
    /// Object reference to parent obligation (for runtime navigation)
    /// </summary>
    public Obligation Obligation { get; set; }

    /// <summary>
    /// Object reference to parent obstacle (for runtime navigation)
    /// Populated at initialization time from obstacle's SituationIds
    /// </summary>
    public Obstacle ParentObstacle { get; set; }

    /// <summary>
    /// Whether this situation is an obligation intro action
    /// </summary>
    public bool IsIntroAction { get; set; } = false;

    /// <summary>
    /// Category that must match the challenge type's category (for Social system)
    /// </summary>
    public string Category { get; set; }

    /// <summary>
    /// Connection type (token type) for this situation (for Social system)
    /// </summary>
    public ConnectionType ConnectionType { get; set; } = ConnectionType.Trust;

    /// <summary>
    /// Current status of the situation
    /// </summary>
    public SituationStatus Status { get; set; } = SituationStatus.Available;

    /// <summary>
    /// Whether this situation is currently available
    /// </summary>
    public bool IsAvailable { get; set; } = true;

    /// <summary>
    /// Whether this situation has been completed
    /// </summary>
    public bool IsCompleted { get; set; } = false;

    /// <summary>
    /// Whether this situation should be deleted from ActiveSituations on successful completion.
    /// Obligation progression situations: true (one-time, remove after complete)
    /// Repeatable situations: false (persist for retry)
    /// Default: true (obligation progression pattern)
    /// </summary>
    public bool DeleteOnSuccess { get; set; } = true;

    /// <summary>
    /// Resources player must pay to attempt this situation
    /// Transparent costs create resource competition and strategic choices
    /// Board game pattern: Situation A costs 20 Focus, Situation B costs 30 Focus - choose wisely
    /// </summary>
    public SituationCosts Costs { get; set; } = new SituationCosts();

    /// <summary>
    /// Difficulty modifiers that reduce/increase difficulty based on player state
    /// Situation ALWAYS visible, difficulty varies transparently
    /// Multiple paths to reduce difficulty create strategic choices
    /// Example: Understanding 2 reduces Exposure by 3, Familiarity 2 reduces Exposure by 2
    /// No boolean gates: All situations always visible, modifiers just change difficulty
    /// </summary>
    public List<DifficultyModifier> DifficultyModifiers { get; set; } = new List<DifficultyModifier>();

    /// <summary>
    /// Situation cards (tactical layer) - inline victory conditions
    /// Each situation card defines a momentum threshold and rewards
    /// </summary>
    public List<SituationCard> SituationCards { get; set; } = new List<SituationCard>();

    /// <summary>
    /// What consequence occurs when situation succeeds
    /// Resolution: Obstacle permanently overcome, removed from play
    /// Bypass: Player passes, obstacle persists
    /// Transform: Obstacle fundamentally changed, properties set to 0
    /// Modify: Obstacle properties reduced, other situations may unlock
    /// Grant: Player receives knowledge/items, obstacle unchanged
    /// </summary>
    public ConsequenceType ConsequenceType { get; set; } = ConsequenceType.Grant;

    /// <summary>
    /// Resolution method this situation sets when completed (for AI narrative context)
    /// </summary>
    public ResolutionMethod SetsResolutionMethod { get; set; } = ResolutionMethod.Unresolved;

    /// <summary>
    /// Relationship outcome this situation sets when completed (affects future interactions)
    /// </summary>
    public RelationshipOutcome SetsRelationshipOutcome { get; set; } = RelationshipOutcome.Neutral;

    /// <summary>
    /// New description for obstacle if Transform consequence (replaces obstacle description)
    /// </summary>
    public string TransformDescription { get; set; }

    /// <summary>
    /// Property reduction to apply to parent obstacle (for Modify consequence)
    /// Reduces obstacle intensity, making other situations easier (NOT unlocking them)
    /// null for Resolution, Bypass, Transform, and Grant consequence types
    /// </summary>
    public ObstaclePropertyReduction PropertyReduction { get; set; }

    /// <summary>
    /// Check if this situation is available to attempt
    /// </summary>
    public bool IsAvailableToAttempt()
    {
        return Status == SituationStatus.Available && IsAvailable;
    }

    /// <summary>
    /// Mark this situation as completed
    /// </summary>
    public void Complete()
    {
        Status = SituationStatus.Completed;
        IsCompleted = true;
    }
}
