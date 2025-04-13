/// <summary>
/// If Encounter has IEnvironmentalProperty present
/// Then apply EffectType
/// But Consider ApproachTag
/// </summary>
public class StrategicEffect
{
    public IEnvironmentalProperty ActivationProperty { get; }
    public StrategicTagEffectType EffectType { get; }
    public ApproachTags TargetApproach { get; }
    public int Mult { get; }

    public StrategicEffect(
        IEnvironmentalProperty activationProperty, 
        StrategicTagEffectType effectType, 
        ApproachTags scalingApproach,
        int mult = 1)
    {
        ActivationProperty = activationProperty;
        EffectType = effectType;
        TargetApproach = scalingApproach;
        Mult = mult;
    }

    public override string ToString()
    {
        string effectDesc = $"{ActivationProperty.ToString()}: {EffectType.ToString()} for each point of {TargetApproach.ToString()}";
        return effectDesc ;
    }

    public bool IsActive(EnvironmentPropertyTag strategicTag)
    {
        if (ActivationProperty.Equals(strategicTag.EnvironmentalProperty))
            return true;

        return false;
    }

    public int GetMomentumModifierForTag(EnvironmentPropertyTag tag, BaseTagSystem tagSystem)
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
            return increase * Mult;
        }

        return 0;
    }

    public int GetPressureModifierForTag(EnvironmentPropertyTag tag, BaseTagSystem tagSystem)
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
            return increase * Mult;
        }

        return 0;
    }

    public int GetInjuryModifierForTag(
        EnvironmentPropertyTag tag,
        ChoiceProjection choiceProjection,
        int currentPressure)
    {
        bool propertiesMatch = IsActive(tag);
        if (propertiesMatch)
        {
            if (EffectType == StrategicTagEffectType.IncreaseInjury)
            {
                return currentPressure * Mult;
            }
        }
        return 0;
    }
}