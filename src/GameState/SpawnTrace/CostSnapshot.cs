/// <summary>
/// Immutable snapshot of choice cost at execution time
/// Captures what player actually paid (zero if choice was free)
/// </summary>
public class CostSnapshot
{
    public int CoinsSpent { get; set; }
    public int StaminaSpent { get; set; }
    public int FocusSpent { get; set; }
    public int HealthSpent { get; set; }
    public int ResolveSpent { get; set; }
    public int TimeSegmentsSpent { get; set; }
}
