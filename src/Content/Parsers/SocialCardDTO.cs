/// <summary>
/// DTO for individual conversation cards
/// </summary>
public class SocialCardDTO
{
    public string Id { get; set; }
    public string Type { get; set; }
    public string ConnectionType { get; set; }
    public int? MomentumThreshold { get; set; } // For request cards
    public string Title { get; set; }
    public string DialogueText { get; set; }
    public int? MinimumTokensRequired { get; set; }

    // Categorical properties - define behavior through context
    public string Persistence { get; set; } // Echo/Statement
    public string ConversationalMove { get; set; } // Remark/Observation/Argument - CORE categorical property

    // Personality targeting - which NPCs can use this card
    public List<string> PersonalityTypes { get; set; }

    // Player stats system - which stat this card is bound to
    public string BoundStat { get; set; } // insight/rapport/authority/diplomacy/cunning

    // NEW: Delivery property - how card affects Cadence when spoken
    public string Delivery { get; set; } // "Standard" (+1), "Commanding" (+2), "Measured" (+0), "Yielding" (-1)

    // 5-Resource System Properties (Understanding + Delivery)
    public int? Depth { get; set; } // 1-10 depth system
    // InitiativeCost - DERIVED from boundStat + depth (not in JSON)
    public CardEffectsDTO Effects { get; set; } // New effects structure

    // Token requirements for signature cards
    public Dictionary<string, int> TokenRequirement { get; set; }

    // NPC-specific targeting for signature cards
    public string NpcSpecific { get; set; }

    // V2 Investigation System - Understanding replaces KnowledgeGranted
    // Knowledge system eliminated - Understanding resource replaces Knowledge tokens
    public List<string> SecretsGranted { get; set; }
}
