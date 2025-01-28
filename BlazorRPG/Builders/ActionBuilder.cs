public class ActionBuilder
{
    private BasicActionTypes actionType;
    private string description;
    private List<TimeWindows> timeWindows = new();
    public List<Requirement> requirements = new();
    public List<Outcome> energyCosts = new();
    public List<Outcome> costs = new();
    public List<Outcome> rewards = new();
    public int hoursPassed = 1;

    public ActionBuilder ForAction(BasicActionTypes actionType)
    {
        this.actionType = actionType;
        return this;
    }

    public ActionBuilder WithDescription(string description)
    {
        this.description = description;
        return this;
    }

    public ActionBuilder WithTimeInvestment(int hoursPassed)
    {
        this.hoursPassed = hoursPassed;
        return this;
    }

    public ActionBuilder RequiresInventorySlots(int slots)
    {
        this.requirements.Add(new InventorySlotsRequirement(slots));
        return this;
    }

    public ActionBuilder AddTimeWindow(TimeWindows timeWindow)
    {
        this.timeWindows.Add(timeWindow);
        return this;
    }

    public ActionBuilder ExpendsHealth(int cost)
    {
        if (cost < 0) return this;

        requirements.Add(new HealthRequirement(cost));
        costs.Add(new HealthOutcome(cost));
        return this;
    }

    public ActionBuilder ExpendsEnergy(int energyCost, EnergyTypes energyType)
    {
        if (energyCost < 0 || energyType == EnergyTypes.None) return this;

        requirements.Add(new EnergyRequirement(energyType, energyCost));
        energyCosts.Add(new EnergyOutcome(energyType, -energyCost));
        return this;
    }

    public ActionBuilder ExpendsCoins(int cost)
    {
        if (cost < 0) return this;

        requirements.Add(new CoinsRequirement(cost));
        costs.Add(new CoinsOutcome(-cost));
        return this;
    }

    public ActionBuilder ExpendsFood(int cost)
    {
        if (cost < 0) return this;

        requirements.Add(new ResourceRequirement(ResourceTypes.Food, cost));
        costs.Add(new ResourceOutcome(ResourceTypes.Food, -cost));
        return this;
    }

    public ActionBuilder ExpendsItem(ResourceTypes item, int cost)
    {
        if (cost < 0) return this;

        requirements.Add(new ResourceRequirement(item, cost));
        costs.Add(new ResourceOutcome(item, -cost));
        return this;
    }

    public ActionBuilder RewardsResource(ResourceTypes resourceType, int count)
    {
        rewards.Add(new ResourceOutcome(resourceType, count));
        return this;
    }

    public ActionBuilder RewardsCoins(int count)
    {
        rewards.Add(new CoinsOutcome(count));
        return this;
    }

    public ActionBuilder RewardsFood(int count)
    {
        rewards.Add(new ResourceOutcome(ResourceTypes.Food, count));
        return this;
    }

    public ActionBuilder RewardsTrust(int count, CharacterNames characterNames)
    {
        return this;
    }

    public ActionBuilder RewardsHealth(int count)
    {
        rewards.Add(new HealthOutcome(count));
        return this;
    }

    public ActionBuilder RewardsEnergy(int count, EnergyTypes energyType)
    {
        rewards.Add(new EnergyOutcome(energyType, count));
        return this;
    }

    public ActionBuilder RewardsReputation(int count)
    {
        rewards.Add(new ReputationOutcome(count));
        return this;
    }

    public ActionBuilder UnlocksAchievement(AchievementTypes achievementType)
    {
        rewards.Add(new AchievementOutcome(achievementType));
        return this;
    }

    public ActionBuilder EndsDay()
    {
        costs.Add(new DayChangeOutcome());
        return this;
    }

    public ActionImplementation Build()
    {
        return new ActionImplementation
        {
            ActionType = actionType,
            Name = description,
            TimeWindows = timeWindows,
            Requirements = requirements,
            EnergyCosts = energyCosts,
            OutcomeConditions = new()
        };
    }
}
