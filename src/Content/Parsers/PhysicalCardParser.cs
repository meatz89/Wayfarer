using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Parser for Physical Cards - converts DTOs to domain models
/// Parallel to MentalCardParser
/// </summary>
public class PhysicalCardParser
{
    public PhysicalCard ParseCard(PhysicalCardDTO dto)
    {
        // VALIDATION: Required fields crash if missing
        if (string.IsNullOrEmpty(dto.Id))
            throw new InvalidOperationException("PhysicalCard missing required field 'id'");
        if (string.IsNullOrEmpty(dto.Name))
            throw new InvalidOperationException($"PhysicalCard '{dto.Id}' missing required field 'name'");
        if (string.IsNullOrEmpty(dto.Approach))
            throw new InvalidOperationException($"PhysicalCard '{dto.Id}' missing required field 'approach'");
        if (string.IsNullOrEmpty(dto.TechniqueType))
            throw new InvalidOperationException($"PhysicalCard '{dto.Id}' missing required field 'techniqueType'");

        // Parse categorical properties from DTO (NO DEFAULTS - crash if invalid)
        Approach approach = ParseApproach(dto.Approach, dto.Id);
        PhysicalCategory category = ParseCategory(dto.TechniqueType, dto.Id);  // FIXED: Use TechniqueType from JSON
        PlayerStatType boundStat = ParseStat(dto.BoundStat, dto.Id);

        // ExertionCost ALWAYS calculated from catalog (no JSON override)
        int exertionCost = PhysicalCardEffectCatalog.GetExertionCostFromDepth(dto.Depth);

        // Fixed values for universal properties (catalog provides these, not JSON)
        RiskLevel riskLevel = RiskLevel.Cautious;  // Fixed until JSON needs variation
        ExertionLevel exertionLevel = ExertionLevel.Light;  // Fixed until JSON needs variation

        // Parse stat requirements
        Dictionary<PlayerStatType, int> statThresholds = new Dictionary<PlayerStatType, int>();
        if (dto.Requirements?.Stats != null)
        {
            foreach (KeyValuePair<string, int> kvp in dto.Requirements.Stats)
            {
                if (!Enum.TryParse<PlayerStatType>(kvp.Key, out PlayerStatType statType))
                    throw new InvalidOperationException($"PhysicalCard '{dto.Id}' has invalid stat requirement '{kvp.Key}'");
                statThresholds[statType] = kvp.Value;
            }
        }

        return new PhysicalCard
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            Depth = dto.Depth,
            BoundStat = boundStat,
            ExertionCost = exertionCost,
            Approach = approach,
            Category = category,

            // Universal properties - catalog provides defaults (not JSON)
            Discipline = PhysicalDiscipline.Combat,  // Fixed value until JSON needs variation
            RiskLevel = riskLevel,
            ExertionLevel = exertionLevel,
            MethodType = MethodType.Direct,  // Fixed value until JSON needs variation

            // COSTS DERIVED FROM CATEGORICAL PROPERTIES VIA CATALOG
            StaminaCost = PhysicalCardEffectCatalog.GetStaminaCost(approach, dto.Depth, exertionLevel),
            DirectHealthCost = PhysicalCardEffectCatalog.GetHealthCost(approach, riskLevel, dto.Depth),
            CoinCost = PhysicalCardEffectCatalog.GetCoinCost(category, dto.Depth),
            XPReward = PhysicalCardEffectCatalog.GetXPReward(dto.Depth),

            // BASE EFFECTS CALCULATED AT PARSE TIME (no runtime catalogue calls!)
            BaseBreakthrough = PhysicalCardEffectCatalog.GetProgressFromProperties(dto.Depth, category),
            BaseDanger = PhysicalCardEffectCatalog.GetDangerFromProperties(dto.Depth, approach),

            // Requirements
            EquipmentCategory = EquipmentCategory.None,  // Fixed value until JSON needs variation
            StatThresholds = statThresholds,
            MinimumHealth = 0,  // Physical cards never require minimum health (costs.health is what you PAY)
            MinimumStamina = 0  // Physical cards never require minimum stamina (costs.stamina is what you PAY)
        };
    }

    // NOTE: Effects are calculated at card play time from categorical properties (Approach, TechniqueType, Depth, RiskLevel, etc.)
    // using PhysicalCardEffectCatalog formulas. This follows the documented parser-based architecture.

    private PlayerStatType ParseStat(string statString, string cardId)
    {
        if (string.IsNullOrEmpty(statString))
            throw new InvalidOperationException($"PhysicalCard '{cardId}' missing required field 'boundStat'");

        if (!Enum.TryParse<PlayerStatType>(statString, out PlayerStatType stat))
            throw new InvalidOperationException(
                $"PhysicalCard '{cardId}' has invalid boundStat '{statString}'. " +
                $"Valid values: Insight, Rapport, Authority, Diplomacy, Cunning");

        return stat;
    }

    private Approach ParseApproach(string approachString, string cardId)
    {
        if (!Enum.TryParse<Approach>(approachString, out Approach approach))
            throw new InvalidOperationException(
                $"PhysicalCard '{cardId}' has invalid approach '{approachString}'. " +
                $"Valid values: Careful, Standard, Bold, Reckless");

        return approach;
    }

    private PhysicalCategory ParseCategory(string categoryString, string cardId)
    {
        if (!Enum.TryParse<PhysicalCategory>(categoryString, out PhysicalCategory category))
            throw new InvalidOperationException(
                $"PhysicalCard '{cardId}' has invalid techniqueType '{categoryString}'. " +
                $"Valid values: Aggressive, Defensive, Tactical, Evasive, Endurance");

        return category;
    }
}
