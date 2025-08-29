using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Represents an active conversation session with an NPC.
/// Tracks all state for the duration of the conversation.
/// </summary>
public class ConversationSession
{
    /// <summary>
    /// The NPC in conversation
    /// </summary>
    public NPC NPC { get; init; }
    
    /// <summary>
    /// The type of conversation (Standard, Crisis, Exchange, etc.)
    /// </summary>
    public ConversationType ConversationType { get; init; }

    /// <summary>
    /// Current emotional state (changes through play)
    /// </summary>
    public EmotionalState CurrentState { get; set; }

    /// <summary>
    /// Cards currently in hand
    /// </summary>
    public List<ConversationCard> HandCards { get; set; }

    /// <summary>
    /// The NPC's conversation deck
    /// </summary>
    public CardDeck Deck { get; init; }

    /// <summary>
    /// Current patience (turns remaining)
    /// </summary>
    public int CurrentPatience { get; set; }

    /// <summary>
    /// Maximum patience at conversation start
    /// </summary>
    public int MaxPatience { get; init; }

    /// <summary>
    /// Comfort built so far
    /// </summary>
    public int CurrentComfort { get; set; }


    /// <summary>
    /// Turn number
    /// </summary>
    public int TurnNumber { get; set; }

    /// <summary>
    /// Whether a letter has been generated this conversation
    /// </summary>
    public bool LetterGenerated { get; set; }
    
    /// <summary>
    /// Observation cards added at start
    /// </summary>
    public List<ConversationCard> ObservationCards { get; set; }
    
    /// <summary>
    /// Token manager for calculating success bonuses
    /// </summary>
    public TokenMechanicsManager TokenManager { get; init; }
    
    /// <summary>
    /// Current momentum (-3 to +3) that modifies weight limits
    /// </summary>
    public int Momentum { get; set; }
    
    /// <summary>
    /// The goal card that was shuffled into the deck for this conversation
    /// </summary>
    public ConversationCard GoalCard { get; set; }
    
    /// <summary>
    /// Whether the goal card has been drawn into hand
    /// </summary>
    public bool GoalCardDrawn { get; set; }
    
    /// <summary>
    /// Urgency counter - turns remaining to play goal card after drawing
    /// </summary>
    public int? GoalUrgencyCounter { get; set; }
    
    /// <summary>
    /// Whether goal card was successfully played
    /// </summary>
    public bool GoalCardPlayed { get; set; }

    /// <summary>
    /// Initialize a new conversation session
    /// </summary>
    public static ConversationSession StartConversation(
        NPC npc,
        ObligationQueueManager queueManager,
        TokenMechanicsManager tokenManager,
        List<ConversationCard> observationCards,
        ConversationType conversationType,
        PlayerResourceState playerResourceState = null,
        GameWorld gameWorld = null)
    {
        // Determine initial state based on NPC condition
        var initialState = ConversationRules.DetermineInitialState(npc, queueManager);
        
        // Calculate starting patience (POC exact formula)
        var basePatience = GetBasePatience(npc.PersonalityType);
        
        // Apply stamina penalty: Low stamina reduces patience
        // Assume stamina 0-10, where lower stamina = more "hunger"/fatigue
        // Formula: penalty = (10 - stamina) * 3 / 10 (max 3 penalty at 0 stamina)
        var staminaPenalty = 0;
        if (playerResourceState != null)
        {
            var staminaDeficit = Math.Max(0, 10 - playerResourceState.Stamina);
            staminaPenalty = staminaDeficit * 3 / 10;  // 0 stamina = -3 patience
        }
        
        // Apply spot modifiers (TODO: Add when spots are implemented)
        var spotModifier = 0;  // Private: +1, etc.
        
        var totalPatience = Math.Max(3, basePatience - staminaPenalty + spotModifier);

        // Initialize conversation deck if needed
        if (npc.ConversationDeck == null)
        {
            npc.InitializeConversationDeck(new NPCDeckFactory(tokenManager));
        }
        
        // Initialize Goal deck for NPCs that have letters
        if (npc.HasPromiseCards() && (npc.GoalDeck == null || !npc.GoalDeck.Any()))
        {
            // Goal decks should be initialized from letter_decks.json during Phase3
            Console.WriteLine($"[ConversationSession] Warning: {npc.Name} has no Goal deck but should have letters");
        }

        // CRITICAL: Comfort always starts at 5 for new conversations
        var startingComfort = 5;
        
        // POC DECK ARCHITECTURE: Select goal from Goal Deck based on conversation type
        ConversationCard goalCard = null;
        
        // Only select a goal card if this isn't a standard conversation
        if (conversationType != ConversationType.FriendlyChat && conversationType != ConversationType.Commerce)
        {
            goalCard = SelectGoalCardForConversation(npc, conversationType, initialState);
            
            if (goalCard != null)
            {
                // CRITICAL: Create a COPY of the conversation deck for this session
                // This preserves the original deck for future conversations
                var sessionDeck = CreateSessionDeck(npc.ConversationDeck);
                
                // Shuffle the selected goal into the copied deck
                sessionDeck.ShuffleInGoalCard(goalCard);
                if (goalCard is ConversationCard goalConv)
                {
                    Console.WriteLine($"[StartConversation] Shuffled {goalConv.GoalCardType} goal card (ID: {goalConv.Id}) into session deck");
                }
                else if (goalCard.Category == CardCategory.Promise)
                {
                    Console.WriteLine($"[StartConversation] Shuffled promise card (ID: {goalCard.Id}) into session deck");
                }
                
                // Use the session deck, not the original
                npc = CreateNPCWithSessionDeck(npc, sessionDeck);
            }
            else
            {
                Console.WriteLine($"[StartConversation] No suitable goal card found for {conversationType} conversation");
            }
        }
        
        // Draw initial hand (3 cards at comfort 5)
        var handCards = new List<ConversationCard>();
        var drawnCards = npc.ConversationDeck.Draw(3, startingComfort);
        handCards.AddRange(drawnCards);
        
        // Check if we drew the goal card in initial hand
        bool goalCardDrawn = false;
        int? goalUrgencyCounter = null;
        if (drawnCards.Any(c => c is ConversationCard conv && conv.IsGoalCard))
        {
            goalCardDrawn = true;
            goalUrgencyCounter = 3;  // 3 turns to play it
            Console.WriteLine($"[StartConversation] Goal card drawn in initial hand! Must play within 3 turns.");
        }
        
        // Add observation cards if any
        if (observationCards != null && observationCards.Any())
        {
            handCards.AddRange(observationCards);
        }
        
        // Check for letters that can be delivered through this NPC
        if (queueManager != null)
        {
            var activeObligations = queueManager.GetActiveObligations();
            var lettersForDelivery = activeObligations
                .Where(o => o != null && (
                    o.RecipientId == npc.ID || 
                    o.RecipientId == npc.Location || 
                    o.RecipientId == npc.SpotId))
                    // REMOVED: IsLocationMatch - string matching violation
                .ToList();
            
            if (lettersForDelivery.Any())
            {
                Console.WriteLine($"[StartConversation] Found {lettersForDelivery.Count} letters deliverable through {npc.Name}");
                
                // Load letter delivery cards from JSON templates based on letter properties
                var deliveryCards = GetLetterDeliveryCards(lettersForDelivery, gameWorld);
                handCards.AddRange(deliveryCards);
            }
        }

        var session = new ConversationSession
        {
            NPC = npc,
            ConversationType = conversationType,
            CurrentState = initialState,
            HandCards = handCards,
            Deck = npc.ConversationDeck,  // Use the NPC's actual deck
            CurrentPatience = totalPatience,
            MaxPatience = totalPatience,
            CurrentComfort = startingComfort,  // ALWAYS starts at 5
            TurnNumber = 0,
            LetterGenerated = false,
            ObservationCards = observationCards ?? new List<ConversationCard>(),
            Momentum = 0,  // Always starts at 0
            GoalCard = goalCard,
            GoalCardDrawn = goalCardDrawn,
            GoalUrgencyCounter = goalUrgencyCounter,
            GoalCardPlayed = false,
            TokenManager = tokenManager
        };
        
        Console.WriteLine($"[StartConversation] Created session with comfort: {session.CurrentComfort} (expected: {startingComfort})");
        Console.WriteLine($"[StartConversation] Initial state: {initialState}, Patience: {totalPatience}, Hand size: {handCards.Count}");
        
        return session;
    }

