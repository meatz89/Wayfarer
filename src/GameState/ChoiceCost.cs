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

/// <summary>
/// Health cost (must be paid to select)
/// Risk-based actions may require health sacrifice
/// </summary>
public int Health { get; set; } = 0;

/// <summary>
/// Hunger cost (increases hunger - negative values reduce hunger)
/// Physical exertion increases hunger
/// </summary>
public int Hunger { get; set; } = 0;

/// <summary>
/// Stamina cost (must be paid to select)
/// Physical and mental exertion depletes stamina
/// </summary>
public int Stamina { get; set; } = 0;

/// <summary>
/// Focus cost (must be paid to select)
/// Mental concentration and investigation depletes focus
/// </summary>
public int Focus { get; set; } = 0;
}
