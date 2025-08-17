
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
    public PersonalityType PersonalityType { get; set; } = PersonalityType.STEADFAST; // Categorical type for mechanics

    // Tier system (1-5) for difficulty/content progression
    public int Tier { get; set; } = 1;

    // NPCs are always available - no schedule system
    public List<ServiceTypes> ProvidedServices { get; set; } = new List<ServiceTypes>();
    public NPCRelationship PlayerRelationship { get; set; } = NPCRelationship.Neutral;

    // Letter Queue Properties
    public List<ConnectionType> LetterTokenTypes { get; set; } = new List<ConnectionType>();

    // Work Properties
    public bool OffersWork => ProvidedServices.Contains(ServiceTypes.Work);

    // Confrontation Tracking
    public int LastConfrontationCount { get; set; } = 0;  // Track confrontations already shown
    public int RedemptionProgress { get; set; } = 0;      // Progress toward emotional recovery
    public bool HasPermanentScar { get; set; } = false;   // Some wounds never fully heal
    
    // Letter offering system
    public bool HasLetterToOffer { get; set; } = false;
    
    // Schedule tracking (for INVESTIGATE verb discoveries)
    public List<ScheduleEntry> DailySchedule { get; set; } = new List<ScheduleEntry>();
    
    // Known routes (for HELP verb sharing)
    private List<RouteOption> _knownRoutes = new List<RouteOption>();
    
    // Conversation deck - each NPC has unique cards representing shared history
    public NPCDeck ConversationDeck { get; set; }
    
    // Initialize deck when NPC is created
    public void InitializeConversationDeck()
    {
        if (ConversationDeck == null)
        {
            ConversationDeck = new NPCDeck(ID, PersonalityType);
        }
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

    // Letter generation methods for VerbContextualizer
    public bool HasLetterToSend()
    {
        // Simple logic: NPCs have letters occasionally
        // In full implementation, would check NPC state, relationships, etc.
        return new Random().Next(3) == 0; // 33% chance
    }

    public Letter GenerateLetter()
    {
        // Generate a simple letter from this NPC
        var letter = new Letter
        {
            Id = Guid.NewGuid().ToString(),
            SenderId = this.ID,
            SenderName = this.Name,
            RecipientId = "player_contact_" + new Random().Next(1, 5),
            RecipientName = "Contact " + new Random().Next(1, 5),
            Description = $"Letter from {Name} about {Profession} matters",
            TokenType = LetterTokenTypes.FirstOrDefault(),
            Stakes = StakeType.REPUTATION,
            DeadlineInHours = new Random().Next(2, 7) * 24,
            QueuePosition = 6, // Add to back of queue
            State = LetterState.Offered
        };
        return letter;
    }

    // Methods expected by VerbContextualizer
    public NPC GetContact()
    {
        // Return an NPC that this NPC knows
        // In full implementation, would be based on NPC's network
        return new NPC
        {
            ID = $"contact_{new Random().Next(1, 5)}",
            Name = $"Contact {new Random().Next(1, 5)}",
            Role = "Acquaintance",
            Description = $"An acquaintance of {Name}",
            Location = this.Location,
            SpotId = this.SpotId,
            Profession = Professions.Merchant
        };
    }

    // Method for adding known routes (used by HELP verb)
    public void AddKnownRoute(RouteOption route)
    {
        if (!_knownRoutes.Any(r => r.Id == route.Id))
        {
            _knownRoutes.Add(route);
        }
    }
    
    // Method for generating letter offers (used by HELP verb)
    public Letter GenerateLetterOffer()
    {
        // Generate a letter based on NPC's profession and current state
        var letter = new Letter
        {
            Id = Guid.NewGuid().ToString(),
            SenderId = this.ID,
            SenderName = this.Name,
            RecipientId = $"recipient_{new Random().Next(1, 5)}",
            RecipientName = GetRecipientNameByProfession(),
            Description = GetLetterDescriptionByProfession(),
            TokenType = LetterTokenTypes.FirstOrDefault(),
            Stakes = GetStakesByProfession(),
            DeadlineInHours = new Random().Next(4, 24),
            Payment = new Random().Next(5, 20),
            // Weight is calculated, not set directly
            State = LetterState.Offered,
            HumanContext = GetHumanContextByProfession(),
            ConsequenceIfLate = GetConsequenceByProfession()
        };
        
        HasLetterToOffer = false; // Mark as offered
        return letter;
    }
    
    // Check if NPC has an urgent letter to offer
    public bool HasUrgentLetter()
    {
        // Simple random chance for now - in full implementation would check state
        return new Random().Next(100) < 20; // 20% chance of urgent letter
    }
    
    // Generate an urgent letter with tight deadline
    public Letter GenerateUrgentLetter()
    {
        var letter = GenerateLetterOffer();
        letter.DeadlineInHours = new Random().Next(2, 8); // Much tighter deadline
        letter.Stakes = StakeType.SAFETY; // Higher stakes
        letter.Payment = new Random().Next(15, 30); // Better payment for urgency
        letter.Description = $"URGENT: {letter.Description}";
        return letter;
    }
    
    private string GetRecipientNameByProfession()
    {
        return Profession switch
        {
            Professions.Merchant => $"Master {new[] {"Goldwin", "Harwick", "Blackstone"}[new Random().Next(3)]}",
            Professions.Scholar => $"Sister {new[] {"Mercy", "Grace", "Hope"}[new Random().Next(3)]}",
            Professions.Noble => $"Lord {new[] {"Ashford", "Ravencrest", "Ironwood"}[new Random().Next(3)]}",
            _ => $"Citizen {new Random().Next(1, 10)}"
        };
    }
    
    private string GetLetterDescriptionByProfession()
    {
        return Profession switch
        {
            Professions.Merchant => "Trade agreement requiring urgent signature",
            Professions.Scholar => "Medical supplies request for the infirmary",
            Professions.Noble => "Summons to appear before the council",
            _ => "Personal correspondence"
        };
    }
    
    private StakeType GetStakesByProfession()
    {
        return Profession switch
        {
            Professions.Merchant => StakeType.WEALTH,
            Professions.Scholar => StakeType.SAFETY,
            Professions.Noble => StakeType.REPUTATION,
            _ => StakeType.REPUTATION
        };
    }
    
    private string GetHumanContextByProfession()
    {
        return Profession switch
        {
            Professions.Merchant => "A crucial trade deal hangs in the balance",
            Professions.Scholar => "Lives depend on these medical supplies arriving",
            Professions.Noble => "Political alliances shift with every delayed message",
            _ => "Someone's future depends on this letter"
        };
    }
    
    private string GetConsequenceByProfession()
    {
        return Profession switch
        {
            Professions.Merchant => "The merchant will lose their largest contract",
            Professions.Scholar => "Patients may not survive without these supplies",
            Professions.Noble => "Your standing with the nobility will be permanently damaged",
            _ => "Trust will be broken beyond repair"
        };
    }
    
    public RouteOption GetSecretRoute()
    {
        // Return a secret route this NPC knows
        // In full implementation, would be based on NPC's knowledge
        return new RouteOption
        {
            Id = $"secret_route_{Location}_{new Random().Next(1, 3)}",
            Name = $"Secret path from {Location}",
            Description = "A hidden route known only to locals",
            Destination = "MarketSquare",
            TravelTimeHours = 1,
            Method = TravelMethods.Walking
        };
    }

    public List<RouteOption> KnownRoutes()
    {
        // Return list of routes this NPC knows
        // In full implementation, would be based on NPC's profession and tier
        return new List<RouteOption> 
        { 
            new RouteOption
            {
                Id = $"route_{Location}_common",
                Name = $"Common route from {Location}",
                Description = "The usual path",
                Destination = "TownGate",
                TravelTimeHours = 2,
                Method = TravelMethods.Walking
            },
            new RouteOption
            {
                Id = $"route_{Location}_trade",
                Name = $"Trade route from {Location}",
                Description = "The merchant's path",
                Destination = "MerchantQuarter",
                TravelTimeHours = 3,
                Method = TravelMethods.Carriage
            }
        };
    }
}
