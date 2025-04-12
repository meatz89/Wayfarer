
/// <summary>
/// Factory for creating common choice types
/// </summary>
public static class ChoiceFactory
{
    // Create a low tier momentum choice with no requirements
    public static ChoiceCard CreateMomentumChoice(string name, string description, FocusTags focus,
                                             CardTiers tier, int baseEffectValue,
                                             ApproachTags optimalApproach, int optimalApproachValue,
                                             int optimalFocusValue, StrategicEffect strategicEffect,
                                             params TagModification[] tagModifications)
    {
        return new ChoiceCard(name, description, focus, EffectTypes.Momentum, tier, baseEffectValue,
                         new RequirementInfo(), strategicEffect, tagModifications,
                         optimalApproach, optimalApproachValue, optimalFocusValue);
    }

    public static ChoiceCard CreatePressureChoice(string name, string description, FocusTags focus,
                                         CardTiers tier, int baseEffectValue,
                                         ApproachTags optimalApproach, int optimalApproachValue,
                                         int optimalFocusValue, StrategicEffect strategicEffect,
                                         params TagModification[] tagModifications)
    {
        return new ChoiceCard(name, description, focus, EffectTypes.Pressure, tier, baseEffectValue,
                         new RequirementInfo(), strategicEffect, tagModifications,
                         optimalApproach, optimalApproachValue, optimalFocusValue);
    }

    // Create a higher tier momentum choice with approach requirement
    public static ChoiceCard CreateMomentumChoiceWithApproachRequirement(
        string name, string description, FocusTags focus,
        CardTiers tier, int baseEffectValue, ApproachTags optimalApproach, int optimalApproachValue,
        int optimalFocusValue, ApproachTags requirementApproach,
        int requirementValue, int reductionAmount, StrategicEffect strategicEffect, 
        params TagModification[] tagModifications)
    {
        return new ChoiceCard(name, description, focus, EffectTypes.Momentum, tier, baseEffectValue,
                         new RequirementInfo(requirementApproach, requirementValue, reductionAmount),
                         strategicEffect, tagModifications,
                         optimalApproach, optimalApproachValue, optimalFocusValue);
    }

    // Create a higher tier pressure choice with approach requirement
    public static ChoiceCard CreatePressureChoiceWithApproachRequirement(
        string name, string description, FocusTags focus,
        CardTiers tier, int baseEffectValue, ApproachTags optimalApproach, int optimalApproachValue,
        int optimalFocusValue, ApproachTags requirementApproach,
        int requirementValue, int reductionAmount, StrategicEffect strategicEffect, params TagModification[] tagModifications)
    {
        return new ChoiceCard(name, description, focus, EffectTypes.Pressure, tier, baseEffectValue,
                         new RequirementInfo(requirementApproach, requirementValue, reductionAmount),
                         strategicEffect, tagModifications,
                         optimalApproach, optimalApproachValue, optimalFocusValue);
    }

    // Create a higher tier momentum choice with focus requirement
    public static ChoiceCard CreateMomentumChoiceWithFocusRequirement(
        string name, string description, FocusTags focus,
        CardTiers tier, int baseEffectValue, ApproachTags optimalApproach, int optimalApproachValue,
        int optimalFocusValue, FocusTags requirementFocus,
        int requirementValue, int reductionAmount, StrategicEffect strategicEffect, params TagModification[] tagModifications)
    {
        return new ChoiceCard(name, description, focus, EffectTypes.Momentum, tier, baseEffectValue,
                         new RequirementInfo(requirementFocus, requirementValue, reductionAmount),
                         strategicEffect, tagModifications,
                         optimalApproach, optimalApproachValue, optimalFocusValue);
    }

    // Create a higher tier pressure choice with focus requirement
    public static ChoiceCard CreatePressureChoiceWithFocusRequirement(
        string name, string description, FocusTags focus,
        CardTiers tier, int baseEffectValue, ApproachTags optimalApproach, int optimalApproachValue,
        int optimalFocusValue, FocusTags requirementFocus,
        int requirementValue, int reductionAmount, StrategicEffect strategicEffect, params TagModification[] tagModifications)
    {
        return new ChoiceCard(name, description, focus, EffectTypes.Pressure, tier, baseEffectValue,
                         new RequirementInfo(requirementFocus, requirementValue, reductionAmount),
                         strategicEffect, tagModifications,
                         optimalApproach, optimalApproachValue, optimalFocusValue);
    }
}
