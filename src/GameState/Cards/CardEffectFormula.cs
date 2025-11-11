/// <summary>
/// Defines the formula types for card effects.
/// Each type represents a different calculation pattern.
/// </summary>
public enum EffectFormulaType
{
    /// <summary>Type A: Fixed value - Simple, predictable (Foundation tier)</summary>
    Fixed,

    /// <summary>Type B: Linear scaling - Effect = base × state_value</summary>
    Scaling,

    /// <summary>Type C: State conditional - Effect depends on threshold</summary>
    Conditional,

    /// <summary>Type D: Resource trading - Consume X of A to gain Y of B</summary>
    Trading,

    /// <summary>Type E: State setting - Transform state directly</summary>
    Setting,

    /// <summary>Compound: Multiple fixed effects (e.g., +2I +1M)</summary>
    Compound
}

/// <summary>
/// Represents a complete card effect with formula-based calculation.
/// This replaces the old template system with a flexible formula engine.
/// </summary>
public class CardEffectFormula
{
    // Formula type determines how the effect is calculated
    public EffectFormulaType FormulaType { get; set; }

    // Primary resource being affected
    public SocialChallengeResourceType TargetResource { get; set; }

    // Base value for Fixed/Compound types
    public int? BaseValue { get; set; }

    // Scaling properties for Type B
    public ScalingSourceType? ScalingSource { get; set; }
    public decimal ScalingMultiplier { get; set; } = 1.0m;
    public int? ScalingMax { get; set; } // Cap for scaling effects

    // Conditional properties for Type C
    public SocialChallengeResourceType? ConditionResource { get; set; }
    public int? ConditionThreshold { get; set; }
    public int? ConditionMetValue { get; set; }
    public int? ConditionUnmetValue { get; set; }

    // Trading properties for Type D
    public SocialChallengeResourceType? ConsumeResource { get; set; }
    public int? ConsumeAmount { get; set; }
    public int? TradeRatio { get; set; }

    // Setting properties for Type E
    public int? SetValue { get; set; } // Direct state value to set

    // Compound effects (multiple fixed values)
    public List<CardEffectFormula> CompoundEffects { get; set; }

    /// <summary>
    /// Calculate the actual effect value based on game state.
    /// Returns the amount to apply to the target resource.
    /// </summary>
    public int CalculateEffect(SocialSession session)
    {
        switch (FormulaType)
        {
            case EffectFormulaType.Fixed:
                if (!BaseValue.HasValue)
                    throw new InvalidOperationException("Fixed effect formula missing required BaseValue");
                return BaseValue.Value;

            case EffectFormulaType.Scaling:
                return CalculateScaling(session);

            case EffectFormulaType.Conditional:
                return CalculateConditional(session);

            case EffectFormulaType.Trading:
                // Trading effects need to be handled specially (consume first, then apply)
                if (!TradeRatio.HasValue)
                    throw new InvalidOperationException("Trading effect formula missing required TradeRatio");
                return TradeRatio.Value;

            case EffectFormulaType.Setting:
                // Setting effects replace current value, not additive
                if (!SetValue.HasValue)
                    throw new InvalidOperationException("Setting effect formula missing required SetValue");
                return SetValue.Value;

            case EffectFormulaType.Compound:
                // Compound handled externally by iterating CompoundEffects
                return 0;

            default:
                return 0;
        }
    }

    private int CalculateScaling(SocialSession session)
    {
        if (!ScalingSource.HasValue) return 0;

        int sourceValue = ScalingSource.Value switch
        {
            ScalingSourceType.Doubt => session.CurrentDoubt,
            ScalingSourceType.PositiveCadence => Math.Max(0, session.Cadence),
            ScalingSourceType.NegativeCadence => Math.Abs(Math.Min(0, session.Cadence)),
            ScalingSourceType.Momentum => session.CurrentMomentum,
            ScalingSourceType.Initiative => session.CurrentInitiative,
            ScalingSourceType.MindCards => session.Deck.HandSize,
            ScalingSourceType.SpokenCards => session.Deck.SpokenPileCount,
            ScalingSourceType.DeckCards => session.Deck.RemainingDeckCards,
            ScalingSourceType.TotalStatements => session.GetTotalStatements(),
            ScalingSourceType.InsightStatements => session.GetStatementCount(PlayerStatType.Insight),
            ScalingSourceType.RapportStatements => session.GetStatementCount(PlayerStatType.Rapport),
            ScalingSourceType.AuthorityStatements => session.GetStatementCount(PlayerStatType.Authority),
            ScalingSourceType.DiplomacyStatements => session.GetStatementCount(PlayerStatType.Diplomacy),
            ScalingSourceType.CunningStatements => session.GetStatementCount(PlayerStatType.Cunning),
            _ => 0
        };

        int scaled = (int)(sourceValue * ScalingMultiplier);

        if (ScalingMax.HasValue)
        {
            scaled = Math.Min(scaled, ScalingMax.Value);
        }

        return scaled;
    }

