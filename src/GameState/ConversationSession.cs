
using System.Linq;

// Conversation session
public class ConversationSession
{
    public string SessionId { get; init; } = Guid.NewGuid().ToString();
    public NPC NPC { get; set; }
    public ConversationType ConversationType { get; set; }
    public ConnectionState CurrentState { get; set; }
    public ConnectionState InitialState { get; set; }
    public int CurrentFlow { get; set; }
    public int CurrentPatience { get; set; }
    public int MaxPatience { get; set; }
    public int TurnNumber { get; set; }
    public bool LetterGenerated { get; set; }
    public bool RequestCardDrawn { get; set; }
    public int? RequestUrgencyCounter { get; set; }
    public bool RequestCardPlayed { get; set; }
    public SessionCardDeck Deck { get; set; }
    public HandDeck Hand { get; set; }
    public HashSet<CardInstance> HandCards
    {
        get
        {
            if (Hand?.Cards != null) return Hand.Cards;
            return new HashSet<CardInstance>();
        }
    }
    public List<CardInstance> PlayedCards { get; set; } = new();
    public List<CardInstance> DiscardedCards { get; set; } = new();
    public TokenMechanicsManager TokenManager { get; set; }
    public FlowManager FlowManager { get; set; }
    public RapportManager RapportManager { get; set; }

    // New focus and atmosphere system
    public int FlowBattery { get; set; } = 0; // -3 to +3
    public int CurrentFocus { get; set; } = 0; // Current spent focus
    public int MaxFocus { get; set; } = 5; // Based on state
    public AtmosphereType CurrentAtmosphere { get; set; } = AtmosphereType.Neutral;

    // Hidden momentum system - improves luck after failures
    public int HiddenMomentum { get; set; } = 0; // Invisible bad luck protection

    public List<CardInstance> ObservationCards { get; set; } = new();
    
    // NPC-specific observation cards (from NPC's ObservationDeck)
    public List<CardInstance> NPCObservationCards { get; set; } = new();

    // Rapport goal for standard conversations (FriendlyChat)
    public int? RapportGoal { get; set; } = null; // Target rapport to earn token reward

    // New helper methods
    public int GetAvailableFocus()
    {
        return Math.Max(0, GetEffectiveFocusCapacity() - CurrentFocus);
    }

    public int GetEffectiveFocusCapacity()
    {
        int baseCapacity = CurrentState switch
        {
            ConnectionState.DISCONNECTED => 3,
            ConnectionState.GUARDED => 4,
            ConnectionState.NEUTRAL => 5,
            ConnectionState.RECEPTIVE => 5,
            ConnectionState.TRUSTING => 6,
            _ => 5
        };

        // Prepared atmosphere adds +1 capacity
        if (CurrentAtmosphere == AtmosphereType.Prepared)
            baseCapacity += 1;

        return baseCapacity;
    }

    public int GetDrawCount()
    {
        int baseCount = CurrentState switch
        {
            ConnectionState.DISCONNECTED => 1,
            ConnectionState.GUARDED => 2,
            ConnectionState.NEUTRAL => 2,
            ConnectionState.RECEPTIVE => 3,
            ConnectionState.TRUSTING => 3,
            _ => 2
        };

        // AtmosphereType modifiers
        if (CurrentAtmosphere == AtmosphereType.Receptive)
            baseCount += 1;
        else if (CurrentAtmosphere == AtmosphereType.Pressured)
            baseCount = Math.Max(1, baseCount - 1);

        return baseCount;
    }

    public void RefreshFocus()
    {
        CurrentFocus = 0;
        MaxFocus = GetEffectiveFocusCapacity();
    }

    public bool IsHandOverflowing()
    {
        return HandCards.Count > 10; // Simplified overflow check
    }

    public bool ShouldEnd()
    {
        // End if patience exhausted or at disconnected with -3 flow
        return CurrentPatience <= 0 || (CurrentState == ConnectionState.DISCONNECTED && FlowBattery <= -3);
    }

