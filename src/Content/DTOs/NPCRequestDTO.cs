using System.Collections.Generic;

/// <summary>
/// Data Transfer Object for NPC one-time requests from JSON
/// </summary>
public class NPCRequestDTO
{
    public string Id { get; set; }
    public string NpcId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string NpcRequestText { get; set; }
    public string ConversationTypeId { get; set; } // REQUIRED: Must specify which conversation type to use
    public List<NPCRequestGoalDTO> Goals { get; set; } = new List<NPCRequestGoalDTO>();
}

/// <summary>
/// Data Transfer Object for NPC request goals with tiered rewards
/// </summary>
public class NPCRequestGoalDTO
{
    public string Id { get; set; }
    public string CardId { get; set; } // REQUIRED: References conversation card from _cards.json
    public string Name { get; set; }
    public string Description { get; set; }
    public int MomentumThreshold { get; set; }
    public int Weight { get; set; }
    public NPCRequestRewardDTO Rewards { get; set; }
}

/// <summary>
/// Data Transfer Object for NPC request goal rewards
/// </summary>
public class NPCRequestRewardDTO
{
    public int? Coins { get; set; }
    public string LetterId { get; set; }
    public string Obligation { get; set; }
    public string Item { get; set; }
    public Dictionary<string, int> Tokens { get; set; } = new Dictionary<string, int>();
}