using System;
using System.Collections.Generic;
using System.Linq;

namespace Wayfarer.Services.Conversation
{
    /// <summary>
    /// Card Slot Manager - Provides stable "addresses" for card positions
    /// and calculates smooth transitions between slot arrangements with presentation state management.
    ///
    /// ENHANCED PRINCIPLES:
    /// - Virtual Slots: Each card position has stable address independent of content
    /// - Slot Transitions: Cards move between addresses with CSS transform coordination
    /// - Presentation States: Cards have animation states (Static, Exiting, Repositioning, Entering)
    /// - CSS Integration: Slot movements map directly to CSS transforms
    /// - Animation Timing: Slot transitions include timing and choreography coordination
    /// </summary>
    public class CardSlotManager
    {
        /// <summary>
        /// Standard vertical spacing between card slots in pixels.
        /// </summary>
        public const int SLOT_SPACING = 120;

        /// <summary>
        /// Animation timing constants for choreography coordination.
        /// </summary>
        public const int STAGGER_DELAY_MS = 100;
        public const int EXIT_DURATION_MS = 650;
        public const int REPOSITION_DURATION_MS = 300;
        public const int ENTER_DURATION_MS = 500;
        public const int EXIT_BASE_DELAY_MS = 0;
        public const int REPOSITION_BASE_DELAY_MS = 650;
        public const int ENTER_BASE_DELAY_MS = 950;

        /// <summary>
        /// Calculate slot arrangement for given cards.
        /// Each slot has a stable address (index) and position.
        /// </summary>
        public List<CardSlot> CalculateSlots(List<CardInstance> cards)
        {
            var slots = new List<CardSlot>();

            if (cards == null)
                return slots;

            for (int i = 0; i < cards.Count; i++)
            {
                var slot = new CardSlot
                {
                    Address = i,
                    Position = new Vector2(0, i * SLOT_SPACING),
                    Card = cards[i],
                    IsOccupied = true
                };

                slots.Add(slot);
            }

            return slots;
        }

        /// <summary>
        /// Calculate transitions between two slot arrangements with complete animation coordination.
        /// Returns mapping of how cards move between their old and new slots with timing and CSS properties.
        /// </summary>
        public SlotTransition CalculateTransitions(List<CardSlot> beforeSlots, List<CardSlot> afterSlots)
        {
            var transition = new SlotTransition
            {
                BeforeSlots = beforeSlots ?? new List<CardSlot>(),
                AfterSlots = afterSlots ?? new List<CardSlot>(),
                CardTransitions = new List<CardSlotTransition>()
            };

            // Create lookup dictionaries for efficient matching
            var beforeCardToSlot = transition.BeforeSlots
                .Where(s => s.IsOccupied && s.Card != null)
                .ToDictionary(s => GetCardKey(s.Card), s => s);

            var afterCardToSlot = transition.AfterSlots
                .Where(s => s.IsOccupied && s.Card != null)
                .ToDictionary(s => GetCardKey(s.Card), s => s);

            // Animation timing coordination
            int exitStaggerIndex = 0;
            int repositionStaggerIndex = 0;
            int enterStaggerIndex = 0;

            // Find cards that moved between slots
            foreach (var card in beforeCardToSlot.Keys.Union(afterCardToSlot.Keys))
            {
                bool existedBefore = beforeCardToSlot.ContainsKey(card);
                bool existsAfter = afterCardToSlot.ContainsKey(card);

                CardSlotTransitionType transitionType;
                CardSlot fromSlot = null;
                CardSlot toSlot = null;

                if (existedBefore && existsAfter)
                {
                    fromSlot = beforeCardToSlot[card];
                    toSlot = afterCardToSlot[card];

                    // Check if card moved to different slot
                    if (fromSlot.Address != toSlot.Address ||
                        !PositionsEqual(fromSlot.Position, toSlot.Position))
                    {
                        transitionType = CardSlotTransitionType.Moved;
                    }
                    else
                    {
                        transitionType = CardSlotTransitionType.Stayed;
                    }
                }
                else if (existedBefore && !existsAfter)
                {
                    fromSlot = beforeCardToSlot[card];
                    transitionType = CardSlotTransitionType.Removed;
                }
                else if (!existedBefore && existsAfter)
                {
                    toSlot = afterCardToSlot[card];
                    transitionType = CardSlotTransitionType.Added;
                }
                else
                {
                    continue; // Should never happen
                }

                var cardTransition = new CardSlotTransition
                {
                    Card = fromSlot?.Card ?? toSlot?.Card,
                    FromSlot = fromSlot,
                    ToSlot = toSlot,
                    TransitionType = transitionType
                };

                // Enhanced: Populate animation timing and CSS coordination
                PopulateAnimationProperties(cardTransition, ref exitStaggerIndex, ref repositionStaggerIndex, ref enterStaggerIndex);

                transition.CardTransitions.Add(cardTransition);
            }

            return transition;
        }

