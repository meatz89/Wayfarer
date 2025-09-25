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
        public static bool AnimationsEnabled = true;

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
        /// Clear all animation states.
        /// Called when starting a new action to reset visual state.
        /// </summary>
        public void ClearAllStates()
        {
            animatingCards.Clear();
            cardStates.Clear();
        }

    }
}