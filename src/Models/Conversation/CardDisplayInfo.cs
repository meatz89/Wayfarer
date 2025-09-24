using System.Collections.Generic;
using Wayfarer.Services.Conversation;

namespace Wayfarer
{
    /// <summary>
    /// Enhanced presentation model for cards in the conversation UI.
    /// Combines regular hand cards, animating cards, and exhausting cards with slot-aware positioning
    /// and comprehensive animation coordination for choreographed card movements.
    ///
    /// ENHANCED FEATURES:
    /// - Slot Awareness: Stable slot addressing and positioning
    /// - Animation Coordination: CSS classes, transforms, and timing properties
    /// - Presentation States: Integration with CardPresentationState system
    /// - Ghost Rendering: Support for removed cards that need exit animations
    /// - CSS Integration: Direct CSS property generation for smooth animations
    /// </summary>
    public class CardDisplayInfo
    {
        // Core card data
        public CardInstance Card { get; set; }
        public bool IsAnimating { get; set; }
        public bool IsExhausting { get; set; }

        // Legacy animation properties (maintained for compatibility)
        public double AnimationDelay { get; set; }
        public string AnimationDirection { get; set; }
        public string AnimationState { get; set; }

        // Enhanced: Slot-aware positioning
        public int SlotAddress { get; set; }                    // Stable slot identifier
        public Vector2 SlotPosition { get; set; }               // Physical position coordinates
        public CardPresentationState PresentationState { get; set; } // Animation state

        // Enhanced: CSS animation coordination
        public string CSSTransform { get; set; }               // CSS transform property
        public string CSSAnimationClass { get; set; }          // choreography-exit, choreography-enter, etc.
        public string CSSStaggerClass { get; set; }            // stagger-1, stagger-2, etc.

        // Enhanced: Ghost rendering for exit animations
        public bool IsGhost { get; set; }                      // Card is being removed but needs animation
        public CardSlotTransitionType TransitionType { get; set; } // Type of transition occurring

        // Enhanced: Animation timing (milliseconds for precision)
        public int AnimationDelayMs { get; set; }              // Start delay in milliseconds
        public int AnimationDurationMs { get; set; }           // Duration in milliseconds

        /// <summary>
        /// Get complete CSS class string for this card's animation.
        /// Combines base classes with animation and stagger classes.
        /// </summary>
        public string GetCSSClasses(string baseClasses = "")
        {
            var classes = new List<string>();

            if (!string.IsNullOrEmpty(baseClasses))
                classes.Add(baseClasses);

            if (!string.IsNullOrEmpty(CSSAnimationClass))
                classes.Add(CSSAnimationClass);

            if (!string.IsNullOrEmpty(CSSStaggerClass))
                classes.Add(CSSStaggerClass);

            if (IsGhost)
                classes.Add("card-ghost");

            if (PresentationState != CardPresentationState.Static)
                classes.Add($"presentation-{PresentationState.ToString().ToLower()}");

            return string.Join(" ", classes);
        }

        /// <summary>
        /// Get CSS custom properties for animation timing and transforms.
        /// </summary>
        public string GetCSSVariables()
        {
            var variables = new List<string>();

            variables.Add($"--animation-delay: {AnimationDelayMs}ms");
            variables.Add($"--animation-duration: {AnimationDurationMs}ms");
            variables.Add($"--slot-address: {SlotAddress}");
            variables.Add($"--slot-x: {SlotPosition.X}px");
            variables.Add($"--slot-y: {SlotPosition.Y}px");

            if (!string.IsNullOrEmpty(CSSTransform))
                variables.Add($"--transform: {CSSTransform}");

            return string.Join("; ", variables);
        }

        /// <summary>
        /// Check if this card should be rendered during animations.
        /// Ghost cards and normal cards should render, but only during appropriate phases.
        /// </summary>
        public bool ShouldRender()
        {
            // Ghost cards render only during their exit animation
            if (IsGhost)
                return PresentationState == CardPresentationState.Exiting;

            // Normal cards always render unless they've been removed
            return TransitionType != CardSlotTransitionType.Removed || IsAnimating;
        }

        /// <summary>
        /// Get display priority for rendering order.
        /// Lower numbers render first (behind), higher numbers render last (on top).
        /// </summary>
        public int GetDisplayPriority()
        {
            return PresentationState switch
            {
                CardPresentationState.Exiting => 1,      // Behind other cards
                CardPresentationState.Static => 2,       // Normal layer
                CardPresentationState.Repositioning => 3, // Above static cards
                CardPresentationState.Entering => 4,     // On top for entrance
                _ => 2
            };
        }
    }
}