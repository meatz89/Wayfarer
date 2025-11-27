
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
    /// </summary>
    public bool IsSatisfied(Player player, GameWorld gameWorld)
    {
        return Type switch
        {
            "BondStrength" => CheckBondStrength(player, Context, Threshold),
            "Scale" => CheckScale(player, Context, Threshold),
            "Resolve" => player.Resolve >= Threshold,
            "Coins" => player.Coins >= Threshold,
            "CompletedSituations" => player.CompletedSituations.Count >= Threshold,
            "Achievement" => CheckAchievement(player, Context, Threshold),
            "State" => CheckState(player, Context, Threshold),
            "PlayerStat" => CheckPlayerStat(player, Context, Threshold),
            "HasItem" => CheckHasItem(player, Context, Threshold),
            _ => false // Unknown type
        };
    }

    /// <summary>
    /// Get the player's current value for this requirement type.
    /// Used for projection display (show current vs required).
    /// </summary>
    public int GetCurrentValue(Player player, GameWorld gameWorld)
    {
        return Type switch
        {
            "BondStrength" => GetBondStrengthValue(player, Context),
            "Scale" => GetScaleValue(player, Context),
            "Resolve" => player.Resolve,
            "Coins" => player.Coins,
            "CompletedSituations" => player.CompletedSituations.Count,
            "Achievement" => player.EarnedAchievements.Any(a => a.Achievement.Name == Context) ? 1 : 0,
            "State" => player.ActiveStates.Any(s => s.Type.ToString() == Context) ? 1 : 0,
            "PlayerStat" => GetPlayerStatValue(player, Context),
            "HasItem" => player.HasItem(Context) ? 1 : 0,
            _ => 0
        };
    }

    private int GetBondStrengthValue(Player player, string npcId)
    {
        NPCTokenEntry entry = player.NPCTokens.FirstOrDefault(t => t.Npc.Name == npcId);
        if (entry == null) return 0;
        return entry.Trust + entry.Diplomacy + entry.Status + entry.Shadow;
    }

    private int GetScaleValue(Player player, string scaleName)
    {
        return scaleName switch
        {
            "Morality" => player.Scales.Morality,
            "Lawfulness" => player.Scales.Lawfulness,
            "Method" => player.Scales.Method,
            "Caution" => player.Scales.Caution,
            "Transparency" => player.Scales.Transparency,
            "Fame" => player.Scales.Fame,
            _ => 0
        };
    }

    private int GetPlayerStatValue(Player player, string statName)
    {
        if (!Enum.TryParse<PlayerStatType>(statName, ignoreCase: true, out PlayerStatType statType))
            return 0;

        return statType switch
        {
            PlayerStatType.Insight => player.Insight,
            PlayerStatType.Rapport => player.Rapport,
            PlayerStatType.Authority => player.Authority,
            PlayerStatType.Diplomacy => player.Diplomacy,
            PlayerStatType.Cunning => player.Cunning,
            _ => 0
        };
    }

    private bool CheckBondStrength(Player player, string npcId, int threshold)
    {
        NPCTokenEntry entry = player.NPCTokens.FirstOrDefault(t => t.Npc.Name == npcId);
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

    private bool CheckAchievement(Player player, string achievementName, int threshold)
    {
        // HIGHLANDER: PlayerAchievement uses object reference, not string ID
        // Context stores achievement Name (not ID) for lookup
        bool hasAchievement = player.EarnedAchievements.Any(a => a.Achievement.Name == achievementName);
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
        int statLevel = statType switch
        {
            PlayerStatType.Insight => player.Insight,
            PlayerStatType.Rapport => player.Rapport,
            PlayerStatType.Authority => player.Authority,
            PlayerStatType.Diplomacy => player.Diplomacy,
            PlayerStatType.Cunning => player.Cunning,
            _ => 0
        };
        return statLevel >= threshold;
    }

    private bool CheckHasItem(Player player, string itemId, int threshold)
    {
        bool hasItem = player.HasItem(itemId);
        return threshold > 0 ? hasItem : !hasItem;
    }
}
