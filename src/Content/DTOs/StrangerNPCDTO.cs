/// <summary>
/// Data Transfer Object for stranger NPC definitions from JSON packages
/// </summary>
public class StrangerNPCDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public int Level { get; set; }
    public string Personality { get; set; }
    public string LocationId { get; set; }
    public string TimeBlock { get; set; }
    public StrangerRequestDTO Request { get; set; } // Single request per stranger
}

/// <summary>
/// DTO for stranger request with tiered rewards
/// </summary>
public class StrangerRequestDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string ConversationTypeId { get; set; } // The conversation type to use
    public List<string> SituationCards { get; set; } = new(); // Not used - cards come from conversation type deck
    public List<string> PromiseCards { get; set; } = new(); // Not used - cards come from conversation type deck
    public List<int> MomentumThresholds { get; set; } = new();
    public List<StrangerRewardDTO> Rewards { get; set; } = new();
}

/// <summary>
/// DTO for stranger conversation rewards
/// </summary>
public class StrangerRewardDTO
{
    public int Coins { get; set; } = 0;
    public int Health { get; set; } = 0;
    public int Food { get; set; } = 0;
    public int Familiarity { get; set; } = 0;
    public string Item { get; set; }
    public string Permit { get; set; }
    public string Observation { get; set; }
    // DOMAIN COLLECTION PRINCIPLE: List of objects instead of Dictionary
    public List<TokenEntryDTO> Tokens { get; set; } = new();
}