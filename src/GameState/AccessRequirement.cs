using System;
using System.Collections.Generic;
using System.Linq;
/// <summary>
/// Defines equipment, token, or other requirements needed to access a location or route.
/// </summary>
public class AccessRequirement
{
    /// <summary>
    /// Unique identifier for this requirement set.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Human-readable name for this requirement (e.g., "Noble District Access")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Required equipment categories (ANY of these will satisfy the equipment requirement).
    /// </summary>
    public List<ItemCategory> RequiredEquipment { get; set; } = new List<ItemCategory>();

    /// <summary>
    /// Required items by ID (ANY of these specific items will satisfy the item requirement).
    /// </summary>
    public List<string> RequiredItemIds { get; set; } = new List<string>();

    /// <summary>
    /// Token requirements - must have at least this many tokens with a specific NPC.
    /// Key: NPC ID, Value: minimum token count required
    /// </summary>
    public Dictionary<string, int> RequiredTokensPerNPC { get; set; } = new Dictionary<string, int>();

    /// <summary>
    /// Token type requirements - must have at least this many tokens of a specific type (from any NPC).
    /// </summary>
    public Dictionary<ConnectionType, int> RequiredTokensPerType { get; set; } = new Dictionary<ConnectionType, int>();

    /// <summary>
    /// Whether all requirements must be met (AND) or just one (OR).
    /// </summary>
    public RequirementLogic Logic { get; set; } = RequirementLogic.And;

    /// <summary>
    /// Narrative message shown when access is blocked.
    /// </summary>
    public string BlockedMessage { get; set; } = "You cannot access this area.";

    /// <summary>
    /// Hint message suggesting how to gain access.
    /// </summary>
    public string HintMessage { get; set; } = string.Empty;
}

public enum RequirementLogic
{
    And, // All requirements must be met
    Or   // Any requirement can be met
}

/// <summary>
/// Result of checking access requirements.
/// </summary>
public class AccessCheckResult
{
    public bool IsAllowed { get; set; }
    public string BlockedMessage { get; set; } = string.Empty;
    public string HintMessage { get; set; } = string.Empty;
    public List<string> MissingRequirements { get; set; } = new List<string>();

    public static AccessCheckResult Allowed()
    {
        return new AccessCheckResult { IsAllowed = true };
    }

    public static AccessCheckResult Blocked(string message, string hint = "", List<string> missing = null)
    {
        return new AccessCheckResult
        {
            IsAllowed = false,
            BlockedMessage = message,
            HintMessage = hint,
            MissingRequirements = missing ?? new List<string>()
        };
    }
}