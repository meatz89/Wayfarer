/// <summary>
/// Immutable snapshot of Player state at a specific point in time
/// Optional - used for detailed choice execution analysis
/// </summary>
public class PlayerStateSnapshot
{
    // Five Stats
    public int Insight { get; set; }
    public int Rapport { get; set; }
    public int Authority { get; set; }
    public int Diplomacy { get; set; }
    public int Cunning { get; set; }

    // Resources
    public int Coins { get; set; }
    public int Health { get; set; }
    public int Stamina { get; set; }
    public int Focus { get; set; }
    public int Resolve { get; set; }

    // Time
    public int CurrentDay { get; set; }
    public TimeBlocks CurrentTimeBlock { get; set; }

    // States (simplified as string list)
    public List<string> ActiveStates { get; set; }
}
