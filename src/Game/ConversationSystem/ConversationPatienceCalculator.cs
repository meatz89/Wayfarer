using System;
using System.Collections.Generic;
using System.Linq;
using Wayfarer.GameState;

/// <summary>
/// Calculates starting patience/focus for conversations based on emotional state and relationships.
/// Integrates emotional state effects while preserving human authenticity.
/// </summary>
public static class ConversationPatienceCalculator
{
    /// <summary>
    /// Calculate starting patience for a conversation based on NPC emotional state and relationship.
    /// This becomes the FocusPoints in ConversationState.
    /// </summary>
    public static int CalculateStartingPatience(
        NPC npc,
        NPCEmotionalState emotionalState,
        Dictionary<ConnectionType, int> playerTokens)
    {
        if (npc == null) throw new ArgumentNullException(nameof(npc));
        if (playerTokens == null) throw new ArgumentNullException(nameof(playerTokens));

        // Base patience derived from NPC personality
        int basePatience = GetBasePatience(npc.PersonalityType);

        // Trust tokens add to patience (deeper relationships = more conversation depth)
        int trustBonus = playerTokens.ContainsKey(ConnectionType.Trust)
            ? playerTokens[ConnectionType.Trust]
            : 0;

        // Negative cards in deck reduce starting patience
        double deckPenalty = npc.ConversationDeck?.GetPatiencePenalty() ?? 0;

        // Emotional state affects patience based on human stress responses
        int emotionalPenalty = GetEmotionalStatePenalty(emotionalState);

        // Calculate final patience ensuring minimum viable conversation
        int result = basePatience + trustBonus - emotionalPenalty - (int)deckPenalty;

        // Ensure minimum patience for meaningful interaction
        return Math.Max(1, result);
    }

    /// <summary>
    /// Get patience penalty based on emotional state.
    /// Represents how stress and emotional pressure affect conversation capacity.
    /// </summary>
    private static int GetEmotionalStatePenalty(NPCEmotionalState state)
    {
        return state switch
        {
            NPCEmotionalState.DESPERATE => 3,  // High stress severely limits patience
            NPCEmotionalState.ANXIOUS => 1,    // Mild stress reduces patience slightly
            NPCEmotionalState.CALCULATING => 0, // Calm state, normal patience
            NPCEmotionalState.WITHDRAWN => 2,   // Disengagement reduces patience
            NPCEmotionalState.HOSTILE => throw new InvalidOperationException(
                "HOSTILE NPCs cannot engage in conversation"),
            _ => 0
        };
    }

    /// <summary>
    /// Get narrative explanation for patience level changes due to emotional state.
    /// Used for UI to explain why conversations are harder without showing formulas.
    /// </summary>
    public static string GetPatienceNarrative(NPCEmotionalState state, string npcName)
    {
        if (string.IsNullOrEmpty(npcName)) npcName = "They";

        return state switch
        {
            NPCEmotionalState.DESPERATE => $"{npcName} seems distracted and keeps checking the time",
            NPCEmotionalState.ANXIOUS => $"{npcName} appears worried about something pressing",
            NPCEmotionalState.CALCULATING => null, // Normal state needs no explanation
            NPCEmotionalState.WITHDRAWN => $"{npcName} seems distant and preoccupied",
            NPCEmotionalState.HOSTILE => $"{npcName} refuses to speak with you",
            _ => null
        };
    }

    /// <summary>
    /// Check if NPC can engage in conversation based on emotional state.
    /// </summary>
    public static bool CanConverse(NPCEmotionalState state)
    {
        return state != NPCEmotionalState.HOSTILE;
    }

    /// <summary>
    /// Get base patience for NPC based on personality type.
    /// Based on user stories indicating personality determines base patience (3-10).
    /// </summary>
    private static int GetBasePatience(PersonalityType personalityType)
    {
        return personalityType switch
        {
            PersonalityType.DEVOTED => 8,     // High patience for personal connections
            PersonalityType.MERCANTILE => 6,  // Moderate patience, time is money
            PersonalityType.PROUD => 5,       // Lower patience, expects respect
            PersonalityType.CUNNING => 7,     // Patient for information gathering
            PersonalityType.STEADFAST => 8,   // High patience, steady and reliable
            _ => 6  // Default baseline
        };
    }
}