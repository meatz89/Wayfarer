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
    /// Bodily harm risk - combat, falling, traps, structural hazards
    /// Natural meaning: actual physical danger in game world
    /// </summary>
    public int PhysicalDanger { get; set; }

    /// <summary>
    /// Cognitive load - puzzle difficulty, pattern obscurity, evidence volume
    /// Natural meaning: actual mental challenge complexity
    /// </summary>
    public int MentalComplexity { get; set; }

    /// <summary>
    /// Interpersonal challenge - suspicious NPC, hostile faction, complex negotiation
    /// Natural meaning: actual social barrier difficulty
    /// Scale: 0 (trivial) to 3 (severe)
    /// </summary>
    public int SocialDifficulty { get; set; }

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
    /// Check if obstacle is fully cleared (all three properties at or below zero)
    /// </summary>
    public bool IsCleared()
    {
        return PhysicalDanger <= 0 &&
               MentalComplexity <= 0 &&
               SocialDifficulty <= 0;
    }

    /// <summary>
    /// Get total challenge magnitude (sum of three core properties)
    /// Useful for UI display of overall difficulty
    /// </summary>
    public int GetTotalMagnitude()
    {
        return PhysicalDanger + MentalComplexity + SocialDifficulty;
    }
}
