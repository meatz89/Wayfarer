using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages all card deck operations for the new conversation system.
/// Handles single-card SPEAK mechanics, focus management, and dice roll success.
/// </summary>
public class CardDeckManager
{
    private readonly GameWorld _gameWorld;
    private readonly Random _random;
    private readonly CardEffectProcessor _effectProcessor;
    private readonly FocusManager _focusManager;
    private readonly AtmosphereManager _atmosphereManager;
    private readonly List<CardInstance> _exhaustedPile = new();

    public CardDeckManager(GameWorld gameWorld, CardEffectProcessor effectProcessor,
        FocusManager focusManager, AtmosphereManager atmosphereManager)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _effectProcessor = effectProcessor ?? throw new ArgumentNullException(nameof(effectProcessor));
        _focusManager = focusManager ?? throw new ArgumentNullException(nameof(focusManager));
        _atmosphereManager = atmosphereManager ?? throw new ArgumentNullException(nameof(atmosphereManager));
        _random = new Random();
    }

    /// <summary>
    /// Create a conversation deck from NPC templates (no filtering by type)
    /// Returns both the deck and any request cards that should start in hand
    /// </summary>
    public (SessionCardDeck deck, CardInstance requestCard) CreateConversationDeck(NPC npc, ConversationType conversationType, List<CardInstance> observationCards = null)
    {
        string sessionId = Guid.NewGuid().ToString();
        
        // Use ExchangeDeck for Commerce conversations, ConversationDeck for others
        List<ConversationCard> cardTemplates;
        if (conversationType == ConversationType.Commerce && npc.ExchangeDeck != null && npc.ExchangeDeck.Any())
        {
            cardTemplates = npc.ExchangeDeck.GetAllCards();
        }
        else
        {
            cardTemplates = npc.ConversationDeck.GetAllCards();
        }
        
        SessionCardDeck deck = SessionCardDeck.CreateFromTemplates(cardTemplates, sessionId);

        // Add observation cards if provided
        if (observationCards != null && observationCards.Any())
        {
            foreach (CardInstance card in observationCards)
            {
                deck.AddCard(card);
            }
        }

        // Get request card but don't add to deck - it goes directly to hand
        CardInstance requestCard = null;
        if (conversationType == ConversationType.Promise || conversationType == ConversationType.Resolution)
        {
            requestCard = SelectValidRequestCard(npc, conversationType);
        }

        return (deck, requestCard);
    }

    /// <summary>
    /// Draw cards based on emotional state (no type filtering)
    /// </summary>
    public List<CardInstance> DrawCards(SessionCardDeck deck, int count)
    {
        return deck.DrawCards(count);
    }

    /// <summary>
    /// Play a single card with dice roll and focus management
    /// </summary>
    public CardPlayResult PlayCard(ConversationSession session, CardInstance selectedCard)
    {
        // Check if card is unplayable
        if (selectedCard.Properties.Contains(CardProperty.Unplayable))
        {
            return new CardPlayResult
            {
                Results = new List<SingleCardResult>
                {
                    new SingleCardResult
                    {
                        Card = selectedCard,
                        Success = false,
                        Flow = 0,
                        Roll = 0,
                        SuccessChance = 0
                    }
                },
                TotalFlow = 0
            };
        }

        // Check for free focus from observation effect
        int focusCost = _atmosphereManager.IsNextSpeakFree() ? 0 : selectedCard.Focus;
        
        // Validate focus availability
        if (!_focusManager.CanAffordCard(focusCost))
        {
            return new CardPlayResult
            {
                Results = new List<SingleCardResult>
                {
                    new SingleCardResult
                    {
                        Card = selectedCard,
                        Success = false,
                        Flow = 0,
                        Roll = 0,
                        SuccessChance = 0
                    }
                },
                TotalFlow = 0
            };
        }

        // Calculate success percentage
        ConversationCard card = ConvertToNewCard(selectedCard);
        int successPercentage = _effectProcessor.CalculateSuccessPercentage(card, session);

        // Roll for success
        bool success = _effectProcessor.RollForSuccess(successPercentage);
        int roll = _random.Next(1, 101);

        // Spend focus (possibly 0 if free) - focus represents effort of speaking
        _focusManager.SpendFocus(focusCost);

        CardEffectResult effectResult = null;
        int flowChange = 0;

        if (success)
        {
            // Success always gives +1 to flow
            flowChange = 1;
            
            // Process card's success effect
            effectResult = _effectProcessor.ProcessSuccessEffect(card, session);
            
            // Apply rapport changes to RapportManager
            if (effectResult.RapportChange != 0 && session.RapportManager != null)
            {
                session.RapportManager.ApplyRapportChange(effectResult.RapportChange, session.CurrentAtmosphere);
            }

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
            // Failure always gives -1 to flow
            flowChange = -1;
            
            // Process card's failure effect
            effectResult = _effectProcessor.ProcessFailureEffect(card, session);
            
            // Apply rapport changes to RapportManager (if any failure effects modify rapport)
            if (effectResult.RapportChange != 0 && session.RapportManager != null)
            {
                session.RapportManager.ApplyRapportChange(effectResult.RapportChange, session.CurrentAtmosphere);
            }
            
            // Add cards from failure effect (e.g. burden cards)
            if (effectResult.CardsToAdd.Any())
            {
                session.Hand.AddCards(effectResult.CardsToAdd);
            }
            
            // Clear atmosphere on failure
            _atmosphereManager.ClearAtmosphereOnFailure();
        }

        // Remove the played card from hand (it was played, so it leaves the hand)
        session.Hand.RemoveCard(selectedCard);
        session.PlayedCards.Add(selectedCard);
        
        // Remove impulse cards from hand after SPEAK, executing exhaust effects
        bool conversationContinues = RemoveImpulseCardsFromHand(session);

        CardPlayResult result = new CardPlayResult
        {
            Results = new List<SingleCardResult>
            {
                new SingleCardResult
                {
                    Card = selectedCard,
                    Success = success,
                    Flow = flowChange,
                    Roll = roll,
                    SuccessChance = successPercentage
                }
            },
            TotalFlow = flowChange
        };

        // Handle exhaust ending conversation
        if (!conversationContinues)
        {
            result.Success = false; // Override to mark conversation as failed
        }

        return result;
    }

    /// <summary>
    /// Execute LISTEN action - refresh focus, draw cards, and exhaust opening cards
    /// </summary>
    public List<CardInstance> ExecuteListen(ConversationSession session)
    {
        // First, exhaust all Opening cards in hand
        if (!ExhaustOpeningCards(session))
        {
            // Exhaust effect ended conversation
            return new List<CardInstance>();
        }

        // Refresh focus
        _focusManager.RefreshPool();

        // Calculate draw count based on state and atmosphere
        int drawCount = session.GetDrawCount();

        // Draw cards (no type filtering)
        List<CardInstance> drawnCards = session.Deck.DrawCards(drawCount);

        // Add to hand
        session.Hand.AddCards(drawnCards);

        // Check if any request cards should become playable
        UpdateRequestCardPlayability(session);

        return drawnCards;
    }

    /// <summary>
    /// Check if request cards should become playable based on focus capacity
    /// </summary>
    private void UpdateRequestCardPlayability(ConversationSession session)
    {
        // Get current focus capacity
        int focusCapacity = _focusManager.CurrentCapacity;

        // Find all request cards in hand
        List<CardInstance> requestCards = session.Hand.Cards
            .Where(c => c.Properties.Contains(CardProperty.Unplayable))
            .ToList();

        foreach (var requestCard in requestCards)
        {
            // Check if we have enough focus capacity for this request card
            if (focusCapacity >= requestCard.Focus)
            {
                // Remove Unplayable and add Impulse + Opening properties
                requestCard.Properties.Remove(CardProperty.Unplayable);
                
                // Add Impulse and Opening if not already present
                if (!requestCard.Properties.Contains(CardProperty.Impulse))
                    requestCard.Properties.Add(CardProperty.Impulse);
                if (!requestCard.Properties.Contains(CardProperty.Opening))
                    requestCard.Properties.Add(CardProperty.Opening);
                
                // Mark that the request card is now playable
                session.RequestCardDrawn = true;
            }
        }
    }

    /// <summary>
    /// Update all cards' playability based on current focus availability
    /// Cards that cost more focus than available are marked Unplayable
    /// </summary>
    public void UpdateCardPlayabilityBasedOnFocus(ConversationSession session)
    {
        int availableFocus = _focusManager.AvailableFocus;
        
        // Check if next speak is free (from observation effect)
        bool isNextSpeakFree = _atmosphereManager.IsNextSpeakFree();

        foreach (var card in session.Hand.Cards)
        {
            // Calculate effective focus cost for this card
            int effectiveFocusCost = isNextSpeakFree ? 0 : card.Focus;
            
            // Check if we can afford this card
            bool canAfford = _focusManager.CanAffordCard(effectiveFocusCost);
            
            // Update playability
            if (!canAfford && !card.Properties.Contains(CardProperty.Unplayable))
            {
                // Mark as unplayable if we can't afford it
                card.Properties.Add(CardProperty.Unplayable);
            }
            else if (canAfford && card.Properties.Contains(CardProperty.Unplayable))
            {
                // Only remove Unplayable if this isn't a request card waiting for focus threshold
                // Request cards should only become playable through UpdateRequestCardPlayability
                ConversationCard conversationCard = ConvertToNewCard(card);
                bool isRequestCard = conversationCard.IsRequest;
                
                if (!isRequestCard)
                {
                    // Regular card that was unplayable due to focus cost can now be played
                    card.Properties.Remove(CardProperty.Unplayable);
                }
            }
        }
    }

    /// <summary>
    /// Remove all impulse cards from hand (happens after every SPEAK)
    /// Executes exhaust effects before removing cards
    /// </summary>
    private bool RemoveImpulseCardsFromHand(ConversationSession session)
    {
        // Get all impulse cards (including requests with both Impulse + Opening)
        List<CardInstance> impulseCards = session.Hand.Cards.Where(c => c.Properties.Contains(CardProperty.Impulse)).ToList();
        
        foreach (var card in impulseCards)
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
    /// Exhaust all opening cards in hand (happens on LISTEN)
    /// </summary>
    private bool ExhaustOpeningCards(ConversationSession session)
    {
        // Get all opening cards (including requests with both Impulse + Opening)
        List<CardInstance> openingCards = session.Hand.Cards
            .Where(c => IsOpeningCard(c))
            .ToList();
        
        foreach (var card in openingCards)
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
    /// Check if a card has the Opening property
    /// </summary>
    private bool IsOpeningCard(CardInstance card)
    {
        var conversationCard = ConvertToNewCard(card);
        return conversationCard.IsOpening;
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
                // Request cards typically have this - conversation ends in failure
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

            case CardEffectType.AddRapport:
                if (int.TryParse(card.ExhaustEffect.Value, out int flow))
                {
                    session.FlowBattery += flow;
                    session.FlowBattery = Math.Clamp(session.FlowBattery, -3, 3);
                }
                return true;

            case CardEffectType.AddFocus:
                if (int.TryParse(card.ExhaustEffect.Value, out int focus))
                {
                    _focusManager.AddFocus(focus);
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
    /// Validate if a card can be played (only focus check now)
    /// </summary>
    public bool CanPlayCard(CardInstance card, ConversationSession session)
    {
        // Only check focus availability - no state restrictions
        return _focusManager.CanAffordCard(card.Focus);
    }

    /// <summary>
    /// Select a valid request card for conversation type
    /// </summary>
    private CardInstance SelectValidRequestCard(NPC npc, ConversationType conversationType)
    {
        if (npc.RequestDeck == null || !npc.RequestDeck.HasCardsAvailable())
            return null;

        List<ConversationCard> requestCards = npc.RequestDeck.GetAllCards()
            .Where(card => IsRequestCardValidForConversation(card, conversationType))
            .ToList();

        if (!requestCards.Any())
            return null;

        ConversationCard selectedRequest = requestCards[_random.Next(requestCards.Count)];
        return new CardInstance(selectedRequest, npc.ID);
    }

    /// <summary>
    /// Check if request card is valid for conversation type
    /// </summary>
    private bool IsRequestCardValidForConversation(ConversationCard card, ConversationType type)
    {
        // Implementation depends on how request cards are categorized
        // For now, allow all request cards in any conversation
        return card.IsRequest;
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
            Description = instance.Description,
            Properties = new List<CardProperty>(instance.Properties),
            TokenType = instance.TokenType,
            Focus = instance.Focus,
            Difficulty = instance.Difficulty,
            SuccessEffect = instance.SuccessEffect ?? CardEffect.None,
            FailureEffect = instance.FailureEffect ?? CardEffect.None,
            ExhaustEffect = instance.ExhaustEffect ?? CardEffect.None,
            DialogueFragment = instance.DialogueFragment,
            VerbPhrase = instance.VerbPhrase
        };
        
        // Ensure request cards have proper exhaust effect
        if (card.IsRequest && (card.ExhaustEffect == null || card.ExhaustEffect.Type == CardEffectType.None))
        {
            card.ExhaustEffect = new CardEffect
            {
                Type = CardEffectType.EndConversation,
                Value = "request_exhausted",
                Data = new Dictionary<string, object>
                {
                    { "reason", "Request card exhausted without being played" }
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
            Description = "Placeholder card",
            Focus = 1,
            Difficulty = Difficulty.Medium,
            TokenType = TokenType.Trust,
            SuccessEffect = new CardEffect { Type = CardEffectType.AddRapport, Value = "1" },
            FailureEffect = CardEffect.None,
            ExhaustEffect = CardEffect.None
        };
    }
}