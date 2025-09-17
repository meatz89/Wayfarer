using System.Collections.Generic;

/// <summary>
/// Defines the card composition for NPC decks
/// </summary>
public class DeckCompositionDTO
{
    /// <summary>
    /// Player starter deck composition
    /// </summary>
    public PlayerDeckDefinitionDTO PlayerStarterDeck { get; set; }

    /// <summary>
    /// NPC-specific deck compositions (key is NPC ID)
    /// </summary>
    public Dictionary<string, NPCDeckDefinitionDTO> NpcDecks { get; set; }
}

/// <summary>
/// Defines the composition of the player's starter deck
/// </summary>
public class PlayerDeckDefinitionDTO
{
    /// <summary>
    /// Player's conversation deck card composition (card ID -> count)
    /// </summary>
    public Dictionary<string, int> ConversationDeck { get; set; }

    /// <summary>
    /// Player's starting observation cards (card ID -> count)
    /// </summary>
    public Dictionary<string, int> ObservationDeck { get; set; }

    public PlayerDeckDefinitionDTO()
    {
        ConversationDeck = new Dictionary<string, int>();
        ObservationDeck = new Dictionary<string, int>();
    }
}

/// <summary>
/// Defines the composition of an NPC's deck
/// </summary>
public class NPCDeckDefinitionDTO
{
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

    public NPCDeckDefinitionDTO()
    {
        ProgressionDeck = new Dictionary<string, int>();
        RequestDeck = new Dictionary<string, int>();
        ExchangeDeck = new Dictionary<string, int>();
    }
}

// Keep old class for backwards compatibility temporarily
public class DeckDefinitionDTO : NPCDeckDefinitionDTO
{
    [Obsolete("Use NPCDeckDefinitionDTO instead")]
    public Dictionary<string, int> ConversationDeck { get; set; }
}