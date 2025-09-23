using System.Collections.Generic;

/// <summary>
/// DTO for card deck definitions loaded from JSON
/// Each deck contains card IDs with their counts
/// </summary>
public class CardDeckDTO
{
    public string Id { get; set; }
    public Dictionary<string, int> CardCounts { get; set; } = new Dictionary<string, int>();
}