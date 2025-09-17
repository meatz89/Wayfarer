using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Wayfarer
{
    /// <summary>
    /// SIMPLIFIED: Minimal animation tracking for visual feedback only.
    /// Prioritizes correctness over complex animations.
    /// All game state changes happen immediately.
    /// </summary>
    public class CardAnimationManager
    {
        /// <summary>
        /// Master flag to enable/disable all animations.
        /// Set to false to focus on fixing mechanical bugs.
        /// </summary>
        public static bool AnimationsEnabled = false;

        private readonly List<AnimatingCard> animatingCards = new();
        private readonly Dictionary<string, CardAnimationState> cardStates = new();

        /// <summary>
        /// Get the list of currently animating cards (played cards only).
        /// </summary>
        public List<AnimatingCard> AnimatingCards => animatingCards;

        /// <summary>
        /// Get the current animation states for cards.
        /// </summary>
        public Dictionary<string, CardAnimationState> CardStates => cardStates;

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
        /// Mark a card as played with success/failure animation.
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
        /// Mark new cards with a simple fade-in animation.
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
        }

        /// <summary>
        /// Clean up old animation states.
        /// Simplified: Only track played card animations (1.5s duration).
        /// </summary>
        public void CleanupOldAnimations()
        {
            DateTime now = DateTime.Now;

            // Clean up played card animations after 1.6 seconds
            animatingCards.RemoveAll(ac => (now - ac.AddedAt).TotalSeconds > 1.6);

            // Remove old card states
            var oldStates = cardStates
                .Where(kvp => (now - kvp.Value.StateChangedAt).TotalSeconds > 1.6)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (string cardId in oldStates)
            {
                cardStates.Remove(cardId);
            }
        }
    }
}