/// <summary>
/// Unified interface for ALL player interactions - conversations, observations, location actions
/// This allows us to use the same UI component and attention system for everything
/// </summary>
public interface IInteractiveChoice
{
    /// <summary>Unique identifier for tracking and effects</summary>
    string Id { get; }

    /// <summary>Display text shown to player</summary>
    string DisplayText { get; }


    /// <summary>Time cost in minutes (for location actions)</summary>
    int TimeCostMinutes { get; }

    /// <summary>Type for styling and behavior</summary>
    InteractionType Type { get; }

    /// <summary>Optional preview of mechanical effects</summary>
    List<string> MechanicalPreviews { get; }

    /// <summary>Is this choice currently available?</summary>
    bool IsAvailable { get; }

    /// <summary>Why is it locked? (if not available)</summary>
    string LockReason { get; }

    /// <summary>Visual style hint for UI</summary>
    InteractionStyle Style { get; }

    /// <summary>Execute the interaction and return results</summary>
    InteractionResult Execute(GameWorld gameWorld);
}

public enum InteractionType
{
    // Conversation verbs
    ConversationHelp,
    ConversationNegotiate,
    ConversationInvestigate,
    ConversationFree,  // 0-cost responses

    // Observations
    ObserveEnvironment,
    ObserveNPC,
    ObserveDetail,

    // Location actions  
    LocationMove,
    LocationWait,
    LocationInteract,

    // Special
    SystemAction
}

public enum InteractionStyle
{
    Default,
    Urgent,      // Red/warning styling
    Beneficial,  // Green/positive styling
    Mysterious,  // Purple/investigation styling
    Locked       // Grayed out
}

public class InteractionResult
{
    public bool Success { get; set; }
    public string NarrativeText { get; set; }
    public List<string> SystemMessages { get; set; } = new();
    public InteractionStateChanges StateChanges { get; set; } = new();
    public bool EndsCurrentContext { get; set; }  // Ends conversation or leaves location
}

/// <summary>
/// Strongly typed state changes from interactions
/// </summary>
public class InteractionStateChanges
{
    public int? RapportChange { get; set; }
    public int? TokensGained { get; set; }
    public ConnectionState? NewConnectionState { get; set; }
    public string UnlockedLocation { get; set; }
    public string UnlockedNpc { get; set; }
    public List<string> ItemsGained { get; set; } = new();
    public List<string> ItemsLost { get; set; } = new();
    public ResourceChange ResourceChange { get; set; }
}

/// <summary>
/// Resource changes from interactions
/// </summary>
public class ResourceChange
{
    public int? CoinsChange { get; set; }
    public int? HealthChange { get; set; }
    public int? HungerChange { get; set; }
}