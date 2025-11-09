/// <summary>
/// Parser for Mental Cards - converts DTOs to domain models
/// Parallel to ConversationCardParser
/// </summary>
public class MentalCardParser
{
public MentalCard ParseCard(MentalCardDTO dto)
{
    // VALIDATION: Required fields crash if missing
    if (string.IsNullOrEmpty(dto.Id))
        throw new InvalidOperationException("MentalCard missing required field 'id'");
    if (string.IsNullOrEmpty(dto.Name))
        throw new InvalidOperationException($"MentalCard '{dto.Id}' missing required field 'name'");
    if (string.IsNullOrEmpty(dto.Method))
        throw new InvalidOperationException($"MentalCard '{dto.Id}' missing required field 'method'");
    if (string.IsNullOrEmpty(dto.ClueType))
        throw new InvalidOperationException($"MentalCard '{dto.Id}' missing required field 'clueType'");

    // Parse categorical properties from DTO (NO DEFAULTS - crash if invalid)
    Method method = ParseMethod(dto.Method, dto.Id);
    MentalCategory category = ParseCategory(dto.ClueType, dto.Id);  // FIXED: Use ClueType from JSON
    PlayerStatType boundStat = ParseStat(dto.BoundStat, dto.Id);

    // AttentionCost ALWAYS calculated from catalog (no JSON override)
    int attentionCost = MentalCardEffectCatalog.GetAttentionCostFromDepth(dto.Depth);

    // Parse stat requirements
    Dictionary<PlayerStatType, int> statThresholds = new Dictionary<PlayerStatType, int>();
    if (dto.Requirements?.Stats != null)
    {
        foreach (KeyValuePair<string, int> kvp in dto.Requirements.Stats)
        {
            if (!Enum.TryParse<PlayerStatType>(kvp.Key, out PlayerStatType statType))
                throw new InvalidOperationException($"MentalCard '{dto.Id}' has invalid stat requirement '{kvp.Key}'");
            statThresholds[statType] = kvp.Value;
        }
    }

    return new MentalCard
    {
        Id = dto.Id,
        Name = dto.Name,
        Description = dto.Description,
        Depth = dto.Depth,
        BoundStat = boundStat,
        AttentionCost = attentionCost,
        Method = method,
        Category = category,

        // Universal properties - catalog provides defaults (not JSON)
        Discipline = ObligationDiscipline.Research,  // Fixed value until JSON needs variation
        ExertionLevel = ExertionLevel.Light,  // Fixed value until JSON needs variation
        MethodType = MethodType.Direct,  // Fixed value until JSON needs variation

        // COSTS DERIVED FROM CATEGORICAL PROPERTIES VIA CATALOG
        CoinCost = MentalCardEffectCatalog.GetCoinCost(category, dto.Depth),
        XPReward = MentalCardEffectCatalog.GetXPReward(dto.Depth),

        // BASE EFFECTS CALCULATED AT PARSE TIME (no runtime catalogue calls!)
        BaseProgress = MentalCardEffectCatalog.GetProgressFromProperties(dto.Depth, category),
        BaseExposure = MentalCardEffectCatalog.GetExposureFromProperties(dto.Depth, method),

        // Requirements
        EquipmentCategory = EquipmentCategory.None,  // Fixed value until JSON needs variation
        StatThresholds = statThresholds,
        MinimumHealth = 0,  // Mental cards never require minimum health (see entity comment line 32)
        MinimumStamina = 0  // Mental cards never require minimum stamina (see entity comment line 32)
    };
}

// NOTE: Effects are calculated at card play time from categorical properties (Method, ClueType, Depth, RiskLevel, etc.)
// using MentalCardEffectCatalog formulas. This follows the documented parser-based architecture.

private PlayerStatType ParseStat(string statString, string cardId)
{
    if (string.IsNullOrEmpty(statString))
        throw new InvalidOperationException($"MentalCard '{cardId}' missing required field 'boundStat'");

    if (!Enum.TryParse<PlayerStatType>(statString, out PlayerStatType stat))
        throw new InvalidOperationException(
            $"MentalCard '{cardId}' has invalid boundStat '{statString}'. " +
            $"Valid values: Insight, Rapport, Authority, Diplomacy, Cunning");

    return stat;
}

private Method ParseMethod(string methodString, string cardId)
{
    if (!Enum.TryParse<Method>(methodString, out Method method))
        throw new InvalidOperationException(
            $"MentalCard '{cardId}' has invalid method '{methodString}'. " +
            $"Valid values: Careful, Standard, Bold, Reckless");

    return method;
}

private MentalCategory ParseCategory(string categoryString, string cardId)
{
    if (!Enum.TryParse<MentalCategory>(categoryString, out MentalCategory category))
        throw new InvalidOperationException(
            $"MentalCard '{cardId}' has invalid clueType '{categoryString}'. " +
            $"Valid values: Analytical, Physical, Observational, Social, Synthesis");

    return category;
}
}
