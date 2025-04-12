public class StrategicEffect
{
    public IEnvironmentalProperty Property { get; }
    public StrategicTagEffectType EffectType { get; }
    public ApproachTags ScalingApproach { get; }

    public StrategicEffect(IEnvironmentalProperty property, StrategicTagEffectType effectType, ApproachTags scalingApproach)
    {
        Property = property;
        EffectType = effectType;
        ScalingApproach = scalingApproach;
    }

    public int GetMomentumModifierForTag(StrategicTag tag, BaseTagSystem tagSystem)
    {
        // Check equality in both directions to handle Any properly
        bool propertiesMatch = tag.EnvironmentalProperty.Equals(Property) || Property.Equals(tag.EnvironmentalProperty);

        // Only apply if properties match and effect types align
        if (propertiesMatch &&
            (EffectType == StrategicTagEffectType.IncreaseMomentum ||
             EffectType == StrategicTagEffectType.DecreaseMomentum))
        {
            // Get current approach value
            int approachValue = tagSystem.GetEncounterStateTagValue(ScalingApproach);

            // Calculate linear effect: 1 point per 2 approach points
            int effectValue = approachValue / 2;

            // Apply positive or negative based on effect type
            return EffectType == StrategicTagEffectType.IncreaseMomentum ? effectValue : -effectValue;
        }

        return 0;
    }

    public int GetPressureModifierForTag(StrategicTag tag, BaseTagSystem tagSystem)
    {
        // Check equality in both directions to handle Any properly
        bool propertiesMatch = tag.EnvironmentalProperty.Equals(Property) || Property.Equals(tag.EnvironmentalProperty);

        // Only apply if properties match and effect types align
        if (propertiesMatch &&
            (EffectType == StrategicTagEffectType.IncreasePressure ||
             EffectType == StrategicTagEffectType.DecreasePressure))
        {
            int approachValue = tagSystem.GetEncounterStateTagValue(ScalingApproach);

            // Calculate linear effect: 1 point per 2 approach points
            int effectValue = approachValue / 2;

            // Apply positive or negative based on effect type
            return EffectType == StrategicTagEffectType.IncreasePressure ? effectValue : -effectValue;
        }

        return 0;
    }
}