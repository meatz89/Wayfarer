/// <summary>
/// PathCard - Runtime entity for route travel choices (action cards on routes)
/// Generated from PathCardDTO by parser OR from ChoiceTemplate when Scene-spawned
/// Represents a single path option available during route segment navigation
/// </summary>
public class PathCard
{
    /// <summary>
    /// Unique identifier for this path card
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Display name for this path card
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Narrative description of this path
    /// </summary>
    public string NarrativeText { get; set; }

    // ==================== DISCOVERY & VISIBILITY ====================

    /// <summary>
    /// Whether card is face-up at game start (immediately visible)
    /// </summary>
    public bool StartsRevealed { get; set; } = false;

    /// <summary>
    /// Whether card is hidden entirely (not displayed even as face-down)
    /// </summary>
    public bool IsHidden { get; set; } = false;

    /// <summary>
    /// Route exploration cubes required to reveal this card
    /// 0 = always visible
    /// 10 = extremely hidden, requires full exploration
    /// Discovery mechanic for path unlocking
    /// </summary>
    public int ExplorationThreshold { get; set; } = 0;

    /// <summary>
    /// Whether this path can only be used once
    /// After first use, path becomes unavailable
    /// </summary>
    public bool IsOneTime { get; set; } = false;

    // ==================== REQUIREMENTS (LEGACY PATTERN) ====================

    /// <summary>
    /// Coin cost to use this path (must have coins to select)
    /// </summary>
    public int CoinRequirement { get; set; } = 0;

    /// <summary>
    /// Permit/license ID required to use this path
    /// null = no permit required
    /// References permit in player inventory
    /// </summary>
    public string PermitRequirement { get; set; }

    /// <summary>
    /// Stat requirements - minimum stat levels required to use this path
    /// Dictionary key = stat name (e.g., "insight", "cunning")
    /// Dictionary value = minimum level required
    /// Player must meet ALL stat requirements
    /// </summary>
    public Dictionary<string, int> StatRequirements { get; set; } = new Dictionary<string, int>();

    // ==================== COSTS (LEGACY PATTERN) ====================

    /// <summary>
    /// Stamina cost to use this path
    /// Deducted from player's current stamina when path selected
    /// </summary>
    public int StaminaCost { get; set; }

    /// <summary>
    /// Time cost in segments (4 segments per time block)
    /// Advances game time when path selected
    /// </summary>
    public int TravelTimeSegments { get; set; }

    /// <summary>
    /// Hunger increase from using this path
    /// Positive value increases hunger (makes player hungrier)
    /// Negative value decreases hunger (provides food)
    /// </summary>
    public int HungerEffect { get; set; } = 0;

    // ==================== REWARDS (LEGACY PATTERN) ====================

    /// <summary>
    /// Stamina restored when using this path
    /// Positive value for REST paths that restore stamina
    /// Replaces StaminaCost for recovery actions
    /// </summary>
    public int StaminaRestore { get; set; } = 0;

    /// <summary>
    /// Health change from this path
    /// Positive = healing, negative = damage
    /// Example: Dangerous path might cost health, safe path might restore
    /// </summary>
    public int HealthEffect { get; set; } = 0;

    /// <summary>
    /// Coins gained from this path
    /// Positive value = earn coins (found treasure, traded with travelers)
    /// </summary>
    public int CoinReward { get; set; } = 0;

    /// <summary>
    /// One-time reward for discovering/using this path
    /// Strongly-typed reward object (coins, observation, etc.)
    /// Set at parse-time from PathCardDTO.OneTimeReward string
    /// </summary>
    public PathReward Reward { get; set; } = PathReward.None;

    /// <summary>
    /// Token gains from using this path
    /// Dictionary key = token type (e.g., "Diplomacy", "Status")
    /// Dictionary value = amount gained
    /// Progression system integration
    /// </summary>
    public Dictionary<string, int> TokenGains { get; set; } = new Dictionary<string, int>();

    // ==================== PATH REVELATIONS ====================

    /// <summary>
    /// List of path card IDs revealed when this path is used
    /// Discovery mechanic: Using one path unlocks knowledge of other paths
    /// </summary>
    public List<string> RevealsPaths { get; set; } = new List<string>();

    // ==================== BEHAVIORAL FLAGS ====================

    /// <summary>
    /// Dead-end path that forces player to return
    /// After selecting this path, must return to previous segment
    /// Cannot continue forward on route
    /// </summary>
    public bool ForceReturn { get; set; } = false;

    /// <summary>
    /// Optional scene ID on this path
    /// References Scene in GameWorld.Scenes
    /// Player can preview scene and see equipment applicability before committing
    /// Scene engagement may be required or optional depending on scene type
    /// </summary>
    public string SceneId { get; set; }

    // ==================== SIR BRANTE LAYER (UNIFIED ACTION ARCHITECTURE) ====================

    /// <summary>
    /// ChoiceTemplate source (Sir Brante layer - Scene-Situation architecture)
    /// COMPOSITION not copy - access CompoundRequirement, ChoiceCost, ChoiceReward through this reference
    ///
    /// null = Static path card parsed directly from route JSON (legacy pattern)
    ///        Uses direct requirement/cost/reward properties above
    ///
    /// non-null = Scene-spawned path card generated from ChoiceTemplate at spawn time
    ///            ChoiceTemplate provides:
    ///            - RequirementFormula (CompoundRequirement with OR paths)
    ///            - CostTemplate (ChoiceCost with Coins/Resolve/TimeSegments)
    ///            - RewardTemplate (ChoiceReward with bonds/scales/states/scene spawns)
    ///
    /// Enables unified action execution: All path cards check ChoiceTemplate if present,
    /// fall back to direct properties if null (legacy coexistence pattern)
    /// </summary>
    public ChoiceTemplate ChoiceTemplate { get; set; }

    /// <summary>
    /// THREE-TIER TIMING MODEL: Source Situation ID
    /// Links ephemeral path card to source Situation for cleanup after execution
    /// Path cards are QUERY-TIME instances (Tier 3), created when Situation activates
    /// After card executes, GameFacade deletes ALL path cards for this Situation
    /// Next time player enters route context, cards recreated fresh from ChoiceTemplates
    /// NOTE: SceneId property (line 151) is for EXISTING scenes on route, NOT for scene spawns
    /// </summary>
    public string SituationId { get; set; }

    /// <summary>
    /// PERFECT INFORMATION: Scene spawn previews
    /// If this path card spawns scenes (ChoiceTemplate.RewardTemplate.ScenesToSpawn),
    /// SceneFacade generates ScenePreview from SceneTemplate metadata
    /// Player sees WHERE scene will spawn, WHAT it contains, BEFORE selecting path card
    /// Enables strategic decision-making with full knowledge of consequences
    ///
    /// HIGHLANDER COMPLIANCE: Replaces provisional scene pattern
    /// - OLD: Create Scene entity with State=Provisional, delete if not selected
    /// - NEW: Generate ScenePreview DTO from template, no entity until card executes
    ///
    /// DISTINCTION: SceneId (line 151) = EXISTING scene on route (already spawned)
    ///              ScenePreviews = Scenes that WILL spawn if this card selected
    /// </summary>
    public List<ScenePreview> ScenePreviews { get; set; } = new List<ScenePreview>();
}
