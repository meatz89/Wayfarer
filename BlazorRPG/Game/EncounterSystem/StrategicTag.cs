public class StrategicTag : IEncounterTag
{
    public string Name { get; }
    public StrategicEffectTypes EffectType { get; }
    public EncounterStateTags ScalingApproachTag { get; }

    public StrategicTag(
        string name,
        StrategicEffectTypes effectType,
        EncounterStateTags scalingApproachTag)
    {
        Name = name;
        EffectType = effectType;
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

    public int GetMomentumModifierForChoice(IChoice choice, BaseTagSystem tagSystem)
    {
        // Only apply if the focus matches (or if no focus is specified)
        if (choice.Approach != ScalingApproachTag)
            return 0;

        int approachValue = tagSystem.GetEncounterStateTagValue(ScalingApproachTag);
        switch (EffectType)
        {
            case StrategicEffectTypes.IncreaseMomentum:
                return 1 * approachValue;
            case StrategicEffectTypes.DecreaseMomentum:
                return -1 * approachValue;
            default:
                return 0;
        }
    }

    public int GetPressureModifierForChoice(IChoice choice, BaseTagSystem tagSystem)
    {
        // Only apply if the focus matches (or if no focus is specified)
        if (choice.Approach != ScalingApproachTag)
            return 0;

        int approachValue = tagSystem.GetEncounterStateTagValue(ScalingApproachTag);
        switch (EffectType)
        {
            case StrategicEffectTypes.DecreasePressure:
                return -1 * approachValue;
            case StrategicEffectTypes.IncreasePressure:
                return 1 * approachValue;
            default:
                return 0;
        }
    }

    public string GetEffectDescription()
    {
        string baseDesc = "";
        switch (EffectType)
        {
            case StrategicEffectTypes.IncreaseMomentum:
                baseDesc = $"Adds momentum proportional to {ScalingApproachTag} value";
                break;
            case StrategicEffectTypes.DecreasePressure:
                baseDesc = $"Reduces pressure proportional to {ScalingApproachTag} value";
                break;
            case StrategicEffectTypes.DecreaseMomentum:
                baseDesc = $"Reduces momentum proportional to {ScalingApproachTag} value";
                break;
            case StrategicEffectTypes.IncreasePressure:
                baseDesc = $"Adds pressure proportional to {ScalingApproachTag} value";
                break;
        }

        return baseDesc;
    }

    public string GetActivationDescription()
    {
        return "Always active"; // Strategic tags are always active
    }
}