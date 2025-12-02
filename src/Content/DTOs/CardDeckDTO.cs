/// <summary>
/// DTO for Conversation system card deck definitions loaded from JSON
/// Each deck contains card IDs with their counts
/// SEPARATE from EngagementTypeDeck (which is for Mental/Physical/Social tactical systems)
/// </summary>
public class CardDeckDTO
{
    public string Id { get; set; }
    public List<CardCountEntry> CardCounts { get; set; } = new List<CardCountEntry>();
}
