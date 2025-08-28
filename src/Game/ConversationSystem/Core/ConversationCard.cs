using System;

/// <summary>
/// Template types for generating card dialogue and display
/// </summary>
public enum CardTemplateType
{
    // Basic conversation templates
    SimpleGreeting,
    ActiveListening,
    CasualInquiry,
    
    // Relationship building
    OfferHelp,
    OpeningUp,
    ShareInformation,
    MakePromise,
    
    // Commerce/Business
    DiscussBusiness,
    OfferWork,
    Exchange,
    SimpleExchange,
    
    // Status/Politics
    DiscussPolitics,
    ShowRespect,
    
    // Shadow/Secret
    ImplyKnowledge,
    RequestDiscretion,
    
    // Special situations
    DesperateRequest,
    UrgentPlea,
    CalmnessAttempt,
    DeliverLetter,
    DiscussObligation,
    StateTransition,
    
    // Observation
    ObservationNotice,
    
    // Missing values found in code
    GoalCard,
    ObservationShare,
    OfferAssistance,
    PersonalStory,
    CalmingResponse,
    NegotiateDeadline,
    ShareSecret,
    ShowingTension,
    ExpressEmpathy,
    SharePersonal,
    ProposeDeal,
    NegotiateTerms,
    AcknowledgePosition,
    MentionLetter,
    ShareUrgentNews,
    MentionObservation
}

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
/// Categories of conversation cards for UI styling and game mechanics
/// </summary>
public enum CardCategory
{
    Comfort,    // Basic conversation cards that build comfort
    Token,      // Cards that grant connection tokens
    State,      // Cards that change NPC emotional state
    Burden,     // Negative cards that require resolution
    Promise,    // Goal cards for letter offers
    Exchange    // Commerce cards for quick trades
}

/// <summary>
/// Types of conversations available in the game.
/// Each type has different attention costs, patience, and mechanics.
/// </summary>
public enum ConversationType
{
    /// <summary>
    /// Quick resource exchange (0 attention, no patience, instant)
    /// </summary>
    Commerce,

    /// <summary>
    /// Trust building conversation (2 attention, 8 patience, full mechanics)
    /// </summary>
    FriendlyChat,

    /// <summary>
    /// Letter offer conversation (2 attention, 6 patience, comfort, tokens and promise card)
    /// Enabled when NPC has letter cards in their Goal deck
    /// </summary>
    Promise,

    /// <summary>
    /// Letter delivery conversation (2 attention, 6 patience, comfort, tokens and letter delivery card)
    /// Enabled when Player has letter for the NPC recipient in his Obligation Queue 
    /// </summary>
    Delivery,

    /// <summary>
    /// Make amends conversation (2 attention, 6 patience, comfort, tokens and burden resolution card)
    /// Enabled when NPC has 1+ burden cards in their conversation deck
    /// </summary>
    Resolution,

    /// <summary>
    /// Crisis conversation (1 attention, 3 patience, forced resolution)
    /// Forced when NPC has crisis cards in their goal deck
    /// </summary>
    Crisis,
}


/// <summary>
/// Persistence behavior when LISTEN action taken
/// </summary>
public enum PersistenceType
{
    Persistent,  // Stays in hand when listening
    Fleeting,    // Vanishes when listening (fleeting moments)
    Opportunity, // Removed from deck when discarded
    Goal         // Goal cards must be played, must be played in same turn, ends conversation
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
    /// Category of this card for game mechanics and UI styling
    /// </summary>
    public CardCategory Category { get; init; }
    
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
    /// Whether this card is an ExchangeCard
    /// </summary>
    public bool IsExchange { get; init; }

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
    public ConversationType? GoalCardType { get; init; }

    /// <summary>
    /// Get effective weight considering state rules
    /// </summary>
    public int GetEffectiveWeight(EmotionalState state)
    {
        var rules = ConversationRules.States[state];
        
        // Check if this card category gets free weight
        if (rules.FreeWeightCardCategories != null && rules.FreeWeightCardCategories.Contains(Category))
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
        return Category.ToString().ToLower();
    }
    
}