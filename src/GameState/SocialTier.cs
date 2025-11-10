/// <summary>
/// Represents a conversation depth tier with its unlock requirements
/// Tiers unlock at specific Understanding thresholds and grant access to deeper card depths
/// </summary>
public class SocialTier
{
public int TierNumber { get; init; }
public int UnderstandingThreshold { get; init; }
public int MinDepth { get; init; }
public int MaxDepth { get; init; }

public SocialTier(int tierNumber, int understandingThreshold, int minDepth, int maxDepth)
{
    TierNumber = tierNumber;
    UnderstandingThreshold = understandingThreshold;
    MinDepth = minDepth;
    MaxDepth = maxDepth;
}

/// <summary>
/// All conversation tiers in the game
/// SINGLE SOURCE OF TRUTH for tier definitions
/// </summary>
public static readonly SocialTier[] AllTiers = new[]
{
    new SocialTier(1, 0, 1, 2),    // Foundation: Always unlocked
    new SocialTier(2, 6, 3, 4),    // Standard: Unlocks at Understanding 6
    new SocialTier(3, 12, 5, 6),   // Decisive: Unlocks at Understanding 12
    new SocialTier(4, 18, 7, 8)    // Master: Unlocks at Understanding 18
};

public static SocialTier GetTier(int tierNumber)
{
    return AllTiers[tierNumber - 1]; // Tiers are 1-indexed
}

public static int GetUnlockThreshold(int tierNumber)
{
    return GetTier(tierNumber).UnderstandingThreshold;
}

public override string ToString()
{
    return $"Tier {TierNumber} (Depths {MinDepth}-{MaxDepth}, Unlocks at Understanding {UnderstandingThreshold})";
}
}
