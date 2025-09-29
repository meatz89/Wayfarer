
using Wayfarer.Subsystems.ObligationSubsystem;

/// <summary>
/// Public API for the Conversation subsystem.
/// Handles all conversation operations with functionality absorbed from ConversationOrchestrator and CardDeckManager.
/// </summary>
public class ConversationFacade
{
    private readonly GameWorld _gameWorld;
    private readonly ExchangeHandler _exchangeHandler;
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
    private PersonalityRuleEnforcer _personalityEnforcer;

    public ConversationFacade(
        GameWorld gameWorld,
        ExchangeHandler exchangeHandler,
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

        // Get connection state from NPC for session initialization
        ConnectionState initialState = npc.GetConnectionState();

        // Focus management is now handled directly by ConversationSession

        // Initialize personality rule enforcer based on NPC's personality
        _personalityEnforcer = new PersonalityRuleEnforcer(npc.ConversationModifier ?? new PersonalityModifier { Type = PersonalityModifierType.None });

        // Get NPC token counts for session initialization
        Dictionary<ConnectionType, int> npcTokens = _tokenManager.GetTokensWithNPC(npc.ID);

        // Create session deck and get request cards from the request
        (SessionCardDeck deck, List<CardInstance> requestCards) = _deckBuilder.CreateConversationDeck(npc, requestId, observationCards);

        // Initialize momentum manager for this conversation with token data
        _momentumManager.InitializeForConversation(npcTokens);


        // Initialize momentum and doubt for the session
        // Starting momentum formula: 2 + floor(highest_stat / 3)
        Player player = _gameWorld.GetPlayer();
        int startingMomentum = 2 + (player.Stats.GetHighestStatLevel() / 3);
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
            CurrentInitiative = 0, // Starts at 0             Cadence = 0, // Starts at 0
            CurrentMomentum = startingMomentum,
            CurrentDoubt = initialDoubt,
            TurnNumber = 0,
            Deck = deck, // HIGHLANDER: Deck manages ALL card piles
            TokenManager = _tokenManager,
            MomentumManager = _momentumManager,
            PersonalityEnforcer = _personalityEnforcer,  // Add personality enforcer to session
            ObservationCards = observationCards ?? new List<CardInstance>(),
            RequestText = requestText // Set request text for Request conversations
        };

        // Set up state synchronization between MomentumManager and ConversationSession
        _momentumManager.SetSession(_currentSession);

        // THEN: Perform initial draw of regular cards with momentum filtering
        // This is the initial conversation start, so we just draw cards without exhausting
        int drawCount = _currentSession.GetDrawCount();
        // Draw with momentum-based filtering
        _currentSession.Deck.DrawToHand(drawCount, _currentSession.CurrentMomentum, player.Stats);

        // Update request card playability based on initiative
        UpdateRequestCardPlayability(_currentSession);

        // Initialize Initiative to 0 (4-resource system)
        _currentSession.CurrentInitiative = 0; // Initiative starts at 0 
        // Update card playability based on initial initiative
        UpdateCardPlayabilityBasedOnInitiative(_currentSession);

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

        // Set relationship flow based on connection state only
        _currentSession.NPC.RelationshipFlow = stateBase;

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
    /// Execute LISTEN action - Complete 4-Resource System Implementation
    /// Sequence: Apply Cadence Effects → Handle Card Persistence → Fixed Card Draw → Refresh Initiative → Check Goal Cards
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

        // ========== 4-RESOURCE SYSTEM LISTEN SEQUENCE ==========

        // 1. Apply Cadence Effects
        ProcessCadenceEffectsOnListen(_currentSession);

        // 2. Apply Doubt Tax on Momentum (handled automatically via GetEffectiveMomentumGain)
        // Doubt reduces momentum gains by 20% per doubt point - applied when momentum is added

        // 3. Handle Card Persistence
        ProcessCardPersistence(_currentSession);

        // 4. Calculate Fixed Card Draw (4 + Cadence bonus)
        int cardsToDraw = _currentSession.GetDrawCount();

        // 5. NO Initiative refresh (must be earned through cards like Steamworld Quest)
        // Initiative stays at current value - only Foundation cards can build it

        // 6. Check Goal Card Activation
        CheckGoalCardActivation(_currentSession);

        // 7. Reset Turn-Based Effects
        _personalityEnforcer?.OnListen(); // Resets Proud personality turn state

        // Draw cards from deck
        List<CardInstance> drawnCards = ExecuteNewListenCardDraw(_currentSession, cardsToDraw);

        // Update card playability based on Initiative system
        UpdateCardPlayabilityForInitiative(_currentSession);

