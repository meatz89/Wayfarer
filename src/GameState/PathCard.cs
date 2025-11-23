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

    /// <summary>
    /// Optional scene on this path
    /// HIGHLANDER: Object reference ONLY, no SceneId
    /// Player can preview scene and see equipment applicability before committing
    /// Scene engagement may be required or optional depending on scene type
    /// </summary>
    public Scene Scene { get; set; }

    // ==================== SIR BRANTE LAYER (UNIFIED ACTION ARCHITECTURE) ====================

    /// <summary>
    /// ChoiceTemplate source (Sir Brante layer - Scene-Situation architecture)
    /// COMPOSITION not copy - access CompoundRequirement, ChoiceCost, ChoiceReward through this reference
    ///
    /// Scene-spawned path cards generated from ChoiceTemplate at spawn time
    /// ChoiceTemplate provides:
    /// - RequirementFormula (CompoundRequirement with OR paths)
    /// - CostTemplate (ChoiceCost with Coins/Resolve/TimeSegments)
    /// - RewardTemplate (ChoiceReward with bonds/scales/states/scene spawns)
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
