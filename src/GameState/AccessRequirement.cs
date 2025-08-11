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
    /// Token requirements - must have at least this many tokens with specific NPCs.
    /// </summary>
    public List<TokenRequirement> RequiredTokensPerNPC { get; set; } = new List<TokenRequirement>();

    /// <summary>
    /// Token type requirements - must have at least this many tokens of a specific type (from any NPC).
    /// </summary>
    public List<TokenTypeRequirement> RequiredTokensPerType { get; set; } = new List<TokenTypeRequirement>();

    /// <summary>
    /// Seal requirements - must have a seal of specific type and minimum tier.
    /// </summary>
    public List<SealRequirement> RequiredSeals { get; set; } = new List<SealRequirement>();

    /// <summary>
    /// Minimum tier required to access this content (1-5).
    /// Tier 1 is always accessible, higher tiers require progression.
    /// </summary>
    public int MinimumTier { get; set; } = 1;

    /// <summary>
    /// Information ID that must be discovered before this can be accessed.
    /// Implements the "Knowledge Gate" of the triple-gate system.
    /// </summary>
    public string RequiredInformationId { get; set; } = string.Empty;

    /// <summary>
    /// Special letter type that can unlock this access (alternative to other requirements).
    /// CONTENT EFFICIENT: Allows travel permits to unlock routes without extra NPCs.
    /// </summary>
    public LetterSpecialType? AlternativeLetterUnlock { get; set; } = null;

    /// <summary>
    /// Whether a travel permit has been delivered to unlock this requirement.
    /// Tracked per-route to allow multiple paths to the same destination.
    /// </summary>
    public bool HasReceivedPermit { get; set; } = false;

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