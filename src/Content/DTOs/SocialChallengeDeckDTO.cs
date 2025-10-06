using System.Collections.Generic;

/// <summary>
/// DTO for loading ConversationEngagementDeck from JSON
/// </summary>
public class ConversationEngagementDeckDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public List<string> CardIds { get; set; } = new List<string>();

    public ConversationEngagementDeck ToDomain()
    {
        return new ConversationEngagementDeck
        {
            Id = Id,
            Name = Name,
            CardIds = CardIds ?? new List<string>()
        };
    }
}
