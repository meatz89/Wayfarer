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
    // Removed exhausted pile - now using SessionCardDeck's discard pile

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

        // Get goal card based on conversation type from JSON data
        // The presence of these cards in JSON determines which conversation types are available
        CardInstance goalCard = null;
        if (conversationType != ConversationType.Commerce)
        {
            goalCard = SelectGoalCardForConversationType(npc, conversationType);
            // Don't add to deck - it will be added directly to hand in ConversationOrchestrator
        }

        return (deck, goalCard);
    }

    /// <summary>
    /// Draw cards based on connection state (no type filtering)
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
        // Check if card is unplayable (but skip this check for promise cards which handle rapport separately)
        if (selectedCard.Properties.Contains(CardProperty.Unplayable) &&
            !selectedCard.Properties.Contains(CardProperty.GoalCard))
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
        int successPercentage = _effectProcessor.CalculateSuccessPercentage(selectedCard, session);

        // Promise/request cards (GoalCard) ALWAYS succeed
        bool success;
        int roll;

        if (selectedCard.Properties.Contains(CardProperty.GoalCard))
        {
            // Promise/request cards always succeed (100% success rate)
            success = true;
            roll = 100; // For display purposes
            successPercentage = 100; // Override to show 100% in UI
        }
        else
        {
            // Use pre-rolled value if available, otherwise generate one (shouldn't happen normally)
            roll = selectedCard.Context?.PreRolledValue ?? _random.Next(1, 101);

            // Check success using the pre-rolled value with momentum system
            success = _effectProcessor.CheckSuccessWithPreRoll(roll, successPercentage, session);
        }

        // Spend focus (possibly 0 if free) - focus represents effort of speaking
        _focusManager.SpendFocus(focusCost);

        CardEffectResult effectResult = null;
        int flowChange = 0;

        if (success)
        {
            // Success always gives +1 to flow
            flowChange = 1;

            // Reset hidden momentum on success (bad luck protection resets)
            session.HiddenMomentum = 0;

            // Process card's success effect
            effectResult = _effectProcessor.ProcessSuccessEffect(selectedCard, session);

            // Apply rapport changes to RapportManager
            if (effectResult.RapportChange != 0 && session.RapportManager != null)
            {
                session.RapportManager.ApplyRapportChange(effectResult.RapportChange, session.CurrentAtmosphere);
            }

            // Add drawn cards to active cards
            if (effectResult.CardsToAdd.Any())
            {
                session.ActiveCards.AddRange(effectResult.CardsToAdd);
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

            // Increment hidden momentum for bad luck protection (invisible to player)
            session.HiddenMomentum = Math.Min(session.HiddenMomentum + 1, 4); // Cap at 4 failures

            // Process card's failure effect
            effectResult = _effectProcessor.ProcessFailureEffect(selectedCard, session);

            // Apply rapport changes to RapportManager (if any failure effects modify rapport)
            if (effectResult.RapportChange != 0 && session.RapportManager != null)
            {
                session.RapportManager.ApplyRapportChange(effectResult.RapportChange, session.CurrentAtmosphere);
            }

            // Add cards from failure effect (e.g. burden cards)
            if (effectResult.CardsToAdd.Any())
            {
                session.ActiveCards.AddRange(effectResult.CardsToAdd);
            }

            // Clear atmosphere on failure
            _atmosphereManager.ClearAtmosphereOnFailure();
        }

        // Remove the played card from active cards and move to exhaust pile
        session.ActiveCards.Remove(selectedCard);
        session.PlayedCards.Add(selectedCard);
        session.ExhaustPile.Add(selectedCard);

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

        // Add to active cards
        session.ActiveCards.AddRange(drawnCards);

        // Check if any request cards should become playable
        UpdateRequestCardPlayability(session);

        return drawnCards;
    }

    /// <summary>
    /// Check if request cards should become playable based on focus capacity
    /// </summary>
    public void UpdateRequestCardPlayability(ConversationSession session)
    {
        // Request cards (promise/goal cards) have their playability determined by rapport threshold,
        // NOT by focus. Their playability is checked in the UI based on rapport.
        // We only mark that a request card exists in the hand.

        // Check if there's a request card in active cards (GoalCard property)
        bool hasRequestCard = session.ActiveCards.Cards
            .Any(c => c.Properties.Contains(CardProperty.GoalCard));

        if (hasRequestCard)
        {
            session.RequestCardDrawn = true;
        }

        // DO NOT modify request card properties here - they stay Unplayable until
        // rapport threshold is met, which is checked in ConversationContent.CanSelectCard()
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

        foreach (CardInstance card in session.ActiveCards.Cards)
        {
            // Skip request/promise cards - their playability is based on rapport, not focus
            if (card.Properties.Contains(CardProperty.GoalCard))
            {
                continue; // Don't modify request card playability here
            }

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
                // Regular card that was unplayable due to focus cost can now be played
                card.Properties.Remove(CardProperty.Unplayable);
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
        List<CardInstance> impulseCards = session.ActiveCards.Cards.Where(c => c.Properties.Contains(CardProperty.Impulse)).ToList();

        foreach (CardInstance card in impulseCards)
        {
            // Execute exhaust effect if it exists
            if (card.ExhaustEffect?.Type != CardEffectType.None)
            {
                if (!ExecuteExhaustEffect(card, session))
                {
                    // Exhaust effect ended conversation
                    return false;
                }
            }

            // Remove from active cards and add to exhaust pile
            session.ActiveCards.Remove(card);
            session.ExhaustPile.Add(card);
        }

        return true; // Conversation continues
    }

    /// <summary>
    /// Exhaust all opening cards in hand (happens on LISTEN)
    /// </summary>
    private bool ExhaustOpeningCards(ConversationSession session)
    {
        // Get all opening cards
        List<CardInstance> openingCards = session.ActiveCards.Cards
            .Where(c => IsOpeningCard(c))
            .ToList();

        foreach (CardInstance card in openingCards)
        {
            // Execute exhaust effect if it exists
            if (card.ExhaustEffect?.Type != CardEffectType.None)
            {
                if (!ExecuteExhaustEffect(card, session))
                {
                    // Exhaust effect ended conversation
                    return false;
                }
            }

            // Remove from active cards and add to exhaust pile
            session.ActiveCards.Remove(card);
            session.ExhaustPile.Add(card);
        }

        return true; // Conversation continues
    }

    /// <summary>
    /// Check if a card has the Opening property
    /// </summary>
    private bool IsOpeningCard(CardInstance card)
    {
        return card.Properties.Contains(CardProperty.Opening);
    }

    /// <summary>
    /// Execute a card's exhaust effect
    /// </summary>
    private bool ExecuteExhaustEffect(CardInstance card, ConversationSession session)
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
                if (Enum.TryParse<AtmosphereType>(card.ExhaustEffect.Value, out AtmosphereType atmosphere))
                {
                    _atmosphereManager.SetAtmosphere(atmosphere);
                }
                return true;

            case CardEffectType.DrawCards:
                if (int.TryParse(card.ExhaustEffect.Value, out int count))
                {
                    List<CardInstance> drawnCards = session.Deck.DrawCards(count);
                    session.ActiveCards.AddRange(drawnCards);
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
    /// Get all exhausted cards (now in discard pile)
    /// </summary>
    public IReadOnlyList<CardInstance> GetExhaustedCards()
    {
        // Exhausted cards are now in the discard pile
        return new List<CardInstance>().AsReadOnly();
    }

    /// <summary>
    /// Validate if a card can be played (focus check and rapport for goal cards)
    /// </summary>
    public bool CanPlayCard(CardInstance card, ConversationSession session)
    {
        // Check focus availability
        if (!_focusManager.CanAffordCard(card.Focus))
            return false;

        // Check rapport threshold ONLY for goal cards
        if (card.Properties.Contains(CardProperty.GoalCard))
        {
            // Use the rapport threshold we stored in CardContext
            int rapportThreshold = card.Context?.RapportThreshold ?? 0;
            int currentRapport = session.RapportManager?.CurrentRapport ?? 0;
            return currentRapport >= rapportThreshold;
        }

        return true;
    }

    /// <summary>
    /// Select appropriate goal card from JSON data based on conversation type
    /// </summary>
    private CardInstance SelectGoalCardForConversationType(NPC npc, ConversationType conversationType)
    {
        switch (conversationType)
        {
            case ConversationType.Promise:
                // For Promise conversations, select from NPC's promise/letter cards
                return SelectValidRequestCard(npc, conversationType);
                
            case ConversationType.FriendlyChat:
                // For FriendlyChat, select from NPC's connection token goal cards
                return SelectConnectionTokenGoalCard(npc);
                
            case ConversationType.Delivery:
                // For Delivery, the goal card is generated based on the letter being delivered
                // This is handled separately as it depends on the obligation
                return null; // TODO: Implement delivery goal card selection
                
            case ConversationType.Resolution:
                // For Resolution, select from burden resolution cards
                return SelectBurdenResolutionCard(npc);
                
            default:
                return null;
        }
    }

    /// <summary>
    /// Select a connection token goal card from NPC's goal deck
    /// </summary>
    private CardInstance SelectConnectionTokenGoalCard(NPC npc)
    {
        // Connection token goal cards should be in the NPC's request deck
        // These are cards that grant connection tokens when played at rapport threshold
        if (npc.RequestDeck == null || !npc.RequestDeck.HasCardsAvailable())
            return null;

        List<RequestCard> goalCards = npc.RequestDeck.GetAllCards()
            .OfType<RequestCard>()
            .Where(card => card.GoalType == "FriendlyChat")
            .ToList();

        if (!goalCards.Any())
            return null;

        RequestCard selectedGoal = goalCards[_random.Next(goalCards.Count)];
        CardInstance goalInstance = new CardInstance(selectedGoal, npc.ID);

        // Store the rapport threshold in the card context (same as Elena's letter)
        if (goalInstance.Context == null)
            goalInstance.Context = new CardContext();
        
        // Use the rapport threshold from the card itself (from JSON)
        goalInstance.Context.RapportThreshold = selectedGoal.RapportThreshold;
        
        return goalInstance;
    }

    /// <summary>
    /// Select a burden resolution card from NPC's goal deck
    /// </summary>
    private CardInstance SelectBurdenResolutionCard(NPC npc)
    {
        // Burden resolution cards should be in the NPC's request deck
        if (npc.RequestDeck == null || !npc.RequestDeck.HasCardsAvailable())
            return null;

        List<RequestCard> resolutionCards = npc.RequestDeck.GetAllCards()
            .OfType<RequestCard>()
            .Where(card => card.GoalType == "Resolution")
            .ToList();

        if (!resolutionCards.Any())
            return null;

        RequestCard selectedResolution = resolutionCards[_random.Next(resolutionCards.Count)];
        CardInstance resolutionInstance = new CardInstance(selectedResolution, npc.ID);

        if (resolutionInstance.Context == null)
            resolutionInstance.Context = new CardContext();
        
        // Use the rapport threshold from the card itself (from JSON)
        resolutionInstance.Context.RapportThreshold = selectedResolution.RapportThreshold;
        
        return resolutionInstance;
    }

    /// <summary>
    /// Select a valid request card for conversation type (legacy method for Promise conversations)
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
        CardInstance requestInstance = new CardInstance(selectedRequest, npc.ID);

        // Store the rapport threshold in the card context if it's a RequestCard
        if (selectedRequest is RequestCard requestCard)
        {
            if (requestInstance.Context == null)
                requestInstance.Context = new CardContext();

            // Store the rapport threshold properly
            requestInstance.Context.RapportThreshold = requestCard.RapportThreshold;
        }

        return requestInstance;
    }

    /// <summary>
    /// Check if request card is valid for conversation type
    /// </summary>
    private bool IsRequestCardValidForConversation(ConversationCard card, ConversationType type)
    {
        // Check if this is a promise/goal card (has GoalCard property)
        return card.Properties.Contains(CardProperty.GoalCard);
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
        session.ActiveCards.Clear();
        session.Deck.Clear();

        // Restore active cards
        foreach (string cardId in handCardIds)
        {
            ConversationCard cardTemplate = FindCardTemplateById(cardId);
            if (cardTemplate != null)
            {
                CardInstance cardInstance = new CardInstance(cardTemplate, session.NPC?.ID ?? "unknown");
                session.ActiveCards.Add(cardInstance);
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