public static class ActionContent
{

    public static List<ActionTemplate> UnlockableActionTemplates()
    {
        List<ActionTemplate> actionTemplates = new List<ActionTemplate>();

        // Labor actions require understanding of market operations
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Help Unload Wagons")
            .WithActionType(BasicActionTypes.Labor)
            .Build());

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Prepare Food")
            .WithActionType(BasicActionTypes.Labor)
            .Build());

        // Advanced social interactions require trading knowledge
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Haggle With Merchants")
            .WithActionType(BasicActionTypes.Persuade)
            .Build());

        return actionTemplates;
    }

    public static List<ActionTemplate> InnActionTemplates()
    {
        List<ActionTemplate> actionTemplates = new List<ActionTemplate>();

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Pay for Room")
            .WithActionType(BasicActionTypes.Persuade)
            .ExpendsCoins(5)
            .Build());

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Haggle price for room")
            .WithDescription("Maybe I could get a discount for the room.")
            .WithActionType(BasicActionTypes.Persuade)
            .StartsEncounter()
            .ExpendsCoins(5)
            .Build());

        return actionTemplates;
    }

    public static List<ActionTemplate> MarketActionTemplates()
    {
        List<ActionTemplate> actionTemplates = new List<ActionTemplate>();

        // Basic observation and investigation - available to everyone
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Browse Wares")
            .WithActionType(BasicActionTypes.Investigate)
            .Build());

        // Opportunity to gain initial market knowledge
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Observe Market Activity")
            .WithActionType(BasicActionTypes.Investigate)
            .Build());

        // Basic social interactions - available to everyone
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Chat With Vendors")
            .WithActionType(BasicActionTypes.Discuss)
            .Build());

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Buy Food")
            .WithActionType(BasicActionTypes.Discuss)
            .Build());

        return actionTemplates;
    }
}