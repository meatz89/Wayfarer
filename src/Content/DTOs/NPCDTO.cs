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
    public string PersonalityType { get; set; }
    public List<string> Services { get; set; } = new List<string>();
    public List<string> LetterTokenTypes { get; set; } = new List<string>();
    public string Role { get; set; }
    public string AvailabilitySchedule { get; set; }
    public int Tier { get; set; }

    // REMOVED: Boolean flags violate deck-based architecture
    // Letters are detected by checking Request deck contents
    // Burden history detected by counting burden cards in burden deck

    // Properties from JSON that weren't being parsed
    public string CurrentState { get; set; }
}

// REMOVED: Letter DTOs violate deck-based architecture
// Letters are now handled as request cards in the Request deck