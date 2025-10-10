using System.Collections.Generic;

/// <summary>
/// Goal - strategic layer that defines UI actions
/// Universal across all three challenge types (Social/Mental/Physical)
/// Determines WHERE actions appear (npcId or locationId)
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
    /// Location ID where this goal is available (Mental/Physical goals)
    /// </summary>
    public string LocationId { get; set; }

    /// <summary>
    /// NPC ID for Social system goals
    /// </summary>
    public string NpcId { get; set; }

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
    public Dictionary<PlayerStatType, int> RequiredStats { get; set; } = new Dictionary<PlayerStatType, int>();
    public int MinimumLocationFamiliarity { get; set; } = 0;
    public List<string> CompletedGoals { get; set; } = new List<string>();
}
