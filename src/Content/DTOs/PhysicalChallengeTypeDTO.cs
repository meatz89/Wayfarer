/// <summary>
/// DTO for loading PhysicalChallengeType from JSON
/// </summary>
public class PhysicalChallengeTypeDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string DeckId { get; set; }
    public string Discipline { get; set; } = "Combat";
    public int VictoryThreshold { get; set; }
    public int DangerThreshold { get; set; }
    public int InitialHandSize { get; set; } = 5;
    public int MaxHandSize { get; set; } = 7;

    public PhysicalChallengeType ToDomain()
    {
        // Parse discipline enum
        PhysicalDiscipline discipline = System.Enum.TryParse<PhysicalDiscipline>(Discipline, out PhysicalDiscipline parsed)
            ? parsed
            : PhysicalDiscipline.Combat;

        return new PhysicalChallengeType
        {
            Id = Id,
            Name = Name,
            DeckId = DeckId,
            Discipline = discipline,
            VictoryThreshold = VictoryThreshold,
            DangerThreshold = DangerThreshold,
            InitialHandSize = InitialHandSize,
            MaxHandSize = MaxHandSize
        };
    }
}
