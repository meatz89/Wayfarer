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

        // Forest actions
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.DirectForestTravel)
            .WithGoal("traverse the dangerous forest path")
            .WithComplication("bandits are known to ambush travelers")
            .WithActionType(BasicActionTypes.Travel)
            .StartsEncounter("Bandit")
            .Build());

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.ScenicForestTravel)
            .WithGoal("travel through the forest by a safer but longer route")
            .WithComplication("a strange hermit claims you're trespassing on sacred ground")
            .WithActionType(BasicActionTypes.Travel)
            .StartsEncounter("HermitEncounter")
            .Build());

        return actionTemplates;
    }
}