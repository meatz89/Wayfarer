
/// <summary>
/// Defines one transition between Situations in a Scene cascade
/// Specifies when and how to advance from one Situation to another
/// ADR-007: Uses TEMPLATE IDs at template tier, object references at runtime tier
/// </summary>
public class SituationTransition
{
    /// <summary>
    /// Source Situation template ID (where transition starts)
    /// TEMPLATE ID: References SituationTemplate.Id within SceneTemplate
    /// Used at template tier (SceneTemplate, catalogues)
    /// Acceptable per ADR-007 (templates are immutable archetypes)
    /// </summary>
    public string SourceSituationId { get; set; }

    /// <summary>
    /// Destination Situation template ID (where transition leads)
    /// TEMPLATE ID: References SituationTemplate.Id within SceneTemplate
    /// Used at template tier (SceneTemplate, catalogues)
    /// Acceptable per ADR-007 (templates are immutable archetypes)
    /// </summary>
    public string DestinationSituationId { get; set; }

    /// <summary>
    /// Source Situation object reference (runtime tier)
    /// Populated during Scene instantiation from template
    /// Used at runtime tier (Scene.SpawnRules)
    /// </summary>
    public Situation SourceSituation { get; set; }

    /// <summary>
    /// Destination Situation object reference (runtime tier)
    /// Populated during Scene instantiation from template
    /// Used at runtime tier (Scene.SpawnRules)
    /// </summary>
    public Situation DestinationSituation { get; set; }

    /// <summary>
    /// Condition that triggers this transition
    /// Always: Always occurs
    /// OnChoice: Occurs if specific choice selected
    /// OnSuccess: Occurs if choice succeeded
    /// OnFailure: Occurs if choice failed
    /// </summary>
    public TransitionCondition Condition { get; set; }

    /// <summary>
    /// Specific Choice template ID (if Condition is OnChoice)
    /// TEMPLATE ID: References ChoiceTemplate.Id
    /// null for other condition types
    /// Acceptable per ADR-007 (templates are immutable archetypes)
    /// </summary>
    public string SpecificChoiceId { get; set; }

    /// <summary>
    /// Specific Choice object reference (runtime tier)
    /// Populated during Scene instantiation
    /// null for other condition types
    /// </summary>
    public ChoiceTemplate SpecificChoice { get; set; }
}
