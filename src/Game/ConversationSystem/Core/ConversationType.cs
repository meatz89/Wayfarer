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
    /// Deep conversation (3 attention, 12 patience, relationship level 3+)
    /// </summary>
    Deep
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
            ConversationType.Deep => 3,
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
            ConversationType.Deep => 12,
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
            ConversationType.Deep => DeckType.Conversation,
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
            ConversationType.Deep => true,
            _ => false
        };
    }
    
    public static int GetMinimumRelationshipLevel(ConversationType type)
    {
        return type switch
        {
            ConversationType.Deep => 3,
            _ => 0
        };
    }
    
    /// <summary>
    /// Determine which conversation type is available/forced based on game state
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
            
        // Check if Deep conversation is available
        if (playerAttention >= 3 && relationshipLevel >= 3)
            return ConversationType.Deep;
            
        // Default to standard if player has enough attention
        if (playerAttention >= 2)
            return ConversationType.Standard;
            
        // Only exchanges possible with low attention
        return ConversationType.QuickExchange;
    }
}