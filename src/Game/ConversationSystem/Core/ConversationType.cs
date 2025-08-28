using System;

/// <summary>
/// Types of conversations available in the game.
/// Each type has different attention costs, patience, and mechanics.
/// </summary>
public enum ConversationType
{
    /// <summary>
    /// Quick resource exchange (0 attention, no patience, instant)
    /// </summary>
    QuickExchange,
    
    /// <summary>
    /// Crisis resolution (1 attention, 3 patience, crisis deck only)
    /// </summary>
    Crisis,
    
    /// <summary>
    /// Standard conversation (2 attention, 8 patience, full mechanics)
    /// </summary>
    Standard,
    
    /// <summary>
    /// Letter offer conversation (2 attention, 6 patience, letter deck cards)
    /// Enabled when NPC has letter cards in their letter deck
    /// </summary>
    LetterOffer,
    
    /// <summary>
    /// Make amends conversation (2 attention, 6 patience, burden resolution)
    /// Enabled when NPC has 2+ burden cards in their conversation deck
    /// </summary>
    MakeAmends,
}

/// <summary>
/// Deck types that NPCs can have
/// </summary>
public enum DeckType
{
    /// <summary>
    /// Exchange deck for quick trades
    /// </summary>
    Exchange,
    
    /// <summary>
    /// Conversation deck for normal interactions
    /// </summary>
    Conversation,
    
    /// <summary>
    /// Crisis deck for urgent situations
    /// </summary>
    Crisis
}

/// <summary>
/// Configuration for each conversation type
/// </summary>
public static class ConversationTypeConfig
{
    public static int GetAttentionCost(ConversationType type)
    {
        return type switch
        {
            ConversationType.QuickExchange => 0,
            ConversationType.Crisis => 1,
            ConversationType.Standard => 2,
            ConversationType.LetterOffer => 2,
            ConversationType.MakeAmends => 2,
            _ => 2
        };
    }
    
    public static int GetBasePatience(ConversationType type)
    {
        return type switch
        {
            ConversationType.QuickExchange => 0,  // No patience for exchanges
            ConversationType.Crisis => 3,
            ConversationType.Standard => 8,
            ConversationType.LetterOffer => 6,  // Focused on letter negotiation
            ConversationType.MakeAmends => 6,    // Focused on burden resolution
            _ => 8
        };
    }
    
    public static DeckType GetDeckType(ConversationType type)
    {
        return type switch
        {
            ConversationType.QuickExchange => DeckType.Exchange,
            ConversationType.Crisis => DeckType.Crisis,
            ConversationType.Standard => DeckType.Conversation,
            ConversationType.LetterOffer => DeckType.Conversation,  // Uses conversation deck with letter cards mixed in
            ConversationType.MakeAmends => DeckType.Conversation,   // Uses conversation deck focused on burdens
            _ => DeckType.Conversation
        };
    }
    
    public static bool RequiresEmotionalStates(ConversationType type)
    {
        return type switch
        {
            ConversationType.QuickExchange => false,
            _ => true
        };
    }
    
    public static bool CanGenerateLetters(ConversationType type)
    {
        return type switch
        {
            ConversationType.QuickExchange => false,
            ConversationType.Crisis => false,  // Crisis resolves issues, doesn't generate letters
            ConversationType.Standard => true,
            ConversationType.LetterOffer => true,  // Primary purpose is letter generation
            ConversationType.MakeAmends => false,   // Resolves burdens, doesn't generate letters
            _ => false
        };
    }
    
    /// <summary>
    /// Determine which conversation type is available/forced based on game state
    /// Note: This method is deprecated - use ConversationManager.GetAvailableConversationTypes instead
    /// which properly analyzes deck contents
    /// </summary>
    public static ConversationType DetermineAvailableType(
        bool hasCrisisCards,
        int relationshipLevel,
        bool playerRequestedExchange,
        int playerAttention)
    {
        // Crisis takes absolute priority
        if (hasCrisisCards)
            return ConversationType.Crisis;
            
        // Player explicitly requested exchange
        if (playerRequestedExchange)
            return ConversationType.QuickExchange;
            
        // Default to standard if player has enough attention
        if (playerAttention >= 2)
            return ConversationType.Standard;
            
        // Only exchanges possible with low attention
        return ConversationType.QuickExchange;
    }
}