using System.Numerics;

public class ChoiceCalculator
{
    private readonly GameState gameState;
    private readonly ChoiceEffectsGenerator baseValueGenerator;
    private readonly LocationPropertyEffectCalculator locationPropertyCalculator;

    public ChoiceCalculator(GameState gameState)
    {
        this.gameState = gameState;
        this.baseValueGenerator = new ChoiceEffectsGenerator();
        this.locationPropertyCalculator = new LocationPropertyEffectCalculator();
    }

    public ChoiceCalculationResult CalculateChoiceEffects(EncounterChoice choice, EncounterContext context)
    {
        // 1. Get base values that are inherent to the choice type
        List<BaseValueChange> baseChanges = baseValueGenerator.GenerateBaseValueChanges(choice.Archetype, choice.Approach);

        // 2. Calculate all modifications from game state and effects
        List<ValueModification> valueModifications = CalculateAllValueChanges(choice, context);

        // 3. Calculate new state after combining base values and modifications
        EncounterStateValues newState = CalculateNewState(context.CurrentValues, choice, baseChanges, valueModifications);

        // 4. Calculate final requirements, costs and rewards
        List<Requirement> requirements = CalculateRequirements(choice, context, gameState.Player);
        List<Outcome> costs = CalculateCosts(choice, context);
        List<Outcome> rewards = CalculateRewards(choice, context);

        // 5. Store all results in the choice for UI preview
        choice.BaseEncounterValueChanges = baseChanges;
        choice.ValueModifications = valueModifications;
        choice.Requirements = requirements;
        choice.Costs = costs;
        choice.Rewards = rewards;
        choice.EnergyCost = CalculateEnergyCost(choice, context, gameState.Player);

        // 6. Return complete calculation result
        return new ChoiceCalculationResult(
            newState,
            baseChanges,          // Base values
            valueModifications,   // Modifications with sources
            choice.EnergyType,    // Energy type
            choice.EnergyCost,    // Energy cost
            requirements,         // Requirements
            costs,                // Costs
            rewards);             // Rewards
    }

    private List<ValueModification> CalculateAllValueChanges(EncounterChoice choice, EncounterContext context)
    {
        List<ValueModification> modifications = new();

        // First handle value decay - this affects what resources we have available
        AddDecayModifications(modifications, choice, context);

        // Then handle state-based modifications that affect our value gains
        AddStateModifications(modifications, choice, context);

        // Finally calculate outcome generation based on our archetype, approach and current values
        AddOutcomeModifications(modifications, choice, context);

        return modifications;
    }


    private void AddOutcomeModifications(List<ValueModification> modifications, EncounterChoice choice, EncounterContext context)
    {
        // Project the state after applying all current modifications
        EncounterStateValues currentValues = context.CurrentValues;

        // Now handle outcome generation based on archetype and approach
        switch (choice.Archetype)
        {
            case ChoiceArchetypes.Physical:
                AddPhysicalOutcomeModifications(modifications, choice, currentValues);
                break;
            case ChoiceArchetypes.Focus:
                AddFocusOutcomeModifications(modifications, choice, currentValues);
                break;
            case ChoiceArchetypes.Social:
                AddSocialOutcomeModifications(modifications, choice, currentValues);
                break;
        }
    }

    private void AddPhysicalOutcomeModifications(List<ValueModification> modifications, EncounterChoice choice, EncounterStateValues values)
    {
        switch (choice.Approach)
        {
            case ChoiceApproaches.Direct:
                // Generate outcome from momentum at 2:1 if pressure is low
                if (values.Momentum > 0 && values.Pressure < 3)
                {
                    int conversionAmount = values.Momentum / 2;
                    modifications.Add(new EncounterValueModification(
                        ValueTypes.Outcome,
                        conversionAmount,
                        "outcome generation 2:1 if pressure is under 3"));
                }
                break;

            case ChoiceApproaches.Improvised:
                // Convert all momentum to outcome 1:1
                if (values.Momentum > 0)
                {
                    modifications.Add(new EncounterValueModification(
                        ValueTypes.Outcome,
                        values.Momentum,
                        "Conversion To outcome 1:1"));
                    modifications.Add(new EncounterValueModification(
                        ValueTypes.Momentum,
                        -values.Momentum,
                        "Conversion To outcome 1:1"));
                }
                break;
        }
    }

