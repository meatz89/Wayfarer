public class FocusThresholdCondition : ActivationCondition
{
    public EncounterStateTags EncounterStateTag { get; }
    public int Threshold { get; }

    public FocusThresholdCondition(EncounterStateTags encounterStateTag, int threshold)
    {
        EncounterStateTag = encounterStateTag;
        Threshold = threshold;
    }

    public override bool IsActive(BaseTagSystem tagSystem)
    {
        return tagSystem.GetEncounterStateTagValue(EncounterStateTag) >= Threshold;
    }

    public override string GetDescription()
    {
        return $"Requires {EncounterStateTag} {Threshold}+";
    }
}
