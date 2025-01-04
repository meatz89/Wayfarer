public class ChoiceBuilder
{
    private int index;
    private string description;
    private ChoiceTypes choiceType;

    private Requirement requirement;
    private Outcome cost;
    private Outcome reward;

    private string narrative;
    private NarrativeState narrativeStateChanges = NarrativeState.NoChange;

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

    public ChoiceBuilder WithNarrative(string narrative)
    {
        this.narrative = narrative;
        return this;
    }

    public ChoiceBuilder ExpendsEnergy(EnergyTypes energy, int count)
    {
        requirement = new EnergyRequirement(energy, count);
        cost = new EnergyOutcome(energy, count);
        return this;
    }

    public ChoiceBuilder RequiresSkill(SkillTypes type, int count)
    {
        requirement = new SkillLevelRequirement(type, count);
        return this;
    }

    public ChoiceBuilder WithMoneyOutcome(int count)
    {
        reward = new CoinsOutcome(count);
        return this;
    }

    public ChoiceBuilder WithMomentumChange(int Momentum)
    {
        this.narrativeStateChanges.Momentum = Momentum;
        return this;
    }

    public ChoiceBuilder WithAdvantageChange(int Advantage)
    {
        this.narrativeStateChanges.Advantage = Advantage;
        return this;
    }

    public ChoiceBuilder WithUnderstandingChange(int Understanding)
    {
        this.narrativeStateChanges.Understanding = Understanding;
        return this;
    }

    public ChoiceBuilder WithConnectionChange(int Connection)
    {
        this.narrativeStateChanges.Connection = Connection;
        return this;
    }

    public ChoiceBuilder WithTensionChange(int Tension)
    {
        this.narrativeStateChanges.Tension = Tension;
        return this;
    }


    public NarrativeChoice Build()
    {
        return new NarrativeChoice
        {
            Index = index,
            ChoiceType = choiceType,
            Description = description,
            Narrative = narrative,
            Requirement = requirement,
            Cost = cost,
            Reward = reward,
            NarrativeStateChanges = narrativeStateChanges
        };
    }
}


