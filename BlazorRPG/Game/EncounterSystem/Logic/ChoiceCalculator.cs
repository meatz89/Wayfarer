public class ChoiceCalculator
{
    private readonly ChoiceBaseValueGenerator baseValueGenerator;
    private readonly LocationPropertyEffectCalculator locationPropertyEffectsCalculator;

    public ChoiceCalculator()
    {
        this.baseValueGenerator = new ChoiceBaseValueGenerator();
        this.locationPropertyEffectsCalculator = new LocationPropertyEffectCalculator();
    }

    public ChoiceCalculationResult CalculateChoiceEffects(EncounterChoice choice, EncounterContext context)
    {
        // Calculate base values
        List<ValueChange> valueChanges = CalculateValueChanges(choice, context);
        List<Requirement> requirements = CalculateRequirements(choice, context);

        // Get energy type and cost
        EnergyTypes energyType = choice.EnergyType;
        int energyCost = CalculateEnergyCost(choice, context);

        // Calculate costs and rewards
        List<Outcome> costs = CalculateCosts(choice, context);
        List<Outcome> rewards = CalculateRewards(choice, context);

        // Store results in choice for UI preview
        choice.ModifiedEncounterValueChanges = valueChanges;
        choice.ModifiedRequirements = requirements;
        choice.EnergyCost = energyCost;
        choice.ModifiedCosts = costs;
        choice.ModifiedRewards = rewards;

        // Calculate new state after these changes
        EncounterStateValues newValues = CalculateNewState(context, valueChanges);

        // Return complete calculation result
        return new ChoiceCalculationResult(
            newValues,
            valueChanges,
            energyType,
            energyCost,
            requirements,
            costs,
            rewards);
    }

    private EncounterStateValues CalculateNewState(EncounterContext context, List<ValueChange> valueChanges)
    {
        // Create a copy of the current state values
        EncounterStateValues newValues = new EncounterStateValues(
            context.CurrentValues.Outcome,
            context.CurrentValues.Momentum,
            context.CurrentValues.Insight,
            context.CurrentValues.Resonance,
            context.CurrentValues.Pressure);

        // Apply value changes to the copied state
        foreach (ValueChange change in valueChanges)
        {
            switch (change.ValueType)
            {
                case ValueTypes.Outcome:
                    newValues.Outcome += change.Amount;
                    break;
                case ValueTypes.Momentum:
                    newValues.Momentum += change.Amount;
                    break;
                case ValueTypes.Insight:
                    newValues.Insight += change.Amount;
                    break;
                case ValueTypes.Resonance:
                    newValues.Resonance += change.Amount;
                    break;
                case ValueTypes.Pressure:
                    newValues.Pressure += change.Amount;
                    break;
            }
        }

        return newValues;
    }

    private List<ValueChange> CalculateValueChanges(EncounterChoice choice, EncounterContext context)
    {
        List<ValueChange> changes = new();
        // add the base value changes first
        changes.AddRange(choice.BaseEncounterValueChanges);

        // Location modifications
        List<ValueChange> propertyChanges = locationPropertyEffectsCalculator.CalculatePropertyEffects(choice, context.LocationProperties);
        changes.AddRange(propertyChanges);

        // Outcome calculation based on secondary value, after applying property effects
        if (CanGenerateOutcome(choice.Archetype, context.CurrentValues))
        {
            int currentValue = choice.Archetype switch
            {
                ChoiceArchetypes.Physical => context.CurrentValues.Momentum + changes.Where(c => c.ValueType == ValueTypes.Momentum).Sum(c => c.Amount),
                ChoiceArchetypes.Focus => context.CurrentValues.Insight + changes.Where(c => c.ValueType == ValueTypes.Insight).Sum(c => c.Amount),
                ChoiceArchetypes.Social => context.CurrentValues.Resonance + changes.Where(c => c.ValueType == ValueTypes.Resonance).Sum(c => c.Amount),
                _ => 0
            };

            // Check if there's already a base outcome change before adding a new one
            if (!changes.Any(vc => vc.ValueType == ValueTypes.Outcome))
            {
                changes.Add(new ValueChange(ValueTypes.Outcome, currentValue));
            }
        }
        else
        {
            changes.Add(new ValueChange(ValueTypes.Pressure, 2));
        }

        return changes;
    }


    private List<Requirement> CalculateRequirements(EncounterChoice choice, EncounterContext context)
    {
        List<Requirement> requirements = baseValueGenerator.GenerateBaseRequirements(choice.Archetype, choice.Approach);

        // Add location-specific requirements
        List<Requirement> propertyRequirements = locationPropertyEffectsCalculator.CalculateLocationRequirements(choice, context.LocationProperties);
        requirements.AddRange(propertyRequirements);

        return requirements;
    }

    private List<Outcome> CalculateCosts(EncounterChoice choice, EncounterContext context)
    {
        List<Outcome> costs = baseValueGenerator.GenerateBaseCosts(choice.Archetype, choice.Approach);

        // Add location-specific costs
        List<Outcome> propertyCosts = locationPropertyEffectsCalculator.CalculatePropertyCosts(choice, context.LocationProperties);
        costs.AddRange(propertyCosts);

        return costs;
    }

    private List<Outcome> CalculateRewards(EncounterChoice choice, EncounterContext context)
    {
        List<Outcome> rewards = baseValueGenerator.GenerateBaseRewards(choice.Archetype, choice.Approach);

        // Add location-specific rewards
        List<Outcome> propertyRewards = locationPropertyEffectsCalculator.CalculatePropertyRewards(choice, context.LocationProperties);
        rewards.AddRange(propertyRewards);

        return rewards;
    }

    private bool CanGenerateOutcome(ChoiceArchetypes archetype, EncounterStateValues values)
    {
        return archetype switch
        {
            ChoiceArchetypes.Physical => values.Pressure <= values.Momentum,
            ChoiceArchetypes.Focus => values.Insight > 0,
            ChoiceArchetypes.Social => values.Resonance > 0,
            _ => false
        };
    }

    private int CalculateEnergyCost(EncounterChoice choice, EncounterContext context)
    {
        int baseEnergyCost = baseValueGenerator.GenerateBaseEnergyCost(choice.Archetype, choice.Approach);
        int propertyModifier = locationPropertyEffectsCalculator.CalculateEnergyCostModifier(choice, context.LocationProperties);
        int pressureModifier = context.CurrentValues.Pressure >= 6 ? context.CurrentValues.Pressure - 5 : 0;

        return baseEnergyCost + propertyModifier + pressureModifier;
    }
}