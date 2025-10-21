using System.Collections.Generic;

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

    // Observation cards this creates
    public List<ObservationCardGeneration> CreatesObservationCards { get; init; } = new List<ObservationCardGeneration>();
}

public class ObservationCardGeneration
{
    public string NpcId { get; set; }
    public string CardId { get; set; }
    public string ConversationType { get; set; }
}
