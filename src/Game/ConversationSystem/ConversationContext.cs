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