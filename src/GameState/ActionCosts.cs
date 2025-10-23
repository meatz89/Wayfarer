/// <summary>
/// Strongly-typed costs for actions.
/// These are concrete values calculated at parse-time from categorical JSON properties.
/// </summary>
public class ActionCosts
{
    /// <summary>
    /// Coin cost to perform this action
    /// </summary>
    public int CoinCost { get; set; }

    /// <summary>
    /// Focus cost to perform this action
    /// </summary>
    public int FocusCost { get; set; }

    /// <summary>
    /// Stamina cost to perform this action
    /// </summary>
    public int StaminaCost { get; set; }

    /// <summary>
    /// Health cost to perform this action
    /// </summary>
    public int HealthCost { get; set; }

    /// <summary>
    /// Creates an ActionCosts instance with all costs set to zero
    /// </summary>
    public static ActionCosts None()
    {
        return new ActionCosts();
    }
}
