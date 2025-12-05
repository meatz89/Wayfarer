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
    /// Initial Situation template identifier (what player sees first)
    /// References SituationTemplate.Id within SceneTemplate
    /// Scene starts with this Situation active
    /// TEMPLATE ID: Acceptable per ADR-007 (templates are immutable archetypes)
    /// </summary>
    public string InitialSituationTemplateId { get; set; }

    // HIGHLANDER: Transitions property REMOVED (see arc42 §8.30)
    // All flow control through Consequence.NextSituationTemplateId and IsTerminal
    // Different choices can now lead to different situations within the same scene

    /// <summary>
    /// Completion condition identifier (optional)
    /// Defines when Scene is considered complete
    /// null = Scene completes when no more active Situations
    /// Can reference specific Situation completion, all Situations complete, etc.
    /// </summary>
    public string CompletionCondition { get; set; }
}
