public class TimeStateDefinition
{
    public TimeRange TimeRange { get; set; }
    public EnvironmentalProperties Properties { get; set; }
    public List<string> ActiveNodeIds { get; set; } = new List<string>();
    public List<SkillProgressionModifier> SkillProgressionModifiers { get; set; } = new List<SkillProgressionModifier>();
}