        /// <summary>
        /// Calculate slot arrangement for LISTEN action.
        /// New cards are added to bottom slots, existing slots remain stable.
        /// </summary>
        public SlotTransition CalculateListenTransition(List<CardInstance> existingCards, List<CardInstance> newCards)
        {
            var beforeSlots = CalculateSlots(existingCards);
            var allCards = new List<CardInstance>();

            if (existingCards != null)
                allCards.AddRange(existingCards);
            if (newCards != null)
                allCards.AddRange(newCards);

            var afterSlots = CalculateSlots(allCards);

            return CalculateTransitions(beforeSlots, afterSlots);
        }

        /// <summary>
        /// Calculate slot arrangement for SPEAK action.
        /// Played card is removed, remaining cards collapse up, new cards added at bottom.
        /// </summary>
        public SlotTransition CalculateSpeakTransition(List<CardInstance> beforeCards, CardInstance playedCard,
            List<CardInstance> effectCards)
        {
            var beforeSlots = CalculateSlots(beforeCards);

            // Calculate remaining cards after played card is removed
            var remainingCards = beforeCards?.Where(c => GetCardKey(c) != GetCardKey(playedCard)).ToList()
                ?? new List<CardInstance>();

            // Add effect cards at the bottom
            var afterCards = new List<CardInstance>(remainingCards);
            if (effectCards != null)
                afterCards.AddRange(effectCards);

            var afterSlots = CalculateSlots(afterCards);

            return CalculateTransitions(beforeSlots, afterSlots);
        }

        /// <summary>
        /// Get expansion slots needed for new cards.
        /// Returns virtual slots that don't exist yet but will be needed.
        /// </summary>
        public List<CardSlot> GetExpansionSlots(List<CardSlot> existingSlots, List<CardInstance> newCards)
        {
            var expansionSlots = new List<CardSlot>();

            if (newCards == null || !newCards.Any())
                return expansionSlots;

            int startAddress = existingSlots?.Count ?? 0;

            for (int i = 0; i < newCards.Count; i++)
            {
                var slot = new CardSlot
                {
                    Address = startAddress + i,
                    Position = new Vector2(0, (startAddress + i) * SLOT_SPACING),
                    Card = newCards[i],
                    IsOccupied = true,
                    IsExpansion = true // Mark as expansion slot for animation purposes
                };

                expansionSlots.Add(slot);
            }

            return expansionSlots;
        }

        /// <summary>
        /// Calculate the total height needed for given number of slots.
        /// </summary>
        public int CalculateContainerHeight(int slotCount)
        {
            if (slotCount == 0) return 0;
            return slotCount * SLOT_SPACING;
        }

