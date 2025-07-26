using Wayfarer.GameState.Constants;

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
    public int XPToNextLevel { get; set; } = GameConstants.Game.XP_TO_NEXT_LEVEL_BASE;

    // Resources
    public int Coins { get; set; } = 10; // Starting coins - intentionally kept as literal as it's game balance
    public int Stamina { get; set; } = 6; // Starting stamina - from GameConfiguration.StartingStamina
    public int Concentration { get; set; } = 10; // Starting concentration - intentionally kept as literal
    public int Reputation { get; set; } = 0;
    public int Health { get; set; }
    public int Food { get; set; }
    public int PatronLeverage { get; set; } = 0;
    public bool HasPatron { get; set; } = false;
    public int LastPatronFundDay { get; set; } = -7; // Allow immediate request on game start - intentionally negative

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

    // Network tracking
    public List<string> UnlockedNPCIds { get; set; } = new List<string>();



    public bool IsInitialized { get; set; } = false;

    public List<string> KnownLocations { get; private set; } = new List<string>();
    public List<string> KnownLocationSpots { get; private set; } = new List<string>();
    public PlayerSkills Skills { get; private set; } = new();

    public Location CurrentLocation { get; set; }
    public LocationSpot CurrentLocationSpot { get; set; }
    public List<MemoryFlag> Memories { get; private set; } = new List<MemoryFlag>();

    public Dictionary<string, List<RouteOption>> KnownRoutes { get; private set; } = new Dictionary<string, List<RouteOption>>();

    public List<string> KnownContracts { get; private set; } = new List<string>();

    public List<Goal> ActiveGoals { get; private set; } = new List<Goal>();
    public List<Goal> CompletedGoals { get; private set; } = new List<Goal>();
    public List<Goal> FailedGoals { get; private set; } = new List<Goal>();

    // Letter Queue System
    public Letter[] LetterQueue { get; private set; } = new Letter[8];
    public Dictionary<ConnectionType, int> ConnectionTokens { get; private set; } = new Dictionary<ConnectionType, int>();
    public Dictionary<string, Dictionary<ConnectionType, int>> NPCTokens { get; private set; } = new Dictionary<string, Dictionary<ConnectionType, int>>();

    // Physical Letter Carrying
    public List<Letter> CarriedLetters { get; private set; } = new List<Letter>(); // Letters physically in inventory for delivery

    // Queue manipulation tracking
    public int LastMorningSwapDay { get; set; } = -1; // Track when morning swap was last used
    public int LastLetterBoardDay { get; set; } = -1; // Track when letter board was last generated
    public List<Letter> DailyBoardLetters { get; set; } = new List<Letter>(); // Store today's board letters

    // Letter history tracking
    public Dictionary<string, LetterHistory> NPCLetterHistory { get; private set; } = new Dictionary<string, LetterHistory>();

    // Standing Obligations System
    public List<StandingObligation> StandingObligations { get; private set; } = new List<StandingObligation>();

    // Token Favor System
    public List<string> PurchasedFavors { get; set; } = new List<string>();
    public List<string> UnlockedLocationIds { get; set; } = new List<string>();
    public List<string> UnlockedServices { get; set; } = new List<string>();

    // Collapse callback - set by GameWorldManager
    public Action OnStaminaExhausted { get; set; }

    // Scenario tracking
    public List<Letter> DeliveredLetters { get; set; } = new List<Letter>();
    public int TotalLettersDelivered { get; set; } = 0;
    public int TotalLettersExpired { get; set; } = 0;
    public int TotalTokensSpent { get; set; } = 0;

    public void AddGoal(Goal goal)
    {
        ActiveGoals.Add(goal);
    }

    public void UpdateGoals(int currentDay)
    {
        for (int i = ActiveGoals.Count - 1; i >= 0; i--)
        {
            Goal goal = ActiveGoals[i];

            if (goal.IsCompleted)
            {
                ActiveGoals.RemoveAt(i);
                CompletedGoals.Add(goal);
            }
            else if (goal.CheckFailure(currentDay))
            {
                ActiveGoals.RemoveAt(i);
                FailedGoals.Add(goal);
            }
        }
    }

    public List<Goal> GetGoalsByType(GoalType type)
    {
        return ActiveGoals.Where(g => g.Type == type).ToList();
    }


    public void AddKnownRoute(RouteOption route)
    {
        string originName = route.Origin;

        if (!KnownRoutes.ContainsKey(originName))
        {
            KnownRoutes[originName] = new List<RouteOption>();
        }

        // Only add if not already known
        if (!KnownRoutes[originName].Any(r => r.Destination == route.Destination))
        {
            KnownRoutes[originName].Add(route);
        }
    }


    public void DiscoverContract(string contractId)
    {
        if (!KnownContracts.Contains(contractId))
        {
            KnownContracts.Add(contractId);
        }
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

        // Initialize letter queue system
        foreach (ConnectionType tokenType in Enum.GetValues<ConnectionType>())
        {
            ConnectionTokens[tokenType] = 0;
        }
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


    public void AddKnownLocation(string location)
    {
        KnownLocations.Add(location);
    }

    public void AddKnownLocationSpot(string locationSpot)
    {
        KnownLocationSpots.Add(locationSpot);
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
        
        // Trigger collapse check if stamina reaches 0
        if (Stamina == 0 && OnStaminaExhausted != null)
        {
            OnStaminaExhausted();
        }
    }

    // ModifyStamina moved to categorical stamina system section below


    public int GetSkillLevel(SkillTypes skill)
    {
        int level = Skills.GetLevelForSkill(skill);
        return level;
    }


    public void AddSilver(int silverReward)
    {
        throw new NotImplementedException();
    }

    public void AddReputation(int reputationReward)
    {
        throw new NotImplementedException();
    }

    public void AddInsightPoints(int insightPointReward)
    {
        throw new NotImplementedException();
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
        clone.CurrentLocation = this.CurrentLocation;
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
        clone.KnownLocations = [.. this.KnownLocations];
        clone.KnownLocationSpots = [.. this.KnownLocationSpots];
        clone.KnownContracts = [.. this.KnownContracts];

        // Deep copy of travel methods
        clone.UnlockedTravelMethods = [.. this.UnlockedTravelMethods];

        // Deep copy of LocationActionAvailability HashSet
        clone.LocationActionAvailability = [.. this.LocationActionAvailability];

        // Deep copy of Skills
        clone.Skills = this.Skills.Clone();

        return clone;
    }


    public void ModifyRelationship(string id, int amount, string source)
    {
        throw new NotImplementedException();
    }

    public void ModifyCoins(int amount)
    {
        Coins += amount;
        if (Coins < 0)
        {
            Coins = 0;
        }
    }

    public void AddKnowledge(object knowledgeItem)
    {
        throw new NotImplementedException();
    }

    public void ModifyCurrency(object amount)
    {
        throw new NotImplementedException();
    }


    public int GetRelationship(object iD)
    {
        throw new NotImplementedException();
    }

    public void SetRelationship(string iD, int newRelationship)
    {
        throw new NotImplementedException();
    }


    public ReputationLevel GetReputationLevel()
    {
        if (Reputation >= 75) return ReputationLevel.Revered;
        if (Reputation >= 50) return ReputationLevel.Respected;
        if (Reputation >= 25) return ReputationLevel.Trusted;
        if (Reputation >= 0) return ReputationLevel.Neutral;
        if (Reputation >= -25) return ReputationLevel.Suspicious;
        if (Reputation >= -50) return ReputationLevel.Distrusted;
        return ReputationLevel.Hated;
    }

    public bool HasItem(string equipment)
    {
        return Inventory.HasItem(equipment);
    }

    public bool SpendStamina(int amount)
    {
        if (Stamina < amount) return false;

        Stamina -= amount;

        // Trigger collapse check if stamina reaches 0
        if (Stamina == 0 && OnStaminaExhausted != null)
        {
            OnStaminaExhausted();
        }

        return true;
    }

    public bool SpendMoney(int amount)
    {
        // Reputation no longer affects prices - emergent gameplay instead
        // High reputation affects: contract availability, credit access, information sharing

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
            
            // Trigger collapse check if stamina reaches 0
            if (Stamina == 0 && OnStaminaExhausted != null)
            {
                OnStaminaExhausted();
            }
            
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
        throw new NotImplementedException();
    }

    internal void SetStamina(int value)
    {
        throw new NotImplementedException();
    }
}
