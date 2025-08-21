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
    CRISIS    // Emergency actions, free in DESPERATE state
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
    /// Whether this card manipulates obligations
    /// </summary>
    public bool ManipulatesObligations { get; init; }

    /// <summary>
    /// Get effective weight considering state
    /// </summary>
    public int GetEffectiveWeight(EmotionalState state)
    {
        // Crisis cards are free in DESPERATE state
        if (Category == CardCategory.CRISIS && state == EmotionalState.DESPERATE)
            return 0;
        return Weight;
    }

    /// <summary>
    /// Get persistence icon for UI display
    /// </summary>
    public string GetPersistenceIcon()
    {
        return Persistence switch
        {
            PersistenceType.Persistent => "♻",
            PersistenceType.Opportunity => "⏱",
            PersistenceType.OneShot => "💠",
            PersistenceType.Burden => "⚠",
            PersistenceType.Crisis => "🔥",
            _ => ""
        };
    }

    /// <summary>
    /// Calculate success chance based on weight and tokens
    /// </summary>
    public int CalculateSuccessChance(int statusTokens)
    {
        var baseChance = 70;
        baseChance -= Weight * 10;
        baseChance += statusTokens * 3;
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
            _ => "comfort"
        };
    }
}