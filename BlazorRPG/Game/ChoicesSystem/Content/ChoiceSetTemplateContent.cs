public static class ChoiceSetTemplateContent
{
    public static List<ChoiceSetTemplate> ActionChoiceSets => new()
    {
        new ChoiceSetTemplateBuilder()
            .WithName("Find a place to stay for the night")
            .WithActionType(BasicActionTypes.Discuss)
            .AddAvailabilityCondition(properties => properties
                .WithArchetype(LocationArchetypes.Tavern))
            .Build(),
    };
}