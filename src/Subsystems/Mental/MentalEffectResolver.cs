using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// PROJECTION PRINCIPLE: Pure projection function that returns what WOULD happen
/// without modifying any game state. The resolver NEVER modifies state directly.
/// Parallel to CategoricalEffectResolver in Conversation system.
/// </summary>
public class MentalEffectResolver
{
    private readonly PlayerExertionCalculator _exertionCalculator;

    public MentalEffectResolver(PlayerExertionCalculator exertionCalculator)
    {
        _exertionCalculator = exertionCalculator ?? throw new ArgumentNullException(nameof(exertionCalculator));
    }

    /// <summary>
    /// PROJECTION: Returns what WILL happen when card is played.
    /// Calculates all resource changes without modifying state.
    /// Perfect information for player decision-making.
    /// DUAL BALANCE SYSTEM: Combines action-based balance + method-based balance
    ///
    /// PERFECT INFORMATION ENHANCEMENT: Tracks base + all bonuses separately for UI display.
    /// </summary>
    public MentalCardEffectResult ProjectCardEffects(CardInstance card, MentalSession session, Player player, MentalActionType actionType)
    {
        MentalCardEffectResult result = new MentalCardEffectResult
        {
            Card = card,
            AttentionChange = 0,
            ProgressChange = 0,
            ExposureChange = 0,
            UnderstandingChange = 0,
            HealthCost = 0,
            StaminaCost = 0,
            CoinsCost = 0,
            CardsToDraw = 1,  // Standard: draw 1 card after playing
            EndsSession = false,
            EffectDescription = ""
        };

        MentalCard template = card.MentalCardTemplate;
        if (template == null)
        {
            result.EffectDescription = "Invalid card";
            return result;
        }

        // Calculate player exertion state for dynamic costs
        PlayerExertionState exertion = _exertionCalculator.CalculateExertion(player);
        int costModifier = exertion.GetMentalCostModifier();

        // ===== ATTENTION (Builder Resource) =====
        // BASE: Attention cost from card depth
        result.BaseAttention = -template.AttentionCost;

        // BONUS: Exertion modifier (penalty when tired)
        if (costModifier != 0)
        {
            result.AttentionBonuses.Add(new EffectBonus
            {
                Source = "Exertion",
                Amount = -costModifier,  // Negative because it increases cost
                Type = BonusType.Exertion
            });
        }

        // Generate Attention from Foundation cards (Depth 1-2)
        int attentionGen = template.GetAttentionGeneration();
        if (attentionGen > 0)
        {
            result.AttentionBonuses.Add(new EffectBonus
            {
                Source = "Foundation Card",
                Amount = attentionGen,
                Type = BonusType.Other
            });
        }

        // Calculate final Attention change
        result.AttentionChange = result.BaseAttention + result.AttentionBonuses.Sum(b => b.Amount);

        // ===== PROGRESS (Victory Resource) =====
        // BASE: Progress from card categorical properties
        result.BaseProgress = MentalCardEffectCatalog.GetProgressFromProperties(template.Depth, template.Category);

        // BONUS 2: Stat Level (Player progression)
        if (template.BoundStat != PlayerStatType.None)
        {
            int statLevel = player.Stats.GetLevel(template.BoundStat);
            if (statLevel >= 5)
            {
                int bonus = (statLevel - 4) / 2;  // +1 at level 5-6, +2 at level 7-8, etc.
                result.ProgressBonuses.Add(new EffectBonus
                {
                    Source = $"{template.BoundStat} Level {statLevel}",
                    Amount = bonus,
                    Type = BonusType.StatLevel
                });
            }
        }

        // Calculate final Progress
        result.ProgressChange = result.BaseProgress + result.ProgressBonuses.Sum(b => b.Amount);

        // ===== EXPOSURE (Consequence Resource) =====
        // BASE: Exposure from card method and depth
        result.BaseExposure = MentalCardEffectCatalog.GetExposureFromProperties(template.Depth, template.Method);

        // BONUS 2: Exertion Risk Modifier
        int riskModifier = exertion.GetRiskModifier();
        if (riskModifier != 0)
        {
            result.ExposureBonuses.Add(new EffectBonus
            {
                Source = "Exertion State",
                Amount = riskModifier,
                Type = BonusType.Exertion
            });
        }

        // Calculate final Exposure
        result.ExposureChange = result.BaseExposure + result.ExposureBonuses.Sum(b => b.Amount);

        // Strategic resource costs - PRE-CALCULATED at parse time via MentalCardEffectCatalog
        // NOTE: Mental cards have NO health/stamina costs - only session-level Focus cost
        // Resolver just uses the values calculated during parsing (no runtime calculation)
        result.CoinsCost = template.CoinCost;

        // Session end detection: Check if this card would end session
        int projectedProgress = session.CurrentProgress + result.ProgressChange;
        int projectedExposure = session.CurrentExposure + result.ExposureChange;
        result.EndsSession = projectedProgress >= 20 || projectedExposure >= 10;

        // Build effect description
        List<string> effects = new List<string>();

        if (result.AttentionChange != 0)
            effects.Add($"Attention {(result.AttentionChange > 0 ? "+" : "")}{result.AttentionChange}");
        if (result.ProgressChange > 0)
            effects.Add($"Progress +{result.ProgressChange}");
        if (result.ExposureChange > 0)
            effects.Add($"Exposure +{result.ExposureChange}");

        if (result.HealthCost > 0) effects.Add($"Health -{result.HealthCost}");
        if (result.StaminaCost > 0) effects.Add($"Stamina -{result.StaminaCost}");
        if (result.CoinsCost > 0) effects.Add($"Coins -{result.CoinsCost}");

        result.EffectDescription = effects.Count > 0 ? string.Join(", ", effects) : "No effect";

        return result;
    }
}
