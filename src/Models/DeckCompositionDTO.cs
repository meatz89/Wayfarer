/// <summary>
/// Defines the card composition for NPC decks
/// DOMAIN COLLECTION PRINCIPLE: All Dictionary patterns replaced with List<Entry>
/// </summary>
public class DeckCompositionDTO
{
    /// <summary>
    /// Player starter deck composition
    /// </summary>
    public PlayerDeckDefinitionDTO PlayerStarterDeck { get; set; }

    /// <summary>
    /// NPC-specific deck compositions
    /// DOMAIN COLLECTION PRINCIPLE: List of objects instead of Dictionary
    /// </summary>
    public List<NpcDeckEntry> NpcDecks { get; set; } = new List<NpcDeckEntry>();
}

/// <summary>
/// Defines the composition of the player's starter deck
/// </summary>
public class PlayerDeckDefinitionDTO
{
    /// <summary>
    /// Player's conversation deck card composition
    /// DOMAIN COLLECTION PRINCIPLE: List of objects instead of Dictionary
    /// </summary>
    public List<CardCountEntry> ConversationDeck { get; set; } = new List<CardCountEntry>();
}

