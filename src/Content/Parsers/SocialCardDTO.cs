/// <summary>
/// DTO for individual conversation cards
/// Field optionality contract documented in field-optionality-contract.md
/// </summary>
public class SocialCardDTO
{
    // ========== REQUIRED FIELDS (100% frequency in JSON) ==========
    // Parser must crash if these are missing
    public string Id { get; set; }
    public string Type { get; set; }
    public string Title { get; set; }
    public string DialogueText { get; set; }
    public string Persistence { get; set; } // Echo/Statement
    public string Delivery { get; set; } // "Standard" (+1), "Commanding" (+2), "Measured" (+0), "Yielding" (-1)

    // ========== CONDITIONAL FIELDS (95% frequency - conversation cards only) ==========
    // These appear in conversation cards but not request cards
    public string ConversationalMove { get; set; } // Remark/Observation/Argument - CORE categorical property
    public string BoundStat { get; set; } // insight/rapport/authority/diplomacy/cunning
    public int? Depth { get; set; } // 1-10 depth system (nullable for request cards that don't have depth)
    public List<string> PersonalityTypes { get; set; }

    // ========== RARE FIELDS (4% frequency - request cards only) ==========
    public int? MomentumThreshold { get; set; } // For request cards

    // ========== OPTIONAL FIELDS (0% frequency - parser handles null gracefully) ==========
    // These fields are NOT in JSON currently but parser checks for them
    // Parser handles null with semantically valid defaults, not bug hiding
    public string ConnectionType { get; set; } // Token type (Trust/Diplomacy/Status/Shadow) - defaults to Trust
    public CardEffectsDTO Effects { get; set; } // Legacy effects structure - returns None if null
    // DOMAIN COLLECTION PRINCIPLE: List of objects instead of Dictionary
    public List<TokenEntryDTO> TokenRequirement { get; set; } // Signature card requirements - empty list if null
}
