public static class LocationActionsContent
{
    public static List<ActionTemplate> LoadLocationActionTemplates()
    {
        List<ActionTemplate> actionTemplates = new List<ActionTemplate>();

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Recover at the Hearth")
            .SetLocationArchetype(LocationArchetypes.Tavern)
            .WithActionType(BasicActionTypes.Recover)
            .SetTimeSlot(TimeWindows.Morning)
            .SetTimeSlot(TimeWindows.Afternoon)
            .SetTimeSlot(TimeWindows.Evening)
            .SetTimeSlot(TimeWindows.Night)
            .SetCrowdDensity(CrowdDensity.Busy)
            .SetLocationScale(LocationScale.Medium)
            .WithTemperature(Temperature.Warm)
            .WithAccessibility(Accessibility.Communal)
            .Build());


        // Social tutorial action - getting the innkeeper's attention
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Ask Innkeeper for a place to stay for the night")
            .SetLocationArchetype(LocationArchetypes.Tavern)
            .WithActionType(BasicActionTypes.Persuade)
            .SetTimeSlot(TimeWindows.Morning)
            .SetTimeSlot(TimeWindows.Afternoon)
            .SetTimeSlot(TimeWindows.Evening)
            .SetTimeSlot(TimeWindows.Night)
            .SetCrowdDensity(CrowdDensity.Busy)
            .SetLocationScale(LocationScale.Medium)
            .WithAccessibility(Accessibility.Public)
            .WithEngagement(Engagement.Service)
            .Build());

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Get local information from other patrons")
            .SetLocationArchetype(LocationArchetypes.Tavern)
            .WithActionType(BasicActionTypes.Investigate)
            .SetTimeSlot(TimeWindows.Morning)
            .SetTimeSlot(TimeWindows.Afternoon)
            .SetTimeSlot(TimeWindows.Evening)
            .SetTimeSlot(TimeWindows.Night)
            .SetCrowdDensity(CrowdDensity.Busy)
            .SetLocationScale(LocationScale.Medium)
            .WithAccessibility(Accessibility.Private)
            .WithRoomLayout(RoomLayout.Secluded)
            .Build());

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Improve relations with other patrons")
            .SetLocationArchetype(LocationArchetypes.Tavern)
            .WithActionType(BasicActionTypes.Mingle)
            .SetTimeSlot(TimeWindows.Morning)
            .SetTimeSlot(TimeWindows.Afternoon)
            .SetTimeSlot(TimeWindows.Evening)
            .SetTimeSlot(TimeWindows.Night)
            .SetCrowdDensity(CrowdDensity.Busy)
            .SetLocationScale(LocationScale.Medium)
            .WithAccessibility(Accessibility.Public)
            .WithAtmosphere(Atmosphere.Social)
            .Build());

        return actionTemplates;
    }
}