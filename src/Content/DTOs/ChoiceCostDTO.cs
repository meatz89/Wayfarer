/// <summary>
/// DTO for ChoiceCost - resource costs VISIBLE before selection
/// Player must pay these costs to select a Choice
/// Maps to ChoiceCost domain entity
/// </summary>
public class ChoiceCostDTO
{
    /// <summary>
    /// Currency cost
    /// 0 = free choice
    /// </summary>
    public int Coins { get; set; } = 0;

    /// <summary>
    /// Willpower cost
    /// Strategic resource enabling special actions
    /// 0 = no Resolve cost
    /// </summary>
    public int Resolve { get; set; } = 0;

    /// <summary>
    /// Time progression cost (number of segments)
    /// 1 segment = 1/4 of a time block
    /// 0 = instant action (no time passes)
    /// </summary>
    public int TimeSegments { get; set; } = 0;
}
