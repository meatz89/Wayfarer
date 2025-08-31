using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Orchestrates conversation flow and manages the conversation lifecycle.
/// Handles action processing, state transitions, and goal management.
/// </summary>
public class ConversationOrchestrator
{
    private readonly CardDeckManager _deckManager;
    private readonly EmotionalStateManager _stateManager;
    private readonly ComfortManager _comfortManager;
    private readonly DialogueGenerator _dialogueGenerator;
    private readonly ObligationQueueManager _queueManager;
    private readonly TokenMechanicsManager _tokenManager;
    private readonly GameWorld _gameWorld;

    public ConversationOrchestrator(
        CardDeckManager deckManager,
        EmotionalStateManager stateManager,
        ComfortManager comfortManager,
        DialogueGenerator dialogueGenerator,
        ObligationQueueManager queueManager,
        TokenMechanicsManager tokenManager,
        GameWorld gameWorld)
    {
        _deckManager = deckManager ?? throw new ArgumentNullException(nameof(deckManager));
        _stateManager = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
        _comfortManager = comfortManager ?? throw new ArgumentNullException(nameof(comfortManager));
        _dialogueGenerator = dialogueGenerator ?? throw new ArgumentNullException(nameof(dialogueGenerator));
        _queueManager = queueManager ?? throw new ArgumentNullException(nameof(queueManager));
        _tokenManager = tokenManager ?? throw new ArgumentNullException(nameof(tokenManager));
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
    }

    /// <summary>
    /// Create a new conversation session
    /// </summary>
    public ConversationSession CreateSession(NPC npc, ConversationType conversationType, List<CardInstance> observationCards)
    {
        // Special handling for exchange conversations
        if (conversationType == ConversationType.Commerce)
        {
            return CreateExchangeSession(npc);
        }

        // Check initial emotional state
        var initialState = ConversationRules.DetermineInitialState(npc, _queueManager);
        if (initialState == EmotionalState.HOSTILE && conversationType != ConversationType.Resolution)
        {
            throw new InvalidOperationException($"{npc.Name} is hostile and won't converse");
        }

        // Create standard conversation session
        var session = ConversationSession.StartConversation(
            npc,
            _queueManager,
            _tokenManager,
            observationCards,
            conversationType,
            _gameWorld.GetPlayerResourceState(),
            _gameWorld
        );

        // Handle special conversation types
        if (conversationType == ConversationType.Promise)
        {
            PreparePromiseConversation(session);
        }
        else if (conversationType == ConversationType.Resolution)
        {
            PrepareResolutionConversation(session);
        }

        return session;
    }

    /// <summary>
    /// Create a simplified exchange session
    /// </summary>
    private ConversationSession CreateExchangeSession(NPC npc)
    {
        // Initialize exchange deck from GameWorld
        if (_gameWorld.NPCExchangeDecks.TryGetValue(npc.ID.ToLower(), out var exchangeCards))
        {
            npc.InitializeExchangeDeck(exchangeCards);
        }
        else
        {
            npc.InitializeExchangeDeck(null);
        }

        var spotDomainTags = _gameWorld.GetCurrentSpotDomainTags();
        return ConversationSession.StartExchange(npc, _gameWorld.GetPlayerResourceState(), _tokenManager, spotDomainTags, _queueManager, _gameWorld);
    }

    /// <summary>
    /// Prepare a promise conversation with letter cards
    /// </summary>
    private void PreparePromiseConversation(ConversationSession session)
    {
        // Promise conversations mix in letter cards and use a Letter goal card
        if (session.NPC.GoalDeck != null && session.NPC.GoalDeck.HasCardsAvailable())
        {
            var goalCard = _deckManager.SelectValidGoalCard(session.NPC, session.CurrentState);
            if (goalCard != null)
            {
                session.GoalCard = goalCard;
                _deckManager.ShuffleGoalIntoDeck(session.Deck, goalCard);
            }
        }
    }

    /// <summary>
    /// Prepare a resolution conversation focused on burden cards
    /// </summary>
    private void PrepareResolutionConversation(ConversationSession session)
    {
        // Resolution conversations focus on burden cards
        // Ensure burden cards are shuffled into the deck
        var burdenCount = session.NPC.CountBurdenCards();
        if (burdenCount < 2)
        {
            Console.WriteLine($"[ConversationOrchestrator] Warning: Resolution conversation started with only {burdenCount} burden cards");
        }
    }

