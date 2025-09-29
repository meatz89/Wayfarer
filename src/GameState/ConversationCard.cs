using System;
using System.Collections.Generic;


public class ConversationCard
{
    // Core identity
    public string Id { get; init; }
    public string Title { get; init; }

    // Single source of truth for card type
    public CardType CardType { get; init; } = CardType.Conversation;

    // Card category defines strategic role and mechanics
    public CardCategory Category { get; init; } = CardCategory.Expression;

    // Categorical properties that define behavior through context
    public PersistenceType Persistence { get; init; } = PersistenceType.Statement;
    public SuccessEffectType SuccessType { get; init; } = SuccessEffectType.None;


    // New 4-Resource System Properties
    public CardDepth Depth { get; init; } = CardDepth.Depth1;
    public int InitiativeCost { get; init; } = 0;
    public ScalingFormula ScalingEffect { get; init; }

    // Parsed effects from JSON
    public int? EffectInitiative { get; init; } // Initiative gained/lost
    public int? EffectMomentum { get; init; } // Momentum gained/lost
    public int? EffectDoubt { get; init; } // Doubt gained/lost (negative = reduction)
    public int? EffectDrawCards { get; init; } // Cards to draw
    public decimal? EffectMomentumMultiplier { get; init; } // Momentum multiplier

    // Card properties
    public ConnectionType TokenType { get; init; }
    public Difficulty Difficulty { get; init; }

    // Token requirements for gated exchanges
    public int MinimumTokensRequired { get; init; } = 0;
    public ConnectionType? RequiredTokenType { get; init; }

    // Personality targeting - which NPCs can use this card
    public IReadOnlyList<string> PersonalityTypes { get; init; } = new List<string>();

    // Momentum threshold for goal cards (Letter, Promise, BurdenGoal)
    public int MomentumThreshold { get; init; } = 0;

    // Promise card specific properties
    public int QueuePosition { get; init; } = 0; // Position to force in queue (usually 1)
    public int InstantMomentum { get; init; } = 0; // Momentum gained from burning tokens
    public string RequestId { get; init; } // Links card to its parent NPCRequest

    // Display properties
    public string DialogueText { get; init; }
    public string VerbPhrase { get; init; }

    // Level bonuses that apply at specific levels
    public IReadOnlyList<CardLevelBonus> LevelBonuses { get; init; } = new List<CardLevelBonus>();

    // Player stats system - which stat this card is bound to for XP progression
    public PlayerStatType? BoundStat { get; init; }

    // Momentum/Doubt effect scaling properties
    public ScalingType MomentumScaling { get; init; } = ScalingType.None;
    public ScalingType DoubtScaling { get; init; } = ScalingType.None;

    // Token requirements for signature cards
    public IReadOnlyDictionary<string, int> TokenRequirements { get; init; } = new Dictionary<string, int>();

    // NPC-specific targeting for signature cards
    public string NpcSpecific { get; init; }

    // Get base success percentage from difficulty tier (for display only)
    public int GetBaseSuccessPercentage()
    {
        return Difficulty switch
        {
            Difficulty.VeryEasy => 85,
            Difficulty.Easy => 70,
            Difficulty.Medium => 60,
            Difficulty.Hard => 50,
            Difficulty.VeryHard => 40,
            _ => 60
        };
    }

    // Get effective Initiative cost considering alternative costs
    public int GetEffectiveInitiativeCost(ConversationSession session = null)
    {
        return InitiativeCost;
    }

