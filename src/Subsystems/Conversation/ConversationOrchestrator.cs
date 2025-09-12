using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Orchestrates the new conversation system with flow battery, atmosphere persistence, and single-card mechanics.
/// Handles state transitions at ±3, focus management, and request card exhaust mechanics.
/// </summary>
public class ConversationOrchestrator
{
    private readonly CardDeckManager _deckManager;
    private readonly ObligationQueueManager _queueManager;
    private readonly TokenMechanicsManager _tokenManager;
    private readonly FocusManager _focusManager;
    private readonly AtmosphereManager _atmosphereManager;
    private readonly CardEffectProcessor _effectProcessor;
    private readonly GameWorld _gameWorld;
    private readonly ConversationNarrativeService _narrativeService;
    private FlowManager? _flowBatteryManager;

    public ConversationOrchestrator(
        CardDeckManager deckManager,
        ObligationQueueManager queueManager,
        TokenMechanicsManager tokenManager,
        FocusManager focusManager,
        AtmosphereManager atmosphereManager,
        CardEffectProcessor effectProcessor,
        GameWorld gameWorld,
        ConversationNarrativeService narrativeService)
    {
        _deckManager = deckManager ?? throw new ArgumentNullException(nameof(deckManager));
        _queueManager = queueManager ?? throw new ArgumentNullException(nameof(queueManager));
        _tokenManager = tokenManager ?? throw new ArgumentNullException(nameof(tokenManager));
        _focusManager = focusManager ?? throw new ArgumentNullException(nameof(focusManager));
        _atmosphereManager = atmosphereManager ?? throw new ArgumentNullException(nameof(atmosphereManager));
        _effectProcessor = effectProcessor ?? throw new ArgumentNullException(nameof(effectProcessor));
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _narrativeService = narrativeService ?? throw new ArgumentNullException(nameof(narrativeService));
    }

    /// <summary>
    /// Create a new conversation session with battery system
    /// </summary>
    public ConversationSession CreateSession(NPC npc, ConversationType conversationType, List<CardInstance> observationCards)
    {
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

        // Create session deck and get goal card
        (SessionCardDeck deck, CardInstance goalCard) = _deckManager.CreateConversationDeck(npc, conversationType, observationCards);

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

        // Set rapport goal for standard conversations based on persisted state
        int? rapportGoal = null;
        if (conversationType == ConversationType.FriendlyChat)
        {
            rapportGoal = initialState switch
            {
                ConnectionState.DISCONNECTED => 15,
                ConnectionState.GUARDED => 20,
                ConnectionState.NEUTRAL => 25,
                ConnectionState.RECEPTIVE => 30,
                ConnectionState.TRUSTING => 35,
                _ => 25
            };
        }

        // Create session with new properties
        ConversationSession session = new ConversationSession
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
            Deck = deck,
            DrawPile = new Pile(),
            ExhaustPile = new Pile(),
            ActiveCards = new Pile(),
            TokenManager = _tokenManager,
            FlowManager = _flowBatteryManager,
            RapportManager = rapportManager,
            ObservationCards = observationCards ?? new List<CardInstance>(),
            RapportGoal = rapportGoal
        };

        // FIRST: Add the goal card to active cards immediately if present
        // This ensures it's visible from the very start of the conversation
        if (goalCard != null)
        {
            session.ActiveCards.Add(goalCard);
        }

        // THEN: Perform initial draw of regular cards
        // This is the initial conversation start, so we just draw cards without exhausting
        _focusManager.RefreshPool();
        int drawCount = session.GetDrawCount();
        List<CardInstance> initialCards = session.Deck.DrawCards(drawCount);
        session.ActiveCards.AddRange(initialCards);

        // Update request card playability based on focus
        _deckManager.UpdateRequestCardPlayability(session);

        // Reset focus after initial draw (as per standard LISTEN)
        session.CurrentFocus = _focusManager.CurrentSpentFocus;
        session.MaxFocus = _focusManager.CurrentCapacity;

        // Update card playability based on initial focus
        _deckManager.UpdateCardPlayabilityBasedOnFocus(session);

