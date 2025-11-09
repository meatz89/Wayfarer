
/// <summary>
/// Represents conversation options available for a specific NPC
/// </summary>
public class NPCConversationOptions
{
public string NpcId { get; set; }
public string NpcName { get; set; }
public List<string> AvailableTypes { get; set; } = new List<string>();
public bool CanAfford { get; set; }
}
