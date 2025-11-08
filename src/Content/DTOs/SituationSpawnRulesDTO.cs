/// <summary>
/// DTO for SituationSpawnRules - defines cascade patterns between Situations
/// Controls how Situations sequence, branch, and converge within a Scene
/// Maps to SituationSpawnRules domain entity
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
    public string InitialSituationId { get; set; }

    /// <summary>
    /// Transition rules between Situations
    /// Defines completion → next situation links
    /// </summary>
    public List<SituationTransitionDTO> Transitions { get; set; } = new List<SituationTransitionDTO>();

    /// <summary>
    /// Condition determining when Scene is complete
    /// Values: "AllSituationsCompleted", "SpecificSituationCompleted", "AnyBranchCompleted", etc.
    /// </summary>
    public string CompletionCondition { get; set; }
}

/// <summary>
/// DTO for SituationTransition - link between two Situations
/// Defines what happens when a Situation completes
/// </summary>
public class SituationTransitionDTO
{
    /// <summary>
    /// Source Situation ID (where transition starts)
    /// References a SituationTemplate.Id within same SceneTemplate
    /// </summary>
    public string SourceSituationId { get; set; }

    /// <summary>
    /// Destination Situation ID (where transition leads)
    /// References a SituationTemplate.Id within same SceneTemplate
    /// </summary>
    public string DestinationSituationId { get; set; }

    /// <summary>
    /// Condition that triggers this transition
    /// Values: "Always", "OnChoice", "OnSuccess", "OnFailure"
    /// </summary>
    public string Condition { get; set; } = "Always";

    /// <summary>
    /// Specific Choice identifier (if Condition is OnChoice)
    /// References Choice.Id within SourceSituation
    /// null for other condition types
    /// </summary>
    public string SpecificChoiceId { get; set; }
}
