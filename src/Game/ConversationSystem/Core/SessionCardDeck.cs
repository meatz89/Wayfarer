using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Represents a deck of card instances for a single conversation session.
/// Each card in the deck is a unique instance with its own ID.
/// </summary>
public class SessionCardDeck : IEnumerable<CardInstance>
{
    private readonly List<CardInstance> cards;
    private readonly List<CardInstance> discardPile;
    private readonly Random random;
    private readonly string sessionId;
    
    public SessionCardDeck(string sessionId)
    {
        this.sessionId = sessionId ?? Guid.NewGuid().ToString();
        cards = new List<CardInstance>();
        discardPile = new List<CardInstance>();
        random = new Random();
    }
    
    /// <summary>
    /// Create a session deck from card templates
    /// </summary>
    public static SessionCardDeck CreateFromTemplates(IEnumerable<ConversationCard> templates, string sessionId = null)
    {
        var deck = new SessionCardDeck(sessionId);
        
        foreach (var template in templates)
        {
            // Create a new instance for each template
            var instance = new CardInstance(template, deck.sessionId);
            deck.cards.Add(instance);
        }
        
        deck.Shuffle();
        return deck;
    }
    
    /// <summary>
    /// Add a card instance to the deck
    /// </summary>
    public void AddCard(CardInstance card)
    {
        cards.Add(card);
    }
    
    /// <summary>
    /// Add a new instance from a template
    /// </summary>
    public CardInstance AddTemplate(ConversationCard template)
    {
        var instance = new CardInstance(template, sessionId);
        cards.Add(instance);
        return instance;
    }
    
    /// <summary>
    /// Remove a specific card instance from the deck
    /// </summary>
    public void RemoveCard(CardInstance card)
    {
        cards.Remove(card);
    }
    
    /// <summary>
    /// Draw cards from the deck
    /// </summary>
    public List<CardInstance> DrawCards(int count)
    {
        var drawn = new List<CardInstance>();
        
        if (!cards.Any())
        {
            Console.WriteLine($"[SessionCardDeck] No cards available in deck");
            return drawn;
        }
        
        // Create a working copy to avoid modifying while iterating
        var availableCards = cards.ToList();
        
        for (int i = 0; i < count && availableCards.Any(); i++)
        {
            var index = random.Next(availableCards.Count);
            var card = availableCards[index];
            
            drawn.Add(card);
            
            // Always remove from available to prevent drawing same instance twice
            availableCards.Remove(card);
            
            // Remove from main deck based on persistence
            if (card.Persistence != PersistenceType.Persistent)
            {
                cards.Remove(card);
            }
        }
        
        Console.WriteLine($"[SessionCardDeck] Drew {drawn.Count} cards from deck");
        return drawn;
    }
    
    /// <summary>
    /// Draw cards filtered by emotional state
    /// </summary>
    public List<CardInstance> DrawFilteredByState(int count, EmotionalState state)
    {
        return DrawFilteredByState(count, 0, state);
    }
    
    /// <summary>
    /// Draw cards filtered by emotional state with comfort level
    /// </summary>
    public List<CardInstance> DrawFilteredByState(int count, int comfort, EmotionalState state)
    {
        var drawn = new List<CardInstance>();
        
        // Filter cards that can be drawn in this state
        var availableCards = cards
            .Where(c => c.DrawableStates == null || 
                       c.DrawableStates.Count == 0 || 
                       c.DrawableStates.Contains(state))
            .ToList();
        
        if (!availableCards.Any())
        {
            Console.WriteLine($"[SessionCardDeck] No cards available for {state} state");
            return drawn;
        }
        
        // PRIORITY: Draw goal cards first if available
        var goalCard = availableCards.FirstOrDefault(c => c.IsGoalCard);
        if (goalCard != null)
        {
            drawn.Add(goalCard);
            availableCards.Remove(goalCard);
            
            if (goalCard.Persistence != PersistenceType.Persistent)
            {
                cards.Remove(goalCard);
            }
            
            count--;
            Console.WriteLine($"[SessionCardDeck] Priority drew goal card: {goalCard.TemplateId} (instance: {goalCard.InstanceId})");
        }
        
        // Draw remaining cards
        for (int i = 0; i < count && availableCards.Any(); i++)
        {
            var index = random.Next(availableCards.Count);
            var card = availableCards[index];
            
            drawn.Add(card);
            availableCards.Remove(card); // Always remove to prevent duplicates
            
            if (card.Persistence != PersistenceType.Persistent)
            {
                cards.Remove(card);
            }
        }
        
        Console.WriteLine($"[SessionCardDeck] Drew {drawn.Count} cards for {state} state");
        return drawn;
    }
    
