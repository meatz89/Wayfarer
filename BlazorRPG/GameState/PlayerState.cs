public class PlayerState
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

    // Afflictions
    public int MaxActionPoints { get; set;  } = 4;
    public int ActionPoints { get; set; } = 4;
    public int MaxVigor { get; set; } = 20;
    public int Vigor { get; set; }
    public int MaxEnergyPoints { get; set; } = 12;
    public int EnergyPoints { get; set; }

    // Resources
    public int Coins { get; set; }

    public Inventory Inventory { get; set; } = new Inventory(10);

    // Relationships with characters
    public RelationshipList Relationships { get; set; } = new();

    // Card collection (player skills)
    public List<CardDefinition> UnlockedCards { get; set; } = new List<CardDefinition>();

    // Location knowledge
    public List<string> DiscoveredLocationIds { get; set; } = new List<string>();

    // Travel capabilities
    public List<string> UnlockedTravelMethods { get; set; } = new List<string>();


    public HashSet<(string, EncounterApproaches)> LocationActionAvailability { get; set; } = new();

    public List<PlayerNegativeStatus> NegativeStatusTypes { get; set; }
    public bool IsInitialized { get; set; } = false;

    public List<string> KnownLocations { get; private set; } = new List<string>();
    public List<string> KnownLocationSpots { get; private set; } = new List<string>();
    public List<CardDefinition> KnownCards { get; private set; } = new List<CardDefinition>();
    public PlayerSkills Skills { get; private set; } = new();

    public int Food { get; set; }

    public int MinHealth { get; set; }
    public int Health { get; set; }
    public int Focus { get; set; }
    public int Spirit { get; set; }
    public int ExhaustionPoints { get; private set; }
    public int HungerPoints { get; private set; }
    public int MentalStrainPoints { get; private set; }
    public int IsolationPoints { get; private set; }

    public int MaxHealth;
    public int MaxFocus;
    public int MaxSpirit;

    public List<Card> Cards { get; private set; } = new List<Card>();

    public PlayerState()
    {
        Background = GameRules.StandardRuleset.Background;
        Inventory = new Inventory(10);

        Coins = 5;
        Level = 1;
        CurrentXP = 0;
        XPToNextLevel = 100;

        NegativeStatusTypes = new();

        Cards = new List<Card>();

        var card1 = new Card() { Name = "Card 1", Type = CardTypes.Physical };
        var card2 = new Card() { Name = "Card 2", Type = CardTypes.Physical };
        var card3 = new Card() { Name = "Card 3", Type = CardTypes.Intellectual };
        var card4 = new Card() { Name = "Card 4", Type = CardTypes.Social };

        Cards.Add(card1);
        Cards.Add(card2);
        Cards.Add(card3);
        Cards.Add(card4);
    }

    public void HealFully()
    {
        EnergyPoints = MaxEnergyPoints;
        Vigor = MaxVigor;

        Health = MaxHealth;
        Focus = MaxFocus;
        Spirit = MaxSpirit;
    }

    public void Initialize(string playerName, Professions selectedArchetype, Genders gender)
    {
        Name = playerName;
        Gender = gender;
        SetArchetype(selectedArchetype);

        HealFully();

        IsInitialized = true;
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

        // Skill bonuses: excels in Endurance and Finesse
        Skills.AddLevelBonus(SkillTypes.Endurance, 1);
        Skills.AddLevelBonus(SkillTypes.Finesse, 1);
    }

    private void InitializeDiplomat()
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

    private void InitializeScholar()
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

    private void InitializeExplorer()
    {
        ClearInventory();
        Inventory.AddItem(ItemTypes.HerbSatchel);
        Inventory.AddItem(ItemTypes.MortarAndPestle);
        Inventory.AddItem(ItemTypes.FieldGuide);

        // Skill bonuses: excels in Insight and Lore
        Skills.AddLevelBonus(SkillTypes.Insight, 1);
        Skills.AddLevelBonus(SkillTypes.Lore, 1);
    }

    private void InitializeRogue()
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

    public bool ConsumeFood(int amount)
    {
        if (Food >= amount)
        {
            Food -= amount;
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

    public void ApplyActionPointCost(int actionPointCost)
    {
        ActionPoints -= actionPointCost;
    }

    public void AddExhaustionPoints(int exhaustionPoints)
    {
        this.ExhaustionPoints += exhaustionPoints;
    }

    public void AddHungerPoints(int hungerPoints)
    {
        this.HungerPoints += hungerPoints;
    }

    public void AddMentalLoadPoints(int mentalLoad)
    {
        this.MentalStrainPoints += mentalLoad;
    }

    public void AddDisconnectPoints(int disconnectionPoints)
    {
        this.IsolationPoints += disconnectionPoints;
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
        return EnergyPoints;
    }

    public void SetNewEnergy(int newEnergy)
    {
        this.EnergyPoints = newEnergy;
    }

    public int CurrentVigor()
    {
        return Vigor;
    }

    public void ModifyVigor(int amount)
    {
        int newVigor = Math.Clamp(Vigor + amount, 0, MaxVigor);
        this.Vigor = newVigor;
    }

    internal void ModifyHunger(int amount)
    {
        this.HungerPoints += amount;
    }

    internal void ModifyEnergy(int amount)
    {
        this.EnergyPoints += amount;
    }

    internal void ModifyExhaustion(int amount)
    {
        this.ExhaustionPoints += amount;
    }

    internal void ModifyMentalStrain(int amount)
    {
        this.MentalStrainPoints += amount;
    }

    internal void ModifyIsolation(int amount)
    {
        this.IsolationPoints += amount;
    }
}
