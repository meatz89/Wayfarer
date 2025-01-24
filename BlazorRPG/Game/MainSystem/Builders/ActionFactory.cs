public static class ActionFactory
{
    public static ActionImplementation CreateAction(ActionTemplate template, Location location)
    {
        ActionBuilder builder = new ActionBuilder()
            .ForAction(template.ActionType)
            .WithDescription(template.Name);

        foreach (TimeWindows timeSlot in template.TimeSlots)
        {
            builder.AddTimeSlot(timeSlot);
        }

        // Add energy costs
        int energyCost = GameRules.GetBaseEnergyCost(template.ActionType);
        builder.ExpendsEnergy(energyCost, GameRules.GetEnergyTypeForAction(template.ActionType));

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
            MinValue = 10 + location.DifficultyLevel,
            MaxValue = int.MaxValue,
            Outcomes = GameRules.CreateRewardsForTemplate(template)
        };

        ActionImplementation actionImplementation = builder.Build();
        actionImplementation.OutcomeConditions.Add(failureCondition);
        actionImplementation.OutcomeConditions.Add(successCondition);

        return actionImplementation;
    }

}