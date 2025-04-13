public class FocusThresholdCondition : ActivationCondition
{
    public ApproachTags EncounterStateTag { get; }
    public int Threshold { get; }

    public FocusThresholdCondition(ApproachTags encounterStateTag, int threshold)
    {
        EncounterStateTag = encounterStateTag;
        Threshold = threshold;
    }

    public override bool IsActive(EncounterTagSystem tagSystem)
    {
        return tagSystem.GetEncounterStateTagValue(EncounterStateTag) >= Threshold;
    }

    public override string GetDescription()
    {
        return $"Requires {EncounterStateTag} {Threshold}+";
    }
}
