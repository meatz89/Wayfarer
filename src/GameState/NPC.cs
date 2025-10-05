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
    public PersonalityModifier ConversationModifier { get; set; } // Personality-specific conversation rules

    // Level system (1-5) for difficulty/content progression and XP scaling
    public int Level { get; set; } = 1;

    // Tier system (1-5) for difficulty/content progression
    public int Tier { get; set; } = 1;

    // Conversation difficulty level (1-3) for XP multipliers
    public int ConversationDifficulty { get; set; } = 1;

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
    // Letters are detected by checking Requests for available request cards
    // Burden history detected by counting burden cards in BurdenDeck
    // Crisis detected by checking CurrentState == ConnectionState.DISCONNECTED

    // Relationship Flow (Single value 0-24 encoding both state and battery)
    // 0-4: DISCONNECTED, 5-9: GUARDED, 10-14: NEUTRAL, 15-19: RECEPTIVE, 20-24: TRUSTING
    // Within each range: 0=-2, 1=-1, 2=0, 3=+1, 4=+2 (displays as -3 to +3, transition at ±4)
    public int RelationshipFlow { get; set; } = 12; // Start at NEUTRAL with 0 flow

    // Calculated properties from single flow value
    public ConnectionState CurrentState => GetConnectionState();


    // NPC DECK ARCHITECTURE
    public List<ExchangeCard> ExchangeDeck { get; set; } = new();  // 5-10 exchange cards: Simple instant trades (Mercantile NPCs only)
    public CardDeck ObservationDeck { get; set; } = new();  // Cards created from location observations
    public CardDeck BurdenDeck { get; set; } = new();  // Burden cards from past conflicts and resolution attempts

    // Signature Deck System - Knowledge cards earned through successful engagements
    public SignatureDeck SignatureDeck { get; set; } 

    // Requests system - Multiple requests, each with multiple cards at different rapport thresholds
    public List<NPCRequest> Requests { get; set; } = new List<NPCRequest>();

    // Initial token values to be applied during game initialization
    public Dictionary<string, int> InitialTokenValues { get; set; } = new Dictionary<string, int>();

    // Stranger-specific properties (for unnamed one-time NPCs)
    public bool IsStranger { get; set; } = false;
    public TimeBlocks? AvailableTimeBlock { get; set; } = null; // When stranger appears
    public bool HasBeenEncountered { get; set; } = false; // One-time flag

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
                // Burden cards should already have CardType.BurdenGoal set
                // No need to modify properties since we use CardType now
                BurdenDeck.AddCard(card);
            }
        }
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





    // Method for adding known routes (used by HELP verb)
    public void AddKnownRoute(RouteOption route)
    {
        if (!_knownRoutes.Any(r => r.Id == route.Id))
        {
            _knownRoutes.Add(route);
        }
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

    /// <summary>
    /// Get available one-time requests
    /// </summary>
    public List<NPCRequest> GetAvailableRequests()
    {
        List<NPCRequest> available = new List<NPCRequest>();
        if (Requests != null)
        {
            foreach (NPCRequest request in Requests)
            {
                if (request.IsAvailable())
                {
                    available.Add(request);
                }
            }
        }
        return available;
    }

    /// <summary>
    /// Check if NPC has any available one-time requests
    /// </summary>
    public bool HasAvailableRequests()
    {
        return GetAvailableRequests().Count > 0;
    }

    /// <summary>
    /// Get a specific request by ID
    /// </summary>
    public NPCRequest GetRequestById(string requestId)
    {
        if (Requests != null)
        {
            foreach (NPCRequest request in Requests)
            {
                if (request.Id == requestId)
                {
                    return request;
                }
            }
        }
        return null;
    }

    /// <summary>
    /// Mark a request as completed
    /// </summary>
    public void CompleteRequest(string requestId)
    {
        NPCRequest request = GetRequestById(requestId);
        if (request != null)
        {
            request.Complete();
        }
    }

    // Stranger-specific methods

    /// <summary>
    /// Check if this stranger is available at the current time block
    /// </summary>
    public bool IsAvailableAtTime(TimeBlocks currentTime)
    {
        if (!IsStranger) return true; // Named NPCs are always available
        return AvailableTimeBlock.HasValue && AvailableTimeBlock.Value == currentTime && !HasBeenEncountered;
    }

    /// <summary>
    /// Mark this stranger as encountered (one-time flag)
    /// </summary>
    public void MarkAsEncountered()
    {
        if (IsStranger)
        {
            HasBeenEncountered = true;
        }
    }

    /// <summary>
    /// Reset stranger availability for new time block
    /// </summary>
    public void RefreshForNewTimeBlock()
    {
        if (IsStranger)
        {
            HasBeenEncountered = false;
        }
    }


}