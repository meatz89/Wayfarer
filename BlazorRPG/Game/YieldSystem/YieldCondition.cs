public class YieldCondition
{
    public ConditionTypes Type { get; set; }
    public string TargetId { get; set; }
    public int RequiredValue { get; set; }
    public float Chance { get; set; } = 100.0f;
}