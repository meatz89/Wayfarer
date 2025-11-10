
/// <summary>
/// Numeric Requirement - individual threshold check
/// Used within CompoundRequirement OR paths
/// </summary>
public class NumericRequirement
{
/// <summary>
/// Type of requirement
/// Values: "BondStrength", "Scale", "Resolve", "Coins", "CompletedSituations", "Achievement", "State", "PlayerStat", "HasItem"
/// </summary>
public string Type { get; set; }

/// <summary>
/// Context for the requirement (depends on Type)
/// - For BondStrength: NPC ID
/// - For Scale: Scale name ("Morality", "Lawfulness", etc.)
/// - For Achievement: Achievement ID
/// - For State: State type ("Trusted", "Celebrated", etc.)
/// - For PlayerStat: Stat name ("Insight", "Rapport", "Authority", "Diplomacy", "Cunning")
/// - For HasItem: Item ID
/// - For others: null or unused
/// </summary>
public string Context { get; set; }

/// <summary>
/// Threshold value that must be met
/// - For BondStrength: minimum bond strength (0-30)
/// - For Scale: minimum scale value (-10 to +10)
/// - For Resolve: minimum resolve (0-30)
/// - For Coins: minimum coins
/// - For CompletedSituations: count of completed situations
/// - For Achievement: 1 = must have, 0 = must not have
/// - For State: 1 = must have state active, 0 = must not have state
/// - For HasItem: 1 = must have item, 0 = must not have item
/// </summary>
public int Threshold { get; set; }

/// <summary>
/// Display label for this requirement (for UI)
/// Example: "Bond 15+ with Martha", "Morality +8", "Have 500 coins"
/// </summary>
public string Label { get; set; }

/// <summary>
/// Check if this requirement is satisfied by current game state
/// Self-contained pattern: markerMap resolves "generated:{templateId}" to actual IDs
/// Pass null markerMap for non-self-contained requirements
/// </summary>
public bool IsSatisfied(Player player, GameWorld gameWorld, Dictionary<string, string> markerMap = null)
{
    return Type switch
    {
        "BondStrength" => CheckBondStrength(player, Context, Threshold),
        "Scale" => CheckScale(player, Context, Threshold),
        "Resolve" => player.Resolve >= Threshold,
        "Coins" => player.Coins >= Threshold,
        "CompletedSituations" => player.CompletedSituationIds.Count >= Threshold,
        "Achievement" => CheckAchievement(player, Context, Threshold),
        "State" => CheckState(player, Context, Threshold),
        "PlayerStat" => CheckPlayerStat(player, Context, Threshold),
        "HasItem" => CheckHasItem(player, Context, Threshold, markerMap),
        _ => false // Unknown type
    };
}

private bool CheckBondStrength(Player player, string npcId, int threshold)
{
    NPCTokenEntry entry = player.NPCTokens.FirstOrDefault(t => t.NpcId == npcId);
    if (entry == null) return false;

    int totalBond = entry.Trust + entry.Diplomacy + entry.Status + entry.Shadow;
    return totalBond >= threshold;
}

private bool CheckScale(Player player, string scaleName, int threshold)
{
    int scaleValue = scaleName switch
    {
        "Morality" => player.Scales.Morality,
        "Lawfulness" => player.Scales.Lawfulness,
        "Method" => player.Scales.Method,
        "Caution" => player.Scales.Caution,
        "Transparency" => player.Scales.Transparency,
        "Fame" => player.Scales.Fame,
        _ => 0
    };

    // For scales, threshold can be positive or negative
    // Positive threshold: scale >= threshold
    // Negative threshold: scale <= threshold
    if (threshold >= 0)
        return scaleValue >= threshold;
    else
        return scaleValue <= threshold;
}

private bool CheckAchievement(Player player, string achievementId, int threshold)
{
    bool hasAchievement = player.EarnedAchievements.Any(a => a.AchievementId == achievementId);
    return threshold > 0 ? hasAchievement : !hasAchievement;
}

private bool CheckState(Player player, string stateTypeName, int threshold)
{
    bool hasState = player.ActiveStates.Any(s => s.Type.ToString() == stateTypeName);
    return threshold > 0 ? hasState : !hasState;
}

private bool CheckPlayerStat(Player player, string statName, int threshold)
{
    // Parse stat name to enum
    if (!Enum.TryParse<PlayerStatType>(statName, ignoreCase: true, out PlayerStatType statType))
    {
        return false; // Unknown stat name
    }

    // Check if player's stat level meets threshold
    int statLevel = player.Stats.GetLevel(statType);
    return statLevel >= threshold;
}

private bool CheckHasItem(Player player, string itemId, int threshold, Dictionary<string, string> markerMap)
{
    // Resolve marker to actual item ID (self-contained pattern)
    // If itemId is "generated:room_key", resolve to actual created item ID
    string resolvedItemId = ResolveMarker(itemId, markerMap ?? new Dictionary<string, string>());

    bool hasItem = player.HasItem(resolvedItemId);
    return threshold > 0 ? hasItem : !hasItem;
}

/// <summary>
/// Resolve marker to actual resource ID
/// If ID is "generated:{templateId}", look up in marker map
/// If ID is not a marker, return as-is
/// </summary>
private string ResolveMarker(string id, Dictionary<string, string> markerMap)
{
    if (string.IsNullOrEmpty(id))
        return id;

    if (id.StartsWith("generated:"))
    {
        if (markerMap.TryGetValue(id, out string resolvedId))
            return resolvedId;
    }

    return id;
}
}
