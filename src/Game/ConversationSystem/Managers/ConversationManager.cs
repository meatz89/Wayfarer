using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Main manager for conversation system.
/// Coordinates all conversation mechanics.
/// </summary>
public class ConversationManager
{
    private readonly GameWorld gameWorld;
    private readonly ObligationQueueManager queueManager;
    private readonly ITimeManager timeManager;
    private readonly TokenMechanicsManager tokenManager;
    private ConversationSession currentSession;

    public ConversationManager(
        GameWorld gameWorld,
        ObligationQueueManager queueManager,
        ITimeManager timeManager,
        TokenMechanicsManager tokenManager)
    {
        this.gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        this.queueManager = queueManager ?? throw new ArgumentNullException(nameof(queueManager));
        this.timeManager = timeManager ?? throw new ArgumentNullException(nameof(timeManager));
        this.tokenManager = tokenManager ?? throw new ArgumentNullException(nameof(tokenManager));
    }

    /// <summary>
    /// Current active session (null if no conversation)
    /// </summary>
    public ConversationSession CurrentSession => currentSession;

    /// <summary>
    /// Whether a conversation is active
    /// </summary>
    public bool IsConversationActive => currentSession != null;

    /// <summary>
    /// Start a new conversation with an NPC of a specific type
    /// The type must be explicitly chosen by the player from available options
    /// </summary>
    public ConversationSession StartConversation(string npcId, ConversationType conversationType, List<ConversationCard> observationCards = null)
    {
        if (IsConversationActive)
        {
            // End the existing conversation first
            Console.WriteLine($"[ConversationManager] Ending existing conversation before starting new one");
            EndConversation();
        }

        var npc = gameWorld.NPCs.FirstOrDefault(n => n.ID == npcId);
        if (npc == null)
        {
            throw new ArgumentException($"NPC with ID {npcId} not found");
        }

        // Validate that the requested conversation type is available
        var availableTypes = GetAvailableConversationTypes(npc);
        if (!availableTypes.Contains(conversationType))
        {
            throw new InvalidOperationException($"Conversation type {conversationType} is not available for {npc.Name}");
        }
        
        // For Quick Exchange, use simplified session
        if (conversationType == ConversationType.QuickExchange)
        {
            return StartExchangeConversation(npc);
        }

        // For Crisis conversation, use crisis deck
        if (conversationType == ConversationType.Crisis)
        {
            return StartCrisisConversation(npc, observationCards);
        }

        // Check if NPC can converse (for standard conversations)
        var initialState = ConversationRules.DetermineInitialState(npc, queueManager);
        if (initialState == EmotionalState.HOSTILE)
        {
            throw new InvalidOperationException($"{npc.Name} is hostile and won't converse");
        }

        currentSession = ConversationSession.StartConversation(
            npc,
            queueManager,
            tokenManager,
            observationCards,
            conversationType
        );

        return currentSession;
    }
    
    /// <summary>
    /// Get available conversation types for an NPC
    /// This determines what options appear on the location screen
    /// </summary>
    public List<ConversationType> GetAvailableConversationTypes(NPC npc)
    {
        var available = new List<ConversationType>();
        
        // If NPC has crisis cards, ONLY crisis conversation is available
        if (npc.HasCrisisCards())
        {
            available.Add(ConversationType.Crisis);
            return available; // Other types are LOCKED
        }
        
        // Check for exchange deck
        npc.InitializeExchangeDeck();
        if (npc.ExchangeDeck != null && npc.ExchangeDeck.Any())
        {
            available.Add(ConversationType.QuickExchange);
        }
        
        // Check for standard conversation deck
        if (npc.ConversationDeck != null && npc.ConversationDeck.RemainingCards > 0)
        {
            available.Add(ConversationType.Standard);
            
            // Check if Deep conversation is available (requires relationship level 3+)
            var relationshipLevel = tokenManager.GetRelationshipLevel(npc.ID);
            if (relationshipLevel >= 3)
            {
                available.Add(ConversationType.Deep);
            }
        }
        
        return available;
    }
    
