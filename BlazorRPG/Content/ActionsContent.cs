public static class ActionsContent
{
    public static List<ActionTemplate> LoadLocationActionTemplates()
    {
        List<ActionTemplate> actionTemplates = new List<ActionTemplate>();

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Serve Drinks")
            .WithActionType(BasicActionTypes.Labor)
            .WithCrowdDensity(CrowdDensity.Bustling)
            .WithAccessibility(Accessibility.Public)
            .WithEngagement(Engagement.Service)
            .Build());

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Recover at the Hearth")
            .WithActionType(BasicActionTypes.Rest)
            .WithCrowdDensity(CrowdDensity.Bustling)
            .WithLocationScale(LocationScale.Medium)
            .WithTemperature(Temperature.Warm)
            .WithAccessibility(Accessibility.Communal)
            .Build());

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Ask Innkeeper for a place to stay for the night")
            .WithActionType(BasicActionTypes.Persuade)
            .WithCrowdDensity(CrowdDensity.Bustling)
            .WithLocationScale(LocationScale.Medium)
            .WithAccessibility(Accessibility.Public)
            .WithEngagement(Engagement.Service)
            .Build());

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Get local information from other patrons")
            .WithActionType(BasicActionTypes.Investigate)
            .WithCrowdDensity(CrowdDensity.Bustling)
            .WithLocationScale(LocationScale.Medium)
            .WithAccessibility(Accessibility.Private)
            .WithRoomLayout(RoomLayout.Secluded)
            .Build());

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Improve relations with other patrons")
            .WithActionType(BasicActionTypes.Discuss)
            .WithCrowdDensity(CrowdDensity.Bustling)
            .WithLocationScale(LocationScale.Medium)
            .WithAccessibility(Accessibility.Public)
            .WithAtmosphere(Atmosphere.Social)
            .Build());

        return actionTemplates;
    }
}