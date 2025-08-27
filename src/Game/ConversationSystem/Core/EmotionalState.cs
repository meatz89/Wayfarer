using System.Collections.Generic;
using System.Linq;

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
    /// Number of crisis cards injected when listening
    /// </summary>
    public int CrisisCardsInjected { get; init; } = 1;

    /// <summary>
    /// Whether opportunity cards are preserved when listening
    /// </summary>
    public bool PreservesOpportunities { get; init; }

    /// <summary>
    /// Maximum cards that can be played (ONE-CARD RULE: always 1)
    /// </summary>
    public int? MaxCards { get; init; }

    /// <summary>
    /// Whether depth advances automatically each turn (CONNECTED)
    /// </summary>
    public bool AutoAdvanceDepth { get; init; }

    /// <summary>
    /// Set bonuses based on number of cards played (OBSOLETE - ONE-CARD RULE)
    /// Kept for backwards compatibility but always returns 0
    /// </summary>
    public Dictionary<int, int> SetBonuses { get; init; } = new();

    /// <summary>
    /// Card categories that cost 0 weight in this state
    /// </summary>
    public List<CardCategory> FreeWeightCategories { get; init; } = new();

    /// <summary>
    /// Card categories allowed to be played in this state (null = all allowed)
    /// </summary>
    public List<CardCategory> AllowedCategories { get; init; }

    /// <summary>
    /// Whether listening ends the conversation (HOSTILE breakdown)
    /// </summary>
    public bool ListenEndsConversation { get; init; }
    
    /// <summary>
    /// Whether to allow one final turn after this state to play crisis cards
    /// </summary>
    public bool AllowOneFinalTurn { get; init; }
    
    /// <summary>
    /// Whether to check letter deck during Listen (OPEN/CONNECTED)
    /// </summary>
    public bool ChecksLetterDeck { get; init; }
}

