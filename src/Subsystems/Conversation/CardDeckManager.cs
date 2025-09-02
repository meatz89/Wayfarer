using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages all card deck operations for the new conversation system.
/// Handles single-card SPEAK mechanics, weight pool management, and dice roll success.
/// </summary>
public class CardDeckManager
{
    private readonly GameWorld _gameWorld;
    private readonly Random _random;
    private readonly CardEffectProcessor _effectProcessor;
    private readonly WeightPoolManager _weightPoolManager;
    private readonly AtmosphereManager _atmosphereManager;
    private readonly List<CardInstance> _exhaustedPile = new();

    public CardDeckManager(GameWorld gameWorld, CardEffectProcessor effectProcessor,
        WeightPoolManager weightPoolManager, AtmosphereManager atmosphereManager)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _effectProcessor = effectProcessor ?? throw new ArgumentNullException(nameof(effectProcessor));
        _weightPoolManager = weightPoolManager ?? throw new ArgumentNullException(nameof(weightPoolManager));
        _atmosphereManager = atmosphereManager ?? throw new ArgumentNullException(nameof(atmosphereManager));
        _random = new Random();
    }

    /// <summary>
    /// Create a conversation deck from NPC templates (no filtering by type)
    /// </summary>
    public SessionCardDeck CreateConversationDeck(NPC npc, ConversationType conversationType, List<CardInstance> observationCards = null)
    {
        string sessionId = Guid.NewGuid().ToString();
        SessionCardDeck deck = SessionCardDeck.CreateFromTemplates(npc.ConversationDeck.GetAllCards(), sessionId);

        // Add observation cards if provided
        if (observationCards != null && observationCards.Any())
        {
            foreach (CardInstance card in observationCards)
            {
                deck.AddCard(card);
            }
        }

        // Add goal cards based on conversation type
        if (conversationType == ConversationType.Promise || conversationType == ConversationType.Resolution)
        {
            CardInstance goalCard = SelectValidGoalCard(npc, conversationType);
            if (goalCard != null)
            {
                deck.AddCard(goalCard);
            }
        }

        return deck;
    }

    /// <summary>
    /// Draw cards based on emotional state (no type filtering)
    /// </summary>
    public List<CardInstance> DrawCards(SessionCardDeck deck, int count)
    {
        return deck.DrawCards(count);
    }

    /// <summary>
    /// Play a single card with dice roll and weight management
    /// </summary>
    public CardPlayResult PlayCard(ConversationSession session, CardInstance selectedCard)
    {
        // Check for free weight from observation effect
        int weightCost = _atmosphereManager.IsNextSpeakFree() ? 0 : selectedCard.Weight;
        
        // Validate weight availability
        if (!_weightPoolManager.CanAffordCard(weightCost))
        {
            return new CardPlayResult
            {
                Results = new List<SingleCardResult>
                {
                    new SingleCardResult
                    {
                        Card = selectedCard,
                        Success = false,
                        Comfort = 0,
                        Roll = 0,
                        SuccessChance = 0
                    }
                },
                TotalComfort = 0
            };
        }

        // Calculate success percentage
        ConversationCard card = ConvertToNewCard(selectedCard);
        int successPercentage = _effectProcessor.CalculateSuccessPercentage(card, session);

        // Roll for success
        bool success = _effectProcessor.RollForSuccess(successPercentage);
        int roll = _random.Next(1, 101);

        // Spend weight (possibly 0 if free) - weight represents effort of speaking
        _weightPoolManager.SpendWeight(weightCost);

        CardEffectResult effectResult = null;
        int comfortChange = 0;

        if (success)
        {
            // Process card's success effect
            effectResult = _effectProcessor.ProcessSuccessEffect(card, session);
            comfortChange = effectResult.ComfortChange;

            // Add drawn cards to hand
            if (effectResult.CardsToAdd.Any())
            {
                session.Hand.AddCards(effectResult.CardsToAdd);
            }

            // Handle atmosphere change
            if (effectResult.AtmosphereTypeChange.HasValue)
            {
                _atmosphereManager.SetAtmosphere(effectResult.AtmosphereTypeChange.Value);
            }
            
            // Consume one-time atmosphere effects after successful card play
            _atmosphereManager.OnCardSuccess();
        }
        else
        {
            // Process card's failure effect
            effectResult = _effectProcessor.ProcessFailureEffect(card, session);
            comfortChange = effectResult.ComfortChange;
            
            // Clear atmosphere on failure
            _atmosphereManager.ClearAtmosphereOnFailure();

            // Check if failure should end conversation (Final atmosphere)
            if (_atmosphereManager.ShouldEndOnFailure())
            {
                // This will be handled by the orchestrator
            }
        }

        // Remove fleeting cards from hand after SPEAK, executing exhaust effects
        bool conversationContinues = RemoveFleetingCardsFromHand(session);

        CardPlayResult result = new CardPlayResult
        {
            Results = new List<SingleCardResult>
            {
                new SingleCardResult
                {
                    Card = selectedCard,
                    Success = success,
                    Comfort = comfortChange,
                    Roll = roll,
                    SuccessChance = successPercentage
                }
            },
            TotalComfort = comfortChange
        };

        // Handle exhaust ending conversation
        if (!conversationContinues)
        {
            result.Success = false; // Override to mark conversation as failed
        }

        return result;
    }

    /// <summary>
    /// Execute LISTEN action - refresh weight pool, draw cards, and exhaust opportunity cards
    /// </summary>
    public List<CardInstance> ExecuteListen(ConversationSession session)
    {
        // First, exhaust all Opportunity cards in hand
        if (!ExhaustOpportunityCards(session))
        {
            // Exhaust effect ended conversation
            return new List<CardInstance>();
        }

        // Refresh weight pool
        _weightPoolManager.RefreshPool();

        // Calculate draw count based on state and atmosphere
        int drawCount = session.GetDrawCount();

        // Draw cards (no type filtering)
        List<CardInstance> drawnCards = session.Deck.DrawCards(drawCount);

        // Add to hand
        session.Hand.AddCards(drawnCards);

        return drawnCards;
    }

    /// <summary>
    /// Remove all fleeting cards from hand (happens after every SPEAK)
    /// Executes exhaust effects before removing cards
    /// </summary>
    private bool RemoveFleetingCardsFromHand(ConversationSession session)
    {
        // Get all fleeting cards (including goals with both Fleeting + Opportunity)
        List<CardInstance> fleetingCards = session.Hand.Cards.Where(c => c.IsFleeting).ToList();
        
        foreach (var card in fleetingCards)
        {
            var conversationCard = ConvertToNewCard(card);
            
            // Execute exhaust effect if it exists
            if (conversationCard.ExhaustEffect?.Type != CardEffectType.None)
            {
                if (!ExecuteExhaustEffect(conversationCard, session))
                {
                    // Exhaust effect ended conversation
                    return false;
                }
            }
            
            // Remove from hand and add to exhausted pile
            session.Hand.RemoveCard(card);
            _exhaustedPile.Add(card);
        }
        
        return true; // Conversation continues
    }

    /// <summary>
    /// Exhaust all opportunity cards in hand (happens on LISTEN)
    /// </summary>
    private bool ExhaustOpportunityCards(ConversationSession session)
    {
        // Get all opportunity cards (including goals with both Fleeting + Opportunity)
        List<CardInstance> opportunityCards = session.Hand.Cards
            .Where(c => IsOpportunityCard(c))
            .ToList();
        
        foreach (var card in opportunityCards)
        {
            var conversationCard = ConvertToNewCard(card);
            
            // Execute exhaust effect if it exists
            if (conversationCard.ExhaustEffect?.Type != CardEffectType.None)
            {
                if (!ExecuteExhaustEffect(conversationCard, session))
                {
                    // Exhaust effect ended conversation
                    return false;
                }
            }
            
            // Remove from hand and add to exhausted pile
            session.Hand.RemoveCard(card);
            _exhaustedPile.Add(card);
        }
        
        return true; // Conversation continues
    }

    /// <summary>
    /// Check if a card has the Opportunity property
    /// </summary>
    private bool IsOpportunityCard(CardInstance card)
    {
        var conversationCard = ConvertToNewCard(card);
        return conversationCard.IsOpportunity;
    }

    /// <summary>
    /// Execute a card's exhaust effect
    /// </summary>
    private bool ExecuteExhaustEffect(ConversationCard card, ConversationSession session)
    {
        if (card.ExhaustEffect == null || card.ExhaustEffect.Type == CardEffectType.None)
            return true; // No exhaust effect, conversation continues

        switch (card.ExhaustEffect.Type)
        {
            case CardEffectType.EndConversation:
                // Goal cards typically have this - conversation ends in failure
                // The orchestrator will handle the actual ending
                return false; // Signal conversation should end

            case CardEffectType.SetAtmosphere:
                if (Enum.TryParse<AtmosphereType>(card.ExhaustEffect.Value, out var atmosphere))
                {
                    _atmosphereManager.SetAtmosphere(atmosphere);
                }
                return true;

            case CardEffectType.DrawCards:
                if (int.TryParse(card.ExhaustEffect.Value, out int count))
                {
                    var drawnCards = session.Deck.DrawCards(count);
                    session.Hand.AddCards(drawnCards);
                }
                return true;

            case CardEffectType.AddComfort:
                if (int.TryParse(card.ExhaustEffect.Value, out int comfort))
                {
                    session.ComfortBattery += comfort;
                    session.ComfortBattery = Math.Clamp(session.ComfortBattery, -3, 3);
                }
                return true;

            case CardEffectType.AddWeight:
                if (int.TryParse(card.ExhaustEffect.Value, out int weight))
                {
                    _weightPoolManager.AddWeight(weight);
                }
                return true;

            default:
                // Unknown exhaust effect, log and continue
                Console.WriteLine($"[CardDeckManager] Unknown exhaust effect type: {card.ExhaustEffect.Type}");
                return true;
        }
    }

    /// <summary>
    /// Get all exhausted cards (separate from discard)
    /// </summary>
    public IReadOnlyList<CardInstance> GetExhaustedCards()
    {
        return _exhaustedPile.AsReadOnly();
    }

    /// <summary>
    /// Validate if a card can be played (only weight check now)
    /// </summary>
    public bool CanPlayCard(CardInstance card, ConversationSession session)
    {
        // Only check weight availability - no state restrictions
        return _weightPoolManager.CanAffordCard(card.Weight);
    }

    /// <summary>
    /// Select a valid goal card for conversation type
    /// </summary>
    private CardInstance SelectValidGoalCard(NPC npc, ConversationType conversationType)
    {
        if (npc.GoalDeck == null || !npc.GoalDeck.HasCardsAvailable())
            return null;

        List<ConversationCard> goalCards = npc.GoalDeck.GetAllCards()
            .Where(card => IsGoalCardValidForConversation(card, conversationType))
            .ToList();

        if (!goalCards.Any())
            return null;

        ConversationCard selectedGoal = goalCards[_random.Next(goalCards.Count)];
        return new CardInstance(selectedGoal, npc.ID);
    }

    /// <summary>
    /// Check if goal card is valid for conversation type
    /// </summary>
    private bool IsGoalCardValidForConversation(ConversationCard card, ConversationType type)
    {
        // Implementation depends on how goal cards are categorized
        // For now, allow all goal cards in any conversation
        return card.IsGoal;
    }

    /// <summary>
    /// Convert CardInstance to ConversationCard format
    /// </summary>
    private ConversationCard ConvertToNewCard(CardInstance instance)
    {
        // Since CardInstance now mirrors ConversationCard structure,
        // we can create a direct mapping
        var card = new ConversationCard
        {
            Id = instance.Id,
            Name = instance.Name,
            Description = instance.Description,
            Properties = new List<CardProperty>(instance.Properties),
            TokenType = instance.TokenType,
            Weight = instance.Weight,
            Difficulty = instance.Difficulty,
            SuccessEffect = instance.SuccessEffect ?? CardEffect.None,
            FailureEffect = instance.FailureEffect ?? CardEffect.None,
            ExhaustEffect = instance.ExhaustEffect ?? CardEffect.None,
            DialogueFragment = instance.DialogueFragment,
            VerbPhrase = instance.VerbPhrase
        };
        
        // Ensure goal cards have proper exhaust effect
        if (card.IsGoal && (card.ExhaustEffect == null || card.ExhaustEffect.Type == CardEffectType.None))
        {
            card.ExhaustEffect = new CardEffect
            {
                Type = CardEffectType.EndConversation,
                Value = "goal_exhausted",
                Data = new Dictionary<string, object>
                {
                    { "reason", "Goal card exhausted without being played" }
                }
            };
        }
        
        return card;
    }

    /// <summary>
    /// Convert legacy success chance to new Difficulty enum
    /// </summary>
    private Difficulty ConvertToDifficulty(int successChance)
    {
        return successChance switch
        {
            >= 70 => Difficulty.Easy,
            >= 60 => Difficulty.Medium,
            >= 50 => Difficulty.Hard,
            _ => Difficulty.VeryHard
        };
    }


    /// <summary>
    /// Create exchange deck for commerce conversations
    /// </summary>
    public List<CardInstance> CreateExchangeDeck(NPC npc, List<string> domainTags)
    {
        List<CardInstance> exchangeCards = new List<CardInstance>();

        if (npc.ExchangeDeck != null)
        {
            foreach (ConversationCard template in npc.ExchangeDeck.GetAllCards())
            {
                exchangeCards.Add(new CardInstance(template, npc.ID));
            }
        }

        return exchangeCards;
    }


    /// <summary>
    /// Restore cards to a session from saved IDs
    /// </summary>
    public void RestoreSessionCards(ConversationSession session, List<string> handCardIds, List<string> deckCardIds)
    {
        // Clear existing cards
        session.Hand.Clear();
        session.Deck.Clear();

        // Restore hand cards
        foreach (string cardId in handCardIds)
        {
            ConversationCard cardTemplate = FindCardTemplateById(cardId);
            if (cardTemplate != null)
            {
                CardInstance cardInstance = new CardInstance(cardTemplate, session.NPC?.ID ?? "unknown");
                session.Hand.AddCard(cardInstance);
            }
        }

        // Restore deck cards
        foreach (string cardId in deckCardIds)
        {
            ConversationCard cardTemplate = FindCardTemplateById(cardId);
            if (cardTemplate != null)
            {
                CardInstance cardInstance = new CardInstance(cardTemplate, session.NPC?.ID ?? "unknown");
                session.Deck.AddCard(cardInstance);
            }
        }
    }

    /// <summary>
    /// Find a card template by ID from game world data
    /// </summary>
    private ConversationCard FindCardTemplateById(string cardId)
    {
        // Search in all card collections in GameWorld
        if (_gameWorld.AllCardDefinitions.TryGetValue(cardId, out ConversationCard? template))
        {
            return template;
        }

        // If not found in main templates, create a basic one
        return new ConversationCard
        {
            Id = cardId,
            Name = "Unknown Card",
            Description = "Placeholder card",
            Weight = 1,
            Difficulty = Difficulty.Medium,
            TokenType = TokenType.Trust,
            SuccessEffect = new CardEffect { Type = CardEffectType.AddComfort, Value = "1" },
            FailureEffect = CardEffect.None,
            ExhaustEffect = CardEffect.None
        };
    }
}