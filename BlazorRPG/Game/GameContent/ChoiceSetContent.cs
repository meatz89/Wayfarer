public static class ChoiceSetContent
{
    public static ChoiceSetTemplate ExampleTemplate => new ChoiceSetTemplateBuilder()
        .WithName("Crowded Labor")
        .WithActionType(BasicActionTypes.Labor)
        .AddAvailabilityCondition(properties => properties
            .WithCrowdLevel(CrowdLevelTypes.Crowded)
            .WithScale(ScaleVariationTypes.Medium))
        .AddStateCondition(values => values
            .WithMaxPressure(6))
        .AddChoice(choice => choice
            .WithType(ChoiceTypes.Aggressive)
            .WithBaseEnergyCost(2, EnergyTypes.Physical)
            .WithBaseValueChanges(values => values
                .WithOutcome(3)
                .WithPressure(2))
            .WithRequirement(new SkillRequirement(SkillTypes.Strength, 2))
            .WithCost(new CoinsOutcome(-5))
            .WithReward(new ReputationOutcome(ReputationTypes.Unbreakable, 1)))
        .AddChoice(choice => choice
            .WithType(ChoiceTypes.Careful)
            .WithBaseEnergyCost(1, EnergyTypes.Focus)
            .WithBaseValueChanges(values => values
                .WithOutcome(1)
                .WithInsight(1))
            .WithRequirement(new InsightRequirement(3)))
        .AddChoice(choice => choice
            .WithType(ChoiceTypes.Tactical)
            .WithBaseEnergyCost(1, EnergyTypes.Social)
            .WithBaseValueChanges(values => values
                .WithOutcome(1)
                .WithResonance(1))
            .WithReward(new ResourceOutcome(ResourceTypes.Food, 1)))
        .Build();

    public static ChoiceSetTemplate MingleChoiceSetTemplate => new ChoiceSetTemplateBuilder()
        .WithName("Mingle Choices")
        .WithActionType(BasicActionTypes.Mingle)
        .AddAvailabilityCondition(properties => properties
            .WithArchetype(LocationArchetypes.Tavern)) // Assuming you have this location type
        .AddChoice(choice => choice
            .WithType(ChoiceTypes.Aggressive)
            .WithBaseEnergyCost(2, EnergyTypes.Social)
            .WithBaseValueChanges(values => values
                .WithResonance(3)
                .WithPressure(2))
            .WithRequirement(new SkillRequirement(SkillTypes.Charisma, 2))
            .WithCost(new CoinsOutcome(-3)) // Costs 3 coins
            .WithReward(new ReputationOutcome(ReputationTypes.RoguesGuild, 1))) // Rewards 1 RoguesGuild reputation
        .AddChoice(choice => choice
            .WithType(ChoiceTypes.Careful)
            .WithBaseEnergyCost(1, EnergyTypes.Focus)
            .WithBaseValueChanges(values => values
                .WithInsight(1)
                .WithResonance(1))
            .WithRequirement(new CoinsRequirement(5))) // Requires 5 coins
        .Build();
}
