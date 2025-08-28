using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Represents an NPC's conversation deck.
/// Each NPC has a unique deck based on personality and relationship.
/// All cards are loaded from conversations.json
/// </summary>
public class CardDeck
{
    private readonly List<ConversationCard> cards;
    private readonly List<ConversationCard> discardPile;
    private readonly Random random;
    private Dictionary<ConnectionType, int> currentTokens;
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
    public void InitializeForNPC(NPC npc, TokenMechanicsManager tokenManager)
    {
        cards.Clear();
        discardPile.Clear();

        if (_gameWorld == null)
        {
            Console.WriteLine("WARNING: CardDeck not initialized with GameWorld - using empty deck");
            return;
        }

        // Store tokens for filtering during draw
        currentTokens = tokenManager?.GetTokensWithNPC(npc.ID) ?? new Dictionary<ConnectionType, int>();

        // Load all cards for this NPC from GameWorld templates
        var npcCards = GetCardsForNPC(npc, currentTokens);
        cards.AddRange(npcCards);

        // NPCs with burden history get additional burden cards
        if (npc.HasBurdenHistory())
        {
            AddBurdenCardsForNPC(npc);
        }

        Shuffle();
    }

    /// <summary>
    /// Initialize crisis deck for an NPC
    /// </summary>
    public void InitializeCrisisDeck(NPC npc)
    {
        cards.Clear();
        discardPile.Clear();

        if (_gameWorld == null)
        {
            Console.WriteLine("WARNING: CardDeck not initialized with GameWorld - using empty crisis deck");
            return;
        }

        // Load crisis cards from GameWorld templates
        var crisisCards = GetCrisisCards(npc);
        cards.AddRange(crisisCards);

        // No shuffle for crisis decks - they're drawn in order
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

        // Filter available cards by depth (comfort level requirement)
        var availableCards = cards
            .Where(c => c.Depth <= currentComfort || c.Depth == 0) // Depth 0 = always available
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
            if (card.Persistence != PersistenceType.Persistent)
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
    public void Discard(ConversationCard card)
    {
        if (card.Persistence == PersistenceType.Opportunity)
        {
            // Opportunity cards are removed from play when discarded
            cards.Remove(card);
        }
        else if (card.Persistence == PersistenceType.Burden)
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
    /// Check if deck has cards available at given comfort level
    /// </summary>
    public bool HasCardsAtComfortLevel(int comfortLevel)
    {
        return cards.Any(c => c.Depth <= comfortLevel || c.Depth == 0);
    }

    /// <summary>
    /// Get all burden cards in the deck
    /// </summary>
    public List<ConversationCard> GetBurdenCards()
    {
        return cards.Where(c => c.Persistence == PersistenceType.Burden).ToList();
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
    /// Get depth threshold for token count
    /// </summary>
    public static int GetDepthThresholdForTokens(ConnectionType type, int tokenCount)
    {
        // More tokens = access to deeper (higher depth) cards
        return tokenCount switch
        {
            0 => 5,   // No tokens: only basic cards (depth 0-5)
            1 => 10,  // 1 token: access to depth 6-10 cards
            2 => 15,  // 2 tokens: access to depth 11-15 cards
            3 => 20,  // 3 tokens: access to depth 16-20 cards
            4 => 25,  // 4 tokens: access to depth 21-25 cards
            _ => 30   // 5+ tokens: access to all cards
        };
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
            cards.Add(goalCard);
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
        
        var availableCards = cards
            .Where(c => (c.Depth <= currentComfort || c.Depth == 0) && c.Category == category)
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
        
        // If types is null, allow all types
        var availableCards = cards
            .Where(c => c.Depth <= currentComfort || c.Depth == 0)
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
            Persistence = PersistenceType.Crisis,
            Weight = 0, // Free in crisis
            BaseComfort = 8,
            Category = CardCategory.CRISIS,
            DisplayName = "Crisis Moment",
            Description = $"{npc.Name} needs immediate help!",
            SuccessRate = 35
        };
    }

    /// <summary>
    /// Get all cards for an NPC's personality from GameWorld templates
    /// </summary>
    private List<ConversationCard> GetCardsForNPC(NPC npc, Dictionary<ConnectionType, int> tokens)
    {
        var cards = new List<ConversationCard>();

        // Add universal cards from GameWorld
        var universalCards = _gameWorld.CardTemplates.Values
            .Where(c => string.IsNullOrEmpty(c.ForNPC) || c.ForNPC == npc.ID)
            .Where(c => c.Category != CardCategory.GOAL && c.Category != CardCategory.CRISIS);

        foreach (var cardDto in universalCards)
        {
            cards.Add(ConvertDTOToCard(cardDto, npc));
        }

        // Add personality-specific cards from GameWorld
        if (_gameWorld.PersonalityMappings.TryGetValue(npc.PersonalityType, out var mapping))
        {
            foreach (var cardId in mapping.Cards)
            {
                if (_gameWorld.CardTemplates.TryGetValue(cardId, out var cardDto))
                {
                    cards.Add(ConvertDTOToCard(cardDto, npc));
                }
            }
        }

        // Add token-unlocked cards if applicable
        if (tokens != null && tokens.Any())
        {
            foreach (var kvp in _gameWorld.TokenUnlocks)
            {
                var requiredTokens = kvp.Key;
                var unlockedCardIds = kvp.Value;
                
                // Check if player has enough tokens
                var totalTokens = tokens.Values.Sum();
                if (totalTokens >= requiredTokens)
                {
                    foreach (var cardId in unlockedCardIds)
                    {
                        if (_gameWorld.CardTemplates.TryGetValue(cardId, out var cardDto))
                        {
                            cards.Add(ConvertDTOToCard(cardDto, npc));
                        }
                    }
                }
            }
        }

        return cards;
    }

    /// <summary>
    /// Get crisis cards for an NPC from GameWorld templates
    /// </summary>
    private List<ConversationCard> GetCrisisCards(NPC npc)
    {
        var cards = new List<ConversationCard>();
        
        var crisisCards = _gameWorld.CardTemplates.Values
            .Where(c => c.Category == CardCategory.CRISIS)
            .Where(c => string.IsNullOrEmpty(c.ForNPC) || c.ForNPC == npc.ID);

        foreach (var cardDto in crisisCards)
        {
            cards.Add(ConvertDTOToCard(cardDto, npc));
        }

        return cards;
    }

    /// <summary>
    /// Convert a DTO from GameWorld into a ConversationCard
    /// </summary>
    private ConversationCard ConvertDTOToCard(ConversationCardDTO dto, NPC npc)
    {
        // Parse template
        CardTemplateType template = CardTemplateType.SimpleGreeting;
        if (!string.IsNullOrEmpty(dto.Template))
        {
            Enum.TryParse<CardTemplateType>(dto.Template, true, out template);
        }

        // Parse goal type if goal card
        GoalType? goalType = null;
        if (dto.IsGoalCard == true && !string.IsNullOrEmpty(dto.GoalCardType))
        {
            if (Enum.TryParse<GoalType>(dto.GoalCardType, true, out var parsed))
            {
                goalType = parsed;
            }
        }

        // Parse success state for state cards
        EmotionalState? successState = null;
        if (dto.IsStateCard == true && !string.IsNullOrEmpty(dto.SuccessState))
        {
            if (Enum.TryParse<EmotionalState>(dto.SuccessState, true, out var parsed))
            {
                successState = parsed;
            }
        }

        // Create card with all init-only properties set at once
        return new ConversationCard
        {
            Id = dto.Id,
            Template = template,
            Context = new CardContext
            {
                Personality = npc?.PersonalityType ?? PersonalityType.STEADFAST,
                EmotionalState = npc?.CurrentEmotionalState ?? EmotionalState.NEUTRAL,
                NPCName = npc?.Name,
                GeneratesLetterOnSuccess = dto.GeneratesLetterOnSuccess ?? false
            },
            Type = Enum.Parse<CardType>(dto.Type, true),
            Persistence = Enum.Parse<PersistenceType>(dto.Persistence, true),
            Weight = dto.Weight,
            BaseComfort = dto.BaseComfort,
            Depth = dto.Depth ?? 0,
            Category = dto.Category,
            IsGoalCard = dto.IsGoalCard ?? false,
            GoalCardType = goalType,
            DisplayName = dto.DisplayName,
            Description = dto.Description,
            SuccessRate = dto.SuccessRate ?? 0,
            SuccessState = successState
        };
    }
}