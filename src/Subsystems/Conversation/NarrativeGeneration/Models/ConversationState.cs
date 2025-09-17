using System.Collections.Generic;

/// <summary>
/// Wrapper for conversation mechanical state used in narrative generation.
/// Contains all the game state information needed to generate contextually appropriate narratives.
/// </summary>
public class ConversationState
{
    /// <summary>
    /// Current flow value (0-24 internal scale).
    /// Represents connection distance - higher values indicate closer connection.
    /// </summary>
    public int Flow { get; set; }

    /// <summary>
    /// Current rapport level (-50 to +50).
    /// Affects success chances and narrative tone during conversation.
    /// </summary>
    public int Rapport { get; set; }

    /// <summary>
    /// Current conversation atmosphere affecting all actions.
    /// Examples: Neutral, Prepared, Focused, Volatile, etc.
    /// </summary>
    public AtmosphereType Atmosphere { get; set; }

    /// <summary>
    /// Currently available focus points for this turn.
    /// Used to determine which cards the player can afford to play.
    /// </summary>
    public int Focus { get; set; }

    /// <summary>
    /// Remaining patience before conversation must end.
    /// Affects narrative urgency and NPC behavior.
    /// </summary>
    public int Patience { get; set; }

    /// <summary>
    /// Current connection state based on flow value.
    /// Determines draw capacity, focus capacity, and narrative accessibility.
    /// </summary>
    public ConnectionState CurrentState { get; set; }

    /// <summary>
    /// Current topic layer being discussed.
    /// Tracks progression from Deflection → Gateway → Core.
    /// </summary>
    public TopicLayer CurrentTopicLayer { get; set; } = TopicLayer.Deflection;

    /// <summary>
    /// Highest topic layer reached in this conversation.
    /// Prevents regression to earlier layers.
    /// </summary>
    public TopicLayer HighestTopicLayerReached { get; set; } = TopicLayer.Deflection;

    /// <summary>
    /// Conversation beats that have occurred.
    /// Used to track progression and prevent repetition.
    /// </summary>
    public HashSet<ConversationBeat> CompletedBeats { get; set; } = new HashSet<ConversationBeat>();

    /// <summary>
    /// Number of turns spent at current topic layer.
    /// Used to vary responses and prevent stagnation.
    /// </summary>
    public int TurnsAtCurrentLayer { get; set; } = 0;

    /// <summary>
    /// Total conversation turns.
    /// Used for pacing and variety.
    /// </summary>
    public int TotalTurns { get; set; } = 0;

    /// <summary>
    /// Previous turns in this conversation for context.
    /// Contains NPC dialogue and player actions to provide AI with conversation flow.
    /// </summary>
    public List<string> ConversationHistory { get; set; } = new List<string>();
}