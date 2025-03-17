public class StrategicTag : IEncounterTag
{
    public string Name { get; }
    public StrategicEffectTypes EffectType { get; }
    public FocusTags? AffectedFocus { get; }
    public ApproachTags? ScalingApproachTag { get; }

    public StrategicTag(
        string name,
        StrategicEffectTypes effectType,
        FocusTags? affectedFocus = null,
        ApproachTags? scalingApproachTag = null)
    {
        Name = name;
        EffectType = effectType;
        AffectedFocus = affectedFocus;
        ScalingApproachTag = scalingApproachTag;
    }

    // Strategic tags are ALWAYS active
    public bool IsActive(BaseTagSystem tagSystem)
    {
        return true;
    }

    public void ApplyEffect(EncounterState state)
    {
        // Strategic tags don't directly apply effects to the state
        // Their effects are calculated during projection and then applied through the projection
    }

    public int GetMomentumModifierForChoice(IChoice choice)
    {
        // Only apply if the focus matches (or if no focus is specified)
        if (AffectedFocus.HasValue && choice.Focus != AffectedFocus.Value)
            return 0;

        switch (EffectType)
        {
            case StrategicEffectTypes.IncreaseMomentum:
                return 1; // Base value, will be scaled by approach tag in ProjectionService
            case StrategicEffectTypes.DecreaseMomentum:
                return -1; // Base value, will be scaled by approach tag in ProjectionService
            default:
                return 0;
        }
    }

    public int GetPressureModifierForChoice(IChoice choice)
    {
        // Only apply if the focus matches (or if no focus is specified)
        if (AffectedFocus.HasValue && choice.Focus != AffectedFocus.Value)
            return 0;

        switch (EffectType)
        {
            case StrategicEffectTypes.DecreasePressure:
                return -1; // Base value, will be scaled by approach tag in ProjectionService
            case StrategicEffectTypes.IncreasePressure:
                return 1; // Base value, will be scaled by approach tag in ProjectionService
            default:
                return 0;
        }
    }

    public int GetScaledEffect(BaseTagSystem tagSystem)
    {
        if (!ScalingApproachTag.HasValue)
            return 1; // Default value if no scaling is specified

        int approachValue = tagSystem.GetEncounterStateTagValue(ScalingApproachTag.Value);
        return approachValue / 2; // 1 effect point per 2 approach points
    }

    public string GetEffectDescription()
    {
        string baseDesc = "";
        switch (EffectType)
        {
            case StrategicEffectTypes.IncreaseMomentum:
                baseDesc = "Adds momentum proportional to {0} value";
                break;
            case StrategicEffectTypes.DecreasePressure:
                baseDesc = "Reduces pressure proportional to {0} value";
                break;
            case StrategicEffectTypes.DecreaseMomentum:
                baseDesc = "Reduces momentum proportional to {0} value";
                break;
            case StrategicEffectTypes.IncreasePressure:
                baseDesc = "Adds pressure proportional to {0} value";
                break;
        }

        if (ScalingApproachTag.HasValue)
        {
            return string.Format(baseDesc, ScalingApproachTag.Value);
        }

        return baseDesc.Replace("{0}", "approach");
    }

    public string GetActivationDescription()
    {
        return "Always active"; // Strategic tags are always active
    }
}