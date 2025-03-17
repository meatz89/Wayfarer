public class StrategicTag : IEncounterTag
{
    public string Name { get; }
    public StrategicEffectTypes EffectType { get; }
    public ApproachTags AffectedApproach { get; }

    public StrategicTag(
        string name,
        StrategicEffectTypes effectType,
        ApproachTags scalingApproachTag)
    {
        Name = name;
        EffectType = effectType;
        AffectedApproach = scalingApproachTag;
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
        if (choice.Approach != AffectedApproach)
            return 0;

        int approachValue = tagSystem.GetEncounterStateTagValue(AffectedApproach);
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
        if (choice.Approach != AffectedApproach)
            return 0;

        int approachValue = tagSystem.GetEncounterStateTagValue(AffectedApproach);
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
                baseDesc = $"Adds momentum proportional to {AffectedApproach} value";
                break;
            case StrategicEffectTypes.DecreasePressure:
                baseDesc = $"Reduces pressure proportional to {AffectedApproach} value";
                break;
            case StrategicEffectTypes.DecreaseMomentum:
                baseDesc = $"Reduces momentum proportional to {AffectedApproach} value";
                break;
            case StrategicEffectTypes.IncreasePressure:
                baseDesc = $"Adds pressure proportional to {AffectedApproach} value";
                break;
        }

        return baseDesc;
    }

    public string GetActivationDescription()
    {
        return "Always active"; // Strategic tags are always active
    }
}