    /// <summary>
    /// Start a Quick Exchange conversation (simplified, no emotional states)
    /// </summary>
    public static ConversationSession StartExchange(NPC npc, PlayerResourceState resourceState, TokenMechanicsManager tokenManager, List<string> spotDomainTags = null, ObligationQueueManager queueManager = null, GameWorld gameWorld = null)
    {
        // Exchange deck should already be initialized from Phase3
        // This is just a fallback
        if (npc.ExchangeDeck == null || !npc.ExchangeDeck.Any())
        {
            npc.InitializeExchangeDeck(null);
        }
        
        var handCards = new List<ConversationCard>();
        var exchangeCards = new List<ConversationCard>();
        
        // Get all available exchange cards from the NPC's deck
        if (npc.ExchangeDeck != null && npc.ExchangeDeck.Any())
        {
            // Show ALL exchange options - player chooses one via SPEAK
            exchangeCards = npc.ExchangeDeck.GetAllCards().Take(4).ToList(); // Up to 4 exchange options
            Console.WriteLine($"[DEBUG] Found {npc.ExchangeDeck.Count} exchange cards in {npc.Name}'s deck");
            foreach (var ec in exchangeCards)
            {
                if (ec is ConversationCard exc)
                {
                    Console.WriteLine($"[DEBUG] Exchange card: {exc.Id} - {exc.TemplateId}");
                }
                else if (ec is ConversationCard conv) 
                {
                    Console.WriteLine($"[DEBUG] Exchange card: {conv.Id} - {conv.TemplateId}");
                }
            }
        }
        
        // Exchange cards are already ConversationCards, just add them to hand
        foreach (var exchangeCard in exchangeCards)
        {
            // Exchange cards should already have proper Category and Context
            handCards.Add(exchangeCard);
        }
        
        // Check for letters that can be delivered through this NPC during exchange
        if (queueManager != null)
        {
            var activeObligations = queueManager.GetActiveObligations();
            var lettersForDelivery = activeObligations
                .Where(o => o != null && (
                    o.RecipientId == npc.ID || 
                    o.RecipientId == npc.Location || 
                    o.RecipientId == npc.SpotId))
                    // REMOVED: IsLocationMatch - string matching violation
                .ToList();
            
            if (lettersForDelivery.Any())
            {
                Console.WriteLine($"[StartExchange] Found {lettersForDelivery.Count} letters deliverable through {npc.Name}");
                
                // Load letter delivery cards from JSON templates based on letter properties
                var deliveryCards = GetLetterDeliveryCards(lettersForDelivery, gameWorld);
                exchangeCards.AddRange(deliveryCards);
            }
        }
        
        // No special handling needed - if no exchanges available, hand will be empty
        // Player can use DECLINE button to exit
        
        // Create simplified session for exchanges
        return new ConversationSession
        {
            NPC = npc,
            ConversationType = ConversationType.Commerce,
            CurrentState = EmotionalState.NEUTRAL, // No emotional states in exchanges
            HandCards = handCards, // Contains offer + accept/decline response cards
            Deck = new CardDeck(), // Empty deck - exchanges use ExchangeDeck instead
            CurrentPatience = 1, // Single turn exchange
            MaxPatience = 1,
            CurrentComfort = 0, // No comfort in exchanges
            TurnNumber = 0,
            LetterGenerated = false,
            ObservationCards = new List<ConversationCard>(),
            TokenManager = tokenManager,
            Momentum = 0  // No momentum in exchanges
        };
    }

