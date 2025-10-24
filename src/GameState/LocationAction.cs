using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Represents a location-specific action that The Single Player can take.
/// Uses property-based matching to dynamically determine availability at different locations.
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
    /// Required location properties for this action to be available.
    /// The location must have ALL of these properties for the action to appear.
    /// </summary>
    public List<LocationPropertyType> RequiredProperties { get; set; } = new List<LocationPropertyType>();

    /// <summary>
    /// Optional location properties that enable this action.
    /// The location must have AT LEAST ONE of these properties (if specified).
    /// </summary>
    public List<LocationPropertyType> OptionalProperties { get; set; } = new List<LocationPropertyType>();

    /// <summary>
    /// Properties that prevent this action from appearing.
    /// If the location has ANY of these properties, the action is unavailable.
    /// </summary>
    public List<LocationPropertyType> ExcludedProperties { get; set; } = new List<LocationPropertyType>();

    /// <summary>
    /// Resource costs required to perform this action
    /// </summary>
    public ActionCosts Costs { get; set; } = new ActionCosts();

    /// <summary>
    /// Resources rewarded for performing this action
    /// </summary>
    public ActionRewards Rewards { get; set; } = new ActionRewards();

    /// <summary>
    /// Time required to complete this action in minutes
    /// </summary>
    public int TimeRequired { get; set; }

    /// <summary>
    /// Time blocks when this action is available (strongly-typed enum)
    /// </summary>
    public List<TimeBlocks> Availability { get; set; } = new List<TimeBlocks>();

    /// <summary>
    /// Priority for sorting when multiple actions match (lower = higher priority)
    /// </summary>
    public int Priority { get; set; } = 100;

    /// <summary>
    /// Action type for execution dispatch - strongly typed enum validated by parser
    /// </summary>
    public LocationActionType ActionType { get; set; }

    /// <summary>
    /// Engagement type for tactical system integration (Mental, Physical, Social, Conversation)
    /// </summary>
    public string EngagementType { get; set; }

    /// <summary>
    /// Obligation ID if this action launches an obligation (V2)
    /// </summary>
    public string ObligationId { get; set; }

    /// <summary>
    /// Check if this action matches a given location's properties
    /// </summary>
    public bool MatchesLocation(Location location, TimeBlocks currentTime)
    {
        if (location == null) return false;

        // Get all active properties for the current time
        List<LocationPropertyType> activeProperties = location.GetActiveProperties(currentTime);

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
