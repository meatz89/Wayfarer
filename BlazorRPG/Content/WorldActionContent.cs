public static class WorldActionContent
{
    public static List<ActionTemplate> GetAllTemplates()
    {
        List<ActionTemplate> actionTemplates = [.. LocationActions(), .. GlobalActions() ];
        return actionTemplates;
    }

    public static List<ActionTemplate> LocationActions()
    {
        List<ActionTemplate> actionTemplates = new List<ActionTemplate>();

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.FollowStream.ToString())
            .WithGoal("follow the stream to another area")
            .WithActionType(BasicActionTypes.Physical)
            .WithDifficulty(1)
            .WithEncounterChance(20)
            .TimeCostInHours(1) // 1 hour
            .ExpendsEnergy(15)
            .StartsEncounter(EncounterNames.SearchSurroundings.ToString())
            .MovesToLocationSpot(LocationNames.DeepForest, "High Ground")
            .Build());

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.HuntGame.ToString())
            .WithGoal("hunt for wild game for food")
            .WithComplication("animals are alert and may detect you")
            .WithActionType(BasicActionTypes.Physical)
            .WithDifficulty(1)
            .WithEncounterChance(70)
            .StartsEncounter(EncounterNames.HuntGame.ToString())
            .TimeCostInHours(3)
            .AvailableDuring(TimeWindows.Morning, TimeWindows.Afternoon) // Hunting during daylight
            .Build());

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.NightWatch.ToString())
            .WithGoal("keep watch during the night for threats")
            .WithComplication("darkness makes observation difficult")
            .WithActionType(BasicActionTypes.Physical)
            .WithDifficulty(1)
            .WithEncounterChance(50)
            .StartsEncounter(EncounterNames.NightWatch.ToString())
            .TimeCostInHours(4)
            .AvailableDuring(TimeWindows.Night) // Only available at night
            .Build());

        // Basic actions
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.Rest.ToString())
            .WithGoal("rest briefly to recover some energy")
            .WithActionType(BasicActionTypes.Physical)
            .WithDifficulty(1)
            .WithEncounterChance(0)
            .TimeCostInHours(2) // 2 hours
            .RestoresEnergy(20)
            .IsRepeatableAction()
            .Build());

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.RestProperly.ToString())
            .WithGoal("rest fully to recover all energy")
            .WithActionType(BasicActionTypes.Physical)
            .WithDifficulty(1)
            .WithEncounterChance(0)
            .TimeCostInHours(4) // 4 hours
            .RestoresEnergy(100) // Full energy
            .IsRepeatableAction()
            .Build());

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.DrinkWater.ToString())
            .WithGoal("drink from the stream to restore health")
            .WithActionType(BasicActionTypes.Physical)
            .WithDifficulty(1)
            .WithEncounterChance(0)
            .TimeCostInHours(1) // 30 minutes
            .RestoresHealth(10)
            .RestoresConcentration(10)
            .RestoresConfidence(10)
            .IsRepeatableAction()
            .Build());

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.ObserveArea.ToString())
            .WithGoal("observe the area to gain information")
            .WithActionType(BasicActionTypes.Physical)
            .WithDifficulty(1)
            .WithEncounterChance(10)
            .TimeCostInHours(1) // 1 hour
            .ExpendsEnergy(10)
            .IsRepeatableAction()
            .Build());

        // Encounter actions

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.ForageForFood.ToString())
            .WithGoal("search for edible berries and roots")
            .WithComplication("some similar-looking plants are poisonous")
            .WithActionType(BasicActionTypes.Physical)
            .WithDifficulty(1)
            .WithEncounterChance(40)
            .StartsEncounter(EncounterNames.ForageForFood.ToString())
            .TimeCostInHours(2) // 2 hours
            .Build());

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.SearchSurroundings.ToString())
            .WithGoal("explore the immediate area to get your bearings")
            .WithComplication("the forest is confusing and disorienting")
            .WithActionType(BasicActionTypes.Physical)
            .WithDifficulty(1)
            .WithEncounterChance(25)
            .StartsEncounter(EncounterNames.SearchSurroundings.ToString())
            .TimeCostInHours(1) // 1 hour
            .RewardsLocationSpotKnowledge("Forest Stream")
            .Build());

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.GatherHerbs.ToString())
            .WithGoal("find medicinal plants to restore health")
            .WithComplication("identifying the correct plants requires careful observation")
            .WithActionType(BasicActionTypes.Physical)
            .WithDifficulty(1)
            .WithEncounterChance(30)
            .StartsEncounter(EncounterNames.GatherHerbs.ToString())
            .TimeCostInHours(2) // 2 hour
            .RewardsHerbs(2)
            .Build());

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.FindPathOut.ToString())
            .WithGoal("find your way out of the forest")
            .WithComplication("as daylight fades, navigation becomes increasingly difficult")
            .WithActionType(BasicActionTypes.Intellectual)
            .WithDifficulty(3)
            .WithEncounterChance(100)
            .StartsEncounter(EncounterNames.FindPathOut.ToString())
            .TimeCostInHours(3) // 3 hours
            .Build());

        return actionTemplates;
    }

    public static List<ActionTemplate> GlobalActions()
    {
        List<ActionTemplate> actionTemplates = new List<ActionTemplate>();

        // Add consume food action
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.ConsumeFood.ToString())
            .WithGoal("eat food to restore energy")
            .WithActionType(BasicActionTypes.Physical)
            .WithDifficulty(0)
            .WithEncounterChance(0)
            .TimeCostInHours(0)
            .ExpendsFood(1)
            .RestoresEnergy(25) // Each food unit restores 25 Energy
            .IsRepeatableAction()
            .Build());

        // Add consume medicinal herbs action
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.ConsumeMedicinalHerbs.ToString())
            .WithGoal("use medicinal herbs to restore health, concentration, and confidence")
            .WithActionType(BasicActionTypes.Physical)
            .WithDifficulty(0)
            .WithEncounterChance(0)
            .TimeCostInHours(0)
            .ExpendsMedicinalHerbs(1)
            .RestoresHealth(15)
            .RestoresConcentration(15)
            .RestoresConfidence(0)
            .IsRepeatableAction()
            .Build());

        return actionTemplates;
    }
}