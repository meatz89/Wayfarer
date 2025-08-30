using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Represents an NPC's conversation deck.
/// Each NPC has a unique deck based on personality and relationship.
/// All cards are loaded from conversations.json
/// </summary>
public class CardDeck : IEnumerable<ConversationCard>
{
    private readonly List<ConversationCard> cards;
    private readonly List<ConversationCard> discardPile;
    private readonly Random random;
    private static GameWorld _gameWorld;

    public CardDeck()
    {
        cards = new List<ConversationCard>();
        discardPile = new List<ConversationCard>();
        random = new Random();
    }

    /// <summary>
    /// Initialize with GameWorld reference for accessing card templates
    /// GameWorld is the single source of truth for all card data
    /// </summary>
    public static void InitializeGameWorld(GameWorld gameWorld)
    {
        _gameWorld = gameWorld;
    }

    /// <summary>
    /// Initialize deck for an NPC based on personality
    /// </summary>
    public void InitializeForNPC(NPC npc)
    {
        cards.Clear();
        discardPile.Clear();

        if (_gameWorld == null)
        {
            Console.WriteLine("WARNING: CardDeck not initialized with GameWorld - using empty deck");
            return;
        }

        // Load all cards for this NPC from GameWorld templates
        var npcCards = GetCardsForNPC(npc);
        cards.AddRange(npcCards);

        // NPCs with burden history get additional burden cards
        if (npc.HasBurdenHistory())
        {
            AddBurdenCardsForNPC(npc);
        }

        Shuffle();
    }

    private void AddBurdenCardsForNPC(NPC npc)
    {
        // Burden cards are defined in conversations.json with forNPC property
        // The parser already handles loading them based on NPC ID
        // This method is kept for any additional runtime burden card logic
    }

    /// <summary>
    /// Add a card to the deck
    /// </summary>
    public void AddCard(ConversationCard card)
    {
        cards.Add(card);
    }

    /// <summary>
    /// Remove a card from the deck
    /// </summary>
    public void RemoveCard(ConversationCard card)
    {
        cards.Remove(card);
    }

    /// <summary>
    /// Draw cards from the deck
    /// </summary>
    public List<ConversationCard> DrawCards(int count, int currentComfort)
    {
        var drawn = new List<ConversationCard>();

        // Cards are no longer filtered by comfort/depth - emotional state filtering happens elsewhere
        var availableCards = cards.ToList();

        if (!availableCards.Any())
        {
            Console.WriteLine($"No cards available in deck");
            return drawn;
        }

        // Draw up to 'count' cards
        for (int i = 0; i < count && availableCards.Any(); i++)
        {
            var index = random.Next(availableCards.Count);
            var card = availableCards[index];
            
            drawn.Add(card);
            
            // Always remove from availableCards to prevent drawing the same card twice in this draw operation
            availableCards.RemoveAt(index);
            
            // Remove non-persistent cards from the main deck after drawing
            if (card.Persistence != PersistenceType.Persistent)
            {
                cards.Remove(card);
            }
        }

        return drawn;
    }

    /// <summary>
    /// Discard a card
    /// </summary>
    public void Discard(ConversationCard card)
    {
        if (card.Persistence == PersistenceType.Opportunity)
        {
            // Opportunity cards are removed from play when discarded
            cards.Remove(card);
        }
        else if (card.Category == CardCategory.Burden)
        {
            // Burden cards stay in deck even when "discarded"
            // They're only removed through specific mechanics
        }
        else
        {
            // Persistent cards go to discard pile
            discardPile.Add(card);
        }
    }

