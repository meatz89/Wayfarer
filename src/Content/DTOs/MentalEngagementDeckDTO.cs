using System.Collections.Generic;

/// <summary>
/// DTO for loading MentalEngagementDeck from JSON
/// </summary>
public class MentalEngagementDeckDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public List<string> CardIds { get; set; } = new List<string>();

    public MentalEngagementDeck ToDomain()
    {
        return new MentalEngagementDeck
        {
            Id = Id,
            Name = Name,
            CardIds = CardIds ?? new List<string>()
        };
    }
}
