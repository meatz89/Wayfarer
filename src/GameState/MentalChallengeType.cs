/// <summary>
/// Mental engagement type definition
/// Bridges strategic goals to tactical Mental (investigation) system
/// </summary>
public class MentalEngagementType
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string DeckId { get; set; }  // References MentalEngagementDeck

    // Victory/Failure Thresholds
    public int VictoryThreshold { get; set; }  // Progress needed
    public int DangerThreshold { get; set; }   // Exposure limit

    // Session Parameters
    public int InitialHandSize { get; set; } = 5;
    public int MaxHandSize { get; set; } = 7;
}