        // Generate narrative using the narrative service
        NarrativeOutput narrative = await _narrativeService.GenerateNarrativeAsync(
            _currentSession,
            _currentSession.NPC,
            drawnCards);
        string npcResponse = narrative.NPCDialogue;

        return new ConversationTurnResult
        {
            Success = true,
            NewState = _currentSession.CurrentState, // Connection State doesn't change during conversation
            NPCResponse = npcResponse,
            DrawnCards = drawnCards,
            DoubtLevel = _currentSession.CurrentDoubt,
            Narrative = narrative
        };
    }

    /// <summary>
    /// Execute SPEAK action - Complete 4-Resource System Implementation
    /// Sequence: Validate Initiative Cost → Check Personality Rules → Pay Cost → Apply Cadence → Calculate Success → Apply Results
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

        // ========== 4-RESOURCE SYSTEM SPEAK SEQUENCE ==========

        // 1. Check Initiative Available
        int initiativeCost = GetCardInitiativeCost(selectedCard);
        if (!_currentSession.CanAffordCardInitiative(initiativeCost))
        {
            // Not enough Initiative - cannot play card
            return new ConversationTurnResult
            {
                Success = false,
                NewState = _currentSession.CurrentState,
                NPCResponse = "You don't have enough Initiative to play that card. Use Foundation cards to build Initiative.",
                DoubtLevel = _currentSession.CurrentDoubt,
                PersonalityViolation = "Insufficient Initiative"
            };
        }

        // 2. Check Personality Restrictions (updated for Initiative system)
        if (_personalityEnforcer != null)
        {
            string violationMessage;
            if (!ValidateInitiativePersonalityRules(selectedCard, out violationMessage))
            {
                return new ConversationTurnResult
                {
                    Success = false,
                    NewState = _currentSession.CurrentState,
                    NPCResponse = violationMessage,
                    DoubtLevel = _currentSession.CurrentDoubt,
                    PersonalityViolation = violationMessage
                };
            }
        }

        // 3. Pay Card Cost (Initiative)
        if (!_currentSession.SpendInitiative(initiativeCost))
        {
            // This should never happen due to check above, but safety check
            return new ConversationTurnResult
            {
                Success = false,
                NewState = _currentSession.CurrentState,
                NPCResponse = "Failed to spend Initiative for card play.",
                DoubtLevel = _currentSession.CurrentDoubt
            };
        }

        // 4. Apply Cadence Change (-1 per card played)
        _currentSession.ApplyCadenceFromSpeak();

        // 5. Calculate Success
        bool success = CalculateInitiativeCardSuccess(selectedCard, _currentSession);

        // 6. Process Card Results
        CardPlayResult playResult = ProcessInitiativeCardPlay(selectedCard, success, _currentSession);

        // 7. Grant XP to player stat (unchanged)
        Player player = _gameWorld.GetPlayer();
        if (selectedCard.ConversationCardTemplate.BoundStat.HasValue)
        {
            int xpAmount = CalculateXPAmount(_currentSession);
            player.Stats.AddXP(selectedCard.ConversationCardTemplate.BoundStat.Value, xpAmount);
        }

        // 8. Record card played for personality tracking
        _personalityEnforcer?.OnCardPlayed(selectedCard);

        // 9. Handle Card Persistence (Standard/Echo/Persistent/Banish)
        ProcessCardAfterPlay(selectedCard, success, _currentSession);

        // 10. Update card playability based on new Initiative level
        UpdateCardPlayabilityForInitiative(_currentSession);

        // 11. Handle special card effects (exchanges, letters, etc.)
        HandleSpecialCardEffects(new HashSet<CardInstance> { selectedCard }, new ConversationTurnResult { Success = success });

        // Generate NPC response through narrative service
        List<CardInstance> activeCards = _currentSession.Deck.HandCards.ToList();
        NarrativeOutput narrative = await _narrativeService.GenerateNarrativeAsync(
            _currentSession,
            _currentSession.NPC,
            activeCards);

        ConversationTurnResult result = new ConversationTurnResult
        {
            Success = success,
            NewState = _currentSession.CurrentState, // Connection State doesn't change
            NPCResponse = narrative.NPCDialogue,
            FlowChange = 0, // No flow             OldFlow = 0, // No flow             NewFlow = 0, // No flow             DoubtLevel = _currentSession.CurrentDoubt,
            CardPlayResult = playResult,
            Narrative = narrative
        };

        // Add turn to history
        AddTurnToHistory(ActionType.Speak, selectedCard, result);

        // Check if conversation should end (doubt at maximum)
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

        // Check Initiative availability - THIS IS CRITICAL FOR ALL CARDS
        int cardInitiative = GetCardInitiativeCost(card);
        int availableInitiative = session.GetCurrentInitiative();
        bool canAfford = session.CanAffordCard(cardInitiative);

        // Console.WriteLine($"[CanPlayCard] Card '{card.Template?.Description}': Focus cost={cardFocus}, Available={availableFocus}, CanAfford={canAfford}"); // Removed excessive logging

        if (!canAfford)
            return false;

        // Additional checks for goal cards that are still in RequestPile
        // Cards that have been moved to ActiveCards have already met their threshold
        if (card.ConversationCardTemplate.CardType == CardType.Letter || card.ConversationCardTemplate.CardType == CardType.Promise || card.ConversationCardTemplate.CardType == CardType.Letter)
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

        // Check initiative cost against available initiative
        int currentInitiativeCost = currentSelection.Sum(c => GetCardInitiativeCost(c));
        int totalInitiativeCost = currentInitiativeCost + GetCardInitiativeCost(card);

        return totalInitiativeCost <= _currentSession.GetCurrentInitiative();
    }

    public ExchangeExecutionResult ExecuteExchange(object exchangeData)
    {
        if (!IsConversationActive())
        {
            return ExchangeExecutionResult.Failed("No active conversation");
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

            return success ? ExchangeExecutionResult.Successful() : ExchangeExecutionResult.Failed("Exchange execution failed");
        }
        catch (Exception ex)
        {
            _messageSystem.AddSystemMessage($"Exchange failed: {ex.Message}", SystemMessageTypes.Warning);
            return ExchangeExecutionResult.Failed(ex.Message);
        }
    }

    public Task<bool> EndConversationAsync()
    {
        ConversationOutcome outcome = EndConversation();
        return Task.FromResult(outcome != null);
    }


    // AtmosphereManager has been deleted - atmosphere is simplified to always Neutral

    #region 4-Resource System Helper Methods

    /// <summary>
    /// Process Cadence effects on LISTEN action - NEW REFACTORED SYSTEM
    /// 1. Calculate doubt to clear
    /// 2. Reset doubt to 0
    /// 3. Reduce momentum by doubt cleared
    /// 4. Apply -3 Cadence (LISTEN decreases Cadence)
    /// </summary>
    private void ProcessCadenceEffectsOnListen(ConversationSession session)
    {
        // NEW REFACTORED LISTEN MECHANICS:
        // 1. Calculate doubt that will be cleared
        int doubtCleared = session.CurrentDoubt;

        // 2. Reset doubt to 0 (complete relief)
        session.CurrentDoubt = 0;

        // 3. Reduce momentum by amount of doubt cleared (minimum 0)
        session.CurrentMomentum = Math.Max(0, session.CurrentMomentum - doubtCleared);

        // 4. Apply Cadence change (-3 for LISTEN action)
        session.Cadence = Math.Max(-5, session.Cadence - 3); // Changed from -2 to -3
    }

    /// <summary>
    /// Handle card persistence after playing
    /// Standard: Goes to Spoken pile
    /// Echo: Returns to hand if conditions met
    /// Persistent: Stays in hand
    /// Banish: Removed entirely
    /// </summary>
    private void ProcessCardPersistence(ConversationSession session)
    {
        // Handle cards that need persistence processing
        // This is handled by the deck system based on card persistence types
        session.Deck.ProcessCardPersistence();
    }

    /// <summary>
    /// Check if goal cards should become active based on momentum thresholds
    /// Basic: 8, Enhanced: 12, Premium: 16
    /// </summary>
    private void CheckGoalCardActivation(ConversationSession session)
    {
        int currentMomentum = session.CurrentMomentum;

        // Move request cards that meet momentum threshold from request pile to hand
        List<CardInstance> activatedCards = session.Deck.CheckRequestThresholds(currentMomentum);

        foreach (CardInstance card in activatedCards)
        {
            _messageSystem.AddSystemMessage(
                $"{card.ConversationCardTemplate.Title} is now available (Momentum threshold met)",
                SystemMessageTypes.Success);
        }
    }

    /// <summary>
    /// Execute card draw with momentum-based filtering
    /// </summary>
    private List<CardInstance> ExecuteNewListenCardDraw(ConversationSession session, int cardsToDraw)
    {
        // Draw with momentum and stat filtering
        Player player = _gameWorld.GetPlayer();
        session.Deck.DrawToHand(cardsToDraw, session.CurrentMomentum, player.Stats);

        // Return the newly drawn cards (last N cards in hand)
        return session.Deck.HandCards.TakeLast(cardsToDraw).ToList();
    }

    /// <summary>
    /// Update card playability based on Initiative system (not Focus)
    /// </summary>
    private void UpdateCardPlayabilityForInitiative(ConversationSession session)
    {
        int currentInitiative = session.CurrentInitiative;

        foreach (CardInstance card in session.Deck.HandCards)
        {
            // Skip request cards - their playability is based on momentum thresholds
            if (card.ConversationCardTemplate.CardType == CardType.Letter || card.ConversationCardTemplate.CardType == CardType.Promise || card.ConversationCardTemplate.CardType == CardType.Letter)
            {
                continue;
            }

            // Check if player can afford this card's Initiative cost
            int initiativeCost = GetCardInitiativeCost(card);
            card.IsPlayable = currentInitiative >= initiativeCost;
        }
    }

    /// <summary>
    /// Get Initiative cost for a card (replaces Focus cost)
    /// </summary>
    private int GetCardInitiativeCost(CardInstance card)
    {
        return card.ConversationCardTemplate?.InitiativeCost ?? 0;
    }

    /// <summary>
    /// Validate personality rules for Initiative system
    /// Proud: Ascending Initiative order (not Focus)
    /// Mercantile: Highest Initiative card gets +30% success
    /// </summary>
    private bool ValidateInitiativePersonalityRules(CardInstance selectedCard, out string violationMessage)
    {
        violationMessage = string.Empty;

        if (_personalityEnforcer == null)
            return true;

        // Use existing personality enforcer but it will need updating for Initiative
        return _personalityEnforcer.ValidatePlay(selectedCard, out violationMessage);
    }

    /// <summary>
    /// Calculate card success with Initiative system
    /// Base% + (2% × Current Momentum) + (10% × Bound Stat Level)
    /// </summary>
    private bool CalculateInitiativeCardSuccess(CardInstance selectedCard, ConversationSession session)
    {
        // Use existing deterministic success calculation for now
        // This handles momentum and stat bonuses correctly
        return _effectResolver.CheckCardSuccess(selectedCard, session);
    }

    /// <summary>
    /// Process card play results with Initiative system
    /// </summary>
    private CardPlayResult ProcessInitiativeCardPlay(CardInstance selectedCard, bool success, ConversationSession session)
    {
        CardEffectResult effectResult = null;

        if (success)
        {
            // Process success effects with doubt tax applied to momentum
            effectResult = _effectResolver.ProcessSuccessEffect(selectedCard, session);

            // Apply momentum with doubt tax
            if (effectResult.MomentumChange > 0)
            {
                int effectiveMomentum = session.GetEffectiveMomentumGain(effectResult.MomentumChange);
                session.CurrentMomentum += effectiveMomentum;
            }

            // Apply doubt changes
            if (effectResult.DoubtChange > 0)
            {
                session.AddDoubt(effectResult.DoubtChange);
            }
            else if (effectResult.DoubtChange < 0)
            {
                session.ReduceDoubt(-effectResult.DoubtChange);
            }

            // Apply Initiative generation (for Foundation cards)
            if (effectResult.InitiativeChange > 0) // Repurpose focus as Initiative
            {
                session.AddInitiative(effectResult.InitiativeChange);
            }

            // Handle other card effects (drawing cards, etc.)
            if (effectResult.CardsToAdd.Any())
            {
                session.Deck.AddCardsToMind(effectResult.CardsToAdd);
            }
        }

        // Create play result
        return new CardPlayResult
        {
            Results = new List<SingleCardResult>
            {
                new SingleCardResult
                {
                    Card = selectedCard,
                    Success = success,
                    Flow = 0, // No flow
                    Roll = 0, // Deterministic system
                    SuccessChance = success ? 100 : 0
                }
            },
            MomentumGenerated = 0 // No flow
        };
    }

    /// <summary>
    /// Process card after playing based on persistence type
    /// </summary>
    private void ProcessCardAfterPlay(CardInstance selectedCard, bool success, ConversationSession session)
    {
        // Handle card based on its persistence type
        session.Deck.PlayCard(selectedCard);
    }

    /// <summary>
    /// Calculate XP amount based on conversation difficulty
    /// </summary>
    private int CalculateXPAmount(ConversationSession session)
    {
        if (session.IsStrangerConversation && session.StrangerLevel.HasValue)
        {
            return session.StrangerLevel.Value; // 1-3x XP
        }
        else if (session.NPC != null)
        {
            return session.NPC.ConversationDifficulty; // 1-3x XP
        }

        return 1; // Base XP
    }

    /// <summary>
    /// Add turn to conversation history
    /// </summary>
    private void AddTurnToHistory(ActionType actionType, CardInstance cardPlayed, ConversationTurnResult result)
    {
        if (_currentSession != null)
        {
            ConversationTurn turn = new ConversationTurn
            {
                ActionType = actionType,
                Narrative = result.Narrative,
                Result = result,
                TurnNumber = _currentSession.TurnNumber,
                CardPlayed = cardPlayed
            };
            _currentSession.TurnHistory.Add(turn);
        }
    }

    #endregion

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
        if (session.CurrentState == ConnectionState.DISCONNECTED &&
            session.CurrentDoubt >= session.MaxDoubt)
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
        else if (session.CurrentState == ConnectionState.DISCONNECTED && session.CurrentDoubt >= session.MaxDoubt)
        {
            success = false;
            reason = "Relationship damaged beyond repair";
        }

        // Calculate token rewards based on final state
        int tokensEarned = CalculateTokenReward(session.CurrentState, session.CurrentMomentum);

        // Check if any request cards were played (Letter, Promise, or BurdenGoal types)
        bool requestAchieved = session.Deck.SpokenCards.Any(c =>
            c.ConversationCardTemplate.CardType == CardType.Letter ||
            c.ConversationCardTemplate.CardType == CardType.Promise ||
            c.ConversationCardTemplate.CardType == CardType.Letter);
        if (requestAchieved)
        {
            tokensEarned += 2; // Bonus for completing request
        }

        return new ConversationOutcome
        {
            Success = success,
            FinalMomentum = session.CurrentMomentum,
            FinalDoubt = session.CurrentDoubt,
            FinalInitiative = session.CurrentInitiative,
            FinalCadence = session.Cadence,
            TokensEarned = tokensEarned,
            RequestAchieved = requestAchieved,
            Reason = reason
        };
    }

    /// <summary>
    /// Calculate token reward based on final state and momentum
    /// </summary>
    private int CalculateTokenReward(ConnectionState finalState, int finalMomentum)
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

        // Bonus for high momentum achievement
        if (finalMomentum >= 12) // Enhanced goal threshold
            baseReward += 1;
        else if (finalMomentum < 4) // Very low momentum penalty
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
               (session.CurrentState == ConnectionState.RECEPTIVE && session.CurrentMomentum > 5);
    }

    /// <summary>
    /// Create a letter obligation from successful conversation
    /// </summary>
    private DeliveryObligation CreateLetterObligation(ConversationSession session)
    {
        int stateValue = (int)session.CurrentState; // Use state as base value
        int momentumBonus = Math.Max(0, session.CurrentMomentum / 5); // Convert momentum to bonus
        int cadenceBonus = Math.Max(0, -session.Cadence / 2); // Negative cadence (good listening) provides bonus

        // Calculate deadline and payment based on relationship quality (segment-based)
        int baseSegments = 12; // ~12 segments base (3/4 of day)
        int deadlineInSegments = Math.Max(2, baseSegments - (stateValue * 2) - (cadenceBonus * 1));
        int payment = 5 + stateValue + cadenceBonus;

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
        if (recipient == null)
        {
            Console.WriteLine($"ERROR: No suitable recipient found for letter from {session.NPC.Name} - only {_gameWorld.NPCs.Count} NPCs available");
            throw new InvalidOperationException($"Cannot create letter - no other NPCs available as recipients (sender: {session.NPC.Name})");
        }

        return new DeliveryObligation
        {
            Id = Guid.NewGuid().ToString(),
            SenderId = session.NPC.ID,
            SenderName = session.NPC.Name,
            RecipientId = recipient.ID,
            RecipientName = recipient.Name,
            TokenType = ConnectionType.Trust,
            Stakes = StakeType.SAFETY,
            DeadlineInSegments = deadlineInSegments,
            Payment = payment,
            Tier = tier,
            EmotionalFocus = focus,
            Description = $"Letter from {session.NPC.Name} (State: {session.CurrentState}, Momentum: {session.CurrentMomentum})"
        };
    }

    /// <summary>
    /// Create an urgent letter from an NPC in distress
    /// </summary>
    private DeliveryObligation CreateUrgentLetter(NPC npc)
    {
        // Find a suitable recipient (family member, friend, etc.)
        List<NPC> allNpcs = _gameWorld.GetAllNPCs();
        NPC recipient = allNpcs.FirstOrDefault(n => n.ID != npc.ID);
        if (recipient == null)
        {
            Console.WriteLine($"ERROR: No suitable recipient found for urgent letter from {npc.Name} - only {allNpcs.Count} NPCs available");
            throw new InvalidOperationException($"Cannot create urgent letter - no other NPCs available as recipients (sender: {npc.Name})");
        }

        return new DeliveryObligation
        {
            Id = Guid.NewGuid().ToString(),
            Title = $"Urgent Letter from {npc.Name}",
            SenderId = npc.ID,
            SenderName = npc.Name,
            RecipientId = recipient.ID,
            RecipientName = recipient.Name,
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

        // Initiative does not refresh on LISTEN 
        // Calculate draw count based on state and atmosphere
        int baseDrawCount = session.GetDrawCount();

        // Apply Impulse doubt penalty - each Impulse card adds +1 doubt on LISTEN
        int impulseCount = session.Deck.HandCards.Count(c => c.Persistence == PersistenceType.Statement);
        if (impulseCount > 0 && session.MomentumManager != null)
        {
            session.MomentumManager.AddDoubt(impulseCount);
        }
        session.Deck.DrawToHand(baseDrawCount);

        // Get the drawn cards for return value
        List<CardInstance> drawnCards = session.Deck.HandCards.TakeLast(baseDrawCount).ToList();

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
            !(selectedCard.ConversationCardTemplate.CardType == CardType.Letter || selectedCard.ConversationCardTemplate.CardType == CardType.Promise || selectedCard.ConversationCardTemplate.CardType == CardType.Letter))
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
                MomentumGenerated = 0
            };
        }

        // Initiative cost is determined by the card's Initiative requirement
        int initiativeCost = GetCardInitiativeCost(selectedCard);

        // Validate Initiative availability
        if (!session.CanAffordCard(initiativeCost))
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
                MomentumGenerated = 0
            };
        }

        // DETERMINISTIC: Check success based on clear rules (no randomness)
        bool success = _effectResolver.CheckCardSuccess(selectedCard, session);

        // Mark request as completed if this is a BurdenGoal (request) card and it succeeds
        if (success && selectedCard.ConversationCardTemplate.CardType == CardType.Letter && selectedCard.Context?.RequestId != null)
        {
            // Find and complete the request
            NPCRequest request = session.NPC.GetRequestById(selectedCard.Context.RequestId);
            if (request != null)
            {
                request.Complete();
                // The conversation will end after this card is played
            }
        }

        // Spend Initiative - Initiative represents built-up conversational energy
        session.SpendInitiative(initiativeCost);

        // Update card playability immediately after spending Initiative
        UpdateCardPlayabilityBasedOnInitiative(session);

        CardEffectResult effectResult = null;
        int cadenceChange = 0;

        if (success)
        {
            // Cadence only changes from explicit card effects (no automatic changes)
            cadenceChange = 0;

            // Reset bad luck protection on success would go here if implemented

            // Process card's success effect
            // PROJECTION PRINCIPLE: Get projection from resolver and apply it
            effectResult = _effectResolver.ProcessSuccessEffect(selectedCard, session);

            // Apply momentum/doubt changes based on card effects
            if (effectResult.MomentumChange > 0 && session.MomentumManager != null)
            {
                session.MomentumManager.AddMomentum(effectResult.MomentumChange);
            }
            if (effectResult.DoubtChange > 0 && session.MomentumManager != null)
            {
                session.MomentumManager.AddDoubt(effectResult.DoubtChange);
            }
            if (effectResult.DoubtChange < 0 && session.MomentumManager != null)
            {
                session.MomentumManager.ReduceDoubt(-effectResult.DoubtChange);
            }

            // Apply initiative restoration (for Initiative-granting success effect)
            if (effectResult.InitiativeChange > 0)
            {
                session.AddInitiative(effectResult.InitiativeChange);
            }

            // Apply initiative change (for Initiative-granting success effects)
            if (effectResult.InitiativeChange != 0)
            {
                session.AddInitiative(effectResult.InitiativeChange);
            }

            // Add drawn cards to active cards (for Threading success effect)
            if (effectResult.CardsToAdd.Any())
            {
                session.Deck.AddCardsToMind(effectResult.CardsToAdd);
            }

            // Handle doubt reduction effect (for Soothe cards with doubt reduction scaling)
            if (selectedCard.ConversationCardTemplate.MomentumScaling == ScalingType.SpendMomentumForDoubt)
            {
                session.PreventNextDoubtIncrease = true;
            }

            // Handle Promise card queue manipulation (for Promising success effect)
            if (selectedCard.ConversationCardTemplate.SuccessType == SuccessEffectType.Promising)
            {
                HandlePromiseCardQueueManipulation(selectedCard, session);
            }

            // Atmosphere effects simplified - no longer tracked
        }

        // HIGHLANDER: Use deck's PlayCard method which handles all transitions
        session.Deck.PlayCard(selectedCard);

        // Remove observation card from NPC's observation deck if it was played
        // ARCHITECTURE: Observations are stored per-NPC, not globally on player
        // This ensures observations are contextually relevant to specific NPCs
        if (selectedCard.ConversationCardTemplate.CardType == CardType.Observation && session.NPC != null)
        {
            // Observation cards are consumed when played - remove from NPC's observation deck
            string observationCardId = selectedCard.ConversationCardTemplate.Id;
            ConversationCard cardToRemove = session.NPC.ObservationDeck?.GetAllCards()
                .FirstOrDefault(c => c.Id == observationCardId);

            if (cardToRemove != null)
            {
                session.NPC.ObservationDeck.RemoveCard(cardToRemove);
                Console.WriteLine($"[ConversationFacade] Consumed observation card {observationCardId} from {session.NPC.Name}'s deck");
            }
        }


        CardPlayResult result = new CardPlayResult
        {
            Results = new List<SingleCardResult>
            {
                new SingleCardResult
                {
                    Card = selectedCard,
                    Success = success,
                    Flow = cadenceChange,
                    Roll = 0, // No dice rolls in deterministic system
                    SuccessChance = success ? 100 : 0 // Deterministic: either succeeds or fails
                }
            },
            MomentumGenerated = cadenceChange
        };


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
            if ((card.ConversationCardTemplate.CardType == CardType.Letter || card.ConversationCardTemplate.CardType == CardType.Promise || card.ConversationCardTemplate.CardType == CardType.Letter)
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
    /// Mark request card presence in conversation session
    /// </summary>
    private void UpdateRequestCardPlayability(ConversationSession session)
    {
        // This is called at conversation start - just check for goal card presence
        bool hasRequestCard = session.Deck.HandCards
            .Any(c => c.ConversationCardTemplate.CardType == CardType.Letter || c.ConversationCardTemplate.CardType == CardType.Promise || c.ConversationCardTemplate.CardType == CardType.Letter);

        if (hasRequestCard)
        {
            session.RequestCardDrawn = true;
        }
    }

    /// <summary>
    /// Update all cards' playability based on current Initiative availability
    /// Cards that cost more Initiative than available are marked Unplayable
    /// </summary>
    private void UpdateCardPlayabilityBasedOnInitiative(ConversationSession session)
    {
        int availableInitiative = session.GetCurrentInitiative();

        foreach (CardInstance card in session.Deck.HandCards)
        {
            // Skip request/promise cards - their playability is based on momentum, not Initiative
            if (card.ConversationCardTemplate.CardType == CardType.Letter || card.ConversationCardTemplate.CardType == CardType.Promise || card.ConversationCardTemplate.CardType == CardType.Letter)
            {
                continue; // Don't modify request card playability here
            }

            // Calculate effective Initiative cost for this card
            int effectiveInitiativeCost = GetCardInitiativeCost(card);

            // Check if we can afford this card
            bool canAfford = session.CanAffordCard(effectiveInitiativeCost);

            // Update playability based on Initiative availability
            card.IsPlayable = canAfford;
        }
    }








    /// <summary>
    /// Handle special card effects like exchanges and letter delivery
    /// </summary>
    private void HandleSpecialCardEffects(HashSet<CardInstance> playedCards, ConversationTurnResult result)
    {
        foreach (CardInstance card in playedCards)
        {
            Console.WriteLine($"[ConversationFacade] Processing card {card.ConversationCardTemplate.Title}, has Context: {card.Context != null}, has ExchangeData: {card.Context?.ExchangeData != null}");

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
                $"{card.ConversationCardTemplate.Title} is now available (Momentum threshold met)",
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

        return _currentSession.Deck.SpokenCards;
    }

    /// <summary>
    /// Check if a card is in the hand
    /// </summary>
    public bool IsCardInHand(CardInstance card)
    {
        if (_currentSession?.Deck == null || card == null)
            return false;

        return _currentSession.Deck.IsCardInMind(card);
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
    public (int deck, int spoken, int mind, int requestPile) GetDeckStatistics()
    {
        if (_currentSession?.Deck == null)
            return (0, 0, 0, 0);

        return (
            _currentSession.Deck.RemainingDeckCards,
            _currentSession.Deck.SpokenPileCount,
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

    #region Preview Methods for UI

    /// <summary>
    /// Get preview text for LISTEN action showing effects of doubt increase, momentum erosion, and card draw
    /// </summary>
    public string GetListenActionPreview()
    {
        if (_currentSession == null) return "";

        List<string> preview = new List<string>();

        // Show Cadence effects (CORRECTED)
        int currentCadence = _currentSession.Cadence;
        int newCadence = Math.Max(-5, currentCadence - 2); // LISTEN gives -2 Cadence (min -5)
        preview.Add($"Cadence: {currentCadence} → {newCadence} (-2 for listening)");

        // Show positive cadence doubt penalty if applicable
        if (currentCadence > 0)
        {
            int doubtPenalty = currentCadence; // +1 Doubt per positive Cadence point
            int newDoubt = Math.Min(_currentSession.MaxDoubt, _currentSession.CurrentDoubt + doubtPenalty);
            preview.Add($"Doubt: {_currentSession.CurrentDoubt} → {newDoubt} (+{doubtPenalty} from positive Cadence)");
        }

        // Show card draw (3 base + negative cadence bonus)
        int drawCount = _currentSession.GetDrawCount();
        int cadenceBonus = newCadence < 0 ? Math.Abs(newCadence) : 0;
        if (cadenceBonus > 0)
        {
            preview.Add($"Draw {drawCount} cards (3 base + {cadenceBonus} from negative Cadence)");
        }
        else
        {
            preview.Add($"Draw {drawCount} cards (3 base)");
        }

        // Show Initiative status (accumulates between LISTEN - no reset)
        preview.Add($"Initiative: {_currentSession.CurrentInitiative} (accumulates - never resets)");

        return string.Join("<br/>", preview);
    }

    /// <summary>
    /// Get preview text for SPEAK action based on selected card
    /// </summary>
    public string GetSpeakActionPreview(CardInstance selectedCard)
    {
        if (_currentSession == null) return "";

        if (selectedCard == null)
        {
            return $"Select a card to play ({_currentSession.CurrentInitiative} Initiative available)";
        }

        int initiativeCost = GetCardInitiativeCost(selectedCard);
        int newInitiative = Math.Max(0, _currentSession.CurrentInitiative - initiativeCost);
        int newCadence = Math.Min(5, _currentSession.Cadence + 1); // SPEAK gives +1 Cadence (max +5)

        List<string> preview = new List<string>();
        preview.Add($"Initiative: {_currentSession.CurrentInitiative} → {newInitiative} (-{initiativeCost})");
        preview.Add($"Cadence: {_currentSession.Cadence} → {newCadence} (+1 for speaking)");

        return string.Join("<br/>", preview);
    }

    /// <summary>
    /// Get action text for SPEAK button details based on selected card
    /// </summary>
    public string GetSpeakActionText(CardInstance selectedCard)
    {
        if (_currentSession == null) return "";

        if (selectedCard != null)
        {
            int initiativeCost = GetCardInitiativeCost(selectedCard);
            int remainingAfter = _currentSession.CurrentInitiative - initiativeCost;
            string continueHint = remainingAfter > 0 ? $" (Can continue with {remainingAfter} Initiative)" : " (Must use Foundation cards to build Initiative)";
            return $"Play Card ({initiativeCost} Initiative){continueHint}";
        }

        int availableInitiative = _currentSession.CurrentInitiative;
        if (availableInitiative == 0)
            return "No Initiative - use Foundation cards to build Initiative";
        else if (availableInitiative == 1)
            return "Select a card to play (1 Initiative available)";
        else
            return $"Select a card to play ({availableInitiative} Initiative available)";
    }

    /// <summary>
    /// Get stat bonus text for cards based on player stats
    /// </summary>
    public string GetCardStatBonus(CardInstance card)
    {
        if (card?.ConversationCardTemplate?.BoundStat == null || _gameWorld == null) return "";

        // Only Expression cards get momentum bonuses
        if (card.ConversationCardTemplate.Category != CardCategory.Expression) return "";

        try
        {
            Player player = _gameWorld.GetPlayer();
            if (player?.Stats == null) return "";

            PlayerStats stats = player.Stats;
            int statLevel = stats.GetLevel(card.ConversationCardTemplate.BoundStat.Value);

            // Level 2 = +1, Level 3 = +2, Level 4 = +3, Level 5 = +4
            if (statLevel >= 2)
            {
                int bonus = statLevel - 1;
                return $"+{bonus} momentum";
            }
        }
        catch
        {
            // Fallback to no bonus display
        }

        return "";
    }

    #endregion

    #endregion
}