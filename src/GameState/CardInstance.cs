
using System.Collections.Generic;


// Card instance - runtime representation of a card
public class CardInstance
{
    public string InstanceId { get; init; } = Guid.NewGuid().ToString();

    // Template reference - single source of truth for card properties
    public SocialCard ConversationCardTemplate { get; init; }
    public MentalCard MentalCardTemplate { get; init; }
    public PhysicalCard PhysicalCardTemplate { get; init; }


    // Categorical properties that define behavior through context
    /// <summary>
    /// Get persistence type for this card, potentially modified by player stat bonuses
    /// Cards gain Statement persistence when bound stat reaches level 3
    /// </summary>
    public PersistenceType GetPersistence(PlayerStats playerStats)
    {
        PersistenceType basePersistence = ConversationCardTemplate.Persistence;

        // Check if bound stat has persistence bonus (level 3+)
        if (ConversationCardTemplate.BoundStat.HasValue && playerStats.HasPersistenceBonus(ConversationCardTemplate.BoundStat.Value))
        {
            // Cards gain Statement persistence if they don't already have it
            if (basePersistence != PersistenceType.Statement)
            {
                return PersistenceType.Statement;
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




    public string GetCategoryClass()
    {
        List<string> classes = new List<string>();

        // Add persistence class
        classes.Add($"card-{Persistence.ToString().ToLower()}");

        // Add success type class if not None
        if (ConversationCardTemplate.SuccessType != SuccessEffectType.None)
        {
            classes.Add($"success-{ConversationCardTemplate.SuccessType.ToString().ToLower()}");
        }

        // Add special classes for request cards
        if (ConversationCardTemplate.CardType == CardType.Request || ConversationCardTemplate.CardType == CardType.Promise || ConversationCardTemplate.CardType == CardType.Request)
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
    public bool CanDeliverLetter => ConversationCardTemplate.CardType == CardType.Request || ConversationCardTemplate.CardType == CardType.Promise || ConversationCardTemplate.CardType == CardType.Request;
    public string DeliveryObligationId => "";
    public string ObservationSource => "";
    // Exchange detection - exchanges are now separate ExchangeCard entities, not ConversationCards
    public bool IsExchange => false;  // CardInstances are only for ConversationCards now
    public CardMechanicsType Mechanics => CardMechanicsType.Standard;

    public int GetEffectiveFocus(ConnectionState state)
    {
        return ConversationCardTemplate.InitiativeCost;
    }




    public CardInstance() { }

    public CardInstance(SocialCard template, string sourceContext = null)
    {
        ConversationCardTemplate = template;
        SourceContext = sourceContext;
    }

    public CardInstance(MentalCard template, string sourceContext = null)
    {
        MentalCardTemplate = template;
        SourceContext = sourceContext;
    }

    public CardInstance(PhysicalCard template, string sourceContext = null)
    {
        PhysicalCardTemplate = template;
        SourceContext = sourceContext;
    }

    public CardType GetCardType()
    {
        if (ConversationCardTemplate != null) return CardType.Conversation;
        if (MentalCardTemplate != null) return CardType.Mental;
        if (PhysicalCardTemplate != null) return CardType.Physical;
        return CardType.Conversation;
    }

    // Removed - now using GameRules.CardProgression.GetLevelFromXp()

}