    private void AddFocusOutcomeModifications(List<ValueModification> modifications, EncounterChoice choice, EncounterStateValues values)
    {
        switch (choice.Approach)
        {
            case ChoiceApproaches.Direct:
                // Generate outcome if insight exceeds pressure
                if (values.Insight > values.Pressure)
                {
                    modifications.Add(new EncounterValueModification(
                        ValueTypes.Outcome,
                        1,
                        "insight exceeds pressure"));
                }
                break;

            case ChoiceApproaches.Tactical:
                // Convert insight at 2:1 ratio
                if (values.Insight >= 2)
                {
                    int conversionAmount = values.Insight / 2;
                    modifications.Add(new EncounterValueModification(
                        ValueTypes.Outcome,
                        conversionAmount,
                        "insight at 2:1 ratio"));
                }
                break;

            case ChoiceApproaches.Improvised:
                // Generate outcome if insight threshold met
                if (values.Insight >= 5)
                {
                    modifications.Add(new EncounterValueModification(
                        ValueTypes.Outcome,
                        1,
                        "insight threshold 5 met"));
                }
                break;
        }
    }

    private void AddSocialOutcomeModifications(List<ValueModification> modifications, EncounterChoice choice, EncounterStateValues values)
    {
        switch (choice.Approach)
        {
            case ChoiceApproaches.Direct:
                // Convert excess resonance to outcome
                if (values.Resonance > values.Pressure)
                {
                    int outcomeGain = values.Resonance - values.Pressure;
                    modifications.Add(new EncounterValueModification(
                        ValueTypes.Outcome,
                        outcomeGain,
                        "excess resonance to outcome"));
                }
                break;

            case ChoiceApproaches.Pragmatic:
                // Convert resonance at 3:1 ratio
                if (values.Resonance >= 3)
                {
                    int conversionAmount = values.Resonance / 3;
                    modifications.Add(new EncounterValueModification(
                        ValueTypes.Outcome,
                        conversionAmount,
                        "resonance at 3:1 ratio"));
                }
                break;

            case ChoiceApproaches.Tactical:
                // Convert resonance at 2:1 and reduce pressure
                if (values.Resonance > 0)
                {
                    int conversionAmount = values.Resonance / 2;
                    modifications.Add(new EncounterValueModification(
                        ValueTypes.Outcome,
                        conversionAmount,
                        "convert resonance at 2:1 and reduce pressure"));
                    modifications.Add(new EncounterValueModification(
                        ValueTypes.Pressure,
                        -2,
                        "convert resonance at 2:1 and reduce pressure"));
                    modifications.Add(new EncounterValueModification(
                        ValueTypes.Resonance,
                        -values.Resonance,
                        "convert resonance at 2:1 and reduce pressure"));
                }
                break;

            case ChoiceApproaches.Improvised:
                // Convert resonance at 4:1 if pressure is low
                if (values.Resonance >= 4 && values.Pressure < 4)
                {
                    modifications.Add(new EncounterValueModification(
                        ValueTypes.Outcome,
                        1,
                        "Convert resonance at 4:1 if pressure is low"));
                    modifications.Add(new EncounterValueModification(
                        ValueTypes.Resonance,
                        -4,
                        "Convert resonance at 4:1 if pressure is low"));
                }
                break;
        }
    }

