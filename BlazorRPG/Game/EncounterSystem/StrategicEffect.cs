public class StrategicEffect
{
    public IEnvironmentalProperty Property { get; }
    public StrategicTagEffectType EffectType { get; }
    public ApproachTags ApproachPosition { get; }

    public StrategicEffect(IEnvironmentalProperty property, StrategicTagEffectType effectType, ApproachTags scalingApproach)
    {
        Property = property;
        EffectType = effectType;
        ApproachPosition = scalingApproach;
    }

    public override string ToString()
    {
        string effectDesc = $"{Property.ToString()}: {EffectType.ToString()} for each point of {ApproachPosition.ToString()}";
        return effectDesc ;
    }

    public bool IsActive(StrategicTag strategicTag)
    {
        if (Property.Equals(strategicTag.EnvironmentalProperty))
            return true;
        return false;
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
            int approachValue = tagSystem.GetEncounterStateTagValue(ApproachPosition);

            // Calculate linear effect: 1 point per approach point
            int effectValue = approachValue;

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
            int approachValue = tagSystem.GetEncounterStateTagValue(ApproachPosition);

            // Calculate linear effect: 1 point per approach point
            int effectValue = approachValue;

            // Apply positive or negative based on effect type
            return EffectType == StrategicTagEffectType.IncreasePressure ? effectValue : -effectValue;
        }

        return 0;
    }

}