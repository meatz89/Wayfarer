/// <summary>
/// Represents an environmental modifier
/// For example, a calm atmosphere may ease the effort needed to gather information,
/// while a chaotic state might make social interactions more challenging.
/// </summary>
public class NarrativeTag : IEncounterTag
{
    public string NarrativeName { get; }
    public int RequirementChangeApproach { get; }
    public int RequirementChangeFocus { get; }

    public NarrativeTag(string narrativeName, int requirementChange)
    {
        NarrativeName = narrativeName;
        RequirementChangeFocus = requirementChange;
    }

    internal string GetEffectDescription()
    {
        return NarrativeName;
    }
}