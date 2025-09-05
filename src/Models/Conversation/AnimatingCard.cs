using System;

namespace Wayfarer
{
    /// <summary>
    /// Tracks a card that is currently being animated (e.g., success/failure flash animation).
    /// Used to maintain card visibility during animation before removal from hand.
    /// </summary>
    public class AnimatingCard
    {
        public CardInstance Card { get; set; }
        public bool Success { get; set; }
        public DateTime AddedAt { get; set; }
        public int OriginalPosition { get; set; } // Track original position in hand
    }
}