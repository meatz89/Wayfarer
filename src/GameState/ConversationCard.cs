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

    // Formula-based effect system (replaces old explicit effect properties)
    public CardEffectFormula EffectFormula { get; init; }

    // DELETED: ALL legacy effect properties
    // EffectInitiative, EffectMomentum, EffectDoubt, EffectDrawCards, EffectMomentumMultiplier, ScalingFormula
    // ALL effects now use EffectFormula system only

    // Card properties
    public ConnectionType TokenType { get; init; }

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

    // Statement requirement system - cards may require prior Statement cards to be playable
    public PlayerStatType? RequiredStat { get; init; } // Which stat's Statement count to check
    public int RequiredStatements { get; init; } = 0; // How many Statements of that stat are required

    // DELETED: MomentumScaling, DoubtScaling - replaced by EffectFormula

    // Token requirements for signature cards
    public IReadOnlyDictionary<string, int> TokenRequirements { get; init; } = new Dictionary<string, int>();

    // NPC-specific targeting for signature cards
    public string NpcSpecific { get; init; }


    // Get effective Initiative cost considering alternative costs
    public int GetEffectiveInitiativeCost(ConversationSession session = null)
    {
        return InitiativeCost;
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
    /// Check if Statement requirements are met for this card
    /// Returns true if no requirements or requirements are satisfied
    /// </summary>
    public bool MeetsStatementRequirements(ConversationSession session)
    {
        if (!RequiredStat.HasValue || RequiredStatements <= 0)
        {
            return true; // No requirements
        }

        int statementCount = session.GetStatementCount(RequiredStat.Value);
        return statementCount >= RequiredStatements;
    }
}