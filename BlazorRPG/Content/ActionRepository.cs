public class ActionRepository
{
    private readonly WorldState _worldState;

    public ActionRepository(GameState gameState)
    {
        _worldState = gameState.WorldState;
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

    // Create default action implementation remains unchanged
    private ActionDefinition CreateDefaultAction(string actionName, string locationName, string locationSpotId)
    {
        // Implementation remains the same
        ActionDefinition action = new ActionDefinition(actionName, actionName, locationSpotId)
        {
            Description = "Description",
        };
        return action;
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
}