    /// <summary>
    /// Execute LISTEN action according to exact POC rules
    /// </summary>
    public void ExecuteListen(TokenMechanicsManager tokenManager = null, ObligationQueueManager queueManager = null, GameWorld gameWorld = null)
    {
        TurnNumber++;
        CurrentPatience--;
        
        // Check urgency rule - if goal card was drawn and counter hits 0, conversation fails
        if (GoalCardDrawn && GoalUrgencyCounter.HasValue)
        {
            GoalUrgencyCounter--;
            if (GoalUrgencyCounter <= 0)
            {
                Console.WriteLine($"[ExecuteListen] Goal card urgency expired! Conversation fails.");
                // Force conversation to end with failure
                CurrentPatience = 0;
                return;
            }
        }

        // Remove all opportunity cards EXCEPT observation cards and goal cards
        // Observation cards are Opportunity type but DON'T vanish on Listen - they decay over time instead
        // Goal cards NEVER vanish once drawn (they must be played within 3 turns)
        HandCards.RemoveAll(c => c is ConversationCard conv && 
            conv.Persistence == PersistenceType.Fleeting && 
            !conv.IsObservation && 
            !conv.IsGoalCard);

        // Get state rules
        var rules = ConversationRules.States[CurrentState];

        // Draw new cards with state-based filtering and guaranteed state card
        Console.WriteLine($"[ExecuteListen] Drawing cards for state {CurrentState} (count: {rules.CardsOnListen}, comfort: {CurrentComfort}, momentum: {Momentum})");
        var newCards = DrawCardsForState(CurrentState, rules.CardsOnListen, CurrentComfort);
        Console.WriteLine($"[ExecuteListen] Drew {newCards.Count} filtered cards for {CurrentState} state");
        
        // Check if we drew the goal card and start urgency countdown
        foreach (var card in newCards)
        {
            if (card is ConversationCard conv && conv.IsGoalCard && !GoalCardDrawn)
            {
                GoalCardDrawn = true;
                GoalUrgencyCounter = 3; // 3 turns to play it
                Console.WriteLine($"[ExecuteListen] Drew goal card! Must play within 3 turns.");
            }
        }
        
        HandCards.AddRange(newCards);
        
        // Check for momentum degradation at -3
        if (Momentum <= -3)
        {
            ApplyMomentumDegradation();
        }

        // Check for letters that can be delivered through this NPC during LISTEN
        if (queueManager != null)
        {
            var activeObligations = queueManager.GetActiveObligations();
            
            // Check for letters where:
            // 1. RecipientId matches NPC.ID (direct delivery to specific NPC), OR  
            // 2. RecipientId matches NPC's location (delivery to location through this NPC)
            var lettersForDelivery = activeObligations
                .Where(o => o != null && (
                    o.RecipientId == NPC.ID || 
                    o.RecipientId == NPC.Location || 
                    o.RecipientId == NPC.SpotId ||
                    false)) // REMOVED: IsLocationMatch string matching violation
                .ToList();
            
            if (lettersForDelivery.Any())
            {
                Console.WriteLine($"[ExecuteListen] Found {lettersForDelivery.Count} letters deliverable through {NPC.Name} during LISTEN");
                
                // Create a delivery card for each letter (if not already in hand)
                foreach (var letter in lettersForDelivery)
                {
                    // Check if delivery card for this letter is already in hand
                    var existingDeliveryCard = HandCards.FirstOrDefault(c => 
                        c is ConversationCard conv && conv.CanDeliverLetter && conv.DeliveryObligationId == letter.Id);
                    
                    // Load letter delivery cards from JSON templates if not already in hand
                    if (existingDeliveryCard == null)
                    {
                        var deliveryCard = GetLetterDeliveryCard(letter, gameWorld);
                        if (deliveryCard != null)
                        {
                            HandCards.Add(deliveryCard);
                        }
                    }
                }
            }
        }

        // Check Goal deck if state requires it (OPEN checks trust, CONNECTED checks all)
        if (rules.ChecksGoalDeck && tokenManager != null && NPC.GoalDeck != null && NPC.GoalDeck.Any())
        {
            var npcTokens = tokenManager.GetTokensWithNPC(NPC.ID);
            
            foreach (var card in NPC.GoalDeck)
            {
                if (card.Category == CardCategory.Promise)
                {
                    // Promise cards are eligible for negotiation
                    {
                        // Use the conversation card directly
                        var conversationCard = card;
                        
                        // Calculate success rate with linear token progression (+5% per token)
                        var successRate = card.CalculateSuccessChance(npcTokens);
                    successRate = Math.Clamp(successRate, 10, 95);
                    
                    // Create new card with calculated success rate (init-only property)
                    conversationCard = new ConversationCard
                    {
                        Id = conversationCard.Id,
                        Template = conversationCard.Template,
                        Context = conversationCard.Context,
                        Type = conversationCard.Type,
                        Persistence = conversationCard.Persistence,
                        Weight = conversationCard.Weight,
                        BaseComfort = conversationCard.BaseComfort,
                        Category = conversationCard.Category,
                        Depth = conversationCard.Depth,
                        DisplayName = conversationCard.DisplayName,
                        Description = conversationCard.Description,
                        CanDeliverLetter = conversationCard.CanDeliverLetter,
                        ManipulatesObligations = conversationCard.ManipulatesObligations,
                        SuccessRate = successRate  // Set during initialization
                    };
                    
                        HandCards.Add(conversationCard);
                        Console.WriteLine($"[ExecuteListen] Added eligible letter: {conversationCard.DisplayName ?? conversationCard.Id} (Success: {successRate}%)");
                    }
                }
            }
        }

        // Transition state according to rules
        var previousState = CurrentState;
        CurrentState = rules.ListenTransition;
        
        // Check if conversation should end (HOSTILE state after listen)
        if (rules.ListenEndsConversation)
        {
            HadFinalCrisisTurn = true;  // Mark conversation as ending
            Console.WriteLine($"[ExecuteListen] HOSTILE state - conversation will end");
        }
    }

