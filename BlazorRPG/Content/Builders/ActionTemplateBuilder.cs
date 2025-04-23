public class ActionTemplateBuilder
{
    private string actionId;
    private string name;
    private string goal;
    private string complication;
    public List<Requirement> requirements = new();
    public List<Outcome> costs = new();
    public List<Outcome> yields = new();
    public bool IsEncounterAction = true;
    public string encounterTemplateName;
    private bool isRepeatable;
    private string movesToLocationSpot;
    private int energyCost = 0;
    private int timeCost = 0;
    private int difficulty = 1;
    private int encounterChance = 0;
    private EncounterTypes basicActionType;
    private List<TimeWindows> timeWindows = new List<TimeWindows>();

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

    public ActionTemplateBuilder AvailableDuring(params TimeWindows[] timeWindows)
    {
        foreach (TimeWindows window in timeWindows)
        {
            this.timeWindows.Add(window);
        }
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

    public ActionTemplateBuilder WithEnergyCost(int energyCost)
    {
        this.energyCost = energyCost;
        return this;
    }

    public ActionTemplateBuilder WithTimeCost(int timeCost)
    {
        this.timeCost = timeCost;
        return this;
    }

    public ActionTemplateBuilder RestoresConcentration(int amount)
    {
        yields.Add(new ConcentrationOutcome(amount));
        return this;
    }

    public ActionTemplateBuilder RestoresConfidence(int amount)
    {
        yields.Add(new ConfidenceOutcome(amount));
        return this;
    }

    public ActionTemplateBuilder RestoresHealth(int amount)
    {
        yields.Add(new HealthOutcome(amount));
        return this;
    }

    public ActionTemplateBuilder RestoresEnergy(int amount)
    {
        yields.Add(new EnergyOutcome(amount));
        return this;
    }

    public ActionTemplateBuilder RewardsHerbs(int amount)
    {
        yields.Add(new MedicinalHerbOutcome(amount));
        return this;
    }

    public ActionTemplateBuilder RewardsLocationSpotKnowledge(string locationSpot)
    {
        yields.Add(new LocationSpotKnowledgeOutcome(locationSpot));
        return this;
    }

    public ActionTemplateBuilder MovesToLocationSpot(string locationName, string locationSpot)
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

    public ActionTemplateBuilder WithEncounterType(EncounterTypes basicActionType)
    {
        this.basicActionType = basicActionType;
        return this;
    }

    public ActionTemplateBuilder AddYield(Action<YieldBuilder> buildActionYield)
    {
        YieldBuilder yieldBuilder = new YieldBuilder();
        buildActionYield(yieldBuilder);
        
        yields.Add(yieldBuilder.Build());
        
        return this;
    }

    public ActionDefinition Build()
    {
        if (actionId == null) actionId = name.Replace(" ", "");

        return new ActionDefinition(
            actionId,
            name,
            difficulty,
            encounterChance,
            basicActionType,
            isRepeatable)
        {
            Description = goal,

            Goal = goal,
            Complication = complication,
            MoveToLocationSpot = movesToLocationSpot,

            AvailableWindows = timeWindows,

            Requirements = requirements,
            Costs = costs,
            Yields = yields,
        };
    }
}