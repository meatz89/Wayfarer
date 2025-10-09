using System;
using System.Collections.Generic;

public class SocialCard
{
    // Core identity
    public string Id { get; init; }
    public string Title { get; init; }

    // Categorical properties that define behavior through context
    public PersistenceType Persistence { get; init; } = PersistenceType.Statement;
    public SuccessEffectType SuccessType { get; init; } = SuccessEffectType.None;
    public ConversationalMove? Move { get; init; } // Null for Letter/Promise/Burden cards (not part of conversation mechanics)


    // New 5-Resource System Properties
    public CardDepth Depth { get; init; } = CardDepth.Depth1;
    public int InitiativeCost { get; init; } = 0;
    public DeliveryType Delivery { get; init; } = DeliveryType.Standard; // NEW: How this card affects Cadence when spoken

    // Universal card properties (apply across ALL tactical systems - Social, Mental, Physical)
    public RiskLevel RiskLevel { get; init; } = RiskLevel.Cautious;
    public Visibility Visibility { get; init; } = Visibility.Moderate;
    public ExertionLevel ExertionLevel { get; init; } = ExertionLevel.Light;
    public MethodType MethodType { get; init; } = MethodType.Direct;
    public EquipmentCategory EquipmentCategory { get; init; } = EquipmentCategory.None;

    // Strategic resource costs (calculated at parse time from categorical properties via CardEffectCatalog)
    public int StaminaCost { get; init; } = 0;
    public int DirectHealthCost { get; init; } = 0;
    public int CoinCost { get; init; } = 0;

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
    public string RequestId { get; init; } // DEPRECATED: Legacy property, no longer used (Goals replaced NPCRequests)

    // Display properties
    public string DialogueText { get; init; }
    public string VerbPhrase { get; init; }

    // Player stats system - which stat this card is bound to for XP progression
    public PlayerStatType? BoundStat { get; init; }

    // Statement requirement system - cards may require prior Statement cards to be playable
    public PlayerStatType? RequiredStat { get; init; } // Which stat's Statement count to check
    public int RequiredStatements { get; init; } = 0; // How many Statements of that stat are required

    // Special traits that modify card behavior
    public IReadOnlyList<CardTrait> Traits { get; init; } = new List<CardTrait>();

    // DELETED: MomentumScaling, DoubtScaling - replaced by EffectFormula

    // Token requirements for signature cards
    public IReadOnlyDictionary<string, int> TokenRequirements { get; init; } = new Dictionary<string, int>();

    // NPC-specific targeting for signature cards
    public string NpcSpecific { get; init; }

    // V2 Investigation System - Knowledge gained when card is played
    public IReadOnlyList<string> KnowledgeGranted { get; init; } = new List<string>();
    public IReadOnlyList<string> SecretsGranted { get; init; } = new List<string>();

    // Get effective Initiative cost considering alternative costs
    public int GetEffectiveInitiativeCost(SocialSession session)
    {
        return InitiativeCost;
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
    /// Get Initiative generation based on conversational move type.
    /// Remarks and Observations (simple conversation moves) ALWAYS generate +1 Initiative.
    /// Arguments (complex developed points) cost Initiative instead of generating it (0 generation).
    /// Cunning specializes in Initiative by having MORE Observation/Remark cards, not higher values.
    /// </summary>
    public int GetInitiativeGeneration()
    {
        // Letter/Promise/Burden cards (Move == null) don't generate Initiative
        if (!Move.HasValue) return 0;

        // Arguments never generate Initiative (they COST it instead)
        if (Move == ConversationalMove.Argument) return 0;

        // ALL Remarks and Observations generate +1 Initiative (no special cases)
        // Specialization comes from deck composition (Cunning has more Observation cards), not multipliers
        return BoundStat.HasValue ? 1 : 0;
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

        foreach (KeyValuePair<string, int> requirement in TokenRequirements)
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
    public bool MeetsStatementRequirements(SocialSession session)
    {
        if (!RequiredStat.HasValue || RequiredStatements <= 0)
        {
            return true; // No requirements
        }

        int statementCount = session.GetStatementCount(RequiredStat.Value);
        return statementCount >= RequiredStatements;
    }

    /// <summary>
    /// Check if card has a specific trait
    /// </summary>
    public bool HasTrait(CardTrait trait)
    {
        return Traits != null && Traits.Contains(trait);
    }
}