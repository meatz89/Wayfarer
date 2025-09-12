using System;
using System.Collections.Generic;
using System.Linq;

public class NPC
{
    // Identity
    public string ID { get; set; }
    public string Name { get; set; }
    public string Role { get; set; }
    public string Description { get; set; }
    public string Location { get; set; }
    public string SpotId { get; set; }

    // Skeleton tracking
    public bool IsSkeleton { get; set; } = false;
    public string SkeletonSource { get; set; } // What created this skeleton (e.g., "letter_template_elena_refusal")

    // Categorical Properties for Logical System Interactions
    public Professions Profession { get; set; }

    // Personality system
    public string PersonalityDescription { get; set; } = string.Empty; // Authentic description from JSON
    public PersonalityType PersonalityType { get; set; } // NO DEFAULT - must be set explicitly from JSON

    // Tier system (1-5) for difficulty/content progression
    public int Tier { get; set; } = 1;

    // NPCs are always available - no schedule system
    public List<ServiceTypes> ProvidedServices { get; set; } = new List<ServiceTypes>();
    public NPCRelationship PlayerRelationship { get; set; } = NPCRelationship.Neutral;

    // DeliveryObligation Queue Properties
    public List<ConnectionType> LetterTokenTypes { get; set; } = new List<ConnectionType>();

    // Work Properties
    public bool OffersWork => ProvidedServices.Contains(ServiceTypes.Work);

    // Confrontation Tracking
    public int LastConfrontationCount { get; set; } = 0;  // Track confrontations already shown
    public int RedemptionProgress { get; set; } = 0;      // Progress toward emotional recovery
    public bool HasPermanentScar { get; set; } = false;   // Some wounds never fully heal

    // DeliveryObligation offering system

    // Work and Home locations (for deeper world building)
    public string WorkLocationId { get; set; }
    public string WorkSpotId { get; set; }
    public string HomeLocationId { get; set; }
    public string HomeSpotId { get; set; }

    // Known routes (for HELP verb sharing)
    private List<RouteOption> _knownRoutes = new List<RouteOption>();

    // REMOVED: Boolean flags violate deck-based architecture
    // Letters are detected by checking RequestDeck contents
    // Burden history detected by counting burden cards in BurdenDeck
    // Crisis detected by checking CurrentState == ConnectionState.DISCONNECTED

    // Relationship Flow (Single value 0-24 encoding both state and battery)
    // 0-4: DISCONNECTED, 5-9: GUARDED, 10-14: NEUTRAL, 15-19: RECEPTIVE, 20-24: TRUSTING
    // Within each range: 0=-2, 1=-1, 2=0, 3=+1, 4=+2 (displays as -3 to +3, transition at ±4)
    public int RelationshipFlow { get; set; } = 12; // Start at NEUTRAL with 0 flow
    
    // Calculated properties from single flow value
    public ConnectionState CurrentState => GetConnectionState();
    public ConnectionState CurrentConnectionState => CurrentState; // Alias for compatibility
    public int CurrentFlow => GetFlowBattery(); // Returns -2 to +2 for display as -3 to +3

    // Daily Patience Economy
    public int DailyPatience { get; set; } // Current remaining patience for the day
    public int MaxDailyPatience { get; set; } // Maximum patience based on personality

    // FIVE DECK ARCHITECTURE (Per Documentation)
    public CardDeck ConversationDeck { get; set; } = new();  // 20-30 cards: Flow, Token, State, Knowledge
    public CardDeck RequestDeck { get; set; } = new();  // 0-3 cards: Promise (letters), Resolution requests
    public List<ExchangeCard> ExchangeDeck { get; set; } = new();  // 5-10 exchange cards: Simple instant trades (Mercantile NPCs only)
    public CardDeck ObservationDeck { get; set; } = new();  // Cards created from location observations
    public CardDeck BurdenDeck { get; set; } = new();  // Burden cards from past conflicts and resolution attempts 

    // Daily exchange selection (removed - handled by GetTodaysExchange method)


    // Initialize request deck from content repository
    public void InitializeRequestDeck(List<ConversationCard> requestCards = null)
    {
        // Only initialize if not already done
        if (RequestDeck == null || !RequestDeck.Any())
        {
            RequestDeck = new CardDeck();
            if (requestCards != null)
            {
                foreach (ConversationCard card in requestCards)
                {
                    RequestDeck.AddCard(card);
                }
            }
        }
    }

    // Initialize exchange deck (for Mercantile NPCs only)
    public void InitializeExchangeDeck(List<ExchangeCard> exchangeCards = null)
    {
        if (ExchangeDeck == null || !ExchangeDeck.Any())
        {
            ExchangeDeck = new List<ExchangeCard>();
            // Only Mercantile NPCs have exchange decks
            if (PersonalityType == PersonalityType.MERCANTILE && exchangeCards != null)
            {
                Console.WriteLine($"[NPC.InitializeExchangeDeck] Adding {exchangeCards.Count} exchange cards for {Name}");
                foreach (ExchangeCard card in exchangeCards)
                {
                    Console.WriteLine($"[NPC.InitializeExchangeDeck] Card {card.Id}");
                    ExchangeDeck.Add(card);
                }
            }
        }
    }

    // Initialize burden deck with burden cards
    public void InitializeBurdenDeck(List<ConversationCard> burdenCards = null)
    {
        // Only initialize if not already done
        if (BurdenDeck == null)
        {
            BurdenDeck = new CardDeck();
        }
        
        if (burdenCards != null)
        {
            foreach (ConversationCard card in burdenCards)
            {
                // Mark cards as burden cards if not already marked
                if (!card.Properties.Contains(CardProperty.Burden))
                {
                    card.Properties.Add(CardProperty.Burden);
                }
                BurdenDeck.AddCard(card);
            }
        }
    }

