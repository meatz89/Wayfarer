/// <summary>
/// Social engagement type definition
/// Bridges strategic goals to tactical Social (conversation) system
/// </summary>
public class SocialEngagementType
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string DeckId { get; set; }  // References ConversationEngagementDeck

    // Victory/Failure Thresholds
    public int VictoryThreshold { get; set; }  // Momentum needed
    public int DangerThreshold { get; set; }   // Doubt limit

    // Session Parameters
    public int InitialHandSize { get; set; } = 5;
    public int MaxHandSize { get; set; } = 7;
}
