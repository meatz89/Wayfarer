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
    /// Which challenge type this goal spawns (ID within SystemType collection)
    /// </summary>
    public string ChallengeTypeId { get; set; }

    /// <summary>
    /// Location ID where this goal is available (Mental/Physical goals)
    /// </summary>
    public string LocationId { get; set; }

    /// <summary>
    /// NPC ID for Social system goals
    /// </summary>
    public string NpcId { get; set; }

    /// <summary>
    /// Request ID for Social system goals (references NPCRequest for conversation content)
    /// </summary>
    public string NpcRequestId { get; set; }

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
    /// Goal cards (tactical layer) - inline victory conditions
    /// </summary>
    public List<GoalCardDTO> GoalCards { get; set; } = new List<GoalCardDTO>();

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
