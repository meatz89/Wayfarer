
public class ActionRepository
{
    private Dictionary<string, ActionTemplate> _actionTemplates = new Dictionary<string, ActionTemplate>();
    private Dictionary<string, EncounterTemplate> _encounterTemplates = new Dictionary<string, EncounterTemplate>();

    public ActionRepository()
    {
        // Initialize with existing action templates
        foreach (ActionTemplate template in ActionContent.GetAllTemplates())
        {
            _actionTemplates[template.Name] = template;
        }

        // Add standard encounter templates
        _encounterTemplates["VillageSquareEncounter"] = EncounterContent.VillageSquareEncounter;
        _encounterTemplates["MerchantEncounter"] = EncounterContent.MerchantEncounter;
        _encounterTemplates["BanditEncounter"] = EncounterContent.BanditEncounter;
        _encounterTemplates["SecretMeetingEncounter"] = EncounterContent.SecretMeetingEncounter;
        _encounterTemplates["ShadyDealEncounter"] = EncounterContent.ShadyDealEncounter;
        _encounterTemplates["InnRoomEncounter"] = EncounterContent.InnRoomEncounter;
        _encounterTemplates["QuestBoardEncounter"] = EncounterContent.QuestBoardEncounter;
        // Add other encounter templates as needed
    }

    public ActionTemplate GetOrCreateAction(
        string actionName,
        string goal,
        string complication,
        BasicActionTypes actionType,
        EncounterTemplate encounterTemplate,
        int coinCost = 0)
    {
        // If action doesn't exist, create and save it
        if (!_actionTemplates.TryGetValue(actionName, out ActionTemplate? existingTemplate))
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
            _actionTemplates[actionName] = newTemplate;
            return newTemplate;
        }

        return existingTemplate;
    }

    public void RegisterEncounterTemplate(string name, EncounterTemplate template)
    {
        _encounterTemplates[name] = template;
    }

    public EncounterTemplate GetEncounterTemplate(string name)
    {
        if (_encounterTemplates.TryGetValue(name, out EncounterTemplate? template))
        {
            return template;
        }

        // Return a default template if not found
        return EncounterContent.VillageSquareEncounter;
    }

    public bool TryGetActionTemplate(string actionName, out ActionTemplate template)
    {
        return _actionTemplates.TryGetValue(actionName, out template);
    }
}