using System.Collections.Generic;

/// <summary>
/// Strongly-typed context for narrative prompt generation.
/// Replaces Dictionary<string, object> usage in PromptBuilder.
/// </summary>
public class NarrativePromptContext
{
    // Mechanical values
    public string Flow { get; set; } = "0";
    public string Rapport { get; set; } = "0";
    public string Atmosphere { get; set; } = "0";
    public string InitiativeAvailable { get; set; } = "0";
    public string Patience { get; set; } = "0";
    public string TurnCount { get; set; } = "0";
    public string TopicLayer { get; set; } = "0";

    // NPC properties
    public string NpcName { get; set; } = "Unknown";
    public string NpcPersonality { get; set; } = "Unknown";
    public string NpcCrisis { get; set; } = "None";
    public string NpcActivity { get; set; } = "Current activity";
    public string NpcEmotionalState { get; set; } = "Determined by flow and rapport";
    public string CurrentTopic { get; set; } = "General conversation";

    // Card properties
    public string CardCount { get; set; } = "0";
    public string InitiativePattern { get; set; } = "Unknown";

    // Boolean flags (stored as lowercase strings for template compatibility)
    public string HasObservation { get; set; } = "false";

    // Template-specific content
    public string CardSummary { get; set; }
    public string CardsDetail { get; set; }

    // Conversation-specific
    public string ConversationType { get; set; } = "standard";
    public string ConversationHistory { get; set; } = "No previous dialogue.";

    /// <summary>
    /// Convert to dictionary for template processing compatibility
    /// </summary>
    public Dictionary<string, object> ToDictionary()
    {
        return new Dictionary<string, object>
        {
            ["flow"] = Flow,
            ["rapport"] = Rapport,
            ["atmosphere"] = Atmosphere,
            ["initiative_available"] = InitiativeAvailable,
            ["patience"] = Patience,
            ["turn_count"] = TurnCount,
            ["topic_layer"] = TopicLayer,
            ["npc_name"] = NpcName,
            ["npc_personality"] = NpcPersonality,
            ["npc_crisis"] = NpcCrisis,
            ["npc_activity"] = NpcActivity,
            ["npc_emotional_state"] = NpcEmotionalState,
            ["current_topic"] = CurrentTopic,
            ["card_count"] = CardCount,
            ["initiative_pattern"] = InitiativePattern,
            ["has_observation"] = HasObservation,
            ["card_summary"] = CardSummary,
            ["cards_detail"] = CardsDetail,
            ["conversation_type"] = ConversationType,
            ["conversation_history"] = ConversationHistory
        };
    }
}
