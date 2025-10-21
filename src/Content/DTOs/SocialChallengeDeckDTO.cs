using System.Collections.Generic;

/// <summary>
/// DTO for loading SocialChallengeDeck from JSON
/// </summary>
public class SocialChallengeDeckDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int DangerThreshold { get; set; }
    public int InitialHandSize { get; set; }
    public int MaxHandSize { get; set; }
    public List<string> CardIds { get; set; } = new List<string>();

    public SocialChallengeDeck ToDomain()
    {
        return new SocialChallengeDeck
        {
            Id = Id,
            Name = Name,
            Description = Description,
            DangerThreshold = DangerThreshold,
            InitialHandSize = InitialHandSize,
            MaxHandSize = MaxHandSize,
            CardIds = CardIds // DTO has inline init, trust it
        };
    }
}
