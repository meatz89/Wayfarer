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

    // Tier system (T1: Stranger, T2: Associate, T3: Confidant)
    public TierLevel CurrentTier { get; set; } = TierLevel.T1;

    // Resources
    public int Coins { get; set; } = 10; // Starting coins - intentionally kept as literal as it's game balance
    public int Stamina { get; set; } = 6; // Starting stamina - from GameConfiguration.StartingStamina
    public int Concentration { get; set; } = 10; // Starting concentration - intentionally kept as literal
    public int Health { get; set; }
    public int Food { get; set; }

    public int MaxStamina { get; set; } = 10;  // From GameConfiguration.MaxStamina
    public int MaxConcentration { get; set; }
    public int MinHealth { get; set; }
    public int MaxHealth { get; set; }


    public Inventory Inventory { get; set; } = new Inventory(6); // Starting inventory size - game balance

    // Relationships with characters
    public RelationshipList Relationships { get; set; } = new();


    // Location knowledge
    // Location knowledge - Moved from action system
    public HashSet<string> LocationActionAvailability { get; set; } = new HashSet<string>();

    public List<string> DiscoveredLocationIds { get; set; } = new List<string>();

    // Travel capabilities
    public List<string> UnlockedTravelMethods { get; set; } = new List<string>();


    public bool IsInitialized { get; set; } = false;

    public PlayerSkills Skills { get; private set; } = new();

    public LocationSpot CurrentLocationSpot { get; set; }
    public List<MemoryFlag> Memories { get; private set; } = new List<MemoryFlag>();

    /// <summary>
    /// Gets the current location derived from the current location spot.
    /// </summary>
    public Location GetCurrentLocation(LocationRepository locationRepository)
    {
        if (CurrentLocationSpot == null) return null;
        return locationRepository.GetLocation(CurrentLocationSpot.LocationId);
    }

    public Dictionary<string, List<RouteOption>> KnownRoutes { get; private set; } = new Dictionary<string, List<RouteOption>>();

    public DeliveryObligation[] ObligationQueue { get; private set; } = new DeliveryObligation[8];
    public List<MeetingObligation> MeetingObligations { get; set; } = new List<MeetingObligation>();
    public Dictionary<string, Dictionary<ConnectionType, int>> NPCTokens { get; private set; } = new Dictionary<string, Dictionary<ConnectionType, int>>();

    // Physical DeliveryObligation Carrying
    public List<Letter> CarriedLetters { get; private set; } = new List<Letter>(); // Letters physically in satchel for delivery
    public int MaxSatchelSize { get; set; } = 12; // Maximum size capacity for letters in satchel

    // Queue manipulation tracking
    public int LastMorningSwapDay { get; set; } = -1; // Track when morning swap was last used
    public int LastLetterBoardDay { get; set; } = -1; // Track when letter board was last generated
    public List<DeliveryObligation> DailyBoardLetters { get; set; } = new List<DeliveryObligation>(); // Store today's board letters

    // DeliveryObligation history tracking
    public Dictionary<string, LetterHistory> NPCLetterHistory { get; private set; } = new Dictionary<string, LetterHistory>();

    // Standing Obligations System
    public List<StandingObligation> StandingObligations { get; private set; } = new List<StandingObligation>();
    
    // PLAYER OBSERVATION DECK - Cards gained from exploring locations
    public CardDeck ObservationDeck { get; set; } = new();


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
    // Key is route ID, value is familiarity level (0=Unknown, 5=Mastered)
    public Dictionary<string, int> RouteFamiliarity { get; set; } = new Dictionary<string, int>();

    public void AddKnownRoute(RouteOption route)
    {
        string originName = route.OriginLocationSpot;

        if (!KnownRoutes.ContainsKey(originName))
        {
            KnownRoutes[originName] = new List<RouteOption>();
        }

        // Only add if not already known
        if (!KnownRoutes[originName].Any(r => r.DestinationLocationSpot == route.DestinationLocationSpot))
        {
            KnownRoutes[originName].Add(route);
        }
    }

    
    /// <summary>
    /// Get familiarity level for a route (0-5 scale)
    /// </summary>
    public int GetRouteFamiliarity(string routeId)
    {
        return RouteFamiliarity.TryGetValue(routeId, out int familiarity) ? familiarity : 0;
    }
    
    /// <summary>
    /// Increase route familiarity after successful travel (max 5)
    /// </summary>
    public void IncreaseRouteFamiliarity(string routeId, int amount = 1)
    {
        int current = GetRouteFamiliarity(routeId);
        RouteFamiliarity[routeId] = Math.Min(5, current + amount);
    }
    
    /// <summary>
    /// Check if route is mastered (familiarity = 5)
    /// </summary>
    public bool IsRouteMastered(string routeId)
    {
        return GetRouteFamiliarity(routeId) >= 5;
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

    public List<MemoryFlag> GetRecentMemories(int currentDay, int count = 5)
    {
        return Memories
            .Where(m => m.IsActive(currentDay))
            .OrderByDescending(m => m.Importance)
            .ThenByDescending(m => m.CreationDay)
            .Take(count)
            .ToList();
    }

    public Player()
    {
        Background = GameRules.StandardRuleset.Background;
        Inventory = new Inventory(10);

        Coins = 5;
        Level = 1;
        CurrentXP = 0;
        XPToNextLevel = 100;

        // Set max values that match initial values
        MaxConcentration = 10; // Match initial Concentration = 10
        MaxHealth = 10; // Set reasonable default for MaxHealth

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
        Stamina = MaxStamina;
        Health = MaxHealth;
        Concentration = MaxConcentration;
    }


    public void SetArchetype(Professions archetype)
    {
        this.Archetype = archetype;

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

    public bool ModifyConcentration(int count)
    {
        int newConcentration = Math.Clamp(Concentration + count, 0, MaxConcentration);
        if (newConcentration != Concentration)
        {
            Concentration = newConcentration;
            return true;
        }
        return false;
    }

    public bool ModifyFood(int amount)
    {
        int newFood = Math.Max(0, Food + amount);
        if (newFood != Food)
        {
            Food = newFood;
            return true;
        }
        return false;
    }

    public bool ConsumeFood(int amount)
    {
        if (Food >= amount)
        {
            Food -= amount;
            return true;
        }
        return false;
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


    public int GetRelationshipLevel(string character)
    {
        return 1;
    }

    public void UpdateRelationship(object characterId, object delta)
    {
    }


    public void SetNewStamina(int newStamina)
    {
        this.Stamina = Math.Clamp(newStamina, 0, MaxStamina);

    }

    public int GetSkillLevel(SkillTypes skill)
    {
        int level = Skills.GetLevelForSkill(skill);
        return level;
    }

    public void AddSilver(int silverReward)
    {
        // Silver is now represented as Coins
        this.Coins += silverReward;
    }

    public void AddInsightPoints(int insightPointReward)
    {
        // Insight points removed from new design - use memories instead
        if (insightPointReward > 0)
        {
            // Use a generic key since we don't have access to GameWorld here
            AddMemory($"insight_{Guid.NewGuid()}", $"Gained {insightPointReward} insight", 0, insightPointReward);
        }
    }

    public Player Serialize()
    {
        // Create a new PlayerState instance
        Player clone = new Player();

        // Copy simple properties
        clone.Name = this.Name;
        clone.Gender = this.Gender;
        clone.Background = this.Background;
        clone.Archetype = this.Archetype;
        clone.Level = this.Level;
        clone.CurrentXP = this.CurrentXP;
        clone.XPToNextLevel = this.XPToNextLevel;
        clone.MaxStamina = this.MaxStamina;
        clone.Stamina = this.Stamina;
        clone.Coins = this.Coins;
        clone.Food = this.Food;
        clone.MinHealth = this.MinHealth;
        clone.Health = this.Health;
        clone.Concentration = this.Concentration;
        clone.MaxHealth = this.MaxHealth;
        clone.MaxConcentration = this.MaxConcentration;
        clone.IsInitialized = this.IsInitialized;
        clone.CurrentLocationSpot = this.CurrentLocationSpot;

        // Deep copy Inventory
        clone.Inventory = new Inventory(this.Inventory.Size);
        foreach (string item in this.Inventory.GetAllItems())
        {
            clone.Inventory.AddItem(item);
        }

        // Deep copy of RelationshipList
        clone.Relationships = this.Relationships.Clone();


        // Deep copy of location knowledge
        clone.DiscoveredLocationIds = [.. this.DiscoveredLocationIds];

        // Deep copy of travel methods
        clone.UnlockedTravelMethods = [.. this.UnlockedTravelMethods];

        // Deep copy of LocationActionAvailability HashSet
        clone.LocationActionAvailability = [.. this.LocationActionAvailability];

        // Deep copy of Skills
        clone.Skills = this.Skills.Clone();

        return clone;
    }

    public void ModifyCoins(int amount)
    {
        Coins += amount;
        if (Coins < 0)
        {
            Coins = 0;
        }
    }

    public bool SpendStamina(int amount)
    {
        if (Stamina < amount) return false;

        Stamina -= amount;


        return true;
    }

    public bool SpendMoney(int amount)
    {
        if (Coins < amount) return false;

        Coins -= amount;
        return true;
    }

    public int GetMaxItemCapacity()
    {
        return Inventory.Size;
    }

    public bool HasVisitedLocation(string requiredLocation)
    {
        return false;
    }

    // === CATEGORICAL STAMINA SYSTEM ===
    // Implementation of deterministic categorical stamina states with hard gates

    /// <summary>
    /// Modifies stamina and updates categorical physical condition
    /// </summary>
    public bool ModifyStamina(int amount)
    {
        int newStamina = Math.Clamp(Stamina + amount, 0, MaxStamina);
        if (newStamina != Stamina)
        {
            Stamina = newStamina;

            return true;
        }
        return false;
    }

    /// <summary>
    /// Checks if player can perform dangerous route travel (requires 4+ stamina)
    /// </summary>
    public bool CanPerformDangerousTravel()
    {
        return Stamina >= 4;
    }

    /// <summary>
    /// Checks if player can perform noble social conversations (requires 3+ stamina)
    /// </summary>
    public bool CanPerformNobleSocialConversation()
    {
        return Stamina >= 3;
    }

    /// <summary>
    /// Applies categorical stamina recovery based on lodging type
    /// </summary>
    public void ApplyCategoricalStaminaRecovery(string lodgingCategory)
    {
        int recoveryAmount = lodgingCategory.ToLower() switch
        {
            "rough" => 2,           // Rough lodging grants 2 points
            "common" => 4,          // Common lodging grants 4 points
            "private" => 6,         // Private lodging grants 6 points
            "noble" => 8,           // Noble invitation grants 8 points
            "noble_invitation" => 8,
            _ => 2                  // Default to rough recovery
        };

        ModifyStamina(recoveryAmount);
    }

    internal void SetCoins(int value)
    {
        Coins = Math.Max(0, value);
    }

    internal void SetStamina(int value)
    {
        Stamina = Math.Clamp(value, 0, MaxStamina);
    }


    /// <summary>
    /// Get connection tokens with a specific NPC.
    /// Returns a connection object that can be modified.
    /// </summary>
    public NPCConnection GetConnection(string npcId, ConnectionType tokenType)
    {
        // Initialize NPC token tracking if needed
        if (!NPCTokens.ContainsKey(npcId))
        {
            NPCTokens[npcId] = new Dictionary<ConnectionType, int>();
            foreach (ConnectionType type in Enum.GetValues<ConnectionType>())
            {
                NPCTokens[npcId][type] = 0;
            }
        }

        // Return a connection wrapper that can be modified
        return new NPCConnection(this, npcId, tokenType);
    }

    /// <summary>
    /// Get total token count across all NPCs and types.
    /// Used for progression and special letter generation.
    /// Aggregates from NPC-specific tokens only.
    /// </summary>
    public int GetTotalTokenCount()
    {
        int total = 0;
        foreach (Dictionary<ConnectionType, int> tokenCounts in NPCTokens.Values)
        {
            // Only count positive tokens (negative represents leverage/debt)
            total += tokenCounts.Values.Where(v => v > 0).Sum();
        }
        return total;
    }

    /// <summary>
    /// Updates player tier based on deliveries and trust tokens.
    /// T1 (Stranger): Default starting tier
    /// T2 (Associate): 5+ deliveries OR 10+ total tokens
    /// T3 (Confidant): 15+ deliveries AND 25+ total tokens
    /// </summary>
    public void UpdateTier()
    {
        int totalTokens = GetTotalTokenCount();
        int totalDeliveries = TotalLettersDelivered;

        TierLevel newTier = TierLevel.T1;

        // Check for T3 (Confidant) - requires both deliveries AND tokens
        if (totalDeliveries >= 15 && totalTokens >= 25)
        {
            newTier = TierLevel.T3;
        }
        // Check for T2 (Associate) - requires deliveries OR tokens
        else if (totalDeliveries >= 5 || totalTokens >= 10)
        {
            newTier = TierLevel.T2;
        }

        CurrentTier = newTier;
    }

    /// <summary>
    /// Get the display name for the current tier.
    /// </summary>
    public string GetTierDisplayName()
    {
        return CurrentTier switch
        {
            TierLevel.T1 => "Stranger",
            TierLevel.T2 => "Associate",
            TierLevel.T3 => "Confidant",
            _ => "Stranger"
        };
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
        if (_player.NPCTokens.ContainsKey(_npcId) &&
            _player.NPCTokens[_npcId].ContainsKey(_tokenType))
        {
            return _player.NPCTokens[_npcId][_tokenType];
        }
        return 0;
    }

    public void AdjustValue(int amount)
    {
        // Ensure NPC tracking exists
        if (!_player.NPCTokens.ContainsKey(_npcId))
        {
            _player.NPCTokens[_npcId] = new Dictionary<ConnectionType, int>();
            foreach (ConnectionType type in Enum.GetValues<ConnectionType>())
            {
                _player.NPCTokens[_npcId][type] = 0;
            }
        }

        // Adjust the token value
        int currentValue = _player.NPCTokens[_npcId].GetValueOrDefault(_tokenType, 0);
        _player.NPCTokens[_npcId][_tokenType] = Math.Max(0, currentValue + amount);

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
