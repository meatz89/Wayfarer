/// <summary>
/// Physical challenge type definition
/// Bridges strategic goals to tactical Physical challenge system
/// </summary>
public class PhysicalChallengeType
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string DeckId { get; set; }  // References PhysicalChallengeDeck

    // Victory/Failure Thresholds
    public int VictoryThreshold { get; set; }  // Breakthrough needed
    public int DangerThreshold { get; set; }   // Danger limit

    // Session Parameters
    public int InitialHandSize { get; set; } = 5;
    public int MaxHandSize { get; set; } = 7;
}
