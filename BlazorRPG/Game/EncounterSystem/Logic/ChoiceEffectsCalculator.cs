public class ChoiceEffectsCalculator
{
    private readonly GameState gameState;
    private readonly BaseValueChangeGenerator baseValueGenerator;
    private readonly LocationPropertyEffectCalculator locationPropertyCalculator;

    public ChoiceEffectsCalculator(GameState gameState)
    {
        this.gameState = gameState;
        this.baseValueGenerator = new BaseValueChangeGenerator();
        this.locationPropertyCalculator = new LocationPropertyEffectCalculator();
    }

    public ChoiceCalculationResult CalculateChoiceEffects(EncounterChoice choice, EncounterContext context)
    {
        // 1. Get base values that are inherent to the choice type
        List<BaseValueChange> baseChanges = baseValueGenerator.GenerateBaseValueChanges(choice.Archetype, choice.Approach);

        // 2. Calculate all modifications from game state and effects
        List<ValueModification> valueModifications = CalculateAllValueChanges(choice, context);

        // 3. Calculate new state after combining base values and modifications
        EncounterValues newState = CalculateNewState(context.CurrentValues, choice, baseChanges, valueModifications);

        // 4. Calculate final requirements, costs and rewards
        List<Requirement> requirements = CalculateRequirements(choice, context, gameState.Player);
        List<Outcome> costs = CalculateCosts(choice, context);
        List<Outcome> rewards = CalculateRewards(choice, context);

        // 5. Store all results in the choice for UI preview
        choice.EnergyCost = CalculateEnergyCost(choice, context, gameState.Player);

        // 6. Return complete calculation result
        ChoiceCalculationResult choiceCalculationResult = new ChoiceCalculationResult(
            newState,
            baseChanges,           // Base values
            valueModifications,    // Modifications with sources
            choice.EnergyType,     // Energy type
            choice.EnergyCost,     // Energy cost
            requirements,          // Requirements
            costs,                 // Costs
            rewards);              // Rewards

        choice.CalculationResult = choiceCalculationResult; // Store the result in the choice

        return choiceCalculationResult;
    }

    private List<ValueModification> CalculateAllValueChanges(EncounterChoice choice, EncounterContext context)
    {
        List<ValueModification> modifications = new();

        AddArchetypeModifications(modifications, choice, context);

        // First handle value decay - this affects what resources we have available
        AddDecayModifications(modifications, choice, context);

        // Then handle state-based modifications that affect our value gains
        AddStateModifications(modifications, choice, context);

        // Finally calculate outcome generation based on our archetype, approach and current values
        AddOutcomeModifications(modifications, choice, context);

        return modifications;
    }

    private void AddArchetypeModifications(List<ValueModification> modifications, EncounterChoice choice, EncounterContext context)
    {
        // Each archetype builds its specific mastery value
        ChoiceApproaches approach = choice.Approach;

        switch (choice.Archetype)
        {
            case ChoiceArchetypes.Physical:
                // Physical actions build Momentum
                if (approach == ChoiceApproaches.Strategic)
                {
                    modifications.Add(new EncounterValueModification(ValueTypes.Momentum, 3,
                        $"Physical Strategic"));
                }
                else if (approach == ChoiceApproaches.Careful)
                {
                    modifications.Add(new EncounterValueModification(ValueTypes.Momentum, 2,
                        $"Physical Careful"));
                }
                else if (approach == ChoiceApproaches.Aggressive)
                {
                    modifications.Add(new EncounterValueModification(ValueTypes.Pressure, 4,
                        $"Physical Aggressive"));
                }
                else if (approach == ChoiceApproaches.Desperate)
                {
                }

                break;

            case ChoiceArchetypes.Focus:
                // Focus actions build Insight
                if (approach == ChoiceApproaches.Strategic)
                {
                    modifications.Add(new EncounterValueModification(ValueTypes.Insight, 3,
                        $"Focus Strategic"));
                }
                else if (approach == ChoiceApproaches.Careful)
                {
                    modifications.Add(new EncounterValueModification(ValueTypes.Insight, 2,
                        $"Focus Careful"));
                }
                else if (approach == ChoiceApproaches.Aggressive)
                {
                    modifications.Add(new EncounterValueModification(ValueTypes.Pressure, 2,
                        $"Focus Aggressive"));
                }
                else if (approach != ChoiceApproaches.Desperate)
                {
                }

                break;

            case ChoiceArchetypes.Social:
                // Social actions build Resonance
                if (approach == ChoiceApproaches.Strategic)
                {
                    modifications.Add(new EncounterValueModification(ValueTypes.Resonance, 3,
                        $"Social Strategic"));
                }
                else if (approach == ChoiceApproaches.Careful)
                {
                    modifications.Add(new EncounterValueModification(ValueTypes.Resonance, 2,
                        $"Social Careful"));
                }
                else if (approach == ChoiceApproaches.Aggressive)
                {
                    modifications.Add(new EncounterValueModification(ValueTypes.Pressure, 3,
                        $"Social Aggressive"));
                }
                else if (approach == ChoiceApproaches.Desperate)
                {
                }

                break;
        }
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
        if (currentValues.Pressure >= 5)
        {
            modifications.Add(new EncounterValueModification(
                ValueTypes.Outcome,
                -1,
                "Medium Pressure Penalty"
            ));
        }
        if (currentValues.Pressure >= 7)
        {
            modifications.Add(new EncounterValueModification(
                ValueTypes.Outcome,
                -1,
                "High Pressure Penalty"
            ));
        }
        AddEnergyEffects(modifications, choice, currentValues);
    }

    private static void AddEnergyEffects(List<ValueModification> modifications, EncounterChoice choice, EncounterValues currentValues)
    {
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

    private int CalculateEnergyCost(EncounterChoice choice, EncounterContext context, PlayerState player)
    {
        int baseEnergyCost = GameRules.GetBaseEnergyCost(choice.Archetype, choice.Approach);

        EncounterValues currentValues = context.CurrentValues;
        int propertyModifier = locationPropertyCalculator.CalculateEnergyCostModifier(choice, context.LocationProperties);
        int pressureModifier = currentValues.Pressure >= 6 ? currentValues.Pressure - 5 : 0;

        // Apply energy cost reductions from modifications, using projected momentum
        int energyReduction = 0;

        if (currentValues.Momentum > 0)
        {
            energyReduction += Math.Min(currentValues.Momentum / 3, 3);
        }

        // Ensure energy cost doesn't go below 0
        int calculatedCost = baseEnergyCost + propertyModifier + pressureModifier - energyReduction;
        int actualCost = Math.Max(0, calculatedCost);

        return actualCost;
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

    private List<Requirement> CalculateRequirements(EncounterChoice choice, EncounterContext context, PlayerState playerState)
    {
        List<Requirement> requirements = GetEnergyRequirements(choice, context, playerState);

        List<Requirement> specialChoiceRequirements = baseValueGenerator
            .GenerateStrategicRequirements(choice.Archetype, choice.Approach);

        requirements.AddRange(specialChoiceRequirements);

        List<Requirement> propertyRequirements = locationPropertyCalculator
            .CalculateLocationRequirements(choice, context.LocationProperties);

        requirements.AddRange(propertyRequirements);

        return requirements;
    }

    private List<Requirement> GetEnergyRequirements(EncounterChoice choice, EncounterContext context, PlayerState playerState)
    {
        List<Requirement> requirements = new List<Requirement>();

        // Calculate energy cost
        int energyCost = CalculateEnergyCost(choice, context, playerState);

        // Only add energy requirements if the cost is greater than 0
        if (energyCost > 0)
        {
            // Check if the player has enough energy to meet the requirement
            if (choice.EnergyType == EnergyTypes.Physical && gameState.Player.PhysicalEnergy < energyCost)
            {
                int healthCost = energyCost - gameState.Player.PhysicalEnergy;
                requirements.Add(new HealthRequirement(healthCost));
            }
            else if (choice.EnergyType == EnergyTypes.Focus && gameState.Player.FocusEnergy < energyCost)
            {
                int concentrationCost = energyCost - gameState.Player.FocusEnergy;
                requirements.Add(new ConcentrationRequirement(concentrationCost));
            }
            else if (choice.EnergyType == EnergyTypes.Social && gameState.Player.SocialEnergy < energyCost)
            {
                int reputationCost = energyCost - gameState.Player.SocialEnergy;
                requirements.Add(new ReputationRequirement(reputationCost));
            }
        }
        return requirements;
    }

    private List<Outcome> CalculateCosts(EncounterChoice choice, EncounterContext context)
    {
        List<Outcome> costs = baseValueGenerator.GenerateBaseCosts(choice.Archetype, choice.Approach);

        List<Outcome> pressureCosts = baseValueGenerator.CalculatePressureCosts(choice, context);
        costs.AddRange(pressureCosts);

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
}