        return session;
    }

    /// <summary>
    /// Process LISTEN action - refresh focus and draw cards
    /// </summary>
    public async Task<ConversationTurnResult> ProcessListenAction(ConversationSession session)
    {
        if (session == null)
            throw new ArgumentNullException(nameof(session));

        session.TurnNumber++;

        // Deduct patience cost (unless Patient atmosphere)
        if (!_atmosphereManager.ShouldWaivePatienceCost())
        {
            session.CurrentPatience--;
            session.NPC.DailyPatience--; // Also deduct from NPC's daily pool
        }

        // Execute LISTEN through deck manager
        List<CardInstance> drawnCards = _deckManager.ExecuteListen(session);

        // Update session focus state
        session.CurrentFocus = _focusManager.CurrentSpentFocus;
        session.MaxFocus = _focusManager.CurrentCapacity;

        // Update card playability based on current focus
        _deckManager.UpdateCardPlayabilityBasedOnFocus(session);

        // Generate narrative using the narrative service
        NarrativeOutput narrative = await _narrativeService.GenerateNarrativeAsync(
            session, 
            session.NPC, 
            drawnCards);
        string npcResponse = narrative.NPCDialogue;

        return new ConversationTurnResult
        {
            Success = true,
            NewState = session.CurrentState,
            NPCResponse = npcResponse,
            DrawnCards = drawnCards,
            PatienceRemaining = session.CurrentPatience,
            Narrative = narrative  // Include the full narrative output
        };
    }

    /// <summary>
    /// Process SPEAK action with single card selection
    /// </summary>
    public async Task<ConversationTurnResult> ProcessSpeakAction(ConversationSession session, CardInstance selectedCard)
    {
        if (session == null)
            throw new ArgumentNullException(nameof(session));
        if (selectedCard == null)
            throw new ArgumentException("Must select a card to speak");

        session.TurnNumber++;

        // SPEAK costs focus (focus), not patience
        // Patience is only deducted for LISTEN actions

        // Play the card through deck manager
        CardPlayResult playResult = _deckManager.PlayCard(session, selectedCard);

        int oldFlow = session.FlowBattery;
        int flowChange = playResult.FinalFlow;

        // Apply flow change through battery manager
        bool conversationEnded = false;
        ConnectionState newState = session.CurrentState;

        if (_flowBatteryManager != null && flowChange != 0)
        {
            (bool stateChanged, ConnectionState resultState, bool shouldEnd) =
                _flowBatteryManager.ApplyFlowChange(flowChange, session.CurrentAtmosphere);

            session.FlowBattery = _flowBatteryManager.CurrentFlow;
            conversationEnded = shouldEnd;

            if (stateChanged)
            {
                newState = resultState;
                session.CurrentState = newState;

                // Update focus capacity for new state
                _focusManager.SetBaseCapacity(newState);
            }
        }

        // Update session atmosphere
        session.CurrentAtmosphere = _atmosphereManager.CurrentAtmosphere;
        session.CurrentFocus = _focusManager.CurrentSpentFocus;
        session.MaxFocus = _focusManager.CurrentCapacity;

        // Update card playability based on current focus
        _deckManager.UpdateCardPlayabilityBasedOnFocus(session);

        // Generate NPC response through narrative service
        List<CardInstance> activeCards = session.ActiveCards.Cards.ToList();
        NarrativeOutput narrative = await _narrativeService.GenerateNarrativeAsync(
            session,
            session.NPC,
            activeCards);
        
        string npcResponse = narrative.NPCDialogue;
        
        ConversationTurnResult result = new ConversationTurnResult
        {
            Success = playResult.Success,
            NewState = newState,
            NPCResponse = npcResponse,
            FlowChange = flowChange,
            OldFlow = oldFlow,
            NewFlow = session.FlowBattery,
            PatienceRemaining = session.CurrentPatience,
            PlayedCards = new List<CardInstance> { selectedCard },
            CardPlayResult = playResult,
            Narrative = narrative  // Pass the full narrative output
        };

        // Mark conversation as ended if needed
        if (conversationEnded || !playResult.Success)
        {
            result.Success = conversationEnded ? false : result.Success;
        }

        return result;
    }

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
    public bool ShouldEndConversation(ConversationSession session)
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
        if (!session.Deck.HasCardsAvailable() && session.ActiveCards.Count == 0)
            return true;

        return false;
    }

    /// <summary>
    /// Get available actions for current state
    /// </summary>
    public List<ConversationAction> GetAvailableActions(ConversationSession session)
    {
        List<ConversationAction> actions = new List<ConversationAction>();

        // Can always listen if deck has cards and have patience
        if (session.Deck.HasCardsAvailable() && session.CurrentPatience > 0)
        {
            actions.Add(new ConversationAction
            {
                ActionType = ActionType.Listen,
                IsAvailable = true
            });
        }

        // Can speak if have cards with available focus
        if (session.ActiveCards.Any())
        {
            List<CardInstance> playableCards = session.ActiveCards.Cards.Where(c =>
                _deckManager.CanPlayCard(c, session)).ToList();

            if (playableCards.Any())
            {
                actions.Add(new ConversationAction
                {
                    ActionType = ActionType.Speak,
                    IsAvailable = true,
                    AvailableCards = playableCards
                });
            }
        }

        return actions;
    }

    /// <summary>
    /// Finalize conversation and calculate outcome
    /// </summary>
    public ConversationOutcome FinalizeConversation(ConversationSession session)
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

        // Check if any request cards were played
        bool requestAchieved = session.PlayedCards?.Any(c => c.Properties.Contains(CardProperty.Impulse) && c.Properties.Contains(CardProperty.Opening)) ?? false;
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

    public async Task<ConversationTurnResult> ProcessSpeakAction(ConversationSession session, HashSet<CardInstance> selectedCards)
    {
        CardInstance? firstCard = selectedCards?.FirstOrDefault();
        if (firstCard == null)
        {
            throw new ArgumentException("Must select exactly one card to speak");
        }

        return await ProcessSpeakAction(session, firstCard);
    }

    // REMOVED: Exchange methods deleted - exchanges use separate Exchange system, not conversation system
    // CreateExchangeSession and ProcessExchangeResponse have been removed
    // Exchanges should use ExchangeFacade/ExchangeOrchestrator, not ConversationOrchestrator

    /// <summary>
    /// Check if letter should be generated (based on positive outcomes)
    /// </summary>
    public bool ShouldGenerateLetter(ConversationSession session)
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
    public DeliveryObligation CreateLetterObligation(ConversationSession session)
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
        NPC? recipient = otherNpcs.Any() ? otherNpcs[new Random().Next(otherNpcs.Count)] : null;

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
    public DeliveryObligation CreateUrgentLetter(NPC npc)
    {
        // Find a suitable recipient (family member, friend, etc.)
        List<NPC> allNpcs = _gameWorld.GetAllNPCs();
        NPC? recipient = allNpcs.FirstOrDefault(n => n.ID != npc.ID) ?? allNpcs.FirstOrDefault();

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
}