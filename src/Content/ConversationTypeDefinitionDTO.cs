using System.Collections.Generic;

/// <summary>
/// DTO for conversation type definitions loaded from JSON
/// </summary>
public class ConversationTypeDefinitionDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string DeckId { get; set; }
    public string Category { get; set; }
    public int AttentionCost { get; set; } = 1;
    public List<string> AvailableTimeBlocks { get; set; } = new List<string>();
}