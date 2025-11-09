
/// <summary>
/// Perfect information preview of scene that will spawn when action is selected
/// Enables strategic decision-making BEFORE committing to action
///
/// HIGHLANDER COMPLIANCE: Replaces provisional scene pattern
/// - OLD: Create Scene entity with State=Provisional, show placement, delete if not selected
/// - NEW: Generate ScenePreview DTO from SceneTemplate metadata, no entity creation
///
/// POPULATED: During action generation (SceneFacade.ActivateSituation*)
/// CONSUMED: By UI to display "This action spawns scene X at location Y"
/// </summary>
public class ScenePreview
{
    /// <summary>
    /// Scene template ID this preview represents
    /// </summary>
    public string SceneTemplateId { get; set; }

    /// <summary>
    /// Display name of scene that will spawn
    /// Example: "A Desperate Plea", "The Final Confrontation"
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    /// Scene complexity tier (0-4)
    /// Tier 0: Safety net, Tier 1: Low, Tier 2: Standard, Tier 3: High, Tier 4: Climactic
    /// </summary>
    public int Tier { get; set; }

    /// <summary>
    /// Where scene will be placed
    /// SameLocation, SameNPC, SpecificLocation, SpecificNPC, etc.
    /// </summary>
    public PlacementRelation PlacementRelation { get; set; }

    /// <summary>
    /// Resolved placement ID if determinable at action generation time
    /// - SameLocation/SameNPC: Resolved immediately
    /// - SpecificLocation/SpecificNPC: Resolved from SpawnReward
    /// - Player/CurrentRoute: Resolved at generation time
    /// null if placement depends on runtime state at execution time
    /// </summary>
    public string ResolvedPlacementId { get; set; }

    /// <summary>
    /// Placement type (Location, NPC, Route)
    /// </summary>
    public PlacementType? PlacementType { get; set; }

    /// <summary>
    /// Number of situations in scene
    /// Indicates scene complexity/length
    /// </summary>
    public int SituationCount { get; set; }

    /// <summary>
    /// Challenge types present in scene (Social, Mental, Physical)
    /// Enables player to prepare appropriate stats/resources
    /// </summary>
    public List<TacticalSystemType> ChallengeTypes { get; set; } = new List<TacticalSystemType>();

    /// <summary>
    /// Expiration window (days from spawn)
    /// null = no expiration (scene persists indefinitely)
    /// </summary>
    public int? ExpiresInDays { get; set; }

    /// <summary>
    /// Scene archetype pattern
    /// Standalone, ChainInitiator, ChainContinuation, ChainConclusion
    /// </summary>
    public SpawnPattern Archetype { get; set; }
}
