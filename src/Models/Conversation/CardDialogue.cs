/// <summary>
/// Represents dialogue text for a card, including player text and contextual variations.
/// </summary>
public class CardDialogue
{
public string playerText { get; set; }
public Dictionary<string, string> contextual { get; set; }
}
