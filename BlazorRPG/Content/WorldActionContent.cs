using Microsoft.VisualBasic.FileIO;

public static class WorldActionContent
{
    public static List<ActionDefinition> GetAllTemplates()
    {
        List<ActionDefinition> actionTemplates = [.. LocationActions(), .. GlobalActions()];
        return actionTemplates;
    }

    public static List<ActionDefinition> LocationActions()
    {
        List<ActionDefinition> actionTemplates = new List<ActionDefinition>();

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Read Signpost")
            .WithGoal("Examine the weathered signpost and the surrounding area")
            .WithComplication("")
            .WithEncounterType(EncounterTypes.Lore)
            .WithEnergyCost(5)
            .WithTimeCost(10)
            .WithEncounterChance(10)
            .WithDifficulty(1)
            .IsRepeatableAction()
            .Build());

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Village Gate Inspection")
            .WithGoal("Examine the Village Gate and the surrounding area")
            .WithComplication("")
            .WithEncounterType(EncounterTypes.Social)
            .WithEnergyCost(5)
            .WithTimeCost(10)
            .WithEncounterChance(10)
            .WithDifficulty(1)
            .IsRepeatableAction()
            .Build());

        return actionTemplates;
    }

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
            .ExpendsMedicinalHerbs(1)
            .RestoresHealth(15)
            .RestoresConcentration(15)
            .RestoresConfidence(0)
            .IsRepeatableAction()
            .Build());

        return actionTemplates;
    }
}