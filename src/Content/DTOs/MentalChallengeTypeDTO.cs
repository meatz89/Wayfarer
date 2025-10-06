/// <summary>
/// DTO for loading MentalChallengeType from JSON
/// </summary>
public class MentalChallengeTypeDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string DeckId { get; set; }
    public int VictoryThreshold { get; set; }
    public int DangerThreshold { get; set; }
    public int InitialHandSize { get; set; } = 5;
    public int MaxHandSize { get; set; } = 7;

    public MentalChallengeType ToDomain()
    {
        return new MentalChallengeType
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
