/// <summary>
/// Public API for the Conversation subsystem.
/// Handles all conversation operations with functionality absorbed from ConversationOrchestrator and CardDeckManager.
/// </summary>
public class SocialFacade
{
    private readonly GameWorld _gameWorld;
    private readonly ExchangeHandler _exchangeHandler;
    private readonly MomentumManager _momentumManager;
    private readonly SocialEffectResolver _effectResolver;
    private readonly SocialNarrativeService _narrativeService;
    private readonly SocialChallengeDeckBuilder _deckBuilder;

    // External dependencies
    private readonly ObservationManager _observationManager;
    private readonly TimeManager _timeManager;
    private readonly TokenMechanicsManager _tokenManager;
    private readonly MessageSystem _messageSystem;
    private readonly KnowledgeService _knowledgeService;
    private readonly InvestigationActivity _investigationActivity;

    private SocialSession _currentSession;
    private SocialChallengeOutcome _lastOutcome;
    private PersonalityRuleEnforcer _personalityEnforcer;

    public SocialFacade(
        GameWorld gameWorld,
        ExchangeHandler exchangeHandler,
        MomentumManager momentumManager,
        SocialEffectResolver effectResolver,
        SocialNarrativeService narrativeService,
        SocialChallengeDeckBuilder deckBuilder,
        ObservationManager observationManager,
        TimeManager timeManager,
        TokenMechanicsManager tokenManager,
        MessageSystem messageSystem,
        KnowledgeService knowledgeService,
        InvestigationActivity investigationActivity)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _exchangeHandler = exchangeHandler ?? throw new ArgumentNullException(nameof(exchangeHandler));
        _momentumManager = momentumManager ?? throw new ArgumentNullException(nameof(momentumManager));
        _effectResolver = effectResolver ?? throw new ArgumentNullException(nameof(effectResolver));
        _narrativeService = narrativeService ?? throw new ArgumentNullException(nameof(narrativeService));
        _deckBuilder = deckBuilder ?? throw new ArgumentNullException(nameof(deckBuilder));
        _observationManager = observationManager ?? throw new ArgumentNullException(nameof(observationManager));
        _timeManager = timeManager ?? throw new ArgumentNullException(nameof(timeManager));
        _tokenManager = tokenManager ?? throw new ArgumentNullException(nameof(tokenManager));
        _messageSystem = messageSystem ?? throw new ArgumentNullException(nameof(messageSystem));
        _knowledgeService = knowledgeService ?? throw new ArgumentNullException(nameof(knowledgeService));
        _investigationActivity = investigationActivity ?? throw new ArgumentNullException(nameof(investigationActivity));
    }

    /// <summary>
    /// Start a new conversation with an NPC using a specific request
    /// </summary>
    public SocialSession StartConversation(string npcId, string requestId)
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

        // Get the request that drives this conversation - from centralized GameWorld storage
        if (!_gameWorld.Goals.TryGetValue(requestId, out Goal goal))
        {
            throw new ArgumentException($"Goal {requestId} not found in GameWorld.Goals");
        }

        // Get connection state from NPC for session initialization
        ConnectionState initialState = npc.GetConnectionState();

        // Focus management is now handled directly by ConversationSession

        // Initialize personality rule enforcer based on NPC's personality
        _personalityEnforcer = new PersonalityRuleEnforcer(npc.ConversationModifier ?? new PersonalityModifier { Type = PersonalityModifierType.None });

        // Get NPC token counts for session initialization
        Dictionary<ConnectionType, int> npcTokens = _tokenManager.GetTokensWithNPC(npc.ID);

        // Create session deck and get request cards from the request
        (SocialSessionCardDeck deck, List<CardInstance> GoalCards) = _deckBuilder.CreateConversationDeck(npc, requestId);

        // Initialize momentum manager for this conversation with token data
        _momentumManager.InitializeForConversation(npcTokens);

        // Calculate starting resources based on player's highest stat
        Player player = _gameWorld.GetPlayer();
        int highestStat = player.Stats.GetHighestLevel();
        int statBonus = (int)Math.Floor(highestStat / 3.0);

        int startingUnderstanding = 2 + statBonus;
        int startingMomentum = 2 + statBonus;
        int startingInitiative = 3 + statBonus;
        int initialDoubt = 0;

        Console.WriteLine($"[ConversationFacade] Starting resources - Understanding: {startingUnderstanding}, Momentum: {startingMomentum}, Initiative: {startingInitiative} (highest stat: {highestStat}, bonus: {statBonus})");

        // Get request text from the goal description
        string requestText = goal.Description;

        // Create session with new properties
        _currentSession = new SocialSession
        {
            NPC = npc,
            RequestId = requestId,
            DeckId = goal.DeckId,
            CurrentState = initialState,
            InitialState = initialState,
            CurrentInitiative = startingInitiative,
            CurrentUnderstanding = startingUnderstanding,
            CurrentMomentum = startingMomentum,
            CurrentDoubt = initialDoubt,
            Cadence = 0, // Starts at 0
            TurnNumber = 0,
            Deck = deck, // HIGHLANDER: Deck manages ALL card piles
            TokenManager = _tokenManager,
            MomentumManager = _momentumManager,
            PersonalityEnforcer = _personalityEnforcer,  // Add personality enforcer to session
            RequestText = requestText // Set request text for Request conversations
        };

        // Set up state synchronization between MomentumManager and ConversationSession
        _momentumManager.SetSession(_currentSession);

        // Check and unlock depth tiers based on starting Understanding (NOT Momentum)
        // Starting Understanding determines which tiers are accessible from the beginning
        _currentSession.CheckAndUnlockTiers();
        Console.WriteLine($"[ConversationFacade] Starting with tiers: {string.Join(", ", _currentSession.UnlockedTiers.OrderBy(t => t))}. Max depth: {_currentSession.GetUnlockedMaxDepth()}");

        // THEN: Perform initial draw of regular cards with tier-based filtering
        // This is the initial conversation start, so we just draw cards without exhausting
        int drawCount = _currentSession.GetDrawCount();
        // Draw with tier-based filtering
        _currentSession.Deck.DrawToHand(drawCount, _currentSession, player.Stats);

        // Update request card playability based on initiative
        UpdateGoalCardPlayability(_currentSession);

        // Update card playability based on starting initiative (already set from formula)
        UpdateCardPlayabilityBasedOnInitiative(_currentSession);

        return _currentSession;
    }

    /// <summary>
    /// End the current conversation
    /// </summary>
    public SocialChallengeOutcome EndConversation()
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

        // Apply token changes from other sources
        if (_lastOutcome.TokensEarned != 0)
        {
            ConnectionType connectionType = DetermineConnectionTypeFromConversation(_currentSession);
            _tokenManager.AddTokensToNPC(connectionType, _lastOutcome.TokensEarned, _currentSession.NPC.ID);
        }

        _currentSession.Deck.ResetForNewConversation();
        _currentSession = null;

        return _lastOutcome;
    }

    /// <summary>
    /// Execute LISTEN action - Complete 4-Resource System Implementation
    /// Sequence: Apply Cadence Effects → Handle Card Persistence → Fixed Card Draw → Refresh Initiative → Check Goal Cards
    /// </summary>
    public async Task<SocialTurnResult> ExecuteListen()
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

        // DELETED: Doubt Tax system - not in specification

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

        // CRITICAL: Reduce cadence AFTER draw calculation (spec line 894)
        ReduceCadenceAfterDraw(_currentSession);

        // Force discard down to 7-card hand limit if necessary
        _currentSession.Deck.DiscardDown(7);

        // Update card playability based on Initiative system
        UpdateCardPlayabilityForInitiative(_currentSession);

        // Generate narrative using the narrative service
        NarrativeOutput narrative = await _narrativeService.GenerateNarrativeAsync(
            _currentSession,
            _currentSession.NPC,
            drawnCards);
        string npcResponse = narrative.NPCDialogue;

        return new SocialTurnResult
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
    public async Task<SocialTurnResult> ExecuteSpeakSingleCard(CardInstance selectedCard)
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
            return new SocialTurnResult
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
                return new SocialTurnResult
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
            return new SocialTurnResult
            {
                Success = false,
                NewState = _currentSession.CurrentState,
                NPCResponse = "Failed to spend Initiative for card play.",
                DoubtLevel = _currentSession.CurrentDoubt
            };
        }

        // 4. Apply Cadence Change based on Delivery property (replaces old +1 flat system)
        ApplyCadenceFromDelivery(selectedCard, _currentSession);

        // 5. Calculate Success
        bool success = CalculateInitiativeCardSuccess(selectedCard, _currentSession);

        // 6. Process Card Results
        CardPlayResult playResult = ProcessInitiativeCardPlay(selectedCard, success, _currentSession);

        // 7. Grant XP to player stat (unchanged)
        Player player = _gameWorld.GetPlayer();
        if (selectedCard.SocialCardTemplate.BoundStat.HasValue)
        {
            int xpAmount = CalculateXPAmount(_currentSession);
            player.Stats.AddXP(selectedCard.SocialCardTemplate.BoundStat.Value, xpAmount);
        }

        // 7b. Grant Knowledge and Secrets (V2 Investigation System)
        foreach (string knowledge in selectedCard.SocialCardTemplate.KnowledgeGranted)
        {
            _knowledgeService.GrantKnowledge(knowledge);
        }
        foreach (string secret in selectedCard.SocialCardTemplate.SecretsGranted)
        {
            player.Knowledge.AddSecret(secret);
        }

        // 8. Record card played for personality tracking
        _personalityEnforcer?.OnCardPlayed(selectedCard);

        // 9. Handle Card Persistence (Standard/Echo/Persistent/Banish)
        ProcessCardAfterPlay(selectedCard, success, _currentSession);

        // 9b. Increment Statement counter if this is a Statement card
        if (selectedCard.SocialCardTemplate.Persistence == PersistenceType.Statement
            && selectedCard.SocialCardTemplate.BoundStat.HasValue)
        {
            _currentSession.IncrementStatementCount(selectedCard.SocialCardTemplate.BoundStat.Value);
        }

        // 10. Update card playability based on new Initiative level
        UpdateCardPlayabilityForInitiative(_currentSession);

        // Generate NPC response through narrative service
        List<CardInstance> activeCards = _currentSession.Deck.HandCards.ToList();
        NarrativeOutput narrative = await _narrativeService.GenerateNarrativeAsync(
            _currentSession,
            _currentSession.NPC,
            activeCards);

        SocialTurnResult result = new SocialTurnResult
        {
            Success = success,
            NewState = _currentSession.CurrentState, // Connection State doesn't change
            NPCResponse = narrative.NPCDialogue,
            FlowChange = 0, // No flow             OldFlow = 0, // No flow             NewFlow = 0, // No flow             DoubtLevel = _currentSession.CurrentDoubt,
            CardPlayResult = playResult,
            Narrative = narrative,
            EndsConversation = playResult.EndsConversation // Request cards end conversation
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
    public SocialSession GetCurrentSession()
    {
        return _currentSession;
    }

    /// <summary>
    /// Create a conversation context for UI - returns typed context
    /// </summary>
    public async Task<SocialChallengeContext> CreateConversationContext(string npcId, string requestId)
    {
        NPC npc = _gameWorld.NPCs.FirstOrDefault(n => n.ID == npcId);
        if (npc == null)
        {
            return SocialContextFactory.CreateInvalidContext("NPC not found");
        }

        // Get request to determine attention cost - from centralized GameWorld storage
        if (!_gameWorld.Goals.TryGetValue(requestId, out Goal goal))
        {
            return SocialContextFactory.CreateInvalidContext($"Goal {requestId} not found in GameWorld.Goals");
        }

        // Start conversation with the request
        SocialSession session = StartConversation(npcId, requestId);

        // Create typed context based on request's conversation type
        SocialChallengeContext context = SocialContextFactory.CreateContext(
            goal.DeckId,
            npc,
            session,
            new List<CardInstance>(), // observationCards - empty for now
            ResourceState.FromPlayerResourceState(_gameWorld.GetPlayerResourceState()),
            _gameWorld.GetPlayer().CurrentLocation.ToString(),
            _timeManager.GetCurrentTimeBlock().ToString());

        // Goal cards now handle domain logic - no context-specific initialization needed

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
    public SocialChallengeOutcome GetLastOutcome()
    {
        return _lastOutcome;
    }

    /// <summary>
    /// Check if a card can be played in the current conversation
    /// </summary>
    public bool CanPlayCard(CardInstance card, SocialSession session)
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
        if (card.CardType == CardTypes.Goal)
        {
            // If card is in RequestPile, check momentum threshold
            if (session.Deck?.IsCardInRequestPile(card) == true)
            {
                int momentumThreshold = card.Context?.threshold ?? 0;
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
            if (exchangeData is not ExchangeCard exchange)
            {
                throw new ArgumentException("exchangeData must be of type ExchangeCard");
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
        SocialChallengeOutcome outcome = EndConversation();
        return Task.FromResult(outcome != null);
    }


    // AtmosphereManager has been deleted - atmosphere is simplified to always Neutral

    #region 5-Resource System Helper Methods (Understanding + Delivery)

    /// <summary>
    /// Apply cadence change using DUAL BALANCE SYSTEM
    /// DUAL BALANCE: Action type (SPEAK = +1) + Card Delivery property
    /// Standard: +1, Commanding: +2, Measured: +0, Yielding: -1
    /// Total: SPEAK (+1) + Delivery = combined Cadence change
    /// </summary>
    private void ApplyCadenceFromDelivery(CardInstance card, SocialSession session)
    {
        // DUAL BALANCE SYSTEM:
        // 1. Action-based balance (SPEAK action)
        int actionBalance = +1; // SPEAK action always +1

        // 2. Delivery-based balance (card property)
        int deliveryBalance = card.SocialCardTemplate.Delivery switch
        {
            DeliveryType.Yielding => -1,
            DeliveryType.Measured => 0,
            DeliveryType.Standard => +1,
            DeliveryType.Commanding => +2,
            _ => +1 // Default to Standard
        };

        // Combine both balance effects
        int totalCadenceChange = actionBalance + deliveryBalance;

        session.Cadence = Math.Clamp(session.Cadence + totalCadenceChange, -10, 10);
        Console.WriteLine($"[ConversationFacade] SPEAK action (+{actionBalance}) + Delivery {card.SocialCardTemplate.Delivery} ({(deliveryBalance >= 0 ? "+" : "")}{deliveryBalance}) = Cadence {(totalCadenceChange >= 0 ? "+" : "")}{totalCadenceChange} → {session.Cadence}");
    }

    /// <summary>
    /// Process Cadence effects on LISTEN action - NEW REFACTORED SYSTEM
    /// 1. Calculate doubt to clear
    /// 2. Reset doubt to 0
    /// 3. Reduce momentum by doubt cleared
    /// 4. Check and unlock tiers (momentum may have changed)
    /// 5. Convert positive cadence to doubt
    /// BEFORE card draw calculation (cadence reduction happens AFTER draw)
    /// </summary>
    private void ProcessCadenceEffectsOnListen(SocialSession session)
    {
        // NEW REFACTORED LISTEN MECHANICS (Per Spec lines 862-896):
        // 1. Calculate doubt that will be cleared
        int doubtCleared = session.CurrentDoubt;

        // 2. Reset doubt to 0 (complete relief)
        session.CurrentDoubt = 0;

        // 3. Reduce MOMENTUM by amount of doubt cleared (minimum 0)
        // CRITICAL: Understanding is NOT reduced - it persists through LISTEN
        session.CurrentMomentum = Math.Max(0, session.CurrentMomentum - doubtCleared);
        Console.WriteLine($"[ConversationFacade] LISTEN cleared {doubtCleared} doubt, reduced Momentum by {doubtCleared}. Momentum: {session.CurrentMomentum}, Understanding: {session.CurrentUnderstanding} (preserved)");

        // 4. Check tier unlocks (uses Understanding, NOT Momentum)
        // Tiers are based on Understanding thresholds (6/12/18), not Momentum
        // Understanding is NOT reduced during LISTEN, so tiers stay unlocked
        session.CheckAndUnlockTiers();

        // 5. Convert positive cadence to doubt (CRITICAL: Check for conversation death)
        if (session.Cadence > 0)
        {
            int cadenceToDoubt = session.Cadence;
            session.CurrentDoubt = Math.Min(session.MaxDoubt, session.CurrentDoubt + cadenceToDoubt);
            Console.WriteLine($"[ConversationFacade] LISTEN converted {cadenceToDoubt} positive cadence to doubt. New doubt: {session.CurrentDoubt}");

            // CRITICAL: If doubt reaches max (10), conversation ends immediately
            // This is the "cadence trap" - listening while dominating can end the conversation
            if (session.CurrentDoubt >= session.MaxDoubt)
            {
                Console.WriteLine($"[ConversationFacade] CONVERSATION ENDED: Cadence refill brought doubt to {session.CurrentDoubt} (max {session.MaxDoubt})");
                // Conversation will end - ExecuteListen will detect this in ShouldEnd() check
            }
        }

        // 6. Cadence reduction by -1 happens AFTER draw calculation (NOT here)
        // This is handled in ReduceCadenceAfterDraw() method called after ExecuteNewListenCardDraw
    }

    /// <summary>
    /// Apply LISTEN action-type balance AFTER card draw (step 7 of LISTEN sequence)
    /// DUAL BALANCE: LISTEN action = -2 cadence (action-type balance, no card played so no Delivery)
    /// </summary>
    private void ReduceCadenceAfterDraw(SocialSession session)
    {
        // DUAL BALANCE SYSTEM: LISTEN action contributes -2 to Cadence
        session.Cadence = Math.Max(-10, session.Cadence - 2);
        Console.WriteLine($"[ConversationFacade] LISTEN action: Cadence -2 → {session.Cadence}");
    }

    /// <summary>
    /// Handle card persistence after playing
    /// Standard: Goes to Spoken pile
    /// Echo: Returns to hand if conditions met
    /// Persistent: Stays in hand
    /// Banish: Removed entirely
    /// </summary>
    private void ProcessCardPersistence(SocialSession session)
    {
        // Handle cards that need persistence processing
        // This is handled by the deck system based on card persistence types
        session.Deck.ProcessCardPersistence();
    }

    /// <summary>
    /// Check if goal cards should become active based on momentum thresholds
    /// Basic: 8, Enhanced: 12, Premium: 16
    /// </summary>
    private void CheckGoalCardActivation(SocialSession session)
    {
        int currentMomentum = session.CurrentMomentum;

        // Move request cards that meet momentum threshold from request pile to hand
        List<CardInstance> activatedCards = session.Deck.CheckRequestThresholds(currentMomentum);

        foreach (CardInstance card in activatedCards)
        {
            _messageSystem.AddSystemMessage(
                $"{card.SocialCardTemplate.Title} is now available (Momentum threshold met)",
                SystemMessageTypes.Success);
        }
    }

    /// <summary>
    /// Execute card draw with tier-based filtering
    /// </summary>
    private List<CardInstance> ExecuteNewListenCardDraw(SocialSession session, int cardsToDraw)
    {
        // Draw with tier and stat filtering
        Player player = _gameWorld.GetPlayer();
        session.Deck.DrawToHand(cardsToDraw, session, player.Stats);

        // Return the newly drawn cards (last N cards in hand)
        return session.Deck.HandCards.TakeLast(cardsToDraw).ToList();
    }

    /// <summary>
    /// Update card playability based on Initiative system and Statement requirements
    /// </summary>
    private void UpdateCardPlayabilityForInitiative(SocialSession session)
    {
        int currentInitiative = session.CurrentInitiative;

        foreach (CardInstance card in session.Deck.HandCards)
        {
            // Skip request cards - their playability is based on momentum thresholds
            if (card.CardType == CardTypes.Goal)
            {
                continue;
            }

            // Check if player can afford this card's Initiative cost
            int initiativeCost = GetCardInitiativeCost(card);
            bool canAffordInitiative = currentInitiative >= initiativeCost;

            // Check if Statement requirements are met
            bool meetsStatementRequirements = card.SocialCardTemplate.MeetsStatementRequirements(session);

            // Card is playable if BOTH conditions are met
            card.IsPlayable = canAffordInitiative && meetsStatementRequirements;
        }
    }

    /// <summary>
    /// Get Initiative cost for a card (replaces Focus cost)
    /// </summary>
    private int GetCardInitiativeCost(CardInstance card)
    {
        return card.SocialCardTemplate?.InitiativeCost ?? 0;
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
    private bool CalculateInitiativeCardSuccess(CardInstance selectedCard, SocialSession session)
    {
        // Use existing deterministic success calculation for now
        // This handles momentum and stat bonuses correctly
        return _effectResolver.CheckCardSuccess(selectedCard, session);
    }

    /// <summary>
    /// Process card play results with Initiative system
    /// PROJECTION PRINCIPLE: Get projection from resolver, then apply to session
    /// </summary>
    private CardPlayResult ProcessInitiativeCardPlay(CardInstance selectedCard, bool success, SocialSession session)
    {
        if (success)
        {
            // Get projection from resolver (single source of truth)
            CardEffectResult projection = _effectResolver.ProcessSuccessEffect(selectedCard, session);

            // Apply projection to session state
            ApplyProjectionToSession(projection, session);
        }

        // Check if card ends conversation (Request, Promise, Burden cards)
        bool endsConversation = selectedCard.CardType == CardTypes.Goal;

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
            MomentumGenerated = 0, // No flow
            EndsConversation = endsConversation // Request cards end conversation
        };
    }

    /// <summary>
    /// Apply a projection result to actual session state
    /// PROJECTION PRINCIPLE: This is the ONLY place where projections become reality
    /// </summary>
    private void ApplyProjectionToSession(CardEffectResult projection, SocialSession session)
    {
        // Apply Initiative changes
        if (projection.InitiativeChange != 0)
        {
            session.AddInitiative(projection.InitiativeChange);
            Console.WriteLine($"[Effect] Initiative {(projection.InitiativeChange > 0 ? "+" : "")}{projection.InitiativeChange} → {session.CurrentInitiative}");
        }

        // Apply Momentum changes (NO TIER UNLOCKS - that's Understanding's job)
        if (projection.MomentumChange != 0)
        {
            session.CurrentMomentum = Math.Max(0, session.CurrentMomentum + projection.MomentumChange);
            Console.WriteLine($"[Effect] Momentum {(projection.MomentumChange > 0 ? "+" : "")}{projection.MomentumChange} → {session.CurrentMomentum}");
        }

        // Apply Understanding changes (TRIGGERS TIER UNLOCKS)
        if (projection.UnderstandingChange != 0)
        {
            session.AddUnderstanding(projection.UnderstandingChange);
            Console.WriteLine($"[Effect] Understanding {(projection.UnderstandingChange > 0 ? "+" : "")}{projection.UnderstandingChange} → {session.CurrentUnderstanding}");
            // Tier unlocks happen inside AddUnderstanding via CheckAndUnlockTiers()
        }

        // Apply Doubt changes
        if (projection.DoubtChange > 0)
        {
            session.AddDoubt(projection.DoubtChange);
            Console.WriteLine($"[Effect] Doubt +{projection.DoubtChange} → {session.CurrentDoubt}");
        }
        else if (projection.DoubtChange < 0)
        {
            session.ReduceDoubt(-projection.DoubtChange);
            Console.WriteLine($"[Effect] Doubt {projection.DoubtChange} → {session.CurrentDoubt}");
        }

        // Apply Cadence changes
        if (projection.CadenceChange != 0)
        {
            session.Cadence = Math.Clamp(session.Cadence + projection.CadenceChange, -10, 10);
            Console.WriteLine($"[Effect] Cadence {(projection.CadenceChange > 0 ? "+" : "")}{projection.CadenceChange} (now {session.Cadence})");
        }

        // Apply card draw
        if (projection.CardsToDraw > 0)
        {
            Player player = _gameWorld.GetPlayer();
            session.Deck.DrawToHand(projection.CardsToDraw, session, player.Stats);
            Console.WriteLine($"[Effect] Draw {projection.CardsToDraw} cards → Hand: {session.Deck.HandSize}");
        }

        // Add any specific card instances (legacy support)
        if (projection.CardsToAdd != null && projection.CardsToAdd.Any())
        {
            session.Deck.AddCardsToMind(projection.CardsToAdd);
            Console.WriteLine($"[Effect] Added {projection.CardsToAdd.Count} specific cards to hand");
        }
    }

    /// <summary>
    /// Process card after playing based on persistence type
    /// </summary>
    private void ProcessCardAfterPlay(CardInstance selectedCard, bool success, SocialSession session)
    {
        // Handle card based on its persistence type
        session.Deck.PlayCard(selectedCard);
    }

    /// <summary>
    /// Calculate XP amount based on conversation difficulty
    /// </summary>
    private int CalculateXPAmount(SocialSession session)
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
    private void AddTurnToHistory(ActionType actionType, CardInstance cardPlayed, SocialTurnResult result)
    {
        if (_currentSession != null)
        {
            SocialTurn turn = new SocialTurn
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
    private bool ShouldEndConversation(SocialSession session)
    {
        // End if request card was played (request cards end conversation immediately)
        if (session.TurnHistory != null && session.TurnHistory.Any())
        {
            SocialTurn lastTurn = session.TurnHistory.Last();
            if (lastTurn?.CardPlayed?.CardType == CardTypes.Goal)
            {
                return true;
            }
        }

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
    private SocialChallengeOutcome FinalizeConversation(SocialSession session)
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
        bool requestAchieved = session.Deck.SpokenCards.Any(c => c.CardType == CardTypes.Goal);
        if (requestAchieved)
        {
            tokensEarned += 2; // Bonus for completing request
        }

        return new SocialChallengeOutcome
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
    /// Determine the connection type based on the conversation type and outcome
    /// </summary>
    private ConnectionType DetermineConnectionTypeFromConversation(SocialSession session)
    {
        // Map conversation types to their corresponding connection types
        return session.DeckId switch
        {
            // Diplomacy removed - exchanges use separate Exchange system
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
    private List<CardInstance> ExecuteListenAction(SocialSession session)
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
        Player player = _gameWorld.GetPlayer();
        session.Deck.DrawToHand(baseDrawCount, session, player.Stats);

        // Get the drawn cards for return value
        List<CardInstance> drawnCards = session.Deck.HandCards.TakeLast(baseDrawCount).ToList();

        // Check if any goal cards should become playable based on rapport
        UpdateGoalCardPlayabilityAfterListen(session);

        return drawnCards;
    }


    /// <summary>
    /// Check if goal cards should become playable after LISTEN based on momentum threshold
    /// </summary>
    private void UpdateGoalCardPlayabilityAfterListen(SocialSession session)
    {
        // Get current momentum
        int currentMomentum = session.MomentumManager?.CurrentMomentum ?? 0;

        // Check all goal cards in active hand
        foreach (CardInstance card in session.Deck.HandCards)
        {
            // Only process goal cards that are currently Unplayable
            if ((card.CardType == CardTypes.Goal)
                && !card.IsPlayable)
            {
                // Check if momentum threshold is met
                int momentumThreshold = card.Context?.threshold ?? 0;

                if (currentMomentum >= momentumThreshold)
                {
                    // Make card playable
                    card.IsPlayable = true;
                    // Request cards already have Impulse + Opening persistence set

                    // Mark that a request card is now playable
                    session.GoalCardDrawn = true;
                }
            }
        }
    }

    /// <summary>
    /// Mark request card presence in conversation session
    /// </summary>
    private void UpdateGoalCardPlayability(SocialSession session)
    {
        // This is called at conversation start - just check for goal card presence
        bool hasGoalCard = session.Deck.HandCards
            .Any(c => c.CardType == CardTypes.Goal);

        if (hasGoalCard)
        {
            session.GoalCardDrawn = true;
        }
    }

    /// <summary>
    /// Update all cards' playability based on current Initiative availability and Statement requirements
    /// Cards that cost more Initiative than available or don't meet Statement requirements are marked Unplayable
    /// </summary>
    private void UpdateCardPlayabilityBasedOnInitiative(SocialSession session)
    {
        int availableInitiative = session.GetCurrentInitiative();

        foreach (CardInstance card in session.Deck.HandCards)
        {
            // Skip request/promise cards - their playability is based on momentum, not Initiative
            if (card.CardType == CardTypes.Goal)
            {
                continue; // Don't modify request card playability here
            }

            // Calculate effective Initiative cost for this card
            int effectiveInitiativeCost = GetCardInitiativeCost(card);

            // Check if we can afford this card
            bool canAfford = session.CanAffordCard(effectiveInitiativeCost);

            // Check if Statement requirements are met
            bool meetsStatementRequirements = card.SocialCardTemplate.MeetsStatementRequirements(session);

            // Update playability based on Initiative availability AND Statement requirements
            card.IsPlayable = canAfford && meetsStatementRequirements;
        }
    }

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

    #region Investigation Integration

    /// <summary>
    /// Check if completed NPC request is part of an active investigation and trigger progress
    /// </summary>
    private void CheckInvestigationProgress(string npcId, string requestId)
    {
        // First check if this is an intro action for a discovered investigation
        foreach (string discoveredId in _gameWorld.InvestigationJournal.DiscoveredInvestigationIds.ToList())
        {
            Investigation investigation = _gameWorld.Investigations.FirstOrDefault(i => i.Id == discoveredId);
            if (investigation?.IntroAction != null &&
                investigation.IntroAction.SystemType == TacticalSystemType.Social &&
                investigation.IntroAction.NpcId == npcId &&
                requestId == "notice_waterwheel")
            {
                // This is intro completion - activate investigation
                // CompleteIntroAction spawns goals directly to ActiveGoals
                _investigationActivity.CompleteIntroAction(discoveredId);

                Console.WriteLine($"[ConversationFacade] Investigation '{investigation.Name}' ACTIVATED");
                return;
            }
        }

        // Search active investigations for a phase matching this npcId + requestId
        foreach (ActiveInvestigation activeInv in _gameWorld.InvestigationJournal.ActiveInvestigations)
        {
            Investigation investigation = _gameWorld.Investigations.FirstOrDefault(i => i.Id == activeInv.InvestigationId);
            if (investigation == null) continue;

            // Find phase that matches this npcId + requestId
            // Phase definitions reference goals - look up goal properties
            InvestigationPhaseDefinition matchingPhase = investigation.PhaseDefinitions.FirstOrDefault(p =>
            {
                // Look up referenced goal from GameWorld.Goals
                if (!_gameWorld.Goals.ContainsKey(p.GoalId))
                    return false;

                Goal goal = _gameWorld.Goals[p.GoalId];
                return goal.SystemType == TacticalSystemType.Social &&
                       goal.NpcId == npcId &&
                       p.Id == requestId;
            });

            if (matchingPhase != null)
            {
                // This request is part of an investigation - mark goal as complete
                InvestigationProgressResult progressResult = _investigationActivity.CompleteGoal(matchingPhase.Id, investigation.Id);

                // Log progress for UI modal display (UI will handle modal)
                Console.WriteLine($"[ConversationFacade] Investigation '{investigation.Name}' progress: {progressResult.CompletedGoalCount}/{progressResult.TotalGoalCount} goals complete");

                if (progressResult.NewLeads != null && progressResult.NewLeads.Count > 0)
                {
                    foreach (NewLeadInfo lead in progressResult.NewLeads)
                    {
                        _messageSystem.AddSystemMessage(
                            $"New lead unlocked: {lead.GoalName} at {lead.LocationName}",
                            SystemMessageTypes.Info);
                    }
                }

                // Check if investigation is now complete
                InvestigationCompleteResult completeResult = _investigationActivity.CheckInvestigationCompletion(investigation.Id);
                if (completeResult != null)
                {
                    // Investigation complete - UI will display completion modal
                    Console.WriteLine($"[ConversationFacade] Investigation '{investigation.Name}' COMPLETE!");
                }

                break; // Found matching investigation, stop searching
            }
        }
    }

    #endregion

    #endregion
}
