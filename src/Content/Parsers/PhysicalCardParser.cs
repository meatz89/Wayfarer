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
        Approach approach = ParseApproach(dto.Approach);
        PhysicalCategory category = ParseCategory(dto.Category);

        // PARSER CATALOG INTEGRATION: Auto-derive values from categorical properties if not specified in JSON
        int positionCost = dto.PositionCost > 0
            ? dto.PositionCost
            : PhysicalCardEffectCatalog.GetPositionCostFromDepth(dto.Depth);

        // Parse simple requirement properties (NOT objects)
        Dictionary<PlayerStatType, int> statThresholds = new Dictionary<PlayerStatType, int>();
        if (dto.Requirements?.Stats != null)
        {
            foreach (var kvp in dto.Requirements.Stats)
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
            CardType = CardType.Physical,
            Depth = dto.Depth,
            BoundStat = ParseStat(dto.BoundStat),
            Tags = dto.Tags ?? new List<string>(),
            PositionCost = positionCost,
            Approach = approach,
            Category = category,

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
}
