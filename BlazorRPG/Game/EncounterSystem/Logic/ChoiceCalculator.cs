public class ChoiceCalculator
{
    private readonly ChoiceEffectsGenerator baseValueGenerator;
    private readonly LocationPropertyEffectCalculator locationPropertyCalculator;

    public ChoiceCalculator()
    {
        this.baseValueGenerator = new ChoiceEffectsGenerator();
        this.locationPropertyCalculator = new LocationPropertyEffectCalculator();
    }

    public ChoiceCalculationResult CalculateChoiceEffects(EncounterChoice choice, EncounterContext context)
    {
        // 1. Get base values that are inherent to the choice type
        List<BaseValueChange> baseChanges = baseValueGenerator
            .GenerateBaseValueChanges(choice.Archetype, choice.Approach);

        // 2. Calculate all modifications from game state and effects
        List<ValueModification> modifications = CalculateAllValueChanges(choice, context);

        // 3. Calculate new state after combining base values and modifications
        EncounterStateValues newState = CalculateNewState(context.CurrentValues, choice, baseChanges, modifications);

        // 4. Calculate final requirements, costs and rewards
        List<Requirement> requirements = CalculateRequirements(choice, context);
        List<Outcome> costs = CalculateCosts(choice, context);
        List<Outcome> rewards = CalculateRewards(choice, context);

        // 5. Store all results in the choice for UI preview
        choice.BaseEncounterValueChanges = baseChanges;
        choice.ValueModifications = modifications;
        choice.Requirements = requirements;
        choice.Costs = costs;
        choice.Rewards = rewards;
        choice.EnergyCost = CalculateEnergyCost(choice, context);

        // 6. Return complete calculation result
        return new ChoiceCalculationResult(
            newState,
            baseChanges,          // Base values
            modifications,        // Modifications with sources
            choice.EnergyType,    // Energy type
            choice.EnergyCost,    // Energy cost
            requirements,         // Requirements
            costs,                // Costs
            rewards);             // Rewards
    }

    private List<ValueModification> CalculateAllValueChanges(EncounterChoice choice, EncounterContext context)
    {
        List<ValueModification> modifications = new();

        // Add decay changes first
        AddDecayModifications(modifications, choice, context);

        // Add cascade effects based on projected state
        AddCascadeModifications(modifications, choice, context);

        // Add state-based modifications
        AddStateModifications(modifications, choice, context);

        return modifications;
    }

    private void AddDecayModifications(List<ValueModification> modifications, EncounterChoice choice, EncounterContext context)
    {
        // Decay for unused values
        if (choice.Archetype != ChoiceArchetypes.Physical)
            modifications.Add(new ValueModification(ValueTypes.Momentum, -1, "Unused Momentum Decay"));

        // Extra decay for repeating choices
        if (context.CurrentValues.LastChoiceType == choice.Archetype)
        {
            switch (choice.Archetype)
            {
                case ChoiceArchetypes.Physical:
                    modifications.Add(new ValueModification(ValueTypes.Momentum, -2, "Repeated Physical Choice"));
                    break;
                case ChoiceArchetypes.Focus:
                    modifications.Add(new ValueModification(ValueTypes.Insight, -1, "Repeated Focus Choice"));
                    break;
                case ChoiceArchetypes.Social:
                    modifications.Add(new ValueModification(ValueTypes.Resonance, -1, "Repeated Social Choice"));
                    break;
            }
        }
    }

    private void AddCascadeModifications(List<ValueModification> modifications, EncounterChoice choice, EncounterContext context)
    {
        // Project state after base values and current modifications
        EncounterStateValues projectedValues = ProjectNewState(
            context.CurrentValues,
            choice.BaseEncounterValueChanges,
            modifications);

        // Add pressure when values are too low
        if (projectedValues.Momentum < projectedValues.Outcome / 3)
            modifications.Add(new ValueModification(ValueTypes.Pressure, 2, "Not enough Momentum"));
        if (projectedValues.Insight < projectedValues.Outcome / 3)
            modifications.Add(new ValueModification(ValueTypes.Pressure, 2, "Not enough Insight"));
        if (projectedValues.Resonance < projectedValues.Outcome / 3)
            modifications.Add(new ValueModification(ValueTypes.Pressure, 2, "Not enough Resonance"));
    }

    private void AddStateModifications(List<ValueModification> modifications, EncounterChoice choice, EncounterContext context)
    {
        // High pressure reduces gains
        if (context.CurrentValues.Pressure >= 6)
        {
            foreach (BaseValueChange baseChange in choice.BaseEncounterValueChanges)
            {
                if (baseChange.ValueType != ValueTypes.Pressure && baseChange.Amount > 0)
                {
                    modifications.Add(new ValueModification(
                        baseChange.ValueType,
                        -1,
                        "High Pressure Penalty"));
                }
            }
        }
    }

    private EncounterStateValues ProjectNewState(
        EncounterStateValues currentValues,
        List<BaseValueChange> baseChanges,
        List<ValueModification> modifications)
    {
        EncounterStateValues newState = new(
            currentValues.Outcome,
            currentValues.Momentum,
            currentValues.Insight,
            currentValues.Resonance,
            currentValues.Pressure);

        // Apply base changes first
        foreach (BaseValueChange change in baseChanges)
        {
            ApplyValueChange(newState, change.ValueType, change.Amount);
        }

        // Then apply modifications
        foreach (ValueModification mod in modifications)
        {
            ApplyValueChange(newState, mod.ValueType, mod.Amount);
        }

        return newState;
    }

    private void ApplyValueChange(EncounterStateValues state, ValueTypes valueType, int amount)
    {
        switch (valueType)
        {
            case ValueTypes.Outcome:
                state.Outcome = Math.Max(0, state.Outcome + amount);
                break;
            case ValueTypes.Momentum:
                state.Momentum = Math.Max(0, state.Momentum + amount);
                break;
            case ValueTypes.Insight:
                state.Insight = Math.Max(0, state.Insight + amount);
                break;
            case ValueTypes.Resonance:
                state.Resonance = Math.Max(0, state.Resonance + amount);
                break;
            case ValueTypes.Pressure:
                state.Pressure = Math.Clamp(state.Pressure + amount, 0, 8);
                break;
        }
    }

    private EncounterStateValues CalculateNewState(
        EncounterStateValues currentValues,
        EncounterChoice choice,
        List<BaseValueChange> baseChanges,
        List<ValueModification> modifications)
    {
        EncounterStateValues newState = ProjectNewState(currentValues, baseChanges, modifications);
        newState.LastChoiceType = choice.Archetype;
        return newState;
    }

    // Rest of the calculation methods stay the same
    private List<Requirement> CalculateRequirements(EncounterChoice choice, EncounterContext context)
    {
        List<Requirement> requirements = baseValueGenerator.GenerateBaseRequirements(choice.Archetype, choice.Approach);
        List<Requirement> propertyRequirements = locationPropertyCalculator.CalculateLocationRequirements(choice, context.LocationProperties);
        requirements.AddRange(propertyRequirements);
        return requirements;
    }

    private List<Outcome> CalculateCosts(EncounterChoice choice, EncounterContext context)
    {
        List<Outcome> costs = baseValueGenerator.GenerateBaseCosts(choice.Archetype, choice.Approach);
        List<Outcome> propertyCosts = locationPropertyCalculator.CalculatePropertyCosts(choice, context.LocationProperties);
        costs.AddRange(propertyCosts);
        return costs;
    }

    private List<Outcome> CalculateRewards(EncounterChoice choice, EncounterContext context)
    {
        List<Outcome> rewards = baseValueGenerator.GenerateBaseRewards(choice.Archetype, choice.Approach);
        List<Outcome> propertyRewards = locationPropertyCalculator.CalculatePropertyRewards(choice, context.LocationProperties);
        rewards.AddRange(propertyRewards);
        return rewards;
    }

    private int CalculateEnergyCost(EncounterChoice choice, EncounterContext context)
    {
        int baseEnergyCost = baseValueGenerator.GenerateBaseEnergyCost(choice.Archetype, choice.Approach);
        int propertyModifier = locationPropertyCalculator.CalculateEnergyCostModifier(choice, context.LocationProperties);
        int pressureModifier = context.CurrentValues.Pressure >= 6 ? context.CurrentValues.Pressure - 5 : 0;

        return baseEnergyCost + propertyModifier + pressureModifier;
    }
}