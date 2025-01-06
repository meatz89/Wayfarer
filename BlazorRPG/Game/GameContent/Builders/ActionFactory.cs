public static class ActionFactory
{
    public static ActionImplementation CreateAction(ActionTemplate template, Location location)
    {
        // Create a new instance based on the template
        ActionImplementation action = template.CreateActionImplementation();

        // Add costs based on location type
        switch (location.LocationType)
        {
            case LocationTypes.Industrial:
                action.Costs.Add(new EnergyOutcome(EnergyTypes.Physical, -1));
                break;
            case LocationTypes.Social:
                action.Costs.Add(new EnergyOutcome(EnergyTypes.Social, -1));
                break;
            case LocationTypes.Nature:
                action.Costs.Add(new EnergyOutcome(EnergyTypes.Focus, -1));
                break;
        }

        // Add rewards based on action type
        switch (action.ActionType)
        {
            case BasicActionTypes.Labor:
                action.Rewards.Add(new CoinsOutcome(3)); // Add 3 coins
                break;
            case BasicActionTypes.Mingle:
                action.Rewards.Add(new ReputationOutcome(location.LocationProperties.ReputationType, 1)); // Add 1 reputation of the type associated with the location
                break;
            case BasicActionTypes.Gather:
                if (location.LocationProperties.Resource.HasValue)
                {
                    action.Rewards.Add(new ResourceOutcome(location.LocationProperties.Resource.Value, 1)); // Add 1 of the location's resource
                }
                break;
        }

        return action;
    }
}