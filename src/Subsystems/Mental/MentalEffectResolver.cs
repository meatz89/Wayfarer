using System;
using System.Collections.Generic;

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
    /// </summary>
    public MentalCardEffectResult ProjectCardEffects(CardInstance card, MentalSession session, Player player, MentalActionType actionType)
    {
        MentalCardEffectResult result = new MentalCardEffectResult
        {
            Card = card,
            AttentionChange = 0,
            ProgressChange = 0,
            ExposureChange = 0,
            BalanceChange = 0,
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

        // Builder resource: Attention
        // Spend Attention cost (modified by mental exertion)
        int modifiedAttentionCost = Math.Max(0, template.AttentionCost + costModifier);
        result.AttentionChange -= modifiedAttentionCost;

        // Generate Attention from Foundation cards (Depth 1-2)
        int attentionGen = template.GetAttentionGeneration();
        result.AttentionChange += attentionGen;

        // DUAL BALANCE SYSTEM:
        // 1. Action-based balance (Observe vs Act rhythm)
        int actionBalance = actionType switch
        {
            MentalActionType.Observe => -2,  // Drawing/observing decreases balance
            MentalActionType.Act => +1,       // Acting increases balance
            _ => 0
        };

        // 2. Method-based balance (card approach)
        int methodBalance = template.Method switch
        {
            Method.Careful => -1,
            Method.Standard => 0,
            Method.Bold => 1,
            Method.Reckless => 2,
            _ => 0
        };

        // Combine both balance effects
        result.BalanceChange = actionBalance + methodBalance;

        // Victory resource: Progress calculated from categorical properties via MentalCardEffectCatalog
        result.ProgressChange = MentalCardEffectCatalog.GetProgressFromProperties(template.Depth, template.Category);

        // Consequence resource: Exposure calculated from categorical properties via MentalCardEffectCatalog
        int baseExposure = MentalCardEffectCatalog.GetExposureFromProperties(template.Depth, template.Method);
        int riskModifier = exertion.GetRiskModifier();
        result.ExposureChange = baseExposure + riskModifier;

        // Balance modifier: High positive balance increases Exposure
        int projectedBalance = session.ObserveActBalance + result.BalanceChange;
        if (projectedBalance > 5)
        {
            result.ExposureChange += 1;
        }

        // Strategic resource costs - PRE-CALCULATED at parse time via MentalCardEffectCatalog
        // Resolver just uses the values calculated during parsing (no runtime calculation)
        result.StaminaCost = template.StaminaCost;
        result.HealthCost = template.DirectHealthCost;
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
        if (result.BalanceChange != 0)
            effects.Add($"Balance {(result.BalanceChange > 0 ? "+" : "")}{result.BalanceChange}");

        if (result.HealthCost > 0) effects.Add($"Health -{result.HealthCost}");
        if (result.StaminaCost > 0) effects.Add($"Stamina -{result.StaminaCost}");
        if (result.CoinsCost > 0) effects.Add($"Coins -{result.CoinsCost}");

        result.EffectDescription = effects.Count > 0 ? string.Join(", ", effects) : "No effect";

        return result;
    }
}
