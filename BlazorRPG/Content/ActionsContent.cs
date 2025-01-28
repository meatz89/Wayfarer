public static class ActionsContent
{
    public static List<ActionTemplate> LoadLocationActionTemplates()
    {
        List<ActionTemplate> actionTemplates = new List<ActionTemplate>();

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Recover at the Hearth")
            .WithLocationArchetype(LocationArchetypes.Tavern)
            .WithActionType(BasicActionTypes.Rest)
            .WithTimeSlot(TimeWindows.Evening)
            .WithCrowdDensity(CrowdDensity.Busy)
            .WithLocationScale(LocationScale.Medium)
            .WithTemperature(Temperature.Warm)
            .WithAccessibility(Accessibility.Communal)
            .Build());


        // Social tutorial action - getting the innkeeper's attention
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Ask Innkeeper for a place to stay for the night")
            .WithLocationArchetype(LocationArchetypes.Tavern)
            .WithActionType(BasicActionTypes.Persuade)
            .WithTimeSlot(TimeWindows.Evening)
            .WithCrowdDensity(CrowdDensity.Busy)
            .WithLocationScale(LocationScale.Medium)
            .WithAccessibility(Accessibility.Public)
            .WithEngagement(Engagement.Service)
            .Build());

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Get local information from other patrons")
            .WithLocationArchetype(LocationArchetypes.Tavern)
            .WithActionType(BasicActionTypes.Investigate)
            .WithTimeSlot(TimeWindows.Evening)
            .WithCrowdDensity(CrowdDensity.Busy)
            .WithLocationScale(LocationScale.Medium)
            .WithAccessibility(Accessibility.Private)
            .WithRoomLayout(RoomLayout.Secluded)
            .Build());

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Improve relations with other patrons")
            .WithLocationArchetype(LocationArchetypes.Tavern)
            .WithActionType(BasicActionTypes.Discuss)
            .WithTimeSlot(TimeWindows.Evening)
            .WithCrowdDensity(CrowdDensity.Busy)
            .WithLocationScale(LocationScale.Medium)
            .WithAccessibility(Accessibility.Public)
            .WithAtmosphere(Atmosphere.Social)
            .Build());

        return actionTemplates;
    }
}