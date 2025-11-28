/// <summary>
/// DTO for Spawn Rules - defines how situations spawn child situations
/// Used to create cascading chains of situations
/// </summary>
public class SpawnRuleDTO
{
    /// <summary>
    /// Template ID of the situation to spawn
    /// References a situation template in situations.json
    /// Spawned situation inherits placement from its SituationTemplate (HIGHLANDER)
    /// </summary>
    public string TemplateId { get; set; }

    /// <summary>
    /// Requirement offsets to apply to the spawned situation
    /// Adjusts difficulty/requirements based on parent situation state
    /// Will be parsed into explicit OrPath property adjustments
    /// </summary>
    public RequirementOffsetsDTO RequirementOffsets { get; set; }

    /// <summary>
    /// Additional conditions that must be met for spawn to occur
    /// Empty list means always spawn on parent completion
    /// </summary>
    public ConditionsDTO Conditions { get; set; }
}

/// <summary>
/// Requirement offsets for spawned situations
/// </summary>
public class RequirementOffsetsDTO
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
/// Conditions for spawn rule execution
/// </summary>
public class ConditionsDTO
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
    /// Required achievement ID
    /// </summary>
    public string RequiredAchievement { get; set; }
}
