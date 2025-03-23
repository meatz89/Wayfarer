public static class ActionContent
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
            .StartsEncounter(EncounterContent.VillageSquareEncounter)
            .Build());

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.ElderCounsel)
            .WithGoal("seek wisdom from the village elder")
            .WithComplication("the elder speaks in riddles and parables")
            .WithActionType(BasicActionTypes.Analyze)
            .StartsEncounter(EncounterContent.ElderEncounter)
            .Build());

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.TradeGoods)
            .WithGoal("acquire necessary supplies for your journey")
            .WithComplication("the merchant is suspicious of outsiders")
            .WithActionType(BasicActionTypes.Persuade)
            .StartsEncounter(EncounterContent.MerchantEncounter)
            .ExpendsCoins(5)
            .Build());

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.GatherInformation)
            .WithGoal("learn about nearby dangers")
            .WithComplication("locals are reluctant to speak openly")
            .WithActionType(BasicActionTypes.Investigate)
            .StartsEncounter(EncounterContent.WellEncounter)
            .Build());

        // Forest actions
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.ForestTravel)
            .WithGoal("traverse the dangerous forest path")
            .WithComplication("bandits are known to ambush travelers")
            .WithActionType(BasicActionTypes.Travel)
            .StartsEncounter(EncounterContent.BanditEncounter)
            .Build());

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.HuntingTraining)
            .WithGoal("learn hunting skills from an experienced tracker")
            .WithComplication("the game is scarce and the conditions harsh")
            .WithActionType(BasicActionTypes.Labor)
            .StartsEncounter(EncounterContent.HuntingEncounter)
            .Build());

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.NatureStudy)
            .WithGoal("discover medicinal plants and their properties")
            .WithComplication("many poisonous plants resemble healing ones")
            .WithActionType(BasicActionTypes.Study)
            .StartsEncounter(EncounterContent.HerbalismEncounter)
            .Build());

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.SecretMeeting)
            .WithGoal("meet with a mysterious contact")
            .WithComplication("you're uncertain if they can be trusted")
            .WithActionType(BasicActionTypes.Discuss)
            .StartsEncounter(EncounterContent.SecretMeetingEncounter)
            .Build());

        // Tavern actions
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.DrinkAndGossip)
            .WithGoal("extract valuable information from patrons")
            .WithComplication("alcohol loosens tongues but clouds memories")
            .WithActionType(BasicActionTypes.Discuss)
            .StartsEncounter(EncounterContent.TavernGossipEncounter)
            .ExpendsCoins(2)
            .Build());

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.SecretDeal)
            .WithGoal("negotiate for contraband or restricted information")
            .WithComplication("authorities may be watching the transaction")
            .WithActionType(BasicActionTypes.Persuade)
            .StartsEncounter(EncounterContent.ShadyDealEncounter)
            .ExpendsCoins(8)
            .Build());

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.RentRoom)
            .WithGoal("secure safe lodging for the night")
            .WithComplication("strange noises from the adjacent room")
            .WithActionType(BasicActionTypes.Rest)
            .StartsEncounter(EncounterContent.InnRoomEncounter)
            .ExpendsCoins(5)
            .Build());

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.FindQuests)
            .WithGoal("discover opportunities for work")
            .WithComplication("some jobs are dangerous or morally questionable")
            .WithActionType(BasicActionTypes.Investigate)
            .StartsEncounter(EncounterContent.QuestBoardEncounter)
            .Build());

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.MeetContact)
            .WithGoal("exchange information with an informant")
            .WithComplication("they demand proof of your trustworthiness")
            .WithActionType(BasicActionTypes.Persuade)
            .StartsEncounter(EncounterContent.InformantEncounter)
            .Build());

        return actionTemplates;
    }
}