    // Calculate actual momentum effect based on success type, category, difficulty, and scaling formula
    public int GetMomentumEffect(ConversationSession session, PlayerStats playerStats = null)
    {
        int baseEffect = GetBaseMomentumFromProperties();
        if (baseEffect == 0) return 0;

        // Apply new scaling formula if specified
        if (ScalingEffect != null)
        {
            baseEffect = ApplyScalingFormula(baseEffect, session);
        }
        // Apply 4-resource system scaling
        else if (MomentumScaling != ScalingType.None)
        {
            baseEffect = MomentumScaling switch
            {
                // Resource-based scaling (Initiative, Cadence, Momentum, Doubt)
                ScalingType.CurrentInitiative => session.CurrentInitiative,
                ScalingType.CurrentCadence => session.Cadence,
                ScalingType.CurrentMomentum => session.CurrentMomentum,
                ScalingType.CurrentDoubt => session.CurrentDoubt,
                ScalingType.DoubleMomentum => session.CurrentMomentum * 2,

                // Pile-based scaling (Mind, Spoken, Deck)
                ScalingType.CardsInMind => session.Deck.HandSize,
                ScalingType.CardsInSpoken => session.Deck.SpokenPileCount,
                ScalingType.CardsInDeck => session.Deck.RemainingDeckCards,

                // Resource conversion effects (momentum spending)
                ScalingType.SpendMomentumForDoubt => -2, // Spend 2 momentum to reduce doubt
                ScalingType.SpendMomentumForInitiative => -3, // Spend 3 momentum to gain initiative

                // Conditional scaling
                ScalingType.DoubtMultiplier => Math.Max(1, session.CurrentDoubt), // Desperation effects
                ScalingType.CadenceBonus => Math.Max(0, session.Cadence), // Positive cadence bonus
                ScalingType.InitiativeThreshold => session.CurrentInitiative >= 5 ? baseEffect * 2 : baseEffect,

                _ => baseEffect
            };
        }

        // Apply doubt tax on momentum gains
        if (baseEffect > 0)
        {
            baseEffect = session.GetEffectiveMomentumGain(baseEffect);
        }

        // Add stat bonus for Expression cards (Category determines if it's an Expression card)
        if (Category == CardCategory.Expression && BoundStat.HasValue && playerStats != null)
        {
            int statLevel = playerStats.GetLevel(BoundStat.Value);

            // According to desperate plea spec: Level 2 = +1, Level 3 = +2, Level 4 = +3, Level 5 = +4
            if (statLevel >= 2)
            {
                baseEffect += (statLevel - 1);
            }
        }

        return baseEffect;
    }

    // Apply new scaling formula system using 4-resource + visible piles
    private int ApplyScalingFormula(int baseEffect, ConversationSession session)
    {
        return ScalingEffect.ScalingType switch
        {
            "Initiative" => baseEffect + (int)(session.CurrentInitiative * ScalingEffect.Multiplier),
            "Cadence" => baseEffect + (int)(session.Cadence * ScalingEffect.Multiplier),
            "Momentum" => baseEffect + (int)(session.CurrentMomentum * ScalingEffect.Multiplier),
            "Doubt" => baseEffect + (int)(session.CurrentDoubt * ScalingEffect.Multiplier),
            "SpokenCards" => baseEffect + (int)(session.Deck.SpokenPileCount * ScalingEffect.Multiplier),
            "MindCards" => baseEffect + (int)(session.Deck.HandSize * ScalingEffect.Multiplier),
            "DeckCards" => baseEffect + (int)(session.Deck.RemainingDeckCards * ScalingEffect.Multiplier),
            _ => baseEffect
        };
    }

    // Calculate actual doubt effect based on success type, category, difficulty, and scaling formula
    public int GetDoubtEffect(ConversationSession session)
    {
        int baseEffect = GetBaseDoubtFromProperties();
        if (baseEffect == 0) return 0;

        // Apply scaling formula if specified
        if (DoubtScaling != ScalingType.None)
        {
            return DoubtScaling switch
            {
                ScalingType.CurrentDoubt => 10 - session.CurrentDoubt,
                _ => baseEffect
            };
        }

        return baseEffect;
    }

    // Calculate base momentum from success type and difficulty only
    public int GetBaseMomentumFromProperties()
    {
        return SuccessType switch
        {
            SuccessEffectType.Strike => GetDifficultyMagnitude(),
            SuccessEffectType.DoubleMomentum => 0, // Special case - doubles existing momentum
            _ => 0
        };
    }

