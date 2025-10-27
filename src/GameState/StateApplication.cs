using Wayfarer.GameState.Enums;

/// <summary>
/// Projected state application or removal
/// Shows temporary condition consequence before player commits to situation
/// </summary>
public class StateApplication
{
    /// <summary>
    /// State type to apply or remove
    /// </summary>
    public StateType StateType { get; set; }

    /// <summary>
    /// Whether to apply (true) or remove (false) this state
    /// Apply = Add to Player.ActiveStates
    /// Remove = Remove from Player.ActiveStates
    /// </summary>
    public bool Apply { get; set; }

    /// <summary>
    /// Human-readable explanation of why state changes
    /// Example: "Exhausting work leaves you Tired"
    /// </summary>
    public string Reason { get; set; }
}