    public ConversationOutcome CheckThresholds()
    {
        if (CurrentFlow >= 100)
        {
            return new ConversationOutcome
            {
                Success = true,
                FinalFlow = CurrentFlow,
                FinalState = CurrentState,
                TokensEarned = CalculateTokenReward(),
                Reason = "Flow threshold reached"
            };
        }

        if (CurrentPatience <= 0)
        {
            return new ConversationOutcome
            {
                Success = false,
                FinalFlow = CurrentFlow,
                FinalState = CurrentState,
                TokensEarned = 0,
                Reason = "Patience exhausted"
            };
        }

        // Conversation ended normally without hitting thresholds
        return new ConversationOutcome
        {
            Success = true,
            FinalFlow = CurrentFlow,
            FinalState = CurrentState,
            TokensEarned = CalculateTokenReward(),
            Reason = "Conversation ended"
        };
    }

    private int CalculateTokenReward()
    {
        if (CurrentFlow >= 100) return 3;
        if (CurrentFlow >= 75) return 2;
        if (CurrentFlow >= 50) return 1;
        return 0;
    }

    public void ExecuteListen(TokenMechanicsManager tokenManager, ObligationQueueManager queueManager, GameWorld gameWorld)
    {
        // Implementation handled by ConversationOrchestrator
    }

    public CardPlayResult ExecuteSpeak(HashSet<CardInstance> selectedCards)
    {
        // Implementation handled by ConversationOrchestrator
        return new CardPlayResult
        {
            TotalFlow = 0,
            Results = new List<SingleCardResult>()
        };
    }

