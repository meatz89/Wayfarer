public class StrategicEffect
{
    public IEnvironmentalProperty RelevantProperty;
    public StrategicTagEffectType EffectType;
    public ApproachTags ScaleWithApproach;

    public StrategicEffect(IEnvironmentalProperty relevantProperty, StrategicTagEffectType effectType, ApproachTags scaleWithApproach)
    {
        RelevantProperty = relevantProperty;
        EffectType = effectType;
        ScaleWithApproach = scaleWithApproach;
    }

    public int GetMomentumModifierForTag(StrategicTag tag, BaseTagSystem tagSystem)
    {
        // Check if this tag's property matches this effect's property
        if (PropertyMatches(tag.EnvironmentalProperty))
        {
            int approachValue = tagSystem.GetEncounterStateTagValue(ScaleWithApproach);
            int effect = approachValue; // Scale at 1 per 1 points

            return EffectType == StrategicTagEffectType.IncreaseMomentum ? effect :
                   EffectType == StrategicTagEffectType.DecreaseMomentum ? -effect : 0;
        }
        return 0;
    }

    public int GetPressureModifierForTag(StrategicTag tag, BaseTagSystem tagSystem)
    {
        // Check if this tag's property matches this effect's property
        if (PropertyMatches(tag.EnvironmentalProperty))
        {
            int approachValue = tagSystem.GetEncounterStateTagValue(ScaleWithApproach);
            int effect = approachValue; // Scale at 1 per 1 points

            return EffectType == StrategicTagEffectType.IncreasePressure ? effect :
                   EffectType == StrategicTagEffectType.DecreasePressure ? -effect : 0;
        }
        return 0;
    }

    private bool PropertyMatches(IEnvironmentalProperty tagProperty)
    {
        return tagProperty.GetPropertyType() == RelevantProperty.GetPropertyType() &&
               tagProperty.GetPropertyValue() == RelevantProperty.GetPropertyValue();
    }
}