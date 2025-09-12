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
    /// Current status of the request
    /// </summary>
    public RequestStatus Status { get; set; } = RequestStatus.Available;
    
    /// <summary>
    /// Request cards with different rapport thresholds offering varying rewards
    /// Each card represents a different level of commitment to the request
    /// </summary>
    public List<ConversationCard> RequestCards { get; set; } = new List<ConversationCard>();
    
    /// <summary>
    /// Promise cards that force queue position 1 and burn tokens for instant rapport
    /// These represent immediate action at the cost of other relationships
    /// </summary>
    public List<ConversationCard> PromiseCards { get; set; } = new List<ConversationCard>();
    
    /// <summary>
    /// Check if this request is available to attempt
    /// </summary>
    public bool IsAvailable() => Status == RequestStatus.Available;
    
    /// <summary>
    /// Mark this request as completed
    /// </summary>
    public void Complete()
    {
        Status = RequestStatus.Completed;
    }
    
    /// <summary>
    /// Get all cards associated with this request (both request and promise cards)
    /// </summary>
    public List<ConversationCard> GetAllCards()
    {
        var allCards = new List<ConversationCard>();
        allCards.AddRange(RequestCards);
        allCards.AddRange(PromiseCards);
        return allCards;
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