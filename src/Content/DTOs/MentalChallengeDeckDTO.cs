using System.Collections.Generic;

/// <summary>
/// DTO for loading MentalChallengeDeck from JSON
/// </summary>
public class MentalChallengeDeckDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int DangerThreshold { get; set; }
    public int? FocusCost { get; set; }
    public int InitialHandSize { get; set; }
    public int MaxHandSize { get; set; }
    public List<string> CardIds { get; set; } = new List<string>();

    public MentalChallengeDeck ToDomain()
    {
        return new MentalChallengeDeck
        {
            Id = Id,
            Name = Name,
            Description = Description,
            DangerThreshold = DangerThreshold,
            FocusCost = FocusCost,
            InitialHandSize = InitialHandSize,
            MaxHandSize = MaxHandSize,
            CardIds = CardIds // DTO has inline init, trust it
        };
    }
}
