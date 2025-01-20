public static class ChoiceSetContent
{
    public static List<ChoiceSetTemplate> TutorialSequence => new()
    {
        new ChoiceSetTemplateBuilder()
            .WithName("Finding a place to stay for the night")
            .WithActionType(BasicActionTypes.Discuss)
            .AddAvailabilityCondition(properties => properties
                .WithArchetype(LocationArchetypes.Tavern))
            //.AddStateCondition(values => values
            //    .WithMaxOutcome(3)     // Early social positioning
            //    .WithMaxPressure(4)    // Some social pressure but not overwhelming
            //    .WithMaxResonance(5))  // Room to build connections
            .Build()
    };
}