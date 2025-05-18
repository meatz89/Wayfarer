public class CommissionDefinition
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public CommissionTypes Type { get; set; }
    public int ProgressThreshold { get; set; }
    public int ExpirationDays { get; set; }
    public int ReputationRequirement { get; set; }
    public string InitialLocationId { get; set; }
    public int SilverReward { get; set; }
    public int ReputationReward { get; set; }
    public int InsightPointReward { get; set; }
    public int Tier { get; set; } = 1;
    public List<ApproachDefinition> Approaches { get; set; } = new List<ApproachDefinition>();
    public CommissionStep InitialStep { get; set; }
}