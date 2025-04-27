/// <summary>
/// Repository for action definitions, backed by the GameObjectRegistry.
/// </summary>
public class ActionRepository
{
    private readonly GameContentRegistry _registry;

    public ActionRepository(GameContentRegistry registry)
    {
        _registry = registry;
    }

    /// <summary>
    /// Get an action by ID.
    /// </summary>
    public ActionDefinition GetAction(string actionId)
    {
        if (_registry.TryGetAction(actionId, out ActionDefinition action))
            return action;

        Console.WriteLine($"Action '{actionId}' not found in registry.");

        ActionDefinition defaultAction = CreateDefaultAction(
            actionId,
            "DefaultLocation",
            "DefaultLocationSpot");

        return defaultAction;
    }

    /// <summary>
    /// Register a new action.
    /// </summary>
    public void RegisterAction(ActionDefinition action)
    {
        if (!_registry.RegisterAction(action.Id, action))
            throw new InvalidOperationException($"Duplicate action ID '{action.Id}'.");
    }

    /// <summary>
    /// Get all registered actions.
    /// </summary>
    public List<ActionDefinition> GetAllActions()
    {
        return _registry.GetAllActions();
    }

    /// <summary>
    /// Create a default action when one is not found.
    /// </summary>
    private ActionDefinition CreateDefaultAction(
        string actionName,
        string locationName,
        string locationSpotName)
    {
        ActionDefinition action = new ActionDefinition(actionName, actionName)
        {
            Goal = "Goal",
            Complication = "Complication",
            Description = "Description",
        };
        return action;
    }
}