        /// <summary>
        /// Calculate total duration needed for a complete slot transition sequence.
        /// Accounts for all animation phases: exits, repositions, and entrances.
        /// </summary>
        public int CalculateSequenceDuration(SlotTransition transition)
        {
            if (transition?.CardTransitions == null || !transition.CardTransitions.Any())
                return 0;

            int maxCompletionTime = 0;

            foreach (var cardTransition in transition.CardTransitions)
            {
                int completionTime = cardTransition.StartDelay + cardTransition.Duration;
                maxCompletionTime = Math.Max(maxCompletionTime, completionTime);
            }

            // Add small buffer for CSS timing precision
            return maxCompletionTime + 50;
        }

        /// <summary>
        /// Get all cards that need presentation state management during transition.
        /// Includes cards that are animating (exiting, repositioning, entering).
        /// </summary>
        public List<CardSlotTransition> GetAnimatingTransitions(SlotTransition transition)
        {
            if (transition?.CardTransitions == null)
                return new List<CardSlotTransition>();

            return transition.CardTransitions
                .Where(t => t.IsAnimating)
                .OrderBy(t => t.StartDelay)
                .ToList();
        }

        /// <summary>
        /// Create ghost card presentations for removed cards.
        /// These cards exist only for animation purposes and should be rendered as ghosts.
        /// </summary>
        public List<CardSlotTransition> CreateGhostPresentations(SlotTransition transition)
        {
            if (transition?.CardTransitions == null)
                return new List<CardSlotTransition>();

            return transition.CardTransitions
                .Where(t => t.IsGhost)
                .ToList();
        }

        /// <summary>
        /// Get a unique key for a card instance to track it across transitions.
        /// </summary>
        private string GetCardKey(CardInstance card)
        {
            if (card == null) return "";
            return card.InstanceId ?? card.Id ?? "";
        }

        /// <summary>
        /// Populate animation timing and CSS coordination properties for a card transition.
        /// </summary>
        private void PopulateAnimationProperties(CardSlotTransition cardTransition,
            ref int exitStaggerIndex, ref int repositionStaggerIndex, ref int enterStaggerIndex)
        {

            switch (cardTransition.TransitionType)
            {
                case CardSlotTransitionType.Removed:
                    cardTransition.StartDelay = EXIT_BASE_DELAY_MS + (exitStaggerIndex * STAGGER_DELAY_MS);
                    cardTransition.Duration = EXIT_DURATION_MS;
                    cardTransition.AnimationClass = "choreography-exit";
                    cardTransition.StaggerClass = $"stagger-{exitStaggerIndex + 1}";
                    cardTransition.PresentationState = CardPresentationState.Exiting;

                    if (cardTransition.FromSlot != null)
                    {
                        cardTransition.CSSTransformFrom = $"translateY({cardTransition.FromSlot.Position.Y}px)";
                        cardTransition.CSSTransformTo = "translateY(-100px) scale(0.8) rotateY(15deg)";
                    }

                    exitStaggerIndex++;
                    break;

                case CardSlotTransitionType.Moved:
                    cardTransition.StartDelay = REPOSITION_BASE_DELAY_MS + (repositionStaggerIndex * STAGGER_DELAY_MS);
                    cardTransition.Duration = REPOSITION_DURATION_MS;
                    cardTransition.AnimationClass = "choreography-reposition";
                    cardTransition.StaggerClass = $"stagger-{repositionStaggerIndex + 1}";
                    cardTransition.PresentationState = CardPresentationState.Repositioning;

                    if (cardTransition.FromSlot != null && cardTransition.ToSlot != null)
                    {
                        cardTransition.CSSTransformFrom = $"translateY({cardTransition.FromSlot.Position.Y}px)";
                        cardTransition.CSSTransformTo = $"translateY({cardTransition.ToSlot.Position.Y}px)";
                    }

                    repositionStaggerIndex++;
                    break;

                case CardSlotTransitionType.Added:
                    cardTransition.StartDelay = ENTER_BASE_DELAY_MS + (enterStaggerIndex * STAGGER_DELAY_MS);
                    cardTransition.Duration = ENTER_DURATION_MS;
                    cardTransition.AnimationClass = "choreography-enter";
                    cardTransition.StaggerClass = $"stagger-{enterStaggerIndex + 1}";
                    cardTransition.PresentationState = CardPresentationState.Entering;

                    if (cardTransition.ToSlot != null)
                    {
                        cardTransition.CSSTransformFrom = "translateY(-100px) scale(0.8)";
                        cardTransition.CSSTransformTo = $"translateY({cardTransition.ToSlot.Position.Y}px) scale(1)";
                    }

                    enterStaggerIndex++;
                    break;

                case CardSlotTransitionType.Stayed:
                default:
                    cardTransition.StartDelay = 0;
                    cardTransition.Duration = 0;
                    cardTransition.AnimationClass = "";
                    cardTransition.StaggerClass = "";
                    cardTransition.PresentationState = CardPresentationState.Static;

                    if (cardTransition.FromSlot != null)
                    {
                        cardTransition.CSSTransformFrom = $"translateY({cardTransition.FromSlot.Position.Y}px)";
                        cardTransition.CSSTransformTo = $"translateY({cardTransition.FromSlot.Position.Y}px)";
                    }
                    break;
            }
        }

