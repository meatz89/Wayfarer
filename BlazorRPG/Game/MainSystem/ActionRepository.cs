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

        // Return a default template if not found
        return WorldEncounterContent.GetAllTemplates().First();
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

    public string CreateActionTemplate(
        string actionId,
        string name,
        string goal,
        string complication,
        BasicActionTypes basicActionTypes,
        ActionTypes actionType,
        int coinCost = 0)
    {
        ActionTemplateBuilder builder = new ActionTemplateBuilder();
        if (actionType == ActionTypes.Encounter)
        {

            builder
                .WithId(actionId)
                .WithName(name)
                .WithGoal(goal)
                .WithComplication(complication)
                .WithActionType(basicActionTypes)
                .StartsEncounter(actionId);
        }
        else
        {
            builder
                .WithId(actionId)
                .WithName(actionId)
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

        return newTemplate.ActionId;
    }
}