    /// <summary>
    /// Execute SPEAK action with selected cards
    /// </summary>
    public CardPlayResult ExecuteSpeak(HashSet<ConversationCard> selectedCards)
    {
        TurnNumber++;
        CurrentPatience--;
        
        // Check urgency rule when speaking (decrement counter if goal not played)
        bool playingGoalCard = selectedCards.Any(c => c is ConversationCard conv && conv.IsGoalCard);
        
        if (!playingGoalCard && GoalCardDrawn && GoalUrgencyCounter.HasValue)
        {
            GoalUrgencyCounter--;
            if (GoalUrgencyCounter <= 0)
            {
                Console.WriteLine($"[ExecuteSpeak] Goal card urgency expired! Conversation fails.");
                // Force conversation to end with failure
                CurrentPatience = 0;
                return new CardPlayResult
                {
                    TotalComfort = 0,
                    Results = new List<SingleCardResult>()  // Empty results = failure
                };
            }
        }

        // Get current tokens for success calculation
        var npcTokens = TokenManager?.GetTokensWithNPC(NPC.ID) ?? new Dictionary<ConnectionType, int>();
        
        var manager = new CardSelectionManager(CurrentState, npcTokens);
        foreach (var card in selectedCards)
        {
            // CardSelectionManager expects ConversationCard
            if (card is ConversationCard convCard)
            {
                manager.ToggleCard(convCard);
            }
        }

        var result = manager.PlaySelectedCards();
        
        // Check if a goal card was played
        if (playingGoalCard)
        {
            var goalCard = selectedCards.First(c => c is ConversationCard conv && conv.IsGoalCard) as ConversationCard;
            GoalCardPlayed = true;
            
            Console.WriteLine($"[ExecuteSpeak] Goal card played: {goalCard.DisplayName}");
            
            // Process the goal card effect based on type
            ProcessGoalCardEffect(goalCard, result);
            
            // Goal cards END the conversation immediately
            CurrentPatience = 0;  // Force end
            LetterGenerated = true;  // Mark as successful completion
            
            Console.WriteLine($"[ExecuteSpeak] Goal card played - conversation will end");
        }

        // Process letter negotiations and create delivery obligations
        if (result.LetterNegotiations.Any())
        {
            ProcessLetterNegotiations(result);
        }

        // Apply comfort
        CurrentComfort += result.TotalComfort;
        
        // Apply momentum change based on success/failure
        if (result.Success)
        {
            Momentum = Math.Min(3, Momentum + 1);  // Cap at +3
        }
        else if (result.TotalComfort <= 0)  // Only lose momentum on actual failure
        {
            Momentum = Math.Max(-3, Momentum - 1);  // Floor at -3
        }

        // Apply state change if any
        if (result.NewState.HasValue)
        {
            CurrentState = result.NewState.Value;
        }

        // Remove played cards from hand
        foreach (var card in selectedCards)
        {
            HandCards.Remove(card);
            
            // Handle different persistence types
            if (card is ConversationCard convCard)
            {
                if (convCard.Persistence == PersistenceType.Fleeting)
                {
                    Deck.RemoveCard(card); // Permanently remove
                }
                else
                {
                    Deck.Discard(card); // Will return to deck later
                }
            }
        }

        // Note: No depth advancement needed - comfort directly enables cards

        return result;
    }


    /// <summary>
    /// Check comfort thresholds for rewards
    /// </summary>
    public ConversationOutcome CheckThresholds()
    {
        var outcome = new ConversationOutcome
        {
            TotalComfort = CurrentComfort,
            FinalState = CurrentState,
            TurnsUsed = TurnNumber
        };

        // Letter generation thresholds from design doc:
        // 5+ comfort: Letters become available
        // Higher comfort = better letters (handled in ConversationManager)
        if (CurrentComfort >= 20)
        {
            outcome.TokensEarned = 3;
            outcome.LetterUnlocked = true;
            outcome.PerfectConversation = true;
            outcome.LetterTier = "Critical"; // 2h deadline, 20 coins
        }
        else if (CurrentComfort >= 15)
        {
            outcome.TokensEarned = 2;
            outcome.LetterUnlocked = true;
            outcome.LetterTier = "Urgent"; // 6h deadline, 15 coins
        }
        else if (CurrentComfort >= 10)
        {
            outcome.TokensEarned = 1;
            outcome.LetterUnlocked = true;
            outcome.LetterTier = "Important"; // 12h deadline, 10 coins
        }
        else if (CurrentComfort >= 5)
        {
            outcome.TokensEarned = 1;
            outcome.LetterUnlocked = true;
            outcome.LetterTier = "Simple"; // 24h deadline, 5 coins
        }
        else
        {
            outcome.TokensEarned = -1; // Relationship damage
            outcome.LetterUnlocked = false;
        }

        return outcome;
    }

    /// <summary>
    /// Check if conversation should end
    /// </summary>
    public bool ShouldEnd()
    {
        // Check if patience is gone
        if (CurrentPatience <= 0)
            return true;
            
        // Check if a letter has been generated (successful resolution)
        if (LetterGenerated)
            return true;
            
        // For crisis conversations, we don't end just because cards are empty
        // The player still gets their patience turns to try to resolve the crisis
        if (ConversationType != ConversationType)
        {
            // For normal conversations, check if deck is empty
            if (Deck.IsEmpty)
                return true;
        }
            
        return false;
    }

    /// <summary>
    /// Check if hand is overflowing (forces SPEAK)
    /// </summary>
    public bool IsHandOverflowing()
    {
        return HandCards.Count > 7;
    }
    
