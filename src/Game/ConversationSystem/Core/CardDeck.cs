using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Represents an NPC's conversation deck.
/// Each NPC has a unique deck based on personality and relationship.
/// All cards are loaded from conversations.json
/// </summary>
public class CardDeck : IEnumerable<ICard>
{
    private readonly List<ICard> cards;
    private readonly List<ICard> discardPile;
    private readonly Random random;
    private static GameWorld _gameWorld;

    public CardDeck()
    {
        cards = new List<ICard>();
        discardPile = new List<ICard>();
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
    public void AddCard(ICard card)
    {
        cards.Add(card);
    }

    /// <summary>
    /// Remove a card from the deck
    /// </summary>
    public void RemoveCard(ICard card)
    {
        cards.Remove(card);
    }

    /// <summary>
    /// Draw cards from the deck
    /// </summary>
    public List<ICard> DrawCards(int count, int currentComfort)
    {
        var drawn = new List<ICard>();

        // Filter available cards by depth (comfort level requirement)
        // Only ConversationCards have Depth property
        var availableCards = cards
            .Where(c => {
                if (c is ConversationCard convCard)
                    return convCard.Depth <= currentComfort || convCard.Depth == 0; // Depth 0 = always available
                return true; // Non-ConversationCards are always available
            })
            .ToList();

        if (!availableCards.Any())
        {
            Console.WriteLine($"No cards available at comfort level {currentComfort}");
            return drawn;
        }

        // Draw up to 'count' cards
        for (int i = 0; i < count && availableCards.Any(); i++)
        {
            var index = random.Next(availableCards.Count);
            var card = availableCards[index];
            
            drawn.Add(card);
            
            // Remove non-persistent cards from deck after drawing
            if (card is ConversationCard convCard && convCard.Persistence != PersistenceType.Persistent)
            {
                cards.Remove(card);
                availableCards.Remove(card);
            }
        }

        return drawn;
    }

    /// <summary>
    /// Discard a card
    /// </summary>
    public void Discard(ICard card)
    {
        // Only ConversationCards have Persistence property
        if (card is ConversationCard convCard)
        {
            if (convCard.Persistence == PersistenceType.Opportunity)
            {
                // Opportunity cards are removed from play when discarded
                cards.Remove(card);
            }
            else if (convCard.Persistence == PersistenceType.Burden)
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
        else
        {
            // Non-ConversationCards go to discard pile by default
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
    /// Check if deck has cards available at given comfort level
    /// </summary>
    public bool HasCardsAtComfortLevel(int comfortLevel)
    {
        return cards.Any(c => {
            if (c is ConversationCard convCard)
                return convCard.Depth <= comfortLevel || convCard.Depth == 0;
            return true; // Non-ConversationCards are always available
        });
    }

    /// <summary>
    /// Get all burden cards in the deck
    /// </summary>
    public List<ICard> GetBurdenCards()
    {
        return cards.Where(c => c is ConversationCard convCard && convCard.Persistence == PersistenceType.Burden).ToList();
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
    public void AddObservationCards(List<ICard> observationCards)
    {
        cards.AddRange(observationCards);
        Shuffle();
    }


    /// <summary>
    /// Get all cards in the deck (for debugging)
    /// </summary>
    public List<ICard> GetAllCards()
    {
        return new List<ICard>(cards);
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
        // Remove any temporary cards (only ConversationCards have Persistence)
        cards.RemoveAll(c => c is ConversationCard convCard && convCard.Persistence == PersistenceType.Fleeting);
        Shuffle();
    }
    
    /// <summary>
    /// Shuffle a goal card into the deck
    /// </summary>
    public void ShuffleInGoalCard(ICard goalCard)
    {
        if (goalCard != null)
        {
            cards.Add(goalCard);
            Shuffle();
        }
    }
    
    /// <summary>
    /// Draw cards (alias for DrawCards for compatibility)
    /// </summary>
    public List<ICard> Draw(int count, int currentComfort)
    {
        return DrawCards(count, currentComfort);
    }
    
    /// <summary>
    /// Draw cards filtered by type
    /// </summary>
    public List<ICard> DrawFilteredByType<T>(int count, int currentComfort) where T : ICard
    {
        var drawn = new List<ICard>();
        
        // Filter by card type and depth if applicable
        var availableCards = cards
            .Where(c => {
                if (c is T)
                {
                    // Check depth if it's a ConversationCard
                    if (c is ConversationCard convCard)
                        return convCard.Depth <= currentComfort || convCard.Depth == 0;
                    return true;
                }
                return false;
            })
            .ToList();
            
        for (int i = 0; i < count && availableCards.Any(); i++)
        {
            var index = random.Next(availableCards.Count);
            var card = availableCards[index];
            
            drawn.Add(card);
            
            if (card is ConversationCard convCard && convCard.Persistence != PersistenceType.Persistent)
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
    public List<ICard> DrawFilteredByTypes(int count, int currentComfort, List<CardType> types, bool includeTokenCards)
    {
        var drawn = new List<ICard>();
        
        // Only ConversationCards have Type, Depth and GrantsToken
        var availableCards = cards
            .Where(c => {
                if (c is ConversationCard convCard)
                {
                    var depthOk = convCard.Depth <= currentComfort || convCard.Depth == 0;
                    var typeOk = types == null || types.Contains(convCard.Type);
                    return depthOk && typeOk;
                }
                return false;
            })
            .ToList();
            
        // Apply token card filter if needed
        if (includeTokenCards)
        {
            // Prefer cards that grant tokens
            var tokenCards = availableCards.Where(c => c is ConversationCard convCard && convCard.GrantsToken).ToList();
            if (tokenCards.Any())
                availableCards = tokenCards;
        }
            
        for (int i = 0; i < count && availableCards.Any(); i++)
        {
            var index = random.Next(availableCards.Count);
            var card = availableCards[index];
            
            drawn.Add(card);
            
            if (card is ConversationCard convCard && convCard.Persistence != PersistenceType.Persistent)
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
    public List<ICard> GetCards()
    {
        return GetAllCards();
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
    public bool Any(Func<ICard, bool> predicate)
    {
        return cards.Any(predicate);
    }
    
    /// <summary>
    /// Get first card matching predicate or null
    /// </summary>
    public ICard FirstOrDefault(Func<ICard, bool> predicate)
    {
        return cards.FirstOrDefault(predicate);
    }
    
    /// <summary>
    /// Take a specific number of cards from the deck
    /// </summary>
    public IEnumerable<ICard> Take(int count)
    {
        return cards.Take(count);
    }
    
    /// <summary>
    /// Get enumerator for foreach support
    /// </summary>
    public IEnumerator<ICard> GetEnumerator()
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
    /// Generate a crisis card dynamically
    /// </summary>
    public static ConversationCard GenerateCrisisCard(NPC npc)
    {
        // Crisis cards should be loaded from JSON, but provide fallback
        return new ConversationCard
        {
            Id = $"crisis_generated_{Guid.NewGuid()}",
            Template = CardTemplateType.UrgentPlea,
            Context = new CardContext
            {
                Personality = npc.PersonalityType,
                EmotionalState = EmotionalState.DESPERATE,
                NPCName = npc.Name,
                UrgencyLevel = 3,
                HasDeadline = true
            },
            Type = CardType.Trust,
            Persistence = PersistenceType.Goal,
            Weight = 0, // Free in crisis
            BaseComfort = 8,
            DisplayName = "Crisis Moment",
            Description = $"{npc.Name} needs immediate help!",
            SuccessRate = 35
        };
    }

    /// <summary>
    /// Get all cards for an NPC's personality from GameWorld templates
    /// </summary>
    private List<ICard> GetCardsForNPC(NPC npc)
    {
        var cards = new List<ICard>();

        // Add universal cards from GameWorld
        var universalCards = _gameWorld.AllCardDefinitions.Values
            .Where(c => string.IsNullOrEmpty(c.Context?.NPCName) || c.Id.Contains(npc.ID))
            .Where(c => c.IsGoalCard != true && !(c is BurdenCard));

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