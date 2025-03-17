public class NarrativeTag : IEncounterTag
{
    public string Name { get; }
    public ActivationCondition Condition { get; }
    public FocusTags BlockedFocus { get; }

    public NarrativeTag(string name, ActivationCondition condition, FocusTags blockedFocus)
    {
        Name = name;
        Condition = condition;
        BlockedFocus = blockedFocus;
    }

    public bool IsActive(BaseTagSystem tagSystem)
    {
        return Condition.IsActive(tagSystem);
    }

    public void ApplyEffect(EncounterState state)
    {
        // Narrative tags don't directly affect state, only card selection
        // The blocked focus is used by the choice generation algorithm
    }

    public string GetActivationDescription()
    {
        return Condition.GetDescription();
    }

    public string GetEffectDescription()
    {
        return $"Blocks {BlockedFocus} focus choices";
    }
}