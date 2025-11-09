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

// ObservationDeck system eliminated - replaced by transparent resource competition

public PlayerDeckDefinitionDTO()
{
    ConversationDeck = new Dictionary<string, int>();
    // ObservationDeck system eliminated
}
}

/// <summary>
/// Defines the composition of an NPC's deck
/// </summary>
public class NPCDeckDefinitionDTO
{
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
    RequestDeck = new Dictionary<string, int>();
    ExchangeDeck = new Dictionary<string, int>();
}
}

