using System;
using System.Collections.Generic;
using System.Linq;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Wayfarer.Services.Conversation
{
    /// <summary>
    /// UI Shadow State Manager - manages the split between real gamestate and animation display state.
    /// Enhanced with slot-aware presentation models for coordinated card animations.
    ///
    /// ENHANCED PRINCIPLES:
    /// - Presentation Models: CardDisplayInfo objects bridge game data with animation states
    /// - Slot Awareness: Each card has stable slot address and position coordinates
    /// - Animation Coordination: Presentation models contain CSS classes, transforms, and timing
    /// - Ghost Rendering: Removed cards persist as ghost presentations during exit animations
    ///
    /// CORE PRINCIPLE: During animations, UI shows a "shadow state" that preserves visual elements
    /// for smooth transitions, while real gamestate updates immediately. After animations complete,
    /// shadow state syncs back to real gamestate.
    ///
    /// This solves the fundamental conflict between reactive UI frameworks (which remove elements
    /// when data changes) and CSS animations (which require elements to persist throughout transitions).
    /// </summary>
    public class ConversationUIStateManager
    {
        private List<CardInstance> _shadowState;
        private bool _isInAnimationTransaction;

        /// <summary>
        /// Capture current visual state as baseline before game action executes.
        /// This preserves the current card layout for animation purposes.
        /// </summary>
        public void CaptureBaseline(IReadOnlyList<CardInstance> currentCards)
        {
            if (_isInAnimationTransaction)
            {
                // Already in transaction, don't capture new baseline
                return;
            }

            _shadowState = currentCards?.ToList() ?? new List<CardInstance>();
            _isInAnimationTransaction = true;

            Console.WriteLine($"[ConversationUIStateManager] Captured baseline with {_shadowState.Count} cards");
        }

        /// <summary>
        /// Get cards for UI display with slot-aware presentation models.
        /// During animations, returns shadow state with animation coordination.
        /// Otherwise returns real gamestate with static positioning.
        /// </summary>
        public List<CardDisplayInfo> GetDisplayCards(ConversationFacade conversationFacade)
        {
            List<CardInstance> cardsToDisplay;

            if (_isInAnimationTransaction && _shadowState != null)
            {
                // During animation: show shadow state with presentation models
                cardsToDisplay = _shadowState;
                Console.WriteLine($"[ConversationUIStateManager] Returning shadow state with {cardsToDisplay.Count} cards");
            }
            else
            {
                // Normal state: show real gamestate with static presentation
                cardsToDisplay = conversationFacade.GetHandCards().ToList();
                Console.WriteLine($"[ConversationUIStateManager] Returning real state with {cardsToDisplay.Count} cards");
            }

            return CreateDisplayCards(cardsToDisplay);
        }

        /// <summary>
        /// Get presentation models with active animation coordination.
        /// Used during choreography to provide cards with animation states applied.
        /// </summary>
        public List<CardDisplayInfo> GetAnimatedPresentationModels(SlotTransition slotTransition)
        {
            if (slotTransition?.CardTransitions == null)
                return GetDisplayCards(null); // Fallback to static display

            var presentationModels = new List<CardDisplayInfo>();

            // Create presentation models for all cards in transition
            foreach (var cardTransition in slotTransition.CardTransitions)
            {
                var displayInfo = new CardDisplayInfo
                {
                    Card = cardTransition.Card,
                    IsAnimating = cardTransition.IsAnimating,
                    IsExhausting = false,

                    // Slot-aware positioning
                    SlotAddress = cardTransition.ToSlot?.Address ?? cardTransition.FromSlot?.Address ?? 0,
                    SlotPosition = cardTransition.ToSlot?.Position ?? cardTransition.FromSlot?.Position ?? new Vector2(0, 0),
                    PresentationState = cardTransition.PresentationState,

                    // Legacy animation properties (for compatibility)
                    AnimationDelay = cardTransition.StartDelay / 1000.0, // Convert ms to seconds
                    AnimationDirection = GetAnimationDirection(cardTransition),
                    AnimationState = GetAnimationState(cardTransition),

                    // Enhanced animation coordination
                    AnimationDelayMs = cardTransition.StartDelay,
                    AnimationDurationMs = cardTransition.Duration,
                    CSSTransform = cardTransition.CSSTransformTo ?? "",
                    CSSAnimationClass = cardTransition.AnimationClass ?? "",
                    CSSStaggerClass = cardTransition.StaggerClass ?? "",

                    // Ghost presentation for removed cards
                    IsGhost = cardTransition.IsGhost,
                    TransitionType = cardTransition.TransitionType
                };

                presentationModels.Add(displayInfo);
            }

            return SortCardsWithPromiseFirst(presentationModels);
        }

        /// <summary>
        /// Determine animation direction from card transition.
        /// </summary>
        private string GetAnimationDirection(CardSlotTransition cardTransition)
        {
            return cardTransition.TransitionType switch
            {
                CardSlotTransitionType.Removed => "exit",
                CardSlotTransitionType.Added => "enter",
                CardSlotTransitionType.Moved => "reposition",
                CardSlotTransitionType.Stayed => "static",
                _ => "static"
            };
        }

        /// <summary>
        /// Determine animation state from card transition.
        /// </summary>
        private string GetAnimationState(CardSlotTransition cardTransition)
        {
            return cardTransition.PresentationState switch
            {
                CardPresentationState.Exiting => "exiting",
                CardPresentationState.Entering => "entering",
                CardPresentationState.Repositioning => "repositioning",
                CardPresentationState.Static => "static",
                _ => "static"
            };
        }

        /// <summary>
        /// Sync UI to real gamestate after animations complete.
        /// Clears shadow state and returns to normal display mode.
        /// </summary>
        public void SyncToRealState()
        {
            if (!_isInAnimationTransaction)
            {
                return;
            }

            _shadowState?.Clear();
            _shadowState = null;
            _isInAnimationTransaction = false;

            Console.WriteLine("[ConversationUIStateManager] Synced to real state");
        }

        /// <summary>
        /// Check if currently in animation transaction (showing shadow state).
        /// </summary>
        public bool IsAnimating()
        {
            return _isInAnimationTransaction;
        }

        /// <summary>
        /// Handle animation started event - ensures shadow state mode is active.
        /// </summary>
        public void OnAnimationStarted()
        {
            // Shadow state should already be captured, this is just a confirmation
            if (!_isInAnimationTransaction)
            {
                Console.WriteLine("[ConversationUIStateManager] WARNING: Animation started but no shadow state captured");
            }
        }

        /// <summary>
        /// Handle animation completed event - triggers sync to real state.
        /// </summary>
        public void OnAnimationCompleted()
        {
            SyncToRealState();
        }

        /// <summary>
        /// Force immediate sync for emergency situations (e.g., action interruption).
        /// </summary>
        public void ForceSync()
        {
            Console.WriteLine("[ConversationUIStateManager] Force sync triggered");
            SyncToRealState();
        }

        /// <summary>
        /// Rollback to real state immediately (for failed actions).
        /// </summary>
        public void RollbackToRealState()
        {
            Console.WriteLine("[ConversationUIStateManager] Rollback triggered");
            SyncToRealState();
        }

        /// <summary>
        /// EDGE CASE: Validate state consistency and cleanup if needed.
        /// Handles scenarios where state may be corrupted or inconsistent.
        /// </summary>
        public bool ValidateAndCleanupState()
        {
            bool wasValid = true;

            // Check for orphaned shadow state (animation transaction without shadow state)
            if (_isInAnimationTransaction && (_shadowState == null || !_shadowState.Any()))
            {
                Console.WriteLine("[ConversationUIStateManager] WARNING: Animation transaction without shadow state - cleaning up");
                _isInAnimationTransaction = false;
                wasValid = false;
            }

            // Check for leaked shadow state (shadow state without animation transaction)
            if (!_isInAnimationTransaction && _shadowState != null && _shadowState.Any())
            {
                Console.WriteLine("[ConversationUIStateManager] WARNING: Shadow state without animation transaction - cleaning up");
                _shadowState.Clear();
                _shadowState = null;
                wasValid = false;
            }

            return wasValid;
        }

        /// <summary>
        /// EDGE CASE: Emergency cleanup for stuck animation states.
        /// Use when normal sync mechanisms fail.
        /// </summary>
        public void EmergencyReset()
        {
            Console.WriteLine("[ConversationUIStateManager] Emergency reset triggered");
            _shadowState?.Clear();
            _shadowState = null;
            _isInAnimationTransaction = false;
        }

        /// <summary>
        /// Create display card objects from card instances with slot-aware presentation states.
        /// Enhanced to work with CardSlotManager for proper animation coordination.
        /// </summary>
        private List<CardDisplayInfo> CreateDisplayCards(List<CardInstance> cards)
        {
            var displayCards = new List<CardDisplayInfo>();

            if (cards != null)
            {
                for (int i = 0; i < cards.Count; i++)
                {
                    var card = cards[i];
                    displayCards.Add(new CardDisplayInfo
                    {
                        Card = card,
                        IsAnimating = _isInAnimationTransaction,
                        IsExhausting = false,

                        // Enhanced: Slot-aware presentation state
                        SlotAddress = i,
                        SlotPosition = new Vector2(0, i * CardSlotManager.SLOT_SPACING),
                        PresentationState = _isInAnimationTransaction ? CardPresentationState.Static : CardPresentationState.Static,

                        // Enhanced: Animation coordination properties
                        AnimationDelay = 0,
                        AnimationDirection = null,
                        AnimationState = null,
                        AnimationDelayMs = 0,
                        AnimationDurationMs = 0,
                        CSSTransform = $"translateY({i * CardSlotManager.SLOT_SPACING}px)",
                        CSSAnimationClass = "",
                        CSSStaggerClass = "",

                        // Enhanced: Ghost and transition properties
                        IsGhost = false,
                        TransitionType = CardSlotTransitionType.Stayed
                    });
                }
            }

            return SortCardsWithPromiseFirst(displayCards);
        }

        /// <summary>
        /// Sort cards so promise/delivery cards appear first, preserving relative order within categories.
        /// </summary>
        private List<CardDisplayInfo> SortCardsWithPromiseFirst(List<CardDisplayInfo> cards)
        {
            var promiseCards = cards.Where(dc => dc.Card.CardType == CardType.Letter).ToList();
            var regularCards = cards.Where(dc => dc.Card.CardType != CardType.Letter).ToList();

            var sorted = new List<CardDisplayInfo>();
            sorted.AddRange(promiseCards);
            sorted.AddRange(regularCards);

            return sorted;
        }
    }
}