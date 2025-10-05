using System.Collections.Generic;

/// <summary>
/// DTO for Conversation system card deck definitions loaded from JSON
/// Each deck contains card IDs with their counts
/// SEPARATE from EngagementTypeDeck (which is for Mental/Physical/Social tactical systems)
/// </summary>
public class CardDeckDTO
{
    public string Id { get; set; }
    public Dictionary<string, int> CardCounts { get; set; } = new Dictionary<string, int>();
}
