public static class WorldActionContent
{
    public static List<ActionDefinition> GetAllTemplates()
    {
        List<ActionDefinition> actionTemplates = [.. Actions, .. GlobalActions()];
        return actionTemplates;
    }

    public static List<ActionDefinition> Actions = new List<ActionDefinition>
    {
        new ActionDefinition("purchase_simple_room", "Purchase Simple Room")
        { TimeWindows = [TimeWindow.Evening], CoinCost = 5, RestoresEnergy = 20, SpotXp = 10 },
        new ActionDefinition("share_evening_story", "Share Evening Story")
        { TimeWindows = [TimeWindow.Evening], ConfidenceCost = 10, RelationshipGains = [new(){ CharacterName = "Maren", ChangeAmount = 2 }], SpotXp = 8 },
        new ActionDefinition("host_festival_preparation", "Host Festival Preparation")
        { TimeWindows = [TimeWindow.Morning], EnergyCost = 20, RelationshipGains = [new(){ CharacterName = "Maren", ChangeAmount = 5 }], SpotXp = 50, IsOneTimeEncounter = true },
        new ActionDefinition("negotiate_tavern_trade", "Negotiate Tavern Trade")
        { TimeWindows = [TimeWindow.Afternoon], ConcentrationCost = 10, CoinGain = 8, RelationshipGains = [new() { CharacterName = "Maren", ChangeAmount = 1 }], SpotXp = 12 }
    };

    public static List<ActionDefinition> GlobalActions()
    {
        List<ActionDefinition> actionTemplates = new List<ActionDefinition>();

        // Add consume food action
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.ConsumeFood.ToString())
            .WithGoal("eat food to restore energy")
            .WithEncounterType(EncounterTypes.None)
            .WithDifficulty(0)
            .WithEncounterChance(0)
            .WithTimeCost(0)
            .ExpendsFood(1)
            .RestoresEnergy(25) // Each food unit restores 25 Energy
            .IsRepeatableAction()
            .Build());

        // Add consume medicinal herbs action
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName(ActionNames.ConsumeMedicinalHerbs.ToString())
            .WithGoal("use medicinal herbs to restore health, concentration, and confidence")
            .WithEncounterType(EncounterTypes.None)
            .WithDifficulty(0)
            .WithEncounterChance(0)
            .WithTimeCost(0)
            .RestoresHealth(15)
            .RestoresConcentration(15)
            .RestoresConfidence(0)
            .IsRepeatableAction()
            .Build());

        return actionTemplates;
    }
}