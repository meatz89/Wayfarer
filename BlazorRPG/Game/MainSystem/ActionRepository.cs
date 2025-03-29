
public class ActionRepository
{
    private List<ActionTemplate> _actionTemplates = new List<ActionTemplate>();
    private List<EncounterTemplate> _encounterTemplates = new List<EncounterTemplate>();

    public ActionRepository()
    {
        List<EncounterTemplate> encounters = WorldEncounterContent.GetAllTemplates();
        foreach (EncounterTemplate encounterTemplate in encounters)
        {
            _encounterTemplates.Add(encounterTemplate);
        }

        List<ActionTemplate> actions = WorldActionContent.GetAllTemplates();
        foreach (ActionTemplate actionTemplate in actions)
        {
            _actionTemplates.Add(actionTemplate);
        }
    }

    public ActionTemplate GetOrCreateAction(
        string actionName,
        string goal,
        string complication,
        BasicActionTypes actionType,
        string encounterTemplate,
        int coinCost = 0)
    {
        // If action doesn't exist, create and save it
        ActionTemplate? existingTemplate = _actionTemplates.FirstOrDefault(x => x.Name == actionName);

        if (existingTemplate == null)
        {
            ActionTemplateBuilder builder = new ActionTemplateBuilder()
                .WithCustomName(actionName)
                .WithGoal(goal)
                .WithComplication(complication)
                .WithActionType(actionType)
                .StartsEncounter(encounterTemplate);

            if (coinCost > 0)
            {
                builder.ExpendsCoins(coinCost);
            }

            ActionTemplate newTemplate = builder.Build();
            _actionTemplates.Add(newTemplate);
            return newTemplate;
        }

        return existingTemplate;
    }

    public void RegisterEncounterTemplate(string name, EncounterTemplate template)
    {
        _encounterTemplates.Add(template);
    }

    public EncounterTemplate GetEncounterTemplate(string name)
    {
        EncounterTemplate? template = _encounterTemplates.FirstOrDefault(x => x.Name == name);
        if (template != null)
        {
            return template;
        }

        // Return a default template if not found
        return WorldEncounterContent.GetAllTemplates().First();
    }

    public ActionTemplate GetAction(string actionName)
    {
        ActionTemplate? existingTemplate = _actionTemplates.FirstOrDefault(x => x.Name == actionName);
        return existingTemplate;
    }
}