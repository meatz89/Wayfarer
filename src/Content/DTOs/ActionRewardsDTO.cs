/// <summary>
/// DTO for deserializing action rewards from JSON.
/// Strongly-typed properties match JSON field names exactly (no JsonPropertyName needed).
/// </summary>
public class ActionRewardsDTO
{
    /// <summary>
    /// Coin reward from this action
    /// </summary>
    public int Coins { get; set; }

    /// <summary>
    /// Health recovery from this action
    /// </summary>
    public int Health { get; set; }

    /// <summary>
    /// Focus recovery from this action
    /// </summary>
    public int Focus { get; set; }

    /// <summary>
    /// Stamina recovery from this action
    /// </summary>
    public int Stamina { get; set; }

    /// <summary>
    /// Whether this action provides full recovery of all resources
    /// </summary>
    public bool FullRecovery { get; set; }
}