    private void AddDecayModifications(List<ValueModification> modifications, EncounterChoice choice, EncounterContext context)
    {
        // First handle value decay
        if (context.CurrentValues.LastChoiceType.HasValue)
        {
            // Momentum decays by 2 each turn without physical action
            if (context.CurrentValues.LastChoiceType != ChoiceArchetypes.Physical)
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
    }

    private void AddStateModifications(List<ValueModification> modifications, EncounterChoice choice, EncounterContext context)
    {
        // Momentum reduces energy costs
        if (context.CurrentValues.Momentum > 0)
        {
            modifications.Add(new EnergyCostReduction(
                choice.EnergyType,
                Math.Min(context.CurrentValues.Momentum, 3),
                "Momentum Energy Reduction"
            ));
        };

        // Insight amplifies value gains
        if (context.CurrentValues.Insight >= 2)
        {
            foreach (BaseValueChange baseChange in choice.BaseEncounterValueChanges)
            {
                if (baseChange.Amount > 0 && baseChange.ValueType != ValueTypes.Pressure)
                {
                    modifications.Add(new EncounterValueModification(
                        baseChange.ValueType,
                        context.CurrentValues.Insight / 2,
                        "Insight Amplification"));
                }
            }
        }

        // Resonance reduces pressure gain
        if (context.CurrentValues.Resonance > 0 &&
            choice.BaseEncounterValueChanges.Any(x => x.ValueType == ValueTypes.Pressure))
        {
            modifications.Add(new EncounterValueModification(
                ValueTypes.Pressure,
                -Math.Min(context.CurrentValues.Resonance, 3),
                "Resonance Pressure Reduction"));
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
        newState.LastChoiceApproach = choice.Approach;
        return newState;
    }

    private List<Requirement> CalculateRequirements(EncounterChoice choice, EncounterContext context, PlayerState playerState)
    {
        List<Requirement> requirements = GetEnergyRequirements(choice, context, playerState);

        List<Requirement> specialChoiceRequirements = baseValueGenerator.GenerateSpecialRequirements(choice.Archetype, choice.Approach);
        requirements.AddRange(specialChoiceRequirements);

        List<Requirement> propertyRequirements = locationPropertyCalculator.CalculateLocationRequirements(choice, context.LocationProperties);
        requirements.AddRange(propertyRequirements);

        return requirements;
    }

    private List<Requirement> GetEnergyRequirements(EncounterChoice choice, EncounterContext context, PlayerState playerState)
    {
        List<Requirement> requirements = new List<Requirement>();

        // Add alternative requirements if not enough energy
        int energyCost = CalculateEnergyCost(choice, context, playerState);

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
        else
        {
            // If the player has enough energy, add the energy requirement
            switch (choice.EnergyType)
            {
                case EnergyTypes.Physical:
                    requirements.Add(new EnergyRequirement(EnergyTypes.Physical, energyCost));
                    break;
                case EnergyTypes.Focus:
                    requirements.Add(new EnergyRequirement(EnergyTypes.Focus, energyCost));
                    break;
                case EnergyTypes.Social:
                    requirements.Add(new EnergyRequirement(EnergyTypes.Social, energyCost));
                    break;
            }
        }
        return requirements;
    }

    private List<Outcome> CalculateCosts(EncounterChoice choice, EncounterContext context)
    {
        List<Outcome> costs = baseValueGenerator.GenerateBaseCosts(choice.Archetype, choice.Approach);
        List<Outcome> propertyCosts = locationPropertyCalculator.CalculatePropertyCosts(choice, context.LocationProperties);
        costs.AddRange(propertyCosts);
        return costs;
    }

    private int CalculateEnergyCost(EncounterChoice choice, EncounterContext context, PlayerState player)
    {
        int baseEnergyCost = baseValueGenerator.GenerateBaseEnergyCost(choice.Archetype, choice.Approach);
        int propertyModifier = locationPropertyCalculator.CalculateEnergyCostModifier(choice, context.LocationProperties);
        int pressureModifier = context.CurrentValues.Pressure >= 6 ? context.CurrentValues.Pressure - 5 : 0;

        return baseEnergyCost + propertyModifier + pressureModifier;
    }

    private List<Outcome> CalculateRewards(EncounterChoice choice, EncounterContext context)
    {
        List<Outcome> rewards = baseValueGenerator.GenerateBaseRewards(choice.Archetype, choice.Approach);
        List<Outcome> propertyRewards = locationPropertyCalculator.CalculatePropertyRewards(choice, context.LocationProperties);
        rewards.AddRange(propertyRewards);
        return rewards;
    }
}