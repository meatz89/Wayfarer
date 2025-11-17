/// <summary>
/// Data Transfer Object for deserializing NPC data from JSON.
/// Uses categorical properties via PlacementFilterDTO for entity resolution (DDR-006).
/// EntityResolver.FindOrCreate pattern: queries existing entities first, generates if no match.
/// </summary>
public class NPCDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Profession { get; set; }

    /// <summary>
    /// Categorical spawn location filter - EntityResolver finds matching location OR creates new
    /// CORRECT PATTERN: Categorical properties (LocationTypes, Purposes, etc.)
    /// WRONG PATTERN: Entity instance ID (LocationId string)
    /// </summary>
    public PlacementFilterDTO SpawnLocation { get; set; }

    public string Description { get; set; }
    public string Personality { get; set; }
    public string PersonalityType { get; set; }
    public List<string> Services { get; set; } = new List<string>();
    public string Role { get; set; }
    public string AvailabilitySchedule { get; set; }
    public int Tier { get; set; }
    public int Level { get; set; } = 1; // Level 1-5 for difficulty/content progression and XP scaling
    public int ConversationDifficulty { get; set; } = 1; // Level 1-3 for XP multipliers

    public string CurrentState { get; set; }
    public Dictionary<string, int> InitialTokens { get; set; } = new Dictionary<string, int>();

    // Orthogonal Categorical Dimensions (Entity Resolution)
    // String values from JSON parsed to enums by NPCParser
    public string SocialStanding { get; set; }
    public string StoryRole { get; set; }
    public string KnowledgeLevel { get; set; }
}
