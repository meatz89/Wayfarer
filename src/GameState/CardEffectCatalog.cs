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
    // SPECIALIST: Cards (2-6 draw) + UNIVERSAL: Momentum/Initiative

    private static List<CardEffectFormula> GetInsightEffects(int depth)
    {
        return depth switch
        {
            // Foundation (Depth 1-2) - Specialist 2x, Universal 1x
            1 => new List<CardEffectFormula>
            {
                // Pure specialist: Draw 2 cards
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Fixed,
                    TargetResource = ConversationResourceType.Cards,
                    BaseValue = 2
                },
                // Specialist + Initiative: Draw 2 cards, +1 Initiative
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Cards, BaseValue = 2 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 1 }
                    }
                },
                // Reduced specialist + Momentum: Draw 1 card, +1 Momentum
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Cards, BaseValue = 1 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 1 }
                    }
                }
            },
            2 => new List<CardEffectFormula>
            {
                // Pure specialist: Draw 2 cards
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Fixed,
                    TargetResource = ConversationResourceType.Cards,
                    BaseValue = 2
                },
                // Specialist + Momentum: Draw 2 cards, +1 Momentum
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Cards, BaseValue = 2 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 1 }
                    }
                },
                // Specialist + Initiative: Draw 2 cards, +1 Initiative
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Cards, BaseValue = 2 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 1 }
                    }
                }
            },

            // Standard (Depth 3-4) - Specialist 2.5x, Universal 1.5x
            3 => new List<CardEffectFormula>
            {
                // Pure specialist: Draw 3 cards
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Fixed,
                    TargetResource = ConversationResourceType.Cards,
                    BaseValue = 3
                },
                // Specialist + Momentum: Draw 3 cards, +2 Momentum
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Cards, BaseValue = 3 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 2 }
                    }
                },
                // Specialist + Initiative: Draw 3 cards, +1 Initiative
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Cards, BaseValue = 3 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 1 }
                    }
                },
                // Specialist + both universals: Draw 3 cards, +2 Momentum, +1 Initiative
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Cards, BaseValue = 3 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 2 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 1 }
                    }
                }
            },
            4 => new List<CardEffectFormula>
            {
                // Specialist + Momentum: Draw 3 cards, +2 Momentum
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Cards, BaseValue = 3 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 2 }
                    }
                },
                // Specialist + both universals: Draw 3 cards, +2 Momentum, +1 Initiative
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Cards, BaseValue = 3 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 2 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 1 }
                    }
                }
            },

            // Advanced (Depth 5-6) - Specialist 3x, Universal 2x
            5 => new List<CardEffectFormula>
            {
                // Specialist + both universals: Draw 4 cards, +3 Momentum, +2 Initiative
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Cards, BaseValue = 4 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 3 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 2 }
                    }
                }
            },
            6 => new List<CardEffectFormula>
            {
                // Specialist + both universals: Draw 4 cards, +3 Momentum, +2 Initiative
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Cards, BaseValue = 4 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 3 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 2 }
                    }
                }
            },

            // Master (Depth 7-8) - Specialist 3-4x, Universal 2-3x
            7 or 8 => new List<CardEffectFormula>
            {
                // Specialist + both universals: Draw 6 cards, +5 Momentum, +3 Initiative
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Cards, BaseValue = 6 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 5 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 3 }
                    }
                }
            },

            _ => new List<CardEffectFormula>()
        };
    }

    // ==================== RAPPORT (CADENCE) ====================
    // SPECIALIST: Cadence (-1 to -3) + UNIVERSAL: Momentum/Initiative

    private static List<CardEffectFormula> GetRapportEffects(int depth)
    {
        return depth switch
        {
            // Foundation (Depth 1-2) - Specialist 2x, Universal 1x
            1 or 2 => new List<CardEffectFormula>
            {
                // Pure specialist: -1 Cadence
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Fixed,
                    TargetResource = ConversationResourceType.Cadence,
                    BaseValue = -1
                },
                // Specialist + Momentum: -1 Cadence, +1 Momentum
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Cadence, BaseValue = -1 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 1 }
                    }
                },
                // Specialist + Initiative: -1 Cadence, +1 Initiative
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Cadence, BaseValue = -1 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 1 }
                    }
                },
                // Specialist + both universals: -1 Cadence, +1 Momentum, +1 Initiative
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Cadence, BaseValue = -1 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 1 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 1 }
                    }
                }
            },

            // Standard (Depth 3-4) - Specialist 2.5x, Universal 1.5x
            3 or 4 => new List<CardEffectFormula>
            {
                // Pure specialist: -2 Cadence
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Fixed,
                    TargetResource = ConversationResourceType.Cadence,
                    BaseValue = -2
                },
                // Specialist + Momentum: -2 Cadence, +2 Momentum
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Cadence, BaseValue = -2 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 2 }
                    }
                },
                // Specialist + Initiative: -2 Cadence, +2 Initiative
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Cadence, BaseValue = -2 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 2 }
                    }
                },
                // Specialist + both universals: -2 Cadence, +2 Momentum, +2 Initiative
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Cadence, BaseValue = -2 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 2 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 2 }
                    }
                }
            },

            // Advanced (Depth 5-6) - Specialist 3x, Universal 2x
            5 or 6 => new List<CardEffectFormula>
            {
                // Specialist + both universals: -3 Cadence, +3 Momentum, +3 Initiative
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Cadence, BaseValue = -3 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 3 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 3 }
                    }
                }
            },

            // Master (Depth 7-8) - Specialist 3-4x, Universal 2-3x
            7 or 8 => new List<CardEffectFormula>
            {
                // Specialist (Setting) + both universals: Set Cadence to -5, +8 Momentum, +5 Initiative
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Setting, TargetResource = ConversationResourceType.Cadence, SetValue = -5 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 8 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 5 }
                    }
                }
            },

            _ => new List<CardEffectFormula>()
        };
    }

    // ==================== AUTHORITY (MOMENTUM) ====================
    // SPECIALIST: Momentum (+2-12) + UNIVERSAL: Initiative + TRADE-OFF: Doubt

    private static List<CardEffectFormula> GetAuthorityEffects(int depth)
    {
        return depth switch
        {
            // Foundation (Depth 1-2) - Specialist 2x, No universal at depth 2, Doubt trade-off
            1 or 2 => new List<CardEffectFormula>
            {
                // Pure specialist: +2 Momentum
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Fixed,
                    TargetResource = ConversationResourceType.Momentum,
                    BaseValue = 2
                },
                // Specialist + trade-off: +2 Momentum, +1 Doubt
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

            // Standard (Depth 3-4) - Specialist 2.5x, Universal 1.5x, Doubt trade-off
            3 or 4 => new List<CardEffectFormula>
            {
                // Specialist only: +5 Momentum
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Fixed,
                    TargetResource = ConversationResourceType.Momentum,
                    BaseValue = 5
                },
                // Specialist + trade-off: +5 Momentum, +2 Doubt
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 5 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Doubt, BaseValue = 2 }
                    }
                },
                // Specialist + Initiative + trade-off: +5 Momentum, +1 Initiative, +2 Doubt
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 5 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 1 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Doubt, BaseValue = 2 }
                    }
                }
            },

            // Advanced (Depth 5-6) - Specialist 3x, Universal 2x, Doubt trade-off
            5 or 6 => new List<CardEffectFormula>
            {
                // Specialist + Initiative + trade-off: +8 Momentum, +2 Initiative, +3 Doubt
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 8 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 2 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Doubt, BaseValue = 3 }
                    }
                }
            },

            // Master (Depth 7-8) - Specialist 3-4x, Universal 2-3x, Doubt trade-off
            7 or 8 => new List<CardEffectFormula>
            {
                // Specialist + Initiative + trade-off: +12 Momentum, +3 Initiative, +4 Doubt
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 12 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 3 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Doubt, BaseValue = 4 }
                    }
                }
            },

            _ => new List<CardEffectFormula>()
        };
    }

    // ==================== COMMERCE (DOUBT) ====================
    // SPECIALIST: Doubt (-1 to -6) + UNIVERSAL: Momentum + TRADING: Consume Momentum

    private static List<CardEffectFormula> GetCommerceEffects(int depth)
    {
        return depth switch
        {
            // Foundation (Depth 1-2) - Specialist 2x, Universal 1x
            1 or 2 => new List<CardEffectFormula>
            {
                // Pure specialist: -1 Doubt
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Fixed,
                    TargetResource = ConversationResourceType.Doubt,
                    BaseValue = -1
                },
                // Specialist + Momentum: -1 Doubt, +1 Momentum
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Doubt, BaseValue = -1 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 1 }
                    }
                }
            },

            // Standard (Depth 3-4) - Specialist 2.5x, Universal 1.5x, Trading pattern
            3 or 4 => new List<CardEffectFormula>
            {
                // Specialist + Momentum + Trading: -2 Doubt, +2 Momentum, Consume 2 Momentum
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Doubt, BaseValue = -2 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 2 },
                        new() { FormulaType = EffectFormulaType.Trading, TargetResource = ConversationResourceType.Momentum, TradeRatio = -2, ConsumeResource = ConversationResourceType.Momentum, ConsumeAmount = 2 }
                    }
                }
            },

            // Advanced (Depth 5-6) - Specialist 3x, Universal 2x, Trading pattern
            5 or 6 => new List<CardEffectFormula>
            {
                // Specialist + Momentum + Trading: -4 Doubt, +3 Momentum, Consume 3 Momentum
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Doubt, BaseValue = -4 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 3 },
                        new() { FormulaType = EffectFormulaType.Trading, TargetResource = ConversationResourceType.Momentum, TradeRatio = -3, ConsumeResource = ConversationResourceType.Momentum, ConsumeAmount = 3 }
                    }
                }
            },

            // Master (Depth 7-8) - Specialist 3-4x (Setting), Universal 2-3x, Trading pattern
            7 or 8 => new List<CardEffectFormula>
            {
                // Specialist (Setting) + Momentum + Trading: Set Doubt to 0, +5 Momentum, Consume 4 Momentum
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Setting, TargetResource = ConversationResourceType.Doubt, SetValue = 0 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 5 },
                        new() { FormulaType = EffectFormulaType.Trading, TargetResource = ConversationResourceType.Momentum, TradeRatio = -4, ConsumeResource = ConversationResourceType.Momentum, ConsumeAmount = 4 }
                    }
                }
            },

            _ => new List<CardEffectFormula>()
        };
    }

    // ==================== CUNNING (INITIATIVE) ====================
    // SPECIALIST: Initiative (+2-10) + UNIVERSAL: Momentum + SECONDARY: Cards

    private static List<CardEffectFormula> GetCunningEffects(int depth)
    {
        return depth switch
        {
            // Foundation (Depth 1-2) - Specialist 2x, Universal 1x
            1 or 2 => new List<CardEffectFormula>
            {
                // Pure specialist: +2 Initiative
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Fixed,
                    TargetResource = ConversationResourceType.Initiative,
                    BaseValue = 2
                },
                // Specialist + Momentum: +2 Initiative, +1 Momentum
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

            // Standard (Depth 3-4) - Specialist 2.5x, Universal 1.5x, Secondary cards
            3 or 4 => new List<CardEffectFormula>
            {
                // Pure specialist: +4 Initiative
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Fixed,
                    TargetResource = ConversationResourceType.Initiative,
                    BaseValue = 4
                },
                // Specialist + Momentum: +4 Initiative, +2 Momentum
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 4 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 2 }
                    }
                },
                // Specialist + Momentum + Secondary: +4 Initiative, +2 Momentum, Draw 1 card
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 4 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 2 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Cards, BaseValue = 1 }
                    }
                }
            },

            // Advanced (Depth 5-6) - Specialist 3x, Universal 2x, Secondary cards
            5 or 6 => new List<CardEffectFormula>
            {
                // Specialist + Momentum + Secondary: +6 Initiative, +3 Momentum, Draw 2 cards
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 6 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 3 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Cards, BaseValue = 2 }
                    }
                }
            },

            // Master (Depth 7-8) - Specialist 3-4x, Universal 2-3x, Secondary cards
            7 or 8 => new List<CardEffectFormula>
            {
                // Specialist + Momentum + Secondary: +10 Initiative, +5 Momentum, Draw 3 cards
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 10 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 5 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Cards, BaseValue = 3 }
                    }
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