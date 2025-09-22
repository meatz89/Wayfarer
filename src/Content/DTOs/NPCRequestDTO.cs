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
    public List<string> RequestCards { get; set; } = new List<string>();
    public List<string> PromiseCards { get; set; } = new List<string>();
}