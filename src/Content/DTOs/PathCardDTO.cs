/// <summary>
/// DTO for path card data from JSON packages
/// Supports DUAL-PATTERN: atmospheric properties (direct costs/rewards) + scene-based (ChoiceTemplateId)
/// </summary>
public class PathCardDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public bool StartsRevealed { get; set; } = false;
    public bool IsHidden { get; set; } = false;
    public int ExplorationThreshold { get; set; } = 0;
    public bool IsOneTime { get; set; } = false;
    public string NarrativeText { get; set; }

    // ==================== ATMOSPHERIC PATTERN (Direct Properties) ====================

    /// <summary>
    /// Coin cost to use this path (ATMOSPHERIC PATTERN)
    /// </summary>
    public int CoinRequirement { get; set; } = 0;

    /// <summary>
    /// Permit ID required to use this path (ATMOSPHERIC PATTERN)
    /// </summary>
    public string PermitRequirement { get; set; }

    /// <summary>
    /// Stat requirements - minimum stat levels required (ATMOSPHERIC PATTERN)
    /// Dictionary key = stat name, value = minimum level
    /// </summary>
    public Dictionary<string, int> StatRequirements { get; set; } = new Dictionary<string, int>();

    /// <summary>
    /// Stamina cost to use this path (ATMOSPHERIC PATTERN)
    /// </summary>
    public int StaminaCost { get; set; }

    /// <summary>
    /// Time cost in segments (ATMOSPHERIC PATTERN)
    /// </summary>
    public int TravelTimeSegments { get; set; }

    /// <summary>
    /// Hunger increase from using this path (ATMOSPHERIC PATTERN)
    /// </summary>
    public int HungerEffect { get; set; } = 0;

    /// <summary>
    /// Stamina restored when using this path (ATMOSPHERIC PATTERN)
    /// </summary>
    public int StaminaRestore { get; set; } = 0;

    /// <summary>
    /// Health change from this path (ATMOSPHERIC PATTERN)
    /// </summary>
    public int HealthEffect { get; set; } = 0;

    /// <summary>
    /// Coins gained from this path (ATMOSPHERIC PATTERN)
    /// </summary>
    public int CoinReward { get; set; } = 0;

    /// <summary>
    /// One-time reward string for discovering/using this path (ATMOSPHERIC PATTERN)
    /// Parsed into strongly-typed PathReward at parse-time
    /// </summary>
    public string OneTimeReward { get; set; }

    /// <summary>
    /// Token gains from using this path (ATMOSPHERIC PATTERN)
    /// </summary>
    public Dictionary<string, int> TokenGains { get; set; } = new Dictionary<string, int>();

    /// <summary>
    /// List of path card IDs revealed when this path is used (ATMOSPHERIC PATTERN)
    /// </summary>
    public List<string> RevealsPaths { get; set; } = new List<string>();

    /// <summary>
    /// Dead-end path that forces player to return (ATMOSPHERIC PATTERN)
    /// </summary>
    public bool ForceReturn { get; set; } = false;

    // ==================== SCENE INTEGRATION ====================

    /// <summary>
    /// Optional scene ID on this path (references GameWorld.Scenes)
    /// Player can preview scene and see equipment applicability before committing
    /// </summary>
    public string SceneId { get; set; }

    // ==================== SCENE-BASED PATTERN (ChoiceTemplate) ====================

    /// <summary>
    /// Optional ChoiceTemplate ID for scene-based path cards (SCENE-BASED PATTERN)
    /// Parser resolves to ChoiceTemplate object reference
    /// </summary>
    public string ChoiceTemplateId { get; set; }
}