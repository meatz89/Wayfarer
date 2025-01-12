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
                .WithExposure(ExposureTypes.Indoor)  // Controlled environment
                .WithActivityLevel(ActivityLevelTypes.Quiet))    // Few distractions
            .AddStateCondition(values => values
                .WithMaxOutcome(4)
                .WithMaxPressure(4)
                .WithMaxInsight(4))
            .AddChoice(choice => choice
                // Direct approach that can lead to Rising Tension
                .WithArchetype(ChoiceArchetypes.Physical)
                .WithApproach(ChoiceApproaches.Direct))
            .AddChoice(choice => choice
                // Study approach that leads to Knowledge Advantage
                .WithArchetype(ChoiceArchetypes.Focus)
                .WithApproach(ChoiceApproaches.Tactical))
            .Build(),

    // Former "Market Haggling Opening" becomes "Social Opening in Busy Space"
    new ChoiceSetTemplateBuilder()
        .WithName("Initial Social Approach")
        .WithActionType(BasicActionTypes.Persuade)
        .AddAvailabilityCondition(properties => properties
            .WithActivityLevel(ActivityLevelTypes.Quiet)      // Need people around
            .WithAtmosphere(AtmosphereTypes.Tense)    // Situation has depth
            .WithExposure(ExposureTypes.Indoor)) // Controlled environment
        .AddStateCondition(values => values
            .WithMaxOutcome(4)
            .WithMaxPressure(4)
            .WithMaxInsight(4)
            .WithMaxResonance(4))
        .AddChoice(choice => choice
            .WithArchetype(ChoiceArchetypes.Social)
            .WithApproach(ChoiceApproaches.Direct))
        .AddChoice(choice => choice
            .WithArchetype(ChoiceArchetypes.Focus)
            .WithApproach(ChoiceApproaches.Tactical))
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
                .WithApproach(ChoiceApproaches.Direct))
            .AddChoice(choice => choice
                .WithArchetype(ChoiceArchetypes.Focus)
                .WithApproach(ChoiceApproaches.Pragmatic))
            .AddChoice(choice => choice
                .WithArchetype(ChoiceArchetypes.Social)
                .WithApproach(ChoiceApproaches.Tactical))
            .Build(),

        new ChoiceSetTemplateBuilder()
            .WithName("Busy Service")
            .WithActionType(BasicActionTypes.Labor)
            .AddAvailabilityCondition(properties => properties
                .WithAccessibility(AccessibilityTypes.Public)
                .WithActivityLevel(ActivityLevelTypes.Bustling)
                .WithAtmosphere(AtmosphereTypes.Causal))
            .AddChoice(choice => choice
                .WithArchetype(ChoiceArchetypes.Physical)
                .WithApproach(ChoiceApproaches.Direct))
            .AddChoice(choice => choice
                .WithArchetype(ChoiceArchetypes.Social)
                .WithApproach(ChoiceApproaches.Tactical))
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
                .WithApproach(ChoiceApproaches.Direct))
            .AddChoice(choice => choice
                .WithArchetype(ChoiceArchetypes.Focus)
                .WithApproach(ChoiceApproaches.Pragmatic))
            .AddChoice(choice => choice
                .WithArchetype(ChoiceArchetypes.Physical)
                .WithApproach(ChoiceApproaches.Tactical))
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
                .WithApproach(ChoiceApproaches.Direct))
            .AddChoice(choice => choice
                .WithArchetype(ChoiceArchetypes.Social)
                .WithApproach(ChoiceApproaches.Tactical))
            .AddChoice(choice => choice
                .WithArchetype(ChoiceArchetypes.Focus)
                .WithApproach(ChoiceApproaches.Pragmatic))
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
                .WithApproach(ChoiceApproaches.Direct))
            .AddChoice(choice => choice
                .WithArchetype(ChoiceArchetypes.Focus)
                .WithApproach(ChoiceApproaches.Tactical))
            .AddChoice(choice => choice
                .WithArchetype(ChoiceArchetypes.Physical)
                .WithApproach(ChoiceApproaches.Pragmatic))
            .Build()
    };
}