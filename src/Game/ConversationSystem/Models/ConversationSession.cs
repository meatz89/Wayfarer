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
    /// Current conversation depth (0-3)
    /// </summary>
    public int CurrentDepth { get; set; }

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
        ConversationType conversationType)
    {
        // Determine initial state based on NPC condition
        var initialState = ConversationRules.DetermineInitialState(npc, queueManager);
        
        // Calculate starting patience
        var basePatience = GetBasePatience(npc.PersonalityType);
        
        // Apply emotional state penalties
        var statepenalty = initialState switch
        {
            EmotionalState.DESPERATE => -3,
            EmotionalState.TENSE => -1,
            _ => 0
        };
        
        var totalPatience = Math.Max(3, basePatience + statepenalty);

        // Create and initialize deck
        var deck = new CardDeck();
        deck.InitializeForNPC(npc, tokenManager);

        // Draw initial hand
        var handCards = new List<ConversationCard>();
        handCards.AddRange(deck.Draw(3, 0)); // Base 3 cards at depth 0
        
        // Add observation cards if any
        if (observationCards != null && observationCards.Any())
        {
            handCards.AddRange(observationCards);
        }

        return new ConversationSession
        {
            NPC = npc,
            CurrentState = initialState,
            HandCards = handCards,
            Deck = deck,
            CurrentPatience = totalPatience,
            MaxPatience = totalPatience,
            CurrentComfort = 0,
            CurrentDepth = 0,
            TurnNumber = 0,
            LetterGenerated = false,
            ObservationCards = observationCards ?? new List<ConversationCard>()
        };
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
            CurrentDepth = 0,
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
        
        // Create crisis deck if needed
        var deck = npc.CrisisDeck ?? new CardDeck();
        
        // Draw initial hand
        var handCards = new List<ConversationCard>();
        handCards.AddRange(deck.Draw(2, 0)); // Start with 2 cards at depth 0
        
        // Add observation cards if any
        if (observationCards != null && observationCards.Any())
        {
            handCards.AddRange(observationCards);
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
            CurrentDepth = 0,
            TurnNumber = 0,
            LetterGenerated = false,
            ObservationCards = observationCards ?? new List<ConversationCard>()
        };
    }

    /// <summary>
    /// Execute LISTEN action
    /// </summary>
    public void ExecuteListen()
    {
        TurnNumber++;
        CurrentPatience--;

        // Remove all opportunity cards
        HandCards.RemoveAll(c => c.Persistence == PersistenceType.Opportunity);

        // Get state rules
        var rules = ConversationRules.States[CurrentState];

        // Draw new cards filtered by current depth
        var newCards = Deck.Draw(rules.CardsOnListen, CurrentDepth);
        HandCards.AddRange(newCards);

        // Inject crisis cards if needed
        if (rules.InjectsCrisis)
        {
            for (int i = 0; i < rules.CrisisCardsInjected; i++)
            {
                var crisisCard = Deck.GenerateCrisisCard(NPC);
                HandCards.Add(crisisCard);
            }
        }

        // Transition state
        var previousState = CurrentState;
        CurrentState = rules.ListenTransition;
        
        // If we just transitioned to HOSTILE, mark that we're allowing a final turn
        if (CurrentState == EmotionalState.HOSTILE && previousState != EmotionalState.HOSTILE)
        {
            HadFinalCrisisTurn = false; // Reset to allow one turn
        }

        // Check depth advancement
        CheckDepthAdvancement();
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

        // Check depth advancement
        CheckDepthAdvancement();

        return result;
    }

    /// <summary>
    /// Check if depth should advance based on comfort thresholds and current state
    /// </summary>
    private void CheckDepthAdvancement()
    {
        var rules = ConversationRules.States[CurrentState];
        
        // CONNECTED auto-advances depth
        if (rules.AutoAdvanceDepth)
        {
            CurrentDepth = Math.Min(GameRules.MAX_CONVERSATION_DEPTH, CurrentDepth + 1);
            return;
        }

        // Depth can only advance in NEUTRAL, OPEN, or CONNECTED states
        if (CurrentState != EmotionalState.NEUTRAL && 
            CurrentState != EmotionalState.OPEN && 
            CurrentState != EmotionalState.CONNECTED)
        {
            return;
        }

        // Check comfort thresholds for depth advancement
        int newDepth = CurrentDepth;
        
        if (CurrentComfort >= GameRules.DEPTH_ADVANCE_THRESHOLD_3 && CurrentDepth < 3)
        {
            newDepth = 3; // Intimate to Deep
        }
        else if (CurrentComfort >= GameRules.DEPTH_ADVANCE_THRESHOLD_2 && CurrentDepth < 2)
        {
            newDepth = 2; // Personal to Intimate
        }
        else if (CurrentComfort >= GameRules.DEPTH_ADVANCE_THRESHOLD_1 && CurrentDepth < 1)
        {
            newDepth = 1; // Surface to Personal
        }

        CurrentDepth = newDepth;
    }

    /// <summary>
    /// Check comfort thresholds for rewards
    /// </summary>
    public ConversationOutcome CheckThresholds()
    {
        var outcome = new ConversationOutcome
        {
            TotalComfort = CurrentComfort,
            FinalDepth = CurrentDepth,
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
        // Check if patience is gone or deck is empty
        if (CurrentPatience <= 0 || Deck.IsEmpty)
            return true;
            
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