using System.Collections.Generic;
using Wayfarer.GameState.Enums;

/// <summary>
/// Obstacle - Strategic barrier with inline goals, property-based gating
/// Lives in GameWorld.Obstacles list (single source of truth)
/// Referenced by Location.ObstacleIds, NPC.ObstacleIds (distributed interaction pattern)
/// Design principle: Location-agnostic obstacles with goals scattered across world
/// </summary>
public class Obstacle
{
    /// <summary>
    /// Unique identifier for this obstacle (used for lookups from ObstacleIds lists)
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Narrative name for this obstacle
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// What player sees and understands about this obstacle
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Context tags that describe this obstacle's nature
    /// Used for equipment context matching to reduce intensity
    /// Examples: ObstacleContext.Darkness, ObstacleContext.Climbing, ObstacleContext.Water
    /// Strongly-typed enum ensures compile-time validation
    /// </summary>
    public List<ObstacleContext> Contexts { get; set; } = new List<ObstacleContext>();

    /// <summary>
    /// Difficulty level of this obstacle (1-3 scale)
    /// 1 = Easy, 2 = Moderate, 3 = Hard
    /// Can be reduced by equipped items with matching contexts
    /// Obstacle cleared when Intensity reaches 0
    /// </summary>
    public int Intensity { get; set; }

    /// <summary>
    /// Current state of obstacle
    /// </summary>
    public ObstacleState State { get; set; } = ObstacleState.Active;

    /// <summary>
    /// How obstacle was overcome (provides AI narrative context)
    /// </summary>
    public ResolutionMethod ResolutionMethod { get; set; } = ResolutionMethod.Unresolved;

    /// <summary>
    /// Social impact of resolution (affects future interactions)
    /// </summary>
    public RelationshipOutcome RelationshipOutcome { get; set; } = RelationshipOutcome.Neutral;

    /// <summary>
    /// New description after Transform consequence (if transformed)
    /// </summary>
    public string TransformedDescription { get; set; }

    /// <summary>
    /// Whether obstacle persists when all properties reach zero
    /// false: Removed when cleared (investigation obstacles, quest obstacles)
    /// true: Persists even at zero, can increase again (weather obstacles, patrol obstacles)
    /// </summary>
    public bool IsPermanent { get; set; }

    /// <summary>
    /// Goal IDs that target this obstacle (references to GameWorld.Goals)
    /// Two types: preparation goals (reduce properties) and resolution goals (remove obstacle)
    /// Goals from investigation content are registered in GameWorld.Goals when obstacle spawns
    /// Filtered by property requirements for visibility
    /// Single source of truth: All goals live in GameWorld.Goals dictionary
    /// </summary>
    public List<string> GoalIds { get; set; } = new List<string>();

    /// <summary>
    /// Check if obstacle is fully cleared (intensity at or below zero)
    /// </summary>
    public bool IsCleared()
    {
        return Intensity <= 0;
    }

    /// <summary>
    /// Get challenge magnitude (returns current intensity)
    /// Useful for UI display of overall difficulty
    /// </summary>
    public int GetTotalMagnitude()
    {
        return Intensity;
    }
}
