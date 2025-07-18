public class Player
{
    // Core identity
    public string Name { get; set; }
    public Genders Gender { get; private set; }
    public string Background { get; set; }

    // Archetype
    public Professions Archetype { get; set; }

    // Progression systems
    public int Level { get; set; } = 1;
    public int CurrentXP { get; set; } = 0;
    public int XPToNextLevel { get; set; } = 100;

    // Resources
    public int Coins { get; set; } = 10;
    public int Stamina { get; set; } = 6;
    public int Concentration { get; set; } = 10;
    public int Reputation { get; set; } = 0;
    public int Health { get; set; }
    public int Food { get; set; }

    public int MaxStamina { get; set; } = 10;  // Changed to 10 to match 0-10 scale
    public int MaxConcentration { get; set; }
    public int MinHealth { get; set; }
    public int MaxHealth { get; set; }

    // Categorical Physical State
    public PhysicalCondition CurrentPhysicalCondition { get; private set; } = PhysicalCondition.Good;

    public Inventory Inventory { get; set; } = new Inventory(6);

    // Relationships with characters
    public RelationshipList Relationships { get; set; } = new();


    // Location knowledge
    public List<string> DiscoveredLocationIds { get; set; } = new List<string>();

    // Travel capabilities
    public List<string> UnlockedTravelMethods { get; set; } = new List<string>();
    
    // Network tracking
    public List<string> UnlockedNPCIds { get; set; } = new List<string>();


    public HashSet<(string, SkillCategories)> LocationActionAvailability { get; set; } = new();

    public bool IsInitialized { get; set; } = false;

    public List<string> KnownLocations { get; private set; } = new List<string>();
    public List<string> KnownLocationSpots { get; private set; } = new List<string>();
    public PlayerSkills Skills { get; private set; } = new();

    // Card collection (player skills)
    public List<SkillCard> PlayerSkillCards { get; set; } = new List<SkillCard>();
    public List<SkillCard> AvailableCards { get; set; } = new List<SkillCard>();
    public Location CurrentLocation { get; set; }
    public LocationSpot CurrentLocationSpot { get; set; }
    public List<MemoryFlag> Memories { get; private set; } = new List<MemoryFlag>();
    public int CurrentDay { get; private set; }


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
    
    // Letter history tracking
    public Dictionary<string, LetterHistory> NPCLetterHistory { get; private set; } = new Dictionary<string, LetterHistory>();
    
    // Standing Obligations System
    public List<StandingObligation> StandingObligations { get; private set; } = new List<StandingObligation>();
    
    // Token Favor System
    public List<string> PurchasedFavors { get; set; } = new List<string>();
    public List<string> UnlockedLocationIds { get; set; } = new List<string>();
    public List<string> UnlockedServices { get; set; } = new List<string>();
    
    // Scenario tracking
    public List<Letter> DeliveredLetters { get; set; } = new List<Letter>();
    public int TotalLettersDelivered { get; set; } = 0;
    public int TotalLettersExpired { get; set; } = 0;
    public int TotalTokensSpent { get; set; } = 0;

    public void AddGoal(Goal goal)
    {
        ActiveGoals.Add(goal);
    }

    public void UpdateGoals()
    {
        for (int i = ActiveGoals.Count - 1; i >= 0; i--)
        {
            Goal goal = ActiveGoals[i];

            if (goal.IsCompleted)
            {
                ActiveGoals.RemoveAt(i);
                CompletedGoals.Add(goal);
            }
            else if (goal.CheckFailure(CurrentDay))
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




    public void AddMemory(string key, string description, int importance, int expirationDays = -1)
    {
        // Remove any existing memory with same key
        Memories.RemoveAll(m => m.Key == key);

        // Add new memory
        Memories.Add(new MemoryFlag
        {
            Key = key,
            Description = description,
            CreationDay = CurrentDay,
            ExpirationDay = expirationDays == -1 ? -1 : CurrentDay + expirationDays,
            Importance = importance
        });
    }

    public bool HasMemory(string key)
    {
        return Memories.Any(m => m.Key == key && m.IsActive(CurrentDay));
    }

    public List<MemoryFlag> GetRecentMemories(int count = 5)
    {
        return Memories
            .Where(m => m.IsActive(CurrentDay))
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

        AvailableCards = new List<SkillCard>();

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
            case Professions.Thief:
                InitializeExplorer();
                break;
            case Professions.Ranger:
                InitializeThief();
                break;
            case Professions.Courtier:
                InitializeCourtier();
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(archetype));
        }
    }

    private void InitializeGuard()
    {
        // Inventory
        ClearInventory();
    }

    private void InitializeCourtier()
    {
        ClearInventory();
    }

    private void InitializeScholar()
    {
        ClearInventory();
    }

    private void InitializeExplorer()
    {
        ClearInventory();
    }

    private void InitializeThief()
    {
        ClearInventory();
    }

    private void InitializeMerchant()
    {
        ClearInventory();
    }

    public bool HasAvailableCard(SkillCategories cardTypes)
    {
        foreach (SkillCard card in AvailableCards)
        {
            if (card.Category == cardTypes && !card.IsExhausted)
            {
                return true;
            }
        }
        return false;
    }

    public void ExhaustCard(SkillCard card)
    {
        card.Exhaust();
    }

    public void RefreshCard(SkillCard card)
    {
        card.Refresh();
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

    public void UnlockCard(SkillCard card)
    {
        PlayerSkillCards.Add(card);
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
        this.Stamina = newStamina;
    }

    // ModifyStamina moved to categorical stamina system section below

    public bool HasNonExhaustedCardOfType(SkillCategories requiredCardType)
    {
        return true;
    }

    public int GetSkillLevel(SkillTypes skill)
    {
        int level = Skills.GetLevelForSkill(skill);
        return level;
    }

    public SkillCard GetSelectedCardForSkill(SkillTypes skill)
    {
        return null;
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

        // Deep copy of card collections
        clone.PlayerSkillCards = [.. this.PlayerSkillCards];
        clone.PlayerSkillCards = [.. this.PlayerSkillCards];

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

    public SkillCard GetBestNonExhaustedCardOfType(SkillCategories requiredCardType)
    {
        throw new NotImplementedException();
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

    public List<SkillCard> GetAvailableCardsByType(SkillCategories requiredCardType)
    {
        return new List<SkillCard>(AvailableCards);
    }

    public List<SkillCard> GetAllAvailableCards()
    {
        return new List<SkillCard>(AvailableCards);
    }

    public SkillCard FindCard(string skillName)
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

    public List<SkillCard> GetCardsOfType(SkillCategories requiredApproach)
    {
        return new List<SkillCard>();
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
            UpdatePhysicalCondition();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Updates the categorical physical condition based on current stamina level
    /// </summary>
    private void UpdatePhysicalCondition()
    {
        CurrentPhysicalCondition = Stamina switch
        {
            >= 9 => PhysicalCondition.Excellent,   // 9-10: Peak performance
            >= 7 => PhysicalCondition.Good,        // 7-8: Normal condition  
            >= 5 => PhysicalCondition.Tired,       // 5-6: Somewhat fatigued
            >= 3 => PhysicalCondition.Exhausted,   // 3-4: Significantly fatigued
            >= 1 => PhysicalCondition.Injured,     // 1-2: Physical impairment
            _ => PhysicalCondition.Sick             // 0: Complete exhaustion
        };
    }

    /// <summary>
    /// Checks if player meets stamina requirements for categorical actions
    /// </summary>
    public bool CanPerformStaminaAction(PhysicalDemand physicalDemand)
    {
        return physicalDemand switch
        {
            PhysicalDemand.None => true,                    // Anyone can perform non-physical actions
            PhysicalDemand.Light => Stamina >= 2,           // Requires 2+ stamina
            PhysicalDemand.Moderate => Stamina >= 4,        // Requires 4+ stamina  
            PhysicalDemand.Heavy => Stamina >= 6,           // Requires 6+ stamina
            PhysicalDemand.Extreme => Stamina >= 8,         // Requires 8+ stamina
            _ => false
        };
    }

    /// <summary>
    /// Checks if player can perform dangerous route travel (requires 4+ stamina)
    /// </summary>
    public bool CanPerformDangerousTravel()
    {
        return Stamina >= 4;
    }

    /// <summary>
    /// Checks if player can perform noble social encounters (requires 3+ stamina)
    /// </summary>
    public bool CanPerformNobleSocialEncounter()
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

    /// <summary>
    /// Applies categorical stamina costs for different action types
    /// </summary>
    public bool ApplyCategoricalStaminaCost(PhysicalDemand physicalDemand)
    {
        int staminaCost = physicalDemand switch
        {
            PhysicalDemand.None => 0,       // No stamina cost
            PhysicalDemand.Light => 1,      // Light work costs 1 stamina
            PhysicalDemand.Moderate => 2,   // Moderate work costs 2 stamina
            PhysicalDemand.Heavy => 3,      // Heavy work costs 3 stamina
            PhysicalDemand.Extreme => 4,    // Extreme work costs 4 stamina
            _ => 0
        };

        if (Stamina >= staminaCost)
        {
            ModifyStamina(-staminaCost);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Gets description of current physical condition for UI display
    /// </summary>
    public string GetPhysicalConditionDescription()
    {
        return CurrentPhysicalCondition switch
        {
            PhysicalCondition.Excellent => "Excellent - Peak physical performance",
            PhysicalCondition.Good => "Good - Normal physical condition",
            PhysicalCondition.Tired => "Tired - Somewhat fatigued, limited heavy work",
            PhysicalCondition.Exhausted => "Exhausted - Significantly fatigued, no heavy work",
            PhysicalCondition.Injured => "Injured - Physical impairment, basic actions only",
            PhysicalCondition.Sick => "Sick - Complete exhaustion, rest required",
            _ => "Unknown condition"
        };
    }
}
