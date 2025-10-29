using Wayfarer.GameState.Enums;

/// <summary>
/// Defines how Situations within a Scene lead into each other
/// Creates rule cascades - narrative progression patterns
/// Embedded in Scene, not stored separately
/// </summary>
public class SituationSpawnRules
{
    /// <summary>
    /// Pattern type classification
    /// Linear, HubAndSpoke, Branching, Converging, etc.
    /// Describes the overall cascade structure
    /// </summary>
    public SpawnPattern Pattern { get; set; } = SpawnPattern.Linear;

    /// <summary>
    /// Initial Situation identifier (what player sees first)
    /// References Situation.Id within Scene
    /// Scene starts with this Situation active
    /// </summary>
    public string InitialSituationId { get; set; }

    /// <summary>
    /// List of transitions between Situations
    /// Defines how completing one Situation leads to next
    /// Empty list for Standalone pattern
    /// </summary>
    public List<SituationTransition> Transitions { get; set; } = new List<SituationTransition>();

    /// <summary>
    /// Completion condition identifier (optional)
    /// Defines when Scene is considered complete
    /// null = Scene completes when no more active Situations
    /// Can reference specific Situation completion, all Situations complete, etc.
    /// </summary>
    public string CompletionCondition { get; set; }
}