    private int CalculateConditional(SocialSession session)
    {
        if (!ConditionResource.HasValue)
            throw new InvalidOperationException("Conditional effect formula missing required ConditionResource");
        if (!ConditionThreshold.HasValue)
            throw new InvalidOperationException("Conditional effect formula missing required ConditionThreshold");
        if (!ConditionMetValue.HasValue)
            throw new InvalidOperationException("Conditional effect formula missing required ConditionMetValue");
        if (!ConditionUnmetValue.HasValue)
            throw new InvalidOperationException("Conditional effect formula missing required ConditionUnmetValue");

        int currentValue = ConditionResource.Value switch
        {
            SocialChallengeResourceType.Doubt => session.CurrentDoubt,
            SocialChallengeResourceType.Cadence => session.Cadence,
            SocialChallengeResourceType.Momentum => session.CurrentMomentum,
            SocialChallengeResourceType.Initiative => session.CurrentInitiative,
            SocialChallengeResourceType.Cards => session.Deck.HandSize,
            _ => 0
        };

        bool conditionMet = currentValue >= ConditionThreshold.Value;

        if (conditionMet)
        {
            return ConditionMetValue.Value;
        }
        else
        {
            return ConditionUnmetValue.Value;
        }
    }

    /// <summary>
    /// Check if this trading effect can be executed (has required resources).
    /// </summary>
    public bool CanExecuteTrade(SocialSession session)
    {
        if (FormulaType != EffectFormulaType.Trading) return true;
        if (!ConsumeResource.HasValue)
            throw new InvalidOperationException("Trading effect formula missing required ConsumeResource");
        if (!ConsumeAmount.HasValue)
            throw new InvalidOperationException("Trading effect formula missing required ConsumeAmount");

        int available = ConsumeResource.Value switch
        {
            SocialChallengeResourceType.Momentum => session.CurrentMomentum,
            SocialChallengeResourceType.Initiative => session.CurrentInitiative,
            _ => 0
        };

        return available >= ConsumeAmount.Value;
    }

    /// <summary>
    /// Execute the trading effect (consume resources).
    /// Returns true if successful.
    /// </summary>
    public bool ExecuteTrade(SocialSession session)
    {
        if (!CanExecuteTrade(session)) return false;
        if (!ConsumeResource.HasValue)
            throw new InvalidOperationException("Trading effect formula missing required ConsumeResource");
        if (!ConsumeAmount.HasValue)
            throw new InvalidOperationException("Trading effect formula missing required ConsumeAmount");

        switch (ConsumeResource.Value)
        {
            case SocialChallengeResourceType.Momentum:
                session.CurrentMomentum = Math.Max(0, session.CurrentMomentum - ConsumeAmount.Value);
                return true;

            case SocialChallengeResourceType.Initiative:
                session.CurrentInitiative = Math.Max(0, session.CurrentInitiative - ConsumeAmount.Value);
                return true;

            default:
                return false;
        }
    }

    public override string ToString()
    {
        return FormulaType switch
        {
            EffectFormulaType.Fixed => $"{TargetResource} {(BaseValue > 0 ? "+" : "")}{BaseValue}",
            EffectFormulaType.Scaling => $"{TargetResource} +{ScalingMultiplier}×{ScalingSource}" + (ScalingMax.HasValue ? $" (max {ScalingMax})" : ""),
            EffectFormulaType.Conditional => $"{TargetResource} if {ConditionResource}≥{ConditionThreshold}: {ConditionMetValue}",
            EffectFormulaType.Trading => $"Consume {ConsumeAmount} {ConsumeResource}: {TargetResource} +{TradeRatio}",
            EffectFormulaType.Setting => $"{TargetResource} → {SetValue}",
            EffectFormulaType.Compound => CompoundEffects == null ? "Compound (no effects)" : $"Compound ({CompoundEffects.Count} effects)",
            _ => "Unknown"
        };
    }
}