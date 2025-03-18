public static class ActionContent
{
    public static List<ActionTemplate> GetAllTemplates()
    {
        List<ActionTemplate> actionTemplates =
            [.. AllActions()];

        return actionTemplates;
    }

    public static List<ActionTemplate> AllActions()
    {
        List<ActionTemplate> actionTemplates = new List<ActionTemplate>();

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.LibraryResearch)
            .WithGoal("find the sacred scripture.")
            .WithComplication("the libaray is vast and he doesn't know where to look.")
            .WithActionType(BasicActionTypes.Analyze)
            .StartsEncounter()
            .ExpendsCoins(5)
            .Build());

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.BackalleyTravel)
            .WithGoal("reach the docks without harm.")
            .WithComplication("a bandit is blocking the way.")
            .WithActionType(BasicActionTypes.Fight)
            .StartsEncounter()
            .Build());

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.MerchantPersuasion)
            .WithGoal("buy food but he has limited funds.")
            .WithComplication("the prices of the merchants are too high.")
            .WithActionType(BasicActionTypes.Persuade)
            .StartsEncounter()
            .ExpendsCoins(5)
            .Build());

        return actionTemplates;
    }

}