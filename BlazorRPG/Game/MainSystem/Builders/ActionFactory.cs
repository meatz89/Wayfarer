public static class ActionFactory
{
    public static ActionImplementation CreateAction(ActionTemplate template, Location location)
    {
        // Create a new instance based on the template
        ActionImplementation action = CreateActionImplementationFromTemplate(template);

        // Add rewards based on action type
        switch (action.ActionType)
        {
            case BasicActionTypes.Labor:
                action.Rewards.Add(new CoinsOutcome(3)); // Add 3 coins
                break;
            case BasicActionTypes.Gather:
                break;
        }

        return action;
    }

    private static ActionImplementation CreateActionImplementationFromTemplate(ActionTemplate template)
    {
        ActionImplementation actionImplementation = new ActionImplementation
        {
            Name = template.Name,
            ActionType = template.ActionType,
            TimeSlots = new List<TimeSlots>(template.TimeSlots),
            SpotAvailabilityConditions = template.AvailabilityConditions,
            LocationArchetype = template.LocationArchetype,
            CrowdDensity = template.CrowdDensity,
            LocationScale = template.LocationScale,
            Costs = CreateCostsForAction(template),
            Rewards = CreateRewardsForTemplate(template),
        };
        return actionImplementation;
    }

    private static List<Outcome> CreateCostsForAction(ActionTemplate template)
    {
        var costs = new List<Outcome>();

        var cost = new HealthOutcome(-1);
        costs.Add(cost);

        var energy = new EnergyOutcome(EnergyTypes.Social, -1);
        costs.Add(energy);

        return costs;
    }

    private static List<Outcome> CreateRewardsForTemplate(ActionTemplate template)
    {
        var rewards = new List<Outcome>();

        var reward = new CoinsOutcome(1);
        rewards.Add(reward);

        return rewards;
    }

}