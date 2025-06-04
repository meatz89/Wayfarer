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
    public int Money { get; set; } = 10;
    public int ActionPoints { get; set; } = 18;
    public int Energy { get; set; } = 10;
    public int Concentration { get; set; } = 10;
    public int Reputation { get; set; } = 0;
    public int Health { get; set; }
    public int Food { get; set; }

    public int MaxActionPoints { get; set; } = 4;
    public int MaxEnergy { get; set; } = 12;
    public int MaxConcentration { get; set; }
    public int MinHealth { get; set; }
    public int MaxHealth { get; set; }


    public Inventory Inventory { get; set; } = new Inventory(10);

    // Relationships with characters
    public RelationshipList Relationships { get; set; } = new();


    // Location knowledge
    public List<string> DiscoveredLocationIds { get; set; } = new List<string>();

    // Travel capabilities
    public List<string> UnlockedTravelMethods { get; set; } = new List<string>();


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

    public List<InformationItem> KnownInformation { get; private set; } = new List<InformationItem>();

    public Dictionary<string, List<TravelRoute>> KnownRoutes { get; private set; } = new Dictionary<string, List<TravelRoute>>();

    public List<Goal> ActiveGoals { get; private set; } = new List<Goal>();
    public List<Goal> CompletedGoals { get; private set; } = new List<Goal>();
    public List<Goal> FailedGoals { get; private set; } = new List<Goal>();

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


    public void AddKnownRoute(TravelRoute route)
    {
        string originName = route.Origin.Name;

        if (!KnownRoutes.ContainsKey(originName))
        {
            KnownRoutes[originName] = new List<TravelRoute>();
        }

        // Only add if not already known
        if (!KnownRoutes[originName].Any(r => r.Destination.Name == route.Destination.Name))
        {
            KnownRoutes[originName].Add(route);
        }
    }

    public void LearnInformation(InformationItem item)
    {
        if (!KnownInformation.Any(i => i.Key == item.Key))
        {
            KnownInformation.Add(item);
        }
    }

    public bool KnowsInformation(string key)
    {
        return KnownInformation.Any(i => i.Key == key);
    }

    public List<InformationItem> GetInformationByTag(string tag)
    {
        return KnownInformation.Where(i => i.Tags.Contains(tag)).ToList();
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

        Money = 5;
        Level = 1;
        CurrentXP = 0;
        XPToNextLevel = 100;

        AvailableCards = new List<SkillCard>();

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
        Energy = MaxEnergy;
        Health = MaxHealth;
        Concentration = MaxConcentration;
    }


    public void SetArchetype(Professions archetype)
    {
        this.Archetype = archetype;

        switch (archetype)
        {
            case Professions.Warrior:
                InitializeGuard();
                break;
            case Professions.Diplomat:
                InitializeDiplomat();
                break;
            case Professions.Scholar:
                InitializeScholar();
                break;
            case Professions.Mystic:
                InitializeExplorer();
                break;
            case Professions.Ranger:
                InitializeRogue();
                break;
            case Professions.Courtier:
                InitializeMerchant();
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(archetype));
        }
    }

    private void InitializeGuard()
    {
        // Inventory
        ClearInventory();
        Inventory.AddItem(ItemTypes.Hammer);
        Inventory.AddItem(ItemTypes.Chisel);
        Inventory.AddItem(ItemTypes.Trowel);
        Inventory.AddItem(ItemTypes.Mortar);
    }

    private void InitializeDiplomat()
    {
        ClearInventory();
        Inventory.AddItem(ItemTypes.FineClothes);
        Inventory.AddItem(ItemTypes.WaxSealKit);
        Inventory.AddItem(ItemTypes.Perfume);
        Inventory.AddItem(ItemTypes.SilverCoins);
        Inventory.AddItem(ItemTypes.WineFlask);
    }

    private void InitializeScholar()
    {
        ClearInventory();
        Inventory.AddItem(ItemTypes.Journal);
        Inventory.AddItem(ItemTypes.QuillAndInk);
        Inventory.AddItem(ItemTypes.Spectacles);
        Inventory.AddItem(ItemTypes.PuzzleBox);
        Inventory.AddItem(ItemTypes.AncientText);
    }

    private void InitializeExplorer()
    {
        ClearInventory();
        Inventory.AddItem(ItemTypes.HerbSatchel);
        Inventory.AddItem(ItemTypes.MortarAndPestle);
        Inventory.AddItem(ItemTypes.FieldGuide);
    }

    private void InitializeRogue()
    {
        ClearInventory();
        Inventory.AddItem(ItemTypes.Lockpicks);
        Inventory.AddItem(ItemTypes.DarkCloak);
        Inventory.AddItem(ItemTypes.GrapplingHook);
        Inventory.AddItem(ItemTypes.PoisonVial);
        Inventory.AddItem(ItemTypes.DisguiseKit);
    }

    private void InitializeMerchant()
    {
        ClearInventory();
        Inventory.AddItem(ItemTypes.Ledger);
        Inventory.AddItem(ItemTypes.Scales);
        Inventory.AddItem(ItemTypes.TradeGoods);
        Inventory.AddItem(ItemTypes.CoinPurse);
        Inventory.AddItem(ItemTypes.TradeDocuments);
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
        int newCoins = Math.Max(0, Money + count);
        if (newCoins != Money)
        {
            Money = newCoins;
        }
    }

    public void UnlockCard(SkillCard card)
    {
        PlayerSkillCards.Add(card);
    }

    public void AddKnownLocation(string location)
    {
        KnownLocationSpots.Add(location);
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

    public void ApplyActionPointCost(int actionPointCost)
    {
        ActionPoints -= actionPointCost;
    }

    public int CurrentActionPoints()
    {
        return ActionPoints;
    }

    public void ModifyActionPoints(int amount)
    {
        int newActionPoints = Math.Clamp(ActionPoints + amount, 0, MaxActionPoints);
        this.ActionPoints = newActionPoints;
    }

    public int CurrentEnergy()
    {
        return Energy;
    }

    public void SetNewEnergy(int newEnergy)
    {
        this.Energy = newEnergy;
    }

    public void ModifyEnergy(int amount)
    {
        this.Energy += amount;
    }

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
        clone.MaxActionPoints = this.MaxActionPoints;
        clone.ActionPoints = this.ActionPoints;
        clone.MaxEnergy = this.MaxEnergy;
        clone.Energy = this.Energy;
        clone.Money = this.Money;
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
        clone.Inventory = new Inventory(this.Inventory.Capacity);
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
        throw new NotImplementedException();
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


    // Resource modification with interdependence
    public bool SpendEnergy(int amount)
    {
        if (Energy < amount) return false;

        Energy -= amount;

        // Automatic time advancement based on energy expenditure
        GameWorld.AdvanceTime(TimeSpan.FromHours(amount));

        return true;
    }

    public bool SpendMoney(int amount)
    {
        // Reputation affects costs
        int actualCost = CalculateAdjustedCost(amount);

        if (Money < actualCost) return false;

        Money -= actualCost;
        return true;
    }

    private int CalculateAdjustedCost(int baseCost)
    {
        // Higher reputation means lower costs
        float multiplier = 1.0f;

        if (Reputation >= 75) multiplier = 0.8f;
        else if (Reputation >= 50) multiplier = 0.9f;
        else if (Reputation >= 25) multiplier = 0.95f;
        else if (Reputation >= 0) multiplier = 1.0f;
        else if (Reputation >= -25) multiplier = 1.1f;
        else if (Reputation >= -50) multiplier = 1.25f;
        else multiplier = 1.5f;

        return (int)(baseCost * multiplier);
    }
}
