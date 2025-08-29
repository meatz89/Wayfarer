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
    private readonly TimeManager timeManager;
    private readonly TokenMechanicsManager tokenManager;
    private readonly ObservationManager observationManager;
    private readonly MessageSystem messageSystem;
    private readonly TimeBlockAttentionManager timeBlockAttentionManager;
    private ConversationSession currentSession;

    public ConversationManager(
        GameWorld gameWorld,
        ObligationQueueManager queueManager,
        TimeManager timeManager,
        TokenMechanicsManager tokenManager,
        ObservationManager observationManager,
        MessageSystem messageSystem,
        TimeBlockAttentionManager timeBlockAttentionManager)
    {
        this.gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        this.queueManager = queueManager ?? throw new ArgumentNullException(nameof(queueManager));
        this.timeManager = timeManager ?? throw new ArgumentNullException(nameof(timeManager));
        this.tokenManager = tokenManager ?? throw new ArgumentNullException(nameof(tokenManager));
        this.observationManager = observationManager ?? throw new ArgumentNullException(nameof(observationManager));
        this.messageSystem = messageSystem ?? throw new ArgumentNullException(nameof(messageSystem));
        this.timeBlockAttentionManager = timeBlockAttentionManager ?? throw new ArgumentNullException(nameof(timeBlockAttentionManager));
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
        if (conversationType == ConversationType.Commerce)
        {
            return StartExchangeConversation(npc);
        }

        // For Letter Offer conversation, mix in letter cards
        if (conversationType == ConversationType.Promise)
        {
            return StartLetterOfferConversation(npc, observationCards);
        }
        
        // For Make Amends conversation, focus on burden resolution
        if (conversationType == ConversationType.Resolution)
        {
            return StartMakeAmendsConversation(npc, observationCards);
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
            conversationType,
            gameWorld.GetPlayerResourceState(),  // Pass player resource state for hunger penalty
            gameWorld  // Pass GameWorld for card templates
        );

        return currentSession;
    }
    
    /// <summary>
    /// Get available conversation types for an NPC
    /// This determines what options appear on the location screen
    /// Types are determined dynamically based on deck contents
    /// </summary>
    public List<ConversationType> GetAvailableConversationTypes(NPC npc)
    {
        var available = new List<ConversationType>();
        
        // Check for exchange deck - depends on personality and location
        // Get current spot's domain tags for hospitality checks
        // Initialize exchange deck from GameWorld if available
        if (gameWorld.NPCExchangeDecks.TryGetValue(npc.ID.ToLower(), out var exchangeCards))
        {
            npc.InitializeExchangeDeck(exchangeCards);
        }
        else
        {
            npc.InitializeExchangeDeck(null);
        }
        if (npc.HasExchangeCards())
        {
            available.Add(ConversationType.Commerce);
        }
        
        // Check for letter cards → Letter Offer conversation
        if (npc.HasPromiseCards())
        {
            available.Add(ConversationType.Promise);
        }
        
        // Check for burden cards → Make Amends conversation
        // Requires at least 2 burden cards to enable this type
        if (npc.CountBurdenCards() >= 2)
        {
            available.Add(ConversationType.Resolution);
        }
        
        // Standard conversation always available if conversation deck exists
        if (npc.ConversationDeck != null && npc.ConversationDeck.RemainingCards > 0)
        {
            available.Add(ConversationType.FriendlyChat);
        }
        
        return available;
    }
    
    /// <summary>
    /// Start a quick exchange conversation (simplified, no emotional states)
    /// </summary>
    private ConversationSession StartExchangeConversation(NPC npc)
    {
        // Initialize exchange deck from GameWorld if available  
        if (gameWorld.NPCExchangeDecks.TryGetValue(npc.ID.ToLower(), out var exchangeCards))
        {
            npc.InitializeExchangeDeck(exchangeCards);
        }
        else
        {
            npc.InitializeExchangeDeck(null);
        }
        
        // Create a simplified session for exchanges
        var spotDomainTags = gameWorld.GetCurrentSpotDomainTags();
        currentSession = ConversationSession.StartExchange(npc, gameWorld.GetPlayerResourceState(), tokenManager, spotDomainTags, queueManager, gameWorld);
        return currentSession;
    }
    
    /// <summary>
    /// Start a letter offer conversation
    /// </summary>
    private ConversationSession StartLetterOfferConversation(NPC npc, List<ConversationCard> observationCards)
    {
        // Letter Offer conversations mix letter cards into conversation deck
        // and use a Letter goal card
        var initialState = ConversationRules.DetermineInitialState(npc, queueManager);
        if (initialState == EmotionalState.HOSTILE)
        {
            throw new InvalidOperationException($"{npc.Name} is hostile and won't discuss letters");
        }
        
        currentSession = ConversationSession.StartConversation(
            npc,
            queueManager,
            tokenManager,
            observationCards,
            ConversationType.Promise,
            gameWorld.GetPlayerResourceState()
        );
        
        return currentSession;
    }
    
    /// <summary>
    /// Start a make amends conversation
    /// </summary>
    private ConversationSession StartMakeAmendsConversation(NPC npc, List<ConversationCard> observationCards)
    {
        // Make Amends conversations focus on burden cards
        // and use a Resolution goal card
        var initialState = ConversationRules.DetermineInitialState(npc, queueManager);
        if (initialState == EmotionalState.HOSTILE)
        {
            throw new InvalidOperationException($"{npc.Name} is hostile and won't discuss problems");
        }
        
        currentSession = ConversationSession.StartConversation(
            npc,
            queueManager,
            tokenManager,
            observationCards,
            ConversationType.Resolution,
            gameWorld.GetPlayerResourceState()
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

        currentSession.ExecuteListen(tokenManager, queueManager, gameWorld);

        if (currentSession.ShouldEnd())
        {
            EndConversation();
        }
    }

    /// <summary>
    /// Execute SPEAK action with selected cards
    /// </summary>
    public async Task<CardPlayResult> ExecuteSpeak(HashSet<ConversationCard> selectedCards)
    {
        if (!IsConversationActive)
        {
            throw new InvalidOperationException("No active conversation");
        }

        var result = currentSession.ExecuteSpeak(selectedCards);

        // Handle special card effects
        await HandleSpecialCardEffectsAsync(selectedCards, result);

        // Remove observation cards from ObservationManager after playing (they're OneShot)
        foreach (var card in selectedCards)
        {
            if (card.IsObservation && card.Persistence == PersistenceType.Fleeting)
            {
                observationManager.RemoveObservationCardByConversationId(card.Id);
                Console.WriteLine($"[ConversationManager] Removed observation card {card.Id} from ObservationManager");
            }
        }

        if (currentSession.ShouldEnd())
        {
            EndConversation();
        }

        return result;
    }

    /// <summary>
    /// Handle letter delivery, obligation manipulation, and exchanges
    /// </summary>
    private async Task HandleSpecialCardEffectsAsync(HashSet<ConversationCard> playedCards, CardPlayResult result)
    {
        foreach (var card in playedCards)
        {
            // Handle exchange cards
            if (card.Context?.ExchangeData != null)
            {
                // Execute the actual exchange directly
                Console.WriteLine($"[EXCHANGE DEBUG] About to execute exchange for card {card.Id}");
                Console.WriteLine($"[EXCHANGE DEBUG] card.Context null? {card.Context == null}");
                Console.WriteLine($"[EXCHANGE DEBUG] card.Context.ExchangeData null? {card.Context?.ExchangeData == null}");
                
                var exchangeSuccess = await ExecuteExchangeAsync(card.Context.ExchangeData);
                if (!exchangeSuccess)
                {
                    Console.WriteLine($"[ConversationManager] Failed to execute exchange {card.Id}");
                    messageSystem.AddSystemMessage("Exchange failed - insufficient resources", SystemMessageTypes.Warning);
                }
                else
                {
                    Console.WriteLine($"[ConversationManager] Successfully executed exchange {card.Id}");
                }
            }
            
            // Handle special Crisis letter that generates letter on success (e.g., Elena's Desperate Promise)
            if (card.Context?.GeneratesLetterOnSuccess == true && result.Results.First(r => r.Card == card).Success)
            {
                Console.WriteLine($"[ConversationManager] Crisis letter {card.Id} succeeded - generating letter!");
                
                // Create Elena's urgent letter
                var urgentLetter = new DeliveryObligation
                {
                    Id = Guid.NewGuid().ToString(),
                    SenderId = currentSession.NPC.ID,
                    SenderName = currentSession.NPC.Name,
                    RecipientId = "elena_family", // Elena's family
                    RecipientName = "Elena's Family",
                    TokenType = ConnectionType.Trust,
                    Stakes = StakeType.SAFETY,
                    DeadlineInMinutes = 120, // 2 hour urgent deadline
                    Payment = 15, // Good payment for urgent delivery
                    Tier = TierLevel.T3,
                    EmotionalWeight = EmotionalWeight.CRITICAL,
                    Description = $"Urgent letter from {currentSession.NPC.Name} to her family"
                };
                
                // Add to queue
                queueManager.AddObligation(urgentLetter);
                messageSystem.AddSystemMessage($"{currentSession.NPC.Name} desperately hands you a letter for her family!", SystemMessageTypes.Success);
                
                // Also grant bonus comfort for helping
                currentSession.CurrentComfort += 5;
                
                // Mark letter as generated
                currentSession.LetterGenerated = true;
            }
            
            // Handle letter delivery
            if (card.CanDeliverLetter && result.Results.First(r => r.Card == card).Success)
            {
                // Use the specific delivery obligation ID from the card
                bool deliverySuccess = false;
                if (!string.IsNullOrEmpty(card.DeliveryObligationId))
                {
                    // Get the obligation details BEFORE delivering (it will be removed from queue)
                    var obligations = queueManager.GetActiveObligations();
                    var deliveredObligation = obligations.FirstOrDefault(o => o.Id == card.DeliveryObligationId);
                    
                    if (deliveredObligation != null)
                    {
                        // Now deliver it
                        deliverySuccess = queueManager.DeliverObligation(card.DeliveryObligationId);
                        
                        if (deliverySuccess)
                        {
                            // Grant payment to player
                            gameWorld.GetPlayer().Coins += deliveredObligation.Payment;
                            
                            // Award tokens based on letter importance
                            int tokenReward = deliveredObligation.EmotionalWeight switch
                            {
                                EmotionalWeight.CRITICAL => 3,
                                EmotionalWeight.HIGH => 2,
                                EmotionalWeight.MEDIUM => 1,
                                _ => 1
                            };
                            
                            // Award tokens with the recipient NPC
                            tokenManager.AddTokensToNPC(deliveredObligation.TokenType, tokenReward, currentSession.NPC.ID);
                            
                            // Add system messages
                            messageSystem.AddSystemMessage(
                                $"📬 Successfully delivered {deliveredObligation.SenderName}'s letter to {currentSession.NPC.Name}!",
                                SystemMessageTypes.Success
                            );
                            messageSystem.AddSystemMessage(
                                $"💰 Earned {deliveredObligation.Payment} coins for the delivery",
                                SystemMessageTypes.Success
                            );
                            messageSystem.AddSystemMessage(
                                $"✨ Gained {tokenReward} {deliveredObligation.TokenType} tokens with {currentSession.NPC.Name}",
                                SystemMessageTypes.Success
                            );
                            
                            // Add bonus comfort for successful delivery
                            currentSession.CurrentComfort += 5;
                        }
                    }
                }
                else
                {
                    // Fallback to old method if no specific ID (shouldn't happen)
                    DeliverLetterThroughConversation();
                    deliverySuccess = true;
                }
                
                result = new CardPlayResult
                {
                    TotalComfort = result.TotalComfort,
                    NewState = result.NewState,
                    Results = result.Results,
                    SetBonus = result.SetBonus,
                    ConnectedBonus = result.ConnectedBonus,
                    EagerBonus = result.EagerBonus,
                    DeliveredLetter = deliverySuccess,
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
    /// Generate a letter request if emotional state allows it
    /// </summary>
    public bool TryGenerateLetter()
    {
        if (!IsConversationActive || currentSession.LetterGenerated)
            return false;

        // Check if current emotional state allows letter generation
        // Letters emerge from emotional state, not comfort thresholds
        var stateRules = ConversationRules.States[currentSession.CurrentState];
        if (stateRules.ChecksGoalDeck)
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

        // Determine letter tier and properties based on linear calculation
        // No artificial breakpoints - use linear scaling
        var comfort = currentSession.CurrentComfort;
        
        // Linear scaling: tighter deadline and higher pay with more comfort
        // Base: 24 hours at 0 comfort, down to 2 hours at 30+ comfort
        // Formula: 1440 - (comfort * 45), clamped to [120, 1440]
        int deadlineMinutes = Math.Max(120, 1440 - (comfort * 45));
        
        // Linear payment: 5 coins + 0.5 per comfort point
        // Results in 5 coins at 0 comfort, up to 25 coins at 40 comfort
        int payment = 5 + (comfort / 2);
        
        // Determine tier based on actual importance (using comfort as proxy)
        // Linear progression: T1 for 0-10, T2 for 11-20, T3 for 21+
        TierLevel tier;
        if (comfort > 20)
            tier = TierLevel.T3;
        else if (comfort > 10) 
            tier = TierLevel.T2;
        else
            tier = TierLevel.T1;
        
        // Emotional weight based on stakes and urgency
        // Use deadline as proxy for urgency
        EmotionalWeight weight;
        if (deadlineMinutes <= 180) // 3 hours or less
            weight = EmotionalWeight.CRITICAL;
        else if (deadlineMinutes <= 360) // 6 hours or less
            weight = EmotionalWeight.HIGH;
        else if (deadlineMinutes <= 720) // 12 hours or less
            weight = EmotionalWeight.MEDIUM;
        else
            weight = EmotionalWeight.LOW;

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
            Payment = payment,
            Tier = tier,
            EmotionalWeight = weight,
            Description = $"Letter from {npc.Name} ({comfort} comfort)"
        };
    }

    /// <summary>
    /// Execute an exchange card's resource trade
    /// </summary>
    private async Task<bool> ExecuteExchangeAsync(ExchangeData exchange)
    {
        Console.WriteLine($"[EXCHANGE DEBUG] ExecuteExchangeAsync called with exchange: {exchange?.ExchangeName ?? "NULL"}");
        
        if (exchange == null)
        {
            Console.WriteLine("[ExecuteExchangeAsync] Exchange is null");
            return false;
        }
        
        // Get player and resources
        var player = gameWorld.GetPlayer();
        var playerResources = gameWorld.GetPlayerResourceState();
        
        // Check if player can afford
        var currentTimeBlock = timeManager.GetCurrentTimeBlock();
        var currentAttention = timeBlockAttentionManager.GetCurrentAttention(currentTimeBlock);
        if (!exchange.CanAfford(playerResources, tokenManager, currentAttention))
        {
            Console.WriteLine("[ExecuteExchangeAsync] Player cannot afford exchange");
            messageSystem.AddSystemMessage("You don't have enough resources for this exchange", SystemMessageTypes.Warning);
            return false;
        }
        
        // Apply costs
        foreach (var cost in exchange.Cost)
        {
            switch (cost.ResourceType)
            {
                case ResourceType.Coins:
                    player.Coins -= cost.Amount;
                    break;
                case ResourceType.Health:
                    player.Health -= cost.Amount;
                    break;
                case ResourceType.Attention:
                    // Attention is managed by TimeBlockAttentionManager
                    var costTimeBlock = timeManager.GetCurrentTimeBlock();
                    var attentionMgr = timeBlockAttentionManager.GetCurrentAttention(costTimeBlock);
                    if (!attentionMgr.TrySpend(cost.Amount))
                    {
                        Console.WriteLine($"[ExecuteExchangeAsync] Failed to spend {cost.Amount} attention");
                        return false;
                    }
                    break;
                case ResourceType.TrustToken:
                    tokenManager.SpendTokens(ConnectionType.Trust, cost.Amount);
                    break;
                case ResourceType.CommerceToken:
                    tokenManager.SpendTokens(ConnectionType.Commerce, cost.Amount);
                    break;
                case ResourceType.StatusToken:
                    tokenManager.SpendTokens(ConnectionType.Status, cost.Amount);
                    break;
                case ResourceType.ShadowToken:
                    tokenManager.SpendTokens(ConnectionType.Shadow, cost.Amount);
                    break;
            }
        }
        
        // Apply rewards
        foreach (var reward in exchange.Reward)
        {
            switch (reward.ResourceType)
            {
                case ResourceType.Coins:
                    player.Coins += reward.Amount;
                    break;
                case ResourceType.Health:
                    if (reward.IsAbsolute)
                        player.Health = reward.Amount;
                    else
                        player.Health += reward.Amount;
                    break;
                case ResourceType.Hunger:
                    // Hunger maps to Food (0 = not hungry, 100 = very hungry)
                    // So setting Hunger to 0 means setting Food to max
                    if (reward.IsAbsolute)
                        player.Food = reward.Amount == 0 ? 100 : (100 - reward.Amount);
                    else
                        player.Food = Math.Max(0, Math.Min(100, player.Food - reward.Amount));
                    break;
                case ResourceType.TrustToken:
                    tokenManager.AddTokens(ConnectionType.Trust, reward.Amount);
                    break;
                case ResourceType.CommerceToken:
                    tokenManager.AddTokens(ConnectionType.Commerce, reward.Amount);
                    break;
                case ResourceType.StatusToken:
                    tokenManager.AddTokens(ConnectionType.Status, reward.Amount);
                    break;
                case ResourceType.ShadowToken:
                    tokenManager.AddTokens(ConnectionType.Shadow, reward.Amount);
                    break;
                case ResourceType.Attention:
                    // Attention rewards add to current time block's attention
                    var rewardTimeBlock = timeManager.GetCurrentTimeBlock();
                    var rewardAttentionMgr = timeBlockAttentionManager.GetCurrentAttention(rewardTimeBlock);
                    if (reward.IsAbsolute)
                    {
                        // Set attention to specific value (for lodging/rest)
                        rewardAttentionMgr.SetAttention(reward.Amount);
                    }
                    else
                    {
                        // Add to current attention
                        rewardAttentionMgr.AddAttention(reward.Amount);
                    }
                    break;
            }
        }
        
        // Generate narrative message
        var exchangeName = exchange.Template switch
        {
            CardTemplateType.Exchange => "completed an exchange",
            CardTemplateType.SimpleExchange => "made a quick trade",
            _ => "completed an exchange"
        };
        messageSystem.AddSystemMessage($"You {exchangeName} with {currentSession.NPC.Name}", SystemMessageTypes.Success);
        
        // Log for debugging
        Console.WriteLine($"[ExecuteExchangeAsync] Completed exchange {exchange.ExchangeName} with {currentSession.NPC.Name}");
        
        // Exchange cards that cost attention should advance time
        if (exchange.Cost.Any(c => c.ResourceType == ResourceType.Attention && c.Amount >= 3))
        {
            // This is a work exchange that advances time
            timeManager.AdvanceTime(1);
            messageSystem.AddSystemMessage("Time passes as you work...", SystemMessageTypes.Info);
        }
        
        return await Task.FromResult(true);
    }
    
    /// <summary>
    /// End the current conversation
    /// </summary>
    public ConversationOutcome EndConversation()
    {
        if (!IsConversationActive)
            return null;

        var outcome = currentSession.CheckThresholds();
        
        // Generate letter if emotional state allows it
        // Letters emerge from emotional state, not comfort thresholds
        TryGenerateLetter();
        
        // Apply token changes
        if (outcome.TokensEarned != 0)
        {
            // Determine which type to modify based on conversation
            var primaryType = currentSession.HandCards
                .OfType<ConversationCard>()
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

            // Award the tokens to the NPC
            tokenManager.AddTokensToNPC(connectionType, outcome.TokensEarned, currentSession.NPC.ID);
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