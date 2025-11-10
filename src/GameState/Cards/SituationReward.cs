
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
public Dictionary<string, int> Tokens { get; set; } = new Dictionary<string, int>();
}
