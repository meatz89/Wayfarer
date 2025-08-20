using System.Collections.Generic;
using System.Linq;
using Wayfarer.GameState;

namespace Wayfarer.Game.ConversationSystem.Core
{
    /// <summary>
    /// Emotional states that define complete conversation rulesets.
    /// Each state changes what's possible in LISTEN and SPEAK actions.
    /// This is the core mechanic - states ARE the game rules.
    /// </summary>
    public enum EmotionalState
    {
        NEUTRAL,     // Default balanced state
        GUARDED,     // Closed off, defensive
        OPEN,        // Receptive, willing
        CONNECTED,   // Peak rapport achieved
        TENSE,       // Stressed, constrained
        EAGER,       // Engaged, wants interaction
        OVERWHELMED, // Needs space
        DESPERATE,   // Crisis mode
        HOSTILE      // Cannot converse
    }

    /// <summary>
    /// Complete ruleset for each emotional state.
    /// States define the entire conversation dynamic.
    /// </summary>
    public class StateRuleset
    {
        /// <summary>
        /// How many cards drawn when listening (1-3)
        /// </summary>
        public int CardsOnListen { get; init; }

        /// <summary>
        /// Maximum total weight that can be played (1-4)
        /// </summary>
        public int MaxWeight { get; init; }

        /// <summary>
        /// State transition when LISTEN action taken
        /// </summary>
        public EmotionalState ListenTransition { get; init; }

        /// <summary>
        /// Whether crisis cards are injected when listening
        /// </summary>
        public bool InjectsCrisis { get; init; }

        /// <summary>
        /// Whether opportunity cards are preserved when listening
        /// </summary>
        public bool PreservesOpportunities { get; init; }

        /// <summary>
        /// Minimum cards that must be played (for EAGER)
        /// </summary>
        public int? RequiredCards { get; init; }

        /// <summary>
        /// Maximum cards that can be played (for OVERWHELMED)
        /// </summary>
        public int? MaxCards { get; init; }

        /// <summary>
        /// Whether depth advances automatically each turn (CONNECTED)
        /// </summary>
        public bool AutoAdvanceDepth { get; init; }

        /// <summary>
        /// Bonus comfort for playing sets (EAGER)
        /// </summary>
        public int SetBonus { get; init; }
    }

    /// <summary>
    /// Defines the complete rules for each emotional state.
    /// This is the heart of the conversation system.
    /// </summary>
    public static class ConversationRules
    {
        public static readonly Dictionary<EmotionalState, StateRuleset> States = new()
        {
            [EmotionalState.NEUTRAL] = new StateRuleset
            {
                CardsOnListen = 2,
                MaxWeight = 3,
                ListenTransition = EmotionalState.NEUTRAL
            },

            [EmotionalState.GUARDED] = new StateRuleset
            {
                CardsOnListen = 1,
                MaxWeight = 2,
                ListenTransition = EmotionalState.NEUTRAL
            },

            [EmotionalState.OPEN] = new StateRuleset
            {
                CardsOnListen = 3,
                MaxWeight = 3,
                ListenTransition = EmotionalState.OPEN
            },

            [EmotionalState.CONNECTED] = new StateRuleset
            {
                CardsOnListen = 3,
                MaxWeight = 4,
                ListenTransition = EmotionalState.CONNECTED,
                AutoAdvanceDepth = true
            },

            [EmotionalState.TENSE] = new StateRuleset
            {
                CardsOnListen = 1,
                MaxWeight = 1,
                ListenTransition = EmotionalState.GUARDED
            },

            [EmotionalState.EAGER] = new StateRuleset
            {
                CardsOnListen = 3,
                MaxWeight = 3,
                ListenTransition = EmotionalState.EAGER,
                RequiredCards = 2,
                SetBonus = 3
            },

            [EmotionalState.OVERWHELMED] = new StateRuleset
            {
                CardsOnListen = 1,
                MaxWeight = 3,
                ListenTransition = EmotionalState.NEUTRAL,
                MaxCards = 1
            },

            [EmotionalState.DESPERATE] = new StateRuleset
            {
                CardsOnListen = 2,
                MaxWeight = 3,
                ListenTransition = EmotionalState.HOSTILE,
                InjectsCrisis = true
            },

            [EmotionalState.HOSTILE] = new StateRuleset
            {
                CardsOnListen = 0,
                MaxWeight = 0,
                ListenTransition = EmotionalState.HOSTILE
            }
        };

        /// <summary>
        /// Get display text for state effects
        /// </summary>
        public static string GetStateEffects(EmotionalState state)
        {
            var rules = States[state];
            var effects = $"Draw {rules.CardsOnListen} • Weight limit {rules.MaxWeight}";

            if (state == EmotionalState.DESPERATE)
                effects += " • Crisis free • Listen worsens";
            else if (state == EmotionalState.EAGER)
                effects += " • +3 bonus for sets";
            else if (state == EmotionalState.OVERWHELMED)
                effects += " • Max 1 card only";
            else if (state == EmotionalState.CONNECTED)
                effects += " • Depth advances";

            return effects;
        }

        /// <summary>
        /// Determine initial state based on NPC condition
        /// </summary>
        public static EmotionalState DetermineInitialState(NPC npc, ObligationQueueManager queueManager)
        {
            // Check for urgent letters creating desperate state
            var obligations = queueManager.GetActiveObligations();
            var urgentLetter = obligations
                .Where(o => o.SenderId == npc.ID && o.MinutesUntilDeadline < 360) // <6 hours
                .FirstOrDefault();

            if (urgentLetter != null)
            {
                if (urgentLetter.MinutesUntilDeadline < 180) // <3 hours
                    return EmotionalState.DESPERATE;
                return EmotionalState.TENSE;
            }

            // Check relationship for hostility
            if (npc.PlayerRelationship == NPCRelationship.Betrayed)
                return EmotionalState.HOSTILE;

            // Default based on personality
            return npc.Personality switch
            {
                PersonalityType.DEVOTED => EmotionalState.OPEN,
                PersonalityType.MERCANTILE => EmotionalState.NEUTRAL,
                PersonalityType.PROUD => EmotionalState.GUARDED,
                PersonalityType.CUNNING => EmotionalState.GUARDED,
                PersonalityType.STEADFAST => EmotionalState.NEUTRAL,
                _ => EmotionalState.NEUTRAL
            };
        }
    }
}