/// <summary>
/// Defines the complete rules for each emotional state.
/// This is the heart of the conversation system.
/// </summary>
public static class ConversationRules
{
    public static readonly Dictionary<EmotionalState, StateRuleset> States = new()
    {
        // EXACT POC SPECIFICATIONS
        [EmotionalState.NEUTRAL] = new StateRuleset
        {
            CardsOnListen = 2,  // Listen draws 2 conversation cards
            MaxWeight = 3,      // Speak weight limit 3
            MaxCards = 1,       // Play exactly ONE card
            ListenTransition = EmotionalState.NEUTRAL,  // Stays NEUTRAL
            ChecksLetterDeck = true  // Check letter deck for urgent letters
        },

        [EmotionalState.GUARDED] = new StateRuleset
        {
            CardsOnListen = 1,  // Listen draws 1 conversation card
            MaxWeight = 2,      // Speak weight limit 2
            MaxCards = 1,       // Play exactly ONE card
            ListenTransition = EmotionalState.NEUTRAL,  // Transitions to NEUTRAL
            ChecksLetterDeck = false
        },

        [EmotionalState.OPEN] = new StateRuleset
        {
            CardsOnListen = 3,  // Listen draws 3 conversation cards
            MaxWeight = 3,      // Speak weight limit 3
            MaxCards = 1,       // Play exactly ONE card
            ListenTransition = EmotionalState.OPEN,  // Stays OPEN
            ChecksLetterDeck = true  // CHECK letter deck for trust letters
        },

        [EmotionalState.CONNECTED] = new StateRuleset
        {
            CardsOnListen = 3,  // Listen draws 3 conversation cards
            MaxWeight = 4,      // Speak weight limit 4
            MaxCards = 1,       // Play exactly ONE card
            ListenTransition = EmotionalState.CONNECTED,  // Stays CONNECTED
            ChecksLetterDeck = true,  // CHECK letter deck for ANY letters
            AutoAdvanceDepth = false  // No depth advancement (comfort gates)
        },

        [EmotionalState.TENSE] = new StateRuleset
        {
            CardsOnListen = 1,  // Listen draws 1 conversation card
            MaxWeight = 1,      // Speak weight limit 1
            MaxCards = 1,       // Play exactly ONE card
            ListenTransition = EmotionalState.GUARDED,  // Transitions to GUARDED
            ChecksLetterDeck = true  // Check letter deck for urgent letters
        },

        [EmotionalState.EAGER] = new StateRuleset
        {
            CardsOnListen = 3,  // Listen draws 3 conversation cards
            MaxWeight = 3,      // Speak weight limit 3
            MaxCards = 1,       // Play exactly ONE card
            ListenTransition = EmotionalState.EAGER,  // Stays EAGER
            ChecksLetterDeck = false
        },

        [EmotionalState.OVERWHELMED] = new StateRuleset
        {
            CardsOnListen = 1,  // Listen draws 1 conversation card
            MaxWeight = 1,      // Speak weight limit 1 (gentle statements only)
            MaxCards = 1,       // Play exactly ONE card
            ListenTransition = EmotionalState.NEUTRAL,  // Transitions to NEUTRAL
            ChecksLetterDeck = false
        },

        [EmotionalState.DESPERATE] = new StateRuleset
        {
            CardsOnListen = 2,  // Listen draws 2 conversation cards
            MaxWeight = 1,      // Speak weight limit 1 (but crisis cards override)
            MaxCards = 1,       // Play exactly ONE card
            ListenTransition = EmotionalState.HOSTILE,  // Transitions to HOSTILE
            InjectsCrisis = true,  // INJECT 1 crisis card
            CrisisCardsInjected = 1,
            FreeWeightCategories = new() { CardCategory.CRISIS },  // Crisis cards cost 0 weight
            ChecksLetterDeck = true  // Check for urgent letters in desperate state
        },

        [EmotionalState.HOSTILE] = new StateRuleset
        {
            CardsOnListen = 1,  // Listen draws 1 conversation card
            MaxWeight = 3,      // Speak weight limit 3 (crisis only)
            MaxCards = 1,       // Play exactly ONE card
            ListenTransition = EmotionalState.HOSTILE,  // Stays HOSTILE
            InjectsCrisis = true,  // INJECT 2 crisis cards
            CrisisCardsInjected = 2,
            FreeWeightCategories = new() { CardCategory.CRISIS },
            AllowedCategories = new() { CardCategory.CRISIS },  // ONLY crisis cards allowed
            ListenEndsConversation = true,  // END conversation after listen
            ChecksLetterDeck = false
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
            effects += " • Engaged mood";
        else if (state == EmotionalState.OVERWHELMED)
            effects += " • Needs space";
        else if (state == EmotionalState.CONNECTED)
            effects += " • +2 comfort bonus";

        return effects;
    }

    /// <summary>
    /// Determine initial state based on NPC condition
    /// </summary>
    public static EmotionalState DetermineInitialState(NPC npc, ObligationQueueManager queueManager)
    {
        // Check for urgent letters creating desperate state
        var obligations = queueManager?.GetActiveObligations();
        if (obligations != null && obligations.Any())
        {
            var urgentLetter = obligations
                .Where(o => o.SenderId == npc.ID && o.MinutesUntilDeadline < 360) // <6 hours
                .FirstOrDefault();

            if (urgentLetter != null)
            {
                if (urgentLetter.MinutesUntilDeadline < 180) // <3 hours
                    return EmotionalState.DESPERATE;
                return EmotionalState.TENSE;
            }
        }
        
        // Check for meeting obligations
        var meeting = queueManager?.GetMeetingWithNPC(npc.ID);
        if (meeting != null)
        {
            // Apply urgency rules based on deadline and stakes
            if (meeting.Stakes == StakeType.SAFETY && meeting.DeadlineInMinutes < 360) // <6 hours
                return EmotionalState.DESPERATE;
            if (meeting.DeadlineInMinutes < 180) // <3 hours
                return EmotionalState.DESPERATE;
            if (meeting.DeadlineInMinutes < 720) // <12 hours
                return EmotionalState.TENSE;
        }

        // Check relationship for hostility
        if (npc.PlayerRelationship == NPCRelationship.Betrayed)
            return EmotionalState.HOSTILE;

        // Default based on personality
        return npc.PersonalityType switch
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