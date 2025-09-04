using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Orchestrates the new conversation system with flow battery, atmosphere persistence, and single-card mechanics.
/// Handles state transitions at ±3, focus management, and request card exhaust mechanics.
/// </summary>
public class ConversationOrchestrator
{
    private readonly CardDeckManager _deckManager;
    private readonly DialogueGenerator _dialogueGenerator;
    private readonly ObligationQueueManager _queueManager;
    private readonly TokenMechanicsManager _tokenManager;
    private readonly FocusManager _focusManager;
    private readonly AtmosphereManager _atmosphereManager;
    private readonly CardEffectProcessor _effectProcessor;
    private readonly GameWorld _gameWorld;
    private FlowManager? _flowBatteryManager;

    public ConversationOrchestrator(
        CardDeckManager deckManager,
        DialogueGenerator dialogueGenerator,
        ObligationQueueManager queueManager,
        TokenMechanicsManager tokenManager,
        FocusManager focusManager,
        AtmosphereManager atmosphereManager,
        CardEffectProcessor effectProcessor,
        GameWorld gameWorld)
    {
        _deckManager = deckManager ?? throw new ArgumentNullException(nameof(deckManager));
        _dialogueGenerator = dialogueGenerator ?? throw new ArgumentNullException(nameof(dialogueGenerator));
        _queueManager = queueManager ?? throw new ArgumentNullException(nameof(queueManager));
        _tokenManager = tokenManager ?? throw new ArgumentNullException(nameof(tokenManager));
        _focusManager = focusManager ?? throw new ArgumentNullException(nameof(focusManager));
        _atmosphereManager = atmosphereManager ?? throw new ArgumentNullException(nameof(atmosphereManager));
        _effectProcessor = effectProcessor ?? throw new ArgumentNullException(nameof(effectProcessor));
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
    }

