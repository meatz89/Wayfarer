using System.Collections.Generic;

/// <summary>
/// Data Transfer Object for deserializing Location action data from JSON.
/// Maps to the structure in the locationActions array in game packages.
/// LocationActions are location-specific (matched by properties), NOT venue-wide.
/// </summary>
public class LocationActionDTO
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
    /// Required location properties (ALL must be present on the location)
    /// </summary>
    public List<string> RequiredProperties { get; set; } = new List<string>();

    /// <summary>
    /// Optional location properties (AT LEAST ONE must be present if specified)
    /// </summary>
    public List<string> OptionalProperties { get; set; } = new List<string>();

    /// <summary>
    /// Properties that prevent this action (if ANY are present, action is unavailable)
    /// </summary>
    public List<string> ExcludedProperties { get; set; } = new List<string>();

    /// <summary>
    /// Resource costs required to perform this action
    /// </summary>
    public ActionCostsDTO Cost { get; set; } = new ActionCostsDTO();

    /// <summary>
    /// Resources rewarded for performing this action
    /// </summary>
    public ActionRewardsDTO Reward { get; set; } = new ActionRewardsDTO();

    /// <summary>
    /// Time required to complete this action in segments
    /// </summary>
    public int TimeRequired { get; set; }

    /// <summary>
    /// Time blocks when this action is available (e.g., Morning, Afternoon)
    /// </summary>
    public List<string> Availability { get; set; } = new List<string>();

    /// <summary>
    /// Priority for sorting when multiple actions match (lower = higher priority)
    /// </summary>
    public int Priority { get; set; } = 100;

    /// <summary>
    /// Action type for special handling (e.g., "travel", "work", "rest", "obligation")
    /// </summary>
    public string ActionType { get; set; }

    /// <summary>
    /// Obligation ID if this action launches an obligation (V2)
    /// </summary>
    public string ObligationId { get; set; }
}