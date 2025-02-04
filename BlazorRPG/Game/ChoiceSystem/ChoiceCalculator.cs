public class ChoiceCalculator
{
    private readonly GameState gameState;
    private readonly LocationPropertyEffectCalculator locationPropertyCalculator;

    public ChoiceCalculator(GameState gameState)
    {
        this.gameState = gameState;
        this.locationPropertyCalculator = new LocationPropertyEffectCalculator();
    }

    public EncounterValues GetProjectedEncounterState(EncounterChoice choice, EncounterValues initialValues, List<ValueModification> valueModifications)
    {
        EncounterValues projectedEncounterState = CalculateNewState(initialValues, choice, valueModifications);
        return projectedEncounterState;
    }

    public void CalculateChoiceEffects(
        EncounterChoice choice,
        EncounterContext context,
        EncounterValues initialEncounterValues)
    {
        // 1. Get base values that are inherent to the choice type
        List<ValueModification> valueModifications = GameRules.GetChoiceBaseValueEffects(choice);

        // 2. Calculate all modifications from game state and effects
        List<ValueModification> modifications = CalculateAllValueChanges(choice, initialEncounterValues);
        valueModifications.AddRange(modifications);

        // 3. Calculate new state after combining base values and modifications
        PlayerState playerState = gameState.Player;

        // 4. Calculate final requirements, costs and rewards
        List<Requirement> requirements = CalculateRequirements(context, choice, playerState, initialEncounterValues, valueModifications);
        List<Outcome> costs = CalculateCosts(choice, context);
        List<Outcome> rewards = CalculateRewards(choice, context);

        // 5. Return complete calculation result
        ChoiceCalculationResult choiceCalculationResult = new ChoiceCalculationResult(
            valueModifications,    // Modifications with sources
            requirements,          // Requirements
            costs,                 // Costs
            rewards);              // Rewards

        choice.CalculationResult.ValueModifications.AddRange(choiceCalculationResult.ValueModifications);
        choice.CalculationResult.Requirements.AddRange(choiceCalculationResult.Requirements);
        choice.CalculationResult.Costs.AddRange(choiceCalculationResult.Costs);
        choice.CalculationResult.Rewards.AddRange(choiceCalculationResult.Rewards);
    }


    private int CalculateEnergyCost(EncounterChoice choice, EncounterValues initialEncounterValues, PlayerState player, EncounterContext context)
    {
        int baseEnergyCost = 0; //GameRules.GetBaseEnergyCost(choice.Archetype, choice.Approach);

        int propertyModifier = locationPropertyCalculator.CalculateEnergyCostModifier(choice, context);

        // Apply energy cost reductions from modifications, using projected momentum
        int energyReduction = 0;

        if (initialEncounterValues.Momentum > 0)
        {
            energyReduction += Math.Min(initialEncounterValues.Momentum / 3, 3);
        }

        // Ensure energy cost doesn't go below 0
        int calculatedCost = baseEnergyCost + propertyModifier - energyReduction;
        int actualCost = Math.Max(0, calculatedCost);

        return actualCost;
    }

    private List<ValueModification> CalculateAllValueChanges(EncounterChoice choice, EncounterValues initialEncounterValues)
    {
        List<ValueModification> modifications = new();

        bool applyPressure = choice.Approach switch
        {
            ChoiceApproaches.Aggressive => applyPressure = true,
            ChoiceApproaches.Careful => applyPressure = true,
            ChoiceApproaches.Desperate => applyPressure = true,
            _ => false
        };

        if (!applyPressure) return modifications;

        if (initialEncounterValues.Pressure > GameRules.GetArchetypeValue(choice.Archetype, initialEncounterValues))
        {
            int reduceBy = initialEncounterValues.Pressure - GameRules.GetArchetypeValue(choice.Archetype, initialEncounterValues);
            reduceBy = reduceBy / 3;
            reduceBy = Math.Max(0, reduceBy);

            modifications.Add(new EncounterValueModification(
                ValueTypes.Outcome,
                -reduceBy,
                "High Pressure"));

        }

        return modifications;
    }

    private EncounterValues ProjectNewState(
        EncounterValues currentValues,
        List<ValueModification> modifications)
    {
        EncounterValues newState = EncounterValues.WithValues(
            momentum: currentValues.Momentum,
            insight: currentValues.Insight,
            resonance: currentValues.Resonance,
            outcome: currentValues.Outcome,
            pressure: currentValues.Pressure);

        foreach (ValueModification mod in modifications)
        {
            if (mod is EncounterValueModification evm)
            {
                ApplyValueChange(newState, evm.ValueType, evm.Amount);
            }
            else if (mod is EnergyCostReduction em)
            {
                // Do nothing here, energy modifications don't directly affect state values
            }
        }

        return newState;
    }

    private EncounterValues CalculateNewState(
        EncounterValues currentValues,
        EncounterChoice choice,
        List<ValueModification> modifications)
    {
        EncounterValues newState = ProjectNewState(currentValues, modifications);
        newState.LastChoiceType = choice.Archetype;
        newState.LastChoiceApproach = choice.Approach;
        return newState;
    }

    private List<Requirement> CalculateRequirements(EncounterContext context, EncounterChoice choice, PlayerState playerState, EncounterValues encounterValues, List<ValueModification> valueModifications)
    {
        CalculateNewState(context.CurrentValues, choice, valueModifications);

        List<Requirement> requirements = new List<Requirement> { };

        List<Requirement> ValueRequirements = GetValueRequirements(valueModifications, choice, playerState, encounterValues);
        requirements.AddRange(ValueRequirements);

        List<Requirement> propertyRequirements = locationPropertyCalculator
            .CalculateLocationRequirements(choice, context);

        requirements.AddRange(propertyRequirements);

        return requirements;
    }

    private List<Requirement> GetValueRequirements(List<ValueModification> valueModifications, EncounterChoice choice, PlayerState playerState, EncounterValues encounterValues)
    {
        List<Requirement> requirements = new();

        // Iterate through value modifications and add requirements for negative changes
        foreach (ValueModification modification in valueModifications)
        {
            // Only consider EncounterValueModifications, not EnergyCostReductions
            if (modification is EncounterValueModification change)
            {
                if (change.Amount < 0)
                {
                    switch (change.ValueType)
                    {
                        case ValueTypes.Momentum:
                            requirements.Add(new MomentumRequirement(-change.Amount));
                            break;
                        case ValueTypes.Insight:
                            requirements.Add(new InsightRequirement(-change.Amount));
                            break;
                        case ValueTypes.Resonance:
                            requirements.Add(new ResonanceRequirement(-change.Amount));
                            break;
                        case ValueTypes.Pressure:
                            // No requirement added for negative pressure change as reducing pressure is not a requirement.
                            break;
                        case ValueTypes.Outcome:
                            // Outcome decrease can be considered as a risk or a different kind of cost
                            // You might represent it differently or not add a direct requirement here
                            break;
                    }
                }
                else if (change.ValueType == ValueTypes.Pressure)
                {
                    // Pressure requirement: current pressure + change must not exceed 10
                    if (encounterValues.Pressure + change.Amount > 20)
                    {
                        requirements.Add(new PressureRequirement(20 - change.Amount));
                    }
                }
            }
        }

        return requirements;
    }

    private List<Outcome> CalculateCosts(EncounterChoice choice, EncounterContext context)
    {
        List<Outcome> costs = GenerateBaseCosts(choice.Archetype, choice.Approach);

        List<Outcome> propertyCosts = locationPropertyCalculator.CalculatePropertyCosts(choice, context);
        costs.AddRange(propertyCosts);
        return costs;
    }

    private List<Outcome> CalculateRewards(EncounterChoice choice, EncounterContext context)
    {
        List<Outcome> rewards = GenerateBaseRewards(choice.Archetype, choice.Approach);
        List<Outcome> propertyRewards = locationPropertyCalculator.CalculatePropertyRewards(choice, context);
        rewards.AddRange(propertyRewards);
        return rewards;
    }

    private List<Outcome> GenerateBaseCosts(ChoiceArchetypes archetype, ChoiceApproaches approach)
    {
        List<Outcome> costs = new List<Outcome>();
        return costs;
    }

    private List<Outcome> GenerateBaseRewards(ChoiceArchetypes archetype, ChoiceApproaches approach)
    {
        List<Outcome> rewards = new List<Outcome>();
        return rewards;
    }


    private void ApplyValueChange(EncounterValues state, ValueTypes valueType, int amount)
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

}