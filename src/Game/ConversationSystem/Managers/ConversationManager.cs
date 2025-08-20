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
    private readonly NPCRelationshipTracker relationshipTracker;
    private readonly ITimeManager timeManager;
    private ConversationSession currentSession;

    public ConversationManager(
        GameWorld gameWorld,
        ObligationQueueManager queueManager,
        NPCRelationshipTracker relationshipTracker,
        ITimeManager timeManager)
    {
        this.gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        this.queueManager = queueManager ?? throw new ArgumentNullException(nameof(queueManager));
        this.relationshipTracker = relationshipTracker ?? throw new ArgumentNullException(nameof(relationshipTracker));
        this.timeManager = timeManager ?? throw new ArgumentNullException(nameof(timeManager));
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
    /// Start a new conversation with an NPC
    /// </summary>
    public ConversationSession StartConversation(string npcId, List<ConversationCard> observationCards = null)
    {
        if (IsConversationActive)
        {
            throw new InvalidOperationException("A conversation is already active");
        }

        var npc = gameWorld.NPCs.FirstOrDefault(n => n.ID == npcId);
        if (npc == null)
        {
            throw new ArgumentException($"NPC with ID {npcId} not found");
        }

        // Check if NPC can converse
        var initialState = ConversationRules.DetermineInitialState(npc, queueManager);
        if (initialState == EmotionalState.HOSTILE)
        {
            throw new InvalidOperationException($"{npc.Name} is hostile and won't converse");
        }

        currentSession = ConversationSession.StartConversation(
            npc,
            queueManager,
            relationshipTracker,
            observationCards
        );

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

        var relationship = relationshipTracker.GetRelationship(currentSession.NPC.ID);
        var statusTokens = relationship.Status;

        var result = currentSession.ExecuteSpeak(selectedCards, statusTokens);

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
        var relationship = relationshipTracker.GetRelationship(npc.ID);
        
        // Determine letter type based on highest relationship
        var connectionType = ConnectionType.Trust;
        var highestValue = relationship.Trust;
        
        if (relationship.Commerce > highestValue)
        {
            connectionType = ConnectionType.Commerce;
            highestValue = relationship.Commerce;
        }
        if (relationship.Status > highestValue)
        {
            connectionType = ConnectionType.Status;
            highestValue = relationship.Status;
        }
        if (relationship.Shadow > highestValue)
        {
            connectionType = ConnectionType.Shadow;
        }

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
            Payment = 5 + highestValue * 2
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
            var relationship = relationshipTracker.GetRelationship(currentSession.NPC.ID);
            
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

            relationshipTracker.ModifyTokens(
                currentSession.NPC.ID,
                connectionType,
                outcome.TokensEarned
            );
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