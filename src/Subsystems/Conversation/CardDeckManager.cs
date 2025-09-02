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
        // Validate weight availability
        if (!_weightPoolManager.CanAffordCard(selectedCard.Weight))
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

        // Spend weight regardless of success - weight represents effort of speaking
        _weightPoolManager.SpendWeight(selectedCard.Weight);

        CardEffectResult effectResult = null;
        int comfortChange = 0;

        if (success)
        {

            // Process card effect
            effectResult = _effectProcessor.ProcessCardEffect(card, session);
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
        }
        else
        {
            // Clear atmosphere on failure
            _atmosphereManager.ClearAtmosphereOnFailure();

            // Check if failure should end conversation (Final atmosphere)
            if (_atmosphereManager.ShouldEndOnFailure())
            {
                // This will be handled by the orchestrator
            }
        }

        // Remove fleeting cards from hand after SPEAK
        RemoveFleetingCardsFromHand(session.Hand);

        // Check Final Word for unplayed goal cards
        bool finalWordTriggered = CheckFinalWordFailure(session.Hand);

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

        // Handle Final Word failure
        if (finalWordTriggered)
        {
            result.Success = false; // Override to mark conversation as failed
        }

        return result;
    }

    /// <summary>
    /// Execute LISTEN action - refresh weight pool and draw cards
    /// </summary>
    public List<CardInstance> ExecuteListen(ConversationSession session)
    {
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
    /// </summary>
    private void RemoveFleetingCardsFromHand(HandDeck hand)
    {
        List<CardInstance> fleetingCards = hand.Cards.Where(c => c.IsFleeting).ToList();
        hand.RemoveCards(fleetingCards);
    }

    /// <summary>
    /// Check if any fleeting goal cards with Final Word are being discarded
    /// </summary>
    private bool CheckFinalWordFailure(HandDeck hand)
    {
        return hand.Cards.Any(c => c.IsGoalCard && c.IsFleeting && ConvertToNewCard(c).HasFinalWord);
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
        return card.IsGoalCard;
    }

    /// <summary>
    /// Convert legacy CardInstance to new ConversationCard format
    /// This is a compatibility method during transition
    /// </summary>
    private ConversationCard ConvertToNewCard(CardInstance instance)
    {
        // This is a temporary conversion method
        // In the full implementation, cards would be loaded with new format
        return new ConversationCard
        {
            Id = instance.Id,
            Name = instance.Name,
            Type = instance.IsGoalCard ? CardType.Goal :
                   instance.IsObservation ? CardType.Observation : CardType.Normal,
            TokenType = ConversationCard.ConvertConnectionToToken(instance.TokenType),
            ConnectionType = instance.TokenType,
            Weight = instance.Weight,
            Difficulty = ConversationCard.ConvertDifficulty(ConvertToDifficulty(instance.BaseSuccessChance)),
            Difficulty_Legacy = ConvertToDifficulty(instance.BaseSuccessChance),
            Persistence = instance.Persistence,
            EffectType = GuessEffectType(instance),
            EffectValue = int.Parse(GuessEffectValue(instance)),
            EffectFormula = GuessEffectValue(instance),
            AtmosphereChange = null, // Would need to be set from JSON
            HasFinalWord = instance.IsGoalCard,
            DialogueText = instance.DialogueFragment
        };
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
    /// Guess effect type from legacy card properties
    /// </summary>
    private CardEffectType GuessEffectType(CardInstance instance)
    {
        if (instance.IsObservation)
            return CardEffectType.FixedComfort; // Default observation effect

        return CardEffectType.FixedComfort; // Default to fixed comfort
    }

    /// <summary>
    /// Guess effect value from legacy card properties
    /// </summary>
    private string GuessEffectValue(CardInstance instance)
    {
        if (instance.IsObservation)
            return "0";

        return instance.BaseComfortReward.ToString();
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
    /// Legacy compatibility method - will be removed
    /// </summary>
    public CardPlayResult PlayCards(ConversationSession session, HashSet<CardInstance> selectedCards)
    {
        // Convert to single card play (take first card)
        CardInstance? firstCard = selectedCards.FirstOrDefault();
        if (firstCard == null)
        {
            return new CardPlayResult { Results = new List<SingleCardResult>(), TotalComfort = 0 };
        }

        return PlayCard(session, firstCard);
    }

    /// <summary>
    /// Legacy compatibility method - will be removed  
    /// </summary>
    public bool ValidateCardSelection(HashSet<CardInstance> cards, EmotionalState currentState)
    {
        // In new system, only validate single card weight
        CardInstance? firstCard = cards.FirstOrDefault();
        if (firstCard == null) return false;

        return _weightPoolManager.CanAffordCard(firstCard.Weight);
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
                CardInstance cardInstance = new CardInstance(cardTemplate, session.NPC.ID);
                session.Hand.AddCard(cardInstance);
            }
        }

        // Restore deck cards
        foreach (string cardId in deckCardIds)
        {
            ConversationCard cardTemplate = FindCardTemplateById(cardId);
            if (cardTemplate != null)
            {
                CardInstance cardInstance = new CardInstance(cardTemplate, session.NPC.ID);
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
            Type = CardType.Normal,
            Weight = 1,
            Difficulty = Difficulty.Medium,
            Difficulty_Legacy = global::Difficulty.Medium,
            Persistence = PersistenceType.Fleeting,
            EffectType = CardEffectType.FixedComfort,
            EffectValue = 1,
            EffectFormula = "1"
        };
    }
}