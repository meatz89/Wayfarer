using System.Collections.Generic;

/// <summary>
/// DTO for loading MentalChallengeDeck from JSON
/// </summary>
public class MentalChallengeDeckDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public List<string> CardIds { get; set; } = new List<string>();

    public MentalChallengeDeck ToDomain()
    {
        return new MentalChallengeDeck
        {
            Id = Id,
            Name = Name,
            CardIds = CardIds ?? new List<string>()
        };
    }
}
