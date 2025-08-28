using System;
using System.Collections.Generic;

/// <summary>
/// A card that primarily grants tokens on success.
/// These cards build relationship strength with NPCs.
/// </summary>
public class TokenCard : ICard
{
    /// <summary>
    /// Unique identifier for this card
    /// </summary>
    public string Id { get; init; }

    /// <summary>
    /// Card template type for frontend text generation
    /// </summary>
    public CardTemplateType Template { get; init; }
    
    /// <summary>
    /// Context data for template rendering
    /// </summary>
    public CardContext Context { get; init; }

    /// <summary>
    /// Which relationship type this card builds
    /// </summary>
    public CardType Type { get; init; }

    /// <summary>
    /// How the card behaves when not played
    /// </summary>
    public PersistenceType Persistence { get; init; }

    /// <summary>
    /// Emotional bandwidth required (0-3)
    /// </summary>
    public int Weight { get; init; }

    /// <summary>
    /// Base comfort gained on success (usually minimal for token cards)
    /// </summary>
    public int BaseComfort { get; init; }

    /// <summary>
    /// Card depth/power level (0-20, deeper cards require more comfort)
    /// </summary>
    public int Depth { get; init; }

    /// <summary>
    /// Override success rate for special cards (null uses calculated rate)
    /// </summary>
    public int? SuccessRate { get; init; }

    /// <summary>
    /// Display name for special cards (null uses template-generated name)
    /// </summary>
    public string DisplayName { get; init; }

    /// <summary>
    /// Description for special cards
    /// </summary>
    public string Description { get; init; }
    
    /// <summary>
    /// Power level determines token requirement for unlocking
    /// </summary>
    public CardPowerLevel PowerLevel { get; init; } = CardPowerLevel.Basic;
    
    /// <summary>
    /// Minimum tokens of matching type required to unlock this card
    /// </summary>
    public int RequiredTokens => (int)PowerLevel;

    /// <summary>
    /// Number of tokens this card grants on success (default 1)
    /// </summary>
    public int TokensGranted { get; init; } = 1;

    /// <summary>
    /// Get effective weight considering state rules
    /// </summary>
    public int GetEffectiveWeight(EmotionalState state)
    {
        var rules = ConversationRules.States[state];
        
        // Check if token cards are free in this state
        if (rules.FreeWeightCategories != null && rules.FreeWeightCategories.Contains(CardCategory.TOKEN))
            return 0;
            
        return Weight;
    }

    /// <summary>
    /// Calculate success chance based on weight and tokens
    /// </summary>
    public int CalculateSuccessChance(Dictionary<ConnectionType, int> tokens = null)
    {
        // Use override if specified (for special cards)
        if (SuccessRate.HasValue)
            return SuccessRate.Value;
            
        var baseChance = 70;
        baseChance -= Weight * 10;
        
        // Apply linear token bonus: +5% per token for cards
        if (tokens != null)
        {
            var tokenCount = tokens.GetValueOrDefault(GetConnectionType(), 0);
            var bonusPerToken = 5;
            var tokenBonus = tokenCount * bonusPerToken;
            baseChance += tokenBonus;
        }
        
        return Math.Clamp(baseChance, 5, 95);
    }

    /// <summary>
    /// Get the connection type this card builds
    /// </summary>
    public ConnectionType GetConnectionType()
    {
        return Type switch
        {
            CardType.Trust => ConnectionType.Trust,
            CardType.Commerce => ConnectionType.Commerce,
            CardType.Status => ConnectionType.Status,
            CardType.Shadow => ConnectionType.Shadow,
            _ => ConnectionType.Trust
        };
    }

    /// <summary>
    /// Get CSS class for card category
    /// </summary>
    public string GetCategoryClass()
    {
        return "token";
    }
    
    /// <summary>
    /// Check if player has enough tokens to unlock this card
    /// </summary>
    public bool IsUnlocked(Dictionary<ConnectionType, int> tokens)
    {
        if (tokens == null) return PowerLevel == CardPowerLevel.Basic;
        
        var relevantTokens = tokens.GetValueOrDefault(GetConnectionType(), 0);
        return relevantTokens >= RequiredTokens;
    }
}