public class NodeAspectDefinitionBuilder
{
    private string name;
    private string description;
    private bool isDiscovered;
    private SkillTypes skillType;
    private int skillXpGain;
    public List<YieldDefinition> Yields = new();

    public NodeAspectDefinitionBuilder WithName(string name)
    {
        this.name = name;
        return this;
    }

    public NodeAspectDefinitionBuilder WithDescription(string description)
    {
        this.description = description;
        return this;
    }

    public NodeAspectDefinitionBuilder Discovered(bool isDiscovered)
    {
        this.isDiscovered = isDiscovered;
        return this;
    }

    public NodeAspectDefinitionBuilder WithSkillType(SkillTypes skillType)
    {
        this.skillType = skillType;
        return this;
    }

    public NodeAspectDefinitionBuilder WithSkillXpGain(int skillXpGain)
    {
        this.skillXpGain = skillXpGain;
        return this;
    }

    public NodeAspectDefinitionBuilder WithYields(Action<YieldDefinitionBuilder> buildYields)
    {
        YieldDefinitionBuilder yieldBuilder = new YieldDefinitionBuilder();
        buildYields(yieldBuilder);

        YieldDefinition yieldDefinition = yieldBuilder.Build();

        this.Yields.Add(yieldDefinition);

        return this;
    }

    internal NodeAspectDefinition Build()
    {
        return new NodeAspectDefinition()
        {
            Id = name,
            Description = description,
            IsDiscovered = isDiscovered,
            SkillType = skillType,
            SkillXPGain = skillXpGain,
            Yields = Yields
        };
    }
}