    // Calculate base doubt from success type and difficulty only
    private int GetBaseDoubtFromProperties()
    {
        return SuccessType switch
        {
            SuccessEffectType.Soothe => -GetDifficultyMagnitude(), // Negative = reduces doubt
            _ => 0
        };
    }

    // Get magnitude based on difficulty
    private int GetDifficultyMagnitude()
    {
        return Difficulty switch
        {
            Difficulty.VeryEasy => 1,
            Difficulty.Easy => 2,
            Difficulty.Medium => 3,
            Difficulty.Hard => 4,
            Difficulty.VeryHard => 5,
            _ => 2
        };
    }

    /// <summary>
    /// Determines card category based on success effect type
    /// Uses the refined mapping: Expression (Strike, Promising), Realization (DoubleMomentum), Regulation (Soothe, Threading)
    /// </summary>
    public static CardCategory DetermineCategoryFromEffect(SuccessEffectType effectType)
    {
        return effectType switch
        {
            SuccessEffectType.Strike => CardCategory.Expression,
            SuccessEffectType.Promising => CardCategory.Expression,
            SuccessEffectType.DoubleMomentum => CardCategory.Realization,
            SuccessEffectType.Soothe => CardCategory.Regulation,
            SuccessEffectType.Threading => CardCategory.Regulation,
            _ => CardCategory.Expression // Default fallback
        };
    }

    /// <summary>
    /// Gets all effect types that belong to a specific category
    /// </summary>
    public static IReadOnlyList<SuccessEffectType> GetEffectTypesForCategory(CardCategory category)
    {
        return category switch
        {
            CardCategory.Expression => new[] { SuccessEffectType.Strike, SuccessEffectType.Promising },
            CardCategory.Realization => new[] { SuccessEffectType.DoubleMomentum },
            CardCategory.Regulation => new[] { SuccessEffectType.Soothe, SuccessEffectType.Threading },
            _ => new[] { SuccessEffectType.Strike } // Default fallback
        };
    }

    /// <summary>
    /// Determines if this card matches its assigned category based on effect type
    /// Used for validation and consistency checking
    /// </summary>
    public bool IsCategoryConsistent()
    {
        return DetermineCategoryFromEffect(SuccessType) == Category;
    }

    /// <summary>
    /// Get the strategic tier based on depth (Foundation, Standard, Decisive)
    /// </summary>
    public string GetStrategicTier()
    {
        return (int)Depth switch
        {
            <= 3 => "Foundation",
            <= 6 => "Standard",
            _ => "Decisive"
        };
    }

    /// <summary>
    /// Check if this card can be accessed by a player with given stat levels
    /// </summary>
    public bool CanAccessWithStats(PlayerStats playerStats)
    {
        if (!BoundStat.HasValue || playerStats == null) return true;
        return playerStats.GetLevel(BoundStat.Value) >= (int)Depth;
    }

    /// <summary>
    /// Check if token requirements are met for this signature card
    /// </summary>
    public bool CanAccessWithTokens(Dictionary<string, int> availableTokens)
    {
        if (TokenRequirements == null || !TokenRequirements.Any()) return true;

        foreach (var requirement in TokenRequirements)
        {
            if (!availableTokens.TryGetValue(requirement.Key, out int available) || available < requirement.Value)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Get the effective Initiative effect from new JSON structure (overrides legacy calculation)
    /// </summary>
    public int GetInitiativeEffect()
    {
        return EffectInitiative ?? 0;
    }

    /// <summary>
    /// Get the effective Momentum effect from new JSON structure (overrides legacy calculation)
    /// </summary>
    public int GetMomentumEffectFromJSON()
    {
        return EffectMomentum ?? 0;
    }

    /// <summary>
    /// Get the effective Doubt effect from new JSON structure (overrides legacy calculation)
    /// </summary>
    public int GetDoubtEffectFromJSON()
    {
        return EffectDoubt ?? 0;
    }

    /// <summary>
    /// Get the effective DrawCards effect from new JSON structure
    /// </summary>
    public int GetDrawCardsEffect()
    {
        return EffectDrawCards ?? 0;
    }
}