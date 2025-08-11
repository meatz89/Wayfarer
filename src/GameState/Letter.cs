using System;
using System.Collections.Generic;

/// <summary>
/// Defines what's at stake for this letter - drives narrative urgency and NPC emotional states
/// </summary>
public enum StakeType
{
    REPUTATION,  // Social consequences, standing at risk
    WEALTH,      // Financial impact, economic consequences
    SAFETY,      // Physical danger, threats to wellbeing  
    SECRET       // Hidden information, dangerous knowledge
}

/// <summary>
/// The emotional weight of this letter - how much it should weigh on the player's conscience
/// </summary>
public enum EmotionalWeight
{
    LOW,      // Routine correspondence
    MEDIUM,   // Important but not life-changing
    HIGH,     // Life-altering consequences
    CRITICAL  // Life or death situations
}

public enum LetterState
{
    Offered,    // NPC has mentioned it, not in queue yet
    Collected,  // In queue, ready for delivery
    Delivering  // Currently being delivered
}

public enum LetterSpecialType
{
    None,         // Regular letter
    Introduction, // Trust - Unlocks NPCs
    AccessPermit, // Commerce - Unlocks locations
    Endorsement,  // Status - Grants temporary bonuses
    Information   // Shadow - Triggers discovery events
}

[Flags]
public enum LetterPhysicalProperties
{
    None = 0,
    Fragile = 1,      // Requires careful handling, can't use fast travel
    Heavy = 2,        // Slows movement, requires strength
    Perishable = 4,   // DeadlineInHours decreases faster
    Valuable = 8,     // Attracts unwanted attention
    Bulky = 16,       // Can't stack with other bulky items
    RequiresProtection = 32  // Needs waterproof container in rain
}

public class Letter
{
    public string Id { get; set; }
    public string SenderName { get; set; }  // Just a string for minimal POC
    public string RecipientName { get; set; }
    public int DeadlineInHours { get; set; } // Hours until letter expires
    public int Payment { get; set; }
    public ConnectionType TokenType { get; set; }
    
    // Literary UI properties - drives narrative generation
    public StakeType Stakes { get; set; } = StakeType.REPUTATION;

    // Tier system (T1-T3) for difficulty progression
    public TierLevel Tier { get; set; } = TierLevel.T1;

    // Three-state system
    public LetterState State { get; set; } = LetterState.Offered;

    // Special letter type
    public LetterSpecialType SpecialType { get; set; } = LetterSpecialType.None;

    // Queue position tracking for leverage visualization
    public int? OriginalQueuePosition { get; set; }
    public int LeverageBoost { get; set; } = 0;

    // Additional properties for future use but set defaults for POC
    public int QueuePosition { get; set; } = 0;
    public SizeCategory Size { get; set; } = SizeCategory.Medium;
    public bool IsFromPatron { get; set; } = false; // Used for messaging only, not positioning

    // Physical properties
    public LetterPhysicalProperties PhysicalProperties { get; set; } = LetterPhysicalProperties.None;
    public ItemCategory? RequiredEquipment { get; set; } = null;

    // Patron letter properties
    // Patron letters use standing obligations and debt leverage, not special properties

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

    // Content and Human Context
    public string Message { get; set; } = "";
    public string HumanContext { get; set; } = ""; // One-line emotional hook showing what's really at stake
    public string ConsequenceIfLate { get; set; } = ""; // What happens if we fail to deliver
    public string ConsequenceIfDelivered { get; set; } = ""; // What we prevent by delivering
    public EmotionalWeight EmotionalWeight { get; set; } = EmotionalWeight.MEDIUM; // How heavy this weighs on conscience

    // Special letter properties
    public string UnlocksNPCId { get; set; } = "";  // For Introduction letters
    public string UnlocksLocationId { get; set; } = "";  // For Access Permit letters
    public int BonusDuration { get; set; } = 0;  // For Endorsement letters (days)
    public string InformationId { get; set; } = "";  // For Information letters

    // Helper properties
    public bool IsExpired => DeadlineInHours <= 0;
    public bool IsSpecial => SpecialType != LetterSpecialType.None;
    public int CarryWeight => PhysicalProperties.HasFlag(LetterPhysicalProperties.Heavy) ? 3 : 1;
    
