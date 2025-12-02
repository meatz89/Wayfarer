/// <summary>
/// Public API for the Conversation subsystem.
/// Handles all conversation operations with functionality absorbed from ConversationOrchestrator and CardDeckManager.
/// </summary>
public class SocialFacade
{
    private readonly GameWorld _gameWorld;
    private readonly MomentumManager _momentumManager;
    private readonly SocialEffectResolver _effectResolver;
    private readonly SocialNarrativeService _narrativeService;
    private readonly SocialChallengeDeckBuilder _deckBuilder;

    // External dependencies
    // ObservationManager eliminated - observation system removed
    private readonly TimeManager _timeManager;
    private readonly TokenMechanicsManager _tokenManager;
    private readonly MessageSystem _messageSystem;
    private readonly ObligationActivity _obligationActivity;
    private readonly SituationCompletionHandler _situationCompletionHandler;
    private readonly SocialResourceCalculator _resourceCalculator;

    private PersonalityRuleEnforcer _personalityEnforcer;

    public SocialFacade(
        GameWorld gameWorld,
        MomentumManager momentumManager,
        SocialEffectResolver effectResolver,
        SocialNarrativeService narrativeService,
        SocialChallengeDeckBuilder deckBuilder,
        TimeManager timeManager,
        TokenMechanicsManager tokenManager,
        MessageSystem messageSystem,
        ObligationActivity obligationActivity,
        SituationCompletionHandler situationCompletionHandler,
        SocialResourceCalculator resourceCalculator)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _momentumManager = momentumManager ?? throw new ArgumentNullException(nameof(momentumManager));
        _effectResolver = effectResolver ?? throw new ArgumentNullException(nameof(effectResolver));
        _narrativeService = narrativeService ?? throw new ArgumentNullException(nameof(narrativeService));
        _deckBuilder = deckBuilder ?? throw new ArgumentNullException(nameof(deckBuilder));
        // ObservationManager eliminated
        _timeManager = timeManager ?? throw new ArgumentNullException(nameof(timeManager));
        _tokenManager = tokenManager ?? throw new ArgumentNullException(nameof(tokenManager));
        _messageSystem = messageSystem ?? throw new ArgumentNullException(nameof(messageSystem));
        _obligationActivity = obligationActivity ?? throw new ArgumentNullException(nameof(obligationActivity));
        _situationCompletionHandler = situationCompletionHandler ?? throw new ArgumentNullException(nameof(situationCompletionHandler));
        _resourceCalculator = resourceCalculator ?? throw new ArgumentNullException(nameof(resourceCalculator));
    }

    /// <summary>
    /// Start a new conversation with an NPC using a specific request
    /// </summary>
    public SocialSession StartConversation(NPC npc, Situation situation)
    {
        if (IsConversationActive())
        {
            EndConversation();
        }

        if (npc == null)
        {
            throw new ArgumentNullException(nameof(npc));
        }

        if (situation == null)
        {
            throw new ArgumentNullException(nameof(situation));
        }

        // Get the challenge deck to determine max doubt threshold
        SocialChallengeDeck challengeDeck = situation.Deck as SocialChallengeDeck;
        if (challengeDeck == null)
        {
            throw new ArgumentException($"Situation '{situation.Name}' has no SocialChallengeDeck");
        }

        // Get connection state from NPC for session initialization
        ConnectionState initialState = npc.GetConnectionState();

        // Focus management is now handled directly by ConversationSession

        // Initialize personality rule enforcer based on NPC's personality
        // TRUST ENTITY INITIALIZATION: npc.ConversationModifier is initialized in NPC constructor
        _personalityEnforcer = new PersonalityRuleEnforcer(npc.ConversationModifier);

        // Get NPC token counts for session initialization
        // HIGHLANDER: Pass NPC object directly, not npc.ID
        // DOMAIN COLLECTION: GetTokensWithNPC returns List<TokenCount>
        List<TokenCount> npcTokens = _tokenManager.GetTokensWithNPC(npc);

        // Create session deck and get request cards from the request
        SocialDeckBuildResult buildResult = _deckBuilder.CreateConversationDeck(npc, situation);
        SocialSessionCardDeck deck = buildResult.Deck;
        List<CardInstance> SituationCards = buildResult.SituationCards;

        // Calculate starting resources based on player's highest stat
        Player player = _gameWorld.GetPlayer();
        int highestStat = Math.Max(player.Insight, Math.Max(player.Rapport, Math.Max(player.Authority, Math.Max(player.Diplomacy, player.Cunning))));
        int statBonus = (int)Math.Floor(highestStat / 3.0);

        int startingUnderstanding = 2 + statBonus;
        int startingMomentum = 2 + statBonus;
        int startingInitiative = 3 + statBonus;
        // Get request text from the situation description
        string requestText = situation.Description;

        // Create session with new properties
        _gameWorld.CurrentSocialSession = new SocialSession
        {
            NPC = npc,
            Situation = situation,
            ChallengeDeck = challengeDeck,
            CurrentState = initialState,
            InitialState = initialState,
            CurrentInitiative = startingInitiative,
            CurrentUnderstanding = startingUnderstanding,
            CurrentMomentum = startingMomentum,
            CurrentDoubt = 0, // Starts at 0, accumulates during play
            MaxDoubt = challengeDeck.DangerThreshold, // Max from deck (varies by difficulty)
            Cadence = 0, // Starts at 0
            TurnNumber = 0,
            Deck = deck, // HIGHLANDER: Deck manages ALL card piles
            TokenManager = _tokenManager,
            MomentumManager = _momentumManager,
            PersonalityEnforcer = _personalityEnforcer,  // Add personality enforcer to session
            RequestText = requestText // Set request text for Request conversations
        };

        // Check and unlock depth tiers based on starting Understanding (NOT Momentum)
        // Starting Understanding determines which tiers are accessible from the beginning
        _gameWorld.CurrentSocialSession.CheckAndUnlockTiers();// THEN: Perform initial draw of regular cards with tier-based filtering
                                                              // This is the initial conversation start, so we just draw cards without exhausting
        int drawCount = _gameWorld.CurrentSocialSession.GetDrawCount();
        // Draw with tier-based filtering (stats passed individually now)
        _gameWorld.CurrentSocialSession.Deck.DrawToHand(drawCount, _gameWorld.CurrentSocialSession, player);

        // Update request card playability based on initiative
        UpdateSituationCardPlayability(_gameWorld.CurrentSocialSession);

        // Update card playability based on starting initiative (already set from formula)
        UpdateCardPlayabilityBasedOnInitiative(_gameWorld.CurrentSocialSession);

        return _gameWorld.CurrentSocialSession;
    }

    /// <summary>
    /// End the current conversation
    /// TRANSITION TRACKING: Calls FailSituation if conversation failed to enable OnFailure transitions
    /// </summary>
    public SocialChallengeOutcome EndConversation()
    {
        if (!IsConversationActive())
            return null;

        _gameWorld.LastSocialOutcome = FinalizeConversation(_gameWorld.CurrentSocialSession);

        // TRANSITION TRACKING: If conversation failed, call FailSituation for OnFailure transitions
        // HIGHLANDER: Use Situation object reference, not SituationId string
        if (!_gameWorld.LastSocialOutcome.Success && _gameWorld.PendingSocialContext?.Situation != null)
        {
            // Call FailSituation to set LastChallengeSucceeded = false and trigger OnFailure
            _situationCompletionHandler.FailSituation(_gameWorld.PendingSocialContext.Situation);
        }

        // Calculate and save the final flow value back to the NPC (persistence)
        int stateBase = _gameWorld.CurrentSocialSession.CurrentState switch
        {
            ConnectionState.DISCONNECTED => 0,
            ConnectionState.GUARDED => 5,
            ConnectionState.NEUTRAL => 10,
            ConnectionState.RECEPTIVE => 15,
            ConnectionState.TRUSTING => 20,
            _ => 10
        };

        // Set relationship flow based on connection state only
        _gameWorld.CurrentSocialSession.NPC.RelationshipFlow = stateBase;

        // Apply token changes from other sources
        if (_gameWorld.LastSocialOutcome.TokensEarned != 0)
        {
            ConnectionType connectionType = DetermineConnectionTypeFromConversation(_gameWorld.CurrentSocialSession);
            _tokenManager.AddTokensToNPC(connectionType, _gameWorld.LastSocialOutcome.TokensEarned, _gameWorld.CurrentSocialSession.NPC);
        }

        // TACTICAL LAYER: Do NOT apply CompletionReward here
        // Rewards are strategic layer concern - GameOrchestrator applies them after receiving outcome
        // PendingContext stays alive for GameOrchestrator to process

        _gameWorld.CurrentSocialSession.Deck.ResetForNewConversation();
        _gameWorld.CurrentSocialSession = null;

        return _gameWorld.LastSocialOutcome;
    }

    /// <summary>
    /// Execute LISTEN action - Complete 4-Resource System Implementation
    /// Sequence: Apply Cadence Effects → Handle Card Persistence → Fixed Card Draw → Refresh Initiative → Check Situation Cards
    /// </summary>
    public async Task<SocialTurnResult> ExecuteListen()
    {
        if (!IsConversationActive())
        {
            throw new InvalidOperationException("No active conversation");
        }

        _gameWorld.CurrentSocialSession.TurnNumber++;

        // Advance time by 1 segment per conversation round (per documentation)
        _timeManager.AdvanceSegments(1);

        // ========== 4-RESOURCE SYSTEM LISTEN SEQUENCE ==========

        // 1. Apply Cadence Effects
        _resourceCalculator.ProcessCadenceEffectsOnListen(_gameWorld.CurrentSocialSession);


        // 3. Handle Card Persistence
        _resourceCalculator.ProcessCardPersistence(_gameWorld.CurrentSocialSession);

        // 4. Calculate Fixed Card Draw (4 + Cadence bonus)
        int cardsToDraw = _gameWorld.CurrentSocialSession.GetDrawCount();

        // 5. NO Initiative refresh (must be earned through cards like Steamworld Quest)
        // Initiative stays at current value - only Foundation cards can build it

        // 6. Check Situation Card Activation
        _resourceCalculator.CheckSituationCardActivation(_gameWorld.CurrentSocialSession);

        // 7. Reset Turn-Based Effects
        // TRUST INITIALIZATION: _personalityEnforcer is initialized in StartConversation
        _personalityEnforcer.OnListen(); // Resets Proud personality turn state

        // Draw cards from deck
        List<CardInstance> drawnCards = _resourceCalculator.ExecuteNewListenCardDraw(_gameWorld.CurrentSocialSession, cardsToDraw);

        // CRITICAL: Reduce cadence AFTER draw calculation (spec line 894)
        _resourceCalculator.ReduceCadenceAfterDraw(_gameWorld.CurrentSocialSession);

        // Force discard down to 7-card hand limit if necessary
        _gameWorld.CurrentSocialSession.Deck.DiscardDown(7);

        // Update card playability based on Initiative system
        _resourceCalculator.UpdateCardPlayabilityForInitiative(_gameWorld.CurrentSocialSession);

        // Generate narrative using the narrative service
        NarrativeOutput narrative = await _narrativeService.GenerateNarrativeAsync(
            _gameWorld.CurrentSocialSession,
            _gameWorld.CurrentSocialSession.NPC,
            drawnCards);
        string npcResponse = narrative.NPCDialogue;

        return new SocialTurnResult
        {
            Success = true,
            NewState = _gameWorld.CurrentSocialSession.CurrentState, // Connection State doesn't change during conversation
            NPCResponse = npcResponse,
            DrawnCards = drawnCards,
            DoubtLevel = _gameWorld.CurrentSocialSession.CurrentDoubt,
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

        _gameWorld.CurrentSocialSession.TurnNumber++;

        // Advance time by 1 segment per conversation round (per documentation)
        _timeManager.AdvanceSegments(1);

        // THREE PARALLEL SYSTEMS SYMMETRY: Check situation card type BEFORE validation
        // Situation cards have no SocialCardTemplate, so must be checked first
        if (selectedCard.CardType == CardTypes.Situation)
        {

            // Complete situation through SituationCompletionHandler (applies rewards: coins, StoryCubes, equipment)
            Situation completedSituation = _gameWorld.CurrentSocialSession.Situation;
            if (completedSituation != null)
            {
                await _situationCompletionHandler.CompleteSituation(completedSituation);
            }

            _gameWorld.CurrentSocialSession.Deck.PlayCard(selectedCard);
            EndConversation();

            // Generate narrative and return
            List<CardInstance> situationActiveCards = _gameWorld.CurrentSocialSession.Deck.HandCards.ToList();
            NarrativeOutput situationNarrative = await _narrativeService.GenerateNarrativeAsync(
                _gameWorld.CurrentSocialSession, _gameWorld.CurrentSocialSession.NPC, situationActiveCards);

            return new SocialTurnResult
            {
                Success = true,
                NewState = _gameWorld.CurrentSocialSession.CurrentState,
                NPCResponse = situationNarrative.NPCDialogue,
                Narrative = situationNarrative,
                EndsConversation = true
            };
        }

        // ========== 4-RESOURCE SYSTEM SPEAK SEQUENCE ==========

        // 1. Check Initiative Available
        int initiativeCost = _resourceCalculator.GetCardInitiativeCost(selectedCard);
        if (!_gameWorld.CurrentSocialSession.CanAffordCardInitiative(initiativeCost))
        {
            // Not enough Initiative - cannot play card
            return new SocialTurnResult
            {
                Success = false,
                NewState = _gameWorld.CurrentSocialSession.CurrentState,
                NPCResponse = "You don't have enough Initiative to play that card. Use Foundation cards to build Initiative.",
                DoubtLevel = _gameWorld.CurrentSocialSession.CurrentDoubt,
                PersonalityViolation = "Insufficient Initiative"
            };
        }

        // 2. Check Personality Restrictions (updated for Initiative system)
        // TRUST INITIALIZATION: _personalityEnforcer is initialized in StartConversation
        string violationMessage;
        if (!_resourceCalculator.ValidateInitiativePersonalityRules(_personalityEnforcer, selectedCard, out violationMessage))
        {
            return new SocialTurnResult
            {
                Success = false,
                NewState = _gameWorld.CurrentSocialSession.CurrentState,
                NPCResponse = violationMessage,
                DoubtLevel = _gameWorld.CurrentSocialSession.CurrentDoubt,
                PersonalityViolation = violationMessage
            };
        }

        // 3. Pay Card Cost (Initiative)
        if (!_gameWorld.CurrentSocialSession.SpendInitiative(initiativeCost))
        {
            // This should never happen due to check above, but safety check
            return new SocialTurnResult
            {
                Success = false,
                NewState = _gameWorld.CurrentSocialSession.CurrentState,
                NPCResponse = "Failed to spend Initiative for card play.",
                DoubtLevel = _gameWorld.CurrentSocialSession.CurrentDoubt
            };
        }

        // 4. Apply Cadence Change based on Delivery property (replaces old +1 flat system)
        _resourceCalculator.ApplyCadenceFromDelivery(selectedCard, _gameWorld.CurrentSocialSession);

        // 5. Calculate Success
        bool success = _resourceCalculator.CalculateInitiativeCardSuccess(selectedCard, _gameWorld.CurrentSocialSession);

        // 6. Process Card Results
        CardPlayResult playResult = _resourceCalculator.ProcessInitiativeCardPlay(selectedCard, success, _gameWorld.CurrentSocialSession);

        // Stats are now simple integers - no XP system
        // XP granting deleted as part of XP system removal
        Player player = _gameWorld.GetPlayer();

        // Knowledge system eliminated - no knowledge/secret granting

        // 8. Record card played for personality tracking
        // TRUST INITIALIZATION: _personalityEnforcer is initialized in StartConversation
        _personalityEnforcer.OnCardPlayed(selectedCard);

        // 9. Handle Card Persistence (Standard/Echo/Persistent/Banish)
        _resourceCalculator.ProcessCardAfterPlay(selectedCard, success, _gameWorld.CurrentSocialSession);

        // 9b. Increment Statement counter if this is a Statement card
        if (selectedCard.SocialCardTemplate.Persistence == PersistenceType.Statement
            && selectedCard.SocialCardTemplate.BoundStat.HasValue)
        {
            _gameWorld.CurrentSocialSession.IncrementStatementCount(selectedCard.SocialCardTemplate.BoundStat.Value);
        }

        // 10. Update card playability based on new Initiative level
        _resourceCalculator.UpdateCardPlayabilityForInitiative(_gameWorld.CurrentSocialSession);

        // Generate NPC response through narrative service
        List<CardInstance> activeCards = _gameWorld.CurrentSocialSession.Deck.HandCards.ToList();
        NarrativeOutput narrative = await _narrativeService.GenerateNarrativeAsync(
            _gameWorld.CurrentSocialSession,
            _gameWorld.CurrentSocialSession.NPC,
            activeCards);

        SocialTurnResult result = new SocialTurnResult
        {
            Success = success,
            NewState = _gameWorld.CurrentSocialSession.CurrentState, // Connection State doesn't change
            NPCResponse = narrative.NPCDialogue,
            FlowChange = 0, // No flow             OldFlow = 0, // No flow             NewFlow = 0, // No flow             DoubtLevel = _gameWorld.CurrentSocialSession.CurrentDoubt,
            CardPlayResult = playResult,
            Narrative = narrative,
            EndsConversation = playResult.EndsConversation // Request cards end conversation
        };

        // Add turn to history
        AddTurnToHistory(ActionType.Speak, selectedCard, result);

        // Check if conversation should end (doubt at maximum)
        if (ShouldEndConversation(_gameWorld.CurrentSocialSession))
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
        return _gameWorld.CurrentSocialSession;
    }

    /// <summary>
    /// Create a conversation context for UI - returns typed context
    /// NOTE: NPC-triggered scene activation is orchestrated by GameOrchestrator BEFORE calling this method
    /// </summary>
    public async Task<SocialChallengeContext> CreateConversationContext(NPC npc, Situation situation)
    {
        if (npc == null)
        {
            return SocialContextFactory.CreateInvalidContext("NPC is null");
        }

        if (situation == null)
        {
            return SocialContextFactory.CreateInvalidContext("Situation is null");
        }

        // Start conversation with the request (doubt starts at 0, max from deck)
        SocialSession session = StartConversation(npc, situation);

        // Create typed context based on request's conversation type
        SocialChallengeContext context = SocialContextFactory.CreateContext(
            situation,
            npc,
            session,
            new List<CardInstance>(), // observationCards - empty for now
            ResourceState.FromPlayerResourceState(_gameWorld.GetPlayerResourceState()),
            _gameWorld.GetPlayerCurrentLocation().ToString(),
            _timeManager.GetCurrentTimeBlock().ToString());

        // Situation cards now handle domain logic - no context-specific initialization needed

        return context;
    }

    /// <summary>
    /// Check if a conversation is currently active
    /// </summary>
    public bool IsConversationActive()
    {
        return _gameWorld.CurrentSocialSession != null;
    }

    /// <summary>
    /// Get the last conversation outcome
    /// </summary>
    public SocialChallengeOutcome GetLastOutcome()
    {
        return _gameWorld.LastSocialOutcome;
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
        int cardInitiative = _resourceCalculator.GetCardInitiativeCost(card);
        int availableInitiative = session.GetCurrentInitiative();
        bool canAfford = session.CanAffordCard(cardInitiative);

        //// Removed excessive logging

        if (!canAfford)
            return false;

        // Additional checks for situation cards that are still in RequestPile
        // Cards that have been moved to ActiveCards have already met their threshold
        if (card.CardType == CardTypes.Situation)
        {
            // If card is in RequestPile, check momentum threshold
            // TRUST SESSION: Deck is initialized in StartConversation
            if (session.Deck.IsCardInRequestPile(card))
            {
                // TRUST DOMAIN MODEL: Situation cards have Context with threshold
                int momentumThreshold = card.Context.threshold;
                int currentMomentum = session.CurrentMomentum;
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
    public bool CanSelectCard(CardInstance card, List<CardInstance> currentSelection)
    {
        if (!IsConversationActive())
            return false;

        // Can't select if already selected
        if (currentSelection.Contains(card))
            return true; // Can deselect

        // Check initiative cost against available initiative
        int currentInitiativeCost = currentSelection.Sum(c => _resourceCalculator.GetCardInitiativeCost(c));
        int totalInitiativeCost = currentInitiativeCost + _resourceCalculator.GetCardInitiativeCost(card);

        return totalInitiativeCost <= _gameWorld.CurrentSocialSession.GetCurrentInitiative();
    }

    // AtmosphereManager has been deleted - atmosphere is simplified to always Neutral

    /// <summary>
    /// Add turn to conversation history
    /// TRUST INITIALIZATION: Called only during active conversation (CurrentSocialSession exists)
    /// </summary>
    private void AddTurnToHistory(ActionType actionType, CardInstance cardPlayed, SocialTurnResult result)
    {
        SocialTurn turn = new SocialTurn
        {
            ActionType = actionType,
            Narrative = result.Narrative,
            Result = result,
            TurnNumber = _gameWorld.CurrentSocialSession.TurnNumber,
            CardPlayed = cardPlayed
        };
        _gameWorld.CurrentSocialSession.TurnHistory.Add(turn);
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
    private bool ShouldEndConversation(SocialSession session)
    {
        // End if request card was played (request cards end conversation immediately)
        // TRUST SESSION INITIALIZATION: TurnHistory is initialized in SocialSession constructor
        if (session.TurnHistory.Any())
        {
            SocialTurn lastTurn = session.TurnHistory.Last();
            // KEEP ?.CardPlayed - LISTEN actions have null CardPlayed (only SPEAK has cards)
            if (lastTurn.CardPlayed?.CardType == CardTypes.Situation)
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

        // Check if any request cards were played (Letter, Promise, or BurdenSituation types)
        bool requestAchieved = session.Deck.SpokenCards.Any(c => c.CardType == CardTypes.Situation);
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
        if (finalMomentum >= 12) // Enhanced situation threshold
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
        // Use the connection type from the situation
        return session.Situation?.ConnectionType ?? ConnectionType.Trust;
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
        // TRUST SESSION INITIALIZATION: MomentumManager is injected in StartConversation
        if (impulseCount > 0)
        {
            session.MomentumManager.AddDoubt(session, impulseCount);
        }
        Player player = _gameWorld.GetPlayer();
        session.Deck.DrawToHand(baseDrawCount, session, player);

        // Get the drawn cards for return value
        List<CardInstance> drawnCards = session.Deck.HandCards.TakeLast(baseDrawCount).ToList();

        // Check if any situation cards should become playable based on rapport
        UpdateSituationCardPlayabilityAfterListen(session);

        return drawnCards;
    }

    /// <summary>
    /// Check if situation cards should become playable after LISTEN based on momentum threshold
    /// </summary>
    private void UpdateSituationCardPlayabilityAfterListen(SocialSession session)
    {
        // Get current momentum
        int currentMomentum = session.CurrentMomentum;

        // Check all situation cards in active hand
        foreach (CardInstance card in session.Deck.HandCards)
        {
            // Only process situation cards that are currently Unplayable
            if ((card.CardType == CardTypes.Situation)
                && !card.IsPlayable)
            {
                // Check if momentum threshold is met
                // TRUST DOMAIN MODEL: Situation cards have Context with threshold
                int momentumThreshold = card.Context.threshold;

                if (currentMomentum >= momentumThreshold)
                {
                    // Make card playable
                    card.IsPlayable = true;
                    // Request cards already have Impulse + Opening persistence set

                    // Mark that a request card is now playable
                    session.SituationCardDrawn = true;
                }
            }
        }
    }

    /// <summary>
    /// Mark request card presence in conversation session
    /// </summary>
    private void UpdateSituationCardPlayability(SocialSession session)
    {
        // This is called at conversation start - just check for situation card presence
        bool hasSituationCard = session.Deck.HandCards
            .Any(c => c.CardType == CardTypes.Situation);

        if (hasSituationCard)
        {
            session.SituationCardDrawn = true;
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
            if (card.CardType == CardTypes.Situation)
            {
                continue; // Don't modify request card playability here
            }

            // Calculate effective Initiative cost for this card
            int effectiveInitiativeCost = _resourceCalculator.GetCardInitiativeCost(card);

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
    /// TRUST INITIALIZATION: CurrentSocialSession and Deck are initialized in StartConversation
    /// </summary>
    public IReadOnlyList<CardInstance> GetHandCards()
    {
        if (_gameWorld.CurrentSocialSession == null)
            return new List<CardInstance>();

        return _gameWorld.CurrentSocialSession.Deck.HandCards;
    }


    #endregion
}
