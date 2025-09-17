using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wayfarer.GameState.Enums;

/// <summary>
/// Public API for the Conversation subsystem.
/// Handles all conversation operations with functionality absorbed from ConversationOrchestrator and CardDeckManager.
/// </summary>
public class ConversationFacade
{
    private readonly GameWorld _gameWorld;
    private readonly ExchangeHandler _exchangeHandler;
    private readonly FocusManager _focusManager;
    private readonly AtmosphereManager _atmosphereManager;
    private readonly CategoricalEffectResolver _effectResolver;
    private readonly ConversationNarrativeService _narrativeService;

    // External dependencies
    private readonly ObligationQueueManager _queueManager;
    private readonly ObservationManager _observationManager;
    private readonly TimeManager _timeManager;
    private readonly TokenMechanicsManager _tokenManager;
    private readonly MessageSystem _messageSystem;
    private readonly TimeBlockAttentionManager _timeBlockAttentionManager;
    private readonly Random _random;

    private ConversationSession _currentSession;
    private ConversationOutcome _lastOutcome;
    private FlowManager _flowBatteryManager;
    private PersonalityRuleEnforcer _personalityEnforcer;

    public ConversationFacade(
        GameWorld gameWorld,
        ExchangeHandler exchangeHandler,
        FocusManager focusManager,
        AtmosphereManager atmosphereManager,
        CategoricalEffectResolver effectResolver,
        ConversationNarrativeService narrativeService,
        ObligationQueueManager queueManager,
        ObservationManager observationManager,
        TimeManager timeManager,
        TokenMechanicsManager tokenManager,
        MessageSystem messageSystem,
        TimeBlockAttentionManager timeBlockAttentionManager)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _exchangeHandler = exchangeHandler ?? throw new ArgumentNullException(nameof(exchangeHandler));
        _focusManager = focusManager ?? throw new ArgumentNullException(nameof(focusManager));
        _atmosphereManager = atmosphereManager ?? throw new ArgumentNullException(nameof(atmosphereManager));
        _effectResolver = effectResolver ?? throw new ArgumentNullException(nameof(effectResolver));
        _narrativeService = narrativeService ?? throw new ArgumentNullException(nameof(narrativeService));
        _queueManager = queueManager ?? throw new ArgumentNullException(nameof(queueManager));
        _observationManager = observationManager ?? throw new ArgumentNullException(nameof(observationManager));
        _timeManager = timeManager ?? throw new ArgumentNullException(nameof(timeManager));
        _tokenManager = tokenManager ?? throw new ArgumentNullException(nameof(tokenManager));
        _messageSystem = messageSystem ?? throw new ArgumentNullException(nameof(messageSystem));
        _timeBlockAttentionManager = timeBlockAttentionManager ?? throw new ArgumentNullException(nameof(timeBlockAttentionManager));
        _random = new Random();
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

        NPC npc = _gameWorld.NPCs.FirstOrDefault(n => n.ID == npcId);
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

        // Extract connection state and flow from single value
        ConnectionState initialState = npc.GetConnectionState();
        int initialFlow = npc.GetFlowBattery(); // -2 to +2

        // Initialize flow battery manager with persisted values
        _flowBatteryManager = new FlowManager(initialState, initialFlow);
        _flowBatteryManager.StateTransitioned += OnStateTransitioned;
        _flowBatteryManager.ConversationEnded += OnConversationEnded;

        // Initialize focus manager
        _focusManager.SetBaseCapacity(initialState);
        _focusManager.Reset();

        // Reset atmosphere manager
        _atmosphereManager.Reset();

        // Initialize personality rule enforcer based on NPC's personality
        _personalityEnforcer = new PersonalityRuleEnforcer(npc.ConversationModifier ?? new PersonalityModifier { Type = PersonalityModifierType.None });

        // Create session deck and get request cards
        (SessionCardDeck deck, List<CardInstance> requestCards) = CreateConversationDeck(npc, conversationType, goalCardId, observationCards);

        // Create rapport manager with initial token counts
        Dictionary<ConnectionType, int> npcTokens = GetNpcTokenCounts(npc);
        RapportManager rapportManager = new RapportManager(npcTokens);

        // Initialize NPC's daily patience if needed
        if (npc.MaxDailyPatience == 0)
        {
            npc.InitializeDailyPatience();
        }

        // Use NPC's current daily patience for the session
        int availablePatience = npc.DailyPatience;

        // Get request text if this is a Request conversation
        string requestText = null;
        if (conversationType == ConversationType.Request && !string.IsNullOrEmpty(goalCardId))
        {
            var request = npc.GetRequestById(goalCardId);
            if (request != null)
            {
                requestText = request.NpcRequestText;
            }
        }

        // Create session with new properties
        _currentSession = new ConversationSession
        {
            NPC = npc,
            ConversationType = conversationType,
            CurrentState = initialState,
            InitialState = initialState,
            FlowBattery = initialFlow, // Start with persisted flow (-2 to +2)
            CurrentFocus = 0,
            MaxFocus = _focusManager.CurrentCapacity,
            CurrentAtmosphere = AtmosphereType.Neutral,
            CurrentPatience = availablePatience, // Use NPC's daily patience
            MaxPatience = npc.MaxDailyPatience,  // Max based on personality
            TurnNumber = 0,
            Deck = deck, // HIGHLANDER: Deck manages ALL card piles
            TokenManager = _tokenManager,
            FlowManager = _flowBatteryManager,
            RapportManager = rapportManager,
            PersonalityEnforcer = _personalityEnforcer,  // Add personality enforcer to session
            ObservationCards = observationCards ?? new List<CardInstance>(),
            RequestText = requestText // Set request text for Request conversations
        };

