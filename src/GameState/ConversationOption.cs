using System;

/// <summary>
/// Represents a specific conversation option that maps to a goal card from the NPC's requests.
/// Each conversation option corresponds to exactly one goal card.
/// </summary>
public class ConversationOption
{
    /// <summary>
    /// The type of conversation (Promise, FriendlyChat, Resolution, etc.)
    /// </summary>
    public ConversationType Type { get; set; }

    /// <summary>
    /// The specific card ID from the NPC's requests that will be used as the goal card
    /// </summary>
    public string GoalCardId { get; set; }

    /// <summary>
    /// Display name for the UI (e.g., "Letter: Marriage Refusal", "Request: Trade Agreement")
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    /// Full description of what this conversation option entails
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The token type associated with this goal card
    /// </summary>
    public ConnectionType TokenType { get; set; }

    /// <summary>
    /// The rapport threshold required to play this goal card
    /// </summary>
    public int RapportThreshold { get; set; }

    /// <summary>
    /// Whether this is a special type of card (Letter, Promise, etc.)
    /// </summary>
    public CardType CardType { get; set; }
}