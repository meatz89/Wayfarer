using System;

namespace Wayfarer
{
    /// <summary>
    /// Tracks animation state for individual cards in the conversation UI.
    /// </summary>
    public class CardAnimationState
    {
        public string CardId { get; set; }
        public string State { get; set; } // "new", "played-success", "played-failure", "exhausting", "normal"
        public DateTime StateChangedAt { get; set; }
    }
}