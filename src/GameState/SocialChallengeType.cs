/// <summary>
/// Social challenge type definition
/// Bridges strategic goals to tactical Social challenge system
/// </summary>
public class SocialChallengeType
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string DeckId { get; set; }  // References SocialChallengeDeck

    // Victory/Failure Thresholds
    public int VictoryThreshold { get; set; }  // Momentum needed
    public int DangerThreshold { get; set; }   // Doubt limit

    // Session Parameters
    public int InitialHandSize { get; set; } = 5;
    public int MaxHandSize { get; set; } = 7;
}
