
/// <summary>
/// Types of market transactions
/// </summary>
public enum TransactionType
{
Buy,
Sell
}

public enum TransactionClass
{
Work,           // Work action at commercial Locations
Delivery,       // Letter delivery completion
Exchange,       // Quick exchange trades
Rest,           // Rest actions (buying attention/food)
Bribe,          // Checkpoint bribes
Penalty,        // Failed delivery penalties
Reward          // Special rewards (observations, etc)
}
