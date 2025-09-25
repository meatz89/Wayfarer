using System;

namespace Wayfarer
{
    /// <summary>
    /// Type of animation to apply to a card
    /// </summary>
    public enum CardAnimationType
    {
        PlayedSuccess,  // Card played successfully
        PlayedFailure,  // Card played but failed
        Exhausting      // Card being exhausted (removed from hand)
    }

    /// <summary>
    /// Tracks a card that is currently being animated (e.g., success/failure flash animation).
    /// Used to maintain card visibility during animation before removal from hand.
    /// </summary>
    public class AnimatingCard
    {
        public CardInstance Card { get; set; }
        public CardAnimationType AnimationType { get; set; }
        public DateTime AddedAt { get; set; }
        public int OriginalPosition { get; set; } // Track original position in hand
    }
}