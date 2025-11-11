/// <summary>
/// DTO for loading PhysicalChallengeDeck from JSON
/// </summary>
public class PhysicalChallengeDeckDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int DangerThreshold { get; set; }
    public int InitialHandSize { get; set; }
    public int MaxHandSize { get; set; }
    public List<string> CardIds { get; set; } = new List<string>();

    public PhysicalChallengeDeck ToDomain()
    {
        return new PhysicalChallengeDeck
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
