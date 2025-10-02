public class Player
{
    // Core identity
    public string Name { get; set; }
    public Genders Gender { get; set; }
    public string Background { get; set; }

    // Archetype
    public Professions Archetype { get; set; }

    // Progression systems
    public int Level { get; set; } = 1;
    public int CurrentXP { get; set; } = 0;
    public int XPToNextLevel { get; set; } = 100; // Base XP required for next level

    // Resources
    public int Coins { get; set; } // Starting coins - intentionally kept as literal as it's game balance
    public int Health { get; set; } // Starting health
    public int Hunger { get; set; } // Starting hunger (0 = not hungry)
    public int Stamina { get; set; } // Starting stamina for travel
    public int MaxStamina { get; set; } = 10; // Maximum stamina

    public int MinHealth { get; set; } = 0; // Minimum health before death
    public int MaxHealth { get; set; } = 100; // Maximum health
    public int MaxHunger { get; set; } = 100; // Maximum hunger before problems


    public Inventory Inventory { get; set; } = new Inventory(10); // 10 weight capacity per documentation

    // Relationships with characters
    public RelationshipList Relationships { get; set; } = new();


    // Location knowledge
    // Location knowledge - Moved from action system
    public List<string> LocationActionAvailability { get; set; } = new List<string>();

    public List<string> DiscoveredLocationIds { get; set; } = new List<string>();

    // Travel capabilities
    public List<string> UnlockedTravelMethods { get; set; } = new List<string>();
    public List<string> DiscoveredRoutes { get; set; } = new List<string>();

    public bool IsInitialized { get; set; } = false;

    public PlayerStats Stats { get; private set; } = new();

    public LocationSpot CurrentLocationSpot { get; set; }
    public List<MemoryFlag> Memories { get; private set; } = new List<MemoryFlag>();


    public List<KnownRouteEntry> KnownRoutes { get; private set; } = new List<KnownRouteEntry>();

    public DeliveryObligation[] ObligationQueue { get; private set; } = new DeliveryObligation[8];
    public List<MeetingObligation> MeetingObligations { get; set; } = new List<MeetingObligation>();
    public List<NPCTokenEntry> NPCTokens { get; private set; } = new List<NPCTokenEntry>();

    // Physical DeliveryObligation Carrying
    public List<Letter> CarriedLetters { get; private set; } = new List<Letter>(); // Letters physically in satchel for delivery
    public int MaxSatchelSize { get; set; } = 12; // Maximum size capacity for letters in satchel

    // Queue manipulation tracking
    public int LastMorningSwapDay { get; set; } = -1; // Track when morning swap was last used

    // DeliveryObligation history tracking
    public List<LetterHistoryEntry> NPCLetterHistory { get; private set; } = new List<LetterHistoryEntry>();

    // Standing Obligations System
    public List<StandingObligation> StandingObligations { get; private set; } = new List<StandingObligation>();


    // Token Favor System
    public List<string> PurchasedFavors { get; set; } = new List<string>();
    public List<string> UnlockedLocationIds { get; set; } = new List<string>();
    public List<string> UnlockedServices { get; set; } = new List<string>();


    // Scenario tracking
    public List<DeliveryObligation> DeliveredLetters { get; set; } = new List<DeliveryObligation>();
    public int TotalLettersDelivered { get; set; } = 0;
    public int TotalLettersExpired { get; set; } = 0;
    public int TotalTokensSpent { get; set; } = 0;

    // Route Familiarity System (0-5 scale per route)
    // ID is route ID, level is familiarity level (0=Unknown, 5=Mastered)
    public List<FamiliarityEntry> RouteFamiliarity { get; set; } = new List<FamiliarityEntry>();

    // Location Familiarity System (Work Packet 1)
    // ID is location ID, level is familiarity level (0-3)
    public List<FamiliarityEntry> LocationFamiliarity { get; set; } = new List<FamiliarityEntry>();

    // Observation tracking - IDs of observation cards collected
    public List<string> CollectedObservations { get; set; } = new List<string>();

    public void AddKnownRoute(RouteOption route)
    {
        string originName = route.OriginLocationSpot;

        KnownRouteEntry routeEntry = KnownRoutes.FirstOrDefault(kr => kr.OriginSpotId == originName);
        if (routeEntry == null)
        {
            routeEntry = new KnownRouteEntry { OriginSpotId = originName };
            KnownRoutes.Add(routeEntry);
        }

        // Only add if not already known
        if (!routeEntry.Routes.Any(r => r.DestinationLocationSpot == route.DestinationLocationSpot))
        {
            routeEntry.Routes.Add(route);
        }
    }

    /// <summary>
    /// Get familiarity level for a route (0-5 scale)
    /// </summary>
    public int GetRouteFamiliarity(string routeId)
    {
        return RouteFamiliarity.GetFamiliarity(routeId);
    }

    /// <summary>
    /// Increase route familiarity after successful travel (max 5)
    /// </summary>
    public void IncreaseRouteFamiliarity(string routeId, int amount = 1)
    {
        int current = GetRouteFamiliarity(routeId);
        RouteFamiliarity.SetFamiliarity(routeId, Math.Min(5, current + amount));
    }

    /// <summary>
    /// Check if route is mastered (familiarity = 5)
    /// </summary>
    public bool IsRouteMastered(string routeId)
    {
        return GetRouteFamiliarity(routeId) >= 5;
    }

    /// <summary>
    /// Get familiarity level for a location (0-3 scale)
    /// </summary>
    public int GetLocationFamiliarity(string locationId)
    {
        return LocationFamiliarity.GetFamiliarity(locationId);
    }

    /// <summary>
    /// Set location familiarity to a specific value (max 3)
    /// </summary>
    public void SetLocationFamiliarity(string locationId, int value)
    {
        LocationFamiliarity.SetFamiliarity(locationId, Math.Min(3, Math.Max(0, value)));
    }

    public void AddMemory(string key, string description, int currentDay, int importance, int expirationDays = -1)
    {
        // Remove any existing memory with same key
        Memories.RemoveAll(m => m.Key == key);

        // Add new memory
        Memories.Add(new MemoryFlag
        {
            Key = key,
            Description = description,
            CreationDay = currentDay,
            ExpirationDay = expirationDays == -1 ? -1 : currentDay + expirationDays,
            Importance = importance
        });
    }

    public bool HasMemory(string key, int currentDay)
    {
        return Memories.Any(m => m.Key == key && m.IsActive(currentDay));
    }

    public MemoryFlag GetMemory(string key)
    {
        return Memories.FirstOrDefault(m => m.Key == key);
    }

    public Player()
    {
        Background = GameRules.StandardRuleset.Background;
        Inventory = new Inventory(10); // 10 weight capacity

        // HIGHLANDER PRINCIPLE: Delete hardcoded defaults
        // ALL values set by ApplyInitialConfiguration from JSON
        // These are minimal defaults ONLY for safety
        Coins = 0;
        Level = 1;
        CurrentXP = 0;
        XPToNextLevel = 100;

        // JSON will set these via ApplyInitialConfiguration
        MaxHealth = 100;
        MaxHunger = 100;

        // Skill cards removed - using letter queue system

        // Token system is purely relational (NPC-specific)
    }

    public void Initialize(string playerName, Professions selectedArchetype, Genders gender)
    {
        Name = playerName;
        Gender = gender;
        SetArchetype(selectedArchetype);

        HealFully();

        IsInitialized = true;
    }

    public void HealFully()
    {
        Health = MaxHealth;
        Hunger = 0; // Reset hunger to not hungry
    }


    public void SetArchetype(Professions archetype)
    {
        Archetype = archetype;

        switch (archetype)
        {
            case Professions.Soldier:
                InitializeGuard();
                break;
            case Professions.Merchant:
                InitializeMerchant();
                break;
            case Professions.Scholar:
                InitializeScholar();
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(archetype));
        }
    }

    private void InitializeGuard()
    {
        ClearInventory();
    }

    private void InitializeScholar()
    {
        ClearInventory();
    }

    private void InitializeMerchant()
    {
        ClearInventory();
    }

    private void ClearInventory()
    {
        Inventory.Clear();
    }

    public bool ModifyHealth(int count)
    {
        int newHealth = Math.Clamp(Health + count, 0, MaxHealth);
        if (newHealth != Health)
        {
            Health = newHealth;
            return true;
        }
        return false;
    }


    public bool ModifyHunger(int amount)
    {
        int newHunger = Math.Clamp(Hunger + amount, 0, MaxHunger);
        if (newHunger != Hunger)
        {
            Hunger = newHunger;
            return true;
        }
        return false;
    }

    public bool ReduceHunger(int amount)
    {
        // Eating reduces hunger
        return ModifyHunger(-amount);
    }

    public void AddExperiencePoints(int xpBonus)
    {
        int newExperiencePoints = Math.Max(0, CurrentXP + xpBonus);
        if (newExperiencePoints != CurrentXP)
        {
            CurrentXP = newExperiencePoints;
        }
    }

    public void AddCoins(int count)
    {
        int newCoins = Math.Max(0, Coins + count);
        if (newCoins != Coins)
        {
            Coins = newCoins;
        }
    }


    public void ModifyCoins(int amount)
    {
        Coins += amount;
        if (Coins < 0)
        {
            Coins = 0;
        }
    }



    internal void SetCoins(int value)
    {
        Coins = Math.Max(0, value);
    }


    /// <summary>
    /// Apply initial player configuration from package starting conditions
    /// </summary>
    public void ApplyInitialConfiguration(PlayerInitialConfig config)
    {
        if (config == null) return;

        if (config.Coins.HasValue) Coins = config.Coins.Value;
        if (config.Health.HasValue) Health = config.Health.Value;
        if (config.MaxHealth.HasValue) MaxHealth = config.MaxHealth.Value;
        if (config.Hunger.HasValue) Hunger = config.Hunger.Value;
        if (config.MaxHunger.HasValue) MaxHunger = config.MaxHunger.Value;

        // Apply satchel capacity - update inventory max weight
        if (config.SatchelCapacity.HasValue)
        {
            Inventory = new Inventory(config.SatchelCapacity.Value);
        }

        // SatchelWeight represents initial weight from starting obligations (Viktor's package)
        // This will be handled by the obligation system when adding starting obligations
    }

    /// <summary>
    /// Get current total weight (inventory + carried letters)
    /// </summary>
    public int GetCurrentWeight(ItemRepository itemRepository)
    {
        int inventoryWeight = Inventory.GetUsedWeight(itemRepository);
        int letterWeight = GetCarriedLetterWeight();
        return inventoryWeight + letterWeight;
    }

    /// <summary>
    /// Get weight from all carried letters (physical objects in satchel)
    /// </summary>
    public int GetCarriedLetterWeight()
    {
        int totalWeight = 0;
        foreach (Letter letter in CarriedLetters)
        {
            totalWeight += letter.GetWeight();
        }
        return totalWeight;
    }

    /// <summary>
    /// Check if player can carry additional weight
    /// </summary>
    public bool CanCarry(int additionalWeight, ItemRepository itemRepository)
    {
        int currentWeight = GetCurrentWeight(itemRepository);
        int maxWeight = Inventory.GetCapacity();
        return (currentWeight + additionalWeight) <= maxWeight;
    }

    /// <summary>
    /// Get current weight as ratio (for UI display)
    /// </summary>
    public (int current, int max) GetWeightStatus(ItemRepository itemRepository)
    {
        return (GetCurrentWeight(itemRepository), Inventory.GetCapacity());
    }

}

