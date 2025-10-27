using System.Collections.Generic;

public class CardInstance
{
    public string InstanceId { get; init; } = Guid.NewGuid().ToString();

    public CardTypes CardType { get; init; }
    public SituationCard SituationCardTemplate { get; init; }
    public SocialCard SocialCardTemplate { get; init; }
    public MentalCard MentalCardTemplate { get; init; }
    public PhysicalCard PhysicalCardTemplate { get; init; }

    /// <summary>
    /// Get base persistence without stat bonuses - used for CSS class generation
    /// For stat-aware persistence, use GetPersistence(PlayerStats)
    /// </summary>
    public PersistenceType Persistence => SocialCardTemplate.Persistence;

    // Runtime context
    public CardContext Context { get; set; } // For exchange data and other context

    // Track if card is currently playable (for request cards)
    public bool IsPlayable { get; set; } = true;

    public CardInstance(SituationCard template)
    {
        SituationCardTemplate = template ?? throw new ArgumentNullException("no template card");
        CardType = CardTypes.Situation;
    }

    public CardInstance(SocialCard template)
    {
        SocialCardTemplate = template ?? throw new ArgumentNullException("no template card");
        CardType = CardTypes.Social;
    }

    public CardInstance(MentalCard template)
    {
        MentalCardTemplate = template ?? throw new ArgumentNullException("no template card");
        CardType = CardTypes.Mental;
    }

    public CardInstance(PhysicalCard template)
    {
        PhysicalCardTemplate = template ?? throw new ArgumentNullException("no template card");
        CardType = CardTypes.Physical;
    }
}