    /// <summary>
    /// Start a quick exchange conversation (simplified, no emotional states)
    /// </summary>
    private ConversationSession StartExchangeConversation(NPC npc)
    {
        // Initialize exchange deck if not already done
        npc.InitializeExchangeDeck();
        
        // Create a simplified session for exchanges
        currentSession = ConversationSession.StartExchange(npc, gameWorld.GetPlayerResourceState(), tokenManager);
        return currentSession;
    }
    
    /// <summary>
    /// Start a crisis conversation
    /// </summary>
    private ConversationSession StartCrisisConversation(NPC npc, List<ConversationCard> observationCards)
    {
        // Crisis conversations use crisis deck exclusively
        currentSession = ConversationSession.StartCrisis(npc, queueManager, tokenManager, observationCards);
        return currentSession;
    }

    /// <summary>
    /// Execute LISTEN action in current conversation
    /// </summary>
    public void ExecuteListen()
    {
        if (!IsConversationActive)
        {
            throw new InvalidOperationException("No active conversation");
        }

        currentSession.ExecuteListen();

        if (currentSession.ShouldEnd())
        {
            EndConversation();
        }
    }

    /// <summary>
    /// Execute SPEAK action with selected cards
    /// </summary>
    public CardPlayResult ExecuteSpeak(HashSet<ConversationCard> selectedCards)
    {
        if (!IsConversationActive)
        {
            throw new InvalidOperationException("No active conversation");
        }

        var result = currentSession.ExecuteSpeak(selectedCards);

        // Handle special card effects
        HandleSpecialCardEffects(selectedCards, result);

        if (currentSession.ShouldEnd())
        {
            EndConversation();
        }

        return result;
    }

    /// <summary>
    /// Handle letter delivery and obligation manipulation
    /// </summary>
    private void HandleSpecialCardEffects(HashSet<ConversationCard> playedCards, CardPlayResult result)
    {
        foreach (var card in playedCards)
        {
            // Handle letter delivery
            if (card.CanDeliverLetter && result.Results.First(r => r.Card == card).Success)
            {
                DeliverLetterThroughConversation();
                result = new CardPlayResult
                {
                    TotalComfort = result.TotalComfort,
                    NewState = result.NewState,
                    Results = result.Results,
                    SetBonus = result.SetBonus,
                    ConnectedBonus = result.ConnectedBonus,
                    EagerBonus = result.EagerBonus,
                    DeliveredLetter = true,
                    ManipulatedObligations = result.ManipulatedObligations
                };
            }

            // Handle obligation manipulation
            if (card.ManipulatesObligations && result.Results.First(r => r.Card == card).Success)
            {
                // Mark that manipulation is available - UI will handle the specifics
                result = new CardPlayResult
                {
                    TotalComfort = result.TotalComfort,
                    NewState = result.NewState,
                    Results = result.Results,
                    SetBonus = result.SetBonus,
                    ConnectedBonus = result.ConnectedBonus,
                    EagerBonus = result.EagerBonus,
                    DeliveredLetter = result.DeliveredLetter,
                    ManipulatedObligations = true
                };
            }
        }
    }

    /// <summary>
    /// Deliver a letter to the current NPC through conversation
    /// </summary>
    private void DeliverLetterThroughConversation()
    {
        var obligations = queueManager.GetActiveObligations();
        var letterToDeliver = obligations.FirstOrDefault(o => 
            o.RecipientId == currentSession.NPC.ID && 
            queueManager.GetQueuePosition(o) == 1);

        if (letterToDeliver != null)
        {
            // Mark as delivered
            queueManager.DeliverObligation(letterToDeliver.Id);
            currentSession.CurrentComfort += 5; // Bonus comfort for delivery
        }
    }

