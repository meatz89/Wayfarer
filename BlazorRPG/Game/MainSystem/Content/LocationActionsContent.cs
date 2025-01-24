public static class LocationActionsContent
{
    public static List<ActionTemplate> LoadLocationActionTemplates()
    {
        List<ActionTemplate> actionTemplates = new List<ActionTemplate>();

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Recover at the Hearth")
            .SetLocationArchetype(LocationArchetypes.Tavern)
            .WithActionType(BasicActionTypes.Recover)
            .AddTimeSlot(TimeWindows.Morning)
            .AddTimeSlot(TimeWindows.Afternoon)
            .AddTimeSlot(TimeWindows.Evening)
            .AddTimeSlot(TimeWindows.Night)
            .SetCrowdDensity(CrowdDensity.Busy)
            .SetLocationScale(LocationScale.Medium)
            .AddAvailabilityCondition(properties => properties
                .WithTemperature(Temperature.Warm)
                .WithAccessability(Accessability.Communal))
            .Build());


        // Social tutorial action - getting the innkeeper's attention
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Ask Innkeeper for a place to stay for the night")
            .SetLocationArchetype(LocationArchetypes.Tavern)
            .WithActionType(BasicActionTypes.Persuade)
            .AddTimeSlot(TimeWindows.Morning)
            .AddTimeSlot(TimeWindows.Afternoon)
            .AddTimeSlot(TimeWindows.Evening)
            .AddTimeSlot(TimeWindows.Night)
            .SetCrowdDensity(CrowdDensity.Busy)
            .SetLocationScale(LocationScale.Medium)
            .AddAvailabilityCondition(properties => properties
                .WithAccessability(Accessability.Public)
                .WithEngagement(Engagement.Service))
            .Build());

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Get local information from other patrons")
            .SetLocationArchetype(LocationArchetypes.Tavern)
            .WithActionType(BasicActionTypes.Investigate)
            .AddTimeSlot(TimeWindows.Morning)
            .AddTimeSlot(TimeWindows.Afternoon)
            .AddTimeSlot(TimeWindows.Evening)
            .AddTimeSlot(TimeWindows.Night)
            .SetCrowdDensity(CrowdDensity.Busy)
            .SetLocationScale(LocationScale.Medium)
            .AddAvailabilityCondition(properties => properties
                .WithAccessability(Accessability.Private)
                .WithRoomLayout(RoomLayout.Secluded))
            .Build());

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Improve relations with other patrons")
            .SetLocationArchetype(LocationArchetypes.Tavern)
            .WithActionType(BasicActionTypes.Mingle)
            .AddTimeSlot(TimeWindows.Morning)
            .AddTimeSlot(TimeWindows.Afternoon)
            .AddTimeSlot(TimeWindows.Evening)
            .AddTimeSlot(TimeWindows.Night)
            .SetCrowdDensity(CrowdDensity.Busy)
            .SetLocationScale(LocationScale.Medium)
            .AddAvailabilityCondition(properties => properties
                .WithAccessability(Accessability.Public)
                .WithAtmosphere(Atmosphere.Social))
            .Build());

        return actionTemplates;
    }
}