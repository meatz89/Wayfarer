using System.Collections.Generic;

/// <summary>
/// DTO for card deck definitions loaded from JSON
/// Each deck is simply a list of card IDs
/// </summary>
public class CardDeckDTO
{
    public string Id { get; set; }
    public List<string> CardIds { get; set; } = new List<string>();
}