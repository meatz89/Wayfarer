public class ActionRepository
{
    private WorldState _worldState;

    public ActionRepository(GameWorld gameState)
    {
        _worldState = gameState.WorldState;
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

    public List<OpportunityDefinition> GetOpportunitiesForSpot(string spotId)
    {
        // First, find the location this spot belongs to
        LocationSpot spot = _worldState.locationSpots.FirstOrDefault(s => s.SpotID == spotId);
        if (spot == null)
        {
            return new List<OpportunityDefinition>();
        }

        string locationId = spot.LocationId;
        List<OpportunityDefinition> Opportunities = new List<OpportunityDefinition>();

        foreach (OpportunityDefinition opportunity in _worldState.Opportunities)
        {
            // For ACCUMULATIVE Opportunities, they're available at any spot within their location
            if (opportunity.Type == OpportunityTypes.Accumulative &&
                opportunity.InitialLocationId == locationId)
            {
                Opportunities.Add(opportunity);
            }
            // For SEQUENTIAL Opportunities, check the current step's location
            else if (opportunity.Type == OpportunityTypes.Sequential &&
                     opportunity.InitialStep != null &&
                     opportunity.InitialStep.LocationId == locationId)
            {
                // If the step specifies a spot, check if it matches
                // If not specified, it's available at any spot in the location
                Opportunities.Add(opportunity);
            }
        }

        return Opportunities;
    }

    public OpportunityDefinition GetOpportunity(string opportunityId)
    {
        OpportunityDefinition opportunity = _worldState.Opportunities.FirstOrDefault(a =>
        {
            return a.Id.Equals(opportunityId, StringComparison.OrdinalIgnoreCase);
        });

        if (opportunity != null)
            return opportunity;

        Console.WriteLine($"Opportunity '{opportunityId}' not found.");
        return CreateDefaultOpportunity(opportunityId, "DefaultLocation", "DefaultLocationSpot");
    }

    private OpportunityDefinition CreateDefaultOpportunity(object opportunityId, string location, string locationSpot)
    {
        OpportunityDefinition action = new OpportunityDefinition()
        {
            Description = "Description",
        };
        return action;
    }
}