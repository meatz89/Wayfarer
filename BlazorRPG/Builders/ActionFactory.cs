using System.Xml.Linq;

public static class ActionFactory
{
    public static ActionImplementation CreateAction(ActionTemplate template)
    {
        ActionImplementation actionImplementation = new ActionImplementation();

        actionImplementation.ActionType = template.ActionType;
        actionImplementation.Name = template.Name;
        actionImplementation.Requirements = template.Requirements;
        actionImplementation.EnergyCosts = template.Energy;
        actionImplementation.Costs = template.Costs;
        actionImplementation.Rewards = template.Rewards;

        actionImplementation.Goal = template.Goal;
        actionImplementation.Complication = template.Complication;

        actionImplementation.IsEncounterAction = template.IsEncounterAction;
        actionImplementation.EncounterTemplate = template.EncounterTemplate;

        // Add energy costs
        EnergyTypes energyType = GameRules.GetEnergyTypeForAction(template.ActionType);
        int energyCost = GameRules.GetBaseEnergyCost(template.ActionType);

        actionImplementation.Requirements.Add(new EnergyRequirement(energyType, energyCost));
        actionImplementation.EnergyCosts.Add(new EnergyOutcome(energyType, -energyCost));

        return actionImplementation;
    }

}