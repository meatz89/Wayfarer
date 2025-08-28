using System;


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
            ConversationType.Commerce => 0,
            ConversationType.Delivery => 1,
            ConversationType.FriendlyChat => 2,
            ConversationType.Promise => 2,
            ConversationType.Resolution => 2,
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
            return ConversationType.Resolution;

        // Letter delivery from player to npc recipient
        if (letterDelivery)
            return ConversationType.Delivery;

        // Letter offers from npc sender to player
        if (letterOffer)
            return ConversationType.Promise;

        // Player explicitly requested exchange
        if (playerRequestedExchange)
            return ConversationType.Commerce;
            
        // Default to standard if player has enough attention
        if (playerAttention >= 2)
            return ConversationType.FriendlyChat;
            
        throw new Exception("No valid conversation type selected");
    }
}