/// <summary>
/// Categorical data about letter positioning for UI translation.
/// Backend provides only data categories, frontend translates to user-facing text.
/// </summary>
public class LetterPositioningMessage
{
    public string SenderName { get; set; } = string.Empty;
    public LetterPositioningReason Reason { get; set; }
    public int Position { get; set; }
    public int RelationshipStrength { get; set; }
    public int RelationshipDebt { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
}