        // THEN: Perform initial draw of regular cards
        // This is the initial conversation start, so we just draw cards without exhausting
        _focusManager.RefreshPool();
        int drawCount = _currentSession.GetDrawCount();
        // HIGHLANDER: Draw directly to hand
        _currentSession.Deck.DrawToHand(drawCount);

        // Update request card playability based on focus
        UpdateRequestCardPlayability(_currentSession);

        // Reset focus after initial draw (as per standard LISTEN)
        _currentSession.CurrentFocus = _focusManager.CurrentSpentFocus;
        _currentSession.MaxFocus = _focusManager.CurrentCapacity;

        // Update card playability based on initial focus
        UpdateCardPlayabilityBasedOnFocus(_currentSession);

        return _currentSession;
    }

    /// <summary>
    /// End the current conversation
    /// </summary>
    public ConversationOutcome EndConversation()
    {
        if (!IsConversationActive())
            return null;

        _lastOutcome = FinalizeConversation(_currentSession);

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
        if (ShouldGenerateLetter(_currentSession))
        {
            DeliveryObligation obligation = CreateLetterObligation(_currentSession);
            _queueManager.AddObligation(obligation);
            _currentSession.LetterGenerated = true;
        }

        _currentSession.Deck.ResetForNewConversation();
        _currentSession = null;

        return _lastOutcome;
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

        _currentSession.TurnNumber++;

        // Deduct patience cost (unless Patient atmosphere)
        if (!_atmosphereManager.ShouldWaivePatienceCost())
        {
            _currentSession.CurrentPatience--;
            _currentSession.NPC.DailyPatience--; // Also deduct from NPC's daily pool
        }

        // Execute LISTEN through deck operations
        List<CardInstance> drawnCards = ExecuteListenAction(_currentSession);

        // Notify personality enforcer that LISTEN occurred (resets turn state for Proud personality)
        _personalityEnforcer?.OnListen();

        // Update session focus state
        _currentSession.CurrentFocus = _focusManager.CurrentSpentFocus;
        _currentSession.MaxFocus = _focusManager.CurrentCapacity;

        // Update card playability based on current focus
        UpdateCardPlayabilityBasedOnFocus(_currentSession);

        // Generate narrative using the narrative service
        NarrativeOutput narrative = await _narrativeService.GenerateNarrativeAsync(
            _currentSession,
            _currentSession.NPC,
            drawnCards);
        string npcResponse = narrative.NPCDialogue;

        return new ConversationTurnResult
        {
            Success = true,
            NewState = _currentSession.CurrentState,
            NPCResponse = npcResponse,
            DrawnCards = drawnCards,
            PatienceRemaining = _currentSession.CurrentPatience,
            Narrative = narrative  // Include the full narrative output
        };
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

        _currentSession.TurnNumber++;

        // SPEAK costs focus (focus), not patience
        // Patience is only deducted for LISTEN actions

        // Validate play against personality rules
        if (_personalityEnforcer != null)
        {
            string violationMessage;
            if (!_personalityEnforcer.ValidatePlay(selectedCard, out violationMessage))
            {
                // Return early with failed result if personality rule violated
                return new ConversationTurnResult
                {
                    Success = false,
                    NewState = _currentSession.CurrentState,
                    NPCResponse = violationMessage,
                    FlowChange = 0,
                    OldFlow = _currentSession.FlowBattery,
                    NewFlow = _currentSession.FlowBattery,
                    PatienceRemaining = _currentSession.CurrentPatience,
                    PersonalityViolation = violationMessage
                };
            }
        }

        // Apply personality success rate modifier before playing the card
        if (_personalityEnforcer != null && selectedCard.Context != null)
        {
            // Get base success rate from effect resolver
            int baseSuccessRate = _effectResolver.CalculateSuccessPercentage(selectedCard, _currentSession);

            // Apply personality modifier (e.g., Mercantile +30% for highest focus)
            int modifiedSuccessRate = _personalityEnforcer.ModifySuccessRate(selectedCard, baseSuccessRate);

            // Store the modified rate for the card to use
            selectedCard.Context.ModifiedSuccessRate = modifiedSuccessRate;
        }

        // Play the card
        CardPlayResult playResult = PlayCard(_currentSession, selectedCard);

        // Grant XP to player stat based on card's bound stat
        Player player = _gameWorld.GetPlayer();
        if (selectedCard.Template.BoundStat.HasValue)
        {
            // Calculate XP amount based on conversation difficulty/level
            int xpAmount = 1; // Base XP

            // Scale XP by conversation difficulty if this is a stranger conversation
            if (_currentSession.IsStrangerConversation && _currentSession.StrangerLevel.HasValue)
            {
                xpAmount = _currentSession.StrangerLevel.Value; // Stranger level 1-3 = 1-3x XP
            }

            // Grant XP to the bound stat regardless of success/failure (practice makes perfect)
            player.Stats.AddXP(selectedCard.Template.BoundStat.Value, xpAmount);
        }

        // Record that this card was played for personality tracking
        _personalityEnforcer?.OnCardPlayed(selectedCard);

        int oldFlow = _currentSession.FlowBattery;
        int flowChange = playResult.FinalFlow;

        // Apply flow change through battery manager
        bool conversationEnded = false;
        ConnectionState newState = _currentSession.CurrentState;

        if (_flowBatteryManager != null && flowChange != 0)
        {
            (bool stateChanged, ConnectionState resultState, bool shouldEnd) =
                _flowBatteryManager.ApplyFlowChange(flowChange, _currentSession.CurrentAtmosphere);

            _currentSession.FlowBattery = _flowBatteryManager.CurrentFlow;
            conversationEnded = shouldEnd;

            if (stateChanged)
            {
                newState = resultState;
                _currentSession.CurrentState = newState;

                // Update focus capacity for new state
                _focusManager.SetBaseCapacity(newState);
            }
        }

        // Update session atmosphere
        _currentSession.CurrentAtmosphere = _atmosphereManager.CurrentAtmosphere;
        _currentSession.CurrentFocus = _focusManager.CurrentSpentFocus;
        _currentSession.MaxFocus = _focusManager.CurrentCapacity;

        // Exhaust all focus on failed SPEAK - forces LISTEN as only option
        // Unless the card ignores failure LISTEN (level 5 mastery)
        if (!playResult.Success && !selectedCard.IgnoresFailureListen(player.Stats))
        {
            // Spend all remaining focus to force LISTEN
            int remainingFocus = _focusManager.AvailableFocus;
            if (remainingFocus > 0)
            {
                _focusManager.SpendFocus(remainingFocus);
                _currentSession.CurrentFocus = _focusManager.CurrentSpentFocus;
            }
        }

        // Update card playability based on current focus
        UpdateCardPlayabilityBasedOnFocus(_currentSession);

        // Handle special card effects
        HashSet<CardInstance> singleCardSet = new HashSet<CardInstance> { selectedCard };
        HandleSpecialCardEffects(singleCardSet, new ConversationTurnResult { Success = playResult.Success });

        // Generate NPC response through narrative service
        List<CardInstance> activeCards = _currentSession.Deck.Hand.Cards.ToList();
        NarrativeOutput narrative = await _narrativeService.GenerateNarrativeAsync(
            _currentSession,
            _currentSession.NPC,
            activeCards);

        string npcResponse = narrative.NPCDialogue;

        ConversationTurnResult result = new ConversationTurnResult
        {
            Success = playResult.Success,
            NewState = newState,
            NPCResponse = npcResponse,
            FlowChange = flowChange,
            OldFlow = oldFlow,
            NewFlow = _currentSession.FlowBattery,
            PatienceRemaining = _currentSession.CurrentPatience,
            CardPlayResult = playResult,
            Narrative = narrative  // Pass the full narrative output
        };

        // Mark conversation as ended if needed
        if (conversationEnded || !playResult.Success)
        {
            result.Success = conversationEnded ? false : result.Success;
        }

        // Add turn to history
        if (_currentSession != null && result != null)
        {
            ConversationTurn turn = new ConversationTurn
            {
                ActionType = ActionType.Speak,
                Narrative = result.Narrative,
                Result = result,
                TurnNumber = _currentSession.TurnNumber,
                CardPlayed = selectedCard
            };
            _currentSession.TurnHistory.Add(turn);
        }

        // Check if conversation should end
        if (ShouldEndConversation(_currentSession))
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
        NPC npc = _gameWorld.NPCs.FirstOrDefault(n => n.ID == npcId);
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

        NPC npc = _gameWorld.NPCs.FirstOrDefault(n => n.ID == memento.NpcId);
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
        RestoreSessionCards(_currentSession, memento.HandCardIds, memento.DeckCardIds);
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
        if (_gameWorld.NPCExchangeCards.TryGetValue(npc.ID.ToLower(), out List<ExchangeCard> exchangeCards))
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
    /// Check if a card can be played in the current conversation
    /// </summary>
    public bool CanPlayCard(CardInstance card, ConversationSession session)
    {
        if (card == null || session == null) return false;

        // Check if card is marked as Unplayable
        if (!card.IsPlayable)
            return false;

        // Check focus availability
        if (!_focusManager.CanAffordCard(card.Focus))
            return false;

        // Only check rapport threshold for goal cards that are still in RequestPile
        // Cards that have been moved to ActiveCards have already met their threshold
        if (card.CardType == CardType.Letter || card.CardType == CardType.Promise || card.CardType == CardType.BurdenGoal)
        {
            // If card is in ActiveCards, it's already playable (threshold was met)
            if (session.Deck?.Hand?.Cards?.Contains(card) == true)
            {
                return true;  // Card already in active hand, no need for rapport check
            }

            // If card is in RequestPile, check rapport threshold
            if (session.Deck?.RequestCards?.Contains(card) == true)
            {
                int rapportThreshold = card.Context?.RapportThreshold ?? 0;
                int currentRapport = session.RapportManager?.CurrentRapport ?? 0;
                return currentRapport >= rapportThreshold;
            }
        }

        return true;
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

    public Task<bool> EndConversationAsync()
    {
        ConversationOutcome outcome = EndConversation();
        return Task.FromResult(outcome != null);
    }

    public int GetAttentionCost(ConversationType type)
    {
        return ConversationTypeConfig.GetAttentionCost(type);
    }

    /// <summary>
    /// Get the atmosphere manager for accessing atmosphere state
    /// </summary>
    public AtmosphereManager GetAtmosphereManager()
    {
        return _atmosphereManager;
    }

    #region Private Methods - Absorbed from ConversationOrchestrator

    /// <summary>
    /// Handle state transition event from flow battery
    /// </summary>
    private void OnStateTransitioned(ConnectionState oldState, ConnectionState newState)
    {
        // Log or handle state transition if needed
    }

    /// <summary>
    /// Handle conversation ended event from flow battery
    /// </summary>
    private void OnConversationEnded()
    {
        // Conversation ends due to DISCONNECTED at -3
    }

    /// <summary>
    /// Check if conversation should end
    /// </summary>
    private bool ShouldEndConversation(ConversationSession session)
    {
        // End if no patience left
        if (session.CurrentPatience <= 0)
            return true;

        // Check with flow battery manager
        if (_flowBatteryManager != null &&
            _flowBatteryManager.CurrentState == ConnectionState.DISCONNECTED &&
            _flowBatteryManager.CurrentFlow <= -3)
            return true;

        // End if deck is empty and no active cards
        if (!session.Deck.HasCardsAvailable() && session.Deck.Hand.Count == 0)
            return true;

        return false;
    }

    /// <summary>
    /// Finalize conversation and calculate outcome
    /// </summary>
    private ConversationOutcome FinalizeConversation(ConversationSession session)
    {
        bool success = true;
        string reason = "Conversation completed";

        // Check ending conditions
        if (session.CurrentPatience <= 0)
        {
            success = false;
            reason = "Patience exhausted";
        }
        else if (session.CurrentState == ConnectionState.DISCONNECTED && session.FlowBattery <= -3)
        {
            success = false;
            reason = "Relationship damaged beyond repair";
        }

        // Calculate token rewards based on final state
        int tokensEarned = CalculateTokenReward(session.CurrentState, session.FlowBattery);

        // Check if any request cards were played (Letter, Promise, or BurdenGoal types)
        bool requestAchieved = session.Deck.PlayedHistory.Cards.Any(c =>
            c.CardType == CardType.Letter ||
            c.CardType == CardType.Promise ||
            c.CardType == CardType.BurdenGoal);
        if (requestAchieved)
        {
            tokensEarned += 2; // Bonus for completing request
        }

        return new ConversationOutcome
        {
            Success = success,
            FinalFlow = session.FlowBattery,
            FinalState = session.CurrentState,
            TokensEarned = tokensEarned,
            RequestAchieved = requestAchieved,
            Reason = reason
        };
    }

    /// <summary>
    /// Calculate token reward based on final state and flow
    /// </summary>
    private int CalculateTokenReward(ConnectionState finalState, int finalFlow)
    {
        // Base reward by state
        int baseReward = finalState switch
        {
            ConnectionState.TRUSTING => 3,
            ConnectionState.RECEPTIVE => 2,
            ConnectionState.NEUTRAL => 1,
            ConnectionState.GUARDED => 0,
            ConnectionState.DISCONNECTED => -1,
            _ => 0
        };

        // Bonus for positive flow
        if (finalFlow > 0)
            baseReward += 1;
        else if (finalFlow < 0)
            baseReward -= 1;

        return Math.Max(0, baseReward);
    }

    /// <summary>
    /// Check if letter should be generated (based on positive outcomes)
    /// </summary>
    private bool ShouldGenerateLetter(ConversationSession session)
    {
        if (session.LetterGenerated)
            return false;

        // Generate letters from positive connections
        return session.CurrentState == ConnectionState.TRUSTING ||
               (session.CurrentState == ConnectionState.RECEPTIVE && session.FlowBattery > 1);
    }

    /// <summary>
    /// Create a letter obligation from successful conversation
    /// </summary>
    private DeliveryObligation CreateLetterObligation(ConversationSession session)
    {
        int stateValue = (int)session.CurrentState; // Use state as base value
        int flowBonus = Math.Max(0, session.FlowBattery);

        // Calculate deadline and payment based on relationship quality (segment-based)
        int baseSegments = 12; // ~12 segments base (3/4 of day)
        int deadlineInSegments = Math.Max(2, baseSegments - (stateValue * 2) - (flowBonus * 1));
        int payment = 5 + stateValue + flowBonus;

        // Determine tier and focus
        TierLevel tier = stateValue >= 4 ? TierLevel.T3 :
                        stateValue >= 2 ? TierLevel.T2 : TierLevel.T1;

        EmotionalFocus focus = deadlineInSegments <= 3 ? EmotionalFocus.CRITICAL :
                                deadlineInSegments <= 6 ? EmotionalFocus.HIGH :
                                deadlineInSegments <= 12 ? EmotionalFocus.MEDIUM :
                                EmotionalFocus.LOW;

        // Find recipient
        List<NPC> otherNpcs = _gameWorld.NPCs.Where(n => n.ID != session.NPC.ID).ToList();
        NPC recipient = otherNpcs.Any() ? otherNpcs[_random.Next(otherNpcs.Count)] : null;

        return new DeliveryObligation
        {
            Id = Guid.NewGuid().ToString(),
            SenderId = session.NPC.ID,
            SenderName = session.NPC.Name,
            RecipientId = recipient?.ID ?? "unknown",
            RecipientName = recipient?.Name ?? "Someone",
            TokenType = ConnectionType.Trust,
            Stakes = StakeType.SAFETY,
            DeadlineInSegments = deadlineInSegments,
            Payment = payment,
            Tier = tier,
            EmotionalFocus = focus,
            Description = $"Letter from {session.NPC.Name} (State: {session.CurrentState}, Flow: {session.FlowBattery})"
        };
    }

    /// <summary>
    /// Create an urgent letter from an NPC in distress
    /// </summary>
    private DeliveryObligation CreateUrgentLetter(NPC npc)
    {
        // Find a suitable recipient (family member, friend, etc.)
        List<NPC> allNpcs = _gameWorld.GetAllNPCs();
        NPC recipient = allNpcs.FirstOrDefault(n => n.ID != npc.ID) ?? allNpcs.FirstOrDefault();

        return new DeliveryObligation
        {
            Id = Guid.NewGuid().ToString(),
            Title = $"Urgent Letter from {npc.Name}",
            SenderId = npc.ID,
            SenderName = npc.Name,
            RecipientId = recipient?.ID ?? "unknown",
            RecipientName = recipient?.Name ?? "Someone",
            TokenType = ConnectionType.Trust,
            Stakes = StakeType.SAFETY,
            DeadlineInSegments = 8, // 8 segments for urgent letters
            Payment = 15, // Higher payment for urgent delivery
            Tier = (TierLevel)npc.Tier,
            EmotionalFocus = EmotionalFocus.HIGH, // High emotional focus for urgency
            Description = $"Urgent letter from {npc.Name} - they disconnectedly need help!"
        };
    }

    /// <summary>
    /// Get current token counts for an NPC to initialize rapport
    /// </summary>
    private Dictionary<ConnectionType, int> GetNpcTokenCounts(NPC npc)
    {
        Dictionary<ConnectionType, int> tokenCounts = new Dictionary<ConnectionType, int>
        {
            { ConnectionType.Trust, _tokenManager.GetTokenCount(ConnectionType.Trust, npc.ID) },
            { ConnectionType.Commerce, _tokenManager.GetTokenCount(ConnectionType.Commerce, npc.ID) },
            { ConnectionType.Status, _tokenManager.GetTokenCount(ConnectionType.Status, npc.ID) },
            { ConnectionType.Shadow, _tokenManager.GetTokenCount(ConnectionType.Shadow, npc.ID) }
        };
        return tokenCounts;
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

    #endregion

    #region Private Methods - Absorbed from CardDeckManager

    /// <summary>
    /// Create a conversation deck from NPC templates (no filtering by type)
    /// Returns both the deck and any request cards that should start in hand
    /// </summary>
    private (SessionCardDeck deck, List<CardInstance> requestCards) CreateConversationDeck(NPC npc, ConversationType conversationType, string goalCardId = null, List<CardInstance> observationCards = null)
    {
        string sessionId = Guid.NewGuid().ToString();

        // Start with player's conversation deck (persistent CardInstances with XP)
        Player player = _gameWorld.GetPlayer();
        List<CardInstance> playerInstances = new List<CardInstance>();

        // Get all player's card instances (these have XP)
        if (player.ConversationDeck != null && player.ConversationDeck.Count > 0)
        {
            playerInstances.AddRange(player.ConversationDeck.GetAllInstances());
        }
        else
        {
            // Critical error - player has no conversation abilities!
            Console.WriteLine("[ConversationFacade] ERROR: Player has no conversation deck! Check PackageLoader initialization.");
            // Continue anyway to avoid crash, but conversation will be unplayable
        }

        // Create session deck from player's instances (preserves XP)
        SessionCardDeck deck = SessionCardDeck.CreateFromInstances(playerInstances, sessionId);

        // Add unlocked NPC progression cards as new instances
        List<ConversationCard> unlockedProgressionCards = GetUnlockedProgressionCards(npc);
        foreach (var progressionCard in unlockedProgressionCards)
        {
            CardInstance progressionInstance = new CardInstance(progressionCard, npc.ID);
            deck.AddCard(progressionInstance);
        }

        // Safety check - ensure we have at least some cards
        if (playerInstances.Count == 0 && unlockedProgressionCards.Count == 0)
        {
            Console.WriteLine($"[ConversationFacade] WARNING: Creating conversation with {npc.Name} but deck has NO cards!");
        }

        // Add observation cards if provided
        if (observationCards != null && observationCards.Any())
        {
            foreach (CardInstance card in observationCards)
            {
                deck.AddCard(card);
            }
        }

        // Get request cards based on conversation type from JSON data
        // For Request conversations, this loads ALL cards from the Request bundle
        List<CardInstance> requestCards = SelectGoalCardsForConversationType(npc, conversationType, goalCardId, deck);

        // HIGHLANDER: Add request cards directly to deck's request pile
        foreach (var requestCard in requestCards)
        {
            deck.AddRequestCard(requestCard);
        }

        // Now shuffle the deck after all cards (including promise cards) have been added
        deck.ShuffleDrawPile();

        // HIGHLANDER: Return only deck, not separate request cards
        return (deck, new List<CardInstance>());
    }

    /// <summary>
    /// Execute LISTEN action - refresh focus, draw cards, and exhaust opening cards
    /// </summary>
    private List<CardInstance> ExecuteListenAction(ConversationSession session)
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

        // HIGHLANDER: Draw directly to hand
        session.Deck.DrawToHand(drawCount);

        // Get the drawn cards for return value
        var drawnCards = session.Deck.Hand.Cards.TakeLast(drawCount).ToList();

        // Check if any goal cards should become playable based on rapport
        UpdateGoalCardPlayabilityAfterListen(session);

        return drawnCards;
    }

    /// <summary>
    /// Play a single card with dice roll and focus management
    /// </summary>
    private CardPlayResult PlayCard(ConversationSession session, CardInstance selectedCard)
    {
        // Check if card is unplayable (but skip this check for promise cards which handle rapport separately)
        if (!selectedCard.IsPlayable &&
            !(selectedCard.CardType == CardType.Letter || selectedCard.CardType == CardType.Promise || selectedCard.CardType == CardType.BurdenGoal))
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
                FinalFlow = 0
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
                FinalFlow = 0
            };
        }

        // Calculate success percentage - use modified rate if personality rules applied it
        int successPercentage = selectedCard.Context?.ModifiedSuccessRate
            ?? _effectResolver.CalculateSuccessPercentage(selectedCard, session);

        // Promise/request cards (GoalCard) ALWAYS succeed
        bool success;
        int roll;

        if (selectedCard.CardType == CardType.Letter || selectedCard.CardType == CardType.Promise || selectedCard.CardType == CardType.BurdenGoal)
        {
            // Promise/request cards always succeed (100% success rate)
            success = true;
            roll = 100; // For display purposes
            successPercentage = 100; // Override to show 100% in UI

            // Mark request as completed if this is a BurdenGoal (request) card
            if (selectedCard.CardType == CardType.BurdenGoal && selectedCard.Context?.RequestId != null)
            {
                // Find and complete the request
                var request = session.NPC.GetRequestById(selectedCard.Context.RequestId);
                if (request != null)
                {
                    request.Complete();
                    // The conversation will end after this card is played
                }
            }
        }
        else
        {
            // Use pre-rolled value if available, otherwise generate one (shouldn't happen normally)
            roll = selectedCard.Context?.PreRolledValue ?? _random.Next(1, 101);

            // Check success using the pre-rolled value with momentum system
            success = _effectResolver.CheckSuccessWithPreRoll(roll, successPercentage, session);
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
            // PROJECTION PRINCIPLE: Get projection from resolver and apply it
            effectResult = _effectResolver.ProcessSuccessEffect(selectedCard, session);

            // Apply personality modifier to rapport change
            int rapportChange = effectResult.RapportChange;
            if (session.PersonalityEnforcer != null && rapportChange != 0)
            {
                rapportChange = session.PersonalityEnforcer.ModifyRapportChange(selectedCard, rapportChange);
            }

            // Apply rapport changes to RapportManager
            if (rapportChange != 0 && session.RapportManager != null)
            {
                session.RapportManager.ApplyRapportChange(rapportChange, session.CurrentAtmosphere);
            }

            // Apply focus restoration (for Focusing success effect)
            if (effectResult.FocusAdded > 0)
            {
                _focusManager.AddFocus(effectResult.FocusAdded);
            }

            // Apply flow battery change (for Advancing success effect)
            if (effectResult.FlowChange != 0)
            {
                flowChange += effectResult.FlowChange; // Add to the base flow change
            }

            // Add drawn cards to active cards (for Threading success effect)
            if (effectResult.CardsToAdd.Any())
            {
                session.Deck.Hand.AddRange(effectResult.CardsToAdd);
            }

            // Handle atmosphere change (for Atmospheric success effect)
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

            // PROJECTION PRINCIPLE: Get projection from resolver
            effectResult = _effectResolver.ProcessFailureEffect(selectedCard, session);

            // Apply personality modifier to rapport change (for failure effects)
            int failureRapportChange = effectResult.RapportChange;
            if (session.PersonalityEnforcer != null && failureRapportChange != 0)
            {
                failureRapportChange = session.PersonalityEnforcer.ModifyRapportChange(selectedCard, failureRapportChange);
            }

            // Apply rapport changes to RapportManager (if any failure effects modify rapport)
            if (failureRapportChange != 0 && session.RapportManager != null)
            {
                session.RapportManager.ApplyRapportChange(failureRapportChange, session.CurrentAtmosphere);
            }

            // Apply focus penalty (for ForceListen effect)
            if (effectResult.FocusAdded < 0)
            {
                _focusManager.AddFocus(effectResult.FocusAdded); // Adding negative = removing
            }

            // Clear atmosphere on failure
            _atmosphereManager.ClearAtmosphereOnFailure();
        }

        // HIGHLANDER: Use deck's PlayCard method which handles all transitions
        session.Deck.PlayCard(selectedCard);

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
            FinalFlow = flowChange
        };

        // Handle exhaust ending conversation
        if (!conversationContinues)
        {
            result.Success = false; // Override to mark conversation as failed
        }

        return result;
    }

    /// <summary>
    /// Check if goal cards should become playable after LISTEN based on rapport threshold
    /// </summary>
    private void UpdateGoalCardPlayabilityAfterListen(ConversationSession session)
    {
        // Get current rapport
        int currentRapport = session.RapportManager?.CurrentRapport ?? 0;

        // Check all goal cards in active hand
        foreach (CardInstance card in session.Deck.Hand.Cards)
        {
            // Only process goal cards that are currently Unplayable
            if ((card.CardType == CardType.Letter || card.CardType == CardType.Promise || card.CardType == CardType.BurdenGoal)
                && !card.IsPlayable)
            {
                // Check if rapport threshold is met
                int rapportThreshold = card.Context?.RapportThreshold ?? 0;

                if (currentRapport >= rapportThreshold)
                {
                    // Make card playable
                    card.IsPlayable = true;
                    // Request cards already have Impulse + Opening persistence set

                    // Mark that a request card is now playable
                    session.RequestCardDrawn = true;
                }
            }
        }
    }

    /// <summary>
    /// Legacy method for compatibility - now just marks request card presence
    /// </summary>
    private void UpdateRequestCardPlayability(ConversationSession session)
    {
        // This is called at conversation start - just check for goal card presence
        bool hasRequestCard = session.Deck.Hand.Cards
            .Any(c => c.CardType == CardType.Letter || c.CardType == CardType.Promise || c.CardType == CardType.BurdenGoal);

        if (hasRequestCard)
        {
            session.RequestCardDrawn = true;
        }
    }

    /// <summary>
    /// Update all cards' playability based on current focus availability
    /// Cards that cost more focus than available are marked Unplayable
    /// </summary>
    private void UpdateCardPlayabilityBasedOnFocus(ConversationSession session)
    {
        int availableFocus = _focusManager.AvailableFocus;

        // Check if next speak is free (from observation effect)
        bool isNextSpeakFree = _atmosphereManager.IsNextSpeakFree();

        foreach (CardInstance card in session.Deck.Hand.Cards)
        {
            // Skip request/promise cards - their playability is based on rapport, not focus
            if (card.CardType == CardType.Letter || card.CardType == CardType.Promise || card.CardType == CardType.BurdenGoal)
            {
                continue; // Don't modify request card playability here
            }

            // Calculate effective focus cost for this card
            int effectiveFocusCost = isNextSpeakFree ? 0 : card.Focus;

            // Check if we can afford this card
            bool canAfford = _focusManager.CanAffordCard(effectiveFocusCost);

            // Update playability based on focus availability
            card.IsPlayable = canAfford;
        }
    }

    /// <summary>
    /// Remove all impulse cards from hand (happens after every SPEAK)
    /// Executes exhaust effects before removing cards
    /// </summary>
    private bool RemoveImpulseCardsFromHand(ConversationSession session)
    {
        // Get all impulse cards
        List<CardInstance> impulseCards = session.Deck.Hand.Cards.Where(c => c.Persistence == PersistenceType.Impulse).ToList();

        foreach (CardInstance card in impulseCards)
        {
            // Execute exhaust effect if it exists
            if (card.ExhaustType != ExhaustEffectType.None)
            {
                if (!ExecuteExhaustEffect(card, session))
                {
                    // Exhaust effect ended conversation
                    return false;
                }
            }

            // Remove from active cards and add to exhaust pile
            session.Deck.ExhaustFromHand(card); // HIGHLANDER: Use deck method
        }

        return true; // Conversation continues
    }

    /// <summary>
    /// Exhaust all opening cards in hand (happens on LISTEN)
    /// </summary>
    private bool ExhaustOpeningCards(ConversationSession session)
    {
        // Get all opening cards
        List<CardInstance> openingCards = session.Deck.Hand.Cards
            .Where(c => c.Persistence == PersistenceType.Opening)
            .ToList();

        foreach (CardInstance card in openingCards)
        {
            // Execute exhaust effect if it exists
            if (card.ExhaustType != ExhaustEffectType.None)
            {
                if (!ExecuteExhaustEffect(card, session))
                {
                    // Exhaust effect ended conversation
                    return false;
                }
            }

            // Remove from active cards and add to exhaust pile
            session.Deck.ExhaustFromHand(card); // HIGHLANDER: Use deck method
        }

        return true; // Conversation continues
    }

    /// <summary>
    /// PROJECTION PRINCIPLE: Execute exhaust effect using projection from resolver.
    /// Exhaust effects are ALWAYS penalties (negative effects).
    /// </summary>
    private bool ExecuteExhaustEffect(CardInstance card, ConversationSession session)
    {
        if (card.ExhaustType == ExhaustEffectType.None)
            return true; // No exhaust effect, conversation continues

        // PROJECTION PRINCIPLE: Get projection from resolver
        var projection = _effectResolver.ProcessExhaustEffect(card, session);

        // Apply rapport penalty
        if (projection.RapportChange < 0 && session.RapportManager != null)
        {
            session.RapportManager.ApplyRapportChange(projection.RapportChange, session.CurrentAtmosphere);
        }

        // Apply focus penalty (negative focus)
        if (projection.FocusAdded < 0)
        {
            _focusManager.AddFocus(projection.FocusAdded); // Adding negative = removing
        }

        // Apply card removal penalty (Threading exhaust loses cards from hand)
        if (projection.CardsToAdd?.Count > 0)
        {
            // For exhaust Threading, CardsToAdd represents cards to LOSE from hand
            int cardsToLose = Math.Min(projection.CardsToAdd.Count, session.Deck.Hand.Count);
            for (int i = 0; i < cardsToLose; i++)
            {
                if (session.Deck.Hand.Cards.Any())
                {
                    var cardToRemove = session.Deck.Hand.Cards.First();
                    session.Deck.ExhaustFromHand(cardToRemove); // HIGHLANDER: Use deck method
                }
            }
        }

        return true; // Conversation continues
    }

    /// <summary>
    /// Get unlocked progression cards for an NPC based on current token counts
    /// </summary>
    private List<ConversationCard> GetUnlockedProgressionCards(NPC npc)
    {
        List<ConversationCard> unlockedCards = new List<ConversationCard>();

        if (npc.ProgressionDeck == null)
            return unlockedCards;

        // Get current token counts for this NPC
        Dictionary<ConnectionType, int> tokenCounts = GetNpcTokenCounts(npc);

        // Check each card's unlock requirements
        foreach (ConversationCard card in npc.ProgressionDeck.GetAllCards())
        {
            if (card.RequiredTokenType.HasValue && card.MinimumTokensRequired > 0)
            {
                ConnectionType requiredType = card.RequiredTokenType.Value;
                int currentTokens = tokenCounts.GetValueOrDefault(requiredType, 0);

                if (currentTokens >= card.MinimumTokensRequired)
                {
                    unlockedCards.Add(card);
                }
            }
            else
            {
                // No token requirement - always unlocked
                unlockedCards.Add(card);
            }
        }

        return unlockedCards;
    }

    /// <summary>
    /// Select appropriate goal card from JSON data based on conversation type
    /// </summary>
    private List<CardInstance> SelectGoalCardsForConversationType(NPC npc, ConversationType conversationType, string goalCardId, SessionCardDeck deck)
    {
        List<CardInstance> requestCards = new List<CardInstance>();

        // If specific card ID provided, this might be a request ID - find that request
        if (!string.IsNullOrEmpty(goalCardId) && npc.Requests != null)
        {
            // First check if it's a request ID
            var request = npc.GetRequestById(goalCardId);
            if (request != null && request.IsAvailable())
            {
                // Load ALL cards from the Request bundle

                // Add ALL request cards to be returned for active pile
                foreach (var requestCardId in request.RequestCardIds)
                {
                    // Retrieve the card from GameWorld - single source of truth
                    if (!_gameWorld.AllCardDefinitions.TryGetValue(requestCardId, out var requestCard))
                    {
                        Console.WriteLine($"[ConversationFacade] Warning: Request card ID '{requestCardId}' not found in GameWorld.AllCardDefinitions");
                        continue;
                    }

                    // Create a new template with BurdenGoal type based on the original
                    ConversationCard burdenGoalTemplate = new ConversationCard
                    {
                        Id = requestCard.Id,
                        Description = requestCard.Description,
                        Focus = requestCard.Focus,
                        Difficulty = requestCard.Difficulty,
                        TokenType = requestCard.TokenType,
                        Persistence = requestCard.Persistence,
                        SuccessType = requestCard.SuccessType,
                        FailureType = requestCard.FailureType,
                        ExhaustType = requestCard.ExhaustType,
                        DialogueFragment = requestCard.DialogueFragment,
                        VerbPhrase = requestCard.VerbPhrase,
                        PersonalityTypes = requestCard.PersonalityTypes,
                        LevelBonuses = requestCard.LevelBonuses,
                        MinimumTokensRequired = requestCard.MinimumTokensRequired,
                        RapportThreshold = requestCard.RapportThreshold,
                        QueuePosition = requestCard.QueuePosition,
                        InstantRapport = requestCard.InstantRapport,
                        RequestId = requestCard.RequestId,
                        IsSkeleton = requestCard.IsSkeleton,
                        SkeletonSource = requestCard.SkeletonSource,
                        RequiredTokenType = requestCard.RequiredTokenType,
                        CardType = CardType.BurdenGoal // Override to mark as BurdenGoal
                    };

                    // Create a new card instance with BurdenGoal template
                    CardInstance instance = new CardInstance(burdenGoalTemplate, npc.ID);

                    // Store the rapport threshold and request ID in the card context
                    instance.Context = new CardContext
                    {
                        RapportThreshold = requestCard.RapportThreshold,
                        RequestId = request.Id
                    };

                    // Request cards start as Unplayable until rapport threshold is met
                    instance.IsPlayable = false;

                    requestCards.Add(instance);
                }

                // Add promise cards to the deck for shuffling (not returned)
                foreach (var promiseCardId in request.PromiseCardIds)
                {
                    // Retrieve the card from GameWorld - single source of truth
                    if (!_gameWorld.AllCardDefinitions.TryGetValue(promiseCardId, out var promiseCard))
                    {
                        Console.WriteLine($"[ConversationFacade] Warning: Promise card ID '{promiseCardId}' not found in GameWorld.AllCardDefinitions");
                        continue;
                    }

                    CardInstance promiseInstance = new CardInstance(promiseCard, npc.ID);
                    deck.AddCard(promiseInstance); // Add to deck for shuffling into draw pile
                }

                return requestCards; // Return all request cards for active pile
            }
        }

        // Fallback to existing logic if no specific card ID provided
        switch (conversationType)
        {
            case ConversationType.FriendlyChat:
                // For FriendlyChat, select from NPC's connection token goal cards
                var goalCard = SelectConnectionTokenGoalCard(npc);
                return goalCard != null ? new List<CardInstance> { goalCard } : new List<CardInstance>();

            case ConversationType.Delivery:
                // For Delivery, the goal card is generated based on the letter being delivered
                // This is handled by the obligation system when the delivery conversation starts
                return new List<CardInstance>();

            case ConversationType.Resolution:
                // For Resolution, select from burden resolution cards
                var burdenCard = SelectBurdenResolutionCard(npc);
                return burdenCard != null ? new List<CardInstance> { burdenCard } : new List<CardInstance>();

            default:
                return new List<CardInstance>();
        }
    }

    /// <summary>
    /// Select a connection token goal card from NPC's goal deck
    /// </summary>
    private CardInstance SelectConnectionTokenGoalCard(NPC npc)
    {
        // Connection token goal cards should be in the NPC's one-time requests
        // These are cards that grant connection tokens when played at rapport threshold
        if (npc.Requests == null || !npc.Requests.Any())
            return null;

        // Look for cards with CardType Promise in available requests
        var availableRequests = npc.GetAvailableRequests();
        if (!availableRequests.Any())
            return null;

        // Get all promise cards from all available requests
        List<ConversationCard> goalCards = new List<ConversationCard>();
        foreach (var request in availableRequests)
        {
            // Retrieve promise cards from GameWorld using IDs
            var promiseCards = request.GetPromiseCards(_gameWorld);
            goalCards.AddRange(promiseCards.Where(card => card.CardType == CardType.Promise));
        }

        if (!goalCards.Any())
            return null;

        ConversationCard selectedGoal = goalCards[_random.Next(goalCards.Count)];
        CardInstance goalInstance = new CardInstance(selectedGoal, npc.ID);

        // Store the rapport threshold in the card context (same as Elena's letter)
        if (goalInstance.Context == null)
            goalInstance.Context = new CardContext();

        // Use the rapport threshold from the card itself (from JSON)
        goalInstance.Context.RapportThreshold = selectedGoal.RapportThreshold;

        return goalInstance;
    }

    /// <summary>
    /// Select a burden resolution card for Resolution conversations
    /// </summary>
    private CardInstance SelectBurdenResolutionCard(NPC npc)
    {
        // For now, return null - burden resolution not fully implemented
        return null;
    }

    /// <summary>
    /// Restore session cards from memento data
    /// </summary>
    private void RestoreSessionCards(ConversationSession session, List<string> handCardIds, List<string> deckCardIds)
    {
        // This would restore cards from saved IDs - implementation depends on persistence system
        // For now, leave as stub
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
                DeliveryObligation deliveredObligation = obligations.FirstOrDefault(o => o.Id == card.DeliveryObligationId);

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
                DeliveryObligation urgentLetter = CreateUrgentLetter(_currentSession.NPC);
                _queueManager.AddObligation(urgentLetter);
                _messageSystem.AddSystemMessage(
                    $"{_currentSession.NPC.Name} disconnectedly hands you a letter for her family!",
                    SystemMessageTypes.Success);
                _currentSession.FlowBattery = Math.Min(_currentSession.FlowBattery + 1, 3); // Flow change
                _currentSession.LetterGenerated = true;
            }
        }
    }

    #endregion
}