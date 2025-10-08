
/// <summary>
/// Reward structure for tiered request goals
/// </summary>
public class NPCRequestRewards
{
    public int? Coins { get; set; }
    public string LetterId { get; set; }
    public string Obligation { get; set; }
    public string Item { get; set; }
    public Dictionary<string, int> Tokens { get; set; } = new Dictionary<string, int>();
}