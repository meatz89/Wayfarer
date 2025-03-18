public class ActionTemplate
{
    public ActionNames Name { get; set; }
    public string Description { get; set; }
    public BasicActionTypes ActionType { get; set; }
    public bool IsEncounterAction { get; }
    public List<Requirement> Requirements { get; }
    public List<Outcome> Energy { get; }
    public List<Outcome> Costs { get; }
    public List<Outcome> Rewards { get; }

    public ActionTemplate(
        ActionNames actionName,
        string description,
        BasicActionTypes actionType,
        bool isEncounterAction,
        List<Requirement> requirements,
        List<Outcome> energy,
        List<Outcome> costs,
        List<Outcome> rewards)
    {
        Name = actionName;
        Description = description;
        ActionType = actionType;
        IsEncounterAction = isEncounterAction;
        Requirements = requirements;
        Energy = energy;
        Costs = costs;
        Rewards = rewards;
    }
}