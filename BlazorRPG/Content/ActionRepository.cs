public class ActionRepository
{
    private WorldState _worldState;

    public ActionRepository(GameWorld gameWorld)
    {
        _worldState = gameWorld.WorldState;
    }

    public void AddAction(ActionDefinition action)
    {
        if (_worldState.actions.Any(a =>
        {
            return a.Name.Equals(action.Name, StringComparison.OrdinalIgnoreCase);
        }))
            throw new InvalidOperationException($"Action '{action.Name}' already exists.");

        _worldState.actions.Add(action);
    }

    public List<ActionDefinition> GetAllActions()
    {
        return _worldState.actions;
    }

    public List<ActionDefinition> GetActionsForSpot(string spotId)
    {
        List<ActionDefinition> actions = new List<ActionDefinition>();
        foreach (ActionDefinition action in _worldState.actions)
        {
            if (action.LocationSpotId == spotId)
            {
                actions.Add(action);
            }
        }

        return actions;
    }


    public ActionDefinition GetAction(string actionId)
    {
        ActionDefinition action = _worldState.actions.FirstOrDefault(a =>
        {
            return a.Id.Equals(actionId, StringComparison.OrdinalIgnoreCase);
        });

        if (action != null)
            return action;

        Console.WriteLine($"Action '{actionId}' not found.");
        return CreateDefaultAction(actionId, "DefaultLocation", "DefaultLocationSpot");
    }

    private ActionDefinition CreateDefaultAction(string actionName, string locationName, string locationSpotId)
    {
        ActionDefinition action = new ActionDefinition(actionName, actionName, locationSpotId)
        {
            Description = "Description",
        };
        return action;
    }

    public Contract GetOpportunity(string contractId)
    {
        Contract contract = _worldState.Contracts.FirstOrDefault(a =>
        {
            return a.Id.Equals(contractId, StringComparison.OrdinalIgnoreCase);
        });

        if (contract != null)
            return contract;

        Console.WriteLine($"Opportunity '{contractId}' not found.");
        return CreateDefaultOpportunity(contractId, "DefaultLocation", "DefaultLocationSpot");
    }

    private Contract CreateDefaultOpportunity(object contractId, string location, string locationSpot)
    {
        Contract action = new Contract()
        {
            Description = "Description",
        };
        return action;
    }
}
