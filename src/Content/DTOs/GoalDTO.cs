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
    /// Location ID where this goal's button appears (distributed interaction pattern)
    /// Semantic: WHERE the button is placed, not who owns the goal
    /// </summary>
    public string PlacementLocationId { get; set; }

    /// <summary>
    /// NPC ID where this goal's button appears (Social system goals)
    /// Semantic: WHERE the button is placed, not who owns the goal
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
    /// What consequence this goal has when completed
    /// Values: "Resolution", "Bypass", "Transform", "Modify", "Grant"
    /// </summary>
    public string ConsequenceType { get; set; }

    /// <summary>
    /// Resolution method to set when goal is completed
    /// Values: "Violence", "Diplomacy", "Stealth", "Authority", "Cleverness", "Preparation"
    /// </summary>
    public string ResolutionMethod { get; set; }

    /// <summary>
    /// Relationship outcome to set when goal is completed
    /// Values: "Hostile", "Neutral", "Friendly", "Allied", "Obligated"
    /// </summary>
    public string RelationshipOutcome { get; set; }

    /// <summary>
    /// New description for obstacle after Transform consequence
    /// </summary>
    public string TransformDescription { get; set; }

    /// <summary>
    /// Property requirements for goal visibility (80 Days pattern)
    /// Goal visible only if parent obstacle properties meet these thresholds
    /// </summary>
    public ObstaclePropertyRequirementsDTO PropertyRequirements { get; set; }

    /// <summary>
    /// Property reduction to apply to parent obstacle (for ReduceProperties effect)
    /// </summary>
    public ObstaclePropertyReductionDTO PropertyReduction { get; set; }

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
    public List<StatRequirementDTO> RequiredStats { get; set; } = new List<StatRequirementDTO>();
    public int MinimumLocationFamiliarity { get; set; } = 0;
    public List<string> CompletedGoals { get; set; } = new List<string>();
}

/// <summary>
/// DTO for stat requirements (strongly-typed, no Dictionary)
/// </summary>
public class StatRequirementDTO
{
    public string StatType { get; set; }
    public int MinimumLevel { get; set; }
}
