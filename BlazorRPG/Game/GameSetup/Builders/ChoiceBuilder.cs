public class ChoiceBuilder
{
    private int index;
    private string description;
    private List<Requirement> requirements = new();
    private List<Outcome> costs = new();
    private List<Outcome> rewards = new();

    public ChoiceBuilder WithIndex(int index)
    {
        this.index = index;
        return this;
    }

    public ChoiceBuilder WithDescription(string description)
    {
        this.description = description;
        return this;
    }

    public ChoiceBuilder ExpendsEnergy(EnergyTypes energy, int amount)
    {
        requirements.Add(new EnergyRequirement(energy, amount));
        costs.Add(new EnergyOutcome(energy, amount));
        return this;
    }

    public ChoiceBuilder RequiresSkill(SkillTypes type, int amount)
    {
        requirements.Add(new SkillLevelRequirement(type, amount));
        return this;
    }

    public ChoiceBuilder WithFoodOutcome(int amount)
    {
        rewards.Add(new ResourceOutcome(ResourceTypes.Food, amount));
        return this;
    }

    public ChoiceBuilder WithMoneyOutcome(int amount)
    {
        rewards.Add(new CoinsOutcome(amount));
        return this;
    }

    public ChoiceBuilder WithHealthOutcome(int amount)
    {
        rewards.Add(new HealthOutcome(amount));
        return this;
    }

    public Choice Build()
    {
        return new Choice
        {
            Index = index,
            Description = description,
            Requirements = requirements,
            Costs = costs,
            Rewards = rewards
        };
    }
}

