using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Comprehensive catalog of all card effect formulas.
/// Organized by stat and depth with multiple variants per combination.
///
/// STAT-TO-RESOURCE MAPPING:
/// - Insight → Cards (Information gathering)
/// - Rapport → Cadence (Rhythm management)
/// - Authority → Momentum (Direct progress)
/// - Commerce → Doubt (Risk mitigation)
/// - Cunning → Initiative (Action economy)
/// </summary>
public static class CardEffectCatalog
{
    /// <summary>
    /// Get all available effect variants for a stat at a specific depth.
    /// Returns multiple options for content variety.
    /// </summary>
    public static List<CardEffectFormula> GetEffectVariants(PlayerStatType stat, int depth)
    {
        return stat switch
        {
            PlayerStatType.Insight => GetInsightEffects(depth),
            PlayerStatType.Rapport => GetRapportEffects(depth),
            PlayerStatType.Authority => GetAuthorityEffects(depth),
            PlayerStatType.Commerce => GetCommerceEffects(depth),
            PlayerStatType.Cunning => GetCunningEffects(depth),
            _ => new List<CardEffectFormula>()
        };
    }

    /// <summary>
    /// Get a specific effect by card index within depth tier.
    /// Index 0 = first base card, index 1 = second base card, etc.
    /// Signature cards are at the end of the list for each depth.
    /// </summary>
    public static CardEffectFormula GetEffectByIndex(PlayerStatType stat, int depth, int index)
    {
        var variants = GetEffectVariants(stat, depth);
        if (index >= 0 && index < variants.Count)
        {
            return variants[index];
        }
        return variants.FirstOrDefault();
    }

    /// <summary>
    /// Get a specific effect variant by name.
    /// Used when JSON specifies which variant to use.
    /// </summary>
    public static CardEffectFormula GetEffectByVariantName(PlayerStatType stat, int depth, string variantName)
    {
        var variants = GetEffectVariants(stat, depth);
        return variants.FirstOrDefault(v => GetVariantName(v) == variantName) ?? variants.FirstOrDefault();
    }

    private static string GetVariantName(CardEffectFormula formula)
    {
        // Generate consistent names for variants
        if (formula.FormulaType == EffectFormulaType.Compound)
        {
            return "Compound";
        }

        string baseName = formula.FormulaType.ToString();
        if (formula.ScalingSource.HasValue)
        {
            baseName += $"_{formula.ScalingSource.Value}";
        }
        return baseName;
    }

    // ==================== INSIGHT (CARDS) ====================

