public class ChoiceBuilder
{
    // Basic properties
    private int index;
    private string description;

    private ChoiceArchetypes archetype;
    private ChoiceApproaches approach;

    private SkillTypes choiceRelevantSkill;

    // Base values that will be used by calculator
    private List<ValueChange> baseValueChanges = new();
    private List<Requirement> requirements = new();
    private List<Outcome> baseCosts = new();
    private List<Outcome> baseRewards = new();

    public ChoiceBuilder WithIndex(int index)
    {
        this.index = index;
        return this;
    }

    public ChoiceBuilder WithName(string description)
    {
        this.description = description;
        return this;
    }

    public ChoiceBuilder WithArchetype(ChoiceArchetypes archetype)
    {
        this.archetype = archetype;
        return this;
    }

    public ChoiceBuilder WithApproach(ChoiceApproaches approach)
    {
        this.approach = approach;
        return this;
    }

    public ChoiceBuilder WithRelevantSkill(SkillTypes skillType)
    {
        this.choiceRelevantSkill = skillType;
        return this;
    }

    public ChoiceBuilder WithValueChange(ValueTypes valueType, int change)
    {
        baseValueChanges.Add(new ValueChange(valueType, change));
        return this;
    }

    public ChoiceBuilder WithValueChanges(List<ValueChange> valueChanges)
    {
        baseValueChanges.AddRange(valueChanges);
        return this;
    }

    // Requirements
    public ChoiceBuilder RequiresEnergy(EnergyTypes energy, int count)
    {
        requirements.Add(new EnergyRequirement(energy, count));
        return this;
    }

    public ChoiceBuilder RequiresSkill(SkillTypes type, int count)
    {
        requirements.Add(new SkillLevelRequirement(type, count));
        return this;
    }

    public ChoiceBuilder RequiresCoins(int count)
    {
        requirements.Add(new CoinsRequirement(count));
        return this;
    }

    public ChoiceBuilder RequiresResource(ResourceTypes type, int count)
    {
        requirements.Add(new ResourceRequirement(type, count));
        return this;
    }

    public ChoiceBuilder RequiresInventorySlots(int count)
    {
        requirements.Add(new InventorySlotsRequirement(count));
        return this;
    }

    // Base Costs
    public ChoiceBuilder WithHealthCost(int count)
    {
        baseCosts.Add(new HealthOutcome(-count));
        return this;
    }

    public ChoiceBuilder WithCoinsCost(int count)
    {
        baseCosts.Add(new CoinsOutcome(-count));
        return this;
    }

    public ChoiceBuilder WithResourceCost(ResourceTypes type, int count)
    {
        baseCosts.Add(new ResourceOutcome(type, -count));
        return this;
    }

    // Base Rewards
    public ChoiceBuilder WithHealthReward(int count)
    {
        baseRewards.Add(new HealthOutcome(count));
        return this;
    }

    public ChoiceBuilder WithCoinsReward(int count)
    {
        baseRewards.Add(new CoinsOutcome(count));
        return this;
    }

    public ChoiceBuilder WithResourceReward(ResourceTypes type, int count)
    {
        baseRewards.Add(new ResourceOutcome(type, count));
        return this;
    }

    public ChoiceBuilder WithSkillReward(SkillTypes type, int count)
    {
        baseRewards.Add(new SkillLevelOutcome(type, count));
        return this;
    }

    public ChoiceBuilder WithReputationReward(ReputationTypes type, int count)
    {
        baseRewards.Add(new ReputationOutcome(type, count));
        return this;
    }

    // Collection methods
    public ChoiceBuilder WithRequirements(List<Requirement> newRequirements)
    {
        requirements.AddRange(newRequirements);
        return this;
    }

    public ChoiceBuilder WithBaseCosts(List<Outcome> newCosts)
    {
        baseCosts.AddRange(newCosts);
        return this;
    }

    public ChoiceBuilder WithBaseRewards(List<Outcome> newRewards)
    {
        baseRewards.AddRange(newRewards);
        return this;
    }

    public EncounterChoice Build()
    {
        return new EncounterChoice
        {
            Index = index,
            Description = description,
            Archetype = archetype,
            Approach = approach,
            ChoiceRelevantSkill = choiceRelevantSkill,
            BaseValueChanges = baseValueChanges,
            Requirements = requirements,
            BaseCosts = baseCosts,
            BaseRewards = baseRewards
        };
    }
}