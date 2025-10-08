using System.Collections.Generic;

public class CardInstance
{
    public string InstanceId { get; init; } = Guid.NewGuid().ToString();

    // Template reference - single source of truth for card properties
    public GoalCard GoalCardTemplate { get; init; }
    public SocialCard SocialCardTemplate { get; init; }
    public MentalCard MentalCardTemplate { get; init; }
    public PhysicalCard PhysicalCardTemplate { get; init; }

    // Categorical properties that define behavior through context
    /// <summary>
    /// Get persistence type for this card, potentially modified by player stat bonuses
    /// Cards gain Statement persistence when bound stat reaches level 3
    /// </summary>
    public PersistenceType GetPersistence(PlayerStats playerStats)
    {
        PersistenceType basePersistence = SocialCardTemplate.Persistence;

        // Check if bound stat has persistence bonus (level 3+)
        if (SocialCardTemplate.BoundStat.HasValue && playerStats.HasPersistenceBonus(SocialCardTemplate.BoundStat.Value))
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
    public PersistenceType Persistence => SocialCardTemplate.Persistence;

    // Runtime context
    public CardContext Context { get; set; } // For exchange data and other context

    // Track if card is currently playable (for request cards)
    public bool IsPlayable { get; set; } = true;

    public string DeliveryObligationId => "";

    public CardInstance(GoalCard template)
    {
        GoalCardTemplate = template;
    }

    public CardInstance(SocialCard template)
    {
        SocialCardTemplate = template;
    }

    public CardInstance(MentalCard template)
    {
        MentalCardTemplate = template;
    }

    public CardInstance(PhysicalCard template)
    {
        PhysicalCardTemplate = template;
    }
}
