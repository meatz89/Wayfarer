public static class ChoiceSetContent
{
    public static ChoiceSetTemplate ExampleTemplate => new ChoiceSetTemplateBuilder()
        .WithName("Crowded Labor")
        .WithLocationArchetype(LocationArchetypes.Library)
        .WithActionType(BasicActionTypes.Labor)
        .AddAvailabilityCondition(properties => properties
            .WithCrowdLevel(CrowdLevelTypes.Crowded)
            .WithScale(ScaleVariationTypes.Medium))
        .AddStateCondition(values => values
            .WithMaxPressure(6))
        .AddChoice(choice => choice
            .WithSkill(SkillTypes.Strength)
            .WithBaseEnergyCost(2, EnergyTypes.Physical)
            .WithBaseValueChanges(values => values
                .WithOutcome(3) // High Outcome
                .WithPressure(2) // High Pressure
                )
            .WithRequirement(new SkillRequirement(SkillTypes.Strength, 2))
            .WithCost(new CoinsOutcome(-5))
            .WithReward(new ReputationOutcome(ReputationTypes.Unbreakable, 1)))
        .AddChoice(choice => choice
            .WithSkill(SkillTypes.Strength)
            .WithBaseEnergyCost(1, EnergyTypes.Focus)
            .WithBaseValueChanges(values => values
                .WithOutcome(1) // Low Outcome
                .WithInsight(2) // High Insight
                )
            .WithRequirement(new InsightRequirement(3)))
        .AddChoice(choice => choice
            .WithSkill(SkillTypes.Strength)
            .WithBaseEnergyCost(1, EnergyTypes.Social)
            .WithBaseValueChanges(values => values
                .WithOutcome(2) // Medium Outcome
                .WithResonance(1)) // Some Resonance
            .WithReward(new ResourceOutcome(ResourceTypes.Food, 1)))
        .Build();

    public static ChoiceSetTemplate MingleChoiceSetTemplate => new ChoiceSetTemplateBuilder()
        .WithName("Mingle Choices")
        .WithActionType(BasicActionTypes.Mingle)
        .WithLocationArchetype(LocationArchetypes.Tavern)
        .AddAvailabilityCondition(properties => properties
            .WithArchetype(LocationArchetypes.Tavern)) // Assuming you have this location type
        .AddChoice(choice => choice
            .WithSkill(SkillTypes.Strength)
            .WithBaseEnergyCost(2, EnergyTypes.Social)
            .WithBaseValueChanges(values => values
                .WithResonance(3)
                .WithPressure(2))
            .WithRequirement(new SkillRequirement(SkillTypes.Charisma, 2))
            .WithCost(new CoinsOutcome(-3)) // Costs 3 coins
            .WithReward(new ReputationOutcome(ReputationTypes.RoguesGuild, 1))) // Rewards 1 RoguesGuild reputation
        .AddChoice(choice => choice
            .WithSkill(SkillTypes.Strength)
            .WithBaseEnergyCost(1, EnergyTypes.Focus)
            .WithBaseValueChanges(values => values
                .WithInsight(1)
                .WithResonance(1))
            .WithRequirement(new CoinsRequirement(5))) // Requires 5 coins
        .Build();
}
