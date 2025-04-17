public class ActionTemplateBuilder
{
    private string actionId;
    private string name;
    private string description;
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
    private int timeCost;
    private int difficulty = 1;
    private int encounterChance = 0;
    private BasicActionTypes basicActionType;
    private List<TimeWindows> timeWindows = new List<TimeWindows>();

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

    public ActionTemplateBuilder AvailableDuring(params TimeWindows[] timeWindows)
    {
        foreach (var window in timeWindows)
        {
            this.timeWindows.Add(window);
        }
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

    public ActionTemplateBuilder ExpendsFood(int amount)
    {
        if (amount < 0) return this;

        requirements.Add(new FoodRequirement(amount));
        costs.Add(new FoodOutcome(-amount));
        return this;
    }

    public ActionTemplateBuilder ExpendsMedicinalHerbs(int amount)
    {
        if (amount < 0) return this;

        requirements.Add(new MedicinalHerbsRequirement(amount));
        costs.Add(new MedicinalHerbOutcome(-amount));
        return this;
    }

    public ActionTemplateBuilder TimeCostInHours(int timeCost)
    {
        this.timeCost = timeCost;
        return this;
    }

    public ActionTemplateBuilder RestoresConcentration(int amount)
    {
        rewards.Add(new ConcentrationOutcome(amount));
        return this;
    }

    public ActionTemplateBuilder RestoresConfidence(int amount)
    {
        rewards.Add(new ConfidenceOutcome(amount));
        return this;
    }

    public ActionTemplateBuilder RestoresHealth(int amount)
    {
        rewards.Add(new HealthOutcome(amount));
        return this;
    }

    public ActionTemplateBuilder RestoresEnergy(int amount)
    {
        rewards.Add(new EnergyOutcome(amount));
        return this;
    }

    public ActionTemplateBuilder RewardsHerbs(int amount)
    {
        rewards.Add(new MedicinalHerbOutcome(amount));
        return this;
    }

    public ActionTemplateBuilder RewardsLocationSpotKnowledge(string locationSpot)
    {
        rewards.Add(new LocationSpotKnowledgeOutcome(locationSpot));
        return this;
    }

    public ActionTemplateBuilder MovesToLocationSpot(LocationNames deepForest, string locationSpot)
    {
        this.movesToLocationSpot = locationSpot;
        return this;
    }

    public ActionTemplateBuilder IsRepeatableAction()
    {
        this.isRepeatable = true;
        return this;
    }

    public ActionTemplateBuilder WithDifficulty(int difficulty)
    {
        this.difficulty = difficulty;
        return this;
    }

    public ActionTemplateBuilder WithEncounterChance(int encounterChance)
    {
        this.encounterChance = encounterChance;
        return this;
    }

    public ActionTemplateBuilder WithActionType(BasicActionTypes basicActionType)
    {
        this.basicActionType = basicActionType;
        return this;
    }

    public ActionTemplate Build()
    {
        if (actionId == null) actionId = name.Replace(" ", "");

        return new ActionTemplate(
            actionId,
            name,
            difficulty,
            encounterChance,
            basicActionType,
            isRepeatable)
        {
            Description = description,

            Goal = goal,
            Complication = complication,
            MoveToLocationSpot = movesToLocationSpot,

            TimeWindows = timeWindows,

            TimeCostHours = timeCost,
            Energy = energy,

            Requirements = requirements,
            Costs = costs,
            Rewards = rewards,
        };
    }
}