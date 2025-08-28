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

    // Schedule tracking (for INVESTIGATE verb discoveries)
    public List<ScheduleEntry> DailySchedule { get; set; } = new List<ScheduleEntry>();

    // Work and Home locations (for deeper world building)
    public string WorkLocationId { get; set; }
    public string WorkSpotId { get; set; }
    public string HomeLocationId { get; set; }
    public string HomeSpotId { get; set; }

    // Known routes (for HELP verb sharing)
    private List<RouteOption> _knownRoutes = new List<RouteOption>();

    // FOUR DECK ARCHITECTURE (POC EXACT)
    public CardDeck ConversationDeck { get; set; }  // Comfort/State/Burden cards (depth 0-20)
    public List<LetterCard> LetterDeck { get; set; } = new();  // Letter cards with eligibility (tokens + state)
    public CardDeck CrisisDeck { get; set; }  // Crisis letters (usually empty unless crisis)
    public List<ExchangeCard> ExchangeDeck { get; set; } = new();  // Exchange cards (Mercantile NPCs only)

    // Daily exchange selection
    public ExchangeCard TodaysExchangeCard { get; set; }
    public int LastExchangeSelectionDay { get; set; } = -1;

    // Initialize conversation deck with proper cards
    public void InitializeConversationDeck(NPCDeckFactory deckFactory)
    {
        ConversationDeck ??= deckFactory.CreateDeckForNPC(this);
    }
    
    // Initialize letter deck with eligibility-based cards
    public void InitializeLetterDeck()
    {
        // Only initialize if not already done
        if (LetterDeck == null || !LetterDeck.Any())
        {
            // For POC, only Elena has letters
            if (ID == "elena_merchant")
            {
                LetterDeck = LetterCardFactory.CreateElenaLetterDeck(ID);
            }
            else
            {
                LetterDeck = new List<LetterCard>();
            }
        }
    }

    // Initialize exchange deck (for NPCs that offer exchanges)
    public void InitializeExchangeDeck(List<string> spotDomainTags = null)
    {
        if (ExchangeDeck == null || !ExchangeDeck.Any())
        {
            // NPCs with exchange capabilities: MERCANTILE always, others based on location
            ExchangeDeck = ExchangeCardFactory.CreateExchangeDeck(PersonalityType, ID, spotDomainTags);
            
            // Log if exchanges were created
            if (ExchangeDeck.Any())
            {
                Console.WriteLine($"[NPC] Initialized {ExchangeDeck.Count} exchange cards for {Name} ({PersonalityType})");
            }
        }
    }

    // Initialize crisis deck (usually empty unless crisis active)
    public void InitializeCrisisDeck()
    {
        // Crisis deck starts empty and cards are added during crisis events
        CrisisDeck ??= new CardDeck();
    }
    
    // Add Crisis letters to the crisis deck
    public void AddCrisisCards(List<ConversationCard> crisisCards)
    {
        if (CrisisDeck == null)
        {
            InitializeCrisisDeck();
        }
        
        foreach (var card in crisisCards)
        {
            CrisisDeck.AddCard(card);
        }
    }

    // Check if NPC has Crisis letters
    public bool HasCrisisCards()
    {
        return CrisisDeck != null && CrisisDeck.RemainingCards > 0;
    }

    // Get today's exchange card (selected randomly at dawn)
    public ExchangeCard GetTodaysExchangeCard()
    {
        return TodaysExchangeCard;
    }

    // Get today's exchange card (selected randomly at dawn)
    public ExchangeCard GetTodaysExchange(int currentDay)
    {
        // If it's a new day, select a new card
        if (LastExchangeSelectionDay != currentDay)
        {
            if (ExchangeDeck?.Any() == true)
            {
                // Use deterministic random based on day and NPC ID
                var random = new Random(currentDay * ID.GetHashCode());
                TodaysExchangeCard = ExchangeDeck[random.Next(ExchangeDeck.Count)];
                LastExchangeSelectionDay = currentDay;
            }
            else
            {
                TodaysExchangeCard = null;
            }
        }
        return TodaysExchangeCard;
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
}