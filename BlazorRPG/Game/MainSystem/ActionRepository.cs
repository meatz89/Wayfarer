public class ActionRepository
{
    private List<SpotAction> _actionTemplates = new List<SpotAction>();
    private List<EncounterTemplate> _encounterTemplates = new List<EncounterTemplate>();

    public ActionRepository()
    {
        List<EncounterTemplate> encounters = WorldEncounterContent.GetAllTemplates();
        foreach (EncounterTemplate encounterTemplate in encounters)
        {
            _encounterTemplates.Add(encounterTemplate);
        }

        List<SpotAction> actions = WorldActionContent.GetAllTemplates();
        foreach (SpotAction actionTemplate in actions)
        {
            _actionTemplates.Add(actionTemplate);
        }
    }
    public EncounterTemplate GetEncounterTemplate(string actionId)
    {
        EncounterTemplate? template = _encounterTemplates.FirstOrDefault(x => x.ActionId == actionId);
        if (template != null)
        {
            return template;
        }

        Console.WriteLine($"Encounter Template {actionId} not found. Choosing default template");
        EncounterTemplate defaultTemplate = WorldEncounterContent.GetDefaultTemplate();
        return defaultTemplate;
    }

    public SpotAction GetAction(string actionId)
    {
        SpotAction? existingTemplate = _actionTemplates.FirstOrDefault(x => x.ActionId == actionId);
        return existingTemplate;
    }

    public void RegisterEncounterTemplate(string actionId, EncounterTemplate template)
    {
        template.ActionId = actionId;
        _encounterTemplates.Add(template);
    }

    public string AddActionTemplate(string actionId, SpotAction spotAction)
    {
        spotAction.ActionId = actionId;
        _actionTemplates.Add(spotAction);
        return spotAction.ActionId;
    }
}