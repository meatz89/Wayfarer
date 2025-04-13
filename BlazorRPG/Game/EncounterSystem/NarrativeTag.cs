/// <summary>
/// Represents an environmental modifier that adjusts the focus requirement for playing choices based on narrative conditions.
/// For example, a calm atmosphere may ease the effort needed to gather information,
/// while a chaotic state might make social interactions more challenging.
/// </summary>
public class NarrativeTag : IEncounterTag
{
    public string NarrativeName { get; }
    public FocusTags AffectedFocus { get; }
    public int RequirementChange { get; }

    public NarrativeTag(string narrativeName, FocusTags affectedFocus, int requirementChange)
    {
        NarrativeName = narrativeName;
        AffectedFocus = affectedFocus;
        RequirementChange = requirementChange;
    }

    public bool IsActive(EncounterTagSystem tagSystem)
    {
        return true;
    }

    public string GetEffectDescription()
    {
        if (RequirementChange > 0)
        {
            return $"Increase required {AffectedFocus} by {RequirementChange}";
        }
        else if (RequirementChange < 0)
        {
            return $"Decrease required {AffectedFocus} by {-RequirementChange}";
        }
        return string.Empty;
    }
}