    // Check if NPC has promise cards (letters) in their request deck  
    public bool HasPromiseCards()
    {
        if (RequestDeck == null) return false;
        return RequestDeck.GetAllCards().Any(c => c.CardType == CardType.Promise);
    }

    // Check if NPC has burden history (cards in burden deck)
    public bool HasBurdenHistory()
    {
        return CountBurdenCards() > 0;
    }

    // Count burden cards in burden deck
    public int CountBurdenCards()
    {
        if (BurdenDeck == null) return 0;

        // All cards in the burden deck are burden cards
        return BurdenDeck.GetAllCards().Count();
    }

    // Check if NPC has exchange cards available
    public bool HasExchangeCards()
    {
        return ExchangeDeck != null && ExchangeDeck.Any();
    }

    // Get today's exchange card (selected deterministically at dawn)
    public ExchangeCard GetTodaysExchange(int currentDay)
    {
        // Exchange cards only for Mercantile NPCs
        if (PersonalityType != PersonalityType.MERCANTILE || ExchangeDeck == null || !ExchangeDeck.Any())
            return null;

        // Use deterministic selection based on day and NPC ID
        if (ExchangeDeck.Count == 0) return null;

        int index = (currentDay * ID.GetHashCode()) % ExchangeDeck.Count;
        return ExchangeDeck[Math.Abs(index)];
    }

    // Helper methods for UI display
    public string ProfessionDescription => Profession.ToString().Replace('_', ' ');

    public string ScheduleDescription => "Always available";

    public string ProvidedServicesDescription => ProvidedServices.Any()
        ? $"Services: {string.Join(", ", ProvidedServices.Select(s => s.ToString().Replace('_', ' ')))}"
        : "No services available";

    public bool IsAvailable(TimeBlocks currentTime)
    {
        // NPCs are always available by default
        return true;
    }

    public bool IsAvailableAtTime(string locationSpotId, TimeBlocks currentTime)
    {
        // NPCs are always available by default
        // Check if NPC is at the specified location spot
        return SpotId == locationSpotId && IsAvailable(currentTime);
    }

    public bool CanProvideService(ServiceTypes requestedService)
    {
        return ProvidedServices.Contains(requestedService);
    }

    internal bool IsAvailableAtLocation(string? spotID)
    {
        // NPCs are available at their assigned location
        return !string.IsNullOrEmpty(spotID) && Location == spotID;
    }




    // Method for adding known routes (used by HELP verb)
    public void AddKnownRoute(RouteOption route)
    {
        if (!_knownRoutes.Any(r => r.Id == route.Id))
        {
            _knownRoutes.Add(route);
        }
    }

    /// <summary>
    /// Check if NPC has valid request cards for Promise conversations
    /// </summary>
    public bool HasValidRequestCard(ConnectionState currentState)
    {
        return RequestDeck != null && RequestDeck.RemainingCards > 0;
    }

    /// <summary>
    /// Initialize daily patience based on personality type
    /// </summary>
    public void InitializeDailyPatience()
    {
        MaxDailyPatience = PersonalityType switch
        {
            PersonalityType.DEVOTED => 15,
            PersonalityType.STEADFAST => 13,
            PersonalityType.MERCANTILE => 12,
            PersonalityType.CUNNING => 12,
            PersonalityType.PROUD => 10,
            _ => 12 // Default fallback
        };
        
        // Set current patience to max on initialization
        DailyPatience = MaxDailyPatience;
    }

    /// <summary>
    /// Refresh patience to maximum (called at dawn)
    /// </summary>
    public void RefreshDailyPatience()
    {
        DailyPatience = MaxDailyPatience;
    }

    /// <summary>
    /// Check if NPC has patience for conversation
    /// </summary>
    public bool HasPatienceForConversation()
    {
        return DailyPatience > 0;
    }

    /// <summary>
    /// Spend patience for an exchange
    /// </summary>
    public void SpendPatience(int amount)
    {
        DailyPatience = Math.Max(0, DailyPatience - amount);
    }

    /// <summary>
    /// Get connection state from single flow value
    /// </summary>
    public ConnectionState GetConnectionState()
    {
        return RelationshipFlow switch
        {
            <= 4 => ConnectionState.DISCONNECTED,
            <= 9 => ConnectionState.GUARDED,
            <= 14 => ConnectionState.NEUTRAL,
            <= 19 => ConnectionState.RECEPTIVE,
            _ => ConnectionState.TRUSTING
        };
    }
    
    /// <summary>
    /// Get flow battery (-2 to +2, displayed as -3 to +3) from single value
    /// </summary>
    public int GetFlowBattery()
    {
        int position = RelationshipFlow % 5;
        return position - 2; // Maps 0->-2, 1->-1, 2->0, 3->+1, 4->+2
    }
    
    /// <summary>
    /// Apply daily decay - move flow toward global neutral value 12
    /// </summary>
    public void ApplyDailyDecay()
    {
        // Move toward global neutral (value 12)
        if (RelationshipFlow < 12)
        {
            RelationshipFlow++; // Move up toward neutral
        }
        else if (RelationshipFlow > 12)
        {
            RelationshipFlow--; // Move down toward neutral
        }
        // If at neutral (12), stay there
    }

}