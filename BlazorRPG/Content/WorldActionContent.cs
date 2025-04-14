public static class WorldActionContent
{
    public static List<SpotAction> GetAllTemplates()
    {
        List<SpotAction> actionTemplates = [.. AllActions()];
        foreach(var actionTemplate in actionTemplates)
        {
            actionTemplate.ActionId = actionTemplate.Name;
        }

        return actionTemplates;
    }

    public static List<SpotAction> AllActions()
    {
        List<SpotAction> actionTemplates = new List<SpotAction>();

        // Basic actions
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.Rest.ToString())
            .WithGoal("rest briefly to recover some energy")
            .WithActionType(BasicActionTypes.Rest)
            .AdvancesTime(2) // 2 hours
            .RestoresEnergy(20)
            .IsRepeatable()
            .Build());

        // Add consume food action
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.ConsumeFood.ToString())
            .WithGoal("eat food to restore energy")
            .WithActionType(BasicActionTypes.Consume)
            .AdvancesTime(1) // 1 hour
            .RequiresFood(1)
            .RestoresEnergy(25) // Each food unit restores 25 Energy
            .IsRepeatable()
            .Build());

        // Add consume medicinal herbs action
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.ConsumeMedicinalHerbs.ToString())
            .WithGoal("use medicinal herbs to restore health, concentration, and confidence")
            .WithActionType(BasicActionTypes.Consume)
            .AdvancesTime(1) // 1 hour
            .RequiresMedicinalHerbs(1)
            .RestoresHealth(15)
            .RestoresConcentration(15)
            .RestoresConfidence(15)
            .IsRepeatable()
            .Build());

        // Original tutorial actions
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.ForageForFood.ToString())
            .WithGoal("search for edible berries and roots")
            .WithComplication("some similar-looking plants are poisonous")
            .WithActionType(BasicActionTypes.Forage)
            .StartsEncounter("ForageForFood")
            .AdvancesTime(2)
            .Build());

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.RestProperly.ToString())
            .WithGoal("rest fully to recover all energy")
            .WithActionType(BasicActionTypes.Rest)
            .AdvancesTime(4) // 4 hours
            .RestoresEnergy(100) // Full energy
            .IsRepeatable()
            .Build());

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.DrinkWater.ToString())
            .WithGoal("drink from the stream to restore health")
            .WithActionType(BasicActionTypes.Consume)
            .AdvancesTime(1) // 30 minutes
            .RestoresHealth(10)
            .RestoresConcentration(10)
            .RestoresConfidence(10)
            .IsRepeatable()
            .Build());

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.FollowStream.ToString())
            .WithGoal("follow the stream to another area")
            .WithActionType(BasicActionTypes.Travel)
            .AdvancesTime(1) // 1 hour
            .ExpendsEnergy(15)
            .MovesToLocationSpot(LocationNames.DeepForest, "High Ground")
            .Build());

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.ObserveArea.ToString())
            .WithGoal("observe the area to gain information")
            .WithActionType(BasicActionTypes.Observe)
            .AdvancesTime(1) // 1 hour
            .ExpendsEnergy(10)
            .IsRepeatable()
            .Build());

        // Encounter actions
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.ForageForFood.ToString())
            .WithGoal("search for edible berries and roots")
            .WithComplication("some similar-looking plants are poisonous")
            .WithActionType(BasicActionTypes.Forage)
            .StartsEncounter("ForageForFood")
            .AdvancesTime(2) // 2 hours
            .Build());

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.SearchSurroundings.ToString())
            .WithGoal("explore the immediate area to get your bearings")
            .WithComplication("the forest is confusing and disorienting")
            .WithActionType(BasicActionTypes.Explore)
            .StartsEncounter("SearchSurroundings")
            .AdvancesTime(1) // 1 hour
            .Build());

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.GatherHerbs.ToString())
            .WithGoal("find medicinal plants to restore health")
            .WithComplication("identifying the correct plants requires careful observation")
            .WithActionType(BasicActionTypes.Forage)
            .StartsEncounter("GatherHerbs")
            .AdvancesTime(2) // 2 hours
            .Build());

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.ClimbTree.ToString())
            .WithGoal("climb a tall tree to gain a vantage point")
            .WithComplication("the branches are slippery and unstable")
            .WithActionType(BasicActionTypes.Climb)
            .StartsEncounter("ClimbTree")
            .AdvancesTime(2) // 2 hours
            .Build());

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.SurveyArea.ToString())
            .WithGoal("use the higher ground to gain information about the forest")
            .WithComplication("the vastness of the forest makes orientation difficult")
            .WithActionType(BasicActionTypes.Observe)
            .StartsEncounter("SearchSurroundings")
            .AdvancesTime(2) // 2 hours
            .Build());

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.MoveStealthily.ToString())
            .WithGoal("move quietly through the underbrush")
            .WithComplication("fallen branches and leaves make silent movement challenging")
            .WithActionType(BasicActionTypes.Travel)
            .StartsEncounter("MoveStealthily")
            .AdvancesTime(2) // 2 hours
            .Build());

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.ForceThrough.ToString())
            .WithGoal("push forcibly through the dense vegetation")
            .WithComplication("thorns and sharp branches present hazards")
            .WithActionType(BasicActionTypes.Travel)
            .StartsEncounter("SearchSurroundings")
            .AdvancesTime(1) // 1 hour
            .Build());

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.FindNaturalPath.ToString())
            .WithGoal("locate natural animal trails through the vegetation")
            .WithComplication("paths may lead in unexpected directions")
            .WithActionType(BasicActionTypes.Explore)
            .StartsEncounter("SearchSurroundings")
            .AdvancesTime(2) // 2 hours
            .Build());

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.FindPathOut.ToString())
            .WithGoal("find your way out of the forest")
            .WithComplication("as daylight fades, navigation becomes increasingly difficult")
            .WithActionType(BasicActionTypes.Travel)
            .StartsEncounter("FindPathOut")
            .AdvancesTime(3) // 3 hours
            .Build());

        return actionTemplates;
    }
}