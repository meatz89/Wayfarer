/// <summary>
/// PathCard - Runtime entity for route travel choices (action cards on routes)
/// Generated from PathCardDTO by parser OR from ChoiceTemplate when Scene-spawned
/// Represents a single path option available during route segment navigation
/// </summary>
public class PathCard
{
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

    // ==================== ATMOSPHERIC PATTERN (Direct Properties) ====================

    /// <summary>
    /// Coin cost to use this path (ATMOSPHERIC PATTERN - must have coins to select)
    /// Used when ChoiceTemplate is null (static route path cards)
    /// </summary>
    public int CoinRequirement { get; set; } = 0;

    /// <summary>
    /// Permit/license required to use this path (ATMOSPHERIC PATTERN)
    /// HIGHLANDER: Object reference ONLY, no PermitRequirement ID
    /// null = no permit required
    /// References permit in player inventory
    /// </summary>
    public Item PermitRequirement { get; set; }

    /// <summary>
    /// Stat requirements - minimum stat levels required to use this path (ATMOSPHERIC PATTERN)
    /// Dictionary key = stat name (e.g., "insight", "cunning")
    /// Dictionary value = minimum level required
    /// Player must meet ALL stat requirements
    /// </summary>
    public Dictionary<string, int> StatRequirements { get; set; } = new Dictionary<string, int>();

    /// <summary>
    /// Stamina cost to use this path (ATMOSPHERIC PATTERN)
    /// Deducted from player's current stamina when path selected
    /// </summary>
    public int StaminaCost { get; set; }

    /// <summary>
    /// Time cost in segments (ATMOSPHERIC PATTERN - 4 segments per time block)
    /// Advances game time when path selected
    /// </summary>
    public int TravelTimeSegments { get; set; }

    /// <summary>
    /// Hunger increase from using this path (ATMOSPHERIC PATTERN)
    /// Positive value increases hunger (makes player hungrier)
    /// Negative value decreases hunger (provides food)
    /// </summary>
    public int HungerEffect { get; set; } = 0;

    /// <summary>
    /// Stamina restored when using this path (ATMOSPHERIC PATTERN)
    /// Positive value for REST paths that restore stamina
    /// Replaces StaminaCost for recovery actions
    /// </summary>
    public int StaminaRestore { get; set; } = 0;

    /// <summary>
    /// Health change from this path (ATMOSPHERIC PATTERN)
    /// Positive = healing, negative = damage
    /// Example: Dangerous path might cost health, safe path might restore
    /// </summary>
    public int HealthEffect { get; set; } = 0;

    /// <summary>
    /// Coins gained from this path (ATMOSPHERIC PATTERN)
    /// Positive value = earn coins (found treasure, traded with travelers)
    /// </summary>
    public int CoinReward { get; set; } = 0;

    /// <summary>
    /// One-time reward for discovering/using this path (ATMOSPHERIC PATTERN)
    /// Strongly-typed reward object (coins, observation, etc.)
    /// Set at parse-time from PathCardDTO.OneTimeReward string
    /// </summary>
    public PathReward Reward { get; set; } = PathReward.None;

    /// <summary>
    /// Token gains from using this path (ATMOSPHERIC PATTERN)
    /// Dictionary key = token type (e.g., "Diplomacy", "Status")
    /// Dictionary value = amount gained
    /// Progression system integration
    /// </summary>
    public Dictionary<string, int> TokenGains { get; set; } = new Dictionary<string, int>();

    /// <summary>
    /// List of path card IDs revealed when this path is used (ATMOSPHERIC PATTERN)
    /// Discovery mechanic: Using one path unlocks knowledge of other paths
    /// </summary>
    public List<string> RevealsPaths { get; set; } = new List<string>();

    /// <summary>
    /// Dead-end path that forces player to return (ATMOSPHERIC PATTERN)
    /// After selecting this path, must return to previous segment
    /// Cannot continue forward on route
    /// </summary>
    public bool ForceReturn { get; set; } = false;

