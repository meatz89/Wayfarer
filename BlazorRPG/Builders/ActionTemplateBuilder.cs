public class ActionTemplateBuilder
{
    private string name;
    private string description;
    private BasicActionTypes actionType;

    public List<Requirement> requirements = new();
    public List<Outcome> energy = new();
    public List<Outcome> costs = new();
    public List<Outcome> rewards = new();

    public bool IsEncounterAction = false;

    public ActionTemplateBuilder WithName(string name)
    {
        this.name = name;
        return this;
    }

    public ActionTemplateBuilder WithDescription(string description)
    {
        this.description = description;
        return this;
    }

    public ActionTemplateBuilder WithActionType(BasicActionTypes actionType)
    {
        this.actionType = actionType;
        return this;
    }

    public ActionTemplateBuilder StartsEncounter()
    {
        this.IsEncounterAction = true;
        return this;
    }

    public ActionTemplateBuilder ExpendsHealth(int cost)
    {
        if (cost < 0) return this;

        requirements.Add(new HealthRequirement(cost));
        costs.Add(new HealthOutcome(cost));
        return this;
    }

    public ActionTemplateBuilder ExpendsEnergy(int energyCost, EnergyTypes energyType)
    {
        if (energyCost < 0 || energyType == EnergyTypes.None) return this;

        requirements.Add(new EnergyRequirement(energyType, energyCost));
        energy.Add(new EnergyOutcome(energyType, -energyCost));
        return this;
    }

    public ActionTemplateBuilder ExpendsCoins(int cost)
    {
        if (cost < 0) return this;

        requirements.Add(new CoinsRequirement(cost));
        costs.Add(new CoinsOutcome(-cost));
        return this;
    }

    public ActionTemplateBuilder ExpendsFood(int cost)
    {
        if (cost < 0) return this;

        requirements.Add(new ResourceRequirement(ItemTypes.Food, cost));
        costs.Add(new ResourceOutcome(ItemTypes.Food, -cost));
        return this;
    }

    public ActionTemplateBuilder ExpendsItem(ItemTypes item, int cost)
    {
        if (cost < 0) return this;

        requirements.Add(new ResourceRequirement(item, cost));
        costs.Add(new ResourceOutcome(item, -cost));
        return this;
    }

    public ActionTemplateBuilder RewardsItem(ItemTypes resourceType, int count)
    {
        rewards.Add(new ResourceOutcome(resourceType, count));
        return this;
    }

    public ActionTemplateBuilder RewardsCoins(int count)
    {
        rewards.Add(new CoinsOutcome(count));
        return this;
    }

    public ActionTemplateBuilder RewardsFood(int count)
    {
        rewards.Add(new ResourceOutcome(ItemTypes.Food, count));
        return this;
    }

    public ActionTemplateBuilder RewardsTrust(int count, CharacterNames characterNames)
    {
        return this;
    }

    public ActionTemplateBuilder RewardsHealth(int count)
    {
        rewards.Add(new HealthOutcome(count));
        return this;
    }

    public ActionTemplateBuilder RewardsEnergy(int count, EnergyTypes energyType)
    {
        rewards.Add(new EnergyOutcome(energyType, count));
        return this;
    }

    public ActionTemplateBuilder RewardsReputation(int count)
    {
        rewards.Add(new ReputationOutcome(count));
        return this;
    }

    public ActionTemplateBuilder UnlocksAchievement(AchievementTypes achievementType)
    {
        rewards.Add(new AchievementOutcome(achievementType));
        return this;
    }

    public ActionTemplate Build()
    {
        // Add validation to ensure required properties are set
        if (string.IsNullOrEmpty(name))
        {
            throw new InvalidOperationException("ActionTemplate must have a name.");
        }
        return new ActionTemplate(
            name,
            description,
            actionType,
            IsEncounterAction,
            requirements,
            energy,
            costs,
            rewards
        );
    }
}
