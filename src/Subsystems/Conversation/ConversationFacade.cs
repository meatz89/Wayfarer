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
    private readonly EmotionalStateManager _stateManager;
    private readonly ComfortManager _comfortManager;
    private readonly DialogueGenerator _dialogueGenerator;
    private readonly ExchangeHandler _exchangeHandler;
    private readonly ConversationStateTracker _stateTracker;
    
    // External dependencies
    private readonly ObligationQueueManager _queueManager;
    private readonly ObservationManager _observationManager;
    private readonly TimeManager _timeManager;
    private readonly TokenMechanicsManager _tokenManager;
    private readonly MessageSystem _messageSystem;
    private readonly TimeBlockAttentionManager _timeBlockAttentionManager;
    private readonly ConversationManager _conversationManager;
    
    private ConversationSession _currentSession;
    private ConversationOutcome _lastOutcome;

    public ConversationFacade(
        GameWorld gameWorld,
        ConversationOrchestrator orchestrator,
        CardDeckManager deckManager,
        EmotionalStateManager stateManager,
        ComfortManager comfortManager,
        DialogueGenerator dialogueGenerator,
        ExchangeHandler exchangeHandler,
        ConversationStateTracker stateTracker,
        ObligationQueueManager queueManager,
        ObservationManager observationManager,
        TimeManager timeManager,
        TokenMechanicsManager tokenManager,
        MessageSystem messageSystem,
        TimeBlockAttentionManager timeBlockAttentionManager,
        ConversationManager conversationManager)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
        _deckManager = deckManager ?? throw new ArgumentNullException(nameof(deckManager));
        _stateManager = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
        _comfortManager = comfortManager ?? throw new ArgumentNullException(nameof(comfortManager));
        _dialogueGenerator = dialogueGenerator ?? throw new ArgumentNullException(nameof(dialogueGenerator));
        _exchangeHandler = exchangeHandler ?? throw new ArgumentNullException(nameof(exchangeHandler));
        _stateTracker = stateTracker ?? throw new ArgumentNullException(nameof(stateTracker));
        _queueManager = queueManager ?? throw new ArgumentNullException(nameof(queueManager));
        _observationManager = observationManager ?? throw new ArgumentNullException(nameof(observationManager));
        _timeManager = timeManager ?? throw new ArgumentNullException(nameof(timeManager));
        _tokenManager = tokenManager ?? throw new ArgumentNullException(nameof(tokenManager));
        _messageSystem = messageSystem ?? throw new ArgumentNullException(nameof(messageSystem));
        _timeBlockAttentionManager = timeBlockAttentionManager ?? throw new ArgumentNullException(nameof(timeBlockAttentionManager));
        _conversationManager = conversationManager ?? throw new ArgumentNullException(nameof(conversationManager));
    }

    /// <summary>
    /// Start a new conversation with an NPC
    /// </summary>
    public ConversationSession StartConversation(string npcId, ConversationType conversationType, List<CardInstance> observationCards = null)
    {
        if (IsConversationActive())
        {
            Console.WriteLine($"[ConversationFacade] Ending existing conversation before starting new one");
            EndConversation();
        }

        var npc = _gameWorld.NPCs.FirstOrDefault(n => n.ID == npcId);
        if (npc == null)
        {
            throw new ArgumentException($"NPC with ID {npcId} not found");
        }

        // Validate conversation type is available
        var availableTypes = GetAvailableConversationTypes(npc);
        if (!availableTypes.Contains(conversationType))
        {
            throw new InvalidOperationException($"Conversation type {conversationType} is not available for {npc.Name}");
        }

        // Create session based on conversation type
        _currentSession = _orchestrator.CreateSession(npc, conversationType, observationCards);
        _stateTracker.BeginTracking(_currentSession);
        
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
        
        // Apply token changes
        if (_lastOutcome.TokensEarned != 0)
        {
            var primaryType = _deckManager.DeterminePrimaryCardType(_currentSession.HandCards);
            var connectionType = MapCardTypeToConnection(primaryType);
            _tokenManager.AddTokensToNPC(connectionType, _lastOutcome.TokensEarned, _currentSession.NPC.ID);
        }

        // Generate letter if eligible
        if (_orchestrator.ShouldGenerateLetter(_currentSession))
        {
            var obligation = _orchestrator.CreateLetterObligation(_currentSession);
            _queueManager.AddObligation(obligation);
            _currentSession.LetterGenerated = true;
        }

        _stateTracker.EndTracking();
        _currentSession.Deck.ResetForNewConversation();
        _currentSession = null;
        
        return _lastOutcome;
    }

    /// <summary>
    /// Process a conversation action (LISTEN or SPEAK)
    /// </summary>
    public ConversationTurnResult ProcessAction(ConversationAction action)
    {
        if (!IsConversationActive())
        {
            throw new InvalidOperationException("No active conversation");
        }

        ConversationTurnResult result;
        
        if (action.ActionType == ActionType.Listen)
        {
            result = _orchestrator.ProcessListenAction(_currentSession);
        }
        else if (action.ActionType == ActionType.Speak)
        {
            result = _orchestrator.ProcessSpeakAction(_currentSession, action.SelectedCards);
            
            // Handle special card effects
            HandleSpecialCardEffects(action.SelectedCards, result);
            
            // Remove used observation cards
            foreach (var card in action.SelectedCards)
            {
                if (card.IsObservation && card.Persistence == PersistenceType.Fleeting)
                {
                    _observationManager.RemoveObservationCard(card.TemplateId);
                }
            }
        }
        else
        {
            throw new ArgumentException($"Unknown action type: {action.ActionType}");
        }

        // Check if conversation should end
        if (_orchestrator.ShouldEndConversation(_currentSession))
        {
            EndConversation();
        }

        _stateTracker.RecordTurn(result);
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
    /// Create a conversation context for UI
    /// </summary>
    public async Task<ConversationContext> CreateConversationContext(string npcId, ConversationType conversationType)
    {
        var npc = _gameWorld.NPCs.FirstOrDefault(n => n.ID == npcId);
        if (npc == null)
        {
            return new ConversationContext
            {
                IsValid = false,
                ErrorMessage = "NPC not found"
            };
        }

        // Check attention cost
        var attentionCost = ConversationTypeConfig.GetAttentionCost(conversationType);
        var currentTimeBlock = _timeManager.GetCurrentTimeBlock();
        var currentAttention = _timeBlockAttentionManager.GetCurrentAttention(currentTimeBlock);
        
        if (!currentAttention.CanAfford(attentionCost))
        {
            return new ConversationContext
            {
                IsValid = false,
                ErrorMessage = "Not enough attention"
            };
        }

        // Spend attention
        if (!currentAttention.TrySpend(attentionCost))
        {
            return new ConversationContext
            {
                IsValid = false,
                ErrorMessage = "Failed to spend attention"
            };
        }

        // Get observation cards
        var observationCardsTemplates = _observationManager.GetObservationCardsAsConversationCards();
        var observationCards = observationCardsTemplates.Select(card => new CardInstance(card, "observation")).ToList();

        // Start conversation
        var session = StartConversation(npcId, conversationType, observationCards);

        return new ConversationContext
        {
            IsValid = true,
            NpcId = npcId,
            Npc = npc,
            Type = conversationType,
            InitialState = session.CurrentState,
            Session = session,
            ObservationCards = observationCards,
            AttentionSpent = attentionCost
        };
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
            CurrentComfort = _currentSession.CurrentComfort,
            CurrentPatience = _currentSession.CurrentPatience,
            MaxPatience = _currentSession.MaxPatience,
            TurnNumber = _currentSession.TurnNumber,
            LetterGenerated = _currentSession.LetterGenerated,
            GoalCardDrawn = _currentSession.GoalCardDrawn,
            GoalUrgencyCounter = _currentSession.GoalUrgencyCounter,
            GoalCardPlayed = _currentSession.GoalCardPlayed,
            HandCardIds = _currentSession.HandCards.Select(c => c.InstanceId).ToList(),
            DeckCardIds = _currentSession.Deck.GetAllCards().Select(c => c.InstanceId).ToList()
        };
    }

    /// <summary>
    /// Restore conversation state from persistence
    /// </summary>
    public void RestoreState(ConversationMemento memento)
    {
        if (memento == null)
            return;

        var npc = _gameWorld.NPCs.FirstOrDefault(n => n.ID == memento.NpcId);
        if (npc == null)
            return;

        // Create basic session
        _currentSession = new ConversationSession
        {
            NPC = npc,
            ConversationType = memento.ConversationType,
            CurrentState = memento.CurrentState,
            CurrentComfort = memento.CurrentComfort,
            CurrentPatience = memento.CurrentPatience,
            TurnNumber = memento.TurnNumber,
            LetterGenerated = memento.LetterGenerated,
            GoalCardDrawn = memento.GoalCardDrawn,
            GoalUrgencyCounter = memento.GoalUrgencyCounter,
            GoalCardPlayed = memento.GoalCardPlayed,
            MaxPatience = memento.MaxPatience,  // Use stored max patience from memento
            TokenManager = _tokenManager,
            Deck = SessionCardDeck.CreateFromTemplates(npc.ConversationDeck.GetAllCards(), npc.ID),
            Hand = new HandDeck()
        };

        // Restore hand and deck cards
        _deckManager.RestoreSessionCards(_currentSession, memento.HandCardIds, memento.DeckCardIds);
        _stateTracker.BeginTracking(_currentSession);
    }

    /// <summary>
    /// Get the last conversation outcome
    /// </summary>
    public ConversationOutcome GetLastOutcome()
    {
        return _lastOutcome;
    }

    /// <summary>
    /// Get available conversation types for an NPC
    /// </summary>
    public List<ConversationType> GetAvailableConversationTypes(NPC npc)
    {
        var available = new List<ConversationType>();
        
        // Check exchange deck
        if (_gameWorld.NPCExchangeDecks.TryGetValue(npc.ID.ToLower(), out var exchangeCards))
        {
            npc.InitializeExchangeDeck(exchangeCards);
        }
        else
        {
            npc.InitializeExchangeDeck(null);
        }
        
        if (npc.HasExchangeCards())
        {
            available.Add(ConversationType.Commerce);
        }
        
        // Check for promise cards with valid states
        if (npc.HasPromiseCards())
        {
            var currentState = ConversationRules.DetermineInitialState(npc, _queueManager);
            if (_stateManager.HasValidGoalCard(npc, currentState))
            {
                available.Add(ConversationType.Promise);
            }
        }
        
        // Check for burden cards
        if (npc.CountBurdenCards() >= 2)
        {
            available.Add(ConversationType.Resolution);
        }
        
        // Check for letter delivery
        if (_queueManager != null)
        {
            var activeObligations = _queueManager.GetActiveObligations();
            var hasLetterForNpc = activeObligations.Any(o => 
                o != null && (o.RecipientId == npc.ID || o.RecipientName == npc.Name));
            
            if (hasLetterForNpc)
            {
                available.Add(ConversationType.Delivery);
            }
        }
        
        // Standard conversation
        if (npc.ConversationDeck != null && npc.ConversationDeck.RemainingCards > 0)
        {
            available.Add(ConversationType.FriendlyChat);
        }
        
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
    /// Handle special card effects like exchanges and letter delivery
    /// </summary>
    private void HandleSpecialCardEffects(HashSet<CardInstance> playedCards, ConversationTurnResult result)
    {
        foreach (var card in playedCards)
        {
            // Handle exchange cards
            if (card.Context?.ExchangeData != null)
            {
                var exchangeSuccess = _exchangeHandler.ExecuteExchange(
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
                var obligations = _queueManager.GetActiveObligations();
                var deliveredObligation = obligations.FirstOrDefault(o => o.Id == card.DeliveryObligationId);
                
                if (deliveredObligation != null && _queueManager.DeliverObligation(card.DeliveryObligationId))
                {
                    // Grant rewards
                    _gameWorld.GetPlayer().Coins += deliveredObligation.Payment;
                    
                    int tokenReward = deliveredObligation.EmotionalWeight switch
                    {
                        EmotionalWeight.CRITICAL => 3,
                        EmotionalWeight.HIGH => 2,
                        EmotionalWeight.MEDIUM => 1,
                        _ => 1
                    };
                    
                    _tokenManager.AddTokensToNPC(deliveredObligation.TokenType, tokenReward, _currentSession.NPC.ID);
                    _currentSession.CurrentComfort += 5;
                    
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
                var urgentLetter = _orchestrator.CreateUrgentLetter(_currentSession.NPC);
                _queueManager.AddObligation(urgentLetter);
                _messageSystem.AddSystemMessage(
                    $"{_currentSession.NPC.Name} desperately hands you a letter for her family!",
                    SystemMessageTypes.Success);
                _currentSession.CurrentComfort += 5;
                _currentSession.LetterGenerated = true;
            }
        }
    }

    private ConnectionType MapCardTypeToConnection(CardType cardType)
    {
        return cardType switch
        {
            CardType.Trust => ConnectionType.Trust,
            CardType.Commerce => ConnectionType.Commerce,
            CardType.Status => ConnectionType.Status,
            CardType.Shadow => ConnectionType.Shadow,
            _ => ConnectionType.Trust
        };
    }
    
    /// <summary>
    /// Get the underlying ConversationManager for backward compatibility
    /// </summary>
    public ConversationManager GetConversationManager()
    {
        return _conversationManager;
    }
}