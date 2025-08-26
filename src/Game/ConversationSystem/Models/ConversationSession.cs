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
    /// Whether we've had the final crisis turn in HOSTILE state
    /// </summary>
    public bool HadFinalCrisisTurn { get; set; }

    /// <summary>
    /// Observation cards added at start
    /// </summary>
    public List<ConversationCard> ObservationCards { get; set; }

    /// <summary>
    /// Initialize a new conversation session
    /// </summary>
    public static ConversationSession StartConversation(
        NPC npc,
        ObligationQueueManager queueManager,
        TokenMechanicsManager tokenManager,
        List<ConversationCard> observationCards,
        ConversationType conversationType,
        PlayerResourceState playerResourceState = null)
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
        
        // Initialize letter deck for Elena (POC character)
        if (npc.ID == "elena_merchant" && (npc.LetterDeck == null || !npc.LetterDeck.Any()))
        {
            npc.LetterDeck = LetterCardFactory.CreateElenaLetterDeck(npc.ID);
        }
        
        // Initialize crisis deck if needed (usually empty)
        if (npc.CrisisDeck == null)
        {
            npc.InitializeCrisisDeck();
        }

        // CRITICAL: Comfort always starts at 5 for new conversations
        var startingComfort = 5;
        
        // Draw initial hand (3 cards at comfort 5)
        var handCards = new List<ConversationCard>();
        handCards.AddRange(npc.ConversationDeck.Draw(3, startingComfort));
        
        // Add observation cards if any
        if (observationCards != null && observationCards.Any())
        {
            handCards.AddRange(observationCards);
        }

        var session = new ConversationSession
        {
            NPC = npc,
            CurrentState = initialState,
            HandCards = handCards,
            Deck = npc.ConversationDeck,  // Use the NPC's actual deck
            CurrentPatience = totalPatience,
            MaxPatience = totalPatience,
            CurrentComfort = startingComfort,  // ALWAYS starts at 5
            TurnNumber = 0,
            LetterGenerated = false,
            ObservationCards = observationCards ?? new List<ConversationCard>()
        };
        
        Console.WriteLine($"[StartConversation] Created session with comfort: {session.CurrentComfort} (expected: {startingComfort})");
        Console.WriteLine($"[StartConversation] Initial state: {initialState}, Patience: {totalPatience}, Hand size: {handCards.Count}");
        
        return session;
    }

    /// <summary>
    /// Start a Quick Exchange conversation (simplified, no emotional states)
    /// </summary>
    public static ConversationSession StartExchange(NPC npc, PlayerResourceState resourceState, TokenMechanicsManager tokenManager)
    {
        // Initialize exchange deck if not already done
        npc.InitializeExchangeDeck();
        
        // Draw 4 exchange cards from the NPC's exchange deck (as per POC spec)
        var handCards = new List<ConversationCard>();
        var exchangeCards = new List<ExchangeCard>();
        
        // Get all available exchange cards from the NPC's deck
        if (npc.ExchangeDeck != null && npc.ExchangeDeck.Any())
        {
            // Take up to 4 cards from the deck
            exchangeCards = npc.ExchangeDeck.Take(4).ToList();
            Console.WriteLine($"[DEBUG] Found {npc.ExchangeDeck.Count} exchange cards in {npc.Name}'s deck");
            foreach (var ec in exchangeCards)
            {
                Console.WriteLine($"[DEBUG] Exchange: {ec.TemplateType} - Cost: {string.Join(", ", ec.Cost.Select(c => c.GetDisplayText()))} -> Reward: {string.Join(", ", ec.Reward.Select(r => r.GetDisplayText()))}");
            }
        }
        
        // Create cards for each exchange option
        foreach (var exchangeCard in exchangeCards)
        {
            // Create an exchange card for each option
            var card = new ConversationCard
            {
                Id = exchangeCard.Id,
                Template = CardTemplateType.Exchange,
                Context = new CardContext
                {
                    NPCName = npc.Name,
                    NPCPersonality = exchangeCard.NPCPersonality,
                    ExchangeData = exchangeCard, // Store the exchange card for execution
                    // Add exchange display info
                    ExchangeName = GetExchangeName(exchangeCard),
                    ExchangeCost = GetExchangeCostDisplay(exchangeCard),
                    ExchangeReward = GetExchangeRewardDisplay(exchangeCard)
                },
                Type = CardType.Commerce,
                Persistence = PersistenceType.Persistent,
                Weight = 0, // Exchanges have no weight cost
                BaseComfort = 0,
                Category = CardCategory.EXCHANGE, // Use EXCHANGE category for proper styling
                IsObservation = false,
                CanDeliverLetter = false,
                ManipulatesObligations = false
            };
            
            handCards.Add(card);
        }
        
        // Always add a DECLINE CARD - Pass on the offer
        if (exchangeCards.Any())
        {
            var declineCard = new ConversationCard
            {
                Id = "decline_exchange",
                Template = CardTemplateType.Exchange, // Use Exchange template for consistency
                Context = new CardContext
                {
                    NPCName = npc.Name,
                    NPCPersonality = npc.PersonalityType,
                    ExchangeName = "Decline all offers",
                    ExchangeCost = "Nothing",
                    ExchangeReward = "End exchange"
                },
                Type = CardType.Commerce,
                Persistence = PersistenceType.Persistent,
                Weight = 0, // No weight cost
                BaseComfort = 0,
                Category = CardCategory.EXCHANGE,
                IsObservation = false,
                CanDeliverLetter = false,
                ManipulatesObligations = false
            };
            
            handCards.Add(declineCard);
        }
        
        // Create simplified session for exchanges
        return new ConversationSession
        {
            NPC = npc,
            CurrentState = EmotionalState.NEUTRAL, // No emotional states in exchanges
            HandCards = handCards, // Contains both accept and decline cards
            Deck = new CardDeck(), // Empty deck - exchanges use ExchangeDeck instead
            CurrentPatience = 1, // Single turn exchange
            MaxPatience = 1,
            CurrentComfort = 0, // No comfort in exchanges
            TurnNumber = 0,
            LetterGenerated = false,
            ObservationCards = new List<ConversationCard>()
        };
    }

    /// <summary>
    /// Start a Crisis conversation (forced resolution)
    /// </summary>
    public static ConversationSession StartCrisis(NPC npc, ObligationQueueManager queueManager,
        TokenMechanicsManager tokenManager, List<ConversationCard> observationCards)
    {
        // Crisis conversations always start in DESPERATE state
        var initialState = EmotionalState.DESPERATE;
        
        // Crisis conversations have limited patience
        var basePatience = 3; // Fixed 3 patience for crisis
        
        // Use existing crisis deck or fail if none exists
        if (npc.CrisisDeck == null || npc.CrisisDeck.RemainingCards == 0)
        {
            Console.WriteLine($"[ERROR] StartCrisis: {npc.Name} has no crisis deck or no crisis cards!");
            // Initialize an empty deck to prevent null reference, but conversation will end immediately
            npc.CrisisDeck = new CardDeck();
        }
        
        var deck = npc.CrisisDeck;
        
        // Draw initial hand - for crisis, we should draw ALL crisis cards available
        var handCards = new List<ConversationCard>();
        
        // Draw all available crisis cards (typically just 1-2)
        var availableCount = Math.Min(deck.RemainingCards, 5); // Cap at 5 to prevent overflow
        if (availableCount > 0)
        {
            handCards.AddRange(deck.Draw(availableCount, 0)); // Draw crisis cards (usually depth 0)
            Console.WriteLine($"[StartCrisis] Drew {availableCount} crisis cards for {npc.Name}");
        }
        else
        {
            Console.WriteLine($"[WARNING] StartCrisis: No cards available in {npc.Name}'s crisis deck!");
        }
        
        // Add observation cards if any
        if (observationCards != null && observationCards.Any())
        {
            handCards.AddRange(observationCards);
            Console.WriteLine($"[StartCrisis] Added {observationCards.Count} observation cards");
        }

        return new ConversationSession
        {
            NPC = npc,
            CurrentState = initialState,
            HandCards = handCards,
            Deck = deck,
            CurrentPatience = basePatience,
            MaxPatience = basePatience,
            CurrentComfort = 0,
            TurnNumber = 0,
            LetterGenerated = false,
            ObservationCards = observationCards ?? new List<ConversationCard>()
        };
    }

    /// <summary>
    /// Execute LISTEN action according to exact POC rules
    /// </summary>
    public void ExecuteListen(TokenMechanicsManager tokenManager = null)
    {
        TurnNumber++;
        CurrentPatience--;

        // Remove all opportunity cards
        HandCards.RemoveAll(c => c.Persistence == PersistenceType.Opportunity);

        // Get state rules
        var rules = ConversationRules.States[CurrentState];

        // Draw new conversation cards filtered by current comfort level
        var newCards = Deck.Draw(rules.CardsOnListen, CurrentComfort);
        HandCards.AddRange(newCards);

        // Check letter deck if state requires it (OPEN checks trust, CONNECTED checks all)
        if (rules.ChecksLetterDeck && tokenManager != null && NPC.LetterDeck != null && NPC.LetterDeck.Any())
        {
            var npcTokens = tokenManager.GetTokensWithNPC(NPC.ID);
            
            foreach (var letterCard in NPC.LetterDeck)
            {
                // Filter by letter type if in OPEN state (trust only)
                if (rules.ChecksTrustLettersOnly && letterCard.ConnectionType != ConnectionType.Trust)
                    continue;
                    
                // Check if letter is eligible with current tokens and state
                if (letterCard.IsEligible(npcTokens, CurrentState))
                {
                    // Convert letter card to conversation card for negotiation
                    var conversationCard = letterCard.ToConversationCard(NPC.ID, NPC.Name);
                    
                    // Calculate success rate with linear token progression (+5% per token)
                    var relevantTokens = npcTokens.GetValueOrDefault(letterCard.ConnectionType, 0);
                    var successRate = letterCard.BaseSuccessRate + (relevantTokens * 5);
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
                    Console.WriteLine($"[ExecuteListen] Added eligible letter: {letterCard.Title} (Success: {successRate}%)");
                }
            }
        }

        // Inject crisis cards if state requires it (DESPERATE/HOSTILE)
        if (rules.InjectsCrisis && rules.CrisisCardsInjected > 0)
        {
            // Check if we have crisis cards in the crisis deck
            if (NPC.CrisisDeck != null && NPC.CrisisDeck.RemainingCards > 0)
            {
                // Draw from crisis deck
                var crisisCards = NPC.CrisisDeck.Draw(rules.CrisisCardsInjected, 0);
                HandCards.AddRange(crisisCards);
                Console.WriteLine($"[ExecuteListen] Injected {crisisCards.Count} crisis cards from deck");
            }
            else
            {
                // Generate crisis cards if deck is empty
                for (int i = 0; i < rules.CrisisCardsInjected; i++)
                {
                    var crisisCard = Deck.GenerateCrisisCard(NPC);
                    HandCards.Add(crisisCard);
                }
                Console.WriteLine($"[ExecuteListen] Generated {rules.CrisisCardsInjected} crisis cards");
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

        var manager = new CardSelectionManager(CurrentState);
        foreach (var card in selectedCards)
        {
            manager.ToggleCard(card);
        }

        var result = manager.PlaySelectedCards();

        // Apply comfort
        CurrentComfort += result.TotalComfort;

        // Apply state change if any
        if (result.NewState.HasValue)
        {
            CurrentState = result.NewState.Value;
        }
        
        // If we're in HOSTILE state and played crisis cards, mark final turn as taken
        if (CurrentState == EmotionalState.HOSTILE)
        {
            HadFinalCrisisTurn = true;
        }

        // Remove played cards from hand
        foreach (var card in selectedCards)
        {
            HandCards.Remove(card);
            
            // Handle different persistence types
            if (card.Persistence == PersistenceType.OneShot)
            {
                Deck.RemoveCard(card); // Permanently remove
            }
            else
            {
                Deck.Discard(card); // Will return to deck later
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
        if (!Deck.IsCrisis())
        {
            // For normal conversations, check if deck is empty
            if (Deck.IsEmpty)
                return true;
        }
            
        // HOSTILE state: Allow one final turn to play crisis cards
        if (CurrentState == EmotionalState.HOSTILE)
        {
            // If we haven't had the final turn yet, don't end
            if (!HadFinalCrisisTurn)
            {
                return false;
            }
            // If we've had the final turn, end the conversation
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
    
    private static string GetExchangeName(ExchangeCard exchange)
    {
        // Generate proper exchange names based on template type
        return exchange.TemplateType switch
        {
            "food" => "Buy Travel Provisions",
            "healing" => "Purchase Medicine",
            "information" => "Information Trade",
            "work" => "Help Inventory Stock",
            "favor" => "Noble Favor",
            "lodging" => "Rest at the Inn",
            _ => "Make Exchange"
        };
    }
    
    private static string GetExchangeCostDisplay(ExchangeCard exchange)
    {
        if (exchange.Cost == null || !exchange.Cost.Any())
            return "Free";
            
        var costParts = exchange.Cost.Select(c => c.GetDisplayText());
        return string.Join(", ", costParts);
    }
    
    private static string GetExchangeRewardDisplay(ExchangeCard exchange)
    {
        if (exchange.Reward == null || !exchange.Reward.Any())
            return "Nothing";
            
        var rewardParts = exchange.Reward.Select(r => r.GetDisplayText());
        return string.Join(", ", rewardParts);
    }
}