/// <summary>
/// Simplified NPC information for narrative generation.
/// Contains only the essential data needed to generate contextually appropriate dialogue.
/// </summary>
public class NPCData
{
    /// <summary>
    /// Unique identifier for the NPC.
    /// Used for logging and debugging, not for hardcoded behavior checks.
    /// </summary>
    public string NpcId { get; set; }
    
    /// <summary>
    /// Display name of the NPC.
    /// Used in narrative generation for natural dialogue references.
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Categorical personality type determining dialogue style and approach.
    /// Examples: DEVOTED, MERCANTILE, PROUD, CUNNING, STEADFAST
    /// </summary>
    public PersonalityType Personality { get; set; }
    
    /// <summary>
    /// Optional description of the NPC's current crisis or problem.
    /// Used to generate crisis-related dialogue when rapport is sufficient.
    /// Null if no active crisis.
    /// </summary>
    public string CurrentCrisis { get; set; }
    
    /// <summary>
    /// Current topic being discussed in the conversation.
    /// Changes based on rapport level and conversation progression.
    /// Examples: "weather", "family_troubles", "forced_marriage"
    /// </summary>
    public string CurrentTopic { get; set; }
}