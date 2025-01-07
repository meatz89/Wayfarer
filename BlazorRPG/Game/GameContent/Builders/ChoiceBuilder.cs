public class ChoiceBuilder
{
    private int index;
    private string description;
    private ChoiceTypes choiceType;
    private Requirement requirement;
    private Outcome cost;
    private Outcome reward;
    private string encounter;
    private EncounterStateValues encounterStateChanges = EncounterStateValues.NoChange;
    private int onlyWhenMomentumAbove;
    private int onlyWhenAdvantageAbove;
    private int onlyWhenUnderstandingAbove;
    private int onlyWhenConnectionAbove;
    private int onlyWhenTensionAbove;

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

    public ChoiceBuilder ExpendsEnergy(EnergyTypes energy, int count)
    {
        requirement = new EnergyRequirement(energy, count);
        cost = new EnergyOutcome(energy, -count); // Negative for consumption
        return this;
    }

    public ChoiceBuilder WithRequirement(Requirement requirement)
    {
        requirement = requirement;
        return this;
    }

    public ChoiceBuilder RequiresSkill(SkillTypes type, int count)
    {
        requirement = new SkillLevelRequirement(type, count);
        return this;
    }

    public ChoiceBuilder RequiresHealth(int count)
    {
        requirement = new HealthRequirement(count);
        return this;
    }

    public ChoiceBuilder RequiresCoins(int count)
    {
        requirement = new CoinsRequirement(count);
        return this;
    }

    public ChoiceBuilder RequiresResource(ResourceTypes type, int count)
    {
        requirement = new ResourceRequirement(type, count);
        return this;
    }

    public ChoiceBuilder RequiresInventorySlots(int count)
    {
        requirement = new InventorySlotsRequirement(count);
        return this;
    }

    // Outcome methods
    public ChoiceBuilder WithHealthOutcome(int count)
    {
        reward = new HealthOutcome(count);
        return this;
    }

    public ChoiceBuilder WithMoneyOutcome(int count)
    {
        reward = new CoinsOutcome(count);
        return this;
    }

    public ChoiceBuilder WithResourceOutcome(ResourceTypes type, int count)
    {
        reward = new ResourceOutcome(type, count);
        return this;
    }

    public ChoiceBuilder WithSkillOutcome(SkillTypes type, int count)
    {
        reward = new SkillLevelOutcome(type, count);
        return this;
    }

    public ChoiceBuilder WithReputationOutcome(ReputationTypes type, int count)
    {
        reward = new ReputationOutcome(type, count);
        return this;
    }

    // Encounter state changes
    public ChoiceBuilder WithMomentumChange(int momentum)
    {
        encounterStateChanges.Momentum = momentum;
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

    // Value thresholds
    public ChoiceBuilder WhenMomentumAbove(int momentum)
    {
        onlyWhenMomentumAbove = momentum;
        return this;
    }

    public ChoiceBuilder WhenAdvantageAbove(int advantage)
    {
        onlyWhenAdvantageAbove = advantage;
        return this;
    }

    public ChoiceBuilder WhenUnderstandingAbove(int understanding)
    {
        onlyWhenUnderstandingAbove = understanding;
        return this;
    }

    public ChoiceBuilder WhenConnectionAbove(int connection)
    {
        onlyWhenConnectionAbove = connection;
        return this;
    }

    public ChoiceBuilder WhenTensionAbove(int tension)
    {
        onlyWhenTensionAbove = tension;
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
            Requirement = requirement,
            Cost = cost,
            Reward = reward,
            EncounterStateChanges = encounterStateChanges,
            // Store thresholds for validation
            ValueThresholds = new EncounterStateValues(
                onlyWhenMomentumAbove,
                onlyWhenAdvantageAbove,
                onlyWhenUnderstandingAbove,
                onlyWhenConnectionAbove,
                onlyWhenTensionAbove
            )
        };
    }
}