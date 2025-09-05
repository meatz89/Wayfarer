using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Wayfarer
{
    /// <summary>
    /// Manages card animations in the conversation UI.
    /// Tracks animating cards and animation states for success/failure feedback.
    /// </summary>
    public class CardAnimationManager
    {
        private readonly List<AnimatingCard> animatingCards = new();
        private readonly Dictionary<string, CardAnimationState> cardStates = new();
        private readonly HashSet<string> exhaustingCardIds = new();

        /// <summary>
        /// Get the list of currently animating cards.
        /// </summary>
        public List<AnimatingCard> AnimatingCards => animatingCards;

        /// <summary>
        /// Get the current animation states for cards.
        /// </summary>
        public Dictionary<string, CardAnimationState> CardStates => cardStates;

        /// <summary>
        /// Get the set of card IDs that are exhausting.
        /// </summary>
        public HashSet<string> ExhaustingCardIds => exhaustingCardIds;

        /// <summary>
        /// Add a card to the animating cards list for post-play animation.
        /// </summary>
        public void AddAnimatingCard(CardInstance card, bool success, int originalPosition, Action stateChangedCallback)
        {
            if (card == null) return;

            animatingCards.Add(new AnimatingCard
            {
                Card = card,
                Success = success,
                AddedAt = DateTime.Now,
                OriginalPosition = originalPosition
            });

            // Remove after animation completes (1.25s flash + 0.25s play-out = 1.5s total)
            Task.Delay(1600).ContinueWith(_ =>
            {
                animatingCards.RemoveAll(ac => ac.Card.InstanceId == card.InstanceId);
                stateChangedCallback?.Invoke();
            });
        }

        /// <summary>
        /// Mark a card as successfully or unsuccessfully played.
        /// </summary>
        public void MarkCardAsPlayed(CardInstance card, bool success, Action stateChangedCallback)
        {
            if (card == null) return;

            string cardId = card.InstanceId ?? card.Id ?? "";
            cardStates[cardId] = new CardAnimationState
            {
                CardId = cardId,
                State = success ? "played-success" : "played-failure",
                StateChangedAt = DateTime.Now
            };

            // Remove state after animation completes
            Task.Delay(1600).ContinueWith(_ =>
            {
                cardStates.Remove(cardId);
                stateChangedCallback?.Invoke();
            });
        }

        /// <summary>
        /// Mark cards for exhaust animation.
        /// </summary>
        public void MarkCardsForExhaust(List<CardInstance> cardsToExhaust, Action stateChangedCallback)
        {
            foreach (CardInstance card in cardsToExhaust)
            {
                string cardId = card.InstanceId ?? card.Id ?? "";
                exhaustingCardIds.Add(cardId);

                cardStates[cardId] = new CardAnimationState
                {
                    CardId = cardId,
                    State = "exhausting",
                    StateChangedAt = DateTime.Now
                };
            }

            // Remove after exhaust animation (0.5s)
            Task.Delay(500).ContinueWith(_ =>
            {
                foreach (CardInstance card in cardsToExhaust)
                {
                    string cardId = card.InstanceId ?? card.Id ?? "";
                    exhaustingCardIds.Remove(cardId);
                    cardStates.Remove(cardId);
                }
                stateChangedCallback?.Invoke();
            });
        }

        /// <summary>
        /// Mark new cards with animation state.
        /// </summary>
        public void MarkNewCards(List<CardInstance> newCards, HashSet<string> newCardIds, Action stateChangedCallback)
        {
            foreach (CardInstance card in newCards)
            {
                string cardId = card.InstanceId ?? card.Id ?? "";
                newCardIds.Add(cardId);

                cardStates[cardId] = new CardAnimationState
                {
                    CardId = cardId,
                    State = "new",
                    StateChangedAt = DateTime.Now
                };
            }

            // Clear new state after animation (0.25s)
            Task.Delay(250).ContinueWith(_ =>
            {
                foreach (CardInstance card in newCards)
                {
                    string cardId = card.InstanceId ?? card.Id ?? "";
                    newCardIds.Remove(cardId);
                    cardStates.Remove(cardId);
                }
                stateChangedCallback?.Invoke();
            });
        }

        /// <summary>
        /// Clear all animation states.
        /// </summary>
        public void ClearAllStates()
        {
            animatingCards.Clear();
            cardStates.Clear();
            exhaustingCardIds.Clear();
        }
    }
}