    /// <summary>
    /// Draw cards filtered by category
    /// </summary>
    public List<CardInstance> DrawFilteredByCategory(int count, CardCategory category)
    {
        return DrawFilteredByCategory(count, 0, category);
    }
    
    /// <summary>
    /// Draw cards filtered by category with comfort level
    /// </summary>
    public List<CardInstance> DrawFilteredByCategory(int count, int comfort, CardCategory category)
    {
        var drawn = new List<CardInstance>();
        
        // Filter cards by category
        var availableCards = cards
            .Where(c => c.Category == category)
            .ToList();
        
        if (!availableCards.Any())
        {
            Console.WriteLine($"[SessionCardDeck] No cards available for {category} category");
            return drawn;
        }
        
        // Draw cards
        for (int i = 0; i < count && availableCards.Any(); i++)
        {
            var index = random.Next(availableCards.Count);
            var card = availableCards[index];
            
            drawn.Add(card);
            availableCards.Remove(card); // Always remove to prevent duplicates
            
            if (card.Persistence != PersistenceType.Persistent)
            {
                cards.Remove(card);
            }
        }
        
        Console.WriteLine($"[SessionCardDeck] Drew {drawn.Count} cards for {category} category");
        return drawn;
    }
    
    /// <summary>
    /// Discard a card instance
    /// </summary>
    public void Discard(CardInstance card)
    {
        if (card.Persistence == PersistenceType.Opportunity)
        {
            // Opportunity cards are removed from play
            cards.Remove(card);
        }
        else if (card.Category == CardCategory.Burden)
        {
            // Burden cards stay in deck
        }
        else
        {
            // Other cards go to discard pile
            discardPile.Add(card);
        }
    }
    
    /// <summary>
    /// Shuffle the deck
    /// </summary>
    public void Shuffle()
    {
        for (int i = cards.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            var temp = cards[i];
            cards[i] = cards[j];
            cards[j] = temp;
        }
    }
    
    /// <summary>
    /// Reshuffle discard pile back into deck
    /// </summary>
    public void ReshuffleDiscardPile()
    {
        cards.AddRange(discardPile);
        discardPile.Clear();
        Shuffle();
    }
    
    /// <summary>
    /// Shuffle a goal card template into the deck
    /// </summary>
    public void ShuffleInGoalCard(ConversationCard goalTemplate)
    {
        if (goalTemplate != null)
        {
            var instance = AddTemplate(goalTemplate);
            Shuffle();
            Console.WriteLine($"[SessionCardDeck] Shuffled in goal card {goalTemplate.Id} as instance {instance.InstanceId}");
        }
    }
    
    /// <summary>
    /// Get all burden cards in the deck
    /// </summary>
    public List<CardInstance> GetBurdenCards()
    {
        return cards.Where(c => c.Category == CardCategory.Burden).ToList();
    }
    
    /// <summary>
    /// Remove all burden cards from the deck
    /// </summary>
    public int RemoveAllBurdenCards()
    {
        var burdenCards = GetBurdenCards();
        foreach (var card in burdenCards)
        {
            cards.Remove(card);
        }
        return burdenCards.Count;
    }
    
    /// <summary>
    /// Get all cards in the deck
    /// </summary>
    public List<CardInstance> GetAllCards()
    {
        return new List<CardInstance>(cards);
    }
    
    /// <summary>
    /// Get all cards in the deck (alias for GetAllCards)
    /// </summary>
    public List<CardInstance> GetCards()
    {
        return GetAllCards();
    }
    
    /// <summary>
    /// Clear the deck completely
    /// </summary>
    public void Clear()
    {
        cards.Clear();
        discardPile.Clear();
    }
    
    // Properties
    public int Count => cards.Count;
    public int RemainingCards => cards.Count;
    public int DiscardCount => discardPile.Count;
    public bool IsEmpty => cards.Count == 0;
    public bool HasCardsAvailable() => cards.Any();
    
    /// <summary>
    /// Reset deck for new conversation - move discarded cards back and remove fleeting cards
    /// </summary>
    public void ResetForNewConversation()
    {
        // Move all cards from discard back to deck
        cards.AddRange(discardPile);
        discardPile.Clear();
        // Remove any temporary cards
        cards.RemoveAll(c => c.Persistence == PersistenceType.Fleeting);
        Shuffle();
    }
    
    // IEnumerable implementation
    public IEnumerator<CardInstance> GetEnumerator() => cards.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}