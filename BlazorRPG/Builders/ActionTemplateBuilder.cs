public class ActionTemplateBuilder
{
    private string customName; // New field for custom names
    private BasicActionTypes actionType;
    private string goal;
    private string complication;
    public List<Requirement> requirements = new();
    public List<Outcome> energy = new();
    public List<Outcome> costs = new();
    public List<Outcome> rewards = new();
    public bool IsEncounterAction = true;
    public string encounterTemplateName;
    private bool isRepeatable;

    public ActionRepository ActionRepository { get; }

    public ActionTemplateBuilder WithName(string name)
    {
        this.customName = name;
        return this;
    }

    // Add a new method for custom string names
    public ActionTemplateBuilder WithCustomName(string name)
    {
        this.customName = name;
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

    public ActionTemplateBuilder StartsEncounter(string encounterTemplate)
    {
        this.IsEncounterAction = true;
        this.encounterTemplateName = encounterTemplate;
        return this;
    }

    public ActionTemplateBuilder ExpendsCoins(int cost)
    {
        if (cost < 0) return this;

        requirements.Add(new CoinsRequirement(cost));
        costs.Add(new CoinsOutcome(-cost));
        return this;
    }

    public ActionTemplateBuilder ExpendsEnergy(int energy)
    {
        if (energy < 0) return this;

        requirements.Add(new EnergyRequirement(energy));
        costs.Add(new EnergyOutcome(-energy));
        return this;
    }

    public ActionTemplateBuilder IsRepeatable()
    {
        this.isRepeatable = true;
        return this;
    }

    public SpotAction Build()
    {
        return new SpotAction()
        {
            Name = customName,
            Goal = goal,
            Complication = complication,
            BasicActionType = actionType,
            ActionType = IsEncounterAction ? ActionTypes.Encounter : ActionTypes.Basic,
            EncounterTemplateName = encounterTemplateName,
            CoinCost = 0,
            IsRepeatable = isRepeatable
        };
    }

}