using System.Collections.Generic;

/// <summary>
/// DTO for loading SocialChallengeDeck from JSON
/// </summary>
public class SocialChallengeDeckDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public List<string> CardIds { get; set; } = new List<string>();

    public SocialChallengeDeck ToDomain()
    {
        return new SocialChallengeDeck
        {
            Id = Id,
            Name = Name,
            CardIds = CardIds ?? new List<string>()
        };
    }
}
