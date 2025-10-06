using System.Collections.Generic;

/// <summary>
/// DTO for loading PhysicalChallengeDeck from JSON
/// </summary>
public class PhysicalChallengeDeckDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public List<string> CardIds { get; set; } = new List<string>();

    public PhysicalChallengeDeck ToDomain()
    {
        return new PhysicalChallengeDeck
        {
            Id = Id,
            Name = Name,
            CardIds = CardIds ?? new List<string>()
        };
    }
}
