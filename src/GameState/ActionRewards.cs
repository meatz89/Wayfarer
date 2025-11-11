/// <summary>
/// Strongly-typed rewards for actions.
/// These are concrete values calculated at parse-time from categorical JSON properties.
/// </summary>
public class ActionRewards
{
    /// <summary>
    /// Coin reward from this action
    /// </summary>
    public int CoinReward { get; set; }

    /// <summary>
    /// Health recovery from this action
    /// </summary>
    public int HealthRecovery { get; set; }

    /// <summary>
    /// Focus recovery from this action
    /// </summary>
    public int FocusRecovery { get; set; }

    /// <summary>
    /// Stamina recovery from this action
    /// </summary>
    public int StaminaRecovery { get; set; }

    /// <summary>
    /// Whether this action provides full recovery of all resources
    /// </summary>
    public bool FullRecovery { get; set; }

    /// <summary>
    /// Creates an ActionRewards instance with no rewards
    /// </summary>
    public static ActionRewards None()
    {
        return new ActionRewards();
    }
}
