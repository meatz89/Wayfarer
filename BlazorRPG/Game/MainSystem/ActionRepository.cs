
public class ActionRepository
{
    private List<ActionTemplate> _actionTemplates = new List<ActionTemplate>();

    public ActionRepository()
    {
        List<ActionTemplate> actions = WorldActionContent.GetAllTemplates();
        foreach (ActionTemplate actionTemplate in actions)
        {
            _actionTemplates.Add(actionTemplate);
        }
    }
    public ActionTemplate GetAction(string actionId)
    {
        ActionTemplate? existingTemplate = _actionTemplates.FirstOrDefault(x => x.ActionId == actionId);
        return existingTemplate;
    }

    public string AddActionTemplate(string actionId, ActionTemplate spotAction)
    {
        _actionTemplates.Add(spotAction);
        return spotAction.ActionId;
    }


    public EncounterTemplate GetEncounterForAction(ActionTemplate actionTemplate)
    {
        ActionGenerationContext context = new ActionGenerationContext
        {
            ActionId = actionTemplate.ActionId,
            Goal = actionTemplate.Goal,
            Complication = actionTemplate.Complication,
            BasicActionType = actionTemplate.BasicActionType.ToString(),
            SpotName = actionTemplate.LocationSpotName,
            LocationName = actionTemplate.LocationName,
        };

        EncounterTemplate encounterTemplate = WorldEncounterContent.GetDefaultTemplate();

        return encounterTemplate;
    }
}