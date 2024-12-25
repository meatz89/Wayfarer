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

    public ChoiceBuilder RequiresResource(ResourceTypes type, int amount)
    {
        requirements.Add(new ResourceReq
        {
            ResourceType = type,
            Amount = amount
        });
        return this;
    }

    public ChoiceBuilder RequiresSkill(SkillTypes type, int amount)
    {
        requirements.Add(new SkillReq
        {
            SkillType = type,
            Amount = amount
        });
        return this;
    }

    public ChoiceBuilder WithResourceOutcome(ResourceTypes type, int amount)
    {
        outcomes.Add(new ResourceOutcome
        {
            ResourceType = type,
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
