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

    public SpotAction GetAction(string actionName)
    {
        SpotAction? existingTemplate = _actionTemplates.FirstOrDefault(x => x.Name == actionName);
        return existingTemplate;
    }


    public string CreateActionTemplate(
        string actionName,
        string goal,
        string complication,
        BasicActionTypes basicActionTypes,
        ActionTypes actionType,
        string encounterTemplate,
        int coinCost = 0)
    {
        ActionTemplateBuilder builder = new ActionTemplateBuilder();
        if (actionType == ActionTypes.Encounter)
        {

            builder
                .WithCustomName(actionName)
                .WithGoal(goal)
                .WithComplication(complication)
                .WithActionType(basicActionTypes)
                .StartsEncounter(encounterTemplate);
        }
        else
        {
            builder
                .WithCustomName(actionName)
                .WithGoal(goal)
                .WithComplication(complication)
                .WithActionType(basicActionTypes)
                .ExpendsCoins(coinCost);
        }

        if (coinCost > 0)
        {
            builder.ExpendsCoins(coinCost);
        }

        SpotAction newTemplate = builder.Build();
        _actionTemplates.Add(newTemplate);
        return newTemplate.Name;
    }

    public void RegisterEncounterTemplate(string name, EncounterTemplate template)
    {
        _encounterTemplates.Add(template);
    }

}