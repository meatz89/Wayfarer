using System.Collections.Generic;

/// <summary>
/// Defines the card composition for NPC decks
/// </summary>
public class DeckCompositionDTO
{
    /// <summary>
    /// Default deck composition for all NPCs
    /// </summary>
    public DeckDefinitionDTO DefaultDeck { get; set; }

    /// <summary>
    /// NPC-specific deck overrides (key is NPC ID)
    /// </summary>
    public Dictionary<string, DeckDefinitionDTO> NpcDecks { get; set; }
}

/// <summary>
/// Defines the composition of a single deck
/// </summary>
public class DeckDefinitionDTO
{
    /// <summary>
    /// Conversation deck card composition (card ID -> count)
    /// </summary>
    public Dictionary<string, int> ConversationDeck { get; set; }

    /// <summary>
    /// Progression deck card composition (card ID -> count)
    /// NPC-specific cards that unlock at token thresholds
    /// </summary>
    public Dictionary<string, int> ProgressionDeck { get; set; }

    /// <summary>
    /// Request deck card composition (card ID -> count)
    /// Used to load request/promise cards that define conversation options
    /// </summary>
    public Dictionary<string, int> RequestDeck { get; set; }

    /// <summary>
    /// Exchange deck card composition (card ID -> count)
    /// </summary>
    public Dictionary<string, int> ExchangeDeck { get; set; }

    public DeckDefinitionDTO()
    {
        ConversationDeck = new Dictionary<string, int>();
        ProgressionDeck = new Dictionary<string, int>();
        RequestDeck = new Dictionary<string, int>();
        ExchangeDeck = new Dictionary<string, int>();
    }
}