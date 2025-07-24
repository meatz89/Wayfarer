using System;
using System.Collections.Generic;

public enum LetterState
{
    Offered,    // NPC has mentioned it, not in queue yet
    Accepted,   // In queue (positions 1-8), not physical
    Collected,  // Physical item in inventory, ready for delivery
    Delivering  // Currently being delivered
}

[Flags]
public enum LetterPhysicalProperties
{
    None = 0,
    Fragile = 1,      // Requires careful handling, can't use fast travel
    Heavy = 2,        // Slows movement, requires strength
    Perishable = 4,   // Deadline decreases faster
    Valuable = 8,     // Attracts unwanted attention
    Bulky = 16,       // Can't stack with other bulky items
    RequiresProtection = 32  // Needs waterproof container in rain
}

public class Letter
{
    public string Id { get; set; }
    public string SenderName { get; set; }  // Just a string for minimal POC
    public string RecipientName { get; set; }
    public int Deadline { get; set; }
    public int Payment { get; set; }
    public ConnectionType TokenType { get; set; }

    // Three-state system
    public LetterState State { get; set; } = LetterState.Offered;

    // Additional properties for future use but set defaults for POC
    public int QueuePosition { get; set; } = 0;
    public SizeCategory Size { get; set; } = SizeCategory.Medium;
    public bool IsFromPatron { get; set; } = false;

    // Physical properties
    public LetterPhysicalProperties PhysicalProperties { get; set; } = LetterPhysicalProperties.None;
    public ItemCategory? RequiredEquipment { get; set; } = null;

    // Patron letter properties
    public bool IsPatronLetter { get; set; } = false;
    public int PatronQueuePosition { get; set; } = 0; // 1-3 for patron letters

    // Tracking
    public string SenderId { get; set; }
    public string RecipientId { get; set; }
    public int DaysInQueue { get; set; } = 0;
    public string Description { get; set; }

    // Properties for forced letter generation
    public bool IsGenerated { get; set; } = false;
    public string GenerationReason { get; set; } = "";

    // Properties for letter chains
    public List<string> UnlocksLetterIds { get; set; } = new List<string>();
    public string ParentLetterId { get; set; } = "";
    public bool IsChainLetter { get; set; } = false;

    // Content
    public string Message { get; set; } = "";

    // Helper properties
    public bool IsExpired => Deadline <= 0;

    /// <summary>
    /// Get the number of inventory slots this letter requires
    /// </summary>
    public int GetRequiredSlots()
    {
        return Size switch
        {
            SizeCategory.Tiny => 1,
            SizeCategory.Small => 1,
            SizeCategory.Medium => 1,
            SizeCategory.Large => 2,
            SizeCategory.Massive => 3,
            _ => 2
        };
    }

    /// <summary>
    /// Convert letter size to item size category for inventory
    /// </summary>
    public SizeCategory GetItemSizeCategory()
    {
        return Size switch
        {
            SizeCategory.Small => SizeCategory.Small,
            SizeCategory.Medium => SizeCategory.Medium,
            SizeCategory.Large => SizeCategory.Large,
            _ => SizeCategory.Medium
        };
    }

    /// <summary>
    /// Create an inventory item representing this physical letter
    /// </summary>
    public Item CreateInventoryItem()
    {
        return new Item
        {
            Id = $"letter_{Id}",
            Name = $"Letter: {SenderName} to {RecipientName}",
            Description = $"A sealed letter to be delivered. Deadline: {GetDeadlineDescription()}",
            Categories = new List<ItemCategory> { ItemCategory.Documents },
            Size = GetItemSizeCategory(),
            Weight = HasPhysicalProperty(LetterPhysicalProperties.Heavy) ? 3 : 1,
            BuyPrice = 0,
            SellPrice = 0,
            InventorySlots = GetRequiredSlots()
        };
    }

    /// <summary>
    /// Check if this letter has a specific physical property
    /// </summary>
    public bool HasPhysicalProperty(LetterPhysicalProperties property)
    {
        return (PhysicalProperties & property) == property;
    }

    /// <summary>
    /// Get a description of physical constraints for UI display
    /// </summary>
    public string GetPhysicalConstraintsDescription()
    {
        List<string> constraints = new List<string>();

        if (HasPhysicalProperty(LetterPhysicalProperties.Fragile))
            constraints.Add("Fragile - no fast travel");
        if (HasPhysicalProperty(LetterPhysicalProperties.Heavy))
            constraints.Add("Heavy - slows movement");
        if (HasPhysicalProperty(LetterPhysicalProperties.Perishable))
            constraints.Add("Perishable - deadline decreases faster");
        if (HasPhysicalProperty(LetterPhysicalProperties.Valuable))
            constraints.Add("Valuable - attracts attention");
        if (HasPhysicalProperty(LetterPhysicalProperties.Bulky))
            constraints.Add("Bulky - can't stack");
        if (HasPhysicalProperty(LetterPhysicalProperties.RequiresProtection))
            constraints.Add("Needs waterproof container");

        if (RequiredEquipment.HasValue)
            constraints.Add($"Requires {RequiredEquipment.Value.ToString().Replace("_", " ")}");

        return constraints.Any() ? string.Join(", ", constraints) : "None";
    }

    public string GetDeadlineDescription()
    {
        if (IsExpired) return "EXPIRED";
        if (Deadline == 1) return "1 day (URGENT!)";
        if (Deadline <= 3) return $"{Deadline} days (urgent)";
        return $"{Deadline} days";
    }

    public string GetTokenTypeIcon()
    {
        return TokenType switch
        {
            ConnectionType.Trust => "❤️",
            ConnectionType.Trade => "🪙",
            ConnectionType.Noble => "👑",
            ConnectionType.Common => "🍺",
            ConnectionType.Shadow => "🌑",
            _ => "❓"
        };
    }

    public Letter()
    {
        Id = Guid.NewGuid().ToString();
    }
}