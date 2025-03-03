public class ChoiceCalculator
{
    //private readonly GameState gameState;
    //private readonly LocationPropertyEffectCalculator locationPropertyCalculator;

    //public ChoiceCalculator(GameState gameState)
    //{
    //    this.gameState = gameState;
    //    this.locationPropertyCalculator = new LocationPropertyEffectCalculator();
    //}


    //public void CalculateChoiceEffects(
    //    Choice choice,
    //    EncounterContext context)
    //{
    //    // 1. Get base values that are inherent to the choice type
    //    //List<ValueModification> valueModifications = GameRules.GetChoiceBaseValueEffects(choice);

    //    // 2. Calculate all modifications from game state and effects
    //    List<ValueModification> modifications = CalculateAllValueChanges(choice);
    //    //valueModifications.AddRange(modifications);

    //    // 3. Calculate new state after combining base values and modifications
    //    PlayerState playerState = gameState.Player;

    //    // 4. Calculate final requirements, costs and rewards
    //    //List<Requirement> requirements = CalculateRequirements(context, choice, playerState, initialEncounterValues, valueModifications);
    //    List<Outcome> costs = CalculateCosts(choice, context);
    //    List<Outcome> rewards = CalculateRewards(choice, context);

    //    // 5. Return complete calculation result
    //    //ChoiceCalculationResult choiceCalculationResult = new ChoiceCalculationResult(
    //    //    valueModifications,    // Modifications with sources
    //    //    requirements,          // Requirements
    //    //    costs,                 // Costs
    //    //    rewards);              // Rewards

    //    //choice.CalculationResult.ValueModifications.AddRange(choiceCalculationResult.ValueModifications);
    //    //choice.CalculationResult.Requirements.AddRange(choiceCalculationResult.Requirements);
    //    //choice.CalculationResult.Costs.AddRange(choiceCalculationResult.Costs);
    //    //choice.CalculationResult.Rewards.AddRange(choiceCalculationResult.Rewards);
    //}


    //private int CalculateEnergyCost(Choice choice, PlayerState player, EncounterContext context)
    //{
    //    int baseEnergyCost = 0; //GameRules.GetBaseEnergyCost(choice.Archetype, choice.Approach);

    //    int propertyModifier = locationPropertyCalculator.CalculateEnergyCostModifier(choice, context);

    //    // Apply energy cost reductions from modifications, using projected momentum
    //    int energyReduction = 0;

    //    // Ensure energy cost doesn't go below 0
    //    int calculatedCost = baseEnergyCost + propertyModifier - energyReduction;
    //    int actualCost = Math.Max(0, calculatedCost);

    //    return actualCost;
    //}

    //private List<ValueModification> CalculateAllValueChanges(Choice choice)
    //{
    //    List<ValueModification> modifications = new();

    //    bool applyPressure = false;

    //    return modifications;
    //}

    //private List<Requirement> CalculateRequirements(EncounterContext context, Choice choice, PlayerState playerState, List<ValueModification> valueModifications)
    //{
    //    List<Requirement> requirements = new List<Requirement> { };

    //    List<Requirement> propertyRequirements = locationPropertyCalculator
    //        .CalculateLocationRequirements(choice, context);

    //    requirements.AddRange(propertyRequirements);

    //    return requirements;
    //}

    //private List<Outcome> CalculateCosts(Choice choice, EncounterContext context)
    //{
    //    //List<Outcome> costs = GenerateBaseCosts(choice.Archetype, choice.Approach);
    //    //List<Outcome> propertyCosts = locationPropertyCalculator.CalculatePropertyCosts(choice, context);
    //    //costs.AddRange(propertyCosts);
    //    //return costs;
    //    return new List<Outcome>();
    //}

    //private List<Outcome> CalculateRewards(Choice choice, EncounterContext context)
    //{
    //    //List<Outcome> rewards = GenerateBaseRewards(choice.Archetype, choice.Approach);
    //    //List<Outcome> propertyRewards = locationPropertyCalculator.CalculatePropertyRewards(choice, context);
    //    //rewards.AddRange(propertyRewards);
    //    //return rewards;
    //    return new List<Outcome>();
    //}

    //private List<Outcome> GenerateBaseCosts(ChoiceArchetypes archetype, ChoiceApproaches approach)
    //{
    //    List<Outcome> costs = new List<Outcome>();
    //    return costs;
    //}

    //private List<Outcome> GenerateBaseRewards(ChoiceArchetypes archetype, ChoiceApproaches approach)
    //{
    //    List<Outcome> rewards = new List<Outcome>();
    //    return rewards;
    //}


}