    /// <summary>
    /// Draw cards filtered by emotional state according to POC rules
    /// </summary>
    private List<ConversationCard> DrawCardsForState(EmotionalState state, int baseCount, int comfort)
    {
        var drawnCards = new List<ConversationCard>();
        Console.WriteLine($"[DrawCardsForState] State: {state}, BaseCount: {baseCount}, Comfort: {comfort}");
        
        // Special handling for GUARDED - state cards only
        if (state == EmotionalState.GUARDED)
        {
            Console.WriteLine($"[DrawCardsForState] GUARDED - Drawing STATE cards only");
            // GUARDED draws state cards only
            var stateCards = Deck.DrawFilteredByCategory(baseCount, comfort, CardCategory.State);
            return stateCards;
        }
        
        // OVERWHELMED only draws 1 card (no guaranteed state)
        if (state == EmotionalState.OVERWHELMED)
        {
            // Can draw +1 card if positive momentum
            var totalCards = baseCount + (Momentum > 0 ? 1 : 0);
            var cards = Deck.Draw(totalCards, comfort);  // Any type, just limited count
            return cards;
        }
        
        // CONNECTED has special 60/40 distribution
        if (state == EmotionalState.CONNECTED)
        {
            var guaranteeStateCard = baseCount > 1;
            var regularCardCount = guaranteeStateCard ? baseCount - 1 : baseCount;
            
            // 60% chance to draw token cards, 40% any type
            for (int i = 0; i < regularCardCount; i++)
            {
                if (random.Next(100) < 60)
                {
                    // Try to draw a token card
                    var tokenCard = Deck.DrawFilteredByTypes(1, comfort, null, true);
                    if (tokenCard.Any())
                    {
                        drawnCards.AddRange(tokenCard);
                    }
                    else
                    {
                        // Fall back to any card if no token cards available
                        var anyCard = Deck.Draw(1, comfort);
                        drawnCards.AddRange(anyCard);
                    }
                }
                else
                {
                    // Draw any type
                    var anyCard = Deck.Draw(1, comfort);
                    drawnCards.AddRange(anyCard);
                }
            }
            
            // Add guaranteed state card
            if (guaranteeStateCard)
            {
                var stateCard = Deck.DrawFilteredByCategory(1, comfort, CardCategory.State);
                drawnCards.AddRange(stateCard);
            }
            
            return drawnCards;
        }
        
        // Standard state handling
        var allowedTypes = GetAllowedCardTypesForState(state);
        var guaranteeState = ShouldGuaranteeStateCard(state);
        
        // Determine if we should include token cards
        bool includeTokenCards = state == EmotionalState.OPEN || state == EmotionalState.EAGER;
        
        Console.WriteLine($"[DrawCardsForState] Standard state: {state}, AllowedTypes: {string.Join(",", allowedTypes?.Select(t => t.ToString()) ?? new[] { "ALL" })}, IncludeTokenCards: {includeTokenCards}");
        
        // Draw cards based on state filtering rules
        var regularCount = guaranteeState ? baseCount - 1 : baseCount;
        
        // Draw regular cards filtered by state
        if (regularCount > 0)
        {
            Console.WriteLine($"[DrawCardsForState] Drawing {regularCount} regular cards filtered by type");
            var regularCards = Deck.DrawFilteredByTypes(regularCount, comfort, allowedTypes, includeTokenCards);
            drawnCards.AddRange(regularCards);
        }
        
        // Add guaranteed state card if required
        if (guaranteeState)
        {
            Console.WriteLine($"[DrawCardsForState] Adding 1 guaranteed STATE card");
            var stateCard = Deck.DrawFilteredByCategory(1, comfort, CardCategory.State);
            drawnCards.AddRange(stateCard);
        }
        
        Console.WriteLine($"[DrawCardsForState] Total cards drawn: {drawnCards.Count}");
        return drawnCards;
    }
    
    private Random random = new Random();
    
    /// <summary>
    /// Get allowed card types for a given emotional state
    /// </summary>
    private List<CardType> GetAllowedCardTypesForState(EmotionalState state)
    {
        return state switch
        {
            EmotionalState.DESPERATE => new List<CardType> { CardType.Trust },  // Trust and Crisis (Crisis added separately)
            EmotionalState.TENSE => new List<CardType> { CardType.Shadow },     // Shadow cards only
            EmotionalState.NEUTRAL => null,  // All types equally
            EmotionalState.GUARDED => null,  // State cards only (handled specially)
            EmotionalState.OPEN => new List<CardType> { CardType.Trust },       // Trust and Token cards
            EmotionalState.EAGER => new List<CardType> { CardType.Commerce },   // Commerce and Token cards
            EmotionalState.OVERWHELMED => null,  // Any type, but only 1 card
            EmotionalState.CONNECTED => null,    // 60% Token, 40% any (handled specially)
            EmotionalState.HOSTILE => null,      // Crisis cards only (handled specially)
            _ => null
        };
    }
    
    /// <summary>
    /// Check if state guarantees a state card when drawing
    /// </summary>
    private bool ShouldGuaranteeStateCard(EmotionalState state)
    {
        // Most states guarantee 1 state card except OVERWHELMED and HOSTILE
        return state != EmotionalState.OVERWHELMED && state != EmotionalState.HOSTILE;
    }
    
    /// <summary>
    /// Apply momentum degradation at -3 momentum
    /// </summary>
    private void ApplyMomentumDegradation()
    {
        var oldState = CurrentState;
        CurrentState = CurrentState switch
        {
            // Positive states degrade to NEUTRAL
            EmotionalState.OPEN => EmotionalState.NEUTRAL,
            EmotionalState.EAGER => EmotionalState.NEUTRAL,
            EmotionalState.CONNECTED => EmotionalState.NEUTRAL,
            
            // NEUTRAL degrades to GUARDED
            EmotionalState.NEUTRAL => EmotionalState.GUARDED,
            
            // Negative states degrade to HOSTILE
            EmotionalState.GUARDED => EmotionalState.HOSTILE,
            EmotionalState.TENSE => EmotionalState.HOSTILE,
            EmotionalState.DESPERATE => EmotionalState.HOSTILE,
            
            // OVERWHELMED and HOSTILE stay as is
            _ => CurrentState
        };
        
        if (oldState != CurrentState)
        {
            Console.WriteLine($"[Momentum Degradation] State degraded from {oldState} to {CurrentState} due to -3 momentum");
        }
    }
    
    /// <summary>
    /// Get maximum weight allowed for current state with momentum modifiers
    /// </summary>
    public int GetMaxWeightForState()
    {
        var rules = ConversationRules.States[CurrentState];
        var baseWeight = rules.MaxWeight;
        
        // Apply momentum effects based on state
        return CurrentState switch
        {
            // DESPERATE: Each momentum point reduces patience cost by 1
            EmotionalState.DESPERATE => baseWeight,  // Weight stays same, patience cost changes
            
            // TENSE: Positive momentum makes observation cards weight 0
            EmotionalState.TENSE => baseWeight,  // Handled in card weight calculation
            
            // GUARDED: Negative momentum increases card weight by 1 per point
            EmotionalState.GUARDED => Math.Max(1, baseWeight + Math.Min(0, Momentum)),  // Negative momentum reduces allowed weight
            
            // EAGER: Each point adds +5% to token card success (no weight change)
            EmotionalState.EAGER => baseWeight,
            
            // OVERWHELMED: Positive momentum allows drawing 1 additional card (no weight change)
            EmotionalState.OVERWHELMED => baseWeight,
            
            // CONNECTED: Positive momentum allows playing one weight category higher
            EmotionalState.CONNECTED => baseWeight + (Momentum > 0 ? 1 : 0),
            
            // Others: No momentum effect on weight
            _ => baseWeight
        };
    }

