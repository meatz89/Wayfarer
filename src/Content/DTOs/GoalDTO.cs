using System.Collections.Generic;

/// <summary>
/// DTO for Goals - strategic layer that defines UI actions
/// Universal across all three challenge types (Social/Mental/Physical)
/// </summary>
public class GoalDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    /// <summary>
    /// THREE PARALLEL SYSTEMS - which tactical system this goal uses
    /// </summary>
    public string SystemType { get; set; }

    /// <summary>
    /// Which deck this goal uses for challenge generation
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
    /// Whether this goal is currently available
    /// </summary>
    public bool IsAvailable { get; set; } = true;

    /// <summary>
    /// Whether this goal has been completed
    /// </summary>
    public bool IsCompleted { get; set; } = false;

    /// <summary>
    /// Whether this goal should be deleted from ActiveGoals on successful completion.
    /// Default: true (investigation progression pattern)
    /// </summary>
    public bool DeleteOnSuccess { get; set; } = true;

    /// <summary>
    /// Goal cards (tactical layer) - inline victory conditions
    /// </summary>
    public List<GoalCardDTO> GoalCards { get; set; } = new List<GoalCardDTO>();

    /// <summary>
    /// Optional index into parent entity's Obstacles list
    /// Parser uses this to set Goal.TargetObstacle reference
    /// null = standalone goal not targeting any obstacle
    /// </summary>
    public int? TargetObstacleIndex { get; set; }

    /// <summary>
    /// Prerequisites for this goal to be available
    /// </summary>
    public GoalRequirementsDTO Requirements { get; set; }
}

/// <summary>
/// DTO for Goal requirements
/// </summary>
public class GoalRequirementsDTO
{
    public List<string> RequiredKnowledge { get; set; } = new List<string>();
    public List<string> RequiredEquipment { get; set; } = new List<string>();
    public Dictionary<string, int> RequiredStats { get; set; } = new Dictionary<string, int>();
    public int MinimumLocationFamiliarity { get; set; } = 0;
    public List<string> CompletedGoals { get; set; } = new List<string>();
}
