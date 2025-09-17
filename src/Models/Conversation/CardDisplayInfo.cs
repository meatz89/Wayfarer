namespace Wayfarer
{
    /// <summary>
    /// Unified display information for cards in the conversation UI.
    /// Combines regular hand cards, animating cards, and exhausting cards for consistent rendering.
    /// </summary>
    public class CardDisplayInfo
    {
        public CardInstance Card { get; set; }
        public bool IsAnimating { get; set; }
        public bool IsExhausting { get; set; } // New: Card is being exhausted
        public double AnimationDelay { get; set; } // New: Delay in seconds for sequential animation
        public string AnimationDirection { get; set; } // New: "left", "right", "up", "down"
        public string AnimationState { get; set; } // "card-exhausting", "card-new", "card-played-success", etc.
    }
}