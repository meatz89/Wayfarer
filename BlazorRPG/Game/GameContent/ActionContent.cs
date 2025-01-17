public static class ActionContent
{
    public static List<ActionTemplate> LoadActionTemplates()
    {
        List<ActionTemplate> actionTemplates = new List<ActionTemplate>();

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Clear Fallen Tree")
            .WithActionType(BasicActionTypes.Labor)
            .AddTimeSlot(TimeSlots.Morning)
            .AddTimeSlot(TimeSlots.Afternoon)
            .AddAvailabilityCondition(properties => properties
                .WithArchetype(LocationArchetypes.Forest)
                .WithExposure(ExposureTypes.Outdoor)
                .WithActivityLevel(ActivityLevelTypes.Deserted))
            .Build());

        // Focus tutorial action - deciphering the signpost
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Study Ancient Waymarks")
            .WithActionType(BasicActionTypes.Investigate)
            .AddTimeSlot(TimeSlots.Morning)
            .AddTimeSlot(TimeSlots.Afternoon)
            .AddAvailabilityCondition(properties => properties
                .WithArchetype(LocationArchetypes.Crossroads)
                .WithActivityLevel(ActivityLevelTypes.Deserted)
                .WithAtmosphere(AtmosphereTypes.Mysterious))
            .Build());

        // Social tutorial action - getting the innkeeper's attention
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Seek Innkeeper's Attention")
            .WithActionType(BasicActionTypes.Persuade)
            .AddTimeSlot(TimeSlots.Morning)
            .AddTimeSlot(TimeSlots.Afternoon)
            .AddTimeSlot(TimeSlots.Evening)
            .AddAvailabilityCondition(properties => properties
                .WithArchetype(LocationArchetypes.Tavern)
                .WithActivityLevel(ActivityLevelTypes.Bustling)
                .WithAccessibility(AccessibilityTypes.Public))
            .Build());

        return actionTemplates;
    }
}