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

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.Travel.ToString())
            .WithActionType(BasicActionTypes.Travel)
            .StartsEncounter("Travel")
            .WithGoal("Travel safely to your destination")
            .IsRepeatable()
            .ExpendsEnergy(1)
            .Build());

        // Forest actions
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.DirectPath.ToString())
            .WithGoal("traverse the dangerous forest path")
            .WithComplication("bandits are known to ambush travelers")
            .WithActionType(BasicActionTypes.Travel)
            .StartsEncounter("Bandit")
            .Build());

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.WindingRoad.ToString())
            .WithGoal("travel through the forest by a safer but longer route")
            .WithComplication("a strange hermit claims you're trespassing on sacred ground")
            .WithActionType(BasicActionTypes.Discuss)
            .StartsEncounter("HermitEncounter")
            .Build());

        return actionTemplates;
    }
}