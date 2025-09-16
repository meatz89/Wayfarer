using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Public API for the Conversation subsystem.
/// Handles all conversation operations and delegates to internal managers.
/// </summary>
public class ConversationFacade
{
    private readonly GameWorld _gameWorld;
    private readonly ConversationOrchestrator _orchestrator;
    private readonly CardDeckManager _deckManager;
    private readonly ExchangeHandler _exchangeHandler;
    private readonly FocusManager _focusManager;
    private readonly AtmosphereManager _atmosphereManager;
    private readonly CategoricalEffectResolver _effectResolver;

    // External dependencies
    private readonly ObligationQueueManager _queueManager;
    private readonly ObservationManager _observationManager;
    private readonly TimeManager _timeManager;
    private readonly TokenMechanicsManager _tokenManager;
    private readonly MessageSystem _messageSystem;
    private readonly TimeBlockAttentionManager _timeBlockAttentionManager;

    private ConversationSession _currentSession;
    private ConversationOutcome _lastOutcome;

    public ConversationFacade(
        GameWorld gameWorld,
        ConversationOrchestrator orchestrator,
        CardDeckManager deckManager,
        ExchangeHandler exchangeHandler,
        FocusManager focusManager,
        AtmosphereManager atmosphereManager,
        CategoricalEffectResolver effectResolver,
        ObligationQueueManager queueManager,
        ObservationManager observationManager,
        TimeManager timeManager,
        TokenMechanicsManager tokenManager,
        MessageSystem messageSystem,
        TimeBlockAttentionManager timeBlockAttentionManager)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
        _deckManager = deckManager ?? throw new ArgumentNullException(nameof(deckManager));
        _exchangeHandler = exchangeHandler ?? throw new ArgumentNullException(nameof(exchangeHandler));
        _focusManager = focusManager ?? throw new ArgumentNullException(nameof(focusManager));
        _atmosphereManager = atmosphereManager ?? throw new ArgumentNullException(nameof(atmosphereManager));
        _effectResolver = effectResolver ?? throw new ArgumentNullException(nameof(effectResolver));
        _queueManager = queueManager ?? throw new ArgumentNullException(nameof(queueManager));
        _observationManager = observationManager ?? throw new ArgumentNullException(nameof(observationManager));
        _timeManager = timeManager ?? throw new ArgumentNullException(nameof(timeManager));
        _tokenManager = tokenManager ?? throw new ArgumentNullException(nameof(tokenManager));
        _messageSystem = messageSystem ?? throw new ArgumentNullException(nameof(messageSystem));
        _timeBlockAttentionManager = timeBlockAttentionManager ?? throw new ArgumentNullException(nameof(timeBlockAttentionManager));
    }

    /// <summary>
    /// Start a new conversation with an NPC
    /// </summary>
    public ConversationSession StartConversation(string npcId, ConversationType conversationType, string goalCardId = null, List<CardInstance> observationCards = null)
    {
        if (IsConversationActive())
        {
            Console.WriteLine($"[ConversationFacade] Ending existing conversation before starting new one");
            EndConversation();
        }

        NPC? npc = _gameWorld.NPCs.FirstOrDefault(n => n.ID == npcId);
        if (npc == null)
        {
            throw new ArgumentException($"NPC with ID {npcId} not found");
        }

        // Validate conversation type is available
        List<ConversationType> availableTypes = GetAvailableConversationTypes(npc);
        if (!availableTypes.Contains(conversationType))
        {
            throw new InvalidOperationException($"Conversation type {conversationType} is not available for {npc.Name}");
        }

        // Create session - Commerce removed, exchanges use separate Exchange system
        _currentSession = _orchestrator.CreateSession(npc, conversationType, goalCardId, observationCards);

        return _currentSession;
    }

    /// <summary>
    /// End the current conversation
    /// </summary>
    public ConversationOutcome EndConversation()
    {
        if (!IsConversationActive())
            return null;

        _lastOutcome = _orchestrator.FinalizeConversation(_currentSession);
        
        // Calculate and save the final flow value back to the NPC (persistence)
        int stateBase = _currentSession.CurrentState switch
        {
            ConnectionState.DISCONNECTED => 0,
            ConnectionState.GUARDED => 5,
            ConnectionState.NEUTRAL => 10,
            ConnectionState.RECEPTIVE => 15,
            ConnectionState.TRUSTING => 20,
            _ => 10
        };
        
        // FlowBattery is -2 to +2, convert to 0-4 range
        int flowPosition = Math.Clamp(_currentSession.FlowBattery + 2, 0, 4);
        _currentSession.NPC.RelationshipFlow = stateBase + flowPosition;

        // Token rewards are now handled via individual card thresholds, not a global rapport goal

        // Apply token changes from other sources
        if (_lastOutcome.TokensEarned != 0)
        {
            ConnectionType connectionType = DetermineConnectionTypeFromConversation(_currentSession);
            _tokenManager.AddTokensToNPC(connectionType, _lastOutcome.TokensEarned, _currentSession.NPC.ID);
        }

        // Generate letter if eligible
        if (_orchestrator.ShouldGenerateLetter(_currentSession))
        {
            DeliveryObligation obligation = _orchestrator.CreateLetterObligation(_currentSession);
            _queueManager.AddObligation(obligation);
            _currentSession.LetterGenerated = true;
        }

        _currentSession.Deck.ResetForNewConversation();
        _currentSession = null;

        return _lastOutcome;
    }

    /// <summary>
    /// Process a conversation action (LISTEN or SPEAK)
    /// </summary>
    public async Task<ConversationTurnResult> ProcessAction(ConversationAction action)
    {
        if (!IsConversationActive())
        {
            throw new InvalidOperationException("No active conversation");
        }

        ConversationTurnResult result;
        CardInstance cardPlayed = null;

        if (action.ActionType == ActionType.Listen)
        {
            result = await _orchestrator.ProcessListenAction(_currentSession);
        }
        else if (action.ActionType == ActionType.Speak)
        {
            // Get the single card for tracking (ProcessSpeakAction takes HashSet for legacy reasons)
            cardPlayed = action.SelectedCards?.FirstOrDefault();
            result = await _orchestrator.ProcessSpeakAction(_currentSession, action.SelectedCards);

            // Handle special card effects
            HandleSpecialCardEffects(action.SelectedCards, result);

            // Remove used observation cards (even though they're Thought type, they expire when used)
            foreach (CardInstance card in action.SelectedCards)
            {
                if (card.CardType == CardType.Observation)
                {
                    _observationManager.RemoveObservationCard(card.Id);
                }
            }
        }
        else
        {
            throw new ArgumentException($"Unknown action type: {action.ActionType}");
        }

        // Add turn to history
        if (_currentSession != null && result != null)
        {
            ConversationTurn turn = new ConversationTurn
            {
                ActionType = action.ActionType,
                Narrative = result.Narrative,
                Result = result,
                TurnNumber = _currentSession.TurnNumber,
                CardPlayed = cardPlayed
            };
            _currentSession.TurnHistory.Add(turn);
        }

        // Check if conversation should end
        if (_orchestrator.ShouldEndConversation(_currentSession))
        {
            EndConversation();
        }

        return result;
    }

    /// <summary>
    /// Get the current conversation session
    /// </summary>
    public ConversationSession GetCurrentSession()
    {
        return _currentSession;
    }

    /// <summary>
    /// Create a conversation context for UI - returns typed context
    /// </summary>
    public async Task<ConversationContextBase> CreateConversationContext(string npcId, ConversationType conversationType, string goalCardId = null)
    {
        NPC? npc = _gameWorld.NPCs.FirstOrDefault(n => n.ID == npcId);
        if (npc == null)
        {
            return ConversationContextFactory.CreateInvalidContext("NPC not found");
        }

        // Check attention cost
        int attentionCost = ConversationTypeConfig.GetAttentionCost(conversationType);
        TimeBlocks currentTimeBlock = _timeManager.GetCurrentTimeBlock();
        AttentionManager currentAttention = _timeBlockAttentionManager.GetCurrentAttention(currentTimeBlock);

        if (!currentAttention.CanAfford(attentionCost))
        {
            return ConversationContextFactory.CreateInvalidContext("Not enough attention");
        }

        // Spend attention
        if (!currentAttention.TrySpend(attentionCost))
        {
            return ConversationContextFactory.CreateInvalidContext("Failed to spend attention");
        }

        // Get observation cards
        List<ConversationCard> observationCardsTemplates = _observationManager.GetObservationCardsAsConversationCards();
        List<CardInstance> observationCards = observationCardsTemplates.Select(card => new CardInstance(card, "observation")).ToList();

        // Start conversation
        ConversationSession session = StartConversation(npcId, conversationType, goalCardId, observationCards);

        // Create typed context
        ConversationContextBase context = ConversationContextFactory.CreateContext(
            conversationType,
            npc,
            session,
            observationCards,
            attentionCost,
            ResourceState.FromPlayerResourceState(_gameWorld.GetPlayerResourceState()),
            _gameWorld.GetPlayer().CurrentLocationSpot.ToString(),
            _timeManager.GetCurrentTimeBlock().ToString());

        // Initialize type-specific data
        ConversationContextFactory.InitializeContextData(context, _gameWorld, _queueManager);

        return context;
    }

    /// <summary>
    /// Get available actions for current conversation state
    /// </summary>
    public List<ConversationAction> GetAvailableActions()
    {
        if (!IsConversationActive())
            return new List<ConversationAction>();

        return _orchestrator.GetAvailableActions(_currentSession);
    }

    /// <summary>
    /// Check if a conversation is currently active
    /// </summary>
    public bool IsConversationActive()
    {
        return _currentSession != null;
    }

    /// <summary>
    /// Save conversation state for persistence
    /// </summary>
    public ConversationMemento SaveState()
    {
        if (!IsConversationActive())
            return null;

        return new ConversationMemento
        {
            NpcId = _currentSession.NPC.ID,
            ConversationType = _currentSession.ConversationType,
            CurrentState = _currentSession.CurrentState,
            CurrentFlow = _currentSession.FlowBattery,
            CurrentPatience = _currentSession.CurrentPatience,
            MaxPatience = _currentSession.MaxPatience,
            TurnNumber = _currentSession.TurnNumber,
            LetterGenerated = _currentSession.LetterGenerated,
            RequestCardDrawn = _currentSession.RequestCardDrawn,
            RequestUrgencyCounter = _currentSession.RequestUrgencyCounter,
            RequestCardPlayed = _currentSession.RequestCardPlayed,
            HandCardIds = _currentSession.Deck.Hand.Cards.Select(c => c.InstanceId).ToList(),
            // HIGHLANDER: Collect all card IDs from all piles in deck
            DeckCardIds = new List<string>()
        };
    }

    /// <summary>
    /// Restore conversation state from persistence
    /// </summary>
    public void RestoreState(ConversationMemento memento)
    {
        if (memento == null)
            return;

        NPC? npc = _gameWorld.NPCs.FirstOrDefault(n => n.ID == memento.NpcId);
        if (npc == null)
            return;

        // Create basic session
        _currentSession = new ConversationSession
        {
            NPC = npc,
            ConversationType = memento.ConversationType,
            CurrentState = memento.CurrentState,
            // FlowBattery restored from memento
            CurrentPatience = memento.CurrentPatience,
            TurnNumber = memento.TurnNumber,
            LetterGenerated = memento.LetterGenerated,
            RequestCardDrawn = memento.RequestCardDrawn,
            RequestUrgencyCounter = memento.RequestUrgencyCounter,
            RequestCardPlayed = memento.RequestCardPlayed,
            MaxPatience = memento.MaxPatience,  // Use stored max patience from memento
            TokenManager = _tokenManager,
            Deck = SessionCardDeck.CreateFromTemplates(npc.ProgressionDeck.GetAllCards(), npc.ID),
        };

        // Restore hand and deck cards
        _deckManager.RestoreSessionCards(_currentSession, memento.HandCardIds, memento.DeckCardIds);
    }

    /// <summary>
    /// Get the last conversation outcome
    /// </summary>
    public ConversationOutcome GetLastOutcome()
    {
        return _lastOutcome;
    }

    /// <summary>
    /// Get available conversation options for an NPC with specific goal cards
    /// </summary>
    public List<ConversationOption> GetAvailableConversationOptions(NPC npc)
    {
        List<ConversationOption> options = new List<ConversationOption>();

        // Initialize daily patience if needed
        if (npc.MaxDailyPatience == 0)
        {
            npc.InitializeDailyPatience();
        }

        // Check if NPC has patience left for conversations
        if (!npc.HasPatienceForConversation())
        {
            // No patience left - no conversations available today
            return options;
        }

        // Get one-time requests as conversation options
        if (npc.Requests != null && npc.Requests.Count > 0)
        {
            var availableRequests = npc.GetAvailableRequests();
            
            // Add each available request as a conversation option
            foreach (var request in availableRequests)
            {
                // For now, add the request as a single option that will show all cards
                // Later we'll update the UI to show all cards from the request
                options.Add(new ConversationOption
                {
                    Type = ConversationType.Request, // One-time requests use Request type
                    GoalCardId = request.Id, // Use request ID to identify which request
                    DisplayName = request.Name,
                    Description = request.Description,
                    TokenType = ConnectionType.Trust, // Default token type
                    RapportThreshold = 0, // Will check individual card thresholds
                    CardType = CardType.Promise
                });
            }
        }

        // DELIVERY: Check if player has letter for this NPC in obligation queue
        if (_queueManager != null)
        {
            DeliveryObligation[] activeObligations = _queueManager.GetActiveObligations();
            bool hasLetterForNpc = activeObligations.Any(o =>
                o != null && (o.RecipientId == npc.ID || o.RecipientName == npc.Description));
            
            if (hasLetterForNpc)
            {
                // Delivery doesn't need a goal card from RequestDeck
                options.Add(new ConversationOption
                {
                    Type = ConversationType.Delivery,
                    GoalCardId = null,
                    DisplayName = "Deliver Letter",
                    Description = "Deliver a letter from your queue",
                    TokenType = ConnectionType.None,
                    RapportThreshold = 0,
                    CardType = CardType.Letter
                });
            }
        }

        return options;
    }

    /// <summary>
    /// Get available conversation types for an NPC (legacy method for backward compatibility)
    /// </summary>
    public List<ConversationType> GetAvailableConversationTypes(NPC npc)
    {
        List<ConversationType> available = new List<ConversationType>();

        // Initialize daily patience if needed
        if (npc.MaxDailyPatience == 0)
        {
            npc.InitializeDailyPatience();
        }

        // Check if NPC has patience left for conversations
        if (!npc.HasPatienceForConversation())
        {
            // No patience left - no conversations available today
            return available;
        }

        // COMMERCE: Check if NPC has exchange deck
        if (_gameWorld.NPCExchangeCards.TryGetValue(npc.ID.ToLower(), out List<ExchangeCard>? exchangeCards))
        {
            npc.InitializeExchangeDeck(exchangeCards);
        }
        else
        {
            npc.InitializeExchangeDeck(null);
        }

        // REMOVED: Commerce is not a conversation type - exchanges use separate Exchange system

        // REQUEST: Check if NPC has available request bundles
        if (npc.HasAvailableRequests())
        {
            available.Add(ConversationType.Request);
        }

        // RESOLUTION: Check if NPC has burden cards that need resolving (2+ burden cards)
        if (npc.CountBurdenCards() >= 2)
        {
            available.Add(ConversationType.Resolution);
        }

        // DELIVERY: Check if player has letter for this NPC in obligation queue
        if (_queueManager != null)
        {
            DeliveryObligation[] activeObligations = _queueManager.GetActiveObligations();
            bool hasLetterForNpc = activeObligations.Any(o =>
                o != null && (o.RecipientId == npc.ID || o.RecipientName == npc.Description));

            if (hasLetterForNpc)
            {
                available.Add(ConversationType.Delivery);
            }
        }

        // FRIENDLYCHAT: Always available (player has starter deck)
        available.Add(ConversationType.FriendlyChat);

        return available;
    }

    /// <summary>
    /// Get attention cost for a conversation type
    /// </summary>
    public int GetConversationAttentionCost(ConversationType type)
    {
        return ConversationTypeConfig.GetAttentionCost(type);
    }

    /// <summary>
    /// Execute LISTEN action in current conversation
    /// </summary>
    public async Task<ConversationTurnResult> ExecuteListen()
    {
        if (!IsConversationActive())
        {
            throw new InvalidOperationException("No active conversation");
        }

        ConversationTurnResult result = await ProcessAction(new ConversationAction
        {
            ActionType = ActionType.Listen,
            SelectedCards = new HashSet<CardInstance>()
        });
        
        return result;
    }

    /// <summary>
    /// Check if a card can be played in the current conversation
    /// </summary>
    public bool CanPlayCard(CardInstance card, ConversationSession session)
    {
        if (card == null || session == null) return false;
        return _deckManager.CanPlayCard(card, session);
    }

    /// <summary>
    /// Execute SPEAK action with a single selected card (ONE CARD RULE)
    /// </summary>
    public async Task<ConversationTurnResult> ExecuteSpeakSingleCard(CardInstance selectedCard)
    {
        if (!IsConversationActive())
        {
            throw new InvalidOperationException("No active conversation");
        }

        if (selectedCard == null)
        {
            throw new ArgumentNullException(nameof(selectedCard), "Must select exactly one card to speak");
        }

        // Create a HashSet with single card for ProcessAction (will be refactored later)
        HashSet<CardInstance> singleCardSet = new HashSet<CardInstance> { selectedCard };

        ConversationTurnResult result = await ProcessAction(new ConversationAction
        {
            ActionType = ActionType.Speak,
            SelectedCards = singleCardSet
        });

        return result;
    }

    /// <summary>
    /// Check if a card can be selected given current selection
    /// </summary>
    public bool CanSelectCard(CardInstance card, HashSet<CardInstance> currentSelection)
    {
        if (!IsConversationActive())
            return false;

        // Can't select if already selected
        if (currentSelection.Contains(card))
            return true; // Can deselect

        // Check focus limit
        int currentFocus = currentSelection.Sum(c => c.Focus);
        int newFocus = currentFocus + card.Focus;

        return newFocus <= _currentSession.GetEffectiveFocusCapacity();
    }

    /// <summary>
    /// Handle special card effects like exchanges and letter delivery
    /// </summary>
    private void HandleSpecialCardEffects(HashSet<CardInstance> playedCards, ConversationTurnResult result)
    {
        foreach (CardInstance card in playedCards)
        {
            Console.WriteLine($"[ConversationFacade] Processing card {card.Description}, has Context: {card.Context != null}, has ExchangeData: {card.Context?.ExchangeData != null}");

            // Handle exchange cards (exchanges use separate ExchangeCard system)
            if (card.Context?.ExchangeData != null)
            {
                bool exchangeSuccess = _exchangeHandler.ExecuteExchange(
                    card.Context.ExchangeData,
                    _currentSession.NPC,
                    _gameWorld.GetPlayer(),
                    _gameWorld.GetPlayerResourceState());

                if (!exchangeSuccess)
                {
                    _messageSystem.AddSystemMessage("Exchange failed - insufficient resources", SystemMessageTypes.Warning);
                }
            }

            // Handle letter delivery
            if (card.CanDeliverLetter && !string.IsNullOrEmpty(card.DeliveryObligationId))
            {
                DeliveryObligation[] obligations = _queueManager.GetActiveObligations();
                DeliveryObligation? deliveredObligation = obligations.FirstOrDefault(o => o.Id == card.DeliveryObligationId);

                if (deliveredObligation != null && _queueManager.DeliverObligation(card.DeliveryObligationId))
                {
                    // Grant rewards
                    _gameWorld.GetPlayer().Coins += deliveredObligation.Payment;

                    int tokenReward = deliveredObligation.EmotionalFocus switch
                    {
                        EmotionalFocus.CRITICAL => 3,
                        EmotionalFocus.HIGH => 2,
                        EmotionalFocus.MEDIUM => 1,
                        _ => 1
                    };

                    _tokenManager.AddTokensToNPC(deliveredObligation.TokenType, tokenReward, _currentSession.NPC.ID);
                    _currentSession.FlowBattery = Math.Min(_currentSession.FlowBattery + 1, 3); // Flow change

                    _messageSystem.AddSystemMessage(
                        $"Successfully delivered {deliveredObligation.SenderName}'s letter to {_currentSession.NPC.Name}!",
                        SystemMessageTypes.Success);
                    _messageSystem.AddSystemMessage(
                        $"Earned {deliveredObligation.Payment} coins",
                        SystemMessageTypes.Success);
                }
            }

            // Handle crisis letter generation
            if (card.Context?.GeneratesLetterOnSuccess == true)
            {
                DeliveryObligation urgentLetter = _orchestrator.CreateUrgentLetter(_currentSession.NPC);
                _queueManager.AddObligation(urgentLetter);
                _messageSystem.AddSystemMessage(
                    $"{_currentSession.NPC.Name} disconnectedly hands you a letter for her family!",
                    SystemMessageTypes.Success);
                _currentSession.FlowBattery = Math.Min(_currentSession.FlowBattery + 1, 3); // Flow change
                _currentSession.LetterGenerated = true;
            }
        }
    }

    private ConnectionType MapCardTypeToConnection(ConnectionType cardType)
    {
        return cardType switch
        {
            ConnectionType.Trust => ConnectionType.Trust,
            ConnectionType.Commerce => ConnectionType.Commerce,
            ConnectionType.Status => ConnectionType.Status,
            ConnectionType.Shadow => ConnectionType.Shadow,
            _ => ConnectionType.Trust
        };
    }

    // ========== MISSING METHODS (STUBS) ==========

    public object ExecuteExchange(object exchangeData)
    {
        if (!IsConversationActive())
        {
            return new { Success = false, Error = "No active conversation" };
        }

        try
        {
            // Cast exchangeData to ExchangeData type
            if (exchangeData is not ExchangeData exchange)
            {
                throw new ArgumentException("exchangeData must be of type ExchangeData");
            }

            // Delegate to the exchange handler
            bool success = _exchangeHandler.ExecuteExchange(
                exchange,
                _currentSession.NPC,
                _gameWorld.GetPlayer(),
                _gameWorld.GetPlayerResourceState());

            return new { Success = success };
        }
        catch (Exception ex)
        {
            _messageSystem.AddSystemMessage($"Exchange failed: {ex.Message}", SystemMessageTypes.Warning);
            return new { Success = false, Error = ex.Message };
        }
    }

    public System.Threading.Tasks.Task<bool> EndConversationAsync()
    {
        ConversationOutcome outcome = EndConversation();
        return System.Threading.Tasks.Task.FromResult(outcome != null);
    }

    public int GetAttentionCost(ConversationType type)
    {
        return ConversationTypeConfig.GetAttentionCost(type);
    }

    /// <summary>
    /// Determine the connection type based on the conversation type and outcome
    /// </summary>
    private ConnectionType DetermineConnectionTypeFromConversation(ConversationSession session)
    {
        // Map conversation types to their corresponding connection types
        return session.ConversationType switch
        {
            // Commerce removed - exchanges use separate Exchange system
            ConversationType.Request => ConnectionType.Trust, // Request bundles with promise cards
            ConversationType.Resolution => ConnectionType.Trust,
            ConversationType.Delivery => ConnectionType.Trust,
            ConversationType.FriendlyChat => ConnectionType.Trust,
            _ => ConnectionType.Trust  // Default fallback
        };
    }

    private ConnectionType DetermineTokenTypeFromPersonality(PersonalityType personality)
    {
        // Map personality types to appropriate token types
        return personality switch
        {
            PersonalityType.DEVOTED => ConnectionType.Trust,
            PersonalityType.MERCANTILE => ConnectionType.Commerce,
            PersonalityType.PROUD => ConnectionType.Status,
            PersonalityType.CUNNING => ConnectionType.Shadow,
            PersonalityType.STEADFAST => ConnectionType.Trust,
            _ => ConnectionType.Trust
        };
    }

    /// <summary>
    /// Get the atmosphere manager for accessing atmosphere state
    /// </summary>
    public AtmosphereManager GetAtmosphereManager()
    {
        return _atmosphereManager;
    }

}