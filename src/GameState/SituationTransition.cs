
/// <summary>
/// Defines one transition between Situations in a Scene cascade
/// Specifies when and how to advance from one Situation to another
/// ADR-007: HIGHLANDER - object references only, no string IDs
/// </summary>
public class SituationTransition
{
    /// <summary>
    /// ADR-007: Source Situation object reference (where transition starts)
    /// HIGHLANDER: Object reference only, no SourceSituationId
    /// </summary>
    public Situation SourceSituation { get; set; }

    /// <summary>
    /// ADR-007: Destination Situation object reference (where transition leads)
    /// HIGHLANDER: Object reference only, no DestinationSituationId
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
    /// ADR-007: Specific Choice template reference (if Condition is OnChoice)
    /// HIGHLANDER: Object reference only, no SpecificChoiceId
    /// null for other condition types
    /// </summary>
    public ChoiceTemplate SpecificChoice { get; set; }
}
