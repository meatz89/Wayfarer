public class ChoiceTemplateBuilder
{
    private ChoiceArchetypes archetype;
    private ChoiceApproaches approach;
    private int baseEnergyCost;
    private EnergyTypes energyType;
    private List<ValueChange> baseValueChanges = new();
    private List<Requirement> requirements = new();
    private List<Outcome> costs = new();
    private List<Outcome> rewards = new();
    private SkillTypes relevantSkill;

    public ChoiceTemplateBuilder WithArchetype(ChoiceArchetypes archetype)
    {
        this.location property = archetype;
        return this;
    }

    public ChoiceTemplateBuilder WithApproach(ChoiceApproaches approach)
    {
        this.approach = approach;
        return this;
    }

    public ChoiceTemplateBuilder WithBaseEnergyCost(int cost, EnergyTypes type)
    {
        this.baseEnergyCost = cost;
        this.energyType = type;
        return this;
    }

    public ChoiceTemplateBuilder WithBaseValueChanges(Action<ValueChangeBuilder> buildValues)
    {
        ValueChangeBuilder builder = new();
        buildValues(builder);
        this.baseValueChanges = builder.Build();
        return this;
    }

    public ChoiceTemplateBuilder WithRequirement(Requirement requirement)
    {
        this.requirements.Add(requirement);
        return this;
    }

    public ChoiceTemplateBuilder WithCost(Outcome cost)
    {
        this.costs.Add(cost);
        return this;
    }

    public ChoiceTemplateBuilder WithReward(Outcome reward)
    {
        this.rewards.Add(reward);
        return this;
    }

    public ChoiceTemplateBuilder WithSkill(SkillTypes skillType)
    {
        this.relevantSkill = skillType;
        return this;
    }

    public ChoiceTemplate Build()
    {
        return new ChoiceTemplate
        {
            Archetype = archetype,
            Approach = approach,
            BaseValueChanges = baseValueChanges,
            EnergyType = energyType,
            BaseEnergyCost = baseEnergyCost,
            RelevantSkill = relevantSkill,
            Requirements = requirements,
            Costs = costs,
            Rewards = rewards,
        };
    }
}