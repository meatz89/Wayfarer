using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Parser for Mental Cards - converts DTOs to domain models
/// Parallel to ConversationCardParser
/// </summary>
public class MentalCardParser
{
    public MentalCard ParseCard(MentalCardDTO dto)
    {
        Method method = ParseMethod(dto.Method);
        MentalCategory category = ParseCategory(dto.Category);

        // PARSER CATALOG INTEGRATION: Auto-derive values from categorical properties if not specified in JSON
        int attentionCost = dto.AttentionCost > 0
            ? dto.AttentionCost
            : MentalCardEffectCatalog.GetAttentionCostFromDepth(dto.Depth);

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

        return new MentalCard
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            CardType = CardType.Mental,
            Depth = dto.Depth,
            BoundStat = ParseStat(dto.BoundStat),
            Tags = dto.Tags ?? new List<string>(),
            AttentionCost = attentionCost,
            Method = method,
            Category = category,

            // Simple requirement properties - parser calculates costs/effects from categorical properties via MentalCardEffectCatalog
            EquipmentCategory = equipmentCategory,
            StatThresholds = statThresholds,
            MinimumHealth = dto.Requirements?.MinHealth ?? 0,
            MinimumStamina = dto.Requirements?.MinStamina ?? 0
        };
    }

    // NOTE: Effects are calculated at card play time from categorical properties (Method, ClueType, Depth, RiskLevel, etc.)
    // using MentalCardEffectCatalog formulas. This follows the documented parser-based architecture.

    private PlayerStatType ParseStat(string statString)
    {
        return Enum.TryParse<PlayerStatType>(statString, out PlayerStatType stat)
            ? stat
            : PlayerStatType.Insight;
    }

    private Method ParseMethod(string methodString)
    {
        return Enum.TryParse<Method>(methodString, out Method method)
            ? method
            : Method.Standard;
    }

    private MentalCategory ParseCategory(string categoryString)
    {
        return Enum.TryParse<MentalCategory>(categoryString, out MentalCategory category)
            ? category
            : MentalCategory.Analytical;
    }
}
