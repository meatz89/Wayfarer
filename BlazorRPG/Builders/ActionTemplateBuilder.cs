

public class ActionTemplateBuilder
{
    private string actionId;
    private string name;
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
    private string movesToLocationSpot;

    public ActionRepository ActionRepository { get; }

    public ActionTemplateBuilder WithId(string id)
    {
        this.actionId = id;
        return this;
    }

    public ActionTemplateBuilder WithName(string name)
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

    public ActionTemplateBuilder RequiresFood(int amount)
    {
        if (amount < 0) return this;

        requirements.Add(new FoodRequirement(amount));
        costs.Add(new FoodOutcome(-amount));
        return this;
    }

    public ActionTemplateBuilder RequiresMedicinalHerbs(int amount)
    {
        if (amount < 0) return this;

        requirements.Add(new MedicinalHerbsRequirement(amount));
        costs.Add(new MedicinalHerbOutcome(-amount));
        return this;
    }

    internal ActionTemplateBuilder AdvancesTime(int timeCost)
    {
        costs.Add(new TimeOutcome(-timeCost));
        return this;
    }

    internal ActionTemplateBuilder RestoresConcentration(int amount)
    {
        rewards.Add(new ConcentrationOutcome(amount));
        return this;
    }

    internal ActionTemplateBuilder RestoresConfidence(int amount)
    {
        rewards.Add(new ConfidenceOutcome(amount));
        return this;
    }

    internal ActionTemplateBuilder RestoresHealth(int amount)
    {
        rewards.Add(new HealthOutcome(amount));
        return this;
    }

    internal ActionTemplateBuilder RestoresEnergy(int amount)
    {
        rewards.Add(new EnergyOutcome(amount));
        return this;
    }

    internal ActionTemplateBuilder MovesToLocationSpot(LocationNames deepForest, string locationSpot)
    {
        this.movesToLocationSpot = locationSpot;
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
            ActionId = actionId,
            Name = name,
            Goal = goal,
            Complication = complication,
            BasicActionType = actionType,
            ActionType = IsEncounterAction ? ActionTypes.Encounter : ActionTypes.Basic,
            EncounterId = encounterTemplateName,
            IsRepeatable = isRepeatable,
            LocationSpotTarget = movesToLocationSpot
        };
    }

}