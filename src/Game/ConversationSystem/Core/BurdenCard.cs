using System;
using System.Collections.Generic;

/// <summary>
/// A card that blocks hand slots until resolved.
/// These cards represent past mistakes or unresolved issues that must be addressed.
/// </summary>
public class BurdenCard : ICard
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
    /// Which relationship type this card affects (usually negative)
    /// </summary>
    public CardType Type { get; init; }

    /// <summary>
    /// Burden cards always have Burden persistence
    /// </summary>
    public PersistenceType Persistence => PersistenceType.Opportunity;

    /// <summary>
    /// Emotional bandwidth consumed (usually high for burden cards)
    /// </summary>
    public int Weight { get; init; }

    /// <summary>
    /// Base comfort gained on success (usually negative or zero)
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
    /// Number of hand slots this burden blocks (default 1)
    /// </summary>
    public int HandSlotsBlocked { get; init; } = 1;

    /// <summary>
    /// Whether this burden can be resolved through conversation
    /// </summary>
    public bool CanBeResolved { get; init; } = true;

    /// <summary>
    /// Specific resolution type required (if any)
    /// </summary>
    public string ResolutionType { get; init; }

    /// <summary>
    /// Get effective weight considering state rules
    /// </summary>
    public int GetEffectiveWeight(EmotionalState state)
    {
        var rules = ConversationRules.States[state];
        
        // Check if burden cards are free in this state
        if (rules.FreeWeightCardTypes != null && rules.FreeWeightCardTypes.Contains(typeof(BurdenCard)))
            return 0;
            
        return Weight;
    }

    /// <summary>
    /// Calculate success chance based on weight and tokens
    /// Burden cards typically have lower success rates
    /// </summary>
    public int CalculateSuccessChance(Dictionary<ConnectionType, int> tokens = null)
    {
        // Use override if specified (for special cards)
        if (SuccessRate.HasValue)
            return SuccessRate.Value;
            
        // Burden cards start with lower base chance
        var baseChance = 50;
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
    /// Get the connection type this card affects
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
        return "burden";
    }
    

    /// <summary>
    /// Check if this burden can be resolved by a specific resolution type
    /// </summary>
    public bool CanBeResolvedBy(string resolutionType)
    {
        if (!CanBeResolved) return false;
        if (string.IsNullOrEmpty(ResolutionType)) return true; // Generic resolution
        return string.Equals(ResolutionType, resolutionType, StringComparison.OrdinalIgnoreCase);
    }
}