        /// <summary>
        /// Check if two positions are effectively equal (accounting for floating point precision).
        /// </summary>
        private bool PositionsEqual(Vector2 pos1, Vector2 pos2, float tolerance = 0.1f)
        {
            return Math.Abs(pos1.X - pos2.X) < tolerance && Math.Abs(pos1.Y - pos2.Y) < tolerance;
        }
    }

    /// <summary>
    /// Represents a virtual slot where a card can be positioned.
    /// Slots have stable addresses independent of their contents.
    /// </summary>
    public class CardSlot
    {
        public int Address { get; set; }           // Stable slot identifier (0, 1, 2, ...)
        public Vector2 Position { get; set; }      // Physical position on screen
        public CardInstance Card { get; set; }     // Card currently in this slot (if any)
        public bool IsOccupied { get; set; }       // Whether slot contains a card
        public bool IsExpansion { get; set; }      // Whether this is a new expansion slot
    }

    /// <summary>
    /// Complete transition between two slot arrangements.
    /// Contains all information needed to animate the change.
    /// </summary>
    public class SlotTransition
    {
        public List<CardSlot> BeforeSlots { get; set; } = new();
        public List<CardSlot> AfterSlots { get; set; } = new();
        public List<CardSlotTransition> CardTransitions { get; set; } = new();

        /// <summary>
        /// Get all cards that need to move during this transition.
        /// </summary>
        public List<CardSlotTransition> GetMovingCards()
        {
            return CardTransitions.Where(t =>
                t.TransitionType == CardSlotTransitionType.Moved ||
                t.TransitionType == CardSlotTransitionType.Added ||
                t.TransitionType == CardSlotTransitionType.Removed).ToList();
        }

        /// <summary>
        /// Get cards that are entering the hand.
        /// </summary>
        public List<CardSlotTransition> GetEnteringCards()
        {
            return CardTransitions.Where(t => t.TransitionType == CardSlotTransitionType.Added).ToList();
        }

        /// <summary>
        /// Get cards that are leaving the hand.
        /// </summary>
        public List<CardSlotTransition> GetExitingCards()
        {
            return CardTransitions.Where(t => t.TransitionType == CardSlotTransitionType.Removed).ToList();
        }

        /// <summary>
        /// Get cards that are moving to new positions within the hand.
        /// </summary>
        public List<CardSlotTransition> GetRepositioningCards()
        {
            return CardTransitions.Where(t => t.TransitionType == CardSlotTransitionType.Moved).ToList();
        }

        /// <summary>
        /// Calculate total sequence duration for this transition.
        /// </summary>
        public int GetTotalDuration()
        {
            if (CardTransitions == null || !CardTransitions.Any())
                return 0;

            int maxCompletionTime = 0;

            foreach (var transition in CardTransitions)
            {
                int completionTime = transition.StartDelay + transition.Duration;
                maxCompletionTime = Math.Max(maxCompletionTime, completionTime);
            }

            return maxCompletionTime + 50; // Buffer for CSS timing precision
        }

