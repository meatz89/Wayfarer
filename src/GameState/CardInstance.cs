
using System.Collections.Generic;


// Card instance - runtime representation of a card
public class CardInstance
{
    public string InstanceId { get; init; } = Guid.NewGuid().ToString();

    // Template reference - single source of truth for card properties
    public ConversationCard ConversationCardTemplate { get; init; }

    // Delegating properties for API compatibility
    public string Id => ConversationCardTemplate.Id;
    public string Description => ConversationCardTemplate.Description;
    public SuccessEffectType SuccessType => ConversationCardTemplate.SuccessType;
    public FailureEffectType FailureType => ConversationCardTemplate.FailureType;
    public CardType CardType => ConversationCardTemplate.CardType;
    public ConnectionType TokenType => ConversationCardTemplate.TokenType;
    public int InitiativeCost => ConversationCardTemplate.InitiativeCost;
    public Difficulty Difficulty => ConversationCardTemplate.Difficulty;
    public int MomentumThreshold => ConversationCardTemplate.MomentumThreshold;
    public string RequestId => ConversationCardTemplate.RequestId;
    public int MinimumTokensRequired => ConversationCardTemplate.MinimumTokensRequired;
    public string DialogueFragment => ConversationCardTemplate.DialogueFragment;
    public string VerbPhrase => ConversationCardTemplate.VerbPhrase;

    // Categorical properties that define behavior through context
    /// <summary>
    /// Get persistence type for this card, potentially modified by player stat bonuses
    /// Cards gain Thought persistence when bound stat reaches level 3
    /// </summary>
    public PersistenceType GetPersistence(PlayerStats playerStats)
    {
        PersistenceType basePersistence = ConversationCardTemplate.Persistence;

        // Check if bound stat has persistence bonus (level 3+)
        if (ConversationCardTemplate.BoundStat.HasValue && playerStats.HasPersistenceBonus(ConversationCardTemplate.BoundStat.Value))
        {
            // Cards gain Thought persistence if they don't already have it
            if (basePersistence != PersistenceType.Standard)
            {
                return PersistenceType.Standard;
            }
        }

        return basePersistence;
    }

    /// <summary>
    /// Get base persistence without stat bonuses - used for CSS class generation
    /// For stat-aware persistence, use GetPersistence(PlayerStats)
    /// </summary>
    public PersistenceType Persistence => ConversationCardTemplate.Persistence;

    // Runtime context
    public string SourceContext { get; init; }
    public CardContext Context { get; set; } // For exchange data and other context

    // Track if card is currently playable (for request cards)
    public bool IsPlayable { get; set; } = true;


    /// <summary>
    /// Check if card ignores forced LISTEN on failure based on player stat level
    /// </summary>
    public bool IgnoresFailureListen(PlayerStats playerStats)
    {
        if (ConversationCardTemplate.BoundStat.HasValue)
        {
            return playerStats.IgnoresFailureListen(ConversationCardTemplate.BoundStat.Value);
        }
        return false;
    }


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
        if (CardType == CardType.Letter || CardType == CardType.Promise || CardType == CardType.Letter)
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
    public bool CanDeliverLetter => CardType == CardType.Letter || CardType == CardType.Promise || CardType == CardType.Letter;
    public string DeliveryObligationId => "";
    public string ObservationSource => "";
    // Exchange detection - exchanges are now separate ExchangeCard entities, not ConversationCards
    public bool IsExchange => false;  // CardInstances are only for ConversationCards now
    public CardMechanicsType Mechanics => CardMechanicsType.Standard;

    public int GetEffectiveFocus(ConnectionState state)
    {
        return InitiativeCost;
    }


    /// <summary>
    /// Get base success percentage applying player stat bonuses
    /// </summary>
    public int GetBaseSuccessPercentage(PlayerStats playerStats)
    {
        int baseChance = ConversationCardTemplate.GetBaseSuccessPercentage();

        // Add stat bonus instead of card level bonus
        if (ConversationCardTemplate.BoundStat.HasValue)
        {
            int statBonus = playerStats.GetSuccessBonus(ConversationCardTemplate.BoundStat.Value);
            return baseChance + statBonus;
        }

        return baseChance;
    }


    public CardInstance() { }

    public CardInstance(ConversationCard template, string sourceContext = null)
    {
        ConversationCardTemplate = template;
        SourceContext = sourceContext;
    }

    // Removed - now using GameRules.CardProgression.GetLevelFromXp()

}
