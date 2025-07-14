
/// <summary>
/// Represents a market transaction requirement for contract completion
/// </summary>
public class ContractTransaction
{
    public string ItemId { get; set; } = "";
    public string LocationId { get; set; } = "";
    public TransactionType TransactionType { get; set; }
    public int Quantity { get; set; } = 1;
    public int? MinPrice { get; set; } // Minimum price for sell transactions
    public int? MaxPrice { get; set; } // Maximum price for buy transactions

    public ContractTransaction() { }

    public ContractTransaction(string itemId, string locationId, TransactionType transactionType, int quantity = 1)
    {
        ItemId = itemId;
        LocationId = locationId;
        TransactionType = transactionType;
        Quantity = quantity;
    }

    /// <summary>
    /// Check if this transaction matches the requirement
    /// </summary>
    public bool Matches(string itemId, string locationId, TransactionType transactionType, int quantity, int price)
    {
        if (ItemId != itemId || LocationId != locationId || TransactionType != transactionType)
            return false;

        if (quantity < Quantity)
            return false;

        // Check price constraints
        if (TransactionType == TransactionType.Buy && MaxPrice.HasValue && price > MaxPrice.Value)
            return false;

        if (TransactionType == TransactionType.Sell && MinPrice.HasValue && price < MinPrice.Value)
            return false;

        return true;
    }
}
