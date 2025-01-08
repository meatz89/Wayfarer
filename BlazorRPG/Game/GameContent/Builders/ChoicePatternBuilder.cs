public class ChoicePatternBuilder
{
    private ChoiceTypes choiceType;
    private int baseEnergyCost;
    private EnergyTypes energyType;
    private List<ValueChange> baseValueChanges = new();
    private SkillRequirement skillRequirement;

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

    public ChoicePatternBuilder WithSkillRequirement(SkillTypes skillType, int level)
    {
        this.skillRequirement = new SkillRequirement(skillType, level);
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
        };
    }
}
