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
    /// Standard conversation (2 attention, 8 patience, full mechanics)
    /// </summary>
    Standard,
    
    /// <summary>
    /// Letter offer conversation (2 attention, 6 patience, comfort, tokens and promise card)
    /// Enabled when NPC has letter cards in their Goal deck
    /// </summary>
    LetterOffer,

    /// <summary>
    /// Letter offer conversation (2 attention, 6 patience, comfort, tokens and letter delivery card)
    /// Enabled when Player has letter for the NPC recipient in his Obligation Queue 
    /// </summary>
    LetterDelivery,

    /// <summary>
    /// Make amends conversation (2 attention, 6 patience, comfort, tokens and burden resolution card)
    /// Enabled when NPC has 1+ burden cards in their conversation deck
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
    /// Goal Decks for letters, promises and burdens
    /// </summary>
    Goal
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
            ConversationType.LetterDelivery => 1,
            ConversationType.Standard => 2,
            ConversationType.LetterOffer => 2,
            ConversationType.MakeAmends => 2,
            _ => 2
        };
    }
    
    /// <summary>
    /// Determine which conversation type is available/forced based on game state
    /// Note: This method is deprecated - use ConversationManager.GetAvailableConversationTypes instead
    /// which properly analyzes deck contents
    /// </summary>
    public static ConversationType DetermineAvailableType(
        bool burden,
        bool letterDelivery,
        bool letterOffer,
        int relationshipLevel,
        bool playerRequestedExchange,
        int playerAttention)
    {
        // Burdens take absolute priority
        if (burden)
            return ConversationType.MakeAmends;

        // Letter delivery from player to npc recipient
        if (letterDelivery)
            return ConversationType.LetterDelivery;

        // Letter offers from npc sender to player
        if (letterOffer)
            return ConversationType.LetterOffer;

        // Player explicitly requested exchange
        if (playerRequestedExchange)
            return ConversationType.QuickExchange;
            
        // Default to standard if player has enough attention
        if (playerAttention >= 2)
            return ConversationType.Standard;
            
        throw new Exception("No valid conversation type selected");
    }
}