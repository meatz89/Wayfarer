public class ActionRepository
{
    private readonly WorldState _worldState;

    public ActionRepository(GameState gameState)
    {
        _worldState = gameState.WorldState;
    }

    public void AddAction(ActionDefinition action)
    {
        if (_worldState.actions.Any(a =>
        {
            return a.Id.Equals(action.Id, StringComparison.OrdinalIgnoreCase);
        }))
            throw new InvalidOperationException($"Action '{action.Id}' already exists.");

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

    internal List<CommissionDefinition> GetCommissionsForSpot(string spotId)
    {
        // First, find the location this spot belongs to
        LocationSpot spot = _worldState.locationSpots.FirstOrDefault(s => s.Id == spotId);
        if (spot == null)
        {
            return new List<CommissionDefinition>();
        }

        string locationId = spot.LocationId;
        List<CommissionDefinition> commissions = new List<CommissionDefinition>();

        foreach (CommissionDefinition commission in _worldState.commissions)
        {
            // For ACCUMULATIVE commissions, they're available at any spot within their location
            if (commission.Type == CommissionTypes.Accumulative &&
                commission.InitialLocationId == locationId)
            {
                commissions.Add(commission);
            }
            // For SEQUENTIAL commissions, check the current step's location
            else if (commission.Type == CommissionTypes.Sequential &&
                     commission.InitialStep != null &&
                     commission.InitialStep.LocationId == locationId)
            {
                // If the step specifies a spot, check if it matches
                // If not specified, it's available at any spot in the location
                commissions.Add(commission);
            }
        }

        return commissions;
    }

    internal CommissionDefinition GetCommission(string commissionId)
    {
        CommissionDefinition commission = _worldState.commissions.FirstOrDefault(a =>
        {
            return a.Id.Equals(commissionId, StringComparison.OrdinalIgnoreCase);
        });

        if (commission != null)
            return commission;

        Console.WriteLine($"Commission '{commissionId}' not found.");
        return CreateDefaultCommission(commissionId, "DefaultLocation", "DefaultLocationSpot");
    }

    private CommissionDefinition CreateDefaultCommission(object commissionId, string location, string locationSpot)
    {
        CommissionDefinition action = new CommissionDefinition()
        {
            Description = "Description",
        };
        return action;
    }
}