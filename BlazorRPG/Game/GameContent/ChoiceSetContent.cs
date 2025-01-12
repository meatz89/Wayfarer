
public static class ChoiceSetContent
{
    public static List<ChoiceSetTemplate> AllChoiceSets { get; set; } = new()
    {
        // Opening Opportunities for any complex, focused task
         new ChoiceSetTemplateBuilder()
            .WithName("Starting Complex Work")
            .WithActionType(BasicActionTypes.Labor)
            .AddAvailabilityCondition(properties => properties
                .WithAtmosphere(AtmosphereTypes.Tense)    // Task requires skill/focus
                .WithExposure(ExposureConditionTypes.Indoor)  // Controlled environment
                .WithActivityLevel(ActivityLevelTypes.Quiet))    // Few distractions
            .AddStateCondition(values => values
                .WithMaxOutcome(4)
                .WithMaxPressure(4)
                .WithMaxInsight(4))
            .AddChoice(choice => choice
                // Direct approach that can lead to Rising Tension
                .WithArchetype(ChoiceArchetypes.Physical)
                .WithApproach(ChoiceApproaches.Direct)
                .WithBaseEnergyCost(3)
                .WithBaseValueChanges(values => values
                    .WithOutcome(3)    // Good progress
                    .WithPressure(2))) // But risks complications
            .AddChoice(choice => choice
                // Study approach that leads to Knowledge Advantage
                .WithArchetype(ChoiceArchetypes.Focus)
                .WithApproach(ChoiceApproaches.Tactical)
                .WithBaseEnergyCost(2)
                .WithBaseValueChanges(values => values
                    .WithInsight(3)    // Building understanding
                    .WithOutcome(1)))
            .Build(),

    // Former "Market Haggling Opening" becomes "Social Opening in Busy Space"
    new ChoiceSetTemplateBuilder()
        .WithName("Initial Social Approach")
        .WithActionType(BasicActionTypes.Persuade)
        .AddAvailabilityCondition(properties => properties
            .WithActivityLevel(ActivityLevelTypes.Quiet)      // Need people around
            .WithAtmosphere(AtmosphereTypes.Tense)    // Situation has depth
            .WithExposure(ExposureConditionTypes.Indoor)) // Controlled environment
        .AddStateCondition(values => values
            .WithMaxOutcome(4)
            .WithMaxPressure(4)
            .WithMaxInsight(4)
            .WithMaxResonance(4))
        .AddChoice(choice => choice
            .WithArchetype(ChoiceArchetypes.Social)
            .WithApproach(ChoiceApproaches.Direct)
            .WithBaseValueChanges(values => values
                .WithResonance(3)
                .WithPressure(2))
            .WithSkill(SkillTypes.Haggling))
        .AddChoice(choice => choice
            .WithArchetype(ChoiceArchetypes.Focus)
            .WithApproach(ChoiceApproaches.Tactical)
            .WithBaseValueChanges(values => values
                .WithInsight(3)
                .WithResonance(1)))
        .Build(),

        // === Opening Opportunities Sets ===
        new ChoiceSetTemplateBuilder()
            .WithName("Careful Investigation")
            .WithActionType(BasicActionTypes.Investigate)
            .AddAvailabilityCondition(properties => properties
                .WithActivityLevel(ActivityLevelTypes.Deserted)
                .WithAtmosphere(AtmosphereTypes.Tense)
                .WithSupervision(SupervisionTypes.Unsupervised))
            .AddStateCondition(values => values
                .WithMaxPressure(5))
            .AddChoice(choice => choice
                .WithArchetype(ChoiceArchetypes.Focus)
                .WithApproach(ChoiceApproaches.Direct)
                .WithBaseValueChanges(values => values
                    .WithInsight(3)
                    .WithPressure(1)))
            .AddChoice(choice => choice
                .WithArchetype(ChoiceArchetypes.Focus)
                .WithApproach(ChoiceApproaches.Pragmatic)
                .WithBaseValueChanges(values => values
                    .WithInsight(1)
                    .WithPressure(-1)))
            .AddChoice(choice => choice
                .WithArchetype(ChoiceArchetypes.Social)
                .WithApproach(ChoiceApproaches.Tactical)
                .WithBaseValueChanges(values => values
                    .WithResonance(2)
                    .WithInsight(1)))
            .Build(),

        new ChoiceSetTemplateBuilder()
            .WithName("Busy Service")
            .WithActionType(BasicActionTypes.Labor)
            .AddAvailabilityCondition(properties => properties
                .WithActivityLevel(ActivityLevelTypes.Bustling)
                .WithExposure(ExposureConditionTypes.Indoor)
                .WithAtmosphere(AtmosphereTypes.Causal))
            .AddStateCondition(values => values
                .WithMaxPressure(5)
                .WithMinResonance(2))
            .AddChoice(choice => choice
                .WithArchetype(ChoiceArchetypes.Physical)
                .WithApproach(ChoiceApproaches.Direct)
                .WithSkill(SkillTypes.Service)
                .WithBaseEnergyCost(3)
                .WithBaseValueChanges(values => values
                    .WithOutcome(3)
                    .WithPressure(2)
                    .WithResonance(1))
                .WithReward(new CoinsOutcome(10)))
            .AddChoice(choice => choice
                .WithArchetype(ChoiceArchetypes.Social)
                .WithApproach(ChoiceApproaches.Tactical)
                .WithBaseEnergyCost(2)
                .WithBaseValueChanges(values => values
                    .WithOutcome(1)
                    .WithResonance(2)
                    .WithInsight(1)))
            .Build(),

        // === Rising Tension Sets ===
        new ChoiceSetTemplateBuilder()
            .WithName("Mounting Pressure")
            .WithActionType(BasicActionTypes.Labor)
            .AddAvailabilityCondition(properties => properties
                .WithAtmosphere(AtmosphereTypes.Tense)
                .WithSupervision(SupervisionTypes.Patrolled))
            .AddStateCondition(values => values
                .WithMinPressure(6)
                .WithMaxOutcome(6))
            .AddChoice(choice => choice
                .WithArchetype(ChoiceArchetypes.Physical)
                .WithApproach(ChoiceApproaches.Direct)
                .WithBaseEnergyCost(4)
                .WithBaseValueChanges(values => values
                    .WithOutcome(4)
                    .WithPressure(3)))
            .AddChoice(choice => choice
                .WithArchetype(ChoiceArchetypes.Focus)
                .WithApproach(ChoiceApproaches.Pragmatic)
                .WithBaseValueChanges(values => values
                    .WithInsight(2)
                    .WithPressure(-2)))
            .AddChoice(choice => choice
                .WithArchetype(ChoiceArchetypes.Physical)
                .WithApproach(ChoiceApproaches.Tactical)
                .WithBaseValueChanges(values => values
                    .WithOutcome(2)
                    .WithPressure(-1)))
            .Build(),

        // === Knowledge Advantage Sets ===
        new ChoiceSetTemplateBuilder()
            .WithName("Expert Approach")
            .WithActionType(BasicActionTypes.Labor)
            .AddAvailabilityCondition(properties => properties
                .WithAtmosphere(AtmosphereTypes.Tense))
            .AddStateCondition(values => values
                .WithMinInsight(7))
            .AddChoice(choice => choice
                .WithArchetype(ChoiceArchetypes.Focus)
                .WithApproach(ChoiceApproaches.Direct)
                .WithBaseValueChanges(values => values
                    .WithOutcome(4)
                    .WithInsight(-1)))
            .AddChoice(choice => choice
                .WithArchetype(ChoiceArchetypes.Social)
                .WithApproach(ChoiceApproaches.Tactical)
                .WithBaseValueChanges(values => values
                    .WithOutcome(2)
                    .WithInsight(2)))
            .AddChoice(choice => choice
                .WithArchetype(ChoiceArchetypes.Focus)
                .WithApproach(ChoiceApproaches.Pragmatic)
                .WithBaseValueChanges(values => values
                    .WithOutcome(2)
                    .WithPressure(-1)))
            .Build(),

        // === Resource Management Sets ===
        new ChoiceSetTemplateBuilder()
            .WithName("Resource Pressure")
            .WithActionType(BasicActionTypes.Gather)
            .AddAvailabilityCondition(properties => properties
                .WithSupervision(SupervisionTypes.Patrolled)
                .WithAnyResource())
            .AddStateCondition(values => values
                .WithMinPressure(6))
            .AddChoice(choice => choice
                .WithArchetype(ChoiceArchetypes.Physical)
                .WithApproach(ChoiceApproaches.Direct)
                .WithBaseEnergyCost(4)
                .WithBaseValueChanges(values => values
                    .WithOutcome(4)
                    .WithPressure(3)))
            .AddChoice(choice => choice
                .WithArchetype(ChoiceArchetypes.Focus)
                .WithApproach(ChoiceApproaches.Tactical)
                .WithBaseValueChanges(values => values
                    .WithInsight(2)
                    .WithPressure(-1)))
            .AddChoice(choice => choice
                .WithArchetype(ChoiceArchetypes.Physical)
                .WithApproach(ChoiceApproaches.Pragmatic)
                .WithBaseValueChanges(values => values
                    .WithOutcome(1)
                    .WithPressure(-2)))
            .Build()
    };
}