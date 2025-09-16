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
                StateChangedAt = DateTime.Now,
                AnimationDelay = 0, // Played card animates immediately
                AnimationDirection = "up" // Exit upward after flash
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
                    StateChangedAt = DateTime.Now,
                    AnimationDelay = 0,
                    AnimationDirection = "right"
                };
            }

            // SYNCHRONOUS PRINCIPLE: No delays! Cards already removed from game state.
            // CSS animation provides visual feedback while game continues.
            stateChangedCallback?.Invoke();
        }

        /// <summary>
        /// Mark cards for sequential exhaust animation with staggered delays.
        /// Cards exit to the right one by one.
        /// </summary>
        public void MarkCardsForExhaustSequential(List<CardInstance> cardsToExhaust, double baseDelay, Action stateChangedCallback)
        {
            const double STAGGER_DELAY = 0.15; // 150ms between each card

            for (int i = 0; i < cardsToExhaust.Count; i++)
            {
                CardInstance card = cardsToExhaust[i];
                string cardId = card.InstanceId ?? card.Id ?? "";

                exhaustingCardIds.Add(cardId);

                cardStates[cardId] = new CardAnimationState
                {
                    CardId = cardId,
                    State = "exhausting",
                    StateChangedAt = DateTime.Now,
                    AnimationDelay = baseDelay + (i * STAGGER_DELAY),
                    SequenceIndex = i,
                    AnimationDirection = "right"
                };
            }

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
        /// Mark new cards for sequential draw animation with staggered delays.
        /// Cards enter from the left one by one.
        /// </summary>
        public void MarkNewCardsSequential(List<CardInstance> newCards, double startDelay, HashSet<string> newCardIds, Action stateChangedCallback)
        {
            const double STAGGER_DELAY = 0.15; // 150ms between each card

            for (int i = 0; i < newCards.Count; i++)
            {
                CardInstance card = newCards[i];
                string cardId = card.InstanceId ?? card.Id ?? "";

                newCardIds.Add(cardId);

                cardStates[cardId] = new CardAnimationState
                {
                    CardId = cardId,
                    State = "new",
                    StateChangedAt = DateTime.Now,
                    AnimationDelay = startDelay + (i * STAGGER_DELAY),
                    SequenceIndex = i,
                    AnimationDirection = "left"
                };
            }

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
            DateTime now = DateTime.Now;

            // Different cleanup times based on animation type
            // Exhaust animations complete in 0.5s, so cleanup at 0.6s
            // Play animations take 1.5s total (1.25s flash + 0.25s out)
            animatingCards.RemoveAll(ac =>
            {
                double secondsSinceAdded = (now - ac.AddedAt).TotalSeconds;
                if (ac.AnimationType == CardAnimationType.Exhausting)
                    return secondsSinceAdded > 0.6; // Cleanup exhausted cards quickly
                else
                    return secondsSinceAdded > 1.6; // Play animations need more time
            });

            // Remove old card states with same timing logic
            var oldStates = cardStates.Where(kvp =>
            {
                double secondsSinceChanged = (now - kvp.Value.StateChangedAt).TotalSeconds;
                return kvp.Value.State == "exhausting" ? secondsSinceChanged > 0.6 : secondsSinceChanged > 1.6;
            }).Select(kvp => kvp.Key).ToList();

            foreach (string cardId in oldStates)
            {
                cardStates.Remove(cardId);
                exhaustingCardIds.Remove(cardId);
            }
        }
    }
}