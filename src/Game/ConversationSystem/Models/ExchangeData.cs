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
    /// Template ID for generating display text (e.g., "food_exchange", "healing_exchange")
    /// </summary>
    public string TemplateId { get; init; }
    
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
    /// Get narrative context for messages based on exchange type
    /// </summary>
    public string GetNarrativeContext()
    {
        // Determine exchange type from reward resources
        var rewardTypes = Reward?.Select(r => r.ResourceType).ToHashSet() ?? new HashSet<ResourceType>();
        
        if (rewardTypes.Contains(ResourceType.Hunger))
            return "made a food exchange";
        if (rewardTypes.Contains(ResourceType.Health))
            return "received healing";
        if (rewardTypes.Any(r => r == ResourceType.Coins))
            return "completed a trade";
            
        return "completed a transaction";
    }
}