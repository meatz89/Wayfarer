public class ChoiceBuilder
{
    private int index;
    private string description;
    private List<Requirement> requirements = new();
    private List<Outcome> costs = new();
    private List<Outcome> rewards = new();
    private string encounter;
    private EncounterStateValues encounterStateChanges = EncounterStateValues.InitialState;
    private List<ValueChange> standardValueChanges = new();
    private Dictionary<string, int> valueModifiers = new();

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

    public ChoiceBuilder WithValueModifier(string name, int value)
    {
        valueModifiers.Add(name, value);
        return this;
    }

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

    public ChoiceBuilder WithHealthCost(int count)
    {
        costs.Add(new HealthOutcome(-count)); // Negative for cost
        return this;
    }

    public ChoiceBuilder WithHealthReward(int count)
    {
        rewards.Add(new HealthOutcome(count));
        return this;
    }

    public ChoiceBuilder WithCoinsCost(int count)
    {
        costs.Add(new CoinsOutcome(-count)); // Negative for cost
        return this;
    }

    public ChoiceBuilder WithCoinsReward(int count)
    {
        rewards.Add(new CoinsOutcome(count));
        return this;
    }

    public ChoiceBuilder WithResourceCost(ResourceTypes type, int count)
    {
        costs.Add(new ResourceOutcome(type, -count)); // Negative for cost
        return this;
    }

    public ChoiceBuilder WithResourceReward(ResourceTypes type, int count)
    {
        rewards.Add(new ResourceOutcome(type, count));
        return this;
    }

    public ChoiceBuilder WithSkillReward(SkillTypes type, int count)
    {
        rewards.Add(new SkillLevelOutcome(type, count));
        return this;
    }

    public ChoiceBuilder WithReputationReward(ReputationTypes type, int count)
    {
        rewards.Add(new ReputationOutcome(type, count));
        return this;
    }

    public ChoiceBuilder WithOutcomeChange(int outcome)
    {
        encounterStateChanges.Outcome = outcome;
        return this;
    }

    public ChoiceBuilder WithInsightChange(int insight)
    {
        encounterStateChanges.Insight = insight;
        return this;
    }

    public ChoiceBuilder WithResonanceChange(int resonance)
    {
        encounterStateChanges.Resonance = resonance;
        return this;
    }

    public ChoiceBuilder WithPressureChange(int pressure)
    {
        encounterStateChanges.Pressure = pressure;
        return this;
    }

    public ChoiceBuilder WithValueChange(ValueTypes valueType, int change)
    {
        standardValueChanges.Add(new ValueChange(valueType, change));
        return this;
    }

    public ChoiceBuilder WithValueChanges(List<ValueChange> valueChanges)
    {
        standardValueChanges.AddRange(valueChanges);
        return this;
    }

    // Updated to be clearer
    public ChoiceBuilder WithRequirements(List<Requirement> newRequirements)
    {
        requirements.AddRange(newRequirements);
        return this;
    }

    public ChoiceBuilder WithCosts(List<Outcome> newCosts)
    {
        costs.AddRange(newCosts);
        return this;
    }

    public ChoiceBuilder WithRewards(List<Outcome> newRewards)
    {
        rewards.AddRange(newRewards);
        return this;
    }

    public ChoiceBuilder AddRequirement(Requirement requirement)
    {
        requirements.Add(requirement);
        return this;
    }

    public ChoiceBuilder AddCost(Outcome cost)
    {
        costs.Add(cost);
        return this;
    }

    public ChoiceBuilder AddReward(Outcome reward)
    {
        rewards.Add(reward);
        return this;
    }

    public EncounterChoice Build()
    {
        return new EncounterChoice
        {
            Index = index,
            Description = description,
            Encounter = encounter,
            ChoiceRequirements = requirements,
            PermanentCosts = costs,
            PermanentRewards = rewards,
            EncounterValueChanges = standardValueChanges,
            ValueModifiers = valueModifiers,
        };
    }
}