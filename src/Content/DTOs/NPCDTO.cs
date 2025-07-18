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
    public string LocationId { get; set; }
    public string SpotId { get; set; }
    public string Description { get; set; }
    public string Personality { get; set; }
    public List<string> Services { get; set; } = new List<string>();
    public List<string> ContractCategories { get; set; } = new List<string>();
    public List<string> LetterTokenTypes { get; set; } = new List<string>();
    public string Role { get; set; }
    public string AvailabilitySchedule { get; set; }
}