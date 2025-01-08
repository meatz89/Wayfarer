
using System;

public class ChoiceBuilder
{
    private int index;
    private string description;
    private ChoiceTypes choiceType;
    private List<Requirement> requirements = new();
    private List<Outcome> costs = new();
    private List<Outcome> rewards = new();
    private string encounter;
    private EncounterStateValues encounterStateChanges = EncounterStateValues.NoChange;
    private List<ValueChange> standardValueChanges = new();

    public ChoiceBuilder WithIndex(int index)
    {
        this.index = index;
        return this;
    }

    public ChoiceBuilder WithChoiceType(ChoiceTypes choiceType)
    {
        this.choiceType = choiceType;
        return this;
    }

    public ChoiceBuilder WithName(string description)
    {
        this.description = description;
        return this;
    }

    public ChoiceBuilder WithEncounter(string encounter)
    {
        this.encounter = encounter;
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

    public ChoiceBuilder RequiresHealth(int count)
    {
        requirements.Add(new HealthRequirement(count));
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

    // Outcome methods
    public ChoiceBuilder WithHealthOutcome(int count)
    {
        rewards.Add(new HealthOutcome(count));
        return this;
    }

    public ChoiceBuilder WithMoneyOutcome(int count)
    {
        rewards.Add(new CoinsOutcome(count));
        return this;
    }

    public ChoiceBuilder WithResourceOutcome(ResourceTypes type, int count)
    {
        rewards.Add(new ResourceOutcome(type, count));
        return this;
    }

    public ChoiceBuilder WithSkillOutcome(SkillTypes type, int count)
    {
        rewards.Add(new SkillLevelOutcome(type, count));
        return this;
    }

    public ChoiceBuilder WithReputationOutcome(ReputationTypes type, int count)
    {
        rewards.Add(new ReputationOutcome(type, count));
        return this;
    }

    public ChoiceBuilder WithAdvantageChange(int advantage)
    {
        encounterStateChanges.Advantage = advantage;
        return this;
    }

    public ChoiceBuilder WithUnderstandingChange(int understanding)
    {
        encounterStateChanges.Understanding = understanding;
        return this;
    }

    public ChoiceBuilder WithConnectionChange(int connection)
    {
        encounterStateChanges.Connection = connection;
        return this;
    }

    public ChoiceBuilder WithTensionChange(int tension)
    {
        encounterStateChanges.Tension = tension;
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

    public ChoiceBuilder WithRequirements(List<Requirement> requirements)
    {
        this.requirements = requirements;
        return this;
    }

    public ChoiceBuilder AddRequirement(Requirement requirement)
    {
        this.requirements.Add(requirement);
        return this;
    }

    public ChoiceBuilder AddCost(Outcome cost)
    {
        this.costs.Add(cost);
        return this;
    }

    public ChoiceBuilder AddReward(Outcome reward)
    {
        this.rewards.Add(reward);
        return this;
    }


    public EncounterChoice Build()
    {
        return new EncounterChoice
        {
            Index = index,
            ChoiceType = choiceType,
            Description = description,
            Encounter = encounter,
            ChoiceRequirements = requirements,
            PermanentCosts = costs,
            PermanentRewards = rewards,
            EncounterValueChanges = standardValueChanges
        };
    }

}