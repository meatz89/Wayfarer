/// <summary>
/// Calculates resource formulas and interdependencies.
/// Central Venue for all resource calculation logic.
/// </summary>
public class ResourceCalculator
{
    private const int INJURED_FOCUS_PENALTY = -1;
    private const int HEALTH_THRESHOLD_FOR_CARRY = 50;

    /// <summary>
    /// Calculate focus carrying capacity based on health.
    /// Injured The Single Player have reduced carrying capacity.
    /// </summary>
    public int CalculateFocusLimit(int health)
    {
        // If health < 50, apply penalty to carrying capacity
        return health < HEALTH_THRESHOLD_FOR_CARRY ? INJURED_FOCUS_PENALTY : 0;
    }
}
