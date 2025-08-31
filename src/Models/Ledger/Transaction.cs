using System;

public class Transaction
{
    public string Id { get; set; }
    public DateTime Timestamp { get; set; }
    public TransactionType Type { get; set; }
    public TransactionClass Class { get; set; }
    public int Amount { get; set; } // Positive for income, negative for expenses
    public string Description { get; set; }
    public string NpcId { get; set; } // Related NPC if applicable
    public string LocationId { get; set; } // Where transaction occurred
    public int BalanceAfter { get; set; } // Player's coin balance after transaction

    public Transaction()
    {
        Id = Guid.NewGuid().ToString();
        Timestamp = DateTime.Now;
    }

    public Transaction(TransactionType type, int amount, string description, string npcId = null, string locationId = null)
        : this()
    {
        Type = type;
        Amount = amount;
        Description = description;
        NpcId = npcId;
        LocationId = locationId;
    }
}