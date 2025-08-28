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
    SECRET,      // Hidden information, dangerous knowledge
    STATUS       // Political power, social standing
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
    AccessPermit  // Commerce - Unlocks routes (locations accessible emergently)
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

public class DeliveryObligation
{
    public string Id { get; set; }
    public string SenderName { get; set; }  // Just a string for minimal POC
    public string RecipientName { get; set; }
    public int DeadlineInMinutes { get; set; } // Minutes until letter expires
    public int Payment { get; set; }
    public ConnectionType TokenType { get; set; }

    // Literary UI properties - drives narrative generation
    public StakeType Stakes { get; set; } = StakeType.REPUTATION;

    // Tier system (T1-T3) for difficulty progression
    public TierLevel Tier { get; set; } = TierLevel.T1;

    // Note: DeliveryObligation has no State - only physical Letters have states

    // Note: DeliveryObligation has no SpecialType - only physical Letters have special types

    // Queue position tracking for leverage visualization
    public int? OriginalQueuePosition { get; set; }
    public int LeverageBoost { get; set; } = 0;

    // Relationship-based positioning data (categorical - UI translates to text)
    public LetterPositioningReason PositioningReason { get; set; } = LetterPositioningReason.Neutral;
    public int RelationshipStrength { get; set; } = 0; // Highest positive token count
    public int RelationshipDebt { get; set; } = 0; // Worst negative token penalty
    public int FinalQueuePosition { get; set; } = 0; // Position after algorithm calculation

    // Additional properties for future use but set defaults for POC
    public int QueuePosition { get; set; } = 0;
    // Patron system removed - deleted all patron-related properties

    // Tracking
    public string SenderId { get; set; }
    public string RecipientId { get; set; }
    public int DaysInQueue { get; set; } = 0;
    public string Description { get; set; }

    // Properties for forced letter generation
    public bool IsGenerated { get; set; } = false;
    public string GenerationReason { get; set; } = "";
    public bool IsCrisisLetter { get; set; } = false; // Crisis letters force position 1

    // Properties for letter chains
    public List<string> UnlocksLetterIds { get; set; } = new List<string>();
    public string ParentLetterId { get; set; } = "";

    // Content
    public string Message { get; set; } = "";
    public string ConsequenceIfLate { get; set; } = ""; // What happens if we fail to deliver
    public string ConsequenceIfDelivered { get; set; } = ""; // What we prevent by delivering
    public EmotionalWeight EmotionalWeight { get; set; } = EmotionalWeight.MEDIUM; // How heavy this weighs on conscience

    // Note: DeliveryObligation has no unlock properties - only physical Letters have unlock properties
    // UnlocksNPCId, UnlocksLocationId, BonusDuration, InformationId are on Letter class only

    // Helper properties
    public bool IsExpired => DeadlineInMinutes <= 0;
    // DeliveryObligations are abstract - they don't have special types (only physical Letters do)
    public int HoursUntilDeadline => DeadlineInMinutes / 60;
    public int MinutesUntilDeadline => DeadlineInMinutes;
    public string SenderNPC => SenderName;



    public string GetDeadlineDescription()
    {
        if (IsExpired) return "EXPIRED!";
        if (DeadlineInMinutes <= 30) return $"{DeadlineInMinutes}m ⚡ CRITICAL!";
        if (DeadlineInMinutes <= 120) return $"{DeadlineInMinutes}m 🔥 URGENT";
        
        int hours = DeadlineInMinutes / 60;
        int minutes = DeadlineInMinutes % 60;
        
        if (hours <= 6) 
        {
            if (minutes == 0) return $"{hours}h ⚠️ urgent";
            return $"{hours}h {minutes}m ⚠️ urgent";
        }
        if (hours <= 16) 
        {
            if (minutes == 0) return $"{hours}h today";
            return $"{hours}h {minutes}m today";
        }
        
        int days = hours / 24;
        int remainingHours = hours % 24;
        if (days == 0) return $"{hours}h";
        if (remainingHours == 0) return $"{days}d";
        return $"{days}d {remainingHours}h";
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

    // Special descriptions are only available on physical Letters, not abstract DeliveryObligations

    // Special icons are only available on physical Letters, not abstract DeliveryObligations
    public string GetSpecialIcon()
    {
        return ""; // Obligations don't have special icons
    }

    /// <summary>
    /// Generate narrative hint about what's at stake - for literary UI
    /// </summary>
    public string GetStakesHint()
    {
        // Use the human context if available, otherwise fall back to generic descriptions

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

    public DeliveryObligation()
    {
        Id = Guid.NewGuid().ToString();
    }
}