using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Wayfarer
{
    /// <summary>
    /// SYNCHRONOUS PRINCIPLE: Card animations are purely visual feedback.
    /// This manager tracks which cards should have CSS animations applied,
    /// but NEVER delays or blocks game logic. All effects happen immediately.
    /// Animations are CSS-only and do not affect game state timing.
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
        /// SYNCHRONOUS PRINCIPLE: Mark a card for visual animation only.
        /// The card is tracked for CSS animation but this NEVER delays game logic.
        /// CSS animations handle timing, not C# code.
        /// </summary>
        public void AddAnimatingCard(CardInstance card, bool success, int originalPosition, Action stateChangedCallback)
        {
            if (card == null) return;

            animatingCards.Add(new AnimatingCard
            {
                Card = card,
                Success = success,
                AnimationType = success ? CardAnimationType.PlayedSuccess : CardAnimationType.PlayedFailure,
                AddedAt = DateTime.Now,
                OriginalPosition = originalPosition
            });

            // SYNCHRONOUS PRINCIPLE: No delays! CSS handles animation timing.
            // Card will be removed on next action or when animation CSS completes.
            // Game logic continues immediately.
            stateChangedCallback?.Invoke();
        }

        /// <summary>
        /// Add a card for exhaust animation specifically
        /// </summary>
        public void AddExhaustingCard(CardInstance card, int originalPosition, Action stateChangedCallback)
        {
            if (card == null) return;

            animatingCards.Add(new AnimatingCard
            {
                Card = card,
                Success = false,  // Not relevant for exhaust
                AnimationType = CardAnimationType.Exhausting,
                AddedAt = DateTime.Now,
                OriginalPosition = originalPosition
            });

            stateChangedCallback?.Invoke();
        }

        /// <summary>
        /// SYNCHRONOUS PRINCIPLE: Mark a card with animation state for CSS.
        /// No delays - CSS handles animation timing.
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

            // SYNCHRONOUS PRINCIPLE: No delays! State cleared on next action.
            // CSS animation runs independently of game logic.
            stateChangedCallback?.Invoke();
        }

        /// <summary>
        /// SYNCHRONOUS PRINCIPLE: Mark cards for CSS exhaust animation.
        /// Cards are removed from game state immediately.
        /// Animation is visual feedback only.
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

            // SYNCHRONOUS PRINCIPLE: No delays! Cards already removed from game state.
            // CSS animation provides visual feedback while game continues.
            stateChangedCallback?.Invoke();
        }

        /// <summary>
        /// SYNCHRONOUS PRINCIPLE: Mark new cards for CSS slide-in animation.
        /// Cards are already in game state, animation is visual only.
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

            // SYNCHRONOUS PRINCIPLE: No delays! Cards immediately playable.
            // CSS animation provides visual feedback for new cards.
            stateChangedCallback?.Invoke();
        }

        /// <summary>
        /// Clear all animation states.
        /// Called when starting a new action to reset visual state.
        /// </summary>
        public void ClearAllStates()
        {
            animatingCards.Clear();
            cardStates.Clear();
            exhaustingCardIds.Clear();
        }

        /// <summary>
        /// SYNCHRONOUS PRINCIPLE: Clean up old animation states.
        /// Called periodically to remove stale animation markers.
        /// Based on CSS animation duration, not C# delays.
        /// </summary>
        public void CleanupOldAnimations()
        {
            DateTime cutoff = DateTime.Now.AddSeconds(-2); // CSS animations complete within 2s

            // Remove old animating cards
            animatingCards.RemoveAll(ac => ac.AddedAt < cutoff);

            // Remove old card states
            var oldStates = cardStates.Where(kvp => kvp.Value.StateChangedAt < cutoff).Select(kvp => kvp.Key).ToList();
            foreach (string cardId in oldStates)
            {
                cardStates.Remove(cardId);
                exhaustingCardIds.Remove(cardId);
            }
        }
    }
}