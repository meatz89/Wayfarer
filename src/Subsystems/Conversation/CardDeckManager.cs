using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages all card deck operations for conversations.
/// Handles drawing, playing, shuffling, and deck management.
/// </summary>
public class CardDeckManager
{
    private readonly GameWorld _gameWorld;
    private readonly Random _random;

    public CardDeckManager(GameWorld gameWorld)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _random = new Random();
    }

    /// <summary>
    /// Create a conversation deck from NPC templates
    /// </summary>
    public SessionCardDeck CreateConversationDeck(NPC npc, ConversationType conversationType, List<CardInstance> observationCards = null)
    {
        // Create a session deck from the NPC's conversation deck templates
        var sessionId = Guid.NewGuid().ToString();
        var deck = SessionCardDeck.CreateFromTemplates(npc.ConversationDeck.GetAllCards(), sessionId);
        
        // Add observation cards if provided
        if (observationCards != null && observationCards.Any())
        {
            foreach (var card in observationCards)
            {
                deck.AddCard(card);
            }
            deck.Shuffle();
        }

        // Add special cards based on conversation type
        if (conversationType == ConversationType.Promise && npc.GoalDeck != null)
        {
            // Mix in some letter cards for promise conversations
            var letterCards = npc.GoalDeck.GetAllCards()
                .Where(c => c.Category == CardCategory.Promise)
                .Take(3);
            foreach (var card in letterCards)
            {
                deck.AddCard(new CardInstance(card, sessionId));
            }
        }
        else if (conversationType == ConversationType.Resolution)
        {
            // Ensure burden cards are in deck for resolution
            // Burden cards are already part of the conversation deck
            // Just make sure we have enough of them
            var burdenCount = npc.CountBurdenCards();
            if (burdenCount < 2)
            {
                Console.WriteLine($"[CardDeckManager] Warning: Only {burdenCount} burden cards for Resolution conversation");
            }
        }

        deck.Shuffle();
        return deck;
    }

    /// <summary>
    /// Draw cards from deck based on emotional state
    /// </summary>
    public List<CardInstance> DrawCards(SessionCardDeck deck, int count, EmotionalState currentState)
    {
        var drawnCards = new List<CardInstance>();
        
        // Special handling for HOSTILE state - draw burden cards
        if (currentState == EmotionalState.HOSTILE)
        {
            var burdenCards = deck.DrawFilteredByCategory(1, CardCategory.Burden);
            drawnCards.AddRange(burdenCards);
        }
        else
        {
            // Normal draw
            var cards = deck.DrawCards(count);
            drawnCards.AddRange(cards);
        }

        return drawnCards;
    }

    /// <summary>
    /// Play cards and process their effects
    /// </summary>
    public CardPlayResult PlayCards(ConversationSession session, HashSet<CardInstance> selectedCards)
    {
        var results = new List<SingleCardResult>();
        int totalComfort = 0;
        EmotionalState newState = session.CurrentState;
        
        // Process each card
        foreach (var card in selectedCards)
        {
            var success = ValidateCardPlay(card, session.CurrentState);
            
            if (success)
            {
                // Calculate comfort based on weight
                var comfort = CalculateCardComfort(card, session.CurrentState);
                totalComfort += comfort;
                
                // Remove card from hand
                session.Hand.RemoveCards(new[] { card });
                
                // Add to discard if not one-shot
                if (card.Persistence != PersistenceType.Fleeting)
                {
                    session.Deck.Discard(card);
                }
            }
            
            results.Add(new SingleCardResult
            {
                Card = card,
                Success = success,
                Comfort = success ? CalculateCardComfort(card, session.CurrentState) : 0
            });
        }

        // Check for set bonuses
        bool hasSetBonus = CheckSetBonus(selectedCards);
        int setBonus = hasSetBonus ? 2 : 0;
        if (hasSetBonus)
        {
            totalComfort += setBonus;
        }

        // Check for state-specific bonuses
        bool hasConnectedBonus = session.CurrentState == EmotionalState.CONNECTED && totalComfort > 5;
        int connectedBonus = hasConnectedBonus ? 1 : 0;
        bool hasEagerBonus = session.CurrentState == EmotionalState.EAGER && selectedCards.Count >= 3;
        int eagerBonus = hasEagerBonus ? 1 : 0;

        return new CardPlayResult
        {
            Results = results,
            TotalComfort = totalComfort,
            NewState = newState,
            SetBonus = setBonus,
            ConnectedBonus = connectedBonus,
            EagerBonus = eagerBonus
        };
    }

    /// <summary>
    /// Validate if cards can be played in current state
    /// </summary>
    public bool ValidateCardSelection(HashSet<CardInstance> cards, EmotionalState currentState)
    {
        var weightLimit = GetWeightLimit(currentState);
        var totalWeight = cards.Sum(c => c.Weight);
        
        // Check weight limit
        if (totalWeight > weightLimit)
            return false;

        // Check individual card validity
        foreach (var card in cards)
        {
            if (!CanPlayCard(card, currentState))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Check if a specific card can be played
    /// </summary>
    private bool CanPlayCard(CardInstance card, EmotionalState currentState)
    {
        // Check if card has state restrictions
        if (card.Context?.ValidStates != null && card.Context.ValidStates.Any())
        {
            if (!card.Context.ValidStates.Contains(currentState))
                return false;
        }

        // HOSTILE state can only play burden cards
        if (currentState == EmotionalState.HOSTILE)
        {
            return card.Category == CardCategory.Burden;
        }

        // DESPERATE state restrictions
        if (currentState == EmotionalState.DESPERATE)
        {
            return card.Weight <= 1;
        }

        return true;
    }

    /// <summary>
    /// Validate individual card play
    /// </summary>
    private bool ValidateCardPlay(CardInstance card, EmotionalState currentState)
    {
        return CanPlayCard(card, currentState);
    }

    /// <summary>
    /// Calculate comfort change from card
    /// </summary>
    private int CalculateCardComfort(CardInstance card, EmotionalState currentState)
    {
        // Burden cards always negative
        if (card.Category == CardCategory.Burden)
            return -Math.Max(1, card.Weight);

        // Goal cards provide bonus comfort
        if (card.IsGoalCard)
            return card.Weight + 2;

        // Normal cards based on weight
        return card.Weight;
    }

    /// <summary>
    /// Get weight limit for emotional state
    /// </summary>
    private int GetWeightLimit(EmotionalState state)
    {
        return state switch
        {
            EmotionalState.DESPERATE => 1,
            EmotionalState.HOSTILE => 0,
            EmotionalState.TENSE => 2,
            EmotionalState.GUARDED => 1,
            EmotionalState.NEUTRAL => 3,
            EmotionalState.OPEN => 3,
            EmotionalState.EAGER => 3,
            EmotionalState.CONNECTED => 4,
            _ => 3
        };
    }

    /// <summary>
    /// Check for set bonus (3+ cards of same type)
    /// </summary>
    private bool CheckSetBonus(HashSet<CardInstance> cards)
    {
        if (cards.Count < 3)
            return false;

        var typeGroups = cards.GroupBy(c => c.Type);
        return typeGroups.Any(g => g.Count() >= 3);
    }

    /// <summary>
    /// Shuffle deck
    /// </summary>
    public void ShuffleDeck(SessionCardDeck deck)
    {
        deck.Shuffle();
    }

    /// <summary>
    /// Add goal card to deck and shuffle
    /// </summary>
    public void ShuffleGoalIntoDeck(SessionCardDeck deck, CardInstance goalCard)
    {
        // Add goal card to middle portion of deck
        var cards = new List<CardInstance>();
        var deckSize = deck.RemainingCards;
        
        // Draw some cards
        int drawCount = Math.Min(deckSize / 3, 5);
        for (int i = 0; i < drawCount; i++)
        {
            if (deck.HasCardsAvailable())
            {
                var drawnCards = deck.DrawCards(1);
                if (drawnCards.Any())
                    cards.AddRange(drawnCards);
            }
        }

        // Add goal card
        deck.AddCard(goalCard);
        
        // Put drawn cards back
        foreach (var card in cards)
        {
            deck.AddCard(card);
        }
        
        // Shuffle to randomize position
        deck.Shuffle();
    }

    /// <summary>
    /// Select a valid goal card for NPC and state
    /// </summary>
    public CardInstance SelectValidGoalCard(NPC npc, EmotionalState currentState)
    {
        if (npc.GoalDeck == null || !npc.GoalDeck.HasCardsAvailable())
            return null;

        var validGoals = npc.GoalDeck.GetAllCards()
            .Where(card => 
            {
                if (card.Context?.ValidStates != null && card.Context.ValidStates.Any())
                {
                    return card.Context.ValidStates.Contains(currentState);
                }
                return true; // No restrictions means always valid
            })
            .ToList();

        if (!validGoals.Any())
            return null;

        // Select random valid goal
        var selectedGoal = validGoals[_random.Next(validGoals.Count)];
        return new CardInstance(selectedGoal, Guid.NewGuid().ToString());
    }

    /// <summary>
    /// Manage hand size limit (max 7 cards)
    /// </summary>
    public void ManageHandSize(HandDeck hand)
    {
        while (hand.Count > 7)
        {
            // Remove oldest card (first in list)
            var oldestCard = hand.GetAllCards().FirstOrDefault();
            if (oldestCard != null)
            {
                hand.RemoveCards(new[] { oldestCard });
            }
        }
    }

    /// <summary>
    /// Handle card persistence (fleeting vs persistent)
    /// </summary>
    public List<CardInstance> ProcessFleetingCards(List<CardInstance> handCards)
    {
        return handCards.Where(c => c.Persistence == PersistenceType.Fleeting).ToList();
    }

    /// <summary>
    /// Determine primary card type from hand
    /// </summary>
    public CardType DeterminePrimaryCardType(List<CardInstance> cards)
    {
        if (!cards.Any())
            return CardType.Trust;

        return cards
            .GroupBy(c => c.Type)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault()?.Key ?? CardType.Trust;
    }

    /// <summary>
    /// Restore session cards from saved state
    /// </summary>
    public void RestoreSessionCards(ConversationSession session, List<string> handCardIds, List<string> deckCardIds)
    {
        // Clear current cards
        session.Hand = new HandDeck();
        
        // Restore hand cards
        foreach (var cardId in handCardIds)
        {
            var template = FindCardTemplate(cardId);
            if (template != null)
            {
                var instance = new CardInstance(template, session.NPC.ID);
                // Note: Can't preserve original ID as it's readonly
                session.Hand.AddCard(instance);
            }
        }

        // Restore deck cards by clearing and repopulating existing deck
        session.Deck.Clear();
        
        foreach (var cardId in deckCardIds)
        {
            var template = FindCardTemplate(cardId);
            if (template != null)
            {
                var instance = new CardInstance(template, session.NPC.ID);
                // Note: Can't preserve original ID as it's readonly
                session.Deck.AddCard(instance);
            }
        }
    }

    /// <summary>
    /// Find card template by ID
    /// </summary>
    private ConversationCard FindCardTemplate(string cardId)
    {
        // Extract template ID from instance ID (format: "templateId_guid")
        var parts = cardId.Split('_');
        if (parts.Length < 2)
            return null;
            
        var templateId = parts[0];
        
        // Search in GameWorld card templates
        if (_gameWorld.AllCardDefinitions.TryGetValue(templateId, out var template))
        {
            return template;
        }

        // Search in NPC decks
        foreach (var npc in _gameWorld.NPCs)
        {
            // Check conversation deck
            if (npc.ConversationDeck != null)
            {
                var card = npc.ConversationDeck.GetAllCards().FirstOrDefault(c => c.TemplateId == templateId);
                if (card != null) return card;
            }
            
            // Check goal deck
            if (npc.GoalDeck != null)
            {
                var card = npc.GoalDeck.GetAllCards().FirstOrDefault(c => c.TemplateId == templateId);
                if (card != null) return card;
            }
            
            // Note: Burden cards are in ConversationDeck with CardCategory.Burden
        }

        return null;
    }

    /// <summary>
    /// Create exchange deck for commerce conversations
    /// </summary>
    public List<CardInstance> CreateExchangeDeck(NPC npc, List<string> domainTags)
    {
        var exchangeCards = new List<CardInstance>();
        
        if (npc.ExchangeDeck != null)
        {
            foreach (var template in npc.ExchangeDeck.GetAllCards())
            {
                // Note: Domain filtering removed - CardContext doesn't have RequiredDomains
                
                exchangeCards.Add(new CardInstance(template, npc.ID));
            }
        }
        
        return exchangeCards;
    }
}