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

    /// <summary>
    /// Duration in segments before state auto-clears
    /// null = use default duration (48 segments = 3 days) or manual clear only
    /// Used when creating ActiveState from this application
    /// </summary>
    public int? DurationSegments { get; set; }
}
