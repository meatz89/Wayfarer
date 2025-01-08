public static class ChoiceSetContent
{
    public static ChoiceSetTemplate ExampleTemplate => new ChoiceSetTemplateBuilder()
        .WithName("Crowded Labor")
        .WithActionType(BasicActionTypes.Labor)
        .AddAvailabilityCondition(properties => properties
            .WithCrowdLevel(CrowdLevelTypes.Crowded)
            .WithScale(ScaleVariationTypes.Medium))
        .AddStateCondition(values => values
            .WithMaxTension(6))
        .AddChoice(choice => choice
            .WithType(ChoiceTypes.Aggressive)
            .WithBaseEnergyCost(2, EnergyTypes.Physical)
            .WithBaseValueChanges(values => values
                .WithAdvantage(3)
                .WithTension(2))
            .WithRequirement(new SkillRequirement(SkillTypes.Strength, 2))
            .WithCost(new CoinsOutcome(-5))
            .WithReward(new ReputationOutcome(ReputationTypes.Unbreakable, 1)))
        .AddChoice(choice => choice
            .WithType(ChoiceTypes.Careful)
            .WithBaseEnergyCost(1, EnergyTypes.Focus)
            .WithBaseValueChanges(values => values
                .WithAdvantage(1)
                .WithUnderstanding(1))
            .WithRequirement(new UnderstandingRequirement(3)))
        .AddChoice(choice => choice
            .WithType(ChoiceTypes.Tactical)
            .WithBaseEnergyCost(1, EnergyTypes.Social)
            .WithBaseValueChanges(values => values
                .WithAdvantage(1)
                .WithConnection(1))
            .WithReward(new ResourceOutcome(ResourceTypes.Food, 1)))
        .Build();
}
