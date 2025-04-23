/// <summary>
/// Repository for action definitions, backed by the ContentRegistry.
/// </summary>
public class ActionRepository
{
    private readonly ContentRegistry _contentRegistry;

    public ActionRepository(ContentRegistry contentRegistry)
    {
        _contentRegistry = contentRegistry;
    }

    public ActionDefinition GetAction(string actionId)
    {
        if (_contentRegistry.TryResolve<ActionDefinition>(actionId, out ActionDefinition? action))
            return action;

        Console.WriteLine($"Action '{actionId}' not found in contentRegistry.");

        ActionDefinition actionDefinition = GetDefaultActionDefinition(actionId, "DefaultLocation", "DefaultLocationSpot");
        return actionDefinition;
    }

    public void RegisterAction(ActionDefinition action)
    {
        if (!_contentRegistry.Register<ActionDefinition>(action.Id, action))
            throw new InvalidOperationException($"Duplicate action ID '{action.Id}'.");
    }

    private ActionDefinition GetDefaultActionDefinition(
        string actionName,
        string locationSpotName,
        string locationName)
    {
        ActionDefinition actionDefinition = new(actionName, actionName, 1, 50, EncounterTypes.Exploration, true)
        {
            Goal = "Goal",
            Complication = "Complication",
            Description = "Description",
            LocationName = locationName,
            LocationSpotName = locationSpotName
        };
        return actionDefinition;
    }
}
