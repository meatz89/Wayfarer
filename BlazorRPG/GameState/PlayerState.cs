public class PlayerState
{
    // Core identity
    public string Name { get; set; }
    public string Background { get; set; }

    // Archetype
    public ArchetypeTypes Archetype { get; set; }
    public ArchetypeConfig ArchetypeConfig { get; set; }

    // Progression systems
    public int Level { get; set; }
    public int ExperiencePoints { get; set; }
    public SkillList Skills { get; set; } = new();

    // Resources
    public int Money { get; set; }
    public int Food { get; set; }
    public int MedicinalHerbs { get; set; }
    public Inventory Inventory { get; set; } = new Inventory(10);

    // Encounter resources (reset at start of encounters)
    public int MinHealth { get; set; }
    public int Health { get; set; }
    public int MaxHealth { get; set; }
    public int Concentration { get; set; }
    public int MaxConcentration { get; set; }
    public int Confidence { get; set; }
    public int MaxConfidence { get; set; }

    public int Energy { get; set; }
    public int MaxEnergy { get; set; }

    // Relationships with characters
    public RelationshipList Relationships { get; set; } = new();

    // Card collection (player skills)
    public List<ChoiceCard> UnlockedCards { get; set; } = new List<ChoiceCard>();

    // Location knowledge
    public List<string> DiscoveredLocationIds { get; set; } = new List<string>();

    // Travel capabilities
    public List<string> UnlockedTravelMethods { get; set; } = new List<string>();

    public int Coins { get; set; }

    public Equipment Equipment { get; set; }
    public List<KnowledgePiece> Knowledge { get; set; } = new();
    public List<string> KnownLocations { get; set; } = new();
    public HashSet<(string, BasicActionTypes)> LocationActionAvailability { get; set; } = new();

    public List<PlayerNegativeStatus> NegativeStatusTypes { get; set; }
    public PlayerConfidenceTypes ConfidenceType { get; set; }
    public bool IsInitialized { get; set; } = false;
    public string StartingLocation { get; private set; }

    public PlayerState()
    {
        Background = GameRules.StandardRuleset.Background;

        Inventory = new Inventory(GameRules.StandardRuleset.StartingInventorySize);
        Equipment = new Equipment();

        Coins = GameRules.StandardRuleset.StartingCoins;
        Food = 2;
        MedicinalHerbs = 2;

        Energy = GameRules.StandardRuleset.StartingPhysicalEnergy;
        MaxEnergy = 10;

        Health = GameRules.StandardRuleset.StartingHealth;
        MinHealth = GameRules.StandardRuleset.MinimumHealth;
        MaxHealth = 20;

        Concentration = GameRules.StandardRuleset.StartingConcentration;
        MaxConcentration = 20;

        Confidence = GameRules.StandardRuleset.StartingConfidence;
        MaxConfidence = 20;

        NegativeStatusTypes = new();
        ConfidenceType = PlayerConfidenceTypes.Neutral;
    }

    public void SetStartingLocation(string startingLocation)
    {
        StartingLocation = startingLocation;
        AddLocationKnowledge(StartingLocation);
    }

    public void Initialize(string playerName, ArchetypeTypes selectedArchetype)
    {
        Name = playerName;
        SetArchetype(selectedArchetype);
        IsInitialized = true;
    }

    public void SetArchetype(ArchetypeTypes archetype)
    {
        this.Archetype = archetype;

        switch (archetype)
        {
            case ArchetypeTypes.Warrior:
                ArchetypeConfig = ArchetypeConfig.CreateWarrior();
                InitializeWarriorInventory();
                break;
            case ArchetypeTypes.Scholar:
                ArchetypeConfig = ArchetypeConfig.CreateScholar();
                InitializeScholarInventory();
                break;
            case ArchetypeTypes.Ranger:
                ArchetypeConfig = ArchetypeConfig.CreateRanger();
                InitializeRangerInventory();
                break;
            case ArchetypeTypes.Bard:
                ArchetypeConfig = ArchetypeConfig.CreateBard();
                InitializeBardInventory();
                break;
            case ArchetypeTypes.Thief:
                ArchetypeConfig = ArchetypeConfig.CreateThief();
                InitializeThiefInventory();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(archetype));
        }
    }

    private void InitializeWarriorInventory()
    {
        // Clear existing inventory first
        ClearInventory();

        // Add warrior-specific items
        Inventory.AddItem(ItemTypes.Sword);
        Inventory.AddItem(ItemTypes.Shield);
        Inventory.AddItem(ItemTypes.LeatherArmor);
        Inventory.AddItem(ItemTypes.Rations);
    }

    private void InitializeScholarInventory()
    {
        // Clear existing inventory first
        ClearInventory();

        // Add scholar-specific items
        Inventory.AddItem(ItemTypes.Book);
        Inventory.AddItem(ItemTypes.Scroll);
        Inventory.AddItem(ItemTypes.WritingKit);
        Inventory.AddItem(ItemTypes.Dagger);
        Inventory.AddItem(ItemTypes.Rations);
    }

    private void InitializeRangerInventory()
    {
        // Clear existing inventory first
        ClearInventory();

        // Add ranger-specific items
        Inventory.AddItem(ItemTypes.Bow);
        Inventory.AddItem(ItemTypes.Arrow);
        Inventory.AddItem(ItemTypes.HuntingKnife);
        Inventory.AddItem(ItemTypes.Rations);
        Inventory.AddItem(ItemTypes.HealingHerbs);
    }

    private void InitializeBardInventory()
    {
        // Clear existing inventory first
        ClearInventory();

        // Add bard-specific items
        Inventory.AddItem(ItemTypes.Lute);
        Inventory.AddItem(ItemTypes.FineClothes);
        Inventory.AddItem(ItemTypes.WineBottle);
        Inventory.AddItem(ItemTypes.Dagger);
        Inventory.AddItem(ItemTypes.Rations);
    }

    private void InitializeThiefInventory()
    {
        // Clear existing inventory first
        ClearInventory();

        // Add thief-specific items
        Inventory.AddItem(ItemTypes.Lockpicks);
        Inventory.AddItem(ItemTypes.Rope);
        Inventory.AddItem(ItemTypes.Dagger);
        Inventory.AddItem(ItemTypes.ClimbingGear);
        Inventory.AddItem(ItemTypes.Rations);
    }

    private void ClearInventory()
    {
        // Simple method to clear all slots
        for (int i = 0; i < Inventory.MaxCapacity; i++)
        {
            if (Inventory.ContainsItem(ItemTypes.None) == false)
            {
                Inventory.RemoveItem(Inventory.GetFirstItem());
            }
        }
    }

    public List<ApproachTags> GetNaturalApproaches()
    {
        return ArchetypeConfig.GetApproachesWithAffinity(AffinityTypes.Natural);
    }
    public List<ApproachTags> GetIncompatibleApproaches()
    {
        return ArchetypeConfig.GetApproachesWithAffinity(AffinityTypes.Incompatible);
    }

    public List<FocusTags> GetNaturalFocuses()
    {
        return ArchetypeConfig.GetFocusesWithAffinity(AffinityTypes.Natural);
    }
    public List<FocusTags> GetIncompatibleFocuses()
    {
        return ArchetypeConfig.GetFocusesWithAffinity(AffinityTypes.Incompatible);
    }

    public AffinityTypes GetApproachAffinity(ApproachTags approach)
    {
        return ArchetypeConfig.GetAffinity(approach);
    }
    public AffinityTypes GetFocusAffinity(FocusTags focus)
    {
        return ArchetypeConfig.GetAffinity(focus);
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

    public bool ModifyEnergy(int count)
    {
        int newEnergy = Math.Clamp(Energy + count, 0, MaxEnergy);
        if (newEnergy != Energy)
        {
            Energy = newEnergy;
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

    public bool ModifyConfidence(int count)
    {
        int newConfidence = Math.Clamp(Confidence + count, 0, MaxConfidence);
        if (newConfidence != Confidence)
        {
            Confidence = newConfidence;
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

    public bool ModifyMedicinalHerbs(int amount)
    {
        int newHerbs = Math.Max(0, MedicinalHerbs + amount);
        if (newHerbs != MedicinalHerbs)
        {
            MedicinalHerbs = newHerbs;
            return true;
        }
        return false;
    }

    public bool ConsumeFood(int amount)
    {
        if (Food >= amount)
        {
            Food -= amount;
            ModifyEnergy(amount * 25); // Each food unit restores 25 Energy
            return true;
        }
        return false;
    }

    public bool ConsumeMedicinalHerbs(int amount)
    {
        if (MedicinalHerbs >= amount)
        {
            MedicinalHerbs -= amount;
            ModifyHealth(amount * 15);
            ModifyConcentration(amount * 15);
            ModifyConfidence(amount * 15);
            return true;
        }
        return false;
    }

    public bool HasAchievement(AchievementTypes achievementType)
    {
        return true;
    }

    public void UnlockAchievement(AchievementTypes achievementType)
    {
    }

    public void ModifyResource(ResourceChangeTypes changeType, ItemTypes resourceType, int amount)
    {
    }


    public int GetRelationshipLevel(CharacterTypes character)
    {
        return 1;
    }

    public bool HasConfidence(PlayerConfidenceTypes expectedValue)
    {
        return ConfidenceType == expectedValue;
    }

    public bool HasStatusEffect(PlayerNegativeStatus expectedValue)
    {
        return NegativeStatusTypes.Contains(expectedValue);
    }

    public void AddLocationKnowledge(string locationName)
    {
        if (KnownLocations.Contains(locationName)) return;
        KnownLocations.Add(locationName);
    }


    public bool HasKnowledge(KnowledgeTags value, int requiredKnowledgeLevel)
    {
        return true;
    }


    public void AddExperiencePoints(int xpBonus)
    {
        int newExperiencePoints = Math.Max(0, ExperiencePoints + xpBonus);
        if (newExperiencePoints != ExperiencePoints)
        {
            ExperiencePoints = newExperiencePoints;
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
}
