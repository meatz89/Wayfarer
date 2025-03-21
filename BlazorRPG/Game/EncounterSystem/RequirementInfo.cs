
/// <summary>
/// Holds information about card requirements
/// </summary>
public class RequirementInfo
{
    public enum RequirementTypes
    {
        None,
        Approach,
        Focus
    }

    public RequirementTypes Type { get; }
    public ApproachTags ApproachTag { get; }
    public FocusTags FocusTag { get; }
    public int Value { get; }
    public int ReductionAmount { get; }

    // No requirement constructor
    public RequirementInfo()
    {
        Type = RequirementTypes.None;
        ApproachTag = ApproachTags.Dominance;
        FocusTag = FocusTags.Physical;
        Value = 0;
        ReductionAmount = 0;
    }

    // Approach requirement constructor
    public RequirementInfo(ApproachTags approach, int value, int reductionAmount)
    {
        Type = RequirementTypes.Approach;
        ApproachTag = approach;
        FocusTag = FocusTags.Physical; // Default
        Value = value;
        ReductionAmount = reductionAmount;
    }

    // Focus requirement constructor
    public RequirementInfo(FocusTags focus, int value, int reductionAmount)
    {
        Type = RequirementTypes.Focus;
        ApproachTag = ApproachTags.Dominance; // Default
        FocusTag = focus;
        Value = value;
        ReductionAmount = reductionAmount;
    }

    public bool IsMet(BaseTagSystem tagSystem)
    {
        if (Type == RequirementTypes.None)
            return true;

        if (Type == RequirementTypes.Approach)
            return tagSystem.GetEncounterStateTagValue(ApproachTag) >= Value;

        // Focus requirement
        return tagSystem.GetFocusTagValue(FocusTag) >= Value;
    }
}
