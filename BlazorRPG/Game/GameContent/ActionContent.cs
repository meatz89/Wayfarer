public static class ActionContent
{
    public static List<ActionTemplate> LoadActionTemplates()
    {
        List<ActionTemplate> actionTemplates = new List<ActionTemplate>();

        // Social tutorial action - getting the innkeeper's attention
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Seek Innkeeper's Attention")
            .WithActionType(BasicActionTypes.Persuade)
            .AddTimeSlot(TimeSlots.Morning)
            .AddTimeSlot(TimeSlots.Afternoon)
            .AddTimeSlot(TimeSlots.Evening)
            .AddTimeSlot(TimeSlots.Night)
            .SetLocationArchetype(LocationArchetypes.Tavern)
            .SetCrowdDensity(CrowdDensity.Busy)
            .SetLocationScale(LocationScale.Medium)
            .AddAvailabilityCondition(properties => properties
                .WithArchetype(LocationArchetypes.Tavern)
                .WithAccessability(Accessability.Public)
                .WithEngagement(Engagement.Service))
            .Build());

        return actionTemplates;
    }
}