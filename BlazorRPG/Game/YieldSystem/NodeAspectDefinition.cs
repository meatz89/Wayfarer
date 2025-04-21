public class NodeAspectDefinition
{
    public string Id { get; set; }
    public string Description { get; set; }
    public SkillTypes SkillType { get; set; }
    public int SkillXPGain { get; set; }
    public bool IsDiscovered { get; set; } = false;
    public List<YieldDefinition> Yields { get; set; } = new List<YieldDefinition>();
}