    private static int GetBasePatience(PersonalityType personality)
    {
        // EXACT POC patience values by personality
        return personality switch
        {
            PersonalityType.DEVOTED => 12,
            PersonalityType.MERCANTILE => 10,
            PersonalityType.PROUD => 8,
            PersonalityType.CUNNING => 10,
            PersonalityType.STEADFAST => 11,
            _ => 10
        };
    }
    
    private static string GetExchangeName(ConversationCard exchange)
    {
        // Determine exchange type from the exchange data
        var exchangeData = exchange.Context?.ExchangeData;
        if (exchangeData?.Reward?.Count > 0)
        {
            var resource = exchangeData.Reward[0].ResourceType;
            return resource switch
            {
                ResourceType.Food => "Buy Travel Provisions",
                ResourceType.Health => "Purchase Medicine",
                ResourceType.Information => "Information Trade",
                ResourceType.Work => "Help Inventory Stock",
                ResourceType.Favor => "Noble Favor",
                ResourceType.Rest => "Rest at the Inn",
                _ => "Make Exchange"
            };
        }
        
        // Fallback to template ID if available
        return exchange.TemplateId switch
        {
            "food_exchange" => "Buy Travel Provisions",
            "healing_exchange" => "Purchase Medicine",
            "information_exchange" => "Information Trade",
            _ => "Make Exchange"
        };
    }
    
    private static string GetExchangeCostDisplay(ConversationCard exchange)
    {
        if (exchange.Cost == null || !exchange.Cost.Any())
            return "Free";
            
        var costParts = exchange.Cost.Select(c => c.GetDisplayText());
        return string.Join(", ", costParts);
    }
    
    private static string GetExchangeRewardDisplay(ConversationCard exchange)
    {
        if (exchange.Reward == null || !exchange.Reward.Any())
            return "Nothing";
            
        var rewardParts = exchange.Reward.Select(r => r.GetDisplayText());
        return string.Join(", ", rewardParts);
    }
    
    /// <summary>
    /// Process goal card effect based on its type
    /// </summary>
    private void ProcessGoalCardEffect(ConversationCard goalCard, CardPlayResult result)
    {
        // Only ConversationCard has GoalCardType
        if (!(goalCard is ConversationCard convGoalCard) || !convGoalCard.GoalCardType.HasValue)
            return;
            
        switch (convGoalCard.GoalCardType.Value)
        {
            case ConversationType.Letter:
                // Create a letter obligation with terms based on success
                var letterObligation = CreateLetterObligationFromGoal(convGoalCard, result.Success);
                result.LetterNegotiations.Add(new LetterNegotiationResult
                {
                    PromiseCardId = convGoalCard.Id,
                    NegotiationSuccess = result.Success,
                    CreatedObligation = letterObligation
                });
                Console.WriteLine($"[ProcessGoalCardEffect] Letter goal created obligation: {letterObligation.Id}");
                break;
                
            case ConversationType.Promise:
                // TODO: Create meeting/escort obligation
                Console.WriteLine($"[ProcessGoalCardEffect] Promise goal - creating meeting obligation");
                break;
                
            case ConversationType.Resolution:
                // Remove burden cards from the deck
                var burdenCards = Deck.GetCards().Where(c => c is ConversationCard conv && conv.Persistence == PersistenceType.Persistent).ToList();
                foreach (var burden in burdenCards)
                {
                    Deck.RemoveCard(burden);
                }
                Console.WriteLine($"[ProcessGoalCardEffect] Resolution goal - removed {burdenCards.Count} burden cards");
                break;
                
            case ConversationType.Commerce:
                // TODO: Enable special trade
                Console.WriteLine($"[ProcessGoalCardEffect] Commerce goal - special trade unlocked");
                break;
        }
    }
    
    /// <summary>
    /// Create a letter obligation from a goal card
    /// </summary>
    private DeliveryObligation CreateLetterObligationFromGoal(ConversationCard goalCard, bool success)
    {
        // Terms based on success/failure of goal negotiation
        // CRITICAL: Failed negotiations force position 1 (POC Package 4)
        var deadlineHours = success ? 24 : 6;  // Much worse deadline on failure
        var payment = success ? 15 : 5;  // Better payment on success
        
        // CLEAN DESIGN: Check Category, not ID strings
        bool isBurden = goalCard.Category == CardCategory.Burden;
        bool forcesPosition1 = isBurden; // Crisis letters force position 1 on failure
        
        // Get recipient info from card context or use defaults
        string recipientId = "unknown";
        string recipientName = "Unknown Recipient";
        
        return new DeliveryObligation
        {
            Id = $"goal_{NPC.ID}_{DateTime.Now.Ticks}",
            SenderName = NPC.Name,
            SenderId = NPC.ID,
            RecipientName = recipientName,
            RecipientId = recipientId,
            DeadlineInMinutes = deadlineHours * 60,
            Payment = payment,
            TokenType = ConnectionType.Trust,  // Letter goals build trust
            Stakes = success ? StakeType.REPUTATION : StakeType.SAFETY,  // Failed = higher stakes
            EmotionalWeight = success ? EmotionalWeight.MEDIUM : EmotionalWeight.CRITICAL,
            Description = $"Letter delivery from {NPC.Name}",
            Message = success ? "Deliver this important letter" : "URGENT: This letter MUST be delivered immediately!",
            
            // Queue position is handled by automatic displacement for failures
            QueuePosition = (forcesPosition1 && !success) ? 1 : (success ? 3 : 1),
            FinalQueuePosition = (forcesPosition1 && !success) ? 1 : (success ? 3 : 1),
            PositioningReason = success ? LetterPositioningReason.Neutral : LetterPositioningReason.PoorStanding,
            
            // Mark for automatic displacement processing
            IsGenerated = true,
            GenerationReason = $"Goal card negotiation: {(success ? "success" : "failure")}",
        };
    }
    
