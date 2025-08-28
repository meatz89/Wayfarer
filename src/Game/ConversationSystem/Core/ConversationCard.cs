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
/// Types of goal cards that can be shuffled into conversation deck
/// </summary>
public enum GoalType
{
    Letter,     // Creates delivery obligation (Elena's main use case)
    Promise,    // Creates meeting/escort/investigation obligation
    Resolution, // Removes burden cards from deck
    Commerce,   // Enables special trades
    Crisis      // Resolves emergency situations
}

/// <summary>
/// Mechanical categories that determine how cards function
/// Cards have categories based on what they DO, not where they come from
/// </summary>
public enum CardCategory
{
    COMFORT,    // Modify comfort value only
    TOKEN,      // Add 1 token of specific type only  
    STATE,      // Change emotional state only
    BURDEN,     // Block hand slots until resolved
    OBSERVATION, // State change cards gained from world (not deck)
    CRISIS,     // Emergency cards with high stakes
    PROMISE,    // Create obligations (includes letters and goals)
    EXCHANGE    // Simple resource trades (mercantile NPCs only)
}

/// <summary>
/// Persistence behavior when LISTEN action taken
/// </summary>
public enum PersistenceType
{
    Persistent,  // Stays in hand when listening
    Fleeting,    // Vanishes when listening (fleeting moments)
    Opportunity, // Removed from deck when discarded
    Burden,      // Stays in deck even when discarded, only removed by special mechanics
    Crisis       // Crisis cards that must be resolved
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
    /// Whether this is a state transition card
    /// </summary>
    public bool IsStateCard { get; init; }
    
    /// <summary>
    /// Whether this card grants a token on success
    /// </summary>
    public bool GrantsToken { get; init; }

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
    /// Whether this is a goal card that was shuffled into the deck
    /// </summary>
    public bool IsGoalCard { get; init; }
    
    /// <summary>
    /// The type of goal this card represents (if IsGoalCard is true)
    /// </summary>
    public GoalType? GoalCardType { get; init; }
    
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
        // Use override if specified (for special cards like Crisis letters)
        if (SuccessRate.HasValue)
            return SuccessRate.Value;
            
        // Exchange cards always succeed if affordable
        if (Category == CardCategory.EXCHANGE)
            return 100;
            
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
        return Category switch
        {
            CardCategory.COMFORT => "comfort",
            CardCategory.TOKEN => "token",
            CardCategory.STATE => "state",
            CardCategory.BURDEN => "burden",
            CardCategory.OBSERVATION => "observation",
            CardCategory.CRISIS => "crisis",
            CardCategory.PROMISE => "promise",
            CardCategory.EXCHANGE => "exchange",
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