public class ActionRepository
{
    private readonly GameWorld _gameWorld;

    public ActionRepository(GameWorld gameWorld)
    {
        _gameWorld = gameWorld;
    }

    public void AddAction(ActionDefinition action)
    {
        if (_gameWorld.WorldState.actions.Any(a =>
        {
            return a.Name.Equals(action.Name, StringComparison.OrdinalIgnoreCase);
        }))
            throw new InvalidOperationException($"Action '{action.Name}' already exists.");

        _gameWorld.WorldState.actions.Add(action);
    }

    public List<ActionDefinition> GetAllActions()
    {
        return _gameWorld.WorldState.actions;
    }

    public List<ActionDefinition> GetActionsForSpot(string spotId)
    {
        List<ActionDefinition> actions = new List<ActionDefinition>();
        foreach (ActionDefinition action in _gameWorld.WorldState.actions)
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
        ActionDefinition action = _gameWorld.WorldState.actions.FirstOrDefault(a =>
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


    // Mark encounter as completed
    public void MarkEncounterCompleted(string actionId)
    {
        _gameWorld.WorldState.MarkEncounterCompleted(actionId);
    }
}
