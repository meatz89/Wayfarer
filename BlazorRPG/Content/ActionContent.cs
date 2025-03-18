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
            .WithDescription("I need to find the sacred scripture.")
            .WithActionType(BasicActionTypes.Analyze)
            .StartsEncounter()
            .ExpendsCoins(5)
            .Build());

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.BackalleyTravel)
            .WithDescription("Navigate past the bandit and reach your destination without harm.")
            .WithActionType(BasicActionTypes.Fight)
            .StartsEncounter()
            .Build());

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.MerchantPersuasion)
            .WithDescription("Convince a merchant to sell you food at a discounted price despite your limited funds.")
            .WithActionType(BasicActionTypes.Persuade)
            .StartsEncounter()
            .ExpendsCoins(5)
            .Build());

        return actionTemplates;
    }

}