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
            .WithGoal("find the sacred scripture")
            .WithComplication("the libaray is vast and he doesn't know where to look")
            .WithActionType(BasicActionTypes.Analyze)
            .StartsEncounter(EncounterContent.LibraryEncounter)
            .ExpendsCoins(5)
            .Build());

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.BackalleyTravel)
            .WithGoal("reach the ancient library without harm")
            .WithComplication("A bandit blocks the way, clearly intending to rob you")
            .WithActionType(BasicActionTypes.Fight)
            .StartsEncounter(EncounterContent.BanditEncounter)
            .Build());

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.MerchantPersuasion)
            .WithGoal("buy food but he has limited funds")
            .WithComplication("the prices of the merchants are too high")
            .WithActionType(BasicActionTypes.Persuade)
            .StartsEncounter(EncounterContent.MerchantEncounter)
            .ExpendsCoins(5)
            .Build());

        return actionTemplates;
    }

}