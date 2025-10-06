using System.Collections.Generic;

/// <summary>
/// Goal available at a location spot that spawns tactical engagement
/// Parallel to NPCRequest but for location-based Mental/Physical challenges
/// </summary>
public class LocationGoal
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    /// <summary>
    /// THREE PARALLEL SYSTEMS - which tactical system this goal uses
    /// </summary>
    public TacticalSystemType SystemType { get; set; }

    /// <summary>
    /// Which engagement type this goal spawns (ID within SystemType collection)
    /// </summary>
    public string EngagementTypeId { get; set; }
    
    /// <summary>
    /// Spot ID where this goal is available (null = available at all spots in location)
    /// </summary>
    public string SpotId { get; set; }
    /// <summary>
    /// Investigation ID for UI grouping and label display
    /// If set, this goal is part of an investigation
    /// </summary>
    public string InvestigationId { get; set; }

    /// <summary>
    /// Prerequisites for this goal to be available
    /// </summary>
    public GoalRequirements Requirements { get; set; }

    /// <summary>
    /// Whether this goal is currently available
    /// </summary>
    public bool IsAvailable { get; set; } = true;

    /// <summary>
    /// Whether this goal has been completed
    /// </summary>
    public bool IsCompleted { get; set; } = false;
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
