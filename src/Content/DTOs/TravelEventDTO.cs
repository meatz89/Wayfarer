/// <summary>
/// DTO for travel events that contain multiple event cards.
/// Represents a single event that can occur during travel with its own narrative and card choices.
/// </summary>
public class TravelEventDTO
{
public string Id { get; set; }
public string Name { get; set; }
public string NarrativeText { get; set; }

// Embedded event cards - populated during loading
public List<PathCardDTO> EventCards { get; set; } = new List<PathCardDTO>();

// Temporary parsing property - used only during JSON loading
public List<string> EventCardIds { get; set; }
}