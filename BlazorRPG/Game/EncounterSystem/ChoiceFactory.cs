
/// <summary>
/// Factory for creating common choice types
/// </summary>
public static class ChoiceFactory
{
    // Create a low tier momentum choice with no requirements
    public static ChoiceCard CreateMomentumChoice(
        string name, 
        string description, 
        CardTiers tier, 
        int baseEffectValue,
        ApproachTags approachPosition,
        int approachPositionValue,
        FocusTags focusPosition,
        int focusPositionValue,
        StrategicEffect strategicEffect,
        params TagModification[] tagModifications
        )
    {
        return new ChoiceCard(name, description, EffectTypes.Momentum, tier, baseEffectValue,
            strategicEffect, tagModifications,
            approachPosition, approachPositionValue, focusPosition, focusPositionValue);
    }

    public static ChoiceCard CreatePressureChoice(
        string name,
        string description,
        CardTiers tier,
        int baseEffectValue,
        ApproachTags approachPosition,
        int approachPositionValue,
        FocusTags focusPosition,
        int focusPositionValue,
        StrategicEffect strategicEffect,
        params TagModification[] tagModifications
        )
    {
        return new ChoiceCard(name, description, EffectTypes.Pressure, tier, baseEffectValue, 
            strategicEffect, tagModifications,
            approachPosition, approachPositionValue, focusPosition, focusPositionValue);
    }
}
