using System.Collections.Generic;

/// <summary>
/// Represents a one-time request from an NPC with multiple cards at different rapport thresholds
/// </summary>
public class NPCRequest
{
    /// <summary>
    /// Unique identifier for the request
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Display name for the request
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Narrative description of what the NPC is asking
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The text displayed when the NPC presents this request (shown on LISTEN action)
    /// </summary>
    public string NpcRequestText { get; set; }

    /// <summary>
    /// The conversation type ID this request uses (e.g., "desperate_request", "trade_negotiation")
    /// </summary>
    public string ConversationTypeId { get; set; }

    /// <summary>
    /// Category that must match the conversation type's category
    /// </summary>
    public string Category { get; set; }

    /// <summary>
    /// Connection type (token type) for this request
    /// </summary>
    public ConnectionType ConnectionType { get; set; } = ConnectionType.Trust;

    /// <summary>
    /// Current status of the request
    /// </summary>
    public RequestStatus Status { get; set; } = RequestStatus.Available;

    /// <summary>
    /// Request card IDs with different rapport thresholds offering varying rewards
    /// Each card represents a different level of commitment to the request
    /// Cards are stored by ID only - actual cards are in GameWorld.AllCardDefinitions
    /// </summary>
    public List<string> RequestCardIds { get; set; } = new List<string>();

    /// <summary>
    /// Promise card IDs that force queue position 1 and burn tokens for instant rapport
    /// These represent immediate action at the cost of other relationships
    /// Cards are stored by ID only - actual cards are in GameWorld.AllCardDefinitions
    /// </summary>
    public List<string> PromiseCardIds { get; set; } = new List<string>();

    /// <summary>
    /// Check if this request is available to attempt
    /// </summary>
    public bool IsAvailable()
    {
        return Status == RequestStatus.Available;
    }

    /// <summary>
    /// Mark this request as completed
    /// </summary>
    public void Complete()
    {
        Status = RequestStatus.Completed;
    }

    /// <summary>
    /// Get all cards associated with this request (both request and promise cards)
    /// Resolves card IDs to actual cards from GameWorld
    /// </summary>
    public List<ConversationCard> GetAllCards(GameWorld gameWorld)
    {
        List<ConversationCard> allCards = new List<ConversationCard>();

        // Resolve request card IDs
        foreach (string cardId in RequestCardIds)
        {
            if (gameWorld.AllCardDefinitions.TryGetValue(cardId, out ConversationCard? card))
                allCards.Add(card);
        }

        // Resolve promise card IDs
        foreach (string cardId in PromiseCardIds)
        {
            if (gameWorld.AllCardDefinitions.TryGetValue(cardId, out ConversationCard? card))
                allCards.Add(card);
        }

        return allCards;
    }

    /// <summary>
    /// Get request cards by resolving IDs from GameWorld
    /// </summary>
    public List<ConversationCard> GetRequestCards(GameWorld gameWorld)
    {
        List<ConversationCard> cards = new List<ConversationCard>();
        foreach (string cardId in RequestCardIds)
        {
            if (gameWorld.AllCardDefinitions.TryGetValue(cardId, out ConversationCard? card))
                cards.Add(card);
        }
        return cards;
    }

    /// <summary>
    /// Get promise cards by resolving IDs from GameWorld
    /// </summary>
    public List<ConversationCard> GetPromiseCards(GameWorld gameWorld)
    {
        List<ConversationCard> cards = new List<ConversationCard>();
        foreach (string cardId in PromiseCardIds)
        {
            if (gameWorld.AllCardDefinitions.TryGetValue(cardId, out ConversationCard? card))
                cards.Add(card);
        }
        return cards;
    }
}

/// <summary>
/// Status of an NPC request
/// </summary>
public enum RequestStatus
{
    /// <summary>
    /// Request is available to attempt
    /// </summary>
    Available,

    /// <summary>
    /// Request has been completed (any card was successfully played)
    /// </summary>
    Completed,

    /// <summary>
    /// Request was failed and is no longer available
    /// </summary>
    Failed
}