    /// <summary>
    /// Process LISTEN action
    /// </summary>
    public ConversationTurnResult ProcessListenAction(ConversationSession session)
    {
        if (session == null)
            throw new ArgumentNullException(nameof(session));

        session.TurnNumber++;
        session.CurrentPatience--;

        // Remove fleeting cards
        var fleetingCards = session.HandCards.Where(c => c.Persistence == PersistenceType.Fleeting).ToList();
        foreach (var card in fleetingCards)
        {
            session.Hand.RemoveCards(new[] { card });
        }

        // Draw new cards based on emotional state
        var drawCount = _stateManager.GetDrawCount(session.CurrentState);
        var newCards = _deckManager.DrawCards(session.Deck, drawCount, session.CurrentState);
        
        foreach (var card in newCards)
        {
            session.Hand.AddCard(card);
            
            // Check if goal card was drawn
            if (session.GoalCard != null && card.InstanceId == session.GoalCard.InstanceId)
            {
                session.GoalCardDrawn = true;
                session.GoalUrgencyCounter = 3; // 3 turns to play
            }
        }

        // Check goal card urgency
        CheckGoalCardUrgency(session);

        // Generate NPC response
        var npcResponse = _dialogueGenerator.GenerateListenResponse(session.NPC, session.CurrentState, newCards);

        return new ConversationTurnResult
        {
            Success = true,
            NewState = session.CurrentState,
            NPCResponse = npcResponse,
            DrawnCards = newCards,
            RemovedCards = fleetingCards,
            PatienceRemaining = session.CurrentPatience
        };
    }

    /// <summary>
    /// Process SPEAK action with selected cards
    /// </summary>
    public ConversationTurnResult ProcessSpeakAction(ConversationSession session, HashSet<CardInstance> selectedCards)
    {
        if (session == null)
            throw new ArgumentNullException(nameof(session));
        if (selectedCards == null || !selectedCards.Any())
            throw new ArgumentException("Must select at least one card to speak");

        session.TurnNumber++;
        session.CurrentPatience--;

        // Validate card selection
        if (!_deckManager.ValidateCardSelection(selectedCards, session.CurrentState))
        {
            return new ConversationTurnResult
            {
                Success = false,
                NPCResponse = "Invalid card selection for current emotional state"
            };
        }

        // Calculate total comfort change
        var comfortChange = _comfortManager.CalculateComfortChange(selectedCards, session.CurrentState);
        var oldComfort = session.CurrentComfort;
        session.CurrentComfort = _comfortManager.UpdateComfort(session.CurrentComfort, comfortChange);

        // Check for state transition
        EmotionalState newState = session.CurrentState;
        if (_comfortManager.ShouldTriggerStateChange(session.CurrentComfort))
        {
            newState = _stateManager.ProcessStateTransition(session.CurrentState, session.CurrentComfort > 0);
            if (newState != session.CurrentState)
            {
                session.CurrentState = newState;
                session.CurrentComfort = 0; // Reset comfort on state change
            }
        }

        // Process card effects
        var playResult = _deckManager.PlayCards(session, selectedCards);
        
        // Check if goal card was played
        if (session.GoalCard != null && selectedCards.Any(c => c.InstanceId == session.GoalCard.InstanceId))
        {
            session.GoalCardPlayed = true;
            session.GoalUrgencyCounter = null;
        }

        // Generate NPC response
        var npcResponse = _dialogueGenerator.GenerateSpeakResponse(
            session.NPC, 
            session.CurrentState, 
            selectedCards,
            playResult,
            comfortChange);

        // Check goal card urgency
        CheckGoalCardUrgency(session);

        return new ConversationTurnResult
        {
            Success = true,
            NewState = newState,
            NPCResponse = npcResponse,
            ComfortChange = comfortChange,
            OldComfort = oldComfort,
            NewComfort = session.CurrentComfort,
            PatienceRemaining = session.CurrentPatience,
            PlayedCards = selectedCards.ToList(),
            CardPlayResult = playResult
        };
    }

    /// <summary>
    /// Process exchange response (for Commerce conversations)
    /// </summary>
    public ConversationTurnResult ProcessExchangeResponse(ConversationSession session, bool accepted, ExchangeData exchangeData)
    {
        if (session.ConversationType != ConversationType.Commerce)
        {
            throw new InvalidOperationException("Exchange responses only valid for Commerce conversations");
        }

        session.TurnNumber++;

        if (accepted)
        {
            // Exchange accepted - process in ExchangeHandler
            var npcResponse = _dialogueGenerator.GenerateExchangeAcceptedResponse(session.NPC, exchangeData);
            
            return new ConversationTurnResult
            {
                Success = true,
                NPCResponse = npcResponse,
                ExchangeAccepted = true
            };
        }
        else
        {
            // Exchange declined
            var npcResponse = _dialogueGenerator.GenerateExchangeDeclinedResponse(session.NPC);
            
            return new ConversationTurnResult
            {
                Success = true,
                NPCResponse = npcResponse,
                ExchangeAccepted = false
            };
        }
    }

    /// <summary>
    /// Check goal card urgency and handle expiration
    /// </summary>
    private void CheckGoalCardUrgency(ConversationSession session)
    {
        if (session.GoalUrgencyCounter.HasValue)
        {
            session.GoalUrgencyCounter--;
            
            if (session.GoalUrgencyCounter <= 0 && !session.GoalCardPlayed)
            {
                // Goal card expired - remove from hand
                var goalInHand = session.HandCards.FirstOrDefault(c => c.InstanceId == session.GoalCard.InstanceId);
                if (goalInHand != null)
                {
                    session.Hand.RemoveCards(new[] { goalInHand });
                    Console.WriteLine($"[ConversationOrchestrator] Goal card expired and removed from hand");
                }
                session.GoalUrgencyCounter = null;
            }
        }
    }