    /// <summary>
    /// Get the weight of this letter for queue management (1-3 slots)
    /// </summary>
    public int Weight => GetRequiredSlots();

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
        if (IsExpired) return "EXPIRED!";
        if (DeadlineInHours <= 2) return $"{DeadlineInHours}h ⚡ CRITICAL!";
        if (DeadlineInHours <= 6) return $"{DeadlineInHours}h 🔥 URGENT";
        if (DeadlineInHours <= 12) return $"{DeadlineInHours}h ⚠️ urgent";
        if (DeadlineInHours <= 24) return $"{DeadlineInHours}h today";
        int days = DeadlineInHours / 24;
        int hours = DeadlineInHours % 24;
        if (hours == 0) return $"{days}d";
        return $"{days}d {hours}h";
    }

    public string GetTokenTypeIcon()
    {
        return TokenType switch
        {
            ConnectionType.Trust => "❤️",
            ConnectionType.Commerce => "🪙",
            ConnectionType.Status => "👑",
            ConnectionType.Shadow => "🌑",
            _ => "❓"
        };
    }

    public string GetSpecialDescription()
    {
        return SpecialType switch
        {
            LetterSpecialType.Introduction => "Letter of Introduction - Will unlock access to a new contact",
            LetterSpecialType.AccessPermit => "Access Permit - Grants permission to visit restricted locations",
            LetterSpecialType.Endorsement => "Letter of Endorsement - Provides temporary relationship benefits",
            LetterSpecialType.Information => "Confidential Information - Contains valuable intelligence",
            _ => ""
        };
    }

    public string GetSpecialIcon()
    {
        return SpecialType switch
        {
            LetterSpecialType.Introduction => "🤝",
            LetterSpecialType.AccessPermit => "🔓",
            LetterSpecialType.Endorsement => "📜",
            LetterSpecialType.Information => "🔍",
            _ => ""
        };
    }

    /// <summary>
    /// Generate narrative hint about what's at stake - for literary UI
    /// </summary>
    public string GetStakesHint()
    {
        // Use the human context if available, otherwise fall back to generic descriptions
        if (!string.IsNullOrEmpty(HumanContext))
            return HumanContext;
            
        return (TokenType, Stakes) switch
        {
            (ConnectionType.Trust, StakeType.REPUTATION) => "a matter of personal honor",
            (ConnectionType.Trust, StakeType.WEALTH) => "a family's financial crisis",
            (ConnectionType.Trust, StakeType.SAFETY) => "a warning between friends",
            (ConnectionType.Trust, StakeType.SECRET) => "a dangerous confession",
            
            (ConnectionType.Commerce, StakeType.REPUTATION) => "a merchant's credibility",
            (ConnectionType.Commerce, StakeType.WEALTH) => "an urgent trade arrangement",
            (ConnectionType.Commerce, StakeType.SAFETY) => "dangerous cargo manifest",
            (ConnectionType.Commerce, StakeType.SECRET) => "smuggler's instructions",
            
            (ConnectionType.Status, StakeType.REPUTATION) => "a noble's standing",
            (ConnectionType.Status, StakeType.WEALTH) => "an inheritance dispute",
            (ConnectionType.Status, StakeType.SAFETY) => "a challenge to duel",
            (ConnectionType.Status, StakeType.SECRET) => "court intrigue",
            
            (ConnectionType.Shadow, StakeType.REPUTATION) => "blackmail material",
            (ConnectionType.Shadow, StakeType.WEALTH) => "thieves' guild dues",
            (ConnectionType.Shadow, StakeType.SAFETY) => "an assassin's warning",
            (ConnectionType.Shadow, StakeType.SECRET) => "information that kills",
            
            _ => "correspondence"
        };
    }
    
    /// <summary>
    /// Get the emotional weight icon for UI display
    /// </summary>
    public string GetEmotionalWeightIcon()
    {
        return EmotionalWeight switch
        {
            EmotionalWeight.LOW => "",
            EmotionalWeight.MEDIUM => "💭",
            EmotionalWeight.HIGH => "💔",
            EmotionalWeight.CRITICAL => "⚠️",
            _ => ""
        };
    }
    
    /// <summary>
    /// Get a short consequence preview for the UI
    /// </summary>
    public string GetConsequencePreview()
    {
        if (!string.IsNullOrEmpty(ConsequenceIfLate))
            return $"If late: {ConsequenceIfLate}";
            
        return Stakes switch
        {
            StakeType.REPUTATION => "If late: Reputation destroyed",
            StakeType.WEALTH => "If late: Financial ruin",
            StakeType.SAFETY => "If late: Someone gets hurt",
            StakeType.SECRET => "If late: Truth remains hidden",
            _ => "If late: Opportunity lost"
        };
    }

    public Letter()
    {
        Id = Guid.NewGuid().ToString();
    }
}