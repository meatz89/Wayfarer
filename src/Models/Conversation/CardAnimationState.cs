/// <summary>
/// Tracks animation state for individual cards in the conversation UI.
/// TYPE SYSTEM: int only for game values (no float/double)
/// </summary>
public class CardAnimationState
{
    public string CardId { get; set; }
    public string State { get; set; } // "new", "played-success", "played-failure", "exhausting", "normal"
    public DateTime StateChangedAt { get; set; }
    public int AnimationDelayMs { get; set; } // Delay in milliseconds for this specific card
    public int SequenceIndex { get; set; } // Position in animation sequence
    public string AnimationDirection { get; set; } // "left", "right", "up", "down"
}
