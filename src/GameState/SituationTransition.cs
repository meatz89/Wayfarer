
/// <summary>
/// Defines one transition between Situations in a Scene cascade
/// Specifies when and how to advance from one Situation to another
/// </summary>
public class SituationTransition
{
    /// <summary>
    /// Source Situation identifier (where transition starts)
    /// References Situation.Id within same Scene
    /// </summary>
    public string SourceSituationId { get; set; }

    /// <summary>
    /// Destination Situation identifier (where transition leads)
    /// References Situation.Id within same Scene
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
    /// Specific Choice identifier (if Condition is OnChoice)
    /// References Choice.Id within SourceSituation
    /// null for other condition types
    /// </summary>
    public string SpecificChoiceId { get; set; }
}
