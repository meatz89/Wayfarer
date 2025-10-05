using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Comprehensive catalog of all card effect formulas.
/// Organized by stat and depth with multiple variants per combination.
///
/// REFINED STAT-TO-RESOURCE MAPPING (UNDERSTANDING + DELIVERY SYSTEM):
/// - Insight → Cards (PRIMARY: 2/3/4/6 draw) + Understanding (SECONDARY: +1/1/2/3)
/// - Rapport → Understanding (PRIMARY: +2/4/6/10) + Initiative/Momentum (SECONDARY)
/// - Authority → Momentum (PRIMARY: +2/5/8/12) + Understanding (SECONDARY)
/// - Diplomacy → -Doubt (PRIMARY: reduce/prevent) + Understanding (SECONDARY), Consume Momentum
/// - Cunning → Initiative (PRIMARY) + Understanding/Momentum (SECONDARY), +Cadence at Advanced/Master
///
/// DELIVERY SYSTEM (affects Cadence via game rules, not card effects):
/// - Standard: +1 Cadence (most cards)
/// - Commanding: +2 Cadence (ALL Authority cards)
/// - Measured: +0 Cadence (questions, careful statements)
/// - Yielding: -1 Cadence (deferential, rare)
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
            PlayerStatType.Diplomacy => GetDiplomacyEffects(depth),
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
    /// Get effect formula from ONLY categorical properties (no JSON variant needed).
    /// Effect determined by: ConversationalMove + BoundStat + Depth + Card ID
    ///
    /// CRITICAL: Specialization through card distribution:
    /// - Card ID hash determines if Foundation card gets specialty or Momentum effect
    /// - ~50% of Foundation cards per stat get Momentum (universal access)
    /// - ~50% get specialty resource (Understanding/Cards/-Doubt)
    /// - Authority always gets Momentum (specialist)
    ///
    /// RULES:
    /// - Remark ALWAYS generates Momentum (pressing conversational points)
    /// - Observation generates EITHER specialty resource OR Momentum based on card ID
    /// - Argument uses complex compound effects
    /// </summary>
    public static CardEffectFormula GetEffectFromCategoricalProperties(ConversationalMove move, PlayerStatType stat, int depth, string cardId)
    {
        // CONVERSATIONAL MOVE determines the effect category
        return move switch
        {
            ConversationalMove.Remark => GetRemarkEffect(stat, depth),
            ConversationalMove.Observation => GetObservationEffect(stat, depth, cardId),
            ConversationalMove.Argument => GetArgumentEffect(stat, depth),
            _ => throw new InvalidOperationException($"Unknown conversational move: {move}")
        };
    }

    /// <summary>
    /// Remark effects - ALWAYS Momentum generation (pressing conversational points)
    /// Authority specializes in Momentum through Remarks (+2 at Foundation)
    /// </summary>
    private static CardEffectFormula GetRemarkEffect(PlayerStatType stat, int depth)
    {
        // Remarks ALWAYS build Momentum regardless of stat
        // This is the "pressing points forward" conversational move
        return new CardEffectFormula
        {
            FormulaType = EffectFormulaType.Fixed,
            TargetResource = ConversationResourceType.Momentum,
            BaseValue = depth switch
            {
                1 or 2 => 2,  // Foundation: Authority specialist bonus
                3 or 4 => 5,  // Standard
                5 or 6 => 8,  // Advanced
                _ => 12       // Master
            }
        };
    }

    /// <summary>
    /// Observation effects - Stat specialty resources (cards, understanding, -doubt)
    ///
    /// CRITICAL: Supports "Specialist with Universal Access" model through card ID hash:
    /// - Certain card IDs generate Momentum +1 (universal access for non-specialists)
    /// - Others generate specialty resource (+2 for specialists)
    ///
    /// Foundation cards (depth 1-2) ONLY have single effects (no compounds).
    /// Specialization achieved through:
    /// 1. Effect Bonus: Specialists get +2, non-specialists get +1 (2x multiplier)
    /// 2. Card Distribution: ~50% specialty, ~50% Momentum (determined by card ID hash)
    /// </summary>
    private static CardEffectFormula GetObservationEffect(PlayerStatType stat, int depth, string cardId)
    {
        // For Foundation tier (depth 1-2), use card ID to determine if Momentum variant
        if (depth <= 2)
        {
            // Deterministic hash: specific card IDs generate Momentum for universal access
            // This achieves ~50% distribution between specialty and Momentum
            if (ShouldGenerateMomentum(cardId))
            {
                return new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Fixed,
                    TargetResource = ConversationResourceType.Momentum,
                    BaseValue = 1  // Non-specialists get +1 Momentum (Authority gets +2 as specialist)
                };
            }
        }

        // Otherwise use the stat's specialty effect (specialty variant or higher depths)
        var variants = GetEffectVariants(stat, depth);
        return variants.FirstOrDefault() ?? throw new InvalidOperationException($"No effect found for {stat} depth {depth}");
    }

    /// <summary>
    /// Deterministic function to decide if a Foundation card should generate Momentum.
    /// Based on card ID suffix/pattern to achieve ~50% distribution.
    /// </summary>
    private static bool ShouldGenerateMomentum(string cardId)
    {
        // Cards with specific suffixes generate Momentum (universal access)
        // This is deterministic based on card naming convention
        return cardId.Contains("encouragement") ||
               cardId.Contains("response") ||
               cardId.Contains("question") ||
               cardId.Contains("detail") ||
               cardId.Contains("alternative") ||
               cardId.Contains("maneuver");
    }

    /// <summary>
    /// Argument effects - Complex compound effects for developed points
    /// </summary>
    private static CardEffectFormula GetArgumentEffect(PlayerStatType stat, int depth)
    {
        // Arguments MUST use compound effects - no fallbacks
        var variants = GetEffectVariants(stat, depth);

        var compoundVariant = variants.FirstOrDefault(v => v.FormulaType == EffectFormulaType.Compound);

        if (compoundVariant == null)
        {
            throw new InvalidOperationException(
                $"ARGUMENT CARD REQUIRES COMPOUND EFFECT: No compound effect found for {stat} depth {depth}. " +
                $"Arguments are complex developed points and MUST have compound effects.");
        }

        return compoundVariant;
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

    // ==================== INSIGHT (CARDS + UNDERSTANDING) ====================
    // PRIMARY: Cards (2/3/4/6 draw)
    // SECONDARY: Understanding (+1/1/2/3)
    // STEAMWORLD PATTERN: Foundation includes Type A (Strike) variants with Momentum+Initiative

    private static List<CardEffectFormula> GetInsightEffects(int depth)
    {
        return depth switch
        {
            // Foundation (Depth 1-2) - Singular effect only (Initiative comes from card property)
            1 or 2 => new List<CardEffectFormula>
            {
                // Insight Foundation: Draw 2 cards (SINGULAR EFFECT)
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Fixed,
                    TargetResource = ConversationResourceType.Cards,
                    BaseValue = 2
                }
            },

            // Standard (Depth 3-4) - Cards 3x, Understanding +1
            3 => new List<CardEffectFormula>
            {
                // Pure specialist: Draw 3 cards
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Fixed,
                    TargetResource = ConversationResourceType.Cards,
                    BaseValue = 3
                },
                // Specialist + Understanding: Draw 3 cards, +1 Understanding
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Cards, BaseValue = 3 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Understanding, BaseValue = 1 }
                    }
                }
            },
            4 => new List<CardEffectFormula>
            {
                // Pure specialist: Draw 3 cards
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Fixed,
                    TargetResource = ConversationResourceType.Cards,
                    BaseValue = 3
                },
                // Specialist + Understanding: Draw 3 cards, +1 Understanding
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Cards, BaseValue = 3 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Understanding, BaseValue = 1 }
                    }
                }
            },

            // Advanced (Depth 5-6) - Cards 4x, Understanding +2
            5 => new List<CardEffectFormula>
            {
                // Specialist + Understanding: Draw 4 cards, +2 Understanding
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Cards, BaseValue = 4 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Understanding, BaseValue = 2 }
                    }
                }
            },
            6 => new List<CardEffectFormula>
            {
                // Specialist + Understanding: Draw 4 cards, +2 Understanding
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Cards, BaseValue = 4 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Understanding, BaseValue = 2 }
                    }
                }
            },

            // Master (Depth 7-8) - Cards 6x, Understanding +3
            7 or 8 => new List<CardEffectFormula>
            {
                // Specialist + Understanding: Draw 6 cards, +3 Understanding
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Cards, BaseValue = 6 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Understanding, BaseValue = 3 }
                    }
                }
            },

            _ => new List<CardEffectFormula>()
        };
    }

    // ==================== RAPPORT (UNDERSTANDING) ====================
    // PRIMARY: Understanding (+2/4/6/10)
    // SECONDARY: Momentum (NOT Initiative - Initiative comes from ConversationalMove only!)
    // CRITICAL: Arguments (depth 3+) COST Initiative, they never generate it

    private static List<CardEffectFormula> GetRapportEffects(int depth)
    {
        return depth switch
        {
            // Foundation (Depth 1-2) - Singular effect only
            // Rapport Observations: +2 Understanding (Initiative generation comes from ConversationalMove.Observation)
            1 or 2 => new List<CardEffectFormula>
            {
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Fixed,
                    TargetResource = ConversationResourceType.Understanding,
                    BaseValue = 2
                }
            },

            // Standard (Depth 3-4) - Compound: Understanding +4, Momentum +1
            // Rapport Arguments: Understanding + Momentum (NO Initiative - Arguments COST Initiative!)
            3 or 4 => new List<CardEffectFormula>
            {
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Understanding, BaseValue = 4 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 2 }
                    }
                }
            },

            // Advanced (Depth 5-6) - Compound: Understanding +6, Momentum +3
            5 or 6 => new List<CardEffectFormula>
            {
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Understanding, BaseValue = 6 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 4 }
                    }
                }
            },

            // Master (Depth 7-8) - Compound: Understanding +10, Momentum +5
            7 or 8 => new List<CardEffectFormula>
            {
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Understanding, BaseValue = 10 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 6 }
                    }
                }
            },

            _ => new List<CardEffectFormula>()
        };
    }

    // ==================== AUTHORITY (MOMENTUM + UNDERSTANDING) ====================
    // PRIMARY: Momentum (+2/5/8/12)
    // SECONDARY: Understanding, +Doubt trade-off
    // CRITICAL: Authority Remarks generate Momentum PLUS Initiative from ConversationalMove.Remark
    // Arguments generate Momentum + Understanding but NO Initiative (Arguments COST Initiative!)

    private static List<CardEffectFormula> GetAuthorityEffects(int depth)
    {
        return depth switch
        {
            // Foundation (Depth 1-2) - Singular Momentum effect
            // Authority Remarks: +2 Momentum (Initiative generation comes from ConversationalMove.Remark)
            1 or 2 => new List<CardEffectFormula>
            {
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Fixed,
                    TargetResource = ConversationResourceType.Momentum,
                    BaseValue = 2
                }
            },

            // Standard (Depth 3-4) - Compound: Momentum +5, Understanding +2
            // Authority Arguments: Momentum + Understanding (NO Initiative - Arguments COST Initiative!)
            3 or 4 => new List<CardEffectFormula>
            {
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 5 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Understanding, BaseValue = 2 }
                    }
                }
            },

            // Advanced (Depth 5-6) - Compound: Momentum +8, Understanding +3
            5 or 6 => new List<CardEffectFormula>
            {
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 8 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Understanding, BaseValue = 3 }
                    }
                }
            },

            // Master (Depth 7-8) - Compound: Momentum +12, Understanding +4
            7 or 8 => new List<CardEffectFormula>
            {
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 12 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Understanding, BaseValue = 4 }
                    }
                }
            },

            _ => new List<CardEffectFormula>()
        };
    }

    // ==================== DIPLOMACY (DOUBT REDUCTION + UNDERSTANDING) ====================
    // PRIMARY: -Doubt (reduce/prevent)
    // SECONDARY: Understanding
    // TRADING: Consume Momentum
    // STEAMWORLD PATTERN: Foundation includes Strike variants with Momentum+Initiative

    private static List<CardEffectFormula> GetDiplomacyEffects(int depth)
    {
        return depth switch
        {
            // Foundation (Depth 1-2) - Singular effect only (Initiative comes from card property)
            1 or 2 => new List<CardEffectFormula>
            {
                // Diplomacy Foundation: -1 Doubt (SINGULAR EFFECT)
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Fixed,
                    TargetResource = ConversationResourceType.Doubt,
                    BaseValue = -1
                }
            },

            // Standard (Depth 3-4) - Doubt -2, Understanding +2, Trading pattern
            3 or 4 => new List<CardEffectFormula>
            {
                // Specialist + Understanding + Trading: -2 Doubt, +2 Understanding, Consume 2 Momentum
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Doubt, BaseValue = -2 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Understanding, BaseValue = 2 },
                        new() { FormulaType = EffectFormulaType.Trading, TargetResource = ConversationResourceType.Momentum, TradeRatio = -2, ConsumeResource = ConversationResourceType.Momentum, ConsumeAmount = 2 }
                    }
                }
            },

            // Advanced (Depth 5-6) - Doubt -4, Understanding +3, Trading pattern
            5 or 6 => new List<CardEffectFormula>
            {
                // Specialist + Understanding + Trading: -4 Doubt, +3 Understanding, Consume 3 Momentum
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Doubt, BaseValue = -4 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Understanding, BaseValue = 3 },
                        new() { FormulaType = EffectFormulaType.Trading, TargetResource = ConversationResourceType.Momentum, TradeRatio = -3, ConsumeResource = ConversationResourceType.Momentum, ConsumeAmount = 3 }
                    }
                }
            },

            // Master (Depth 7-8) - Set Doubt to 0, Understanding +4, Trading pattern
            7 or 8 => new List<CardEffectFormula>
            {
                // Specialist (Setting) + Understanding + Trading: Set Doubt to 0, +4 Understanding, Consume 4 Momentum
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Setting, TargetResource = ConversationResourceType.Doubt, SetValue = 0 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Understanding, BaseValue = 4 },
                        new() { FormulaType = EffectFormulaType.Trading, TargetResource = ConversationResourceType.Momentum, TradeRatio = -4, ConsumeResource = ConversationResourceType.Momentum, ConsumeAmount = 4 }
                    }
                }
            },

            _ => new List<CardEffectFormula>()
        };
    }

    // ==================== CUNNING (INITIATIVE SPECIALIST) ====================
    // CRITICAL: Cunning's Initiative comes from ConversationalMove (Observations generate 3 Initiative!)
    // PRIMARY EFFECT: Understanding, Momentum, Cadence (NO Initiative in effects!)
    // Cunning Arguments provide tactical positioning effects: Understanding + Momentum + Cadence
    // Foundation Observations have NO card effect - pure Initiative generation from ConversationalMove

    private static List<CardEffectFormula> GetCunningEffects(int depth)
    {
        return depth switch
        {
            // Foundation (Depth 1-2) - NO card effect (pure Initiative from ConversationalMove.Observation)
            // Cunning Observations generate 3 Initiative from GetInitiativeGeneration() - that's their ONLY purpose
            // Use Understanding +0 as a no-op placeholder since parser requires a formula
            1 or 2 => new List<CardEffectFormula>
            {
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Fixed,
                    TargetResource = ConversationResourceType.Understanding,
                    BaseValue = 0  // Cunning Foundation has NO effect - Initiative comes from ConversationalMove only
                }
            },

            // Standard (Depth 3-4) - Compound: Understanding +3, Momentum +3, Cadence +1
            // Cunning Arguments: Tactical positioning (NO Initiative - Arguments COST Initiative!)
            3 or 4 => new List<CardEffectFormula>
            {
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Understanding, BaseValue = 3 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 3 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Cadence, BaseValue = 1 }
                    }
                }
            },

            // Advanced (Depth 5-6) - Compound: Understanding +4, Momentum +4, Cadence +2
            5 or 6 => new List<CardEffectFormula>
            {
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Understanding, BaseValue = 4 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 4 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Cadence, BaseValue = 2 }
                    }
                }
            },

            // Master (Depth 7-8) - Compound: Understanding +6, Momentum +6, Cadence +3
            7 or 8 => new List<CardEffectFormula>
            {
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Understanding, BaseValue = 6 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 6 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Cadence, BaseValue = 3 }
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

        // Diplomacy: Expensive (safety costs)
        if (stat == PlayerStatType.Diplomacy)
        {
            return depth switch
            {
                1 or 2 => 1,  // Foundation costs 1 Initiative
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
            PlayerStatType.Diplomacy => 65,  // Negotiation tools repeatable
            PlayerStatType.Insight => 60,   // Analysis repeats, insights don't
            PlayerStatType.Authority => 50, // Commands repeat, declarations don't
            _ => 60
        };
    }

    // ==================== STRATEGIC RESOURCE COSTS ====================
    // Parallel to effect derivation - costs derived from categorical properties
    // NO Tags, NO string matching, NO hardcoding

    /// <summary>
    /// Get stamina cost from categorical properties
    /// ExertionLevel is primary driver, ConversationalMove/Depth provide modifiers
    /// PARSER-BASED ARCHITECTURE: Costs calculated once at parse time
    /// Social cards represent mental/emotional exertion (stress, concentration)
    /// </summary>
    public static int GetStaminaCost(ConversationalMove move, int depth, ExertionLevel exertion)
    {
        int baseCost = exertion switch
        {
            ExertionLevel.Minimal => 0,
            ExertionLevel.Light => 1,
            ExertionLevel.Moderate => 2,
            ExertionLevel.Heavy => 3,
            ExertionLevel.Extreme => 5,
            _ => 1
        };

        // ConversationalMove modifier - Arguments are mentally taxing
        int moveModifier = move switch
        {
            ConversationalMove.Remark => 0,        // Standard conversational flow
            ConversationalMove.Observation => -1,  // Observing conserves energy
            ConversationalMove.Argument => 1,      // Arguments are exhausting
            _ => 0
        };

        // Depth scaling - deeper conversations require more concentration
        int depthScaling = depth / 3;

        return Math.Max(0, baseCost + moveModifier + depthScaling);
    }

    /// <summary>
    /// Get health cost from categorical properties
    /// Direct health costs are RARE in Social system - most health loss from Doubt threshold
    /// Only high-stress social situations (Argument + Dangerous) cause direct health loss
    /// </summary>
    public static int GetHealthCost(ConversationalMove move, RiskLevel risk, int depth)
    {
        // High-stress arguments in dangerous social situations cause health loss (emotional toll)
        if (move == ConversationalMove.Argument && risk == RiskLevel.Dangerous)
        {
            return 1 + (depth / 4); // Scales slowly with depth
        }

        // Very high social risk at high depth
        if (risk == RiskLevel.Dangerous && depth >= 6)
        {
            return 1;
        }

        return 0; // Default: no direct health cost
    }

    /// <summary>
    /// Get coin cost from categorical properties
    /// Social cards rarely cost coins directly (no bribes in conversation mechanics)
    /// Most coin costs come from separate bribe/payment systems
    /// </summary>
    public static int GetCoinCost(ConversationalMove move, int depth)
    {
        // Social cards don't typically cost coins
        // Bribes/payments are separate mechanics, not card costs
        return 0;
    }
}