    /// <summary>
    /// Process letter negotiations and complete them with full obligation details
    /// </summary>
    private void ProcessLetterNegotiations(CardPlayResult result)
    {
        for (int i = 0; i < result.LetterNegotiations.Count; i++)
        {
            var negotiation = result.LetterNegotiations[i];
            
            // Find the source promise card from the NPC's Goal deck
            var sourcePromiseCard = NPC.GoalDeck?.FirstOrDefault(lc => lc.Id == negotiation.PromiseCardId);
            if (sourcePromiseCard == null)
            {
                Console.WriteLine($"[ProcessLetterNegotiations] WARNING: Could not find source promise card {negotiation.PromiseCardId}");
                continue;
            }

            // Create default terms based on negotiation success
            // TODO: Store terms in ConversationCard or elsewhere
            /*
            var terms = new LetterNegotiationTerms
            {
                DeadlineHours = negotiation.NegotiationSuccess ? 24 : 12,
                QueuePosition = negotiation.NegotiationSuccess ? 2 : 3,
                Payment = negotiation.NegotiationSuccess ? 5 : 3,
                ForcesPositionOne = false
            };
            */

            // Create the delivery obligation from the negotiated terms  
            // var obligation = CreateDeliveryObligationFromCard(sourcePromiseCard, terms, NPC);

            // Complete the negotiation result (replace the incomplete one)
            var completedNegotiation = new LetterNegotiationResult
            {
                PromiseCardId = negotiation.PromiseCardId,
                NegotiationSuccess = negotiation.NegotiationSuccess,
                FinalTerms = terms,
                SourcePromiseCard = sourcePromiseCard,
                CreatedObligation = obligation
            };

            // Replace in the list
            result.LetterNegotiations[i] = completedNegotiation;

            Console.WriteLine($"[ProcessLetterNegotiations] Created letter obligation: {obligation.Id} - {terms.DeadlineHours}h deadline, {terms.Payment} coins");
            LetterGenerated = true; // Mark that a letter was generated in this conversation
        }
    }

    /// <summary>
    /// Create a DeliveryObligation from negotiated letter terms
    /// </summary>
    /* 
    private DeliveryObligation CreateDeliveryObligationFromCard(ConversationCard promiseCard, LetterNegotiationTerms terms, NPC sender)
    {
        // Get recipient from promise card (hardcoded based on letter ID for now)
        string recipientId = "unknown";
        string recipientName = "Unknown Recipient";
        
        // CLEAN DESIGN: Get recipient from card context, not ID matching
        if (promiseCard.Context != null)
        {
            // Use Context properties if they have recipient info
            recipientId = promiseCard.Context.CustomData?.GetValueOrDefault("RecipientId", "unknown") ?? "unknown";
            recipientName = promiseCard.Context.CustomData?.GetValueOrDefault("RecipientName", "Unknown Recipient") ?? "Unknown Recipient";
        }

        return new DeliveryObligation
        {
            Id = $"{sender.ID}_{promiseCard.Id}_{DateTime.Now.Ticks}",
            SenderName = sender.Name,
            SenderId = sender.ID,
            RecipientName = recipientName,
            RecipientId = recipientId,
            DeadlineInMinutes = terms.DeadlineHours * 60,
            Payment = terms.Payment,
            TokenType = ConnectionType.Trust, // Default to Trust for letters
            
            // CLEAN DESIGN: Stakes determined by card metadata, not ID strings
            // Stakes should be part of the card's design, not inferred from naming
            Stakes = promiseCard.Context?.CustomData?.ContainsKey("Stakes") == true 
                ? Enum.Parse<StakeType>(promiseCard.Context.CustomData["Stakes"])
                : StakeType.REPUTATION,
            
            EmotionalWeight = terms.DeadlineHours <= 2 ? EmotionalWeight.CRITICAL :
                             terms.DeadlineHours <= 6 ? EmotionalWeight.HIGH :
                             EmotionalWeight.MEDIUM,
                             
            Description = $"{promiseCard.DisplayName ?? "Letter"} for {sender.Name}",
            Message = promiseCard.Description,
            
            // Queue positioning based on terms
            QueuePosition = terms.QueuePosition,
            FinalQueuePosition = terms.ForcesPositionOne ? 1 : terms.QueuePosition,
            PositioningReason = terms.ForcesPositionOne ? LetterPositioningReason.Obligation : LetterPositioningReason.Neutral,
            
            // Generation tracking
            IsGenerated = true,
            GenerationReason = $"Letter negotiation for {sender.Name}"
        };
    }
    */
    
    /// <summary>
    /// Get letter delivery cards from JSON templates based on letter properties
    /// </summary>
    private static List<ConversationCard> GetLetterDeliveryCards(List<DeliveryObligation> letters, GameWorld gameWorld)
    {
        var deliveryCards = new List<ConversationCard>();
        
        foreach (var letter in letters)
        {
            var card = GetLetterDeliveryCard(letter, gameWorld);
            if (card != null)
            {
                deliveryCards.Add(card);
            }
        }
        
        return deliveryCards;
    }
    