    /// <summary>
    /// Optional scene on this path
    /// HIGHLANDER: Object reference ONLY, no SceneId
    /// Player can preview scene and see equipment applicability before committing
    /// Scene engagement may be required or optional depending on scene type
    /// </summary>
    public Scene Scene { get; set; }

    // ==================== SIR BRANTE LAYER (UNIFIED ACTION ARCHITECTURE) ====================

    /// <summary>
    /// ChoiceTemplate source (SCENE-BASED PATTERN - Scene-Situation architecture)
    /// COMPOSITION not copy - access CompoundRequirement, OnSuccessConsequence, OnFailureConsequence through this reference
    ///
    /// PATTERN DISCRIMINATION:
    /// - IF ChoiceTemplate != null → SCENE-BASED path card (use template consequences)
    /// - IF ChoiceTemplate == null → ATMOSPHERIC path card (use direct properties above)
    ///
    /// Scene-spawned path cards generated from ChoiceTemplate at spawn time.
    /// ChoiceTemplate provides:
    /// - RequirementFormula (CompoundRequirement with OR paths)
    /// - OnSuccessConsequence (Consequence with resource changes, bonds, scales, states, scene spawns)
    /// - OnFailureConsequence (Consequence with failure outcomes)
    ///
    /// See DUAL_TIER_ACTION_ARCHITECTURE.md for complete explanation.
    /// </summary>
    public ChoiceTemplate ChoiceTemplate { get; set; }

    /// <summary>
    /// THREE-TIER TIMING MODEL: Source Situation
    /// HIGHLANDER: Object reference ONLY, no SituationId
    /// Links ephemeral path card to source Situation for cleanup after execution
    /// Path cards are QUERY-TIME instances (Tier 3), created when Situation activates
    /// After card executes, GameFacade deletes ALL path cards for this Situation
    /// Next time player enters route context, cards recreated fresh from ChoiceTemplates
    /// NOTE: Scene property above is for EXISTING scenes on route, NOT for scene spawns
    /// </summary>
    public Situation Situation { get; set; }

    /// <summary>
    /// PERFECT INFORMATION: Scene spawn previews
    /// If this path card spawns scenes (ChoiceTemplate.OnSuccessConsequence.ScenesToSpawn),
    /// SceneFacade generates ScenePreview from SceneTemplate metadata
    /// Player sees WHERE scene will spawn, WHAT it contains, BEFORE selecting path card
    /// Enables strategic decision-making with full knowledge of consequences
    ///
    /// HIGHLANDER COMPLIANCE: Replaces provisional scene pattern
    /// - OLD: Create Scene entity with State=Provisional, delete if not selected
    /// - NEW: Generate ScenePreview DTO from template, no entity until card executes
    ///
    /// DISTINCTION: Scene (line 141) = EXISTING scene on route (already spawned)
    ///              ScenePreviews = Scenes that WILL spawn if this card selected
    /// </summary>
    public List<ScenePreview> ScenePreviews { get; set; } = new List<ScenePreview>();

    /// <summary>
    /// Entity-derived scaled requirement for display (query-time scaling).
    /// TWO-PHASE SCALING MODEL (arc42 §8.26):
    /// - Parse-time: Catalogue generates rhythm structure + tier-based values
    /// - Query-time: Entity-derived adjustments from RuntimeScalingContext
    ///
    /// Created by SceneFacade when building PathCard from ChoiceTemplate.
    /// null = no scaling applied (use ChoiceTemplate.RequirementFormula directly)
    /// non-null = scaled version reflecting current entity context
    /// </summary>
    public CompoundRequirement ScaledRequirement { get; set; }

    /// <summary>
    /// Entity-derived scaled consequence for display (query-time scaling).
    /// TWO-PHASE SCALING MODEL (arc42 §8.26):
    /// Costs adjusted based on route danger and location quality.
    /// </summary>
    public Consequence ScaledConsequence { get; set; }
}
