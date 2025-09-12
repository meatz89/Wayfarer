using System.Collections.Generic;

/// <summary>
/// Data Transfer Object for NPC one-time requests from JSON
/// </summary>
public class NPCRequestDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public List<string> RequestCardIds { get; set; } = new List<string>();
    public List<string> PromiseCardIds { get; set; } = new List<string>();
}