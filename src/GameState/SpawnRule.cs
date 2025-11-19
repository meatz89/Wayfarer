
/// <summary>
/// Spawn Rule - defines how a situation spawns child situations on completion
/// Creates cascading chains of situations for narrative progression
/// Spawned situation inherits placement from its SituationTemplate
/// </summary>
public class SpawnRule
{
    /// <summary>
    /// Template ID of the situation to spawn
    /// References a situation definition that will be instantiated
    /// Spawned situation uses SituationTemplate.PlacementFilter
    /// </summary>
    public string TemplateId { get; set; }

    /// <summary>
    /// Requirement offsets to apply to spawned situation
    /// Adjusts difficulty/requirements based on parent situation state
    /// </summary>
    public RequirementOffsets RequirementOffsets { get; set; }

    /// <summary>
    /// Additional conditions that must be met for spawn to occur
    /// Empty/null means always spawn on parent completion
    /// </summary>
    public SituationSpawnConditions Conditions { get; set; }
}

/// <summary>
/// Requirement offsets for spawned situations
/// Allows parent situation to make child easier/harder
/// </summary>
public class RequirementOffsets
{
    /// <summary>
    /// Bond strength offset to apply
    /// Example: -5 means spawned situation requires 5 less bond strength
    /// </summary>
    public int? BondStrengthOffset { get; set; }

    /// <summary>
    /// Scale value offset to apply
    /// </summary>
    public int? ScaleOffset { get; set; }

    /// <summary>
    /// Other numeric requirement offsets
    /// </summary>
    public int? NumericOffset { get; set; }
}

/// <summary>
/// Conditions for situation spawn rule execution (recursive situation spawning)
/// Additional checks beyond parent situation completion
/// Renamed from SpawnConditions to avoid collision with SceneTemplate.SpawnConditions
/// SCOPE: Situation-level spawning (child situations from parent situations)
/// </summary>
public class SituationSpawnConditions
{
    /// <summary>
    /// Minimum player Resolve required for spawn
    /// </summary>
    public int? MinResolve { get; set; }

    /// <summary>
    /// Required active state type
    /// </summary>
    public string RequiredState { get; set; }

    /// <summary>
    /// Required achievement
    /// HIGHLANDER: Object reference only, no string ID
    /// </summary>
    public Achievement RequiredAchievement { get; set; }
}
