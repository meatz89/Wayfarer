public class ChoiceCalculator
{
    private readonly GameState gameState;
    private readonly LocationPropertyEffectCalculator locationPropertyCalculator;

    public ChoiceCalculator(GameState gameState)
    {
        this.gameState = gameState;
        this.locationPropertyCalculator = new LocationPropertyEffectCalculator();
    }

    public EncounterStageState GetProjectedEncounterState(EncounterChoice choice, EncounterStageState initialValues, List<ValueModification> valueModifications)
    {
        EncounterStageState projectedEncounterState = CalculateNewState(initialValues, choice, valueModifications);
        return projectedEncounterState;
    }

    public void CalculateChoiceEffects(
        EncounterChoice choice,
        EncounterContext context,
        EncounterStageState initialEncounterValues)
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


    private int CalculateEnergyCost(EncounterChoice choice, EncounterStageState initialEncounterValues, PlayerState player, EncounterContext context)
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

    private List<ValueModification> CalculateAllValueChanges(EncounterChoice choice, EncounterStageState initialEncounterValues)
    {
        List<ValueModification> modifications = new();

        bool applyPressure = choice.Approach switch
        {
            ChoiceApproaches.Aggressive => applyPressure = true,
            ChoiceApproaches.Careful => applyPressure = true,
            ChoiceApproaches.Desperate => applyPressure = true,
            _ => false
        };

        return modifications;
    }

    private EncounterStageState ProjectNewState(
        EncounterStageState currentValues,
        List<ValueModification> modifications)
    {
        EncounterStageState newState = new EncounterStageState(currentValues.Momentum);
        foreach (ValueModification mod in modifications)
        {
            if (mod is MomentumModification evm)
            {
                ApplyMomentumChange(newState, evm.Amount);
            }
            else if (mod is EnergyCostReduction em)
            {
                // Do nothing here, energy modifications don't directly affect state values
            }
        }
        return newState;
    }

    private void ApplyMomentumChange(EncounterStageState newState, int amount)
    {
        newState.Momentum += amount;
    }

    private EncounterStageState CalculateNewState(
        EncounterStageState currentValues,
        EncounterChoice choice,
        List<ValueModification> modifications)
    {
        EncounterStageState newState = ProjectNewState(currentValues, modifications);
        newState.LastChoice = choice;
        newState.LastChoiceType = choice.Archetype;
        newState.LastChoiceApproach = choice.Approach;
        return newState;
    }

    private List<Requirement> CalculateRequirements(EncounterContext context, EncounterChoice choice, PlayerState playerState, EncounterStageState encounterValues, List<ValueModification> valueModifications)
    {
        CalculateNewState(encounterValues, choice, valueModifications);

        List<Requirement> requirements = new List<Requirement> { };

        List<Requirement> propertyRequirements = locationPropertyCalculator
            .CalculateLocationRequirements(choice, context);

        requirements.AddRange(propertyRequirements);

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


}