    private static List<CardEffectFormula> GetInsightEffects(int depth)
    {
        return depth switch
        {
            // Foundation (Depth 1-2) - 4 base cards only, NO signatures
            1 => new List<CardEffectFormula>
            {
                // "Quick Scan" - Base Echo
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Fixed,
                    TargetResource = ConversationResourceType.Cards,
                    BaseValue = 2
                },
                // "Ask Question" - Base Statement
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Cards, BaseValue = 1 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 1 }
                    }
                }
            },
            2 => new List<CardEffectFormula>
            {
                // "Careful Analysis" - Base Echo
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Fixed,
                    TargetResource = ConversationResourceType.Cards,
                    BaseValue = 2
                },
                // "Notice Detail" - Base Statement
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Cards, BaseValue = 2 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 1 }
                    }
                }
            },

            // Standard (Depth 3-4) - 3 base + 1 signature
            3 => new List<CardEffectFormula>
            {
                // "Connect Evidence" - Base Echo
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Scaling,
                    TargetResource = ConversationResourceType.Cards,
                    ScalingSource = ScalingSourceType.TotalStatements,
                    ScalingMultiplier = 0.5m,
                    ScalingMax = 3
                },
                // "Cross-Reference" - Base Statement
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Cards, BaseValue = 2 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 1 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 1 }
                    }
                },
                // "Pattern Synthesis" - Signature Echo (3+ Insight Statements)
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Scaling,
                    TargetResource = ConversationResourceType.Cards,
                    ScalingSource = ScalingSourceType.InsightStatements,
                    ScalingMultiplier = 1.0m,
                    ScalingMax = 4
                }
            },
            4 => new List<CardEffectFormula>
            {
                // "Identify Pattern" - Base Statement
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Cards, BaseValue = 3 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 2 }
                    }
                },
                // "Complex Analysis" - Signature Statement (3+ Insight Statements)
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Cards, BaseValue = 4 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 3 }
                    }
                }
            },

            // Advanced (Depth 5-6) - 2 base + 1 signature
            5 => new List<CardEffectFormula>
            {
                // "Synthesize Information" - Base Echo
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Scaling,
                    TargetResource = ConversationResourceType.Cards,
                    ScalingSource = ScalingSourceType.TotalStatements,
                    ScalingMultiplier = 0.5m,
                    ScalingMax = 5
                },
                // "Reveal Implication" - Base Statement
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Cards, BaseValue = 3 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 4 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 2 }
                    }
                },
                // "Analytical Mastery" - Signature Echo (5+ Insight Statements)
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Scaling,
                    TargetResource = ConversationResourceType.Cards,
                    ScalingSource = ScalingSourceType.InsightStatements,
                    ScalingMultiplier = 1.0m,
                    ScalingMax = 6
                }
            },
            6 => new List<CardEffectFormula>
            {
                // "Draw Conclusion" - Base Statement
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Cards, BaseValue = 4 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 5 }
                    }
                },
                // "Perfect Deduction" - Signature Statement (5+ Insight Statements)
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Cards, BaseValue = 6 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 6 }
                    }
                }
            },

            // Master (Depth 7-8) - 1 signature only (specialist tier)
            8 => new List<CardEffectFormula>
            {
                // "Complete Understanding" - Signature Statement (8+ Insight Statements)
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Cards, BaseValue = 8 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 10 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 4 }
                    }
                }
            },

            _ => new List<CardEffectFormula>()
        };
    }

    // ==================== RAPPORT (CADENCE) ====================

    private static List<CardEffectFormula> GetRapportEffects(int depth)
    {
        return depth switch
        {
            // Foundation (Depth 1-2)
            1 or 2 => new List<CardEffectFormula>
            {
                // Type A: Reduce Cadence by 1
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Fixed,
                    TargetResource = ConversationResourceType.Cadence,
                    BaseValue = -1
                },
                // Compound: -1 Cadence, +1 Momentum
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Cadence, BaseValue = -1 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 1 }
                    }
                },
                // Compound: +1 Initiative, +1 Momentum (balanced growth)
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 1 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 1 }
                    }
                }
            },

            // Standard (Depth 3-4)
            3 or 4 => new List<CardEffectFormula>
            {
                // Type A: Reduce Cadence by 2
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Fixed,
                    TargetResource = ConversationResourceType.Cadence,
                    BaseValue = -2
                },
                // Type B: Reduce Cadence by 1 per 3 Statements (max 3)
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Scaling,
                    TargetResource = ConversationResourceType.Cadence,
                    ScalingSource = ScalingSourceType.TotalStatements,
                    ScalingMultiplier = -0.33m,
                    ScalingMax = 3
                },
                // Type E: Set Cadence to 0
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Setting,
                    TargetResource = ConversationResourceType.Cadence,
                    SetValue = 0
                }
            },

            // Advanced (Depth 5-6)
            5 or 6 => new List<CardEffectFormula>
            {
                // Type B: Reduce Cadence by 1 per 2 Doubt
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Scaling,
                    TargetResource = ConversationResourceType.Cadence,
                    ScalingSource = ScalingSourceType.Doubt,
                    ScalingMultiplier = -0.5m
                },
                // Type D: Reduce Cadence by 3, Consume 2 Momentum
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Trading,
                    TargetResource = ConversationResourceType.Cadence,
                    TradeRatio = -3,
                    ConsumeResource = ConversationResourceType.Momentum,
                    ConsumeAmount = 2
                },
                // Type E: Set Cadence to -2
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Setting,
                    TargetResource = ConversationResourceType.Cadence,
                    SetValue = -2
                }
            },

            // Master (Depth 7-8)
            7 or 8 => new List<CardEffectFormula>
            {
                // Type A: Reduce Cadence by 5
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Fixed,
                    TargetResource = ConversationResourceType.Cadence,
                    BaseValue = -5
                },
                // Type E: Set Cadence to -5
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Setting,
                    TargetResource = ConversationResourceType.Cadence,
                    SetValue = -5
                },
                // Type B: Reduce Cadence by 1 per 2 Momentum
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Scaling,
                    TargetResource = ConversationResourceType.Cadence,
                    ScalingSource = ScalingSourceType.Momentum,
                    ScalingMultiplier = -0.5m
                }
            },

            _ => new List<CardEffectFormula>()
        };
    }

    // ==================== AUTHORITY (MOMENTUM) ====================

    private static List<CardEffectFormula> GetAuthorityEffects(int depth)
    {
        return depth switch
        {
            // Foundation (Depth 1-2)
            1 or 2 => new List<CardEffectFormula>
            {
                // Type A: +2 Momentum
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Fixed,
                    TargetResource = ConversationResourceType.Momentum,
                    BaseValue = 2
                },
                // Compound: +2 Momentum, +1 Doubt
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 2 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Doubt, BaseValue = 1 }
                    }
                }
            },

            // Standard (Depth 3-4)
            3 or 4 => new List<CardEffectFormula>
            {
                // Type A: +4 Momentum
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Fixed,
                    TargetResource = ConversationResourceType.Momentum,
                    BaseValue = 4
                },
                // Type B: +1 Momentum per positive Cadence
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Scaling,
                    TargetResource = ConversationResourceType.Momentum,
                    ScalingSource = ScalingSourceType.PositiveCadence,
                    ScalingMultiplier = 1.0m
                },
                // Compound: +5 Momentum, +2 Doubt
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 5 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Doubt, BaseValue = 2 }
                    }
                }
            },

            // Advanced (Depth 5-6)
            5 or 6 => new List<CardEffectFormula>
            {
                // Compound: +7 Momentum, +3 Doubt
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 7 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Doubt, BaseValue = 3 }
                    }
                },
                // Type D: Consume 3 Momentum: +10 Momentum
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Trading,
                    TargetResource = ConversationResourceType.Momentum,
                    TradeRatio = 10,
                    ConsumeResource = ConversationResourceType.Momentum,
                    ConsumeAmount = 3
                },
                // Type B: +1 Momentum per 2 Statements
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Scaling,
                    TargetResource = ConversationResourceType.Momentum,
                    ScalingSource = ScalingSourceType.TotalStatements,
                    ScalingMultiplier = 0.5m
                }
            },

            // Master (Depth 7-8)
            7 or 8 => new List<CardEffectFormula>
            {
                // Compound: +12 Momentum, +4 Doubt
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 12 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Doubt, BaseValue = 4 }
                    }
                },
                // Type B: +2 Momentum per card in Mind
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Scaling,
                    TargetResource = ConversationResourceType.Momentum,
                    ScalingSource = ScalingSourceType.MindCards,
                    ScalingMultiplier = 2.0m
                }
            },

            _ => new List<CardEffectFormula>()
        };
    }

    // ==================== COMMERCE (DOUBT) ====================

    private static List<CardEffectFormula> GetCommerceEffects(int depth)
    {
        return depth switch
        {
            // Foundation (Depth 1-2)
            1 or 2 => new List<CardEffectFormula>
            {
                // Type A: -1 Doubt
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Fixed,
                    TargetResource = ConversationResourceType.Doubt,
                    BaseValue = -1
                },
                // Type D: -1 Doubt, Consume 1 Momentum
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Trading,
                    TargetResource = ConversationResourceType.Doubt,
                    TradeRatio = -1,
                    ConsumeResource = ConversationResourceType.Momentum,
                    ConsumeAmount = 1
                }
            },

            // Standard (Depth 3-4)
            3 or 4 => new List<CardEffectFormula>
            {
                // Type A: -2 Doubt
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Fixed,
                    TargetResource = ConversationResourceType.Doubt,
                    BaseValue = -2
                },
                // Type D: -3 Doubt, Consume 2 Momentum
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Trading,
                    TargetResource = ConversationResourceType.Doubt,
                    TradeRatio = -3,
                    ConsumeResource = ConversationResourceType.Momentum,
                    ConsumeAmount = 2
                }
            },

            // Advanced (Depth 5-6)
            5 or 6 => new List<CardEffectFormula>
            {
                // Type B: -1 Doubt per negative Cadence
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Scaling,
                    TargetResource = ConversationResourceType.Doubt,
                    ScalingSource = ScalingSourceType.NegativeCadence,
                    ScalingMultiplier = -1.0m
                },
                // Type D: -4 Doubt, Consume 3 Momentum
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Trading,
                    TargetResource = ConversationResourceType.Doubt,
                    TradeRatio = -4,
                    ConsumeResource = ConversationResourceType.Momentum,
                    ConsumeAmount = 3
                }
            },

            // Master (Depth 7-8)
            7 or 8 => new List<CardEffectFormula>
            {
                // Type E: Set Doubt to 0
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Setting,
                    TargetResource = ConversationResourceType.Doubt,
                    SetValue = 0
                },
                // Type D: -6 Doubt, Consume 4 Momentum
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Trading,
                    TargetResource = ConversationResourceType.Doubt,
                    TradeRatio = -6,
                    ConsumeResource = ConversationResourceType.Momentum,
                    ConsumeAmount = 4
                },
                // Type B: -1 Doubt per 2 Statements
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Scaling,
                    TargetResource = ConversationResourceType.Doubt,
                    ScalingSource = ScalingSourceType.TotalStatements,
                    ScalingMultiplier = -0.5m
                }
            },

            _ => new List<CardEffectFormula>()
        };
    }

    // ==================== CUNNING (INITIATIVE) ====================

    private static List<CardEffectFormula> GetCunningEffects(int depth)
    {
        return depth switch
        {
            // Foundation (Depth 1-2)
            1 or 2 => new List<CardEffectFormula>
            {
                // Type A: +2 Initiative
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Fixed,
                    TargetResource = ConversationResourceType.Initiative,
                    BaseValue = 2
                },
                // Compound: +2 Initiative, +1 Momentum
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 2 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 1 }
                    }
                }
            },

            // Standard (Depth 3-4)
            3 or 4 => new List<CardEffectFormula>
            {
                // Type A: +4 Initiative
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Fixed,
                    TargetResource = ConversationResourceType.Initiative,
                    BaseValue = 4
                },
                // Type B: +1 Initiative per 2 cards in Mind
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Scaling,
                    TargetResource = ConversationResourceType.Initiative,
                    ScalingSource = ScalingSourceType.MindCards,
                    ScalingMultiplier = 0.5m
                },
                // Compound: +3 Initiative, Draw 1
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 3 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Cards, BaseValue = 1 }
                    }
                }
            },

            // Advanced (Depth 5-6)
            5 or 6 => new List<CardEffectFormula>
            {
                // Type B: +1 Initiative per Doubt
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Scaling,
                    TargetResource = ConversationResourceType.Initiative,
                    ScalingSource = ScalingSourceType.Doubt,
                    ScalingMultiplier = 1.0m
                },
                // Type B: +1 Initiative per positive Cadence
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Scaling,
                    TargetResource = ConversationResourceType.Initiative,
                    ScalingSource = ScalingSourceType.PositiveCadence,
                    ScalingMultiplier = 1.0m
                },
                // Type D: +6 Initiative, Consume 2 Momentum
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Trading,
                    TargetResource = ConversationResourceType.Initiative,
                    TradeRatio = 6,
                    ConsumeResource = ConversationResourceType.Momentum,
                    ConsumeAmount = 2
                }
            },

            // Master (Depth 7-8)
            7 or 8 => new List<CardEffectFormula>
            {
                // Type A: +10 Initiative
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Fixed,
                    TargetResource = ConversationResourceType.Initiative,
                    BaseValue = 10
                },
                // Type B: +2 Initiative per Statement in Spoken
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Scaling,
                    TargetResource = ConversationResourceType.Initiative,
                    ScalingSource = ScalingSourceType.TotalStatements,
                    ScalingMultiplier = 2.0m
                }
            },

            _ => new List<CardEffectFormula>()
        };
    }

    /// <summary>
    /// Get suggested Initiative cost for a depth and stat.
    /// Different stats have different cost curves based on their resource generation efficiency.
    /// </summary>
    public static int GetSuggestedInitiativeCost(PlayerStatType stat, int depth)
    {
        // Cunning: Cheapest (generates Initiative)
        if (stat == PlayerStatType.Cunning)
        {
            return depth switch
            {
                1 or 2 => 0,
                3 or 4 => 2,
                5 or 6 => 4,
                7 or 8 => 6,
                _ => 0
            };
        }

        // Rapport: Cheap (sustainable rhythm)
        if (stat == PlayerStatType.Rapport)
        {
            return depth switch
            {
                1 or 2 => 0,
                3 or 4 => 2,
                5 or 6 => 4,
                7 or 8 => 6,
                _ => 0
            };
        }

        // Insight: Moderate (information value)
        if (stat == PlayerStatType.Insight)
        {
            return depth switch
            {
                1 or 2 => 0,
                3 or 4 => 3,
                5 or 6 => 5,
                7 or 8 => 7,
                _ => 0
            };
        }

        // Commerce: Expensive (safety costs)
        if (stat == PlayerStatType.Commerce)
        {
            return depth switch
            {
                1 or 2 => 0,
                3 or 4 => 4,
                5 or 6 => 6,
                7 or 8 => 8,
                _ => 0
            };
        }

        // Authority: Most Expensive (power costs)
        if (stat == PlayerStatType.Authority)
        {
            return depth switch
            {
                1 or 2 => 0,
                3 or 4 => 4,
                5 or 6 => 6,
                7 or 8 => 8,
                _ => 0
            };
        }

        return 0;
    }

    /// <summary>
    /// Get suggested Echo percentage for a stat at Foundation tier.
    /// Higher percentage = more repeatable cards of that stat.
    /// </summary>
    public static int GetSuggestedEchoPercentage(PlayerStatType stat)
    {
        return stat switch
        {
            PlayerStatType.Cunning => 80,   // Tactics highly repeatable
            PlayerStatType.Rapport => 75,   // Empathy techniques repeatable
            PlayerStatType.Commerce => 65,  // Negotiation tools repeatable
            PlayerStatType.Insight => 60,   // Analysis repeats, insights don't
            PlayerStatType.Authority => 50, // Commands repeat, declarations don't
            _ => 60
        };
    }
}