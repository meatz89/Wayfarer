using System;

/// <summary>
/// Types of conversation cards matching relationship types
/// </summary>
public enum CardType
{
    Trust,
    Commerce,
    Status,
    Shadow
}

/// <summary>
/// Mechanical categories that determine how cards function
/// </summary>
public enum CardCategory
{
    COMFORT,  // Build comfort, can combine with other comfort cards
    STATE,    // Change emotional state, must play alone
    CRISIS,   // Emergency actions, free in DESPERATE state
    EXCHANGE, // Exchange cards for Quick Exchange conversations
    LETTER    // Letter negotiation cards for creating obligations
}

/// <summary>
/// Persistence behavior when LISTEN action taken
/// </summary>
public enum PersistenceType
{
    Persistent,  // Stays in hand when listening
    Opportunity, // Vanishes when listening (fleeting moments)
    OneShot,     // Removed after playing (major confessions)
    Burden,      // Cannot vanish (negative cards)
    Crisis       // Emergency cards, free in DESPERATE state
}

/// <summary>
/// Card power levels for token-based progression
/// </summary>
public enum CardPowerLevel
{
    Basic = 0,       // 0 tokens required
    Intermediate = 3, // 3 tokens required
    Advanced = 5,    // 5 tokens required  
    Master = 10      // 10 tokens required
}

/// <summary>
/// A single conversation card representing something to say or do.
/// Cards are the atomic units of conversation.
/// </summary>
public class ConversationCard
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
    /// Base comfort gained on success
    /// </summary>
    public int BaseComfort { get; init; }

    /// <summary>
    /// Mechanical category determining how card functions
    /// </summary>
    public CardCategory Category { get; init; }

    /// <summary>
    /// State to transition to on success (STATE cards only)
    /// </summary>
    public EmotionalState? SuccessState { get; init; }

    /// <summary>
    /// State to transition to on failure (STATE cards only)
    /// </summary>
    public EmotionalState? FailureState { get; init; }

    /// <summary>
    /// Whether this card came from an observation
    /// </summary>
    public bool IsObservation { get; init; }

    /// <summary>
    /// Source of observation if applicable
    /// </summary>
    public string ObservationSource { get; init; }

    /// <summary>
    /// Whether this card can deliver a letter in the queue
    /// </summary>
    public bool CanDeliverLetter { get; init; }
    
    /// <summary>
    /// The ID of the delivery obligation this card delivers (if CanDeliverLetter is true)
    /// </summary>
    public string DeliveryObligationId { get; init; }

    /// <summary>
    /// Whether this card manipulates obligations
    /// </summary>
    public bool ManipulatesObligations { get; init; }

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
    /// Get effective weight considering state rules
    /// </summary>
    public int GetEffectiveWeight(EmotionalState state)
    {
        var rules = ConversationRules.States[state];
        
        // Check if this card's category is free in this state
        if (rules.FreeWeightCategories != null && rules.FreeWeightCategories.Contains(Category))
            return 0;
            
        return Weight;
    }

    /// <summary>
    /// Get persistence icon for UI display
    /// </summary>

    /// <summary>
    /// Calculate success chance based on weight and tokens
    /// </summary>
    public int CalculateSuccessChance(Dictionary<ConnectionType, int> tokens = null)
    {
        // Use override if specified (for special cards like crisis cards)
        if (SuccessRate.HasValue)
            return SuccessRate.Value;
            
        // Exchange cards always succeed if affordable
        if (Category == CardCategory.EXCHANGE)
            return 100;
            
        var baseChance = 70;
        baseChance -= Weight * 10;
        
        // Apply linear token bonus: +5% per token of matching type
        if (tokens != null)
        {
            var tokenBonus = tokens.GetValueOrDefault(GetConnectionType(), 0) * 5;
            baseChance += tokenBonus;
        }
        
        return Math.Clamp(baseChance, 10, 95);
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
        return Category switch
        {
            CardCategory.COMFORT => "comfort",
            CardCategory.STATE => "state",
            CardCategory.CRISIS => "crisis",
            CardCategory.EXCHANGE => "exchange",
            CardCategory.LETTER => "letter",
            _ => "comfort"
        };
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