public class ActionRepository
{
    private List<ActionDefinition> _actionTemplates = new List<ActionDefinition>();

    public ActionRepository()
    {
        List<ActionDefinition> actions = WorldActionContent.GetAllTemplates();
        foreach (ActionDefinition actionTemplate in actions)
        {
            _actionTemplates.Add(actionTemplate);
        }
    }
    public ActionDefinition GetAction(string actionId)
    {
        ActionDefinition? existingTemplate = _actionTemplates.FirstOrDefault(x => x.Id == actionId);
        return existingTemplate;
    }

    public string AddActionTemplate(ActionDefinition spotAction)
    {
        _actionTemplates.Add(spotAction);
        return spotAction.Id;
    }


    public EncounterTemplate GetEncounterForAction(ActionDefinition actionTemplate)
    {
        ActionGenerationContext context = new ActionGenerationContext
        {
            ActionId = actionTemplate.Id,
            Goal = actionTemplate.Goal,
            Complication = actionTemplate.Complication,
            BasicActionType = actionTemplate.EncounterType.ToString(),
            SpotName = actionTemplate.LocationSpotName,
            LocationName = actionTemplate.LocationName,
        };

        EncounterTemplate encounterTemplate = WorldEncounterContent.GetDefaultTemplate();

        return encounterTemplate;
    }
}