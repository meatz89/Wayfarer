public static class LocationActionsContent
{
    public static List<ActionTemplate> LoadLocationActionTemplates()
    {
        List<ActionTemplate> actionTemplates = new List<ActionTemplate>();

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Recover at the Hearth")
            .WithActionType(BasicActionTypes.Recover)
            .AddTimeSlot(TimeSlots.Morning)
            .AddTimeSlot(TimeSlots.Afternoon)
            .AddTimeSlot(TimeSlots.Evening)
            .AddTimeSlot(TimeSlots.Night)
            .SetLocationArchetype(LocationArchetypes.Tavern)
            .SetCrowdDensity(CrowdDensity.Busy)
            .SetLocationScale(LocationScale.Medium)
            .AddAvailabilityCondition(properties => properties
                .WithArchetype(LocationArchetypes.Tavern)
                .WithTemperature(Temperature.Warm)
                .WithAccessability(Accessability.Communal))
            .Build());


        // Social tutorial action - getting the innkeeper's attention
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Ask Innkeeper for a place to stay for the night")
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

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Get local information from other patrons")
            .WithActionType(BasicActionTypes.Investigate)
            .AddTimeSlot(TimeSlots.Morning)
            .AddTimeSlot(TimeSlots.Afternoon)
            .AddTimeSlot(TimeSlots.Evening)
            .AddTimeSlot(TimeSlots.Night)
            .SetLocationArchetype(LocationArchetypes.Tavern)
            .SetCrowdDensity(CrowdDensity.Busy)
            .SetLocationScale(LocationScale.Medium)
            .AddAvailabilityCondition(properties => properties
                .WithArchetype(LocationArchetypes.Tavern)
                .WithAccessability(Accessability.Private)
                .WithRoomLayout(RoomLayout.Secluded))
            .Build());

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Improve relations with other patrons")
            .WithActionType(BasicActionTypes.Mingle)
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
                .WithAtmosphere(Atmosphere.Social))
            .Build());

        return actionTemplates;
    }
}