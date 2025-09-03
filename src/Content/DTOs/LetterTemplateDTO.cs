using System.Collections.Generic;

/// <summary>
/// Data Transfer Object for deserializing letter template data from JSON.
/// Maps to the structure in letter_templates.json.
/// </summary>
public class LetterTemplateDTO
{
    public string Id { get; set; }
    public string Description { get; set; }
    public string TokenType { get; set; }
    public int MinDeadlineInMinutes { get; set; }
    public int MaxDeadlineInMinutes { get; set; }
    public int MinPayment { get; set; }
    public int MaxPayment { get; set; }

    // DeliveryObligation category and requirements
    public string Category { get; set; } = "Basic";
    public int? MinTokensRequired { get; set; } = 3;

    // Tier level for this letter template
    public string TierLevel { get; set; } = "T1";

    // Special letter properties
    public string SpecialType { get; set; } = "None";
    public string SpecialTargetId { get; set; }

    // Optional fields
    public List<string> PossibleSenders { get; set; }
    public List<string> PossibleRecipients { get; set; }

    // DeliveryObligation chain properties
    public List<string> UnlocksLetterIds { get; set; }

    // Physical properties
    public string Size { get; set; } = "Medium";
    public List<string> PhysicalProperties { get; set; }
    public string RequiredEquipment { get; set; }

    // Consequences
    public string ConsequenceIfLate { get; set; }
    public string ConsequenceIfDelivered { get; set; }
    public string EmotionalFocus { get; set; } = "MEDIUM";
    public string Stakes { get; set; } = "REPUTATION";
}