    /// <summary>
    /// Shuffle the deck
    /// </summary>
    public void Shuffle()
    {
        // Fisher-Yates shuffle
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
    /// Get count of cards in deck
    /// </summary>
    public int Count => cards.Count;
    
    /// <summary>
    /// Alias for Count for compatibility
    /// </summary>
    public int RemainingCards => cards.Count;

    /// <summary>
    /// Get count of cards in discard pile
    /// </summary>
    public int DiscardCount => discardPile.Count;
    
    /// <summary>
    /// Check if deck is empty
    /// </summary>
    public bool IsEmpty => cards.Count == 0;

    /// <summary>
    /// Check if deck has any cards available
    /// </summary>
    public bool HasCardsAvailable()
    {
        return cards.Any();
    }

    /// <summary>
    /// Get all burden cards in the deck
    /// </summary>
    public List<ConversationCard> GetBurdenCards()
    {
        return cards.Where(c => c.Category == CardCategory.Burden).ToList();
    }

    /// <summary>
    /// Remove all burden cards from the deck (for resolution goals)
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
    /// Add observation cards to the deck
    /// </summary>
    public void AddObservationCards(List<ConversationCard> observationCards)
    {
        cards.AddRange(observationCards);
        Shuffle();
    }


    /// <summary>
    /// Get all cards in the deck (for debugging)
    /// </summary>
    public List<ConversationCard> GetAllCards()
    {
        return new List<ConversationCard>(cards);
    }

    /// <summary>
    /// Clear the deck completely
    /// </summary>
    public void Clear()
    {
        cards.Clear();
        discardPile.Clear();
    }
    
    /// <summary>
    /// Reset deck for new conversation
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
    
    /// <summary>
    /// Shuffle a goal card into the deck
    /// </summary>
    public void ShuffleInGoalCard(ConversationCard goalCard)
    {
        if (goalCard != null)
        {
            goalCard.DrawableStates = goalCard.Context.ValidStates;

            cards.Add(goalCard);
            var states = goalCard.DrawableStates != null ? string.Join(", ", goalCard.DrawableStates) : "Any";
            Console.WriteLine($"[ShuffleInGoalCard] Added goal card {goalCard.Id} with existing DrawableStates: {states}");
            
            Shuffle();
        }
    }
    
    /// <summary>
    /// Draw cards (alias for DrawCards for compatibility)
    /// </summary>
    public List<ConversationCard> Draw(int count, int currentComfort)
    {
        return DrawCards(count, currentComfort);
    }
    
    /// <summary>
    /// Draw cards filtered by category
    /// </summary>
    public List<ConversationCard> DrawFilteredByCategory(int count, int currentComfort, CardCategory category)
    {
        var drawn = new List<ConversationCard>();
        
        // Filter by card category only (depth filtering removed)
        var availableCards = cards
            .Where(c => c.Category == category)
            .ToList();
            
        for (int i = 0; i < count && availableCards.Any(); i++)
        {
            var index = random.Next(availableCards.Count);
            var card = availableCards[index];
            
            drawn.Add(card);
            
            if (card.Persistence != PersistenceType.Persistent)
            {
                cards.Remove(card);
                availableCards.Remove(card);
            }
        }
        
        return drawn;
    }
    
    /// <summary>
    /// Draw cards filtered by types
    /// </summary>
    public List<ConversationCard> DrawFilteredByTypes(int count, int currentComfort, List<CardType> types, bool includeTokenCards)
    {
        var drawn = new List<ConversationCard>();
        
        var availableCards = cards
            .Where(c => types == null || types.Contains(c.Type))
            .ToList();
            
        // Apply token card filter if needed
        if (includeTokenCards)
        {
            // Prefer cards that grant tokens
            var tokenCards = availableCards.Where(c => c.GrantsToken).ToList();
            if (tokenCards.Any())
                availableCards = tokenCards;
        }
            
        for (int i = 0; i < count && availableCards.Any(); i++)
        {
            var index = random.Next(availableCards.Count);
            var card = availableCards[index];
            
            drawn.Add(card);
            
            if (card.Persistence != PersistenceType.Persistent)
            {
                cards.Remove(card);
                availableCards.Remove(card);
            }
        }
        
        return drawn;
    }
    
    /// <summary>
    /// Get all cards (alias for GetAllCards)
    /// </summary>
    public List<ConversationCard> GetCards()
    {
        return GetAllCards();
    }
    
    /// <summary>
    /// Draw cards filtered by emotional state using DrawableStates property
    /// This replaces the old type-based filtering with explicit state declarations
    /// </summary>
    public List<ConversationCard> DrawFilteredByState(int count, int currentComfort, EmotionalState state)
    {
        var drawn = new List<ConversationCard>();
        
        // Filter cards that can be drawn in this state
        var availableCards = cards
            .Where(c => c.DrawableStates == null || 
                       c.DrawableStates.Count == 0 || 
                       c.DrawableStates.Contains(state))
            .ToList();
        
        // PRIORITY: If there's a drawable goal card, it MUST be drawn first
        var drawableGoalCard = availableCards.FirstOrDefault(c => c.IsGoalCard);
        if (drawableGoalCard != null)
        {
            drawn.Add(drawableGoalCard);
            if (drawableGoalCard.Persistence != PersistenceType.Persistent)
            {
                cards.Remove(drawableGoalCard);
                availableCards.Remove(drawableGoalCard);
            }
            count--; // Reduce count since we drew the goal card
            
            Console.WriteLine($"[DrawFilteredByState] Priority drew goal card: {drawableGoalCard.Id} in {state} state");
        }
            
        for (int i = 0; i < count && availableCards.Any(); i++)
        {
            var index = random.Next(availableCards.Count);
            var card = availableCards[index];
            
            drawn.Add(card);
            
            if (card.Persistence != PersistenceType.Persistent)
            {
                cards.Remove(card);
                availableCards.Remove(card);
            }
        }
        
        return drawn;
    }
    
    /// <summary>
    /// Check if deck has any cards
    /// </summary>
    public bool Any()
    {
        return cards.Any();
    }
    
    /// <summary>
    /// Check if deck has any cards matching a predicate
    /// </summary>
    public bool Any(Func<ConversationCard, bool> predicate)
    {
        return cards.Any(predicate);
    }
    
    /// <summary>
    /// Get first card matching predicate or null
    /// </summary>
    public ConversationCard FirstOrDefault(Func<ConversationCard, bool> predicate)
    {
        return cards.FirstOrDefault(predicate);
    }
    
    /// <summary>
    /// Take a specific number of cards from the deck
    /// </summary>
    public IEnumerable<ConversationCard> Take(int count)
    {
        return cards.Take(count);
    }
    
    /// <summary>
    /// Get enumerator for foreach support
    /// </summary>
    public IEnumerator<ConversationCard> GetEnumerator()
    {
        return cards.GetEnumerator();
    }
    
    /// <summary>
    /// Non-generic enumerator
    /// </summary>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    
    /// <summary>
    /// Get all cards for an NPC's personality from GameWorld templates
    /// </summary>
    private List<ConversationCard> GetCardsForNPC(NPC npc)
    {
        var cards = new List<ConversationCard>();

        // Add universal cards from GameWorld
        var universalCards = _gameWorld.AllCardDefinitions.Values
            .Where(c => string.IsNullOrEmpty(c.Context?.NPCName) || c.Id.Contains(npc.ID))
            .Where(c => c.IsGoalCard != true && c.Category != CardCategory.Burden);

        foreach (var card in universalCards)
        {
            cards.Add(card);
        }

        // Add personality-specific cards from GameWorld
        if (_gameWorld.PersonalityMappings.TryGetValue(npc.PersonalityType, out var mapping))
        {
            foreach (var cardId in mapping.Cards)
            {
                if (_gameWorld.AllCardDefinitions.TryGetValue(cardId, out var card))
                {
                    cards.Add(card);
                }
            }
        }

        return cards;
    }


}