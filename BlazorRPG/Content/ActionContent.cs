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

    public static List<ActionTemplate> LibraryActions()
    {
        List<ActionTemplate> actionTemplates = new List<ActionTemplate>();

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Find the ancient tome")
            .WithDescription("I need to find the sacred scripture.")
            .WithActionType(BasicActionTypes.Persuade)
            .StartsEncounter()
            .ExpendsCoins(5)
            .Build());

        return actionTemplates;
    }

}