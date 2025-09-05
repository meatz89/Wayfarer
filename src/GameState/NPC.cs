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
    // Burden history detected by counting burden cards in ConversationDeck
    // Crisis detected by checking CurrentState == ConnectionState.DISCONNECTED

    // Connection State
    public ConnectionState CurrentState { get; set; } = ConnectionState.NEUTRAL;
    public ConnectionState CurrentConnectionState => CurrentState; // Alias for compatibility

    // THREE DECK ARCHITECTURE (POC EXACT)
    public CardDeck ConversationDeck { get; set; } = new();  // 20-30 cards: Flow, Token, State, Knowledge, Burden
    public CardDeck RequestDeck { get; set; } = new();  // 0-3 cards: Promise (letters), Resolution requests
    public CardDeck ExchangeDeck { get; set; } = new();  // 5-10 cards: Simple instant trades (Mercantile NPCs only) 

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
    public void InitializeExchangeDeck(List<ConversationCard> exchangeCards = null)
    {
        if (ExchangeDeck == null || !ExchangeDeck.Any())
        {
            ExchangeDeck = new CardDeck();
            // Only Mercantile NPCs have exchange decks
            if (PersonalityType == PersonalityType.MERCANTILE && exchangeCards != null)
            {
                Console.WriteLine($"[NPC.InitializeExchangeDeck] Adding {exchangeCards.Count} exchange cards for {Name}");
                foreach (ConversationCard card in exchangeCards)
                {
                    Console.WriteLine($"[NPC.InitializeExchangeDeck] Card {card.Id}");
                    // Exchange data now stored in card effects, not context
                    ExchangeDeck.AddCard(card);
                }
            }
        }
    }

    // Check if NPC has promise cards (letters) in their request deck  
    public bool HasPromiseCards()
    {
        if (RequestDeck == null) return false;
        return RequestDeck.GetAllCards().Any(c => c.Category == CardCategory.Promise.ToString());
    }

    // Check if NPC has burden history (cards in conversation deck)
    public bool HasBurdenHistory()
    {
        return CountBurdenCards() > 0;
    }

    // Count burden cards in conversation deck
    public int CountBurdenCards()
    {
        if (ConversationDeck == null) return 0;

        // Burden cards are identified by their properties
        return ConversationDeck.GetAllCards()
            .Count(card => card.IsBurden);
    }

    // Check if NPC has exchange cards available
    public bool HasExchangeCards()
    {
        return ExchangeDeck != null && ExchangeDeck.Any();
    }

    // Get today's exchange card (selected deterministically at dawn)
    public ConversationCard GetTodaysExchange(int currentDay)
    {
        // Exchange cards only for Mercantile NPCs
        if (PersonalityType != PersonalityType.MERCANTILE || ExchangeDeck == null || !ExchangeDeck.Any())
            return null;

        // Use deterministic selection based on day and NPC ID
        List<ConversationCard> cards = ExchangeDeck.GetAllCards();
        if (cards.Count == 0) return null;

        int index = (currentDay * ID.GetHashCode()) % cards.Count;
        return cards[Math.Abs(index)];
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

}