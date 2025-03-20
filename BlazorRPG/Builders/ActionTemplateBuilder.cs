public class ActionTemplateBuilder
{
    private ActionNames name;
    private BasicActionTypes actionType;
    private string goal;
    private string complication;

    public List<Requirement> requirements = new();
    public List<Outcome> energy = new();
    public List<Outcome> costs = new();
    public List<Outcome> rewards = new();

    public bool IsEncounterAction = true;
    public EncounterTemplate encounterTemplate;

    public ActionTemplateBuilder WithName(ActionNames name)
    {
        this.name = name;
        return this;
    }

    public ActionTemplateBuilder WithGoal(string goal)
    {
        this.goal = goal;
        return this;
    }

    public ActionTemplateBuilder WithComplication(string complication)
    {
        this.complication = complication;
        return this;
    }

    public ActionTemplateBuilder WithActionType(BasicActionTypes actionType)
    {
        this.actionType = actionType;
        return this;
    }

    public ActionTemplateBuilder StartsEncounter(EncounterTemplate encounterTemplate)
    {
        this.IsEncounterAction = true;
        this.encounterTemplate = encounterTemplate;
        return this;
    }

    public ActionTemplateBuilder ExpendsCoins(int cost)
    {
        if (cost < 0) return this;

        requirements.Add(new CoinsRequirement(cost));
        costs.Add(new CoinsOutcome(-cost));
        return this;
    }

    public ActionTemplate Build()
    {
        return new ActionTemplate(
            name,
            goal,
            complication,
            actionType,
            IsEncounterAction,
            encounterTemplate,
            requirements,
            energy,
            costs,
            rewards
        );
    }
}
