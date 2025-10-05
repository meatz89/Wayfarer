using System;

/// <summary>
/// Tracks animation state for individual cards in the conversation UI.
/// </summary>
public class CardAnimationState
{
    public string CardId { get; set; }
    public string State { get; set; } // "new", "played-success", "played-failure", "exhausting", "normal"
    public DateTime StateChangedAt { get; set; }
    public double AnimationDelay { get; set; } // Delay in seconds for this specific card
    public int SequenceIndex { get; set; } // Position in animation sequence
    public string AnimationDirection { get; set; } // "left", "right", "up", "down"
}