    public static ConversationSession StartConversation(NPC npc, ObligationQueueManager queueManager, TokenMechanicsManager tokenManager,
        List<CardInstance> observationCards, ConversationType conversationType, PlayerResourceState playerResourceState, GameWorld gameWorld)
    {
        // Use properly typed parameters
        List<CardInstance> obsCards = observationCards ?? new List<CardInstance>();
        ConversationType convType = conversationType;
        GameWorld world = gameWorld;

        // Determine initial state
        ConnectionState initialState = ConversationRules.DetermineInitialState(npc, queueManager);

        // Create session deck from NPC's conversation cards
        SessionCardDeck sessionDeck = SessionCardDeck.CreateFromTemplates(npc.ConversationDeck?.GetAllCards() ?? new List<ConversationCard>(), npc.ID);

        // Add observation cards if provided
        foreach (CardInstance obsCard in obsCards)
        {
            sessionDeck.AddCard(obsCard);
        }
        
        // Load NPC observation cards from their ObservationDeck
        List<CardInstance> npcObservationCards = new List<CardInstance>();
        if (npc.ObservationDeck != null && npc.ObservationDeck.Any())
        {
            foreach (ConversationCard obsCard in npc.ObservationDeck.GetAllCards())
            {
                npcObservationCards.Add(new CardInstance
                {
                    Id = obsCard.Id,
                    Description = obsCard.Description,
                    Focus = 0, // Observation cards always cost 0 focus
                    Difficulty = obsCard.Difficulty,
                    SuccessEffect = obsCard.SuccessEffect,
                    FailureEffect = obsCard.FailureEffect,
                    ExhaustEffect = obsCard.ExhaustEffect,
                    Properties = new List<CardProperty>(obsCard.Properties),
                    TokenType = obsCard.TokenType,
                    DialogueFragment = obsCard.DialogueFragment,
                    VerbPhrase = obsCard.VerbPhrase
                });
            }
        }

        // Determine rapport goal for standard conversations
        int? rapportGoal = null;
        if (convType == ConversationType.FriendlyChat)
        {
            // Set goal based on connection state - harder when disconnected
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

        // Create session with proper initialization
        ConversationSession session = new ConversationSession
        {
            NPC = npc,
            ConversationType = convType,
            CurrentState = initialState,
            InitialState = initialState,
            CurrentFlow = 0,
            CurrentPatience = 10,
            MaxPatience = 10,
            TurnNumber = 0,
            Deck = sessionDeck,
            Hand = new HandDeck(),
            TokenManager = tokenManager,
            ObservationCards = obsCards,
            NPCObservationCards = npcObservationCards,
            RapportGoal = rapportGoal
        };

        return session;
    }

    public static ConversationSession StartExchange(NPC npc, PlayerResourceState playerResourceState, TokenMechanicsManager tokenManager,
        List<string> spotDomainTags, ObligationQueueManager queueManager, GameWorld gameWorld)
    {
        Console.WriteLine($"[ConversationSession.StartExchange] Starting exchange with NPC: {npc.ID}");
        
        // Create session deck from NPC's exchange cards
        List<ConversationCard> exchangeCards = npc.ExchangeDeck?.GetAllCards() ?? new List<ConversationCard>();
        Console.WriteLine($"[ConversationSession.StartExchange] Found {exchangeCards.Count} exchange cards");
        
        SessionCardDeck sessionDeck = SessionCardDeck.CreateFromTemplates(exchangeCards, npc.ID);
        Console.WriteLine($"[ConversationSession.StartExchange] Created session deck with {sessionDeck.GetAllCards().Count()} cards");
        
        // Populate exchange data for all cards in the deck
        foreach (CardInstance card in sessionDeck.GetAllCards())
        {
            Console.WriteLine($"[ConversationSession.StartExchange] Processing card {card.Id}");
            Console.WriteLine($"[ConversationSession.StartExchange] SuccessEffect: Type={card.SuccessEffect?.Type}, Value={card.SuccessEffect?.Value}");
            
            if (card.SuccessEffect != null && 
                card.SuccessEffect.Type == CardEffectType.Exchange &&
                !string.IsNullOrEmpty(card.SuccessEffect.Value))
            {
                Console.WriteLine($"[ConversationSession.StartExchange] Looking for exchange definition: {card.SuccessEffect.Value}");
                
                // Look up exchange definition by ID
                ExchangeDTO exchangeDef = gameWorld.ExchangeDefinitions
                    .FirstOrDefault(e => e.Id == card.SuccessEffect.Value);
                    
                if (exchangeDef != null)
                {
                    Console.WriteLine($"[ConversationSession.StartExchange] Found exchange definition: {exchangeDef.Id}");
                    
                    // Create context if it doesn't exist
                    if (card.Context == null)
                        card.Context = new CardContext();
                    
                    // Create ExchangeData object and populate from definition
                    ExchangeData exchangeData = new ExchangeData
                    {
                        Id = exchangeDef.Id,
                        Name = exchangeDef.Name,
                        Cost = new Dictionary<ResourceType, int>(),
                        Reward = new Dictionary<ResourceType, int>()
                    };
                    
                    // Convert string currency to ResourceType for cost
                    ResourceType? costType = ParseResourceType(exchangeDef.GiveCurrency);
                    if (costType.HasValue)
                    {
                        exchangeData.Cost[costType.Value] = exchangeDef.GiveAmount;
                        Console.WriteLine($"[ConversationSession.StartExchange] Set cost: {exchangeDef.GiveAmount} {costType.Value}");
                    }
                    
                    // Convert string currency to ResourceType for reward
                    ResourceType? rewardType = ParseResourceType(exchangeDef.ReceiveCurrency);
                    if (rewardType.HasValue)
                    {
                        exchangeData.Reward[rewardType.Value] = exchangeDef.ReceiveAmount;
                        Console.WriteLine($"[ConversationSession.StartExchange] Set reward: {exchangeDef.ReceiveAmount} {rewardType.Value}");
                    }
                    
                    // Set the ExchangeData on the context
                    card.Context.ExchangeData = exchangeData;
                    
                    // Also populate the effect's ExchangeData
                    card.SuccessEffect.ExchangeData = exchangeData;
                    
                    Console.WriteLine($"[ConversationSession.StartExchange] Successfully populated exchange data for card {card.Id}");
                }
                else
                {
                    Console.WriteLine($"[ConversationSession.StartExchange] WARNING: Exchange definition not found for ID: {card.SuccessEffect.Value}");
                }
            }
        }

        // Determine initial state  
        ConnectionState initialState = ConversationRules.DetermineInitialState(npc, queueManager);

        ConversationSession session = new ConversationSession
        {
            NPC = npc,
            ConversationType = ConversationType.Commerce,
            CurrentState = initialState,
            InitialState = initialState,
            CurrentFlow = 0,
            CurrentPatience = 10,
            MaxPatience = 10,
            TurnNumber = 0,
            Deck = sessionDeck,
            Hand = new HandDeck(),
            TokenManager = tokenManager
        };

        return session;
    }
    
    private static ResourceType? ParseResourceType(string name)
    {
        return name?.ToLower() switch
        {
            "coins" => ResourceType.Coins,
            "health" => ResourceType.Health,
            "food" => ResourceType.Food,
            "hunger" => ResourceType.Hunger,
            "attention" => ResourceType.Attention,
            _ => null
        };
    }
}
