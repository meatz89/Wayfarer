namespace Wayfarer
{
    /// <summary>
    /// Unified display information for cards in the conversation UI.
    /// Combines regular hand cards with animating cards for consistent rendering.
    /// </summary>
    public class CardDisplayInfo
    {
        public CardInstance Card { get; set; }
        public bool IsAnimating { get; set; }
        public string AnimationState { get; set; } // "success" or "failure" for animation CSS class
    }
}