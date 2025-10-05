using System;
using System.Collections.Generic;

/// <summary>
/// PROJECTION PRINCIPLE: Pure projection function that returns what WOULD happen
/// without modifying any game state. The resolver NEVER modifies state directly.
/// Parallel to CategoricalEffectResolver in Conversation system.
/// </summary>
public class PhysicalEffectResolver
{
    private readonly PlayerExertionCalculator _exertionCalculator;

    public PhysicalEffectResolver(PlayerExertionCalculator exertionCalculator)
    {
        _exertionCalculator = exertionCalculator ?? throw new ArgumentNullException(nameof(exertionCalculator));
    }

    /// <summary>
    /// PROJECTION: Returns what WILL happen when card is played.
    /// Calculates all resource changes without modifying state.
    /// Perfect information for player decision-making.
    /// DUAL BALANCE SYSTEM: Combines action-based balance + approach-based balance
    /// </summary>
    public PhysicalCardEffectResult ProjectCardEffects(CardInstance card, PhysicalSession session, Player player, PhysicalActionType actionType)
    {
        PhysicalCardEffectResult result = new PhysicalCardEffectResult
        {
            Card = card,
            PositionChange = 0,
            BreakthroughChange = 0,
            DangerChange = 0,
            BalanceChange = 0,
            ReadinessChange = 0,
            HealthCost = 0,
            StaminaCost = 0,
            CoinsCost = 0,
            CardsToDraw = 1,  // Standard: draw 1 card after playing
            EndsSession = false,
            EffectDescription = ""
        };

        PhysicalCard template = card.PhysicalCardTemplate;
        if (template == null)
        {
            result.EffectDescription = "Invalid card";
            return result;
        }

        // Calculate player exertion state for dynamic costs
        PlayerExertionState exertion = _exertionCalculator.CalculateExertion(player);
        int costModifier = exertion.GetPhysicalCostModifier();

        // Builder resource: Position
        // Spend Position cost (modified by physical exertion)
        int modifiedPositionCost = Math.Max(0, template.PositionCost + costModifier);
        result.PositionChange -= modifiedPositionCost;

        // Generate Position from Foundation cards (Depth 1-2)
        int positionGen = template.GetPositionGeneration();
        result.PositionChange += positionGen;

        // DUAL BALANCE SYSTEM:
        // 1. Action-based balance (Assess vs Execute rhythm)
        int actionBalance = actionType switch
        {
            PhysicalActionType.Assess => -2,   // Drawing/assessing decreases balance
            PhysicalActionType.Execute => +1,  // Executing increases balance
            _ => 0
        };

        // 2. Approach-based balance (card approach)
        int approachBalance = template.Approach switch
        {
            Approach.Methodical => -1,
            Approach.Standard => 0,
            Approach.Aggressive => 1,
            Approach.Reckless => 2,
            _ => 0
        };

        // Combine both balance effects
        result.BalanceChange = actionBalance + approachBalance;

        // Victory resource: Breakthrough calculated from categorical properties via PhysicalCardEffectCatalog
        result.BreakthroughChange = PhysicalCardEffectCatalog.GetProgressFromProperties(template.Depth, template.Category);

        // Consequence resource: Danger calculated from categorical properties via PhysicalCardEffectCatalog
        int baseDanger = PhysicalCardEffectCatalog.GetDangerFromProperties(template.Depth, template.Approach);
        int riskModifier = exertion.GetRiskModifier();
        result.DangerChange = baseDanger + riskModifier;

        // Balance modifier: High positive balance increases Danger
        int projectedBalance = session.Commitment + result.BalanceChange;
        if (projectedBalance > 5)
        {
            result.DangerChange += 1;
        }

        // Strategic resource costs - PRE-CALCULATED at parse time via PhysicalCardEffectCatalog
        // Resolver just uses the values calculated during parsing (no runtime calculation)
        result.StaminaCost = template.StaminaCost;
        result.HealthCost = template.DirectHealthCost;
        result.CoinsCost = template.CoinCost;

        // Session end detection: Check if this card would end session
        int projectedProgress = session.CurrentBreakthrough + result.BreakthroughChange;
        int projectedDanger = session.CurrentDanger + result.DangerChange;
        result.EndsSession = projectedProgress >= 20 || projectedDanger >= session.MaxDanger;

        // Build effect description
        List<string> effects = new List<string>();

        if (result.PositionChange != 0)
            effects.Add($"Position {(result.PositionChange > 0 ? "+" : "")}{result.PositionChange}");
        if (result.BreakthroughChange > 0)
            effects.Add($"Breakthrough +{result.BreakthroughChange}");
        if (result.DangerChange > 0)
            effects.Add($"Danger +{result.DangerChange}");
        if (result.BalanceChange != 0)
            effects.Add($"Balance {(result.BalanceChange > 0 ? "+" : "")}{result.BalanceChange}");

        if (result.HealthCost > 0) effects.Add($"Health -{result.HealthCost}");
        if (result.StaminaCost > 0) effects.Add($"Stamina -{result.StaminaCost}");
        if (result.CoinsCost > 0) effects.Add($"Coins -{result.CoinsCost}");

        result.EffectDescription = effects.Count > 0 ? string.Join(", ", effects) : "No effect";

        return result;
    }
}
