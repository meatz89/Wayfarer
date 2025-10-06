using System.Collections.Generic;

/// <summary>
/// DTO for loading PhysicalEngagementDeck from JSON
/// </summary>
public class PhysicalEngagementDeckDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public List<string> CardIds { get; set; } = new List<string>();

    public PhysicalEngagementDeck ToDomain()
    {
        return new PhysicalEngagementDeck
        {
            Id = Id,
            Name = Name,
            CardIds = CardIds ?? new List<string>()
        };
    }
}
