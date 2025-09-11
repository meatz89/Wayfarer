using System.Collections.Generic;

/// <summary>
/// DTO for travel events that contain multiple event cards.
/// Represents a single event that can occur during travel with its own narrative and card choices.
/// </summary>
public class TravelEventDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string NarrativeText { get; set; }
    public List<string> EventCardIds { get; set; } = new List<string>();
}