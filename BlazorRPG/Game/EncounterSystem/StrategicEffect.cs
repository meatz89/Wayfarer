/// <summary>
/// If Encounter has IEnvironmentalProperty present
/// Then apply EffectType
/// But Consider ApproachTag
/// </summary>
public class StrategicEffect
{
    public string Name;
    public StrategicTagEffectType EffectType { get; }
    public ApproachTags TargetApproach { get; }

    public List<IEnvironmentalProperty> ActivationProperties { get; }

    public StrategicEffect(
        List<IEnvironmentalProperty> activationProperties, 
        StrategicTagEffectType effectType, 
        ApproachTags scalingApproach,
        int mult = 1)
    {
        ActivationProperties = activationProperties;
        EffectType = effectType;
        TargetApproach = scalingApproach;
    }

    public override string ToString()
    {
        string activationProperties = string.Empty;
        foreach (var property in ActivationProperties)
        {
            if(!string.IsNullOrWhiteSpace(activationProperties)) activationProperties += ", ";
            activationProperties += property.ToString();
        }

        string effectDesc = $"{activationProperties}: {EffectType.ToString()} for each point of {TargetApproach.ToString()}";
        return effectDesc ;
    }

    public bool IsActive(EnvironmentPropertyTag strategicTag)
    {
        if (ActivationProperties.Contains(strategicTag.EnvironmentalProperty))
            return true;

        return false;
    }

    public int GetMomentumModifierForTag(EnvironmentPropertyTag tag, EncounterTagSystem tagSystem)
    {
        bool propertiesMatch = IsActive(tag);

        // Only apply if properties match and effect types align
        if (propertiesMatch &&
            (EffectType == StrategicTagEffectType.IncreaseMomentum ||
             EffectType == StrategicTagEffectType.DecreaseMomentum))
        {
            // Get current approach value
            int approachValue = tagSystem.GetEncounterStateTagValue(TargetApproach);

            // Calculate linear effect: 1 point per approach point
            int effectValue = approachValue;

            // Apply positive or negative based on effect type
            int increase = EffectType == StrategicTagEffectType.IncreaseMomentum ? effectValue : -effectValue;
            return increase;
        }

        return 0;
    }

    public int GetPressureModifierForTag(EnvironmentPropertyTag tag, EncounterTagSystem tagSystem)
    {
        bool propertiesMatch = IsActive(tag);
        
        // Only apply if properties match and effect types align
        if (propertiesMatch &&
            (EffectType == StrategicTagEffectType.IncreasePressure ||
             EffectType == StrategicTagEffectType.DecreasePressure))
        {
            int approachValue = tagSystem.GetEncounterStateTagValue(TargetApproach);

            // Calculate linear effect: 1 point per approach point
            int effectValue = approachValue;

            // Apply positive or negative based on effect type
            int increase = EffectType == StrategicTagEffectType.IncreasePressure ? effectValue : -effectValue;
            return increase;
        }

        return 0;
    }
}