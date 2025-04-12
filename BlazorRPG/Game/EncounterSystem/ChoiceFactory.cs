
/// <summary>
/// Factory for creating common choice types
/// </summary>
public static class ChoiceFactory
{
    // Create a low tier momentum choice with no requirements
    public static ChoiceCard CreateMomentumChoice(string name, string description, CardTiers tier, int baseEffectValue,
        ApproachTags optimalApproach, int optimalApproachValue, FocusTags focus,
        int optimalFocusValue, StrategicEffect strategicEffect,
        params TagModification[] tagModifications)
    {
        return new ChoiceCard(name, description, focus, EffectTypes.Momentum, tier, baseEffectValue,
                         new RequirementInfo(), strategicEffect, tagModifications,
                         optimalApproach, optimalApproachValue, optimalFocusValue);
    }

    public static ChoiceCard CreatePressureChoice(string name, string description, CardTiers tier, int baseEffectValue,
        ApproachTags optimalApproach, int optimalApproachValue, FocusTags focus,
        int optimalFocusValue, StrategicEffect strategicEffect,
        params TagModification[] tagModifications)
    {
        return new ChoiceCard(name, description, focus, EffectTypes.Pressure, tier, baseEffectValue,
                         new RequirementInfo(), strategicEffect, tagModifications,
                         optimalApproach, optimalApproachValue, optimalFocusValue);
    }
}
