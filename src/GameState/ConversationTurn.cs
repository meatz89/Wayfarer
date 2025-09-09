using System;

/// <summary>
/// Represents a single turn in a conversation, containing all data from that turn
/// </summary>
public class ConversationTurn
{
    /// <summary>
    /// The type of action taken (LISTEN or SPEAK)
    /// </summary>
    public ActionType ActionType { get; init; }
    
    /// <summary>
    /// The narrative output generated for this turn
    /// Contains NPC dialogue, card narratives, and environmental text
    /// </summary>
    public NarrativeOutput Narrative { get; init; }
    
    /// <summary>
    /// The result of the conversation turn
    /// Contains state changes, flow changes, cards played, etc.
    /// </summary>
    public ConversationTurnResult Result { get; init; }
    
    /// <summary>
    /// When this turn occurred
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    
    /// <summary>
    /// The turn number in the conversation
    /// </summary>
    public int TurnNumber { get; init; }
    
    /// <summary>
    /// Cards played during this turn (for SPEAK actions)
    /// </summary>
    public CardInstance CardPlayed { get; init; }
}