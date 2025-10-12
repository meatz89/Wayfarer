using System.Collections.Generic;
using Wayfarer.GameState.Enums;

/// <summary>
/// Goal - approach to overcome obstacle (lives inside obstacle as child)
/// Universal across all three challenge types (Social/Mental/Physical)
/// PlacementLocationId/PlacementNpcId determines WHERE button appears (not ownership)
/// </summary>
public class Goal
{
    /// <summary>
    /// Unique identifier for the goal
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Display name for the goal
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Narrative description of the goal
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// THREE PARALLEL SYSTEMS - which tactical system this goal uses
    /// </summary>
    public TacticalSystemType SystemType { get; set; } = TacticalSystemType.Social;

    /// <summary>
    /// The deck ID this goal uses for challenge generation
    /// </summary>
    public string DeckId { get; set; }

    /// <summary>
    /// Location ID where this goal's button appears in UI (semantic: placement, not ownership)
    /// Used for Mental/Physical goals and distributed obstacle goals
    /// </summary>
    public string PlacementLocationId { get; set; }

    /// <summary>
    /// NPC ID where this goal's button appears in UI (semantic: placement, not ownership)
    /// Used for Social goals and distributed obstacle goals
    /// </summary>
    public string PlacementNpcId { get; set; }

    /// <summary>
    /// Investigation ID for UI grouping and label display
    /// </summary>
    public string InvestigationId { get; set; }

    /// <summary>
    /// Whether this goal is an investigation intro action
    /// </summary>
    public bool IsIntroAction { get; set; } = false;

    /// <summary>
    /// Category that must match the challenge type's category (for Social system)
    /// </summary>
    public string Category { get; set; }

    /// <summary>
    /// Connection type (token type) for this goal (for Social system)
    /// </summary>
    public ConnectionType ConnectionType { get; set; } = ConnectionType.Trust;

    /// <summary>
    /// Current status of the goal
    /// </summary>
    public GoalStatus Status { get; set; } = GoalStatus.Available;

    /// <summary>
    /// Whether this goal is currently available
    /// </summary>
    public bool IsAvailable { get; set; } = true;

    /// <summary>
    /// Whether this goal has been completed
    /// </summary>
    public bool IsCompleted { get; set; } = false;

    /// <summary>
    /// Whether this goal should be deleted from ActiveGoals on successful completion.
    /// Investigation progression goals: true (one-time, remove after complete)
    /// Repeatable goals: false (persist for retry)
    /// Default: true (investigation progression pattern)
    /// </summary>
    public bool DeleteOnSuccess { get; set; } = true;

    /// <summary>
    /// Prerequisites for this goal to be available
    /// </summary>
    public GoalRequirements Requirements { get; set; }

    /// <summary>
    /// Goal cards (tactical layer) - inline victory conditions
    /// Each goal card defines a momentum threshold and rewards
    /// </summary>
    public List<GoalCard> GoalCards { get; set; } = new List<GoalCard>();

    /// <summary>
    /// What consequence occurs when goal succeeds
    /// Resolution: Obstacle permanently overcome, removed from play
    /// Bypass: Player passes, obstacle persists
    /// Transform: Obstacle fundamentally changed, properties set to 0
    /// Modify: Obstacle properties reduced, other goals may unlock
    /// Grant: Player receives knowledge/items, obstacle unchanged
    /// </summary>
    public ConsequenceType ConsequenceType { get; set; } = ConsequenceType.Grant;

    /// <summary>
    /// Resolution method this goal sets when completed (for AI narrative context)
    /// </summary>
    public ResolutionMethod SetsResolutionMethod { get; set; } = ResolutionMethod.Unresolved;

    /// <summary>
    /// Relationship outcome this goal sets when completed (affects future interactions)
    /// </summary>
    public RelationshipOutcome SetsRelationshipOutcome { get; set; } = RelationshipOutcome.Neutral;

    /// <summary>
    /// New description for obstacle if Transform consequence (replaces obstacle description)
    /// </summary>
    public string TransformDescription { get; set; }

    /// <summary>
    /// Property requirements for goal visibility (80 Days pattern)
    /// Goal visible only if parent obstacle properties meet these thresholds
    /// null = always visible (for ambient goals)
    /// </summary>
    public ObstaclePropertyRequirements PropertyRequirements { get; set; }

    /// <summary>
    /// Property reduction to apply to parent obstacle (for Modify consequence)
    /// Unlocks better resolution options by lowering obstacle difficulty
    /// null for Resolution, Bypass, Transform, and Grant consequence types
    /// </summary>
    public ObstaclePropertyReduction PropertyReduction { get; set; }

    /// <summary>
    /// Check if this goal is available to attempt
    /// </summary>
    public bool IsAvailableToAttempt()
    {
        return Status == GoalStatus.Available && IsAvailable;
    }

    /// <summary>
    /// Mark this goal as completed
    /// </summary>
    public void Complete()
    {
        Status = GoalStatus.Completed;
        IsCompleted = true;
    }
}

/// <summary>
/// Requirements for a goal to be available
/// </summary>
public class GoalRequirements
{
    public List<string> RequiredKnowledge { get; set; } = new List<string>();
    public List<string> RequiredEquipment { get; set; } = new List<string>();
    public List<StatRequirement> RequiredStats { get; set; } = new List<StatRequirement>();
    public int MinimumLocationFamiliarity { get; set; } = 0;
    public List<string> CompletedGoals { get; set; } = new List<string>();
}

/// <summary>
/// Stat requirement (strongly-typed, no Dictionary)
/// </summary>
public class StatRequirement
{
    public PlayerStatType StatType { get; set; }
    public int MinimumLevel { get; set; }
}
