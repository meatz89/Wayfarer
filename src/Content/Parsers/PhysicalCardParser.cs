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
        // Parse categorical properties from DTO
        Approach approach = ParseApproach(dto.Approach);
        PhysicalCategory category = ParseCategory(dto.Category);
        PhysicalDiscipline discipline = ParseDiscipline(dto.Discipline);
        RiskLevel riskLevel = ParseRiskLevel(dto.RiskLevel);
        ExertionLevel exertionLevel = ParseExertionLevel(dto.ExertionLevel);
        MethodType methodType = ParseMethodType(dto.MethodType);

        // PARSER CATALOG INTEGRATION: Auto-derive values from categorical properties if not specified in JSON
        int exertionCost = dto.ExertionCost > 0
            ? dto.ExertionCost
            : PhysicalCardEffectCatalog.GetExertionCostFromDepth(dto.Depth);

        // Parse simple requirement properties (NOT objects)
        Dictionary<PlayerStatType, int> statThresholds = new Dictionary<PlayerStatType, int>();
        if (dto.Requirements?.Stats != null)
        {
            foreach (KeyValuePair<string, int> kvp in dto.Requirements.Stats)
            {
                if (Enum.TryParse<PlayerStatType>(kvp.Key, out PlayerStatType statType))
                {
                    statThresholds[statType] = kvp.Value;
                }
            }
        }

        EquipmentCategory equipmentCategory = EquipmentCategory.None;
        if (dto.Requirements?.EquipmentCategory != null)
        {
            Enum.TryParse<EquipmentCategory>(dto.Requirements.EquipmentCategory, out equipmentCategory);
        }

        return new PhysicalCard
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            Depth = dto.Depth,
            BoundStat = ParseStat(dto.BoundStat),
            ExertionCost = exertionCost,
            Approach = approach,
            Category = category,
            Discipline = discipline,

            // Universal card properties
            RiskLevel = riskLevel,
            ExertionLevel = exertionLevel,
            MethodType = methodType,

            // COSTS DERIVED FROM CATEGORICAL PROPERTIES VIA CATALOG (calculated ONCE at parse time)
            StaminaCost = PhysicalCardEffectCatalog.GetStaminaCost(approach, dto.Depth, exertionLevel),
            DirectHealthCost = PhysicalCardEffectCatalog.GetHealthCost(approach, riskLevel, dto.Depth),
            CoinCost = PhysicalCardEffectCatalog.GetCoinCost(category, dto.Depth),
            XPReward = PhysicalCardEffectCatalog.GetXPReward(dto.Depth),

            // Simple requirement properties - parser calculates costs/effects from categorical properties via PhysicalCardEffectCatalog
            EquipmentCategory = equipmentCategory,
            StatThresholds = statThresholds,
            MinimumHealth = dto.Requirements?.MinHealth ?? 0,
            MinimumStamina = dto.Requirements?.MinStamina ?? 0
        };
    }

    // NOTE: Effects are calculated at card play time from categorical properties (Approach, TechniqueType, Depth, RiskLevel, etc.)
    // using PhysicalCardEffectCatalog formulas. This follows the documented parser-based architecture.

    private PlayerStatType ParseStat(string statString)
    {
        return Enum.TryParse<PlayerStatType>(statString, out PlayerStatType stat)
            ? stat
            : PlayerStatType.Authority;  // Authority = Forceful Approach (physical default per architecture)
    }

    private Approach ParseApproach(string approachString)
    {
        return Enum.TryParse<Approach>(approachString, out Approach approach)
            ? approach
            : Approach.Standard;
    }

    private PhysicalCategory ParseCategory(string categoryString)
    {
        return Enum.TryParse<PhysicalCategory>(categoryString, out PhysicalCategory category)
            ? category
            : PhysicalCategory.Aggressive;
    }

    private RiskLevel ParseRiskLevel(string riskString)
    {
        return Enum.TryParse<RiskLevel>(riskString, out RiskLevel risk)
            ? risk
            : RiskLevel.Cautious;
    }

    private ExertionLevel ParseExertionLevel(string exertionString)
    {
        return Enum.TryParse<ExertionLevel>(exertionString, out ExertionLevel exertion)
            ? exertion
            : ExertionLevel.Light;
    }

    private MethodType ParseMethodType(string methodTypeString)
    {
        return Enum.TryParse<MethodType>(methodTypeString, out MethodType methodType)
            ? methodType
            : MethodType.Direct;
    }

    private PhysicalDiscipline ParseDiscipline(string disciplineString)
    {
        return Enum.TryParse<PhysicalDiscipline>(disciplineString, out PhysicalDiscipline discipline)
            ? discipline
            : PhysicalDiscipline.Combat;
    }
}
