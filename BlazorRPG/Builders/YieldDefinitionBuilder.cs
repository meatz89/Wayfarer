public class YieldDefinitionBuilder
{
    private YieldTypes yieldType;
    private string targetId;
    private int baseAmount;
    private float skillMultiplier;
    private SkillTypes? scalingSkillType;
    private List<YieldCondition> conditions;

    public YieldDefinitionBuilder WithType(YieldTypes yieldTypes)
    {
        this.yieldType = yieldType;
        return this;
    }

    public YieldDefinitionBuilder WithTargetId(string targetId)
    {
        this.targetId = targetId;
        return this;
    }

    public YieldDefinitionBuilder WithBaseAmount(int baseAmount)
    {
        this.baseAmount = baseAmount;
        return this;
    }

    public YieldDefinitionBuilder WithSkillMultiplier(float skillMultiplier)
    {
        this.skillMultiplier = skillMultiplier;
        return this;
    }

    public YieldDefinitionBuilder WithScalingSkillType(SkillTypes scalingSkillType)
    {
        this.scalingSkillType = scalingSkillType;
        return this;
    }

    internal YieldDefinition Build()
    {
        return new YieldDefinition()
        {
            Type = yieldType,
            TargetId = targetId,
            BaseAmount = baseAmount,
            SkillMultiplier = skillMultiplier,
            ScalingSkillType = scalingSkillType,
            Conditions = new()
        };
    }
}