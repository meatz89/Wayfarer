/// <summary>
/// Type of reward granted by PathCard one-time effects.
/// Parse-time categorization of reward strings into strongly-typed rewards.
/// </summary>
public enum PathRewardType
{
    /// <summary>
    /// No reward
    /// </summary>
    None,

    /// <summary>
    /// Grants coins to player
    /// </summary>
    Coins,

    /// <summary>
    /// Grants observation card (not implemented in current design)
    /// </summary>
    Observation
}
