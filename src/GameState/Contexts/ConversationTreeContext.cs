/// <summary>
/// Context for ConversationTree screens containing conversation state and metadata.
/// Provides view model data for simple dialogue tree navigation.
/// </summary>
public class ConversationTreeContext
{
public bool IsValid { get; set; }
public string ErrorMessage { get; set; }

// Conversation data
public ConversationTree Tree { get; set; }
public DialogueNode CurrentNode { get; set; }
public NPC Npc { get; set; }

// Player state
public int CurrentFocus { get; set; }
public int MaxFocus { get; set; }
public int CurrentRelationship { get; set; }
public Dictionary<PlayerStatType, int> PlayerStats { get; set; }
public List<string> PlayerKnowledge { get; set; }

// Display info
public string LocationName { get; set; }
public string TimeDisplay { get; set; }

public ConversationTreeContext()
{
    IsValid = true;
    ErrorMessage = string.Empty;
    PlayerStats = new Dictionary<PlayerStatType, int>();
    PlayerKnowledge = new List<string>();
}

// Helper methods for UI
public List<DialogueResponse> GetAvailableResponses()
{
    if (CurrentNode == null) return new List<DialogueResponse>();

    return CurrentNode.Responses
        .Where(r => CanAffordResponse(r) && MeetsRequirements(r))
        .ToList();
}

public List<DialogueResponse> GetBlockedResponses()
{
    if (CurrentNode == null) return new List<DialogueResponse>();

    return CurrentNode.Responses
        .Where(r => !CanAffordResponse(r) || !MeetsRequirements(r))
        .ToList();
}

public bool CanAffordResponse(DialogueResponse response)
{
    return CurrentFocus >= response.FocusCost;
}

public bool MeetsRequirements(DialogueResponse response)
{
    if (!response.RequiredStat.HasValue) return true;
    if (!response.RequiredStatLevel.HasValue) return true;

    if (!PlayerStats.ContainsKey(response.RequiredStat.Value))
        return false;

    return PlayerStats[response.RequiredStat.Value] >= response.RequiredStatLevel.Value;
}

public string GetBlockReason(DialogueResponse response)
{
    if (!CanAffordResponse(response))
        return $"Requires {response.FocusCost} Focus (you have {CurrentFocus})";

    if (!MeetsRequirements(response))
        return $"Requires {response.RequiredStat} level {response.RequiredStatLevel}";

    return "";
}
}
