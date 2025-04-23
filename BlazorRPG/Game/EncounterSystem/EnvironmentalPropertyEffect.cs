public class EnvironmentalPropertyEffect
{
    public string Name;
    public List<IEnvironmentalProperty> ActivationProperties { get; }
    public StrategicTagEffectType EffectType { get; }
    public ApproachTags TargetApproach { get; }
    public float Multiplier { get; set; }

    public EnvironmentalPropertyEffect(
        List<IEnvironmentalProperty> activationProperties,
        StrategicTagEffectType effectType,
        ApproachTags scalingApproach,
        int multiplier = 1)
    {
        ActivationProperties = activationProperties;
        EffectType = effectType;
        TargetApproach = scalingApproach;
        Multiplier = multiplier;
    }

    public override string ToString()
    {
        string activationProperties = string.Empty;
        foreach (IEnvironmentalProperty property in ActivationProperties)
        {
            if (!string.IsNullOrWhiteSpace(activationProperties)) activationProperties += ", ";
            activationProperties += property.ToString();
        }

        string effectDesc = $"{activationProperties}: {EffectType.ToString()} for each point of {TargetApproach.ToString()}";
        return effectDesc;
    }

    public bool IsActive(StrategicTag strategicTag)
    {
        if (ActivationProperties.Contains(strategicTag.EnvironmentalProperty))
            return true;

        return false;
    }

    public int GetMomentumModifierForTag(StrategicTag tag, EncounterTagSystem tagSystem)
    {
        bool propertiesMatch = IsActive(tag);

        // Only apply if properties match and effect types align
        if (propertiesMatch &&
            (EffectType == StrategicTagEffectType.IncreaseMomentum ||
             EffectType == StrategicTagEffectType.DecreaseMomentum))
        {
            // Get current approach value
            int approachValue = tagSystem.GetApproachTagValue(TargetApproach);

            // Calculate linear effect: 1 point per approach point
            int effectValue = approachValue;

            // Apply positive or negative based on effect type
            int increase = EffectType == StrategicTagEffectType.IncreaseMomentum ? effectValue : -effectValue;
            return increase;
        }

        return 0;
    }

    public int GetPressureModifierForTag(StrategicTag tag, EncounterTagSystem tagSystem)
    {
        bool propertiesMatch = IsActive(tag);

        // Only apply if properties match and effect types align
        if (propertiesMatch &&
            (EffectType == StrategicTagEffectType.IncreasePressure ||
             EffectType == StrategicTagEffectType.DecreasePressure))
        {
            int approachValue = tagSystem.GetApproachTagValue(TargetApproach);

            // Calculate linear effect: 1 point per approach point
            int effectValue = approachValue;

            // Apply positive or negative based on effect type
            int increase = EffectType == StrategicTagEffectType.IncreasePressure ? effectValue : -effectValue;
            return increase;
        }

        return 0;
    }
}