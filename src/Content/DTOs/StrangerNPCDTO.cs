using System.Collections.Generic;

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
    public List<StrangerConversationDTO> AvailableConversationTypes { get; set; } = new();
}

/// <summary>
/// DTO for stranger conversation types and thresholds
/// </summary>
public class StrangerConversationDTO
{
    public string Type { get; set; }
    public List<int> RapportThresholds { get; set; } = new();
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
    public Dictionary<string, int> Tokens { get; set; } = new();
}