    /// <summary>
    /// Generate a letter request if threshold met
    /// </summary>
    public bool TryGenerateLetter()
    {
        if (!IsConversationActive || currentSession.LetterGenerated)
            return false;

        var outcome = currentSession.CheckThresholds();
        if (outcome.LetterUnlocked)
        {
            // Create a new delivery obligation
            var obligation = CreateLetterObligation(currentSession.NPC);
            queueManager.AddObligation(obligation);
            currentSession.LetterGenerated = true;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Execute obligation manipulation from conversation
    /// </summary>
    public bool ExecuteObligationManipulation(string obligationId, ObligationManipulationType manipulation)
    {
        if (!IsConversationActive)
            return false;
            
        return queueManager.ManipulateObligation(obligationId, manipulation, currentSession.NPC.ID);
    }

    /// <summary>
    /// Create a delivery obligation from conversation
    /// </summary>
    private DeliveryObligation CreateLetterObligation(NPC npc)
    {
        // Determine letter type based on highest relationship
        var connectionType = ConnectionType.Trust;

        // Calculate deadline based on urgency
        var deadlineMinutes = currentSession.CurrentState switch
        {
            EmotionalState.DESPERATE => 180, // 3 hours
            EmotionalState.TENSE => 360, // 6 hours
            _ => 720 // 12 hours
        };

        return new DeliveryObligation
        {
            Id = Guid.NewGuid().ToString(),
            SenderId = npc.ID,
            SenderName = npc.Name,
            RecipientId = DetermineRecipient(npc),
            RecipientName = DetermineRecipientName(npc),
            TokenType = connectionType,
            Stakes = DetermineStakes(connectionType),
            DeadlineInMinutes = deadlineMinutes,
            Payment = 5
        };
    }

    /// <summary>
    /// End the current conversation
    /// </summary>
    public ConversationOutcome EndConversation()
    {
        if (!IsConversationActive)
            return null;

        var outcome = currentSession.CheckThresholds();
        
        // Apply token changes
        if (outcome.TokensEarned != 0)
        {
            // Determine which type to modify based on conversation
            var primaryType = currentSession.HandCards
                .GroupBy(c => c.Type)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault()?.Key ?? CardType.Trust;

            var connectionType = primaryType switch
            {
                CardType.Trust => ConnectionType.Trust,
                CardType.Commerce => ConnectionType.Commerce,
                CardType.Status => ConnectionType.Status,
                CardType.Shadow => ConnectionType.Shadow,
                _ => ConnectionType.Trust
            };
        }

        // Reset deck for next conversation
        currentSession.Deck.ResetForNewConversation();

        var finalOutcome = outcome;
        currentSession = null;
        return finalOutcome;
    }

    /// <summary>
    /// Get available conversation cards for UI display
    /// </summary>
    public List<ConversationCard> GetAvailableCards()
    {
        return currentSession?.HandCards ?? new List<ConversationCard>();
    }

    /// <summary>
    /// Check if a card can be selected
    /// </summary>
    public bool CanSelectCard(ConversationCard card, HashSet<ConversationCard> currentSelection)
    {
        if (!IsConversationActive)
            return false;

        var manager = new CardSelectionManager(currentSession.CurrentState);
        foreach (var selected in currentSelection)
        {
            manager.ToggleCard(selected);
        }
        
        return manager.CanSelectCard(card);
    }

    // Helper methods for letter generation
    private string DetermineRecipient(NPC sender)
    {
        // Find another NPC in the world
        var otherNpcs = gameWorld.NPCs.Where(n => n.ID != sender.ID).ToList();
        if (otherNpcs.Any())
        {
            var index = new Random().Next(otherNpcs.Count);
            return otherNpcs[index].ID;
        }
        return "unknown";
    }

    private string DetermineRecipientName(NPC sender)
    {
        var recipientId = DetermineRecipient(sender);
        var recipient = gameWorld.NPCs.FirstOrDefault(n => n.ID == recipientId);
        return recipient?.Name ?? "Someone";
    }

    private StakeType DetermineStakes(ConnectionType connectionType)
    {
        return connectionType switch
        {
            ConnectionType.Trust => StakeType.SAFETY,
            ConnectionType.Commerce => StakeType.WEALTH,
            ConnectionType.Status => StakeType.REPUTATION,
            ConnectionType.Shadow => StakeType.SECRET,
            _ => StakeType.SAFETY
        };
    }
}