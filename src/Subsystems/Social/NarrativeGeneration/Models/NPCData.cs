/// <summary>
/// Simplified NPC information for narrative generation.
/// Contains only the essential data needed to generate contextually appropriate dialogue.
/// HIGHLANDER: No ID property - Name is sufficient for AI narrative generation
/// </summary>
public class NPCData
{
    /// <summary>
    /// Display name of the NPC.
    /// Used in narrative generation for natural dialogue references and identification.
    /// HIGHLANDER: Name serves both display and identification purposes
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