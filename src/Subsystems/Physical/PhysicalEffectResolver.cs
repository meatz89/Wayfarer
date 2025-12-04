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
    ///
    /// PERFECT INFORMATION ENHANCEMENT: Tracks base + all bonuses separately for UI display.
    /// </summary>
    public PhysicalCardEffectResult ProjectCardEffects(CardInstance card, PhysicalSession session, Player player, PhysicalActionType actionType)
    {
        PhysicalCardEffectResult result = new PhysicalCardEffectResult
        {
            Card = card,
            ExertionChange = 0,
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
        PlayerExertionState exertion = _exertionCalculator.CalculateExertion();
        int costModifier = exertion.GetPhysicalCostModifier();

        // ===== EXERTION (Builder Resource) =====
        // BASE: Exertion cost from card depth
        result.BaseExertion = -template.ExertionCost;

        // BONUS: Exertion modifier (penalty when tired)
        if (costModifier != 0)
        {
            result.ExertionBonuses.Add(new EffectBonus
            {
                Source = "Exertion",
                Amount = -costModifier,  // Negative because it increases cost
                Type = BonusType.Exertion
            });
        }

        // Generate Exertion from Foundation cards (Depth 1-2)
        int exertionGen = template.GetExertionGeneration();
        if (exertionGen > 0)
        {
            result.ExertionBonuses.Add(new EffectBonus
            {
                Source = "Foundation Card",
                Amount = exertionGen,
                Type = BonusType.Other
            });
        }

        // Calculate final Exertion change
        result.ExertionChange = result.BaseExertion + result.ExertionBonuses.Sum(b => b.Amount);

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

        // ===== BREAKTHROUGH (Victory Resource) =====
        // BASE: Breakthrough from card (PRE-CALCULATED at parse time, stored on card)
        result.BaseBreakthrough = template.BaseBreakthrough;

        // BONUS 2: Stat Level (Player progression)
        if (template.BoundStat != PlayerStatType.None)
        {
            int statLevel = template.BoundStat switch
            {
                PlayerStatType.Insight => player.Insight,
                PlayerStatType.Rapport => player.Rapport,
                PlayerStatType.Authority => player.Authority,
                PlayerStatType.Diplomacy => player.Diplomacy,
                PlayerStatType.Cunning => player.Cunning,
                _ => 0
            };
            if (statLevel >= 5)
            {
                int bonus = (statLevel - 4) / 2;  // +1 at level 5-6, +2 at level 7-8, etc.
                result.BreakthroughBonuses.Add(new EffectBonus
                {
                    Source = $"{template.BoundStat} Level {statLevel}",
                    Amount = bonus,
                    Type = BonusType.StatLevel
                });
            }
        }

        // Calculate final Breakthrough
        result.BreakthroughChange = result.BaseBreakthrough + result.BreakthroughBonuses.Sum(b => b.Amount);

        // ===== DANGER (Consequence Resource) =====
        // BASE: Danger from card (PRE-CALCULATED at parse time, stored on card)
        result.BaseDanger = template.BaseDanger;

        // BONUS 2: Exertion Risk Modifier
        int riskModifier = exertion.GetRiskModifier();
        if (riskModifier != 0)
        {
            result.DangerBonuses.Add(new EffectBonus
            {
                Source = "Exertion State",
                Amount = riskModifier,
                Type = BonusType.Exertion
            });
        }

        // BONUS 3: Balance Modifier (High positive Aggression increases Danger)
        int projectedAggression = session.Aggression + result.BalanceChange;
        if (projectedAggression > 5)
        {
            result.DangerBonuses.Add(new EffectBonus
            {
                Source = "High Balance",
                Amount = 1,
                Type = BonusType.Other
            });
        }

        // Calculate final Danger
        result.DangerChange = result.BaseDanger + result.DangerBonuses.Sum(b => b.Amount);

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

        if (result.ExertionChange != 0)
            effects.Add($"Exertion {(result.ExertionChange > 0 ? "+" : "")}{result.ExertionChange}");
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
