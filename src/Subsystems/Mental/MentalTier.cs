/// <summary>
/// Represents a obligation depth tier with its unlock requirements
/// Tiers unlock at specific Understanding thresholds and grant access to deeper card depths
/// </summary>
public class MentalTier
{
public int TierNumber { get; init; }
public int UnderstandingThreshold { get; init; }
public int MinDepth { get; init; }
public int MaxDepth { get; init; }

public MentalTier(int tierNumber, int understandingThreshold, int minDepth, int maxDepth)
{
    TierNumber = tierNumber;
    UnderstandingThreshold = understandingThreshold;
    MinDepth = minDepth;
    MaxDepth = maxDepth;
}

/// <summary>
/// All obligation tiers in the game
/// SINGLE SOURCE OF TRUTH for tier definitions
/// </summary>
public static readonly MentalTier[] AllTiers = new[]
{
    new MentalTier(1, 0, 1, 2),    // Foundation: Always unlocked
    new MentalTier(2, 6, 3, 4),    // Standard: Unlocks at Understanding 6
    new MentalTier(3, 12, 5, 6),   // Decisive: Unlocks at Understanding 12
    new MentalTier(4, 18, 7, 8)    // Master: Unlocks at Understanding 18
};

public static MentalTier GetTier(int tierNumber)
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