        /// <summary>
        /// Get cards organized by animation phase for coordinated rendering.
        /// </summary>
        public (List<CardSlotTransition> exiting, List<CardSlotTransition> repositioning, List<CardSlotTransition> entering) GetCardsByPhase()
        {
            var exiting = GetExitingCards();
            var repositioning = GetRepositioningCards();
            var entering = GetEnteringCards();

            return (exiting, repositioning, entering);
        }
    }

    /// <summary>
    /// Represents how a single card transitions between slots with animation coordination.
    /// ENHANCED: Includes timing, CSS transforms, and presentation state management.
    /// </summary>
    public class CardSlotTransition
    {
        public CardInstance Card { get; set; }
        public CardSlot FromSlot { get; set; }     // Slot card is moving from (null if entering)
        public CardSlot ToSlot { get; set; }       // Slot card is moving to (null if exiting)
        public CardSlotTransitionType TransitionType { get; set; }

        // ENHANCED: Animation timing coordination
        public int StartDelay { get; set; }        // Delay before animation starts (ms)
        public int Duration { get; set; }          // Animation duration (ms)

        // ENHANCED: CSS transform coordination
        public string CSSTransformFrom { get; set; }  // Starting CSS transform
        public string CSSTransformTo { get; set; }    // Target CSS transform
        public string AnimationClass { get; set; }    // choreography-exit, choreography-enter, etc.
        public string StaggerClass { get; set; }      // stagger-1, stagger-2, etc.

        // ENHANCED: Presentation state management
        public CardPresentationState PresentationState { get; set; }

        /// <summary>
        /// Whether this card should be rendered as a ghost (removed but still animating).
        /// </summary>
        public bool IsGhost => TransitionType == CardSlotTransitionType.Removed;

        /// <summary>
        /// Whether this card is currently animating.
        /// </summary>
        public bool IsAnimating => TransitionType != CardSlotTransitionType.Stayed;

        /// <summary>
        /// Get the complete CSS class string for this transition animation.
        /// Combines animation class and stagger class for proper CSS application.
        /// </summary>
        public string GetCSSClasses()
        {
            var classes = new List<string>();

            if (!string.IsNullOrEmpty(AnimationClass))
                classes.Add(AnimationClass);

            if (!string.IsNullOrEmpty(StaggerClass))
                classes.Add(StaggerClass);

            return string.Join(" ", classes);
        }

        /// <summary>
        /// Get CSS custom properties for this transition animation.
        /// Provides CSS variables for animation timing and transforms.
        /// </summary>
        public string GetCSSVariables()
        {
            var variables = new List<string>();

            variables.Add($"--start-delay: {StartDelay}ms");
            variables.Add($"--duration: {Duration}ms");

            if (!string.IsNullOrEmpty(CSSTransformFrom))
                variables.Add($"--transform-from: {CSSTransformFrom}");

            if (!string.IsNullOrEmpty(CSSTransformTo))
                variables.Add($"--transform-to: {CSSTransformTo}");

            return string.Join("; ", variables);
        }
    }

    /// <summary>
    /// Types of transitions a card can undergo between slot arrangements.
    /// </summary>
    public enum CardSlotTransitionType
    {
        Stayed,      // Card remained in same slot
        Moved,       // Card moved to different slot
        Added,       // Card was added to a new slot
        Removed      // Card was removed from its slot
    }

    /// <summary>
    /// Presentation states for card animation coordination.
    /// Maps slot transitions to visual animation states.
    /// </summary>
    public enum CardPresentationState
    {
        Static,           // Normal card in stable position
        Exiting,          // Card being removed (ghost card)
        Repositioning,    // Card moving to new slot position
        Entering          // Card entering from deck
    }

    /// <summary>
    /// Vector2 structure for slot positioning.
    /// </summary>
    public struct Vector2
    {
        public float X { get; set; }
        public float Y { get; set; }

        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }
    }
}