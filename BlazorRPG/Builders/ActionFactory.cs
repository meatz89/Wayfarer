public static class ActionFactory
{
    public static ActionImplementation CreateAction(ActionTemplate template, Location location)
    {
        ActionBuilder builder = new ActionBuilder()
            .ForAction(template.ActionType)
            .WithDescription(template.Name);

        // Add energy costs
        int energyCost = GameRules.GetBaseEnergyCost(template.ActionType);

        EnergyTypes energyType = GameRules.GetEnergyTypeForAction(template.ActionType);
        if (energyType != EnergyTypes.None)
        {
            builder.ExpendsEnergy(energyCost, energyType);
        }

        // Add success/failure conditions
        OutcomeCondition failureCondition = new OutcomeCondition
        {
            EncounterResults = EncounterResults.EncounterFailure,
            ValueType = ValueTypes.Outcome,
            MaxValue = 0,
            MinValue = int.MinValue,
            Outcomes = GameRules.CreateCostsForAction(template)
        };

        OutcomeCondition successCondition = new OutcomeCondition
        {
            EncounterResults = EncounterResults.EncounterSuccess,
            ValueType = ValueTypes.Outcome,
            MinValue = 10 + location.Difficulty,
            MaxValue = int.MaxValue,
            Outcomes = GameRules.CreateRewardsForTemplate(template)
        };

        ActionImplementation actionImplementation = builder.Build();
        actionImplementation.OutcomeConditions.Add(failureCondition);
        actionImplementation.OutcomeConditions.Add(successCondition);

        return actionImplementation;
    }

}