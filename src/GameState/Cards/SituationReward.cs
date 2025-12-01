
/// <summary>
/// Reward for reaching a rapport threshold in stranger conversations
/// </summary>
public class SituationReward
{
    public int Coins { get; set; }
    public int Health { get; set; }
    public int Food { get; set; }
    public int Familiarity { get; set; }
    public string Item { get; set; }
    public string Permit { get; set; }
    public string Observation { get; set; }
    // DOMAIN COLLECTION PRINCIPLE: List<T> instead of Dictionary
    public List<StringTokenEntry> Tokens { get; set; } = new List<StringTokenEntry>();
}
