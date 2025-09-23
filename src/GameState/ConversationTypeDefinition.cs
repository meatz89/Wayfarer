using System.Collections.Generic;

/// <summary>
/// Defines a conversation type loaded from JSON
/// Fully extensible without code changes
/// </summary>
public class ConversationTypeDefinition
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string DeckId { get; set; }
    public string Category { get; set; }  // Links requests to conversation types
    public List<string> AvailableTimeBlocks { get; set; } = new();
    public int DoubtPerListen { get; set; } = 0;
    public bool MomentumErosion { get; set; } = false;
    public int MaxDoubt { get; set; } = 10;
}

/// <summary>
/// A collection of card IDs that form a deck definition
/// Reusable across multiple conversation types
/// </summary>
public class CardDeckDefinition
{
    public string Id { get; set; }
    public List<string> CardIds { get; set; } = new();
}