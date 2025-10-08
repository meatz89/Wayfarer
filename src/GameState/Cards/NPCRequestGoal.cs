
/// <summary>
/// Tiered request goal with weight-based rewards
/// </summary>
public class NPCRequestGoal
{
    public string Id { get; set; }
    public string CardId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int MomentumThreshold { get; set; }
    public int Weight { get; set; }
    public NPCRequestRewards Rewards { get; set; } = new NPCRequestRewards();
}
