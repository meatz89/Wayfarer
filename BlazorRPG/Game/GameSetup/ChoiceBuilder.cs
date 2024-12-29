public class ChoiceBuilder
{
    private int index;
    private string description;
    private List<IRequirement> requirements = new();
    private List<IOutcome> outcomes = new();

    public ChoiceBuilder WithIndex(int index)
    {
        this.index = index;
        return this;
    }

    public ChoiceBuilder WithDescription(string description)
    {
        this.description = description;
        return this;
    }

    public ChoiceBuilder ExpendsPhysicalEnergy(int amount)
    {
        requirements.Add(new PhysicalEnergyRequirement
        {
            Amount = amount
        });

        outcomes.Add(new PhysicalEnergyOutcome
        {
            Amount = -amount
        });
        return this;
    }

    public ChoiceBuilder ExpendsFocusEnergy(int amount)
    {
        requirements.Add(new FocusEnergyRequirement
        {
            Amount = amount
        });

        outcomes.Add(new FocusEnergyOutcome
        {
            Amount = -amount
        });
        return this;
    }

    public ChoiceBuilder ExpendsSocialEnergy(int amount)
    {
        requirements.Add(new SocialEnergyRequirement
        {
            Amount = amount
        });

        outcomes.Add(new SocialEnergyOutcome
        {
            Amount = -amount
        });
        return this;
    }

    public ChoiceBuilder RequiresSkill(SkillTypes type, int amount)
    {
        requirements.Add(new SkillLevelRequirement
        {
            SkillType = type,
            Amount = amount
        });
        return this;
    }

    public ChoiceBuilder WithMoneyOutcome(int amount)
    {
        outcomes.Add(new CoinsOutcome
        {
            Amount = amount
        });
        return this;
    }

    public ChoiceBuilder WithHealthOutcome(int amount)
    {
        outcomes.Add(new HealthOutcome
        {
            Amount = amount
        });
        return this;
    }

    public Choice Build()
    {
        return new Choice
        {
            Index = index,
            Description = description,
            Requirements = requirements,
            Outcomes = outcomes
        };
    }
}

