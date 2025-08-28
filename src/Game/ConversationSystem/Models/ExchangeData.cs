using System.Collections.Generic;

/// <summary>
/// Exchange data containing cost/reward information for exchange cards.
/// This is NOT a ConversationCard - it's separate exchange-specific data.
/// </summary>
public class ExchangeData
{
    /// <summary>
    /// Display name for the exchange (e.g., "Buy Travel Provisions")
    /// </summary>
    public string ExchangeName { get; init; }
    
    /// <summary>
    /// NPC personality for contextual exchanges
    /// </summary>
    public PersonalityType NPCPersonality { get; init; }
    
    /// <summary>
    /// Resource costs for this exchange
    /// </summary>
    public List<ResourceExchange> Cost { get; init; } = new();
    
    /// <summary>
    /// Resource rewards for this exchange
    /// </summary>
    public List<ResourceExchange> Reward { get; init; } = new();
    
    /// <summary>
    /// Base success rate for this exchange (before modifiers)
    /// </summary>
    public int BaseSuccessRate { get; init; } = 90;
    
    /// <summary>
    /// Whether this exchange allows bartering/negotiation
    /// </summary>
    public bool CanBarter { get; init; } = false;
    
    /// <summary>
    /// Template type for generating display text
    /// </summary>
    public CardTemplateType Template { get; init; }
    
    /// <summary>
    /// Check if player can afford this exchange
    /// </summary>
    public bool CanAfford(object playerResources, object tokenManager, object attentionManager)
    {
        // For now, returning true to fix compilation
        // TODO: Implement proper affordability checking
        return true;
    }
    
    /// <summary>
    /// Get narrative context for messages
    /// </summary>
    public string GetNarrativeContext()
    {
        return Template switch
        {
            CardTemplateType.Exchange => "completed an exchange",
            CardTemplateType.SimpleExchange => "made a quick trade",
            _ => "completed a transaction"
        };
    }
}