using System;
using System.Collections.Generic;
using System.Linq;

namespace Wayfarer
{
    /// <summary>
    /// SYNCHRONOUS PRINCIPLE: Stores copies of exhausting cards temporarily for animation display.
    /// Cards are removed from game state IMMEDIATELY, but kept here for visual animation only.
    /// This allows the DOM elements to persist while CSS animations play.
    /// </summary>
    public class ExhaustingCardStore
    {
        private readonly List<ExhaustingCardInfo> exhaustingCards = new();
        private readonly object lockObject = new();

        /// <summary>
        /// Add cards that are being exhausted. These are COPIES for display only.
        /// The real cards have already been removed from game state.
        /// </summary>
        public void AddExhaustingCards(List<CardInstance> cards, double baseDelay = 0.0)
        {
            if (cards == null || !cards.Any()) return;

            const double STAGGER_DELAY = 0.15; // 150ms between each card

            lock (lockObject)
            {
                for (int i = 0; i < cards.Count; i++)
                {
                    // Create a copy with the same template and context
                    // This preserves the card for display while it animates out
                    var cardCopy = new CardInstance
                    {
                        InstanceId = cards[i].InstanceId,
                        Template = cards[i].Template,
                        Context = cards[i].Context
                    };

                    exhaustingCards.Add(new ExhaustingCardInfo
                    {
                        Card = cardCopy,
                        AnimationDelay = baseDelay + (i * STAGGER_DELAY),
                        SequenceIndex = i,
                        AnimationDirection = "right",
                        AddedAt = DateTime.Now,
                        // Animation duration 0.5s + delay + 0.1s buffer
                        RemoveAt = DateTime.Now.AddSeconds(baseDelay + (i * STAGGER_DELAY) + 0.6)
                    });
                }
            }
        }

        /// <summary>
        /// Get all currently exhausting cards for display
        /// </summary>
        public List<ExhaustingCardInfo> GetExhaustingCards()
        {
            lock (lockObject)
            {
                return exhaustingCards.ToList();
            }
        }

        /// <summary>
        /// Clean up cards whose animations have completed
        /// </summary>
        public void CleanupExpiredCards()
        {
            lock (lockObject)
            {
                var now = DateTime.Now;
                exhaustingCards.RemoveAll(c => c.RemoveAt <= now);
            }
        }

        /// <summary>
        /// Clear all exhausting cards (used when starting new action)
        /// </summary>
        public void Clear()
        {
            lock (lockObject)
            {
                exhaustingCards.Clear();
            }
        }

        /// <summary>
        /// Get count of exhausting cards
        /// </summary>
        public int Count
        {
            get
            {
                lock (lockObject)
                {
                    return exhaustingCards.Count;
                }
            }
        }
    }

    /// <summary>
    /// Information about a card that is currently exhausting
    /// </summary>
    public class ExhaustingCardInfo
    {
        public CardInstance Card { get; set; }
        public double AnimationDelay { get; set; }
        public int SequenceIndex { get; set; }
        public string AnimationDirection { get; set; }
        public DateTime AddedAt { get; set; }
        public DateTime RemoveAt { get; set; }
    }
}