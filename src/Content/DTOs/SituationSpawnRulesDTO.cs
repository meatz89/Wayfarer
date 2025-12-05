/// <summary>
/// DTO for SituationSpawnRules - defines cascade patterns between Situations
/// Controls how Situations sequence, branch, and converge within a Scene
/// Maps to SituationSpawnRules domain entity
/// HIGHLANDER: Flow control now through Consequence.NextSituationTemplateId (arc42 §8.30)
/// </summary>
public class SituationSpawnRulesDTO
{
    /// <summary>
    /// Pattern type classification
    /// Values: "Linear", "HubAndSpoke", "Branching", "Converging", "Discovery", etc.
    /// Maps to SpawnPattern enum
    /// </summary>
    public string Pattern { get; set; }

    /// <summary>
    /// First Situation ID player sees when Scene activates
    /// References a SituationTemplate.Id within parent SceneTemplate
    /// </summary>
    public string InitialSituationTemplateId { get; set; }

    // HIGHLANDER: Transitions property REMOVED (arc42 §8.30)
    // Flow control now through Consequence.NextSituationTemplateId and IsTerminal
    // Different choices can lead to different situations within the same scene

    /// <summary>
    /// Condition determining when Scene is complete
    /// Values: "AllSituationsCompleted", "SpecificSituationCompleted", "AnyBranchCompleted", etc.
    /// </summary>
    public string CompletionCondition { get; set; }
}

// HIGHLANDER: SituationTransitionDTO class DELETED (arc42 §8.30)
// Flow control now through Consequence.NextSituationTemplateId and IsTerminal
