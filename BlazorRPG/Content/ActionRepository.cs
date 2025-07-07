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

    public List<ContractDefinition> GetOpportunitiesForSpot(string spotId)
    {
        // First, find the location this spot belongs to
        LocationSpot spot = _worldState.locationSpots.FirstOrDefault(s => s.SpotID == spotId);
        if (spot == null)
        {
            return new List<ContractDefinition>();
        }

        string locationId = spot.LocationId;
        List<ContractDefinition> Opportunities = new List<ContractDefinition>();

        foreach (ContractDefinition contract in _worldState.Opportunities)
        {
            // For ACCUMULATIVE Opportunities, they're available at any spot within their location
            if (contract.Type == ContractTypes.Accumulative &&
                contract.InitialLocationId == locationId)
            {
                Opportunities.Add(contract);
            }
            // For SEQUENTIAL Opportunities, check the current step's location
            else if (contract.Type == ContractTypes.Sequential &&
                     contract.InitialStep != null &&
                     contract.InitialStep.LocationId == locationId)
            {
                // If the step specifies a spot, check if it matches
                // If not specified, it's available at any spot in the location
                Opportunities.Add(contract);
            }
        }

        return Opportunities;
    }

    public ContractDefinition GetOpportunity(string contractId)
    {
        ContractDefinition contract = _worldState.Opportunities.FirstOrDefault(a =>
        {
            return a.Id.Equals(contractId, StringComparison.OrdinalIgnoreCase);
        });

        if (contract != null)
            return contract;

        Console.WriteLine($"Opportunity '{contractId}' not found.");
        return CreateDefaultOpportunity(contractId, "DefaultLocation", "DefaultLocationSpot");
    }

    private ContractDefinition CreateDefaultOpportunity(object contractId, string location, string locationSpot)
    {
        ContractDefinition action = new ContractDefinition()
        {
            Description = "Description",
        };
        return action;
    }
}