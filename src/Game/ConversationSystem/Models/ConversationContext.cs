using System.Collections.Generic;

/// <summary>
/// Complete context for a conversation, containing all data needed for the conversation flow.
/// This is the single source of truth for conversation state, created atomically before navigation.
/// </summary>
public class ConversationContext
{
    /// <summary>
    /// The ID of the NPC in conversation
    /// </summary>
    public string NpcId { get; set; }
    
    /// <summary>
    /// The NPC object with all their data
    /// </summary>
    public NPC Npc { get; set; }
    
    /// <summary>
    /// Type of conversation (QuickExchange, Standard, Crisis)
    /// </summary>
    public ConversationType Type { get; set; }
    
    /// <summary>
    /// The active conversation session with all game state
    /// </summary>
    public ConversationSession Session { get; set; }
    
    /// <summary>
    /// Observation cards available to play in this conversation
    /// </summary>
    public List<CardInstance> ObservationCards { get; set; }
    
    /// <summary>
    /// Amount of attention spent to start this conversation
    /// </summary>
    public int AttentionSpent { get; set; }
    
    /// <summary>
    /// The NPC's emotional state at conversation start
    /// </summary>
    public EmotionalState InitialState { get; set; }
    
    /// <summary>
    /// The player's current resources for UI display
    /// </summary>
    public PlayerResourceState PlayerResources { get; set; }
    
    /// <summary>
    /// Current location name for UI display
    /// </summary>
    public string LocationName { get; set; }
    
    /// <summary>
    /// Current time display for UI
    /// </summary>
    public string TimeDisplay { get; set; }
    
    /// <summary>
    /// Whether this conversation can be started (all preconditions met)
    /// </summary>
    public bool IsValid { get; set; }
    
    /// <summary>
    /// Error message if conversation cannot be started
    /// </summary>
    public string ErrorMessage { get; set; }
    
    /// <summary>
    /// Letters the player is carrying for this NPC
    /// </summary>
    public List<DeliveryObligation> LettersCarriedForNpc { get; set; }
}