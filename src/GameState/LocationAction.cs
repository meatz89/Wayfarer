using System.Collections.Generic;

/// <summary>
/// Represents a location-specific action that players can take.
/// Loaded from JSON data and used by LocationActionManager to provide dynamic actions.
/// </summary>
public class LocationAction
{
    /// <summary>
    /// Unique identifier for this action
    /// </summary>
    public string Id { get; set; } = "";

    /// <summary>
    /// Display name shown to the player
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Detailed description of what this action does
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// Location ID where this action is available
    /// </summary>
    public string LocationId { get; set; } = "";

    /// <summary>
    /// List of spot IDs where this action can be performed
    /// </summary>
    public List<string> SpotIds { get; set; } = new List<string>();

    /// <summary>
    /// Resource costs required to perform this action (e.g., attention, coins)
    /// </summary>
    public Dictionary<string, int> Cost { get; set; } = new Dictionary<string, int>();

    /// <summary>
    /// Resources rewarded for performing this action (e.g., coins, stamina)
    /// </summary>
    public Dictionary<string, int> Reward { get; set; } = new Dictionary<string, int>();

    /// <summary>
    /// Time required to complete this action in minutes
    /// </summary>
    public int TimeRequired { get; set; }

    /// <summary>
    /// Time blocks when this action is available (e.g., Morning, Afternoon)
    /// </summary>
    public List<string> Availability { get; set; } = new List<string>();

    /// <summary>
    /// Icon to display for this action (optional)
    /// </summary>
    public string Icon { get; set; } = "💼";
}