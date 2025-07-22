/// <summary>
/// Context for AI-driven conversations with NPCs
/// </summary>
public class ConversationContext
{
    public GameWorld GameWorld { get; set; }
    public Player Player { get; set; }
    
    // Location context
    public string LocationName { get; set; }
    public string LocationSpotName { get; set; }
    public List<string> LocationProperties { get; set; }
    
    // Conversation context
    public NPC TargetNPC { get; set; }
    public string ConversationTopic { get; set; }
    public int StartingFocusPoints { get; set; }
    
    // Relationship tracking
    public Dictionary<ConnectionType, int> CurrentTokens { get; set; }
    public int RelationshipLevel { get; set; }
    
    // Conversation flow
    public List<string> ConversationHistory { get; set; } = new List<string>();
    
}

/// <summary>
/// Extended context for queue management conversations
/// </summary>
public class QueueManagementContext : ConversationContext
{
    public Letter TargetLetter { get; set; }
    public string ManagementAction { get; set; } // "SkipDeliver", "Purge", etc.
    public int TokenCost { get; set; }
    public Dictionary<int, Letter> SkippedLetters { get; set; } // For skip action - letters that would be skipped
}

/// <summary>
/// Extended context for action-based conversations
/// </summary>
public class ActionConversationContext : ConversationContext
{
    public ActionOption SourceAction { get; set; }
    public string InitialNarrative { get; set; }
    public List<ChoiceTemplate> AvailableTemplates { get; set; }
}

/// <summary>
/// Extended context for travel conversations
/// </summary>
public class TravelConversationContext : ConversationContext
{
    public RouteOption Route { get; set; }
    public Location Origin { get; set; }
    public Location Destination { get; set; }
    public TravelEncounterType EncounterType { get; set; }
}