/// <summary>
/// Represents an environmental modifier that adjusts the focus requirement for playing choices based on narrative conditions.
/// For example, a calm atmosphere may ease the effort needed to gather information,
/// while a chaotic state might make social interactions more challenging.
/// </summary>
public class NarrativeTag : IEncounterTag
{
    public string NarrativeName { get; }
    public ApproachTags AffectedApproach { get; }
    public FocusTags AffectedFocus { get; }
    public int RequirementChangeApproach { get; }
    public int RequirementChangeFocus { get; }

    public NarrativeTag(string narrativeName, FocusTags affectedFocus, int requirementChange)
    {
        NarrativeName = narrativeName;
        AffectedFocus = affectedFocus;
        RequirementChangeFocus = requirementChange;
    }

    public bool IsActive(EncounterTagSystem tagSystem)
    {
        return true;
    }

    public string GetEffectDescription()
    {
        if (RequirementChangeFocus > 0)
        {
            return $"Increase required {AffectedFocus} by {RequirementChangeFocus}";
        }
        else if (RequirementChangeFocus < 0)
        {
            return $"Decrease required {AffectedFocus} by {-RequirementChangeFocus}";
        }
        return string.Empty;
    }
}