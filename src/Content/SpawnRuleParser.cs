/// <summary>
/// Parser for converting SpawnRuleDTO to SpawnRule domain model
/// Handles situation spawn cascades and requirement offsets
/// </summary>
public static class SpawnRuleParser
{
    /// <summary>
    /// Convert a SpawnRuleDTO to a SpawnRule domain model
    /// </summary>
    public static SpawnRule ConvertDTOToSpawnRule(SpawnRuleDTO dto, string parentSituationId)
    {
        if (dto == null)
            return null;

        if (string.IsNullOrEmpty(dto.TemplateId))
            throw new InvalidOperationException($"SpawnRule in situation {parentSituationId} missing required 'TemplateId' field");

        // NOTE: TargetPlacement (legacy string field) deleted
        // Spawned situations inherit placement from their SituationTemplate (HIGHLANDER - one mechanism only)
        SpawnRule spawnRule = new SpawnRule
        {
            TemplateId = dto.TemplateId,
            RequirementOffsets = ParseRequirementOffsets(dto.RequirementOffsets),
            Conditions = ParseSituationSpawnConditions(dto.Conditions)
        };

        return spawnRule;
    }

    /// <summary>
    /// Parse requirement offsets from DTO
    /// All offsets are optional (null means no adjustment)
    /// </summary>
    private static RequirementOffsets ParseRequirementOffsets(RequirementOffsetsDTO dto)
    {
        if (dto == null)
            return new RequirementOffsets();

        return new RequirementOffsets
        {
            BondStrengthOffset = dto.BondStrengthOffset,
            ScaleOffset = dto.ScaleOffset,
            NumericOffset = dto.NumericOffset
        };
    }

    /// <summary>
    /// Parse spawn conditions from DTO
    /// All conditions are optional (null/empty means always spawn)
    /// </summary>
    private static SituationSpawnConditions ParseSituationSpawnConditions(ConditionsDTO dto)
    {
        if (dto == null)
            return new SituationSpawnConditions();

        return new SituationSpawnConditions
        {
            MinResolve = dto.MinResolve,
            RequiredState = dto.RequiredState,
            RequiredAchievement = dto.RequiredAchievement
        };
    }

    /// <summary>
    /// Parse a list of spawn rules from DTOs
    /// </summary>
    public static List<SpawnRule> ParseSpawnRules(List<SpawnRuleDTO> dtos, string parentSituationId)
    {
        if (dtos == null || !dtos.Any())
            return new List<SpawnRule>();

        List<SpawnRule> spawnRules = new List<SpawnRule>();
        foreach (SpawnRuleDTO dto in dtos)
        {
            try
            {
                SpawnRule spawnRule = ConvertDTOToSpawnRule(dto, parentSituationId);
                if (spawnRule != null)
                {
                    spawnRules.Add(spawnRule);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to parse spawn rule '{dto?.TemplateId}' in situation '{parentSituationId}': {ex.Message}", ex);
            }
        }

        return spawnRules;
    }
}
