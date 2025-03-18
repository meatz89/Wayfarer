public static class ActionFactory
{
    public static ActionImplementation CreateAction(ActionTemplate template)
    {
        ActionImplementation actionImplementation = new ActionImplementation()
        {
            ActionType = template.ActionType,
            IsEncounterAction = template.IsEncounterAction,
            Name = template.Name,
            Description = template.Description
        };

        actionImplementation.Requirements = template.Requirements;
        actionImplementation.EnergyCosts = template.Energy;
        actionImplementation.Costs = template.Costs;
        actionImplementation.Rewards = template.Rewards;

        // Add energy costs
        EnergyTypes energyType = GameRules.GetEnergyTypeForAction(template.ActionType);
        int energyCost = GameRules.GetBaseEnergyCost(template.ActionType);

        actionImplementation.Requirements.Add(new EnergyRequirement(energyType, energyCost));
        actionImplementation.EnergyCosts.Add(new EnergyOutcome(energyType, -energyCost));

        return actionImplementation;
    }

}