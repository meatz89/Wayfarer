public static class ActionFactory
{
    public static ActionImplementation CreateAction(ActionTemplate template, Location location)
    {
        // Create a new instance based on the template
        ActionImplementation action = template.CreateActionImplementation();

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
}