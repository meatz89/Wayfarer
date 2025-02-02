public static class ActionContent
{
    public static List<ActionTemplate> UnlockableActionTemplates()
    {
        List<ActionTemplate> actionTemplates = new List<ActionTemplate>();

        // Labor actions require understanding of market operations
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Help Unload Wagons")
            .WithActionType(BasicActionTypes.Labor)
            .WithCrowdDensity(CrowdDensity.Bustling)
            .WithAccessibility(Accessibility.Public)
            .WithEngagement(Engagement.Service)
            .WithRoomLayout(RoomLayout.Open)
            .Build());

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Prepare Food")
            .WithActionType(BasicActionTypes.Labor)
            .WithCrowdDensity(CrowdDensity.Bustling)
            .WithAccessibility(Accessibility.Public)
            .WithEngagement(Engagement.Service)
            .WithTemperature(Temperature.Warm)
            .Build());

        // Advanced social interactions require trading knowledge
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Haggle With Merchants")
            .WithActionType(BasicActionTypes.Persuade)
            .WithCrowdDensity(CrowdDensity.Bustling)
            .WithAccessibility(Accessibility.Public)
            .WithEngagement(Engagement.Service)
            .WithAtmosphere(Atmosphere.Social)
            .Build());

        return actionTemplates;
    }

    public static List<ActionTemplate> LocationActionTemplates()
    {
        List<ActionTemplate> actionTemplates = new List<ActionTemplate>();

        // Basic observation and investigation - available to everyone
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Browse Wares")
            .WithActionType(BasicActionTypes.Investigate)
            .WithCrowdDensity(CrowdDensity.Bustling)
            .WithAccessibility(Accessibility.Public)
            .WithEngagement(Engagement.Service)
            .WithAtmosphere(Atmosphere.Social)
            .Build());

        // Opportunity to gain initial market knowledge
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Observe Market Activity")
            .WithActionType(BasicActionTypes.Investigate)
            .WithCrowdDensity(CrowdDensity.Bustling)
            .WithAccessibility(Accessibility.Private)
            .WithRoomLayout(RoomLayout.Secluded)
            .WithAtmosphere(Atmosphere.Aloof)
            .Build());

        // Basic social interactions - available to everyone
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Chat With Vendors")
            .WithActionType(BasicActionTypes.Discuss)
            .WithCrowdDensity(CrowdDensity.Bustling)
            .WithAccessibility(Accessibility.Public)
            .WithEngagement(Engagement.Service)
            .WithAtmosphere(Atmosphere.Social)
            .Build());

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Buy Food")
            .WithActionType(BasicActionTypes.Discuss)
            .WithCrowdDensity(CrowdDensity.Bustling)
            .WithAccessibility(Accessibility.Public)
            .WithEngagement(Engagement.Service)
            .WithTemperature(Temperature.Warm)
            .Build());

        return actionTemplates;
    }
}