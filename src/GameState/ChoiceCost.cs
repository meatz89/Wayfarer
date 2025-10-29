/// <summary>
/// Resources player must pay to select a Choice
/// Sir Brante pattern: Costs are VISIBLE before selection
/// Creates strategic resource competition
/// </summary>
public class ChoiceCost
{
    /// <summary>
    /// Coin cost (must be paid to select)
    /// </summary>
    public int Coins { get; set; } = 0;

    /// <summary>
    /// Resolve cost (must be paid to select)
    /// </summary>
    public int Resolve { get; set; } = 0;

    /// <summary>
    /// Time cost in segments (4 segments per time block)
    /// Selecting this Choice advances time
    /// </summary>
    public int TimeSegments { get; set; } = 0;
}
