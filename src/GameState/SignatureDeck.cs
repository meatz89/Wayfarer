using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Signature deck for NPC or Location containing earned knowledge cards
/// Each NPC and Location has ONE signature deck that accumulates knowledge through successful engagements
/// Knowledge cards of matching tactical type are added to starting hand during tactical engagements
/// </summary>
public class SignatureDeck
{
    public string EntityId { get; set; }
    public SignatureDeckType DeckType { get; set; }
    public List<SignatureKnowledgeCard> Cards { get; set; } = new List<SignatureKnowledgeCard>();

    /// <summary>
    /// Get knowledge cards matching specific tactical type for starting hand
    /// </summary>
    public List<SignatureKnowledgeCard> GetCardsForTacticalType(TacticalSystemType tacticalType)
    {
        return Cards.Where(c => c.TacticalType == tacticalType && !c.IsConsumed).ToList();
    }

    /// <summary>
    /// Add knowledge card to signature deck
    /// </summary>
    public void AddKnowledgeCard(SignatureKnowledgeCard card)
    {
        Cards.Add(card);
    }

    /// <summary>
    /// Consume a knowledge card (for Consumable persistence mode)
    /// </summary>
    public void ConsumeCard(string cardId)
    {
        SignatureKnowledgeCard card = Cards.FirstOrDefault(c => c.CardId == cardId);
        if (card != null && card.PersistenceMode == KnowledgePersistence.Consumable)
        {
            card.IsConsumed = true;
        }
    }
}

/// <summary>
/// Knowledge card entry in signature deck
/// References actual card definition plus metadata about persistence
/// </summary>
public class SignatureKnowledgeCard
{
    public string CardId { get; set; }
    public TacticalSystemType TacticalType { get; set; }
    public KnowledgePersistence PersistenceMode { get; set; }
    public bool IsConsumed { get; set; } = false;
    public string EarnedFromEngagementId { get; set; }
}

/// <summary>
/// Type of signature deck
/// </summary>
public enum SignatureDeckType
{
    NPC,        // Signature deck for an NPC
    Location    // Signature deck for a Location
}

/// <summary>
/// Knowledge card persistence mode
/// Persistent: Remains in signature deck after being played (ongoing understanding)
/// Consumable: Removed from signature deck after being played (one-time opportunity)
/// </summary>
public enum KnowledgePersistence
{
    Persistent,   // Card remains useful indefinitely
    Consumable    // Card is spent after one use
}
