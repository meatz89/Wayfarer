public enum LetterCategory
{
    None,     // No category (insufficient tokens)
    Basic,    // 1-2 tokens required
    Quality,  // 3-4 tokens required  
    Premium   // 5+ tokens required
}

public class LetterTemplate
{
    public string Id { get; set; }
    public string Description { get; set; }
    public ConnectionType TokenType { get; set; }
    public int MinDeadlineInSegments { get; set; }
    public int MaxDeadlineInSegments { get; set; }
    public int MinPayment { get; set; }
    public int MaxPayment { get; set; }

    // DeliveryObligation category and requirements
    public LetterCategory Category { get; set; } = LetterCategory.Basic;
    public int MinTokensRequired { get; set; } = 1; // Minimum tokens with NPC to unlock this template

    // Tier level for this letter template (T1-T3)
    public TierLevel TierLevel { get; set; } = TierLevel.T1;

    // Special letter properties
    public LetterSpecialType SpecialType { get; set; } = LetterSpecialType.None;
    public string SpecialTargetId { get; set; } = ""; // NPC/Location/Information ID for special letters

    // Optional fields for future expansion
    public string[] PossibleSenders { get; set; } // NPCs who can send this type
    public string[] PossibleRecipients { get; set; } // NPCs who can receive this type

    // DeliveryObligation chain properties
    public string[] UnlocksLetterIds { get; set; } = new string[0]; // DeliveryObligation templates unlocked by delivering this letter

    // Physical properties
    public SizeCategory Size { get; set; } = SizeCategory.Medium;
    public LetterPhysicalProperties PhysicalProperties { get; set; } = LetterPhysicalProperties.None;
    public ItemCategory? RequiredEquipment { get; set; } = null;

    // Consequences
    public string ConsequenceIfLate { get; set; } = ""; // What happens if we fail
    public string ConsequenceIfDelivered { get; set; } = ""; // What we prevent
    public EmotionalFocus EmotionalFocus { get; set; } = EmotionalFocus.MEDIUM;
    public StakeType Stakes { get; set; } = StakeType.REPUTATION; // What's at stake
}