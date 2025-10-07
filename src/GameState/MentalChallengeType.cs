/// <summary>
/// Mental challenge type definition
/// Bridges strategic goals to tactical Mental challenge system
/// </summary>
public class MentalChallengeType
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string DeckId { get; set; }  // References MentalChallengeDeck

    // Victory/Failure Thresholds
    public int VictoryThreshold { get; set; }  // Progress needed
    public int DangerThreshold { get; set; }   // Exposure limit

    // Session Parameters
    public int InitialHandSize { get; set; } = 5;
    public int MaxHandSize { get; set; } = 7;
    public int? FocusCost { get; set; } = 10;  // Focus cost to start investigation (nullable for backwards compatibility)
}
