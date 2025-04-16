public static class WorldActionContent
{

    public static List<SpotAction> GetAllTemplates()
    {
        List<SpotAction> actionTemplates = [.. LocationActions(), .. GlobalActions() ];
        foreach(var actionTemplate in actionTemplates)
        {
            actionTemplate.ActionId = actionTemplate.Name;
        }

        return actionTemplates;
    }

    public static List<SpotAction> LocationActions()
    {
        List<SpotAction> actionTemplates = new List<SpotAction>();

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.FollowStream.ToString())
            .WithGoal("follow the stream to another area")
            .WithActionType(BasicActionTypes.Travel)
            .AdvancesTime(1) // 1 hour
            .ExpendsEnergy(15)
            .StartsEncounter(EncounterNames.SearchSurroundings.ToString())
            .MovesToLocationSpot(LocationNames.DeepForest, "High Ground")
            .Build());

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.HuntGame.ToString())
            .WithGoal("hunt for wild game for food")
            .WithComplication("animals are alert and may detect you")
            .WithActionType(BasicActionTypes.Forage)
            .StartsEncounter(EncounterNames.HuntGame.ToString())
            .AdvancesTime(3)
            .AvailableDuring(TimeWindows.Morning, TimeWindows.Afternoon) // Hunting during daylight
            .Build());

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.NightWatch.ToString())
            .WithGoal("keep watch during the night for threats")
            .WithComplication("darkness makes observation difficult")
            .WithActionType(BasicActionTypes.Observe)
            .StartsEncounter(EncounterNames.NightWatch.ToString())
            .AdvancesTime(4)
            .AvailableDuring(TimeWindows.Night) // Only available at night
            .Build());

        // Basic actions
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.Rest.ToString())
            .WithGoal("rest briefly to recover some energy")
            .WithActionType(BasicActionTypes.Rest)
            .AdvancesTime(2) // 2 hours
            .RestoresEnergy(20)
            .IsRepeatableAction()
            .Build());

        // Original tutorial actions
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.RestProperly.ToString())
            .WithGoal("rest fully to recover all energy")
            .WithActionType(BasicActionTypes.Rest)
            .AdvancesTime(4) // 4 hours
            .RestoresEnergy(100) // Full energy
            .IsRepeatableAction()
            .Build());

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.DrinkWater.ToString())
            .WithGoal("drink from the stream to restore health")
            .WithActionType(BasicActionTypes.Consume)
            .AdvancesTime(1) // 30 minutes
            .RestoresHealth(10)
            .RestoresConcentration(10)
            .RestoresConfidence(10)
            .IsRepeatableAction()
            .Build());

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.ObserveArea.ToString())
            .WithGoal("observe the area to gain information")
            .WithActionType(BasicActionTypes.Observe)
            .AdvancesTime(1) // 1 hour
            .ExpendsEnergy(10)
            .IsRepeatableAction()
            .Build());

        // Encounter actions

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.ForageForFood.ToString())
            .WithGoal("search for edible berries and roots")
            .WithComplication("some similar-looking plants are poisonous")
            .WithActionType(BasicActionTypes.Forage)
            .StartsEncounter(EncounterNames.ForageForFood.ToString())
            .AdvancesTime(2) // 2 hours
            .Build());

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.SearchSurroundings.ToString())
            .WithGoal("explore the immediate area to get your bearings")
            .WithComplication("the forest is confusing and disorienting")
            .WithActionType(BasicActionTypes.Explore)
            .StartsEncounter(EncounterNames.SearchSurroundings.ToString())
            .AdvancesTime(1) // 1 hour
            .RewardsLocationSpotKnowledge("Forest Stream")
            .Build());

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.GatherHerbs.ToString())
            .WithGoal("find medicinal plants to restore health")
            .WithComplication("identifying the correct plants requires careful observation")
            .WithActionType(BasicActionTypes.Forage)
            .StartsEncounter(EncounterNames.GatherHerbs.ToString())
            .AdvancesTime(2) // 2 hour
            .RewardsHerbs(2)
            .Build());

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.FindPathOut.ToString())
            .WithGoal("find your way out of the forest")
            .WithComplication("as daylight fades, navigation becomes increasingly difficult")
            .WithActionType(BasicActionTypes.Travel)
            .StartsEncounter(EncounterNames.FindPathOut.ToString())
            .AdvancesTime(3) // 3 hours
            .Build());

        return actionTemplates;
    }

    public static List<SpotAction> GlobalActions()
    {
        List<SpotAction> actionTemplates = new List<SpotAction>();

        // Add consume food action
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.ConsumeFood.ToString())
            .WithGoal("eat food to restore energy")
            .WithActionType(BasicActionTypes.Consume)
            .AdvancesTime(1) // 1 hour
            .ExpendsFood(1)
            .RestoresEnergy(25) // Each food unit restores 25 Energy
            .IsRepeatableAction()
            .Build());

        // Add consume medicinal herbs action
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.ConsumeMedicinalHerbs.ToString())
            .WithGoal("use medicinal herbs to restore health, concentration, and confidence")
            .WithActionType(BasicActionTypes.Consume)
            .AdvancesTime(1) // 1 hour
            .ExpendsMedicinalHerbs(1)
            .RestoresHealth(15)
            .RestoresConcentration(15)
            .RestoresConfidence(15)
            .IsRepeatableAction()
            .Build());

        return actionTemplates;
    }
}