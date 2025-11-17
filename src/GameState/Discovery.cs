/// <summary>
/// Named clue discovered during obligation
/// </summary>
public class Discovery
{
    public string Id { get; init; }
    public string Name { get; init; }
    public DiscoveryType Type { get; init; }
    public string Description { get; init; }

    // What this discovery unlocks
    public List<string> UnlocksCardIds { get; init; } = new List<string>();

    // ADR-007: CreatesObservationCards property DELETED - never read (dead code)
    // ADR-007: ObservationCardGeneration class DELETED - never instantiated (dead code)
    //   - ObservationCardGeneration.NpcId (line 20) DELETED
    //   - ObservationCardGeneration.CardId (line 21) DELETED
    //   - ObservationCardGeneration.ConversationType (line 22) DELETED
}
