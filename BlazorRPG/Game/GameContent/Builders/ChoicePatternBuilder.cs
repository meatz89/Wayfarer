public class ChoicePatternBuilder
{
    private ChoiceTypes choiceType;
    private int baseEnergyCost;
    private EnergyTypes energyType;
    private List<ValueChange> baseValueChanges = new();
    private List<Requirement> requirements = new();
    private List<Outcome> costs = new();
    private List<Outcome> rewards = new();

    public ChoicePatternBuilder WithType(ChoiceTypes type)
    {
        this.choiceType = type;
        return this;
    }

    public ChoicePatternBuilder WithBaseEnergyCost(int cost, EnergyTypes type)
    {
        this.baseEnergyCost = cost;
        this.energyType = type;
        return this;
    }

    public ChoicePatternBuilder WithBaseValueChanges(Action<ValueChangeBuilder> buildValues)
    {
        ValueChangeBuilder builder = new();
        buildValues(builder);
        this.baseValueChanges = builder.Build();
        return this;
    }

    // New: Methods for adding requirements
    public ChoicePatternBuilder WithRequirement(Requirement requirement)
    {
        this.requirements.Add(requirement);
        return this;
    }

    // New: Methods for adding costs
    public ChoicePatternBuilder WithCost(Outcome cost)
    {
        this.costs.Add(cost);
        return this;
    }

    // New: Methods for adding rewards
    public ChoicePatternBuilder WithReward(Outcome reward)
    {
        this.rewards.Add(reward);
        return this;
    }

    public ChoicePattern Build()
    {
        return new ChoicePattern
        {
            ChoiceType = choiceType,
            BaseValueChanges = baseValueChanges,
            EnergyType = energyType,
            BaseCost = baseEnergyCost,
            Requirements = requirements,
            Costs = costs,
            Rewards = rewards
        };
    }
}