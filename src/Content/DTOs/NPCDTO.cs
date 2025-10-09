using System.Collections.Generic;

/// <summary>
/// Data Transfer Object for deserializing NPC data from JSON.
/// Maps to the structure in npcs.json.
/// </summary>
public class NPCDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Profession { get; set; }
    public string VenueId { get; set; }
    public string LocationId { get; set; }
    public string Description { get; set; }
    public string Personality { get; set; }
    public string PersonalityType { get; set; }
    public List<string> Services { get; set; } = new List<string>();
    public string Role { get; set; }
    public string AvailabilitySchedule { get; set; }
    public int Tier { get; set; }
    public int Level { get; set; } = 1; // Level 1-5 for difficulty/content progression and XP scaling
    public int ConversationDifficulty { get; set; } = 1; // Level 1-3 for XP multipliers

    public string CurrentState { get; set; }
    public Dictionary<string, int> InitialTokens { get; set; } = new Dictionary<string, int>();

    public List<NPCRequestDTO> Requests { get; set; } = new List<NPCRequestDTO>();
}
