/// <summary>
/// Defines one transition between Situations in a Scene cascade
/// Specifies when and how to advance from one Situation to another
/// HIGHLANDER: Uses TEMPLATE IDs only (templates are immutable archetypes)
/// </summary>
public class SituationTransition
{
    /// <summary>
    /// Source Situation template ID (where transition starts)
    /// TEMPLATE ID: References SituationTemplate.Id within SceneTemplate
    /// </summary>
    public string SourceSituationId { get; set; }

    /// <summary>
    /// Destination Situation template ID (where transition leads)
    /// TEMPLATE ID: References SituationTemplate.Id within SceneTemplate
    /// </summary>
    public string DestinationSituationId { get; set; }

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
    /// </summary>
    public string SpecificChoiceId { get; set; }
}
