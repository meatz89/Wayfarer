public static class ChoiceSetContent
{
    public static List<ChoiceSetTemplate> AllChoiceSets { get; set; } = new()
    {
        // Heavy Labor - Simple physical task with focus support
        new ChoiceSetTemplateBuilder()
            .WithName("Heavy Labor")
            .WithActionType(BasicActionTypes.Labor)  // This automatically sets up 2 Physical + 1 Focus
            .AddAvailabilityCondition(properties => properties
                .WithActivityLevel(ActivityLevelTypes.Quiet)
                .WithSupervision(SupervisionTypes.Unsupervised))
            .AddStateCondition(values => values
                .WithMaxPressure(5))
            .Build(),

        // Starting Complex Work - Balanced physical and mental approach
        new ChoiceSetTemplateBuilder()
            .WithName("Starting Complex Work")
            .WithActionType(BasicActionTypes.Labor)
            .WithComposition(new CompositionPattern
            {
                PrimaryArchetype = ChoiceArchetypes.Physical,
                SecondaryArchetype = ChoiceArchetypes.Focus,
                PrimaryCount = 1,    // One physical for direct action
                SecondaryCount = 1    // One focus for tactical planning
            })
            .AddAvailabilityCondition(properties => properties
                .WithAtmosphere(AtmosphereTypes.Tense)
                .WithExposure(ExposureTypes.Indoor)
                .WithActivityLevel(ActivityLevelTypes.Quiet))
            .AddStateCondition(values => values
                .WithMaxOutcome(4)
                .WithMaxPressure(4)
                .WithMaxInsight(4))
            .Build(),

        // Initial Social Approach - Social lead with focus support
        new ChoiceSetTemplateBuilder()
            .WithName("Initial Social Approach")
            .WithActionType(BasicActionTypes.Persuade)  // This sets up 2 Social + 1 Focus
            .AddAvailabilityCondition(properties => properties
                .WithActivityLevel(ActivityLevelTypes.Quiet)
                .WithAtmosphere(AtmosphereTypes.Tense)
                .WithExposure(ExposureTypes.Indoor))
            .AddStateCondition(values => values
                .WithMaxOutcome(4)
                .WithMaxPressure(4)
                .WithMaxInsight(4)
                .WithMaxResonance(4))
            .Build(),

        // Careful Investigation - Focus-heavy with social backup
        new ChoiceSetTemplateBuilder()
            .WithName("Careful Investigation")
            .WithActionType(BasicActionTypes.Investigate)  // This sets up 2 Focus + 1 Social
            .AddAvailabilityCondition(properties => properties
                .WithActivityLevel(ActivityLevelTypes.Deserted)
                .WithAtmosphere(AtmosphereTypes.Tense)
                .WithSupervision(SupervisionTypes.Unsupervised))
            .AddStateCondition(values => values
                .WithMaxPressure(5))
            .Build(),

        // Busy Service - Physical lead with social support
        new ChoiceSetTemplateBuilder()
            .WithName("Busy Service")
            .WithActionType(BasicActionTypes.Labor)
            .WithComposition(new CompositionPattern
            {
                PrimaryArchetype = ChoiceArchetypes.Physical,
                SecondaryArchetype = ChoiceArchetypes.Social,
                PrimaryCount = 1,
                SecondaryCount = 1
            })
            .AddAvailabilityCondition(properties => properties
                .WithAccessibility(AccessibilityTypes.Public)
                .WithActivityLevel(ActivityLevelTypes.Bustling)
                .WithAtmosphere(AtmosphereTypes.Relaxed))
            .Build(),

        // Mounting Pressure - Physical focus with tactical options
        new ChoiceSetTemplateBuilder()
            .WithName("Mounting Pressure")
            .WithActionType(BasicActionTypes.Labor)
            .WithComposition(new CompositionPattern
            {
                PrimaryArchetype = ChoiceArchetypes.Physical,
                SecondaryArchetype = ChoiceArchetypes.Focus,
                PrimaryCount = 2,    // Two physical choices
                SecondaryCount = 1    // One focus for tactical support
            })
            .AddAvailabilityCondition(properties => properties
                .WithAtmosphere(AtmosphereTypes.Tense)
                .WithSupervision(SupervisionTypes.Patrolled))
            .AddStateCondition(values => values
                .WithMinPressure(6)
                .WithMaxOutcome(6))
            .Build()
    };
}