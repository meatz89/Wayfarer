public static class ChoiceSetContent
{
    public static ChoiceSetTemplate ServingDrinks => new ChoiceSetTemplateBuilder()
        .WithName("Rush Hour Service")
        .WithActionType(BasicActionTypes.Labor)
        .AddAvailabilityCondition(properties => properties
            .WithArchetype(LocationArchetypes.Tavern)
            .WithCrowdLevel(CrowdLevelTypes.Crowded)
            .WithExposure(ExposureConditionTypes.Indoor))
        .AddStateCondition(values => values
            .WithMaxPressure(5)
            .WithMinResonance(2))
        .AddChoice(choice => choice
            .WithArchetype(ChoiceArchetypes.Physical)
            .WithApproach(ChoiceApproaches.Aggressive)
            .WithSkill(SkillTypes.Service)
            .WithBaseEnergyCost(3, EnergyTypes.Physical)
            .WithBaseValueChanges(values => values
                .WithOutcome(3)
                .WithPressure(2)
                .WithResonance(1))
            .WithRequirement(new SkillRequirement(SkillTypes.Service, 1))
            .WithReward(new CoinsOutcome(10)))
        .AddChoice(choice => choice
            .WithArchetype(ChoiceArchetypes.Social)
            .WithApproach(ChoiceApproaches.Strategic)
            .WithSkill(SkillTypes.Service)
            .WithBaseEnergyCost(2, EnergyTypes.Social)
            .WithBaseValueChanges(values => values
                .WithOutcome(1)
                .WithResonance(2)
                .WithInsight(1))
            .WithReward(new ReputationOutcome(ReputationTypes.Reliable, 1)))
        .AddChoice(choice => choice
            .WithArchetype(ChoiceArchetypes.Focus)
            .WithApproach(ChoiceApproaches.Careful)
            .WithSkill(SkillTypes.Service)
            .WithBaseEnergyCost(2, EnergyTypes.Focus)
            .WithBaseValueChanges(values => values
                .WithOutcome(2)
                .WithInsight(2)
                .WithPressure(-1)))
        .Build();
}