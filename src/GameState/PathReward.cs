/// <summary>
/// Strongly-typed reward for PathCard one-time effects.
/// Replaces string parsing with categorical reward system.
/// Created at parse-time from PathCardDTO.OneTimeReward string.
/// </summary>
public class PathReward
{
    /// <summary>
    /// Type of reward
    /// </summary>
    public PathRewardType RewardType { get; init; }

    /// <summary>
    /// Amount for numeric rewards (coins, items with quantities)
    /// Null for non-numeric rewards
    /// </summary>
    public int? Amount { get; init; }

    /// <summary>
    /// Specific ID for observation cards or items
    /// Null for simple numeric rewards like coins
    /// </summary>
    public string SpecificId { get; init; }

    /// <summary>
    /// Sentinel value for no reward
    /// </summary>
    public static readonly PathReward None = new PathReward
    {
        RewardType = PathRewardType.None
    };

    /// <summary>
    /// Create a coins reward
    /// </summary>
    public static PathReward Coins(int amount)
    {
        return new PathReward
        {
            RewardType = PathRewardType.Coins,
            Amount = amount
        };
    }

    /// <summary>
    /// Create an observation card reward (not implemented in current design)
    /// </summary>
    public static PathReward Observation(string observationId)
    {
        return new PathReward
        {
            RewardType = PathRewardType.Observation,
            SpecificId = observationId
        };
    }
}
