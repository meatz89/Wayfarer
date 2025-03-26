public class ActionTemplate
{
    public string Name { get; set; }
    public BasicActionTypes ActionType { get; set; }
    public bool IsEncounterAction { get; }
    public List<Requirement> Requirements { get; }
    public List<Outcome> Energy { get; }
    public List<Outcome> Costs { get; }
    public List<Outcome> Rewards { get; }

    public string Goal { get; set; }
    public string Complication { get; set; }
    public EncounterTemplate EncounterTemplate { get; set; }

    public ActionTemplate(
        string actionName,
        string goal,
        string complication,
        BasicActionTypes actionType,
        bool isEncounterAction,
        EncounterTemplate encounterTemplate,
        List<Requirement> requirements,
        List<Outcome> energy,
        List<Outcome> costs,
        List<Outcome> rewards)
    {
        Name = actionName;
        Goal = goal;
        Complication = complication;
        ActionType = actionType;
        IsEncounterAction = isEncounterAction;
        EncounterTemplate = encounterTemplate;
        Requirements = requirements;
        Energy = energy;
        Costs = costs;
        Rewards = rewards;
    }
}