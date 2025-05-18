public class CommissionStep
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string LocationId { get; set; }
    public int ProgressGoal { get; set; }
    public List<ApproachDefinition> Approaches { get; set; } = new List<ApproachDefinition>();
}