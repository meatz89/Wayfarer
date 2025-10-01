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
    /// Get a specific effect variant by enum.
    /// Type-safe variant selection for card effects.
    /// </summary>
    public static CardEffectFormula GetEffectByVariant(PlayerStatType stat, int depth, CardEffectVariant variant)
    {
        var variants = GetEffectVariants(stat, depth);

        // Foundation tier (depth 1-2) uses Steamworld-inspired Strike/Setup/Specialist pattern
        if (depth <= 2)
        {
            return variant switch
            {
                CardEffectVariant.Strike => variants.ElementAtOrDefault(1) ?? variants.FirstOrDefault(),     // Type A - Momentum+Initiative
                CardEffectVariant.Setup => variants.ElementAtOrDefault(2) ?? variants.FirstOrDefault(),      // Type B - Initiative heavy
                CardEffectVariant.Specialist => variants.ElementAtOrDefault(0) ?? variants.FirstOrDefault(), // Type C - Stat specialty
                CardEffectVariant.Base => variants.ElementAtOrDefault(1) ?? variants.FirstOrDefault(),       // Default to Strike for Foundation
                CardEffectVariant.Signature => variants.LastOrDefault() ?? variants.FirstOrDefault(),        // Signature if exists
                _ => variants.FirstOrDefault()
            };
        }

        // Higher tiers: Base = first variant, Signature = last variant, Advanced = middle
        return variant switch
        {
            CardEffectVariant.Signature => variants.LastOrDefault() ?? variants.FirstOrDefault(),
            CardEffectVariant.Advanced => variants.ElementAtOrDefault(1) ?? variants.FirstOrDefault(),
            _ => variants.FirstOrDefault()
        };
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
            // Foundation (Depth 1-2) - Steamworld-inspired Strike/Setup/Specialist pattern
            1 => new List<CardEffectFormula>
            {
                // Type C - Specialist: Draw 2 cards (pure card advantage)
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Fixed,
                    TargetResource = ConversationResourceType.Cards,
                    BaseValue = 2
                },
                // Type A - Strike: +2 Momentum, +1 Initiative, Draw 1 card (productive + enabling)
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 2 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 1 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Cards, BaseValue = 1 }
                    }
                },
                // Type B - Setup: +1 Momentum, +2 Initiative, Draw 1 card (enabling focused)
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 1 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 2 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Cards, BaseValue = 1 }
                    }
                }
            },
            2 => new List<CardEffectFormula>
            {
                // Type C - Specialist: Draw 2 cards (pure card advantage)
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Fixed,
                    TargetResource = ConversationResourceType.Cards,
                    BaseValue = 2
                },
                // Type A - Strike: +2 Momentum, +1 Initiative, Draw 1 card (productive + enabling)
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 2 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 1 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Cards, BaseValue = 1 }
                    }
                },
                // Type B - Setup: +1 Momentum, +2 Initiative, Draw 1 card (enabling focused)
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 1 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 2 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Cards, BaseValue = 1 }
                    }
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
    // SECONDARY: Initiative, Momentum
    // STEAMWORLD PATTERN: Foundation includes Type A (Strike) variants with Momentum+Initiative

    private static List<CardEffectFormula> GetRapportEffects(int depth)
    {
        return depth switch
        {
            // Foundation (Depth 1-2) - Steamworld-inspired Strike/Setup/Specialist pattern
            1 or 2 => new List<CardEffectFormula>
            {
                // Type C - Specialist: +2 Understanding (pure depth building)
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Fixed,
                    TargetResource = ConversationResourceType.Understanding,
                    BaseValue = 2
                },
                // Type A - Strike: +2 Momentum, +1 Initiative, +1 Understanding (productive + enabling)
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 2 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 1 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Understanding, BaseValue = 1 }
                    }
                },
                // Type B - Setup: +1 Momentum, +2 Initiative, +1 Understanding (enabling focused)
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 1 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 2 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Understanding, BaseValue = 1 }
                    }
                }
            },

            // Standard (Depth 3-4) - Understanding +4, Initiative +1, Momentum +1
            3 or 4 => new List<CardEffectFormula>
            {
                // Pure specialist: +4 Understanding
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Fixed,
                    TargetResource = ConversationResourceType.Understanding,
                    BaseValue = 4
                },
                // Specialist + both secondaries: +4 Understanding, +1 Initiative, +1 Momentum
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Understanding, BaseValue = 4 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 1 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 1 }
                    }
                }
            },

            // Advanced (Depth 5-6) - Understanding +6, Initiative +2, Momentum +2
            5 or 6 => new List<CardEffectFormula>
            {
                // Specialist + both secondaries: +6 Understanding, +2 Initiative, +2 Momentum
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Understanding, BaseValue = 6 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 2 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 2 }
                    }
                }
            },

            // Master (Depth 7-8) - Understanding +10, Initiative +3, Momentum +3
            7 or 8 => new List<CardEffectFormula>
            {
                // Specialist + both secondaries: +10 Understanding, +3 Initiative, +3 Momentum
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Understanding, BaseValue = 10 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 3 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 3 }
                    }
                }
            },

            _ => new List<CardEffectFormula>()
        };
    }

    // ==================== AUTHORITY (MOMENTUM + UNDERSTANDING) ====================
    // PRIMARY: Momentum (+2/5/8/12)
    // SECONDARY: Understanding, +Doubt trade-off
    // STEAMWORLD PATTERN: Authority is naturally Strike-focused (already generates Momentum)

    private static List<CardEffectFormula> GetAuthorityEffects(int depth)
    {
        return depth switch
        {
            // Foundation (Depth 1-2) - Steamworld-inspired Strike/Setup/Specialist pattern
            1 or 2 => new List<CardEffectFormula>
            {
                // Type C - Specialist: +3 Momentum (pure commanding power)
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Fixed,
                    TargetResource = ConversationResourceType.Momentum,
                    BaseValue = 3
                },
                // Type A - Strike: +2 Momentum, +1 Initiative (balanced productive + enabling)
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 2 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 1 }
                    }
                },
                // Type B - Setup: +1 Momentum, +2 Initiative (enabling with some progress)
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 1 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 2 }
                    }
                }
            },

            // Standard (Depth 3-4) - Momentum +5, Understanding +1, Initiative +1
            3 or 4 => new List<CardEffectFormula>
            {
                // Specialist + secondaries: +5 Momentum, +1 Understanding, +1 Initiative
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 5 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Understanding, BaseValue = 1 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 1 }
                    }
                }
            },

            // Advanced (Depth 5-6) - Momentum +8, Understanding +1, Initiative +2
            5 or 6 => new List<CardEffectFormula>
            {
                // Specialist + secondaries: +8 Momentum, +1 Understanding, +2 Initiative
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 8 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Understanding, BaseValue = 1 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 2 }
                    }
                }
            },

            // Master (Depth 7-8) - Momentum +12, Understanding +2, Initiative +3
            7 or 8 => new List<CardEffectFormula>
            {
                // Specialist + secondaries: +12 Momentum, +2 Understanding, +3 Initiative
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 12 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Understanding, BaseValue = 2 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 3 }
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
            // Foundation (Depth 1-2) - Steamworld-inspired Strike/Setup/Specialist pattern
            1 or 2 => new List<CardEffectFormula>
            {
                // Type C - Specialist: -1 Doubt (pure crisis management)
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Fixed,
                    TargetResource = ConversationResourceType.Doubt,
                    BaseValue = -1
                },
                // Type A - Strike: +2 Momentum, +1 Initiative, -1 Doubt (productive + enabling + safe)
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 2 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 1 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Doubt, BaseValue = -1 }
                    }
                },
                // Type B - Setup: +1 Momentum, +2 Initiative (enabling with minimal progress)
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 1 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 2 }
                    }
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

    // ==================== CUNNING (INITIATIVE + UNDERSTANDING) ====================
    // PRIMARY: Initiative (+2-10)
    // SECONDARY: Understanding, Momentum
    // SPECIAL: +Cadence at Advanced/Master tiers
    // STEAMWORLD PATTERN: Cunning is naturally Setup-focused (generates Initiative)

    private static List<CardEffectFormula> GetCunningEffects(int depth)
    {
        return depth switch
        {
            // Foundation (Depth 1-2) - Steamworld-inspired Strike/Setup/Specialist pattern
            1 or 2 => new List<CardEffectFormula>
            {
                // Type C - Specialist: +3 Initiative (pure tactical setup)
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Fixed,
                    TargetResource = ConversationResourceType.Initiative,
                    BaseValue = 3
                },
                // Type A - Strike: +2 Momentum, +1 Initiative (productive + enabling)
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 2 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 1 }
                    }
                },
                // Type B - Setup: +1 Momentum, +2 Initiative (enabling with minimal progress)
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 1 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 2 }
                    }
                }
            },

            // Standard (Depth 3-4) - Initiative +4, Understanding +2
            3 or 4 => new List<CardEffectFormula>
            {
                // Pure specialist: +4 Initiative
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Fixed,
                    TargetResource = ConversationResourceType.Initiative,
                    BaseValue = 4
                },
                // Specialist + Understanding + Momentum: +4 Initiative, +2 Understanding, +1 Momentum
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 4 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Understanding, BaseValue = 2 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 1 }
                    }
                }
            },

            // Advanced (Depth 5-6) - Initiative +6, Understanding +3, +Cadence SPECIAL
            5 or 6 => new List<CardEffectFormula>
            {
                // Specialist + Understanding + Momentum + Cadence: +6 Initiative, +3 Understanding, +2 Momentum, +1 Cadence
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 6 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Understanding, BaseValue = 3 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 2 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Cadence, BaseValue = 1 }
                    }
                }
            },

            // Master (Depth 7-8) - Initiative +10, Understanding +4, +Cadence SPECIAL
            7 or 8 => new List<CardEffectFormula>
            {
                // Specialist + Understanding + Momentum + Cadence: +10 Initiative, +4 Understanding, +3 Momentum, +2 Cadence
                new CardEffectFormula
                {
                    FormulaType = EffectFormulaType.Compound,
                    CompoundEffects = new List<CardEffectFormula>
                    {
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 10 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Understanding, BaseValue = 4 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 3 },
                        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Cadence, BaseValue = 2 }
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
}