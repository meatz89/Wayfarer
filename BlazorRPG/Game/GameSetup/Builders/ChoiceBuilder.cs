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

    public ChoiceBuilder ExpendsEnergy(EnergyTypes energy, int count)
    {
        requirements.Add(new EnergyRequirement(energy, count));
        costs.Add(new EnergyOutcome(energy, count));
        return this;
    }

    public ChoiceBuilder RequiresSkill(SkillTypes type, int count)
    {
        requirements.Add(new SkillLevelRequirement(type, count));
        return this;
    }

    public ChoiceBuilder WithFoodOutcome(int count)
    {
        rewards.Add(new ResourceOutcome(ResourceTypes.Food, count));
        return this;
    }

    public ChoiceBuilder WithMoneyOutcome(int count)
    {
        rewards.Add(new CoinsOutcome(count));
        return this;
    }

    public ChoiceBuilder WithHealthOutcome(int count)
    {
        rewards.Add(new HealthOutcome(count));
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

