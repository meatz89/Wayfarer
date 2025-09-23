using Wayfarer.GameState.Enums;
using Wayfarer.Subsystems.ObligationSubsystem;

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
    private readonly FlowManager _flowManager;
    private readonly MomentumManager _momentumManager;
    private readonly CategoricalEffectResolver _effectResolver;
    private readonly ConversationNarrativeService _narrativeService;
    private readonly ConversationDeckBuilder _deckBuilder;

    // External dependencies
    private readonly ObligationQueueManager _queueManager;
    private readonly ObservationManager _observationManager;
    private readonly TimeManager _timeManager;
    private readonly TokenMechanicsManager _tokenManager;
    private readonly MessageSystem _messageSystem;
    private readonly DisplacementCalculator _displacementCalculator;

    private ConversationSession _currentSession;
    private ConversationOutcome _lastOutcome;
    private FlowManager _flowBatteryManager;
    private PersonalityRuleEnforcer _personalityEnforcer;

    public ConversationFacade(
        GameWorld gameWorld,
        ExchangeHandler exchangeHandler,
        FocusManager focusManager,
        AtmosphereManager atmosphereManager,
        FlowManager flowManager,
        MomentumManager momentumManager,
        CategoricalEffectResolver effectResolver,
        ConversationNarrativeService narrativeService,
        ConversationDeckBuilder deckBuilder,
        ObligationQueueManager queueManager,
        ObservationManager observationManager,
        TimeManager timeManager,
        TokenMechanicsManager tokenManager,
        MessageSystem messageSystem,
        DisplacementCalculator displacementCalculator)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _exchangeHandler = exchangeHandler ?? throw new ArgumentNullException(nameof(exchangeHandler));
        _focusManager = focusManager ?? throw new ArgumentNullException(nameof(focusManager));
        _atmosphereManager = atmosphereManager ?? throw new ArgumentNullException(nameof(atmosphereManager));
        _flowManager = flowManager ?? throw new ArgumentNullException(nameof(flowManager));
        _momentumManager = momentumManager ?? throw new ArgumentNullException(nameof(momentumManager));
        _effectResolver = effectResolver ?? throw new ArgumentNullException(nameof(effectResolver));
        _narrativeService = narrativeService ?? throw new ArgumentNullException(nameof(narrativeService));
        _deckBuilder = deckBuilder ?? throw new ArgumentNullException(nameof(deckBuilder));
        _queueManager = queueManager ?? throw new ArgumentNullException(nameof(queueManager));
        _observationManager = observationManager ?? throw new ArgumentNullException(nameof(observationManager));
        _timeManager = timeManager ?? throw new ArgumentNullException(nameof(timeManager));
        _tokenManager = tokenManager ?? throw new ArgumentNullException(nameof(tokenManager));
        _messageSystem = messageSystem ?? throw new ArgumentNullException(nameof(messageSystem));
        _displacementCalculator = displacementCalculator ?? throw new ArgumentNullException(nameof(displacementCalculator));
    }

    /// <summary>
    /// Start a new conversation with an NPC using a specific request
    /// </summary>
    public ConversationSession StartConversation(string npcId, string requestId, List<CardInstance> observationCards = null)
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

        // Get the request that drives this conversation
        NPCRequest request = npc.GetRequestById(requestId);
        if (request == null)
        {
            throw new ArgumentException($"Request {requestId} not found for NPC {npc.Name}");
        }

        // Extract connection state and flow from single value
        ConnectionState initialState = npc.GetConnectionState();
        int initialFlow = npc.GetFlowBattery(); // -2 to +2

        // Initialize flow battery manager with persisted values
        _flowManager.InitializeForConversation(initialState, initialFlow);
        _flowManager.StateTransitioned += OnStateTransitioned;
        _flowManager.ConversationEnded += OnConversationEnded;
        _flowBatteryManager = _flowManager;

        // Initialize focus manager
        _focusManager.SetBaseCapacity(initialState);
        _focusManager.Reset();

        // Reset atmosphere manager
        _atmosphereManager.Reset();

        // Initialize personality rule enforcer based on NPC's personality
        _personalityEnforcer = new PersonalityRuleEnforcer(npc.ConversationModifier ?? new PersonalityModifier { Type = PersonalityModifierType.None });

        // Create session deck and get request cards from the request
        (SessionCardDeck deck, List<CardInstance> requestCards) = _deckBuilder.CreateConversationDeck(npc, requestId, observationCards);

        // Get NPC token counts directly from TokenMechanicsManager
        Dictionary<ConnectionType, int> npcTokens = _tokenManager.GetTokensWithNPC(npc.ID);

        // Initialize momentum manager for this conversation with token data
        _momentumManager.InitializeForConversation(npcTokens);

        // Initialize NPC's daily patience if needed
        if (npc.MaxDailyPatience == 0)
        {
            npc.InitializeDailyPatience();
        }

        // Initialize momentum and doubt for the session
        int initialMomentum = 0;
        int initialDoubt = 0;

        // Get request text from the request
        string requestText = request.NpcRequestText;

        // Create session with new properties
        _currentSession = new ConversationSession
        {
            NPC = npc,
            RequestId = requestId,
            ConversationTypeId = request.ConversationTypeId,
            CurrentState = initialState,
            InitialState = initialState,
            FlowBattery = initialFlow, // Start with persisted flow (-2 to +2)
            CurrentFocus = 0,
            MaxFocus = _focusManager.CurrentCapacity,
            CurrentAtmosphere = AtmosphereType.Neutral,
            CurrentMomentum = initialMomentum,
            CurrentDoubt = initialDoubt,
            TurnNumber = 0,
            Deck = deck, // HIGHLANDER: Deck manages ALL card piles
            TokenManager = _tokenManager,
            FlowManager = _flowBatteryManager,
            MomentumManager = _momentumManager,
            PersonalityEnforcer = _personalityEnforcer,  // Add personality enforcer to session
            ObservationCards = observationCards ?? new List<CardInstance>(),
            RequestText = requestText // Set request text for Request conversations
        };

        // Set up state synchronization between MomentumManager and ConversationSession
        _momentumManager.SetSession(_currentSession);

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

        // Advance time by 1 segment per conversation round (per documentation)
        _timeManager.AdvanceSegments(1);

        // Apply LISTEN mechanics based on conversation type
        ConversationTypeEntry? typeEntry = _gameWorld.ConversationTypes.FindById(_currentSession.ConversationTypeId);
        if (typeEntry?.Definition != null && _currentSession.MomentumManager != null)
        {
            ConversationTypeDefinition conversationType = typeEntry.Definition;

            // 1. Add doubt from conversation type (e.g., +3 for desperate_request)
            if (conversationType.DoubtPerListen > 0)
            {
                _currentSession.MomentumManager.AddDoubt(conversationType.DoubtPerListen);
            }

            // 2. Apply momentum erosion (current doubt reduces momentum)
            if (conversationType.MomentumErosion)
            {
                _currentSession.MomentumManager.ApplyMomentumErosion(_personalityEnforcer);
            }

            // 3. Apply focus penalties (unspent focus adds doubt)
            int unspentFocus = _focusManager.AvailableFocus;
            if (unspentFocus > 0)
            {
                _currentSession.MomentumManager.AddDoubt(unspentFocus);
            }
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
            DoubtLevel = _currentSession.CurrentDoubt,
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

        // Advance time by 1 segment per conversation round (per documentation)
        _timeManager.AdvanceSegments(1);

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
                    DoubtLevel = _currentSession.CurrentDoubt,
                    PersonalityViolation = violationMessage
                };
            }
        }

        // Personality enforcement is now handled within the deterministic success check
        // No need for percentage-based modifications

        // Play the card
        CardPlayResult playResult = PlayCard(_currentSession, selectedCard);

        // Grant XP to player stat based on card's bound stat
        Player player = _gameWorld.GetPlayer();
        if (selectedCard.Template.BoundStat.HasValue)
        {
            // Calculate XP amount based on conversation difficulty/level
            int xpAmount = 1; // Base XP

            // Conversations give 1x/2x/3x XP based on difficulty level
            if (_currentSession.IsStrangerConversation && _currentSession.StrangerLevel.HasValue)
            {
                xpAmount = _currentSession.StrangerLevel.Value; // Stranger level 1-3 = 1-3x XP
            }
            else if (_currentSession.NPC != null)
            {
                // Regular NPC conversation difficulty (1-3 for XP multiplier)
                xpAmount = _currentSession.NPC.ConversationDifficulty;
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
        List<CardInstance> activeCards = _currentSession.Deck.HandCards.ToList();
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
            DoubtLevel = _currentSession.CurrentDoubt,
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
    public async Task<ConversationContextBase> CreateConversationContext(string npcId, string requestId)
    {
        NPC npc = _gameWorld.NPCs.FirstOrDefault(n => n.ID == npcId);
        if (npc == null)
        {
            return ConversationContextFactory.CreateInvalidContext("NPC not found");
        }

        // Get request to determine attention cost
        NPCRequest request = npc.GetRequestById(requestId);
        if (request == null)
        {
            return ConversationContextFactory.CreateInvalidContext($"Request {requestId} not found");
        }


        // Get observation cards from the specific NPC's deck
        // ARCHITECTURE: Each NPC maintains their own observation deck
        // This ensures observations are contextually relevant to conversations
        List<ConversationCard> observationCardsTemplates = _observationManager.GetObservationCardsAsConversationCards(npcId);
        List<CardInstance> observationCards = observationCardsTemplates.Select(card => new CardInstance(card, "observation")).ToList();

        // Start conversation with the request
        ConversationSession session = StartConversation(npcId, requestId, observationCards);

        // Create typed context based on request's conversation type
        ConversationContextBase context = ConversationContextFactory.CreateContext(
            request.ConversationTypeId,
            npc,
            session,
            observationCards,
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
            List<NPCRequest> availableRequests = npc.GetAvailableRequests();

            // Add each available request as a conversation option
            foreach (NPCRequest request in availableRequests)
            {
                // CRITICAL: All requests MUST have valid conversation types defined in JSON
                if (string.IsNullOrEmpty(request.ConversationTypeId))
                {
                    throw new InvalidOperationException($"NPCRequest '{request.Id}' for NPC '{npc.ID}' has no conversationTypeId defined in JSON. All requests must specify a valid conversation type.");
                }

                // Verify the conversation type actually exists
                ConversationTypeEntry? typeEntry = _gameWorld.ConversationTypes.FindById(request.ConversationTypeId);
                if (typeEntry == null)
                {
                    throw new InvalidOperationException($"NPCRequest '{request.Id}' references conversation type '{request.ConversationTypeId}' which does not exist in JSON. All conversation types must be defined.");
                }

                options.Add(new ConversationOption
                {
                    RequestId = request.Id, // Store the actual request ID
                    ConversationTypeId = request.ConversationTypeId, // Use the actual conversation type from JSON
                    GoalCardId = request.Id, // Use request ID to identify which request
                    DisplayName = request.Name,
                    Description = request.Description,
                    TokenType = ConnectionType.Trust, // Default token type
                    MomentumThreshold = 0, // Will check individual card thresholds
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
                    ConversationTypeId = "delivery",
                    GoalCardId = null,
                    DisplayName = "Deliver Letter",
                    Description = "Deliver a letter from your queue",
                    TokenType = ConnectionType.None,
                    MomentumThreshold = 0,
                    CardType = CardType.Letter
                });
            }
        }

        return options;
    }

    /// <summary>
    /// Get available requests for an NPC that can be started as conversations
    /// </summary>
    public List<NPCRequest> GetAvailableRequests(NPC npc)
    {
        List<NPCRequest> available = new List<NPCRequest>();

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

        // Get all available requests from the NPC
        foreach (NPCRequest request in npc.Requests)
        {
            if (request.IsAvailable())
            {
                available.Add(request);
            }
        }

        return available;
    }

    /// <summary>
    /// Get attention cost for a request's conversation type
    /// </summary>

    /// <summary>
    /// Check if a card can be played in the current conversation
    /// </summary>
    public bool CanPlayCard(CardInstance card, ConversationSession session)
    {
        if (card == null || session == null) return false;

        // Check if card is marked as Unplayable
        if (!card.IsPlayable)
            return false;

        // Check focus availability - THIS IS CRITICAL FOR ALL CARDS
        int cardFocus = card.Focus;
        int availableFocus = _focusManager.AvailableFocus;
        bool canAfford = _focusManager.CanAffordCard(cardFocus);

        Console.WriteLine($"[CanPlayCard] Card '{card.Template?.Description}': Focus cost={cardFocus}, Available={availableFocus}, CanAfford={canAfford}");

        if (!canAfford)
            return false;

        // Additional checks for goal cards that are still in RequestPile
        // Cards that have been moved to ActiveCards have already met their threshold
        if (card.CardType == CardType.Letter || card.CardType == CardType.Promise || card.CardType == CardType.BurdenGoal)
        {
            // If card is in RequestPile, check momentum threshold
            if (session.Deck?.IsCardInRequestPile(card) == true)
            {
                int momentumThreshold = card.Context?.MomentumThreshold ?? 0;
                int currentMomentum = session.MomentumManager?.CurrentMomentum ?? 0;
                return currentMomentum >= momentumThreshold;
            }
            // If card is in ActiveCards (hand), it's already playable (threshold was met)
        }

        // All other checks passed, card can be played
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
        // End if doubt at maximum (10 pips filled)
        if (session.CurrentDoubt >= session.MaxDoubt)
            return true;

        // Check with flow battery manager
        if (_flowBatteryManager != null &&
            _flowBatteryManager.CurrentState == ConnectionState.DISCONNECTED &&
            _flowBatteryManager.CurrentFlow <= -3)
            return true;

        // End if deck is empty and no active cards
        if (!session.Deck.HasCardsAvailable() && session.Deck.HandSize == 0)
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
        if (session.CurrentDoubt >= session.MaxDoubt)
        {
            success = false;
            reason = "Doubt overwhelmed conversation";
        }
        else if (session.CurrentState == ConnectionState.DISCONNECTED && session.FlowBattery <= -3)
        {
            success = false;
            reason = "Relationship damaged beyond repair";
        }

        // Calculate token rewards based on final state
        int tokensEarned = CalculateTokenReward(session.CurrentState, session.FlowBattery);

        // Check if any request cards were played (Letter, Promise, or BurdenGoal types)
        bool requestAchieved = session.Deck.PlayedHistoryCards.Any(c =>
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

        // Find recipient (deterministic - first available NPC)
        List<NPC> otherNpcs = _gameWorld.NPCs.Where(n => n.ID != session.NPC.ID).ToList();
        NPC recipient = otherNpcs.FirstOrDefault();

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
    /// Determine the connection type based on the conversation type and outcome
    /// </summary>
    private ConnectionType DetermineConnectionTypeFromConversation(ConversationSession session)
    {
        // Map conversation types to their corresponding connection types
        return session.ConversationTypeId switch
        {
            // Commerce removed - exchanges use separate Exchange system
            "request" => ConnectionType.Trust, // Request bundles with promise cards
            "resolution" => ConnectionType.Trust,
            "delivery" => ConnectionType.Trust,
            "friendly_chat" => ConnectionType.Trust,
            _ => ConnectionType.Trust  // Default fallback
        };
    }

    #endregion

    #region Private Methods - Absorbed from CardDeckManager


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
        int baseDrawCount = session.GetDrawCount();

        // Apply impulse penalty: each unplayed Impulse card reduces draw by 1
        int impulseCount = session.Deck.HandCards.Count(c => c.Persistence == PersistenceType.Impulse);
        int finalDrawCount = Math.Max(1, baseDrawCount - impulseCount); // Minimum 1 card draw

        // HIGHLANDER: Draw directly to hand
        session.Deck.DrawToHand(finalDrawCount);

        // Get the drawn cards for return value
        List<CardInstance> drawnCards = session.Deck.HandCards.TakeLast(finalDrawCount).ToList();

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

        // DETERMINISTIC: Check success based on clear rules (no randomness)
        bool success = _effectResolver.CheckCardSuccess(selectedCard, session);

        // Mark request as completed if this is a BurdenGoal (request) card and it succeeds
        if (success && selectedCard.CardType == CardType.BurdenGoal && selectedCard.Context?.RequestId != null)
        {
            // Find and complete the request
            NPCRequest request = session.NPC.GetRequestById(selectedCard.Context.RequestId);
            if (request != null)
            {
                request.Complete();
                // The conversation will end after this card is played
            }
        }

        // Spend focus (possibly 0 if free) - focus represents effort of speaking
        _focusManager.SpendFocus(focusCost);
        session.CurrentFocus = _focusManager.CurrentSpentFocus;

        // Update card playability immediately after spending focus
        UpdateCardPlayabilityBasedOnFocus(session);

        CardEffectResult effectResult = null;
        int flowChange = 0;

        if (success)
        {
            // Flow only changes from explicit "Advancing" effect type (no automatic changes)
            flowChange = 0;

            // Reset bad luck protection on success would go here if implemented

            // Process card's success effect
            // PROJECTION PRINCIPLE: Get projection from resolver and apply it
            effectResult = _effectResolver.ProcessSuccessEffect(selectedCard, session);

            // Apply momentum/doubt changes based on card effects
            if (effectResult.MomentumChange > 0 && session.MomentumManager != null)
            {
                session.MomentumManager.AddMomentum(effectResult.MomentumChange, session.CurrentAtmosphere);
            }
            if (effectResult.DoubtChange > 0 && session.MomentumManager != null)
            {
                session.MomentumManager.AddDoubt(effectResult.DoubtChange, session.CurrentAtmosphere);
            }
            if (effectResult.DoubtChange < 0 && session.MomentumManager != null)
            {
                session.MomentumManager.ReduceDoubt(-effectResult.DoubtChange, session.CurrentAtmosphere);
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
                session.Deck.AddCardsToHand(effectResult.CardsToAdd);
            }

            // Handle atmosphere change (for Atmospheric success effect)
            if (effectResult.AtmosphereTypeChange.HasValue)
            {
                _atmosphereManager.SetAtmosphere(effectResult.AtmosphereTypeChange.Value);
            }

            // Handle Promise card queue manipulation (for Promising success effect)
            if (selectedCard.SuccessType == SuccessEffectType.Promising)
            {
                HandlePromiseCardQueueManipulation(selectedCard, session);
            }

            // Consume one-time atmosphere effects after successful card play
            _atmosphereManager.OnCardSuccess();
        }
        else
        {
            // Flow only changes from explicit "Advancing" effect type (no automatic changes)
            flowChange = 0;

            // Bad luck protection tracking would go here if implemented

            // PROJECTION PRINCIPLE: Get projection from resolver
            effectResult = _effectResolver.ProcessFailureEffect(selectedCard, session);

            // Apply doubt on failure (standard failure adds 1 doubt)
            if (session.MomentumManager != null)
            {
                session.MomentumManager.AddDoubt(1, session.CurrentAtmosphere);
            }

            // Apply other failure momentum/doubt changes if any
            if (effectResult.MomentumChange < 0 && session.MomentumManager != null)
            {
                session.MomentumManager.LoseMomentum(-effectResult.MomentumChange);
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

        // Remove observation card from NPC's observation deck if it was played
        // ARCHITECTURE: Observations are stored per-NPC, not globally on player
        // This ensures observations are contextually relevant to specific NPCs
        if (selectedCard.CardType == CardType.Observation && session.NPC != null)
        {
            // Observation cards are consumed when played - remove from NPC's observation deck
            string observationCardId = selectedCard.Id;
            ConversationCard cardToRemove = session.NPC.ObservationDeck?.GetAllCards()
                .FirstOrDefault(c => c.Id == observationCardId);

            if (cardToRemove != null)
            {
                session.NPC.ObservationDeck.RemoveCard(cardToRemove);
                Console.WriteLine($"[ConversationFacade] Consumed observation card {observationCardId} from {session.NPC.Name}'s deck");
            }
        }

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
                    Roll = 0, // No dice rolls in deterministic system
                    SuccessChance = success ? 100 : 0 // Deterministic: either succeeds or fails
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
    /// Handle Promise card queue manipulation - moves target obligation to position 1
    /// and burns tokens with all displaced NPCs
    /// </summary>
    private void HandlePromiseCardQueueManipulation(CardInstance promiseCard, ConversationSession session)
    {
        // Promise cards manipulate the queue mid-conversation
        // They force a specific obligation to position 1, burning tokens with displaced NPCs

        if (_displacementCalculator == null || _queueManager == null)
        {
            Console.WriteLine("[ConversationFacade] Cannot manipulate queue - displacement or queue manager not available");
            return;
        }

        // Find the target obligation for this promise card
        // Promise cards are typically associated with a specific NPC's request
        DeliveryObligation targetObligation = FindTargetObligationForPromise(promiseCard, session);

        if (targetObligation == null)
        {
            Console.WriteLine("[ConversationFacade] No target obligation found for promise card");
            return;
        }

        // Calculate the current position of the target obligation
        int currentPosition = GetObligationPosition(targetObligation);

        if (currentPosition <= 0)
        {
            Console.WriteLine("[ConversationFacade] Target obligation not in queue");
            return;
        }

        if (currentPosition == 1)
        {
            Console.WriteLine("[ConversationFacade] Obligation already at position 1, no displacement needed");
            return;
        }

        // Execute automatic displacement to position 1
        string displacementReason = $"Promise made to {session.NPC?.Name ?? "unknown"} - immediate action guaranteed";
        DisplacementResult result = _displacementCalculator.ExecuteAutomaticDisplacement(
            targetObligation,
            1, // Force to position 1
            displacementReason
        );

        if (result.CanExecute)
        {
            _messageSystem.AddSystemMessage(
                $"💫 Your promise to {session.NPC?.Name} moves their letter to the front of the queue!",
                SystemMessageTypes.Success
            );

            // The displacement calculator already handled token burning and burden cards
            Console.WriteLine($"[ConversationFacade] Promise card successfully moved obligation to position 1");
        }
        else
        {
            Console.WriteLine($"[ConversationFacade] Failed to execute promise displacement: {result.ErrorMessage}");
        }
    }

    /// <summary>
    /// Find the target obligation for a promise card
    /// </summary>
    private DeliveryObligation FindTargetObligationForPromise(CardInstance promiseCard, ConversationSession session)
    {
        // Promise cards are associated with the NPC in the current conversation
        // Find an obligation from this NPC in the queue

        if (session.NPC == null || _queueManager == null)
            return null;

        DeliveryObligation[] activeObligations = _queueManager.GetActiveObligations();

        // Look for an obligation from the current NPC
        // Priority: obligations where this NPC is the sender
        DeliveryObligation targetObligation = activeObligations.FirstOrDefault(o =>
            o != null && (o.SenderId == session.NPC.ID || o.SenderName == session.NPC.Name));

        if (targetObligation == null)
        {
            // Fallback: look for obligations where this NPC is the recipient
            targetObligation = activeObligations.FirstOrDefault(o =>
                o != null && (o.RecipientId == session.NPC.ID || o.RecipientName == session.NPC.Name));
        }

        return targetObligation;
    }

    /// <summary>
    /// Get the current position of an obligation in the queue
    /// </summary>
    private int GetObligationPosition(DeliveryObligation obligation)
    {
        if (obligation == null || _gameWorld == null)
            return -1;

        DeliveryObligation[] queue = _gameWorld.GetPlayer().ObligationQueue;
        for (int i = 0; i < queue.Length; i++)
        {
            if (queue[i]?.Id == obligation.Id)
            {
                return i + 1; // Return 1-based position
            }
        }

        return -1; // Not found
    }

    /// <summary>
    /// Check if goal cards should become playable after LISTEN based on momentum threshold
    /// </summary>
    private void UpdateGoalCardPlayabilityAfterListen(ConversationSession session)
    {
        // Get current momentum
        int currentMomentum = session.MomentumManager?.CurrentMomentum ?? 0;

        // Check all goal cards in active hand
        foreach (CardInstance card in session.Deck.HandCards)
        {
            // Only process goal cards that are currently Unplayable
            if ((card.CardType == CardType.Letter || card.CardType == CardType.Promise || card.CardType == CardType.BurdenGoal)
                && !card.IsPlayable)
            {
                // Check if momentum threshold is met
                int momentumThreshold = card.Context?.MomentumThreshold ?? 0;

                if (currentMomentum >= momentumThreshold)
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
        bool hasRequestCard = session.Deck.HandCards
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

        foreach (CardInstance card in session.Deck.HandCards)
        {
            // Skip request/promise cards - their playability is based on momentum, not focus
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
        List<CardInstance> impulseCards = session.Deck.HandCards.Where(c => c.Persistence == PersistenceType.Impulse).ToList();

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
        List<CardInstance> openingCards = session.Deck.HandCards
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
        CardEffectResult projection = _effectResolver.ProcessExhaustEffect(card, session);

        // Apply doubt penalty on exhaust
        if (projection.DoubtChange > 0 && session.MomentumManager != null)
        {
            session.MomentumManager.AddDoubt(projection.DoubtChange, session.CurrentAtmosphere);
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
            int cardsToLose = Math.Min(projection.CardsToAdd.Count, session.Deck.HandSize);
            for (int i = 0; i < cardsToLose; i++)
            {
                if (session.Deck.HandCards.Any())
                {
                    CardInstance cardToRemove = session.Deck.HandCards.First();
                    session.Deck.ExhaustFromHand(cardToRemove); // HIGHLANDER: Use deck method
                }
            }
        }

        return true; // Conversation continues
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
                    // Flow no longer changes automatically - only from explicit "Advancing" cards

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
                // Flow no longer changes automatically - only from explicit "Advancing" cards
                _currentSession.LetterGenerated = true;
            }
        }
    }

    /// <summary>
    /// Check and move request cards to hand if momentum threshold is met
    /// This should only be called by UI components, never directly on Session
    /// </summary>
    public List<CardInstance> CheckAndMoveRequestCards()
    {
        if (_currentSession == null || _currentSession.Deck == null)
        {
            return new List<CardInstance>();
        }

        int currentMomentum = _currentSession.MomentumManager?.CurrentMomentum ?? 0;
        List<CardInstance> movedCards = _currentSession.Deck.CheckRequestThresholds(currentMomentum);

        // Notify about moved cards
        foreach (CardInstance card in movedCards)
        {
            card.IsPlayable = true;
            _messageSystem.AddSystemMessage(
                $"{card.Description} is now available (Momentum threshold met)",
                SystemMessageTypes.Success);
        }

        return movedCards;
    }

    #region UI Access Methods - Encapsulated Deck Access

    /// <summary>
    /// Get current hand cards (read-only) for UI display
    /// UI should NEVER access Session.Deck directly
    /// </summary>
    public IReadOnlyList<CardInstance> GetHandCards()
    {
        if (_currentSession?.Deck == null)
            return new List<CardInstance>();

        return _currentSession.Deck.HandCards;
    }

    /// <summary>
    /// Get request pile cards (read-only) for UI display
    /// </summary>
    public IReadOnlyList<CardInstance> GetRequestCards()
    {
        if (_currentSession?.Deck == null)
            return new List<CardInstance>();

        return _currentSession.Deck.RequestCards;
    }

    /// <summary>
    /// Get played cards history (read-only) for UI display
    /// </summary>
    public IReadOnlyList<CardInstance> GetPlayedHistory()
    {
        if (_currentSession?.Deck == null)
            return new List<CardInstance>();

        return _currentSession.Deck.PlayedHistoryCards;
    }

    /// <summary>
    /// Check if a card is in the hand
    /// </summary>
    public bool IsCardInHand(CardInstance card)
    {
        if (_currentSession?.Deck == null || card == null)
            return false;

        return _currentSession.Deck.IsCardInHand(card);
    }

    /// <summary>
    /// Check if a card is in the request pile
    /// </summary>
    public bool IsCardInRequestPile(CardInstance card)
    {
        if (_currentSession?.Deck == null || card == null)
            return false;

        return _currentSession.Deck.IsCardInRequestPile(card);
    }

    /// <summary>
    /// Get the current hand size
    /// </summary>
    public int GetHandSize()
    {
        if (_currentSession?.Deck == null)
            return 0;

        return _currentSession.Deck.HandSize;
    }

    /// <summary>
    /// Get the current request pile size
    /// </summary>
    public int GetRequestPileSize()
    {
        if (_currentSession?.Deck == null)
            return 0;

        return _currentSession.Deck.RequestPileSize;
    }

    /// <summary>
    /// Get deck statistics for UI display
    /// </summary>
    public (int drawPile, int discardPile, int handSize, int requestPile) GetDeckStatistics()
    {
        if (_currentSession?.Deck == null)
            return (0, 0, 0, 0);

        return (
            _currentSession.Deck.RemainingDrawCards,
            _currentSession.Deck.DiscardPileCount,
            _currentSession.Deck.HandSize,
            _currentSession.Deck.RequestPileSize
        );
    }

    /// <summary>
    /// Check if there are cards available to draw
    /// </summary>
    public bool HasCardsAvailable()
    {
        if (_currentSession?.Deck == null)
            return false;

        return _currentSession.Deck.HasCardsAvailable();
    }

    #endregion

    #endregion
}