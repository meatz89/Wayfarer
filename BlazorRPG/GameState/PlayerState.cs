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
    public PlayerSkills Skills { get; private set; } = new();

    public int MinHealth { get; set; }
    public int Health { get; set; }
    public int Focus { get; set; }
    public int Spirit { get; set; }
    public int Energy { get; set; }

    public int MaxHealth;
    public int MaxEnergy;
    public int MaxFocus;
    public int MaxSpirit;

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

        HealFully();
    }

    public void HealFully()
    {
        Energy = MaxEnergy;
        Health = MaxHealth;
        Focus = MaxFocus;
        Spirit = MaxSpirit;
    }

    public void Initialize(string playerName, ArchetypeTypes selectedArchetype, Genders gender)
    {
        Name = playerName;
        Gender = gender;
        SetArchetype(selectedArchetype);

        HealFully();

        IsInitialized = true;
    }

    public void SetCharacterStats()
    {
        MaxHealth = 10 + Skills.BonusMaxHealth;
        MaxEnergy = 10 + Skills.BonusMaxEnergy;
        MaxFocus = 10 + Skills.BonusMaxConcentration;
        MaxSpirit = 10 + Skills.BonusMaxConfidence;
    }

    public void SetArchetype(ArchetypeTypes archetype)
    {
        this.Archetype = archetype;

        switch (archetype)
        {
            case ArchetypeTypes.Artisan:
                InitializeArtisan();
                break;
            case ArchetypeTypes.Courtier:
                InitializeCourtier();
                break;
            case ArchetypeTypes.Scribe:
                InitializeScribe();
                break;
            case ArchetypeTypes.Herbalist:
                InitializeHerbalist();
                break;
            case ArchetypeTypes.Shadow:
                InitializeShadow();
                break;
            case ArchetypeTypes.Merchant:
                InitializeMerchant();
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(archetype));
        }
    }

    private void InitializeArtisan()
    {
        // Inventory
        ClearInventory();
        Inventory.AddItem(ItemTypes.Hammer);
        Inventory.AddItem(ItemTypes.Chisel);
        Inventory.AddItem(ItemTypes.Trowel);
        Inventory.AddItem(ItemTypes.Mortar);

        // Skill bonuses: excels in Endurance and Finesse
        Skills.AddLevelBonus(SkillTypes.Endurance, 1);
        Skills.AddLevelBonus(SkillTypes.Finesse, 1);
    }

    private void InitializeCourtier()
    {
        ClearInventory();
        Inventory.AddItem(ItemTypes.FineClothes);
        Inventory.AddItem(ItemTypes.WaxSealKit);
        Inventory.AddItem(ItemTypes.Perfume);
        Inventory.AddItem(ItemTypes.SilverCoins);
        Inventory.AddItem(ItemTypes.WineFlask);

        // Skill bonuses: excels in Charm and Diplomacy
        Skills.AddLevelBonus(SkillTypes.Charm, 1);
        Skills.AddLevelBonus(SkillTypes.Diplomacy, 1);
    }

    private void InitializeScribe()
    {
        ClearInventory();
        Inventory.AddItem(ItemTypes.Journal);
        Inventory.AddItem(ItemTypes.QuillAndInk);
        Inventory.AddItem(ItemTypes.Spectacles);
        Inventory.AddItem(ItemTypes.PuzzleBox);
        Inventory.AddItem(ItemTypes.AncientText);

        // Skill bonuses: excels in Lore and Insight
        Skills.AddLevelBonus(SkillTypes.Lore, 1);
        Skills.AddLevelBonus(SkillTypes.Lore, 1);
    }

    private void InitializeHerbalist()
    {
        ClearInventory();
        Inventory.AddItem(ItemTypes.HerbSatchel);
        Inventory.AddItem(ItemTypes.MortarAndPestle);
        Inventory.AddItem(ItemTypes.FieldGuide);

        // Skill bonuses: excels in Insight and Lore
        Skills.AddLevelBonus(SkillTypes.Insight, 1);
        Skills.AddLevelBonus(SkillTypes.Lore, 1);
    }

    private void InitializeShadow()
    {
        ClearInventory();
        Inventory.AddItem(ItemTypes.Lockpicks);
        Inventory.AddItem(ItemTypes.DarkCloak);
        Inventory.AddItem(ItemTypes.GrapplingHook);
        Inventory.AddItem(ItemTypes.PoisonVial);
        Inventory.AddItem(ItemTypes.DisguiseKit);

        // Skill bonuses: excels in Finesse and Insight
        Skills.AddLevelBonus(SkillTypes.Finesse, 1);
        Skills.AddLevelBonus(SkillTypes.Lore, 1);
    }

    private void InitializeMerchant()
    {
        ClearInventory();
        Inventory.AddItem(ItemTypes.Ledger);
        Inventory.AddItem(ItemTypes.Scales);
        Inventory.AddItem(ItemTypes.TradeGoods);
        Inventory.AddItem(ItemTypes.CoinPurse);
        Inventory.AddItem(ItemTypes.TradeDocuments);

        // Skill bonuses: excels in Diplomacy and Charm
        Skills.AddLevelBonus(SkillTypes.Diplomacy, 1);
        Skills.AddLevelBonus(SkillTypes.Charm, 1);
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
        int newConcentration = Math.Clamp(Focus + count, 0, MaxFocus);
        if (newConcentration != Focus)
        {
            Focus = newConcentration;
            return true;
        }
        return false;
    }

    public bool ModifyConfidence(int count)
    {
        int newConfidence = Math.Clamp(Spirit + count, 0, MaxSpirit);
        if (newConfidence != Spirit)
        {
            Spirit = newConfidence;
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

    public int GetRelationshipLevel(string character)
    {
        return 1;
    }

    public void UpdateRelationship(object characterId, object delta)
    {
    }

    public int GetReputation(string location)
    {
        throw new NotImplementedException();
    }

}
