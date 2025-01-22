public class ChoiceCalculator
{
    private readonly GameState gameState;
    private readonly LocationPropertyEffectCalculator locationPropertyCalculator;

    public ChoiceCalculator(GameState gameState)
    {
        this.gameState = gameState;
        this.locationPropertyCalculator = new LocationPropertyEffectCalculator();
    }

    public ChoiceCalculationResult CalculateChoiceEffects(EncounterChoice choice, EncounterContext context)
    {
        // 1. Get base values that are inherent to the choice type
        List<BaseValueChange> baseChanges = new List<BaseValueChange>();
        List<ValueModification> valueModifications = GameRules.GetChoiceBaseValueEffects(choice, context);

        // 2. Calculate all modifications from game state and effects
        List<ValueModification> modifications = CalculateAllValueChanges(choice, context);
        valueModifications.AddRange(modifications);

        // 3. Calculate new state after combining base values and modifications
        EncounterValues newState = CalculateNewState(context.CurrentValues, choice, baseChanges, valueModifications);

        choice.EnergyCost = CalculateEnergyCost(choice, context, gameState.Player);

        // 4. Calculate final requirements, costs and rewards
        List<Requirement> requirements = CalculateRequirements(valueModifications, choice, context, gameState.Player);
        List<Outcome> costs = CalculateCosts(choice, context);
        List<Outcome> rewards = CalculateRewards(choice, context);

        // 5. Return complete calculation result
        ChoiceCalculationResult choiceCalculationResult = new ChoiceCalculationResult(
            newState,
            baseChanges,           // Base values
            valueModifications,    // Modifications with sources
            choice.EnergyType,     // Energy type
            choice.EnergyCost,     // Energy cost
            requirements,          // Requirements
            costs,                 // Costs
            rewards);              // Rewards

        return choiceCalculationResult;
    }


    private int CalculateEnergyCost(EncounterChoice choice, EncounterContext context, PlayerState player)
    {
        int baseEnergyCost = GameRules.GetBaseEnergyCost(choice.Archetype, choice.Approach);

        EncounterValues currentValues = context.CurrentValues;
        int propertyModifier = locationPropertyCalculator.CalculateEnergyCostModifier(choice, context.LocationProperties);

        // Apply energy cost reductions from modifications, using projected momentum
        int energyReduction = 0;

        if (currentValues.Momentum > 0)
        {
            energyReduction += Math.Min(currentValues.Momentum / 3, 3);
        }

        // Ensure energy cost doesn't go below 0
        int calculatedCost = baseEnergyCost + propertyModifier - energyReduction;
        int actualCost = Math.Max(0, calculatedCost);

        return actualCost;
    }

    private List<ValueModification> CalculateAllValueChanges(EncounterChoice choice, EncounterContext context)
    {
        List<ValueModification> modifications = new();

        // First handle value decay - this affects what resources we have available
        AddDecayModifications(modifications, choice, context);

        // Then handle state-based modifications that affect our value gains
        //AddStateModifications(modifications, choice, context);
        //AddEnergyEffects(modifications, choice, context);

        // Finally calculate outcome generation based on our archetype, approach and current values
        AddOutcomeModifications(modifications, choice, context);

        return modifications;
    }

    private EncounterValues ProjectNewState(
        EncounterValues currentValues,
        List<BaseValueChange> baseChanges,
        List<ValueModification> modifications)
    {
        EncounterValues newState = EncounterValues.WithValues(
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

    private EncounterValues CalculateNewState(
        EncounterValues currentValues,
        EncounterChoice choice,
        List<BaseValueChange> baseChanges,
        List<ValueModification> modifications)
    {
        EncounterValues newState = ProjectNewState(currentValues, baseChanges, modifications);
        newState.LastChoiceType = choice.Archetype;
        newState.LastChoiceApproach = choice.Approach;
        return newState;
    }

    private List<Requirement> CalculateRequirements(List<ValueModification> valueModifications, EncounterChoice choice, EncounterContext context, PlayerState playerState)
    {
        List<Requirement> requirements = GetEnergyRequirements(choice, context, playerState);

        List<Requirement> baseValueRequirements = GetBaseValueRequirements(valueModifications, choice, context, playerState);
        requirements.AddRange(baseValueRequirements);

        List<Requirement> propertyRequirements = locationPropertyCalculator
            .CalculateLocationRequirements(choice, context.LocationProperties);

        requirements.AddRange(propertyRequirements);

        return requirements;
    }

    private List<Requirement> GetBaseValueRequirements(List<ValueModification> valueModifications, EncounterChoice choice, EncounterContext context, PlayerState playerState)
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
                            requirements.Add(new MomentumRequirement(-change.Amount)); // Note: Negate the amount to make it positive for the requirement
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
                    if (context.CurrentValues.Pressure + change.Amount > 10)
                    {
                        requirements.Add(new PressureRequirement(10 - change.Amount));
                    }
                }
            }
        }

        return requirements;
    }

    private List<Requirement> GetEnergyRequirements(EncounterChoice choice, EncounterContext context, PlayerState playerState)
    {
        List<Requirement> requirements = new List<Requirement>();

        // Calculate energy cost
        int energyCost = choice.EnergyCost;

        // Only add energy requirements if the cost is greater than 0
        if (energyCost > 0)
        {
            // Check if the player has enough energy to meet the requirement
            if (choice.EnergyType == EnergyTypes.Physical)
            {
                if (gameState.Player.PhysicalEnergy < energyCost)
                {
                    int healthCost = energyCost - gameState.Player.PhysicalEnergy;
                    requirements.Add(new HealthRequirement(healthCost));
                }
                else
                {
                    requirements.Add(new EnergyRequirement(EnergyTypes.Physical, energyCost));
                }
            }
            else if (choice.EnergyType == EnergyTypes.Focus)
            {
                if (gameState.Player.FocusEnergy < energyCost)
                {
                    int concentrationCost = energyCost - gameState.Player.FocusEnergy;
                    requirements.Add(new ConcentrationRequirement(concentrationCost));
                }
                else
                {
                    requirements.Add(new EnergyRequirement(EnergyTypes.Focus, energyCost));
                }
            }
            else if (choice.EnergyType == EnergyTypes.Social)
            {
                if (gameState.Player.SocialEnergy < energyCost)
                {
                    int reputationCost = energyCost - gameState.Player.SocialEnergy;
                    requirements.Add(new ReputationRequirement(reputationCost));
                }
                else
                {
                    requirements.Add(new EnergyRequirement(EnergyTypes.Social, energyCost));
                }
            }
        }
        return requirements;
    }

    private List<Outcome> CalculateCosts(EncounterChoice choice, EncounterContext context)
    {
        List<Outcome> costs = GenerateBaseCosts(choice.Archetype, choice.Approach);

        List<Outcome> pressureCosts = CalculatePressureCosts(choice, context);
        costs.AddRange(pressureCosts);

        List<Outcome> propertyCosts = locationPropertyCalculator.CalculatePropertyCosts(choice, context.LocationProperties);
        costs.AddRange(propertyCosts);
        return costs;
    }

    private List<Outcome> CalculateRewards(EncounterChoice choice, EncounterContext context)
    {
        List<Outcome> rewards = GenerateBaseRewards(choice.Archetype, choice.Approach);
        List<Outcome> propertyRewards = locationPropertyCalculator.CalculatePropertyRewards(choice, context.LocationProperties);
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

    private void AddOutcomeModifications(List<ValueModification> modifications, EncounterChoice choice, EncounterContext context)
    {
    }

    private void AddDecayModifications(List<ValueModification> modifications, EncounterChoice choice, EncounterContext context)
    {
        /*
        // First handle value decay
        if (context.CurrentValues.LastChoiceType.HasValue)
        {
            // Momentum decays by 2 each turn without physical action
            if (choice.Archetype != ChoiceArchetypes.Physical)
            {
                modifications.Add(new EncounterValueModification(ValueTypes.Momentum, -1, "No Physical Choice"));
            }
            // Insight decays by 1 when repeating choice type
            if (choice.Archetype == context.CurrentValues.LastChoiceType)
            {
                modifications.Add(new EncounterValueModification(ValueTypes.Insight, -1, "Repeated Choice Type"));
            }
            // Resonance decays by 1 when repeating choice approaches
            if (choice.Approach == context.CurrentValues.LastChoiceApproach)
            {
                modifications.Add(new EncounterValueModification(ValueTypes.Resonance, -1, "Repeated Choice Approach"));
            }
        }
        */
    }

    public List<Outcome> CalculatePressureCosts(EncounterChoice choice, EncounterContext context)
    {
        List<Outcome> costs = new();

        //// Add pressure-based complications at high pressure
        //if (context.CurrentValues.Pressure >= 7)
        //{
        //    switch (choice.Archetype)
        //    {
        //        case ChoiceArchetypes.Physical:
        //            costs.Add(new HealthOutcome(-1));
        //            break;
        //        case ChoiceArchetypes.Focus:
        //            costs.Add(new ConcentrationOutcome(-1));
        //            break;
        //        case ChoiceArchetypes.Social:
        //            costs.Add(new ReputationOutcome(-1));
        //            break;
        //    }
        //}

        return costs;
    }

    private void AddStateModifications(List<ValueModification> modifications, EncounterChoice choice, EncounterContext context)
    {
        EncounterValues currentValues = context.CurrentValues;
        //AddBonusToOutcome(modifications, currentValues);

        // Add Pressure penalty to Outcome
        if (currentValues.Insight > 5)
        {
            modifications.Add(new EncounterValueModification(
                ValueTypes.Outcome,
                1,
                "High Insight Bonus"
            ));
        }
        if (currentValues.Resonance > 5)
        {
            modifications.Add(new EncounterValueModification(
                ValueTypes.Outcome,
                1,
                "High Resonance Bonus"
            ));
        }
        if (currentValues.Pressure >= 6)
        {
            modifications.Add(new EncounterValueModification(
                ValueTypes.Outcome,
                -1,
                "Medium Pressure Penalty"
            ));
        }
        if (currentValues.Pressure >= 8)
        {
            modifications.Add(new EncounterValueModification(
                ValueTypes.Outcome,
                -1,
                "High Pressure Penalty"
            ));
        }

    }

    private static void AddEnergyEffects(List<ValueModification> modifications, EncounterChoice choice, EncounterContext context)
    {
        EncounterValues currentValues = context.CurrentValues;

        // Pressure affects energy costs
        if (currentValues.Pressure > 5)
        {
            int energyIncrease = currentValues.Pressure - 5;
            modifications.Add(new EnergyCostReduction(
                choice.EnergyType,
                -energyIncrease, // Negative because it's increasing cost
                $"High Pressure (+{energyIncrease} Energy Cost)"
            ));
        }
        // Momentum affects physical energy costs
        if (choice.EnergyType == EnergyTypes.Physical)
        {
            int momentumEffect = currentValues.Momentum - 5; // Positive or negative based on base 5
            if (momentumEffect != 0)
            {
                modifications.Add(new EnergyCostReduction(
                    EnergyTypes.Social,
                    momentumEffect,
                    $"From Momentum {(momentumEffect > 0 ? "Bonus" : "Penalty")}"
                ));
            }
        }
    }

}