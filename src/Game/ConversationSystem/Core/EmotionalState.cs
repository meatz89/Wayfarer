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

    /// <summary>
    /// Set bonuses based on number of cards played (card count -> bonus)
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
            ListenTransition = EmotionalState.NEUTRAL,
            SetBonuses = new() { { 2, 2 }, { 3, 5 }, { 4, 8 } }
        },

        [EmotionalState.GUARDED] = new StateRuleset
        {
            CardsOnListen = 1,
            MaxWeight = 2,
            ListenTransition = EmotionalState.NEUTRAL,
            SetBonuses = new() { { 2, 1 }, { 3, 3 }, { 4, 5 } }
        },

        [EmotionalState.OPEN] = new StateRuleset
        {
            CardsOnListen = 3,
            MaxWeight = 3,
            ListenTransition = EmotionalState.OPEN,
            SetBonuses = new() { { 2, 2 }, { 3, 5 }, { 4, 8 } }
        },

        [EmotionalState.CONNECTED] = new StateRuleset
        {
            CardsOnListen = 3,
            MaxWeight = 4,
            ListenTransition = EmotionalState.CONNECTED,
            AutoAdvanceDepth = true,
            SetBonuses = new() { { 2, 3 }, { 3, 6 }, { 4, 10 } }
        },

        [EmotionalState.TENSE] = new StateRuleset
        {
            CardsOnListen = 1,
            MaxWeight = 1,
            ListenTransition = EmotionalState.GUARDED,
            SetBonuses = new() { { 2, 1 }, { 3, 2 }, { 4, 3 } }
        },

        [EmotionalState.EAGER] = new StateRuleset
        {
            CardsOnListen = 3,
            MaxWeight = 3,
            ListenTransition = EmotionalState.EAGER,
            RequiredCards = 2,
            SetBonus = 3,
            SetBonuses = new() { { 2, 2 }, { 3, 5 }, { 4, 8 } }
        },

        [EmotionalState.OVERWHELMED] = new StateRuleset
        {
            CardsOnListen = 1,
            MaxWeight = 3,
            ListenTransition = EmotionalState.NEUTRAL,
            MaxCards = 1,
            SetBonuses = new() // Empty - no bonuses
        },

        [EmotionalState.DESPERATE] = new StateRuleset
        {
            CardsOnListen = 2,
            MaxWeight = 3,
            ListenTransition = EmotionalState.HOSTILE,
            InjectsCrisis = true,
            CrisisCardsInjected = 1,
            FreeWeightCategories = new() { CardCategory.CRISIS },  // Crisis cards cost 0 weight
            SetBonuses = new() { { 2, 2 }, { 3, 5 }, { 4, 8 } }
        },

        [EmotionalState.HOSTILE] = new StateRuleset
        {
            CardsOnListen = 1,
            MaxWeight = 3,
            ListenTransition = EmotionalState.HOSTILE,
            InjectsCrisis = true,
            CrisisCardsInjected = 2,
            FreeWeightCategories = new() { CardCategory.CRISIS },  // Crisis cards cost 0 weight
            AllowedCategories = new() { CardCategory.CRISIS },  // ONLY crisis cards can be played
            ListenEndsConversation = false,  // Allow one turn to play crisis cards
            AllowOneFinalTurn = true,  // Give player chance to resolve with crisis cards
            SetBonuses = new() // Empty - crisis only, no bonuses
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
        
        // Check for meeting obligations
        var meeting = queueManager.GetMeetingWithNPC(npc.ID);
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