/// <summary>
/// Wrapper class for NPC connections that allows token manipulation.
/// CONTENT EFFICIENT: Works with existing token system.
/// </summary>
public class NPCConnection
{
    private readonly Player _player;
    private readonly string _npcId;
    private readonly ConnectionType _tokenType;

    public NPCConnection(Player player, string npcId, ConnectionType tokenType)
    {
        _player = player;
        _npcId = npcId;
        _tokenType = tokenType;
    }

    public int GetCurrentValue()
    {
        return _player.NPCTokens.GetTokenCount(_npcId, _tokenType);
    }

    public void AdjustValue(int amount)
    {
        // Get current value and adjust it
        int currentValue = _player.NPCTokens.GetTokenCount(_npcId, _tokenType);
        int newValue = Math.Max(0, currentValue + amount);

        // Set the new value
        _player.NPCTokens.SetTokenCount(_npcId, _tokenType, newValue);

        // Tokens are purely relational - no global count
    }
}

// Extension methods for Player
public static class PlayerExtensions
{
    public static void AddKnownLocation(this Player player, string locationId)
    {
        if (!player.DiscoveredLocationIds.Contains(locationId))
        {
            player.DiscoveredLocationIds.Add(locationId);
        }
    }

    public static void AddKnownLocationSpot(this Player player, string spotId)
    {
        // Track location spots in the LocationActionAvailability set
        if (!player.LocationActionAvailability.Contains(spotId))
        {
            player.LocationActionAvailability.Add(spotId);
        }
    }
}
