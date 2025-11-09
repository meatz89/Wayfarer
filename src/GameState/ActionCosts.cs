/// <summary>
/// Strongly-typed costs for actions.
/// These are concrete values calculated at parse-time from categorical JSON properties.
/// </summary>
public class ActionCosts
{
/// <summary>
/// Coin cost to perform this action
/// </summary>
public int Coins { get; set; }

/// <summary>
/// Focus cost to perform this action
/// </summary>
public int Focus { get; set; }

/// <summary>
/// Stamina cost to perform this action
/// </summary>
public int Stamina { get; set; }

/// <summary>
/// Health cost to perform this action
/// </summary>
public int Health { get; set; }

/// <summary>
/// Creates an ActionCosts instance with all costs set to zero
/// </summary>
public static ActionCosts None()
{
    return new ActionCosts();
}
}
