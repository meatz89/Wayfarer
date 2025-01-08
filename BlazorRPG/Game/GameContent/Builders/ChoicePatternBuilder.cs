public class ChoicePatternBuilder
{
    private int baseEnergyCost;
    private EnergyTypes energyType;
    private List<ValueChange> baseValueChanges = new();
    private List<Requirement> requirements = new();
    private List<Outcome> costs = new();
    private List<Outcome> rewards = new();

    public ChoicePatternBuilder WithBaseEnergyCost(int cost, EnergyTypes type)
    {
        this.baseEnergyCost = cost;
        this.energyType = type;
        return this;
    }

    // No changes necessary here, as we are still using BaseValueChanges
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
            BaseValueChanges = baseValueChanges,
            EnergyType = energyType,
            BaseCost = baseEnergyCost,
            Requirements = requirements,
            Costs = costs,
            Rewards = rewards,
        };
    }
}