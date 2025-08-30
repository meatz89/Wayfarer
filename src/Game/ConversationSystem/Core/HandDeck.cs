using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Represents the player's hand of cards during a conversation.
/// Cards are transferred here from the SessionCardDeck when drawn.
/// </summary>
public class HandDeck
{
    private readonly List<CardInstance> cards;
    private const int MAX_SIZE = 7;
    
    public HandDeck()
    {
        cards = new List<CardInstance>();
    }
    
    /// <summary>
    /// Check if hand is overflowing (forces SPEAK action)
    /// </summary>
    public bool IsOverflowing => cards.Count > MAX_SIZE;
    
    /// <summary>
    /// Number of cards in hand
    /// </summary>
    public int Count => cards.Count;
    
    /// <summary>
    /// Add a card to the hand (transferred from deck)
    /// </summary>
    public void AddCard(CardInstance card)
    {
        if (card == null)
            throw new ArgumentNullException(nameof(card));
            
        cards.Add(card);
        Console.WriteLine($"[HandDeck] Added card {card.TemplateId} to hand. Hand size: {cards.Count}");
    }
    
    /// <summary>
    /// Add multiple cards to the hand
    /// </summary>
    public void AddCards(IEnumerable<CardInstance> cardsToAdd)
    {
        if (cardsToAdd == null)
            return;
            
        foreach (var card in cardsToAdd)
        {
            AddCard(card);
        }
    }
    
    /// <summary>
    /// Remove fleeting cards during LISTEN action
    /// </summary>
    public void RemoveFleetingCards()
    {
        var fleetingCount = cards.Count(c => 
            c.Persistence == PersistenceType.Fleeting && 
            !c.IsObservation && 
            !c.IsGoalCard);
            
        cards.RemoveAll(c => 
            c.Persistence == PersistenceType.Fleeting && 
            !c.IsObservation && 
            !c.IsGoalCard);
            
        if (fleetingCount > 0)
        {
            Console.WriteLine($"[HandDeck] Removed {fleetingCount} fleeting cards from hand");
        }
    }
    
    /// <summary>
    /// Remove specific cards from hand (when played)
    /// </summary>
    public void RemoveCards(IEnumerable<CardInstance> cardsToRemove)
    {
        if (cardsToRemove == null)
            return;
            
        var toRemove = new HashSet<CardInstance>(cardsToRemove);
        var removedCount = cards.RemoveAll(c => toRemove.Contains(c));
        
        Console.WriteLine($"[HandDeck] Removed {removedCount} played cards from hand");
    }
    
    /// <summary>
    /// Get all cards in hand (for display)
    /// </summary>
    public List<CardInstance> GetAllCards()
    {
        return new List<CardInstance>(cards);
    }
    
    /// <summary>
    /// Clear the hand completely
    /// </summary>
    public void Clear()
    {
        cards.Clear();
    }
    
    /// <summary>
    /// Check if a specific card is in hand
    /// </summary>
    public bool Contains(CardInstance card)
    {
        return cards.Contains(card);
    }
    
    /// <summary>
    /// Check if hand contains any cards matching predicate
    /// </summary>
    public bool Any(Func<CardInstance, bool> predicate = null)
    {
        return predicate == null ? cards.Any() : cards.Any(predicate);
    }
    
    /// <summary>
    /// Find first card matching predicate
    /// </summary>
    public CardInstance FirstOrDefault(Func<CardInstance, bool> predicate)
    {
        return cards.FirstOrDefault(predicate);
    }
}