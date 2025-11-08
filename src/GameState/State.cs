/// <summary>
/// State definition - metadata about a type of temporary condition
/// Loaded from 18_states.json, stored in GameWorld.States
/// Not the instance - instances are ActiveState in Player.ActiveStates
/// </summary>
public class State
{
    /// <summary>
    /// State type (unique identifier)
    /// </summary>
    public StateType Type { get; set; }

    /// <summary>
    /// Category of this state (Physical, Mental, Social)
    /// </summary>
    public StateCategory Category { get; set; }

    /// <summary>
    /// Human-readable description of what this state represents
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Actions blocked while this state is active
    /// Currently not implemented, placeholder for future restrictions
    /// </summary>
    public List<string> BlockedActions { get; set; } = new List<string>();

    /// <summary>
    /// Actions enabled while this state is active
    /// Currently not implemented, placeholder for future bonuses
    /// </summary>
    public List<string> EnabledActions { get; set; } = new List<string>();

    /// <summary>
    /// Strongly-typed behavior object describing HOW this state can be cleared
    /// Set at parse time by StateClearConditionsCatalogue
    /// Checked at runtime by StateClearingResolver
    /// NEVER null - always initialized by parser
    /// </summary>
    public StateClearingBehavior ClearingBehavior { get; set; }

    /// <summary>
    /// Default duration in segments before state auto-clears
    /// null = does not auto-clear based on time, must be cleared manually or by condition
    /// Used as default when creating ActiveState instances
    /// </summary>
    public int? Duration { get; set; }
}
