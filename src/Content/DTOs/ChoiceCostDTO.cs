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

/// <summary>
/// Health cost (physical risk/injury)
/// 0 = no health risk
/// </summary>
public int Health { get; set; } = 0;

/// <summary>
/// Hunger increase (physical exertion)
/// Positive values increase hunger, negative values decrease hunger
/// 0 = no hunger impact
/// </summary>
public int Hunger { get; set; } = 0;

/// <summary>
/// Stamina cost (physical/mental exertion)
/// 0 = no stamina cost
/// </summary>
public int Stamina { get; set; } = 0;

/// <summary>
/// Focus cost (mental concentration)
/// 0 = no focus cost
/// </summary>
public int Focus { get; set; } = 0;
}