    /// <summary>
    /// Finalize conversation and calculate outcome
    /// </summary>
    public ConversationOutcome FinalizeConversation(ConversationSession session)
    {
        var outcome = session.CheckThresholds();
        
        // Calculate token rewards based on emotional state and comfort
        if (session.CurrentState == EmotionalState.CONNECTED || session.CurrentState == EmotionalState.EAGER)
        {
            outcome.TokensEarned = Math.Max(1, session.CurrentComfort / 2);
        }
        else if (session.CurrentState == EmotionalState.OPEN)
        {
            outcome.TokensEarned = Math.Max(0, (session.CurrentComfort - 1) / 2);
        }
        
        // Check if goal was achieved
        if (session.GoalCardPlayed)
        {
            outcome.GoalAchieved = true;
            outcome.TokensEarned += 2; // Bonus for completing goal
        }

        return outcome;
    }

    /// <summary>
    /// Check if conversation should end
    /// </summary>
    public bool ShouldEndConversation(ConversationSession session)
    {
        // End if no patience left
        if (session.CurrentPatience <= 0)
            return true;

        // End if hostile
        if (session.CurrentState == EmotionalState.HOSTILE)
            return true;

        // End if deck is empty and hand is empty
        if (!session.Deck.HasCardsAvailable() && session.HandCards.Count == 0)
            return true;

        // End if exchange conversation and turn limit reached
        if (session.ConversationType == ConversationType.Commerce && session.TurnNumber >= 5)
            return true;

        return false;
    }

    /// <summary>
    /// Get available actions for current state
    /// </summary>
    public List<ConversationAction> GetAvailableActions(ConversationSession session)
    {
        var actions = new List<ConversationAction>();

        // Can always listen if deck has cards
        if (session.Deck.HasCardsAvailable() && session.CurrentPatience > 0)
        {
            actions.Add(new ConversationAction
            {
                ActionType = ActionType.Listen,
                IsAvailable = true
            });
        }

        // Can speak if have valid cards
        if (session.HandCards.Any())
        {
            var validCards = session.HandCards.Where(c => 
                _stateManager.CanPlayCard(c, session.CurrentState)).ToList();
                
            if (validCards.Any())
            {
                actions.Add(new ConversationAction
                {
                    ActionType = ActionType.Speak,
                    IsAvailable = true,
                    AvailableCards = validCards
                });
            }
        }

        // Can leave if not in critical state
        if (session.CurrentState != EmotionalState.DESPERATE)
        {
            actions.Add(new ConversationAction
            {
                ActionType = ActionType.None,  // Using None for leave action
                IsAvailable = true
            });
        }

        return actions;
    }

    /// <summary>
    /// Check if letter should be generated
    /// </summary>
    public bool ShouldGenerateLetter(ConversationSession session)
    {
        if (session.LetterGenerated)
            return false;

        // Letters emerge from certain emotional states
        var stateRules = ConversationRules.States[session.CurrentState];
        return stateRules.ChecksGoalDeck && session.CurrentComfort > 10;
    }

    /// <summary>
    /// Create a letter obligation from conversation
    /// </summary>
    public DeliveryObligation CreateLetterObligation(ConversationSession session)
    {
        var comfort = session.CurrentComfort;
        
        // Linear scaling for deadline and payment
        int deadlineMinutes = Math.Max(120, 1440 - (comfort * 45));
        int payment = 5 + (comfort / 2);
        
        // Determine tier
        TierLevel tier = comfort > 20 ? TierLevel.T3 : 
                        comfort > 10 ? TierLevel.T2 : TierLevel.T1;
        
        // Determine weight
        EmotionalWeight weight = deadlineMinutes <= 180 ? EmotionalWeight.CRITICAL :
                                deadlineMinutes <= 360 ? EmotionalWeight.HIGH :
                                deadlineMinutes <= 720 ? EmotionalWeight.MEDIUM :
                                EmotionalWeight.LOW;

        // Find recipient
        var otherNpcs = _gameWorld.NPCs.Where(n => n.ID != session.NPC.ID).ToList();
        var recipient = otherNpcs.Any() ? otherNpcs[new Random().Next(otherNpcs.Count)] : null;

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
            EmotionalWeight = weight,
            Description = $"Letter from {session.NPC.Name} ({comfort} comfort)"
        };
    }

    /// <summary>
    /// Create an urgent letter for crisis situations
    /// </summary>
    public DeliveryObligation CreateUrgentLetter(NPC npc)
    {
        return new DeliveryObligation
        {
            Id = Guid.NewGuid().ToString(),
            SenderId = npc.ID,
            SenderName = npc.Name,
            RecipientId = "elena_family",
            RecipientName = "Elena's Family",
            TokenType = ConnectionType.Trust,
            Stakes = StakeType.SAFETY,
            DeadlineInMinutes = 120,
            Payment = 15,
            Tier = TierLevel.T3,
            EmotionalWeight = EmotionalWeight.CRITICAL,
            Description = $"Urgent letter from {npc.Name} to her family"
        };
    }
}