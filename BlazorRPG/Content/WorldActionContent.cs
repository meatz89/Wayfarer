public static class WorldActionContent
{
    public static List<ActionTemplate> GetAllTemplates()
    {
        List<ActionTemplate> actionTemplates = [.. AllActions()];
        return actionTemplates;
    }

    public static List<ActionTemplate> AllActions()
    {
        List<ActionTemplate> actionTemplates = new List<ActionTemplate>();

        // Village actions
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.VillageGathering)
            .WithGoal("gather information about recent events")
            .WithComplication("villagers have conflicting accounts")
            .WithActionType(BasicActionTypes.Discuss)
            .StartsEncounter("VillageSquare")
            .Build());

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.TradeGoods)
            .WithGoal("acquire necessary supplies for your journey")
            .WithComplication("the merchant is suspicious of outsiders")
            .WithActionType(BasicActionTypes.Persuade)
            .StartsEncounter("Merchant")
            .ExpendsCoins(5)
            .Build());

        // Forest actions
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.ForestTravel)
            .WithGoal("traverse the dangerous forest path")
            .WithComplication("bandits are known to ambush travelers")
            .WithActionType(BasicActionTypes.Travel)
            .StartsEncounter("Bandit")
            .Build());

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.SecretMeeting)
            .WithGoal("meet with a mysterious contact")
            .WithComplication("you're uncertain if they can be trusted")
            .WithActionType(BasicActionTypes.Discuss)
            .StartsEncounter("SecretMeeting")
            .Build());

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.SecretDeal)
            .WithGoal("negotiate for contraband or restricted information")
            .WithComplication("authorities may be watching the transaction")
            .WithActionType(BasicActionTypes.Persuade)
            .StartsEncounter("ShadyDeal")
            .ExpendsCoins(8)
            .Build());

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.RentRoom)
            .WithGoal("secure safe lodging for the night")
            .WithComplication("strange noises from the adjacent room")
            .WithActionType(BasicActionTypes.Rest)
            .StartsEncounter("InnRoom")
            .ExpendsCoins(5)
            .Build());

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.FindQuests)
            .WithGoal("discover opportunities for work")
            .WithComplication("some jobs are dangerous or morally questionable")
            .WithActionType(BasicActionTypes.Investigate)
            .StartsEncounter("QuestBoard")
            .Build());

        return actionTemplates;
    }
}