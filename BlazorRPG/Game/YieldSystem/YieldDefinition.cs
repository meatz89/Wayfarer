public class YieldDefinition
{
    public YieldTypes Type { get; set; }
    public string TargetId { get; set; }
    public int BaseAmount { get; set; }
    public float SkillMultiplier { get; set; } = 0.0f;
    public SkillTypes? ScalingSkillType { get; set; }
    public List<YieldCondition> Conditions { get; set; } = new List<YieldCondition>();
}