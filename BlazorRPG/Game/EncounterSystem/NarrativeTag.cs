using BlazorRPG.Game.EncounterManager;

public class NarrativeTag : IEncounterTag
{
    public string Name { get; }
    public ApproachTags? BlockedApproach { get; }
    public ActivationCondition Condition { get; }

    public NarrativeTag(string name, ActivationCondition condition, ApproachTags? blockedApproach = null)
    {
        Name = name;
        Condition = condition;
        BlockedApproach = blockedApproach;
    }

    public bool IsActive(BaseTagSystem tagSystem)
    {
        return Condition.IsActive(tagSystem);
    }

    public void ApplyEffect(EncounterState state)
    {
        // Narrative tags don't directly affect state, only card selection
    }

    public string GetActivationDescription()
    {
        return Condition.GetDescription();
    }
}
