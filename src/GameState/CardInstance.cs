
using System.Collections.Generic;
using Wayfarer.GameState.Enums;

// Card instance - runtime representation of a card
public class CardInstance
{
    public string InstanceId { get; init; } = Guid.NewGuid().ToString();

    // Template reference - single source of truth for card properties
    public ConversationCard Template { get; init; }

    // Delegating properties for API compatibility
    public string Id => Template.Id;
    public string Description => Template.Description;
    public SuccessEffectType SuccessType => Template.SuccessType;
    public FailureEffectType FailureType => Template.FailureType;
    public ExhaustEffectType ExhaustType => Template.ExhaustType;
    public CardType CardType => Template.CardType;
    public ConnectionType TokenType => Template.TokenType;
    public int Focus => Template.Focus;
    public Difficulty Difficulty => Template.Difficulty;
    public int RapportThreshold => Template.RapportThreshold;
    public string RequestId => Template.RequestId;
    public int MinimumTokensRequired => Template.MinimumTokensRequired;
    public string DialogueFragment => Template.DialogueFragment;
    public string VerbPhrase => Template.VerbPhrase;

    // Categorical properties that define behavior through context
    /// <summary>
    /// Get persistence type for this card, potentially modified by player stat bonuses
    /// Cards gain Thought persistence when bound stat reaches level 3
    /// </summary>
    public PersistenceType GetPersistence(PlayerStats playerStats)
    {
        var basePersistence = Template.Persistence;

        // Check if bound stat has persistence bonus (level 3+)
        if (Template.BoundStat.HasValue && playerStats.HasPersistenceBonus(Template.BoundStat.Value))
        {
            // Cards gain Thought persistence if they don't already have it
            if (basePersistence != PersistenceType.Thought)
            {
                return PersistenceType.Thought;
            }
        }

        return basePersistence;
    }

    /// <summary>
    /// Legacy Persistence property for backwards compatibility
    /// Use GetPersistence(PlayerStats) for stat-aware persistence
    /// </summary>
    public PersistenceType Persistence => Template.Persistence;

    // Runtime context
    public string SourceContext { get; init; }
    public CardContext Context { get; set; } // For exchange data and other context

    // Track if card is currently playable (for request cards)
    public bool IsPlayable { get; set; } = true;

    /// <summary>
    /// Get effective level for this card based on player stats
    /// Cards derive their level from the bound stat level, not individual card XP
    /// </summary>
    public int GetEffectiveLevel(PlayerStats playerStats)
    {
        if (Template.BoundStat.HasValue)
        {
            return playerStats.GetLevel(Template.BoundStat.Value);
        }
        return 1; // Default level if no bound stat
    }

    /// <summary>
    /// Check if card ignores forced LISTEN on failure based on player stat level
    /// </summary>
    public bool IgnoresFailureListen(PlayerStats playerStats)
    {
        if (Template.BoundStat.HasValue)
        {
            return playerStats.IgnoresFailureListen(Template.BoundStat.Value);
        }
        return false;
    }

    /// <summary>
    /// Legacy XP property for backwards compatibility - now always returns 0
    /// XP is tracked at the stat level, not individual cards
    /// </summary>
    [Obsolete("XP is now tracked at player stat level, not individual cards")]
    public int XP { get; set; } = 0;

    /// <summary>
    /// Legacy Level property for backwards compatibility - use GetEffectiveLevel instead
    /// </summary>
    [Obsolete("Use GetEffectiveLevel(PlayerStats) instead")]
    public int Level => 1;

    public string GetCategoryClass()
    {
        List<string> classes = new List<string>();

        // Add persistence class
        classes.Add($"card-{Persistence.ToString().ToLower()}");

        // Add success type class if not None
        if (SuccessType != SuccessEffectType.None)
        {
            classes.Add($"success-{SuccessType.ToString().ToLower()}");
        }

        // Add failure type class if not None
        if (FailureType != FailureEffectType.None)
        {
            classes.Add($"failure-{FailureType.ToString().ToLower()}");
        }

        // Add special classes for request cards
        if (CardType == CardType.Letter || CardType == CardType.Promise || CardType == CardType.BurdenGoal)
        {
            classes.Add("card-request");
            if (!IsPlayable)
            {
                classes.Add("card-unplayable");
            }
        }

        return string.Join(" ", classes);
    }

    // Removed misleading Category and Type properties - use Properties list directly

    public ConnectionState? SuccessState => null;
    public ConnectionState? FailureState => null;
    public int BaseFlow => 1;
    public bool CanDeliverLetter => CardType == CardType.Letter || CardType == CardType.Promise || CardType == CardType.BurdenGoal;
    public string DeliveryObligationId => "";
    public string ObservationSource => "";
    // Exchange detection - exchanges are now separate ExchangeCard entities, not ConversationCards
    public bool IsExchange => false;  // CardInstances are only for ConversationCards now
    public CardMechanicsType Mechanics => CardMechanicsType.Standard;

    public int GetEffectiveFocus(ConnectionState state)
    {
        return Focus;
    }

    public int CalculateSuccessChance()
    {
        // Request/Promise cards always succeed (100%)
        if (CardType == CardType.Letter || CardType == CardType.Promise || CardType == CardType.BurdenGoal)
            return 100;

        return GetBaseSuccessPercentage();
    }

    public int CalculateSuccessChance(ConnectionState state)
    {
        // Request/Promise cards always succeed (100%)
        if (CardType == CardType.Letter || CardType == CardType.Promise || CardType == CardType.BurdenGoal)
            return 100;

        return GetBaseSuccessPercentage();
    }
    public ConnectionType GetConnectionType()
    {
        return TokenType;
    }

    /// <summary>
    /// Get base success percentage applying player stat bonuses
    /// </summary>
    public int GetBaseSuccessPercentage(PlayerStats playerStats)
    {
        int baseChance = Template.GetBaseSuccessPercentage();

        // Add stat bonus instead of card level bonus
        if (Template.BoundStat.HasValue)
        {
            int statBonus = playerStats.GetSuccessBonus(Template.BoundStat.Value);
            return baseChance + statBonus;
        }

        return baseChance;
    }

    /// <summary>
    /// Legacy method for backwards compatibility - use GetBaseSuccessPercentage(PlayerStats) instead
    /// </summary>
    [Obsolete("Use GetBaseSuccessPercentage(PlayerStats) instead")]
    public int GetBaseSuccessPercentage()
    {
        return Template.GetBaseSuccessPercentage();
    }

    public CardInstance() { }

    public CardInstance(ConversationCard template, string sourceContext = null)
    {
        Template = template;
        SourceContext = sourceContext;
    }

    // Removed - now using GameRules.CardProgression.GetLevelFromXp()

}