    /// <summary>
    /// Create a new conversation session with battery system
    /// </summary>
    public ConversationSession CreateSession(NPC npc, ConversationType conversationType, List<CardInstance> observationCards)
    {
        // All conversations start in NEUTRAL state
        EmotionalState initialState = EmotionalState.NEUTRAL;

        // Initialize flow battery manager
        _flowBatteryManager = new FlowManager(initialState);
        _flowBatteryManager.StateTransitioned += OnStateTransitioned;
        _flowBatteryManager.ConversationEnded += OnConversationEnded;

        // Initialize focus manager
        _focusManager.SetBaseCapacity(initialState);
        _focusManager.Reset();

        // Reset atmosphere manager
        _atmosphereManager.Reset();

        // Create session deck and get request card
        var (deck, requestCard) = _deckManager.CreateConversationDeck(npc, conversationType, observationCards);

        // Create rapport manager with initial token counts
        Dictionary<ConnectionType, int> npcTokens = GetNpcTokenCounts(npc);
        RapportManager rapportManager = new RapportManager(npcTokens);

        // Create session with new properties
        ConversationSession session = new ConversationSession
        {
            NPC = npc,
            ConversationType = conversationType,
            CurrentState = initialState,
            InitialState = initialState,
            FlowBattery = 0, // Start at 0
            CurrentFocus = 0,
            MaxFocus = _focusManager.CurrentCapacity,
            CurrentAtmosphere = AtmosphereType.Neutral,
            CurrentPatience = 10,
            MaxPatience = 10,
            TurnNumber = 0,
            Deck = deck,
            Hand = new HandDeck(),
            TokenManager = _tokenManager,
            FlowManager = _flowBatteryManager,
            RapportManager = rapportManager,
            ObservationCards = observationCards ?? new List<CardInstance>()
        };

        // Add request card directly to hand if present (starts as Unplayable)
        if (requestCard != null)
        {
            session.Hand.AddCard(requestCard);
        }

        // Perform initial LISTEN with no patience cost - draws cards based on emotional state
        List<CardInstance> initialCards = _deckManager.ExecuteListen(session);
        
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
    public ConversationTurnResult ProcessListenAction(ConversationSession session)
    {
        if (session == null)
            throw new ArgumentNullException(nameof(session));

        session.TurnNumber++;

        // Deduct patience cost (unless Patient atmosphere)
        if (!_atmosphereManager.ShouldWaivePatienceCost())
        {
            session.CurrentPatience--;
        }

        // Execute LISTEN through deck manager
        List<CardInstance> drawnCards = _deckManager.ExecuteListen(session);

        // Update session focus state
        session.CurrentFocus = _focusManager.CurrentSpentFocus;
        session.MaxFocus = _focusManager.CurrentCapacity;

        // Update card playability based on current focus
        _deckManager.UpdateCardPlayabilityBasedOnFocus(session);

        // Generate NPC response
        string npcResponse = _dialogueGenerator.GenerateListenResponse(session.NPC, session.CurrentState, drawnCards);

        return new ConversationTurnResult
        {
            Success = true,
            NewState = session.CurrentState,
            NPCResponse = npcResponse,
            DrawnCards = drawnCards,
            PatienceRemaining = session.CurrentPatience
        };
    }

    /// <summary>
    /// Process SPEAK action with single card selection
    /// </summary>
    public ConversationTurnResult ProcessSpeakAction(ConversationSession session, CardInstance selectedCard)
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
        int flowChange = playResult.TotalFlow;

        // Apply flow change through battery manager
        bool conversationEnded = false;
        EmotionalState newState = session.CurrentState;
        
        if (_flowBatteryManager != null && flowChange != 0)
        {
            var (stateChanged, resultState, shouldEnd) = 
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

        // Generate NPC response
        string npcResponse = _dialogueGenerator.GenerateSpeakResponse(
            session.NPC,
            session.CurrentState,
            new HashSet<CardInstance> { selectedCard },
            playResult,
            flowChange);

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
            CardPlayResult = playResult
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
    private void OnStateTransitioned(EmotionalState oldState, EmotionalState newState)
    {
        // Log or handle state transition if needed
    }

    /// <summary>
    /// Handle conversation ended event from flow battery
    /// </summary>
    private void OnConversationEnded()
    {
        // Conversation ends due to DESPERATE at -3
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
            _flowBatteryManager.CurrentState == EmotionalState.DESPERATE && 
            _flowBatteryManager.CurrentFlow <= -3)
            return true;

        // End if deck is empty and hand is empty
        if (!session.Deck.HasCardsAvailable() && session.HandCards.Count == 0)
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
        if (session.HandCards.Any())
        {
            List<CardInstance> playableCards = session.HandCards.Where(c =>
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
        else if (session.CurrentState == EmotionalState.DESPERATE && session.FlowBattery <= -3)
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
    private int CalculateTokenReward(EmotionalState finalState, int finalFlow)
    {
        // Base reward by state
        int baseReward = finalState switch
        {
            EmotionalState.CONNECTED => 3,
            EmotionalState.OPEN => 2,
            EmotionalState.NEUTRAL => 1,
            EmotionalState.TENSE => 0,
            EmotionalState.DESPERATE => -1,
            _ => 0
        };

        // Bonus for positive flow
        if (finalFlow > 0)
            baseReward += 1;
        else if (finalFlow < 0)
            baseReward -= 1;

        return Math.Max(0, baseReward);
    }
    
    public ConversationTurnResult ProcessSpeakAction(ConversationSession session, HashSet<CardInstance> selectedCards)
    {
        CardInstance? firstCard = selectedCards?.FirstOrDefault();
        if (firstCard == null)
        {
            throw new ArgumentException("Must select exactly one card to speak");
        }

        return ProcessSpeakAction(session, firstCard);
    }

    /// <summary>
    /// Create exchange session (simplified for new system)
    /// </summary>
    public ConversationSession CreateExchangeSession(NPC npc)
    {
        // Initialize exchange deck from GameWorld
        if (_gameWorld.NPCExchangeDecks.TryGetValue(npc.ID.ToLower(), out List<ConversationCard>? exchangeCards))
        {
            npc.InitializeExchangeDeck(exchangeCards);
        }
        else
        {
            npc.InitializeExchangeDeck(null);
        }

        return CreateSession(npc, ConversationType.Commerce, null);
    }

    /// <summary>
    /// Process exchange response (maintained for commerce conversations)
    /// </summary>
    public ConversationTurnResult ProcessExchangeResponse(ConversationSession session, bool accepted, ExchangeData exchangeData)
    {
        if (session.ConversationType != ConversationType.Commerce)
        {
            throw new InvalidOperationException("Exchange responses only valid for Commerce conversations");
        }

        session.TurnNumber++;

        string npcResponse;
        if (accepted)
        {
            npcResponse = _dialogueGenerator.GenerateExchangeAcceptedResponse(session.NPC, exchangeData);
        }
        else
        {
            npcResponse = _dialogueGenerator.GenerateExchangeDeclinedResponse(session.NPC);
        }

        return new ConversationTurnResult
        {
            Success = true,
            NPCResponse = npcResponse,
            ExchangeAccepted = accepted
        };
    }

    /// <summary>
    /// Check if letter should be generated (based on positive outcomes)
    /// </summary>
    public bool ShouldGenerateLetter(ConversationSession session)
    {
        if (session.LetterGenerated)
            return false;

        // Generate letters from positive connections
        return session.CurrentState == EmotionalState.CONNECTED ||
               (session.CurrentState == EmotionalState.OPEN && session.FlowBattery > 1);
    }

    /// <summary>
    /// Create a letter obligation from successful conversation
    /// </summary>
    public DeliveryObligation CreateLetterObligation(ConversationSession session)
    {
        int stateValue = (int)session.CurrentState; // Use state as base value
        int flowBonus = Math.Max(0, session.FlowBattery);

        // Calculate deadline and payment based on relationship quality
        int baseMinutes = 720; // 12 hours base
        int deadlineMinutes = Math.Max(120, baseMinutes - (stateValue * 60) - (flowBonus * 30));
        int payment = 5 + stateValue + flowBonus;

        // Determine tier and focus
        TierLevel tier = stateValue >= 4 ? TierLevel.T3 :
                        stateValue >= 2 ? TierLevel.T2 : TierLevel.T1;

        EmotionalFocus focus = deadlineMinutes <= 180 ? EmotionalFocus.CRITICAL :
                                deadlineMinutes <= 360 ? EmotionalFocus.HIGH :
                                deadlineMinutes <= 720 ? EmotionalFocus.MEDIUM :
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
            DeadlineInMinutes = deadlineMinutes,
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
            DeadlineInMinutes = 240, // 4 hours for urgent letters
            Payment = 15, // Higher payment for urgent delivery
            Tier = (TierLevel)npc.Tier,
            EmotionalFocus = EmotionalFocus.HIGH, // High emotional focus for urgency
            Description = $"Urgent letter from {npc.Name} - they desperately need help!"
        };
    }

    /// <summary>
    /// Get current token counts for an NPC to initialize rapport
    /// </summary>
    private Dictionary<ConnectionType, int> GetNpcTokenCounts(NPC npc)
    {
        var tokenCounts = new Dictionary<ConnectionType, int>
        {
            { ConnectionType.Trust, _tokenManager.GetTokenCount(ConnectionType.Trust, npc.ID) },
            { ConnectionType.Commerce, _tokenManager.GetTokenCount(ConnectionType.Commerce, npc.ID) },
            { ConnectionType.Status, _tokenManager.GetTokenCount(ConnectionType.Status, npc.ID) },
            { ConnectionType.Shadow, _tokenManager.GetTokenCount(ConnectionType.Shadow, npc.ID) }
        };
        return tokenCounts;
    }
}