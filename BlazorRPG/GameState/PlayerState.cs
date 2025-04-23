public class PlayerState
{
    // Core identity
    public string Name { get; set; }
    public Genders Gender { get; private set; }
    public string Background { get; set; }

    // Archetype
    public ArchetypeTypes Archetype { get; set; }

    // Progression systems
    public int Level { get; set; } = 1;
    public int CurrentXP { get; set; } = 0;
    public int XPToNextLevel { get; set; } = 100;

    // Resources
    public int Money { get; set; }
    public int Food { get; set; }
    public int MedicinalHerbs { get; set; }
    public Inventory Inventory { get; set; } = new Inventory(10);

    // Relationships with characters
    public RelationshipList Relationships { get; set; } = new();

    // Card collection (player skills)
    public List<CardDefinition> UnlockedCards { get; set; } = new List<CardDefinition>();

    // Location knowledge
    public List<string> DiscoveredLocationIds { get; set; } = new List<string>();

    // Travel capabilities
    public List<string> UnlockedTravelMethods { get; set; } = new List<string>();

    public int Coins { get; set; }

    public HashSet<(string, EncounterTypes)> LocationActionAvailability { get; set; } = new();

    public List<PlayerNegativeStatus> NegativeStatusTypes { get; set; }
    public bool IsInitialized { get; set; } = false;

    public List<string> KnownLocations { get; private set; } = new List<string>();
    public List<string> KnownLocationSpots { get; private set; } = new List<string>();
    public List<CardDefinition> KnownCards { get; private set; } = new List<CardDefinition>();
    public PlayerSkills PlayerSkills { get; private set; } = new();

    private int BaseMaxEnergy { get; set; } = 10;
    private int BaseMaxHealth { get; set; } = 20;
    private int BaseMaxConcentration { get; set; } = 20;
    private int BaseMaxConfidence { get; set; } = 20;
    private int BaseMaxReputation { get; set; } = 20;

    public int MinHealth { get; set; }
    public int Health { get; set; }
    public int Concentration { get; set; }
    public int Confidence { get; set; }
    public int Energy { get; set; }
    public int Reputation { get; set; }

    public int MaxHealth;
    public int MaxEnergy;
    public int MaxConcentration;
    public int MaxConfidence;
    public int MaxReputation;

    public PlayerState()
    {
        Background = GameRules.StandardRuleset.Background;

        Inventory = new Inventory(GameRules.StandardRuleset.StartingInventorySize);

        Level = 1;
        CurrentXP = 0;
        XPToNextLevel = 100;

        Coins = GameRules.StandardRuleset.StartingCoins;
        Food = 2;
        MedicinalHerbs = 2;

        NegativeStatusTypes = new();

        SetCharacterStats();
        HealFully();
    }

    public void HealFully()
    {
        Energy = MaxEnergy;
        Health = MaxHealth;
        Concentration = MaxConcentration;
        Confidence = MaxConfidence;
    }

    public void SetCharacterStats()
    {
        MaxHealth = BaseMaxHealth + PlayerSkills.BonusMaxHealth;
        MaxEnergy = BaseMaxEnergy + PlayerSkills.BonusMaxEnergy;
        MaxConcentration = BaseMaxConcentration + PlayerSkills.BonusMaxConcentration;
        MaxConfidence = BaseMaxConfidence + PlayerSkills.BonusMaxConfidence;
        MaxReputation = BaseMaxReputation + PlayerSkills.BonusReputation;
    }

    public void Initialize(string playerName, ArchetypeTypes selectedArchetype, Genders gender)
    {
        Name = playerName;
        Gender = gender;
        SetArchetype(selectedArchetype);

        SetCharacterStats();
        HealFully();

        IsInitialized = true;
    }

    public void SetArchetype(ArchetypeTypes archetype)
    {
        this.Archetype = archetype;

        switch (archetype)
        {
            case ArchetypeTypes.Knight:
                InitializeKnightInventory();
                break;

            case ArchetypeTypes.Courtier:
                InitializeCourtierInventory();
                break;

            case ArchetypeTypes.Sage:
                InitializeSageInventory();
                break;

            case ArchetypeTypes.Forester:
                InitializeForesterInventory();
                break;

            case ArchetypeTypes.Shadow:
                InitializeShadowInventory();
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(archetype));
        }
    }

    private void InitializeKnightInventory()
    {
        // Clear existing inventory first
        ClearInventory();

        // Add warrior-specific items
        Inventory.AddItem(ItemTypes.Sword);
        Inventory.AddItem(ItemTypes.Shield);
        Inventory.AddItem(ItemTypes.Chainmail);
        Inventory.AddItem(ItemTypes.Whetstone);
        Inventory.AddItem(ItemTypes.FlintAndSteel);
    }

    private void InitializeCourtierInventory()
    {
        // Clear existing inventory first
        ClearInventory();

        // Add bard-specific items
        Inventory.AddItem(ItemTypes.FineClothes);
        Inventory.AddItem(ItemTypes.WaxSealKit);
        Inventory.AddItem(ItemTypes.Perfume);
        Inventory.AddItem(ItemTypes.SilverCoins);
        Inventory.AddItem(ItemTypes.WineFlask);
    }

    private void InitializeSageInventory()
    {
        // Clear existing inventory first
        ClearInventory();

        // Add scholar-specific items
        Inventory.AddItem(ItemTypes.Journal);
        Inventory.AddItem(ItemTypes.QuillAndInk);
        Inventory.AddItem(ItemTypes.Spectacles);
        Inventory.AddItem(ItemTypes.PuzzleBox);
        Inventory.AddItem(ItemTypes.AncientText);
    }

    private void InitializeForesterInventory()
    {
        // Clear existing inventory first
        ClearInventory();

        // Add ranger-specific items
        Inventory.AddItem(ItemTypes.Bow);
        Inventory.AddItem(ItemTypes.SkinningKnife);
        Inventory.AddItem(ItemTypes.Snares);
        Inventory.AddItem(ItemTypes.FlintAndSteel);
        Inventory.AddItem(ItemTypes.HerbPouch);
    }

    private void InitializeShadowInventory()
    {
        // Clear existing inventory first
        ClearInventory();

        // Add thief-specific items
        Inventory.AddItem(ItemTypes.Lockpicks);
        Inventory.AddItem(ItemTypes.DarkCloak);
        Inventory.AddItem(ItemTypes.GrapplingHook);
        Inventory.AddItem(ItemTypes.PoisonVial);
        Inventory.AddItem(ItemTypes.DisguiseKit);
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

    public bool HasStatusEffect(PlayerNegativeStatus expectedValue)
    {
        return NegativeStatusTypes.Contains(expectedValue);
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

    public void UnlockCard(CardDefinition card)
    {
        KnownCards.Add(card);
    }

    public void AddKnownLocation(string location)
    {
        KnownLocationSpots.Add(location);
    }

    public void AddKnownLocationSpot(string locationSpot)
    {
        KnownLocationSpots.Add(locationSpot);
    }

    internal void AddFood(int amount)
    {
        throw new NotImplementedException();
    }

    internal void AddEnergy(int amount)
    {
        throw new NotImplementedException();
    }

    internal int GetRelationshipLevel(string character)
    {
        throw new NotImplementedException();
    }

    internal void UpdateRelationship(object characterId, object delta)
    {
        throw new NotImplementedException();
    }
}
