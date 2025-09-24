using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Wayfarer.Services.Conversation
{
    /// <summary>
    /// Enhanced Layout Choreography Engine - Coordinates slot-aware animation sequences
    /// with direct integration to CardSlotManager for precise positioning and timing.
    ///
    /// ENHANCED PRINCIPLES:
    /// - Slot-Based Coordination: Uses CardSlotManager's SlotTransition system directly
    /// - Presentation Layer: Converts slot transitions into presentation models
    /// - No Movement Duplication: Eliminates redundant CardMovement in favor of CardSlotTransition
    /// - Direct CSS Integration: Provides CSS classes and timing directly from slot transitions
    /// - Event Coordination: Manages choreography lifecycle with proper timing
    /// </summary>
    public class LayoutChoreographyEngine
    {
        private readonly List<ChoreographyExecution> _activeExecutions = new();
        private readonly CardSlotManager _slotManager;

        public event Action<ChoreographyExecution> OnChoreographyStarted;
        public event Action<ChoreographyExecution> OnChoreographyCompleted;

        public LayoutChoreographyEngine()
        {
            _slotManager = new CardSlotManager();
        }

        /// <summary>
        /// Create and execute LISTEN choreography using enhanced slot transitions.
        /// Returns presentation models ready for UI rendering.
        /// </summary>
        public async Task<ChoreographyExecution> ExecuteListenChoreographyAsync(List<CardInstance> existingCards, List<CardInstance> newCards)
        {
            var slotTransition = _slotManager.CalculateListenTransition(existingCards, newCards);
            return await ExecuteChoreographyAsync(slotTransition, ChoreographyType.Listen);
        }

        /// <summary>
        /// Create and execute SPEAK choreography using enhanced slot transitions.
        /// Returns presentation models ready for UI rendering.
        /// </summary>
        public async Task<ChoreographyExecution> ExecuteSpeakChoreographyAsync(List<CardInstance> beforeCards,
            CardInstance playedCard, List<CardInstance> effectCards)
        {
            var slotTransition = _slotManager.CalculateSpeakTransition(beforeCards, playedCard, effectCards);
            return await ExecuteChoreographyAsync(slotTransition, ChoreographyType.Speak);
        }

        /// <summary>
        /// Execute choreography from slot transition with proper timing coordination.
        /// This is the core orchestration method that manages the entire animation lifecycle.
        /// </summary>
        public async Task<ChoreographyExecution> ExecuteChoreographyAsync(SlotTransition slotTransition, ChoreographyType type)
        {
            if (slotTransition?.CardTransitions == null || !slotTransition.CardTransitions.Any())
                return null;

            var execution = new ChoreographyExecution
            {
                SlotTransition = slotTransition,
                Type = type,
                StartedAt = DateTime.UtcNow,
                TotalDuration = _slotManager.CalculateSequenceDuration(slotTransition),
                IsActive = true
            };

            _activeExecutions.Add(execution);
            OnChoreographyStarted?.Invoke(execution);

            // Wait for the complete animation sequence
            await Task.Delay(execution.TotalDuration);

            // Mark as completed and notify
            execution.IsActive = false;
            execution.CompletedAt = DateTime.UtcNow;
            _activeExecutions.Remove(execution);
            OnChoreographyCompleted?.Invoke(execution);

            return execution;
        }

        /// <summary>
        /// Get presentation models for active choreography.
        /// Used by UI to render cards with proper animation states.
        /// </summary>
        public List<CardDisplayInfo> GetPresentationModels(SlotTransition slotTransition, ConversationUIStateManager uiStateManager)
        {
            if (slotTransition == null || uiStateManager == null)
                return new List<CardDisplayInfo>();

            return uiStateManager.GetAnimatedPresentationModels(slotTransition);
        }

        /// <summary>
        /// Get active choreography execution if any is running.
        /// Used by UI to check if animations are in progress.
        /// </summary>
        public ChoreographyExecution GetActiveExecution()
        {
            return _activeExecutions.FirstOrDefault();
        }

        /// <summary>
        /// Force complete all active choreography executions.
        /// Used for emergency cleanup or state transitions.
        /// </summary>
        public void ForceCompleteAll()
        {
            foreach (var execution in _activeExecutions.ToList())
            {
                execution.IsActive = false;
                execution.CompletedAt = DateTime.UtcNow;
                OnChoreographyCompleted?.Invoke(execution);
            }
            _activeExecutions.Clear();
        }







    }

    /// <summary>
    /// Enhanced choreography execution that manages slot-based animations.
    /// Replaces legacy ChoreographySequence with direct SlotTransition integration.
    /// </summary>
    public class ChoreographyExecution
    {
        public SlotTransition SlotTransition { get; set; }
        public ChoreographyType Type { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int TotalDuration { get; set; }
        public bool IsActive { get; set; }

        /// <summary>
        /// Get all cards that are currently animating in this execution.
        /// </summary>
        public List<CardSlotTransition> GetAnimatingCards()
        {
            return SlotTransition?.CardTransitions?.Where(t => t.IsAnimating).ToList() ?? new List<CardSlotTransition>();
        }

        /// <summary>
        /// Check if the choreography execution is still within its duration.
        /// </summary>
        public bool IsStillRunning()
        {
            if (!IsActive || CompletedAt.HasValue)
                return false;

            var elapsed = (DateTime.UtcNow - StartedAt).TotalMilliseconds;
            return elapsed < TotalDuration;
        }

        /// <summary>
        /// Get progress percentage of the choreography (0.0 to 1.0).
        /// </summary>
        public double GetProgress()
        {
            if (!IsActive || TotalDuration <= 0)
                return CompletedAt.HasValue ? 1.0 : 0.0;

            var elapsed = (DateTime.UtcNow - StartedAt).TotalMilliseconds;
            return Math.Min(1.0, elapsed / TotalDuration);
        }
    }

    /// <summary>
    /// Types of choreography sequences supported by the engine.
    /// </summary>
    public enum ChoreographyType
    {
        Listen,  // New cards entering from deck
        Speak    // Card exits, stack collapses, new cards enter
    }

}