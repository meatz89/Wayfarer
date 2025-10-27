using System.Collections.Generic;

/// <summary>
/// DTO for State definitions - metadata about temporary conditions players can have
/// States are applied to players and tracked as ActiveState instances
/// </summary>
public class StateDTO
{
    /// <summary>
    /// State type (must match StateType enum)
    /// Values: "Wounded", "Exhausted", "Sick", "Injured", "Starving", "Armed", "Provisioned", "Rested",
    ///         "Confused", "Traumatized", "Inspired", "Focused", "Obsessed",
    ///         "Wanted", "Celebrated", "Shunned", "Humiliated", "Disguised", "Indebted", "Trusted"
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// Category of this state
    /// Values: "Physical", "Mental", "Social"
    /// </summary>
    public string Category { get; set; }

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
    /// Conditions that can clear this state
    /// Examples: "Rest", "RestAtSafeLocation", "ConsumeFood", "Manual", "TimePassage"
    /// </summary>
    public List<string> ClearConditions { get; set; } = new List<string>();

    /// <summary>
    /// Duration in segments before state auto-clears
    /// null = does not auto-clear based on time, must be cleared manually or by condition
    /// </summary>
    public int? Duration { get; set; }
}
