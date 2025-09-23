using System;
using System.Collections.Generic;
using Wayfarer.GameState.Enums;

public class ConversationCard
{
    // Core identity
    public string Id { get; init; }
    public string Description { get; init; }

    // Single source of truth for card type
    public CardType CardType { get; init; } = CardType.Conversation;

    // Card category defines strategic role and mechanics
    public CardCategory Category { get; init; } = CardCategory.Expression;

    // Categorical properties that define behavior through context
    public PersistenceType Persistence { get; init; } = PersistenceType.Thought;
    public SuccessEffectType SuccessType { get; init; } = SuccessEffectType.None;
    public FailureEffectType FailureType { get; init; } = FailureEffectType.None;

    // Skeleton tracking - consistent with other entities
    public bool IsSkeleton { get; init; } = false;
    public string SkeletonSource { get; init; } // What created this skeleton

    // Core mechanics
    public ConnectionType TokenType { get; init; }
    public int Focus { get; init; }
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
    public string DialogueFragment { get; init; }
    public string VerbPhrase { get; init; }

    // Level bonuses that apply at specific levels
    public IReadOnlyList<CardLevelBonus> LevelBonuses { get; init; } = new List<CardLevelBonus>();

    // Player stats system - which stat this card is bound to for XP progression
    public PlayerStatType? BoundStat { get; init; }

    // Momentum/Doubt effect scaling properties
    public ScalingType MomentumScaling { get; init; } = ScalingType.None;
    public ScalingType DoubtScaling { get; init; } = ScalingType.None;

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

    // Calculate actual momentum effect based on success type, category, difficulty, and scaling formula
    public int GetMomentumEffect(ConversationSession session, PlayerStats playerStats = null)
    {
        int baseEffect = GetBaseMomentumFromProperties();
        if (baseEffect == 0) return 0;

        // Apply scaling formula if specified
        if (MomentumScaling != ScalingType.None)
        {
            baseEffect = MomentumScaling switch
            {
                ScalingType.CardsInHand => session.Deck.HandSize,
                ScalingType.CardsInHandDivided => (int)Math.Ceiling((double)session.Deck.HandSize / 2),
                ScalingType.DoubtReduction => Math.Max(1, 8 - session.CurrentDoubt), // 8 instead of 10 for rebalanced economy
                ScalingType.DoubtHalved => (8 - session.CurrentDoubt) / 2,
                ScalingType.DoubleCurrent => session.CurrentMomentum,
                ScalingType.PatienteDivided => session.GetAvailableFocus() / 3,
                ScalingType.DoubtMultiplier => Math.Max(1, session.CurrentDoubt), // Desperation effects

                // Resource conversion effects (these indicate momentum requirements, not generation)
                ScalingType.SpendForDoubt => -2, // Spend 2 momentum to reduce doubt by 3
                ScalingType.SpendForFlow => -3, // Spend 3 momentum to gain 1 flow
                ScalingType.SpendForFlowMajor => -4, // Spend 4 momentum to gain 2 flow
                ScalingType.CardDiscard => 1, // Gain 1 momentum per discarded card (handled specially)
                ScalingType.PreventDoubt => 0, // Special effect, no momentum change
                _ => baseEffect
            };
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
                ScalingType.DoubtHalved => (10 - session.CurrentDoubt) / 2,
                ScalingType.DoubtReduction => 10 - session.CurrentDoubt,
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
    /// Uses the design mapping: Expression (Strike, Promising), Realization (Advancing, DoubleMomentum), Regulation (Soothe, Threading, Focusing, Atmospheric)
    /// </summary>
    public static CardCategory DetermineCategoryFromEffect(SuccessEffectType effectType)
    {
        return effectType switch
        {
            SuccessEffectType.Strike => CardCategory.Expression,
            SuccessEffectType.Promising => CardCategory.Expression,
            SuccessEffectType.Advancing => CardCategory.Realization,
            SuccessEffectType.DoubleMomentum => CardCategory.Realization,
            SuccessEffectType.Soothe => CardCategory.Regulation,
            SuccessEffectType.Threading => CardCategory.Regulation,
            SuccessEffectType.Focusing => CardCategory.Regulation,
            SuccessEffectType.Atmospheric => CardCategory.Regulation,
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
            CardCategory.Realization => new[] { SuccessEffectType.Advancing, SuccessEffectType.DoubleMomentum },
            CardCategory.Regulation => new[] { SuccessEffectType.Soothe, SuccessEffectType.Threading, SuccessEffectType.Focusing, SuccessEffectType.Atmospheric },
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
    /// Creates a deep clone of this conversation card
    /// Used when adding observation cards to multiple NPCs
    /// Each NPC needs their own instance to track consumption
    /// </summary>
    public ConversationCard DeepClone()
    {
        return new ConversationCard
        {
            Id = this.Id,
            Description = this.Description,
            CardType = this.CardType,
            Category = this.Category,
            Persistence = this.Persistence,
            SuccessType = this.SuccessType,
            FailureType = this.FailureType,
            IsSkeleton = this.IsSkeleton,
            SkeletonSource = this.SkeletonSource,
            TokenType = this.TokenType,
            Focus = this.Focus,
            Difficulty = this.Difficulty,
            MinimumTokensRequired = this.MinimumTokensRequired,
            RequiredTokenType = this.RequiredTokenType,
            PersonalityTypes = this.PersonalityTypes,
            MomentumThreshold = this.MomentumThreshold,
            QueuePosition = this.QueuePosition,
            InstantMomentum = this.InstantMomentum,
            RequestId = this.RequestId,
            DialogueFragment = this.DialogueFragment,
            VerbPhrase = this.VerbPhrase,
            LevelBonuses = this.LevelBonuses,
            BoundStat = this.BoundStat,
            MomentumScaling = this.MomentumScaling,
            DoubtScaling = this.DoubtScaling
        };
    }
}