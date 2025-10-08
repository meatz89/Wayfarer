using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Represents a location-specific action that players can take.
/// Uses property-based matching to dynamically determine availability at different spots.
/// </summary>
public class LocationAction
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
    /// Required spot properties for this action to be available.
    /// The spot must have ALL of these properties for the action to appear.
    /// </summary>
    public List<SpotPropertyType> RequiredProperties { get; set; } = new List<SpotPropertyType>();

    /// <summary>
    /// Optional spot properties that enable this action.
    /// The spot must have AT LEAST ONE of these properties (if specified).
    /// </summary>
    public List<SpotPropertyType> OptionalProperties { get; set; } = new List<SpotPropertyType>();

    /// <summary>
    /// Properties that prevent this action from appearing.
    /// If the spot has ANY of these properties, the action is unavailable.
    /// </summary>
    public List<SpotPropertyType> ExcludedProperties { get; set; } = new List<SpotPropertyType>();

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
    /// Priority for sorting when multiple actions match (lower = higher priority)
    /// </summary>
    public int Priority { get; set; } = 100;

    /// <summary>
    /// Action type for special handling (e.g., "travel", "work", "rest", "investigation")
    /// </summary>
    public string ActionType { get; set; }

    /// <summary>
    /// Engagement type for tactical system integration (Mental, Physical, Social, Conversation)
    /// </summary>
    public string EngagementType { get; set; }

    /// <summary>
    /// Investigation ID if this action launches an investigation (V2)
    /// </summary>
    public string InvestigationId { get; set; }

    /// <summary>
    /// Check if this action matches a given spot's properties
    /// </summary>
    public bool MatchesSpot(LocationSpot spot, TimeBlocks currentTime)
    {
        if (spot == null) return false;

        // Get all active properties for the current time
        List<SpotPropertyType> activeProperties = spot.GetActiveProperties(currentTime);

        // Check excluded properties first (fast rejection)
        if (ExcludedProperties.Any() && activeProperties.Any(p => ExcludedProperties.Contains(p)))
            return false;

        // Check required properties (all must be present)
        if (RequiredProperties.Any() && !RequiredProperties.All(p => activeProperties.Contains(p)))
            return false;

        // Check optional properties (at least one must be present if specified)
        if (OptionalProperties.Any() && !OptionalProperties.Any(p => activeProperties.Contains(p)))
            return false;

        return true;
    }
}