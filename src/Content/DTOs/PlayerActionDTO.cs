using System.Collections.Generic;

/// <summary>
/// Data Transfer Object for deserializing global Player action data from JSON.
/// PlayerActions are available everywhere, not location-specific like LocationActions.
/// Maps to the structure in the playerActions array in game packages.
/// </summary>
public class PlayerActionDTO
{
    /// <summary>
    /// Unique identifier for this action
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Display name shown to the player
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Detailed description of what this action does
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Resource costs required to perform this action (e.g., attention, coins)
    /// </summary>
    public Dictionary<string, int> Cost { get; set; } = new Dictionary<string, int>();

    /// <summary>
    /// Time required to complete this action in segments
    /// </summary>
    public int TimeRequired { get; set; }

    /// <summary>
    /// Priority for sorting when multiple actions are available (lower = higher priority)
    /// </summary>
    public int Priority { get; set; } = 100;

    /// <summary>
    /// Action type for special handling (e.g., "check_belongings", "open_journal")
    /// </summary>
    public string ActionType { get; set; }
}
