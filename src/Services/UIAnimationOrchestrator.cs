using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace Wayfarer.Services
{
    /// <summary>
    /// Global UI Animation Orchestrator - manages animation sequences and input blocking.
    ///
    /// CORE PRINCIPLES:
    /// - Game state changes happen immediately (synchronous)
    /// - UI animations provide delayed visual feedback
    /// - Input is blocked during animation sequences to prevent state corruption
    /// - Supports staggered and overlapping animations for professional polish
    /// </summary>
    public class UIAnimationOrchestrator
    {
        private readonly List<AnimationSequence> _activeSequences = new();
        private readonly Dictionary<string, DateTime> _componentAnimationStates = new();

        /// <summary>
        /// Global flag indicating if any animations are currently playing.
        /// When true, user input should be blocked across all components.
        /// </summary>
        public bool IsAnimating => _activeSequences.Count > 0;

        /// <summary>
        /// Event triggered when animation state changes (for UI updates)
        /// </summary>
        public event Action OnAnimationStateChanged;

        /// <summary>
        /// Start a new animation sequence (e.g., LISTEN card draw, SPEAK card play)
        /// </summary>
        public async Task StartAnimationSequence(string sequenceId, AnimationSequenceType type, int duration = 1000)
        {
            var sequence = new AnimationSequence
            {
                Id = sequenceId,
                Type = type,
                StartTime = DateTime.Now,
                Duration = duration
            };

            _activeSequences.Add(sequence);
            OnAnimationStateChanged?.Invoke();

            // Wait for animation to complete, then remove from active sequences
            await Task.Delay(duration);
            _activeSequences.RemoveAll(s => s.Id == sequenceId);
            OnAnimationStateChanged?.Invoke();
        }

        /// <summary>
        /// Start a LISTEN animation sequence (staggered card draws)
        /// </summary>
        public async Task StartListenSequence(int cardCount)
        {
            string sequenceId = $"listen_{DateTime.Now.Ticks}";
            int totalDuration = 500 + (cardCount * 150); // Base duration + stagger per card

            await StartAnimationSequence(sequenceId, AnimationSequenceType.Listen, totalDuration);
        }

        /// <summary>
        /// Start a SPEAK animation sequence (card exit → effect → new cards if any)
        /// </summary>
        public async Task StartSpeakSequence(bool hasDrawEffect = false, int newCardCount = 0)
        {
            string sequenceId = $"speak_{DateTime.Now.Ticks}";

            // Calculate total duration based on sequence complexity
            int baseDuration = 1250; // Card success/failure flash (1.25s from CSS)
            int effectDuration = hasDrawEffect ? (500 + newCardCount * 150) : 0;
            int totalDuration = baseDuration + effectDuration;

            await StartAnimationSequence(sequenceId, AnimationSequenceType.Speak, totalDuration);
        }

        /// <summary>
        /// Start a card effect animation (drawing cards from spell effects, etc.)
        /// </summary>
        public async Task StartCardEffectSequence(int cardCount)
        {
            string sequenceId = $"effect_{DateTime.Now.Ticks}";
            int duration = 500 + (cardCount * 150); // Staggered based on card count

            await StartAnimationSequence(sequenceId, AnimationSequenceType.CardEffect, duration);
        }

        /// <summary>
        /// Check if a specific component should be disabled due to animations
        /// </summary>
        public bool ShouldDisableInput(string componentId = "")
        {
            return IsAnimating;
        }

        /// <summary>
        /// Get current animation status for debugging/display
        /// </summary>
        public string GetAnimationStatus()
        {
            if (!IsAnimating) return "Ready";

            var activeTypes = new List<string>();
            foreach (var sequence in _activeSequences)
            {
                activeTypes.Add(sequence.Type.ToString());
            }

            return $"Animating: {string.Join(", ", activeTypes)}";
        }

        /// <summary>
        /// Force clear all animations (for emergency situations)
        /// </summary>
        public void ClearAllAnimations()
        {
            _activeSequences.Clear();
            _componentAnimationStates.Clear();
            OnAnimationStateChanged?.Invoke();
        }
    }

    /// <summary>
    /// Represents an active animation sequence
    /// </summary>
    public class AnimationSequence
    {
        public string Id { get; set; }
        public AnimationSequenceType Type { get; set; }
        public DateTime StartTime { get; set; }
        public int Duration { get; set; } // in milliseconds
        public bool IsComplete => DateTime.Now.Subtract(StartTime).TotalMilliseconds >= Duration;
    }

    /// <summary>
    /// Types of animation sequences
    /// </summary>
    public enum AnimationSequenceType
    {
        Listen,      // Drawing cards from LISTEN action
        Speak,       // Playing a card with SPEAK action
        CardEffect,  // Card effects that draw additional cards
        Exchange,    // Exchange card animations
        Travel,      // Travel-related animations
        Other        // Generic animation sequence
    }
}