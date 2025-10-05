/// <summary>
/// DTO for loading PhysicalEngagementType from JSON
/// </summary>
public class PhysicalEngagementTypeDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string DeckId { get; set; }
    public int VictoryThreshold { get; set; }
    public int DangerThreshold { get; set; }
    public int InitialHandSize { get; set; } = 5;
    public int MaxHandSize { get; set; } = 7;

    public PhysicalEngagementType ToDomain()
    {
        return new PhysicalEngagementType
        {
            Id = Id,
            Name = Name,
            DeckId = DeckId,
            VictoryThreshold = VictoryThreshold,
            DangerThreshold = DangerThreshold,
            InitialHandSize = InitialHandSize,
            MaxHandSize = MaxHandSize
        };
    }
}