    /// <summary>
    /// Get a letter delivery card from JSON templates based on letter properties
    /// </summary>
    private static ConversationCard GetLetterDeliveryCard(DeliveryObligation letter, GameWorld gameWorld)
    {
        // Find letter delivery cards by category
        var deliveryTemplates = gameWorld.AllCardDefinitions.Values
            .Where(card => card.Category == "LETTER_DELIVERY" && card.CanDeliverLetter)
            .ToList();
        
        if (!deliveryTemplates.Any())
        {
            Console.WriteLine("[ConversationSession] No LETTER_DELIVERY cards found in AllCardDefinitions");
            return null;
        }
        
        // Select appropriate template based on letter properties
        ConversationCard template;
        if (letter.IsUrgent)
        {
            template = deliveryTemplates.FirstOrDefault(c => c.DisplayName?.Contains("Urgent") == true) 
                      ?? deliveryTemplates.FirstOrDefault();
        }
        else if (letter.IsSecure)
        {
            template = deliveryTemplates.FirstOrDefault(c => c.DisplayName?.Contains("Discreet") == true) 
                      ?? deliveryTemplates.FirstOrDefault();
        }
        else
        {
            template = deliveryTemplates.FirstOrDefault(c => c.DisplayName?.Contains("Deliver") == true) 
                      ?? deliveryTemplates.FirstOrDefault();
        }
        
        if (template == null)
        {
            Console.WriteLine("[ConversationSession] No suitable LETTER_DELIVERY template found");
            return null;
        }
        
        // Create customized card instance
        var deliveryCard = new ConversationCard
        {
            Id = $"delivery_{letter.Id}_{Guid.NewGuid()}",
            TemplateId = template.TemplateId,
            Mechanics = template.Mechanics,
            Category = template.Category,
            Type = template.Type,
            Persistence = template.Persistence,
            Weight = template.Weight,
            BaseComfort = template.BaseComfort,
            CanDeliverLetter = true,
            DeliveryObligationId = letter.Id,
            ManipulatesObligations = template.ManipulatesObligations,
            Depth = template.Depth,
            SuccessState = template.SuccessState,
            FailureState = template.FailureState,
            SuccessRate = template.SuccessRate,
            DisplayName = $"Deliver Letter to {FormatRecipientName(letter.RecipientId)}",
            Description = $"Deliver {letter.Subject} to {FormatRecipientName(letter.RecipientId)}",
            Context = new CardContext
            {
                Personality = PersonalityType.STEADFAST,
                EmotionalState = EmotionalState.NEUTRAL,
                UrgencyLevel = letter.IsUrgent ? 2 : 0,
                HasDeadline = letter.HasDeadline,
                MinutesUntilDeadline = letter.MinutesUntilDeadline,
                LetterId = letter.Id,
                TargetNpcId = letter.RecipientId
            }
        };
        
        Console.WriteLine($"[ConversationSession] Created delivery card {deliveryCard.Id} for letter {letter.Id}");
        return deliveryCard;
    }
    
    private static string FormatRecipientName(string recipientId)
    {
        if (string.IsNullOrEmpty(recipientId))
            return null;
            
        // Replace underscores with spaces and capitalize each word
        var words = recipientId.Replace("_", " ").Split(' ');
        for (int i = 0; i < words.Length; i++)
        {
            if (words[i].Length > 0)
            {
                words[i] = char.ToUpper(words[i][0]) + (words[i].Length > 1 ? words[i].Substring(1).ToLower() : "");
            }
        }
        return string.Join(" ", words);
    }
    
    /// <summary>
    /// Select the appropriate goal card from NPC's Goal Deck based on conversation type and state
    /// POC Architecture: Goals determine conversation types
    /// </summary>
    private static ConversationCard SelectGoalCardForConversation(NPC npc, ConversationType conversationType, EmotionalState currentState)
    {
        // No goal deck means no goal card
        if (npc.GoalDeck == null || !npc.GoalDeck.Any())
        {
            Console.WriteLine($"[SelectGoalCard] NPC {npc.Name} has no Goal Deck");
            return null;
        }
        
        var allGoals = npc.GoalDeck.GetAllCards();
        Console.WriteLine($"[SelectGoalCard] NPC {npc.Name} has {allGoals.Count} goals in deck");

        // Filter goals based on conversation type
        var eligibleGoals = conversationType switch
        {
            // CLEAN DESIGN: GoalCardType is determined by conversation type the PLAYER CHOSE
            // This is orthogonal to Category which determines what the card DOES
            ConversationType.Promise => allGoals.Where(g =>
                g is ConversationCard conv && conv.GoalCardType == ConversationType.Letter),

            ConversationType.Resolution => allGoals.Where(g =>
                g is ConversationCard conv && conv.GoalCardType == ConversationType.Resolution),

            ConversationType => allGoals.Where(g =>
                g is ConversationCard conv && conv.GoalCardType == ConversationType),

            _ => allGoals // Standard conversations don't filter by type
        };
        
        // Further filter by emotional state requirements
        var stateFilteredGoals = eligibleGoals.Where(g =>
        {
            // Check if goal has state requirements
            if (g.Context?.ValidStates != null && g.Context.ValidStates.Any())
            {
                // Goal must allow current state
                return g.Context.ValidStates.Contains(currentState);
            }
            
            // No state requirements means always eligible
            return true;
        }).ToList();
        
        Console.WriteLine($"[SelectGoalCard] After filtering: {stateFilteredGoals.Count} eligible goals for {conversationType} in {currentState} state");
        
        if (!stateFilteredGoals.Any())
        {
            Console.WriteLine($"[SelectGoalCard] No eligible goals found");
            return null;
        }
        
        // Select based on priority:
        // 1. State-specific goals (match current state)
        // 2. Any eligible goal
        
        
        // Check for state-specific goals
        var stateSpecificGoal = stateFilteredGoals.FirstOrDefault(g =>
            g.Context?.ValidStates != null && 
            g.Context.ValidStates.Contains(currentState));
        if (stateSpecificGoal != null)
        {
            Console.WriteLine($"[SelectGoalCard] Selected state-specific goal: {stateSpecificGoal.Id}");
            return stateSpecificGoal;
        }
        
        // Return first eligible goal
        var selectedGoal = stateFilteredGoals.First();
        Console.WriteLine($"[SelectGoalCard] Selected goal: {selectedGoal.Id}");
        return selectedGoal;
    }
    
    /// <summary>
    /// Create a copy of a deck for use in a single conversation session
    /// This preserves the original deck for future conversations
    /// </summary>
    private static CardDeck CreateSessionDeck(CardDeck originalDeck)
    {
        if (originalDeck == null)
            return new CardDeck();
            
        var sessionDeck = new CardDeck();
        
        // Copy all cards from the original deck
        foreach (var card in originalDeck.GetAllCards())
        {
            sessionDeck.AddCard(card);
        }
        
        // Shuffle the session deck
        sessionDeck.Shuffle();
        
        Console.WriteLine($"[CreateSessionDeck] Created session deck with {sessionDeck.Count} cards");
        return sessionDeck;
    }
    
    /// <summary>
    /// Create a modified NPC reference with a session-specific deck
    /// This allows using a copied deck without modifying the original NPC
    /// </summary>
    private static NPC CreateNPCWithSessionDeck(NPC original, CardDeck sessionDeck)
    {
        // For now, just modify the conversation deck directly
        // In a full implementation, we'd create a wrapper or copy
        original.ConversationDeck = sessionDeck;
        return original;
    }
}