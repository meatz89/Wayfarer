
public class BasicActionDefinitionBuilder
{
    private BasicActionTypes actionType;
    private string description;
    private List<TimeWindows> timeSlots = new();
    public List<Requirement> requirements = new();
    public List<Outcome> costs = new();
    public List<Outcome> rewards = new();
    public int hoursPassed = 1;

    public BasicActionDefinitionBuilder ForAction(BasicActionTypes actionType)
    {
        this.actionType = actionType;
        return this;
    }


    public BasicActionDefinitionBuilder WithDescription(string description)
    {
        this.description = description;
        return this;
    }

    public BasicActionDefinitionBuilder WithTimeInvestment(int hoursPassed)
    {
        this.hoursPassed = 0;
        return this;
    }


    public BasicActionDefinitionBuilder RequiresInventorySlots(int slots)
    {
        this.requirements.Add(new InventorySlotsRequirement(slots));
        return this;
    }

    public BasicActionDefinitionBuilder AddTimeSlot(TimeWindows timeSlot)
    {
        this.timeSlots.Add(timeSlot);
        return this;
    }

    public BasicActionDefinitionBuilder ExpendsHealth(int cost)
    {
        requirements.Add(new HealthRequirement(cost));
        costs.Add(new HealthOutcome(cost));
        return this;
    }

    public BasicActionDefinitionBuilder ExpendsEnergy(int energyCost, EnergyTypes energyType)
    {
        requirements.Add(new EnergyRequirement(energyType, energyCost));
        costs.Add(new EnergyOutcome(energyType, -energyCost));
        return this;
    }

    public BasicActionDefinitionBuilder ExpendsCoins(int cost)
    {
        requirements.Add(new CoinsRequirement(cost));
        costs.Add(new CoinsOutcome(-cost));
        return this;
    }

    public BasicActionDefinitionBuilder ExpendsFood(int cost)
    {
        requirements.Add(new ResourceRequirement(ResourceTypes.Food, cost));    
        costs.Add(new ResourceOutcome(ResourceTypes.Food, -cost));
        return this;
    }

    public BasicActionDefinitionBuilder ExpendsItem(ResourceTypes item, int count)
    {
        requirements.Add(new ResourceRequirement(item, count));
        costs.Add(new ResourceOutcome(item, -count));
        return this;
    }

    public BasicActionDefinitionBuilder RewardsResource(ResourceTypes resourceType, int amount)
    {
        rewards.Add(new ResourceOutcome(resourceType, amount));
        return this;
    }

    public BasicActionDefinitionBuilder RewardsCoins(int amount)
    {
        rewards.Add(new CoinsOutcome(amount));
        return this;
    }

    public BasicActionDefinitionBuilder RewardsFood(int amount)
    {
        rewards.Add(new ResourceOutcome(ResourceTypes.Food, amount));
        return this;
    }

    public BasicActionDefinitionBuilder RewardsTrust(int amount, CharacterNames characterNames)
    {
        return this;
    }

    public BasicActionDefinitionBuilder RewardsHealth(int amount)
    {
        rewards.Add(new HealthOutcome(amount));
        return this;
    }

    public BasicActionDefinitionBuilder RewardsEnergy(int amount, EnergyTypes energyType)
    {
        rewards.Add(new EnergyOutcome(energyType, amount));
        return this;
    }

    public BasicActionDefinitionBuilder RewardsReputation(ReputationTypes reputationType, int amount)
    {
        rewards.Add(new ReputationOutcome(reputationType, amount));
        return this;
    }

    public BasicActionDefinitionBuilder UnlocksAchievement(AchievementTypes achievementType)
    {
        rewards.Add(new AchievementOutcome(achievementType));
        return this;
    }

    public BasicActionDefinitionBuilder EndsDay()
    {
        costs.Add(new DayChangeOutcome());
        return this;
    }

    public BasicAction Build()
    {
        return new BasicAction
        {
            ActionType = actionType,
            Description = description,
            TimeSlots = timeSlots,
            Requirements = requirements,
            Costs = costs,
            Rewards = rewards,
            TimeInvestment = hoursPassed
        };
    }

}
