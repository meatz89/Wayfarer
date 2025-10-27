using Wayfarer.GameState.Enums;

/// <summary>
/// Projected scale value shift for player behavioral reputation
/// Shows reputation consequence before player commits to situation
/// </summary>
public class ScaleShift
{
    /// <summary>
    /// Which scale type this shift affects
    /// </summary>
    public ScaleType ScaleType { get; set; }

    /// <summary>
    /// Scale value delta (positive or negative)
    /// Applied to Player.Scales[ScaleType] (clamped to -10 to +10 range)
    /// </summary>
    public int Delta { get; set; }

    /// <summary>
    /// Human-readable explanation of why scale shifts
    /// Example: "Helping the poor increases Morality"
    /// </summary>
    public string Reason { get; set; }
}
