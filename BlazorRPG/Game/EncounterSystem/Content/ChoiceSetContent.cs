public static class ChoiceSetContent
{
    public static List<ChoiceSetTemplate> TutorialSequence => new()
    {
        // First encounter: Physical challenge with the fallen tree
        // Teaches: Momentum building, pressure management, and physical energy
        new ChoiceSetTemplateBuilder()
            .WithName("Clearing the Path")
            .WithActionType(BasicActionTypes.Labor)
            .WithComposition(new CompositionPattern
            {
                PrimaryArchetype = ChoiceArchetypes.Physical,
                SecondaryArchetype = ChoiceArchetypes.Focus,
                PrimaryCount = 2,    // Two physical choices for direct and pragmatic approaches
                SecondaryCount = 1    // One focus choice for tactical assessment
            })
            .AddAvailabilityCondition(properties => properties
                .WithArchetype(LocationArchetypes.Forest)
                .WithActivityLevel(ActivityLevelTypes.Deserted)
                .WithExposure(ExposureTypes.Outdoor)
                .WithSupervision(SupervisionTypes.Unsupervised))
            //.AddStateCondition(values => values
            //    .WithMaxOutcome(3)    // Early stage - plenty of work ahead
            //    .WithMaxPressure(4)   // Not too threatening yet
            //    .WithMaxMomentum(3))  // Room to build momentum
            .Build(),

        // Second encounter: Mental challenge at the crossroads
        // Teaches: Insight building, outcome generation through understanding
        new ChoiceSetTemplateBuilder()
            .WithName("Decoding Ancient Signs")
            .WithActionType(BasicActionTypes.Investigate)
            .WithComposition(new CompositionPattern
            {
                PrimaryArchetype = ChoiceArchetypes.Focus,
                SecondaryArchetype = ChoiceArchetypes.Physical,
                PrimaryCount = 2,    // Two focus choices for direct observation and careful study
                SecondaryCount = 1    // One physical choice to clear debris/get better view
            })
            .AddAvailabilityCondition(properties => properties
                .WithArchetype(LocationArchetypes.Crossroads)
                .WithActivityLevel(ActivityLevelTypes.Deserted)
                .WithAtmosphere(AtmosphereTypes.Mysterious))
            //.AddStateCondition(values => values
            //    .WithMaxOutcome(3)    // Early stage investigation
            //    .WithMaxPressure(4)   // Some time pressure but not urgent
            //    .WithMaxInsight(5))   // Room to build understanding
            .Build(),

        // Third encounter: Social challenge at the tavern
        // Teaches: Resonance building, social energy management
        new ChoiceSetTemplateBuilder()
            .WithName("Finding Your Place")
            .WithActionType(BasicActionTypes.Persuade)
            .WithComposition(new CompositionPattern
            {
                PrimaryArchetype = ChoiceArchetypes.Social,
                SecondaryArchetype = ChoiceArchetypes.Focus,
                PrimaryCount = 2,    // Two social choices for different approaches
                SecondaryCount = 1    // One focus choice to read the room
            })
            .AddAvailabilityCondition(properties => properties
                .WithArchetype(LocationArchetypes.Tavern)
                .WithActivityLevel(ActivityLevelTypes.Bustling)
                .WithAccessibility(AccessibilityTypes.Public)
                .WithAtmosphere(AtmosphereTypes.Relaxed))
            //.AddStateCondition(values => values
            //    .WithMaxOutcome(3)     // Early social positioning
            //    .WithMaxPressure(4)    // Some social pressure but not overwhelming
            //    .WithMaxResonance(5))  // Room to build connections
            .Build()
    };
}