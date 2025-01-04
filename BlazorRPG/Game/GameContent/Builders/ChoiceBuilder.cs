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
    private int onlyWhenMomentumAbove;
    private int onlyWhenAdvantageAbove;
    private int onlyWhenUnderstandingAbove;
    private int onlyWhenTensionAbove;

    public int onlyWhenConnectionAbove { get; private set; }

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

    public ChoiceBuilder WithMomentumChange(int momentum)
    {
        this.narrativeStateChanges.Momentum = momentum;
        return this;
    }

    public ChoiceBuilder WithAdvantageChange(int advantage)
    {
        this.narrativeStateChanges.Advantage = advantage;
        return this;
    }

    public ChoiceBuilder WithUnderstandingChange(int understanding)
    {
        this.narrativeStateChanges.Understanding = understanding;
        return this;
    }

    public ChoiceBuilder WithConnectionChange(int connection)
    {
        this.narrativeStateChanges.Connection = connection;
        return this;
    }

    public ChoiceBuilder WithTensionChange(int tension)
    {
        this.narrativeStateChanges.Tension = tension;
        return this;
    }

    public ChoiceBuilder WhenMomentumAbove(int momentum)
    {
        this.onlyWhenMomentumAbove = momentum;
        return this;
    }

    public ChoiceBuilder WhenAdvantageAbove(int advantage)
    {
        this.onlyWhenAdvantageAbove = advantage;
        return this;
    }

    public ChoiceBuilder WhenUnderstandingAbove(int understanding)
    {
        this.onlyWhenUnderstandingAbove = understanding;
        return this;
    }

    public ChoiceBuilder WhenConnectionAbove(int connection)
    {
        this.onlyWhenConnectionAbove = connection;
        return this;
    }

    public ChoiceBuilder WhenTensionAbove(int tension)
    {
        this.onlyWhenTensionAbove = tension;
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


