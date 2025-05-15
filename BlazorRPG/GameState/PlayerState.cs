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
    public int MaxActionPoints { get; set; } = 4;
    public int ActionPoints { get; set; } = 4;
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


    public HashSet<(string, EncounterCategories)> LocationActionAvailability { get; set; } = new();

    public List<PlayerNegativeStatus> NegativeStatusTypes { get; set; }
    public bool IsInitialized { get; set; } = false;

    public List<string> KnownLocations { get; private set; } = new List<string>();
    public List<string> KnownLocationSpots { get; private set; } = new List<string>();
    public List<CardDefinition> KnownCards { get; private set; } = new List<CardDefinition>();
    public PlayerSkills Skills { get; private set; } = new();

    public int Food { get; set; }

    public int MinHealth { get; set; }
    public int Health { get; set; }
    public int Concentration { get; set; }

    public int MaxHealth { get; set; }
    public int MaxConcentration { get; set; }

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

        Card card1 = new Card() { Name = "Card 1", Type = CardTypes.Physical };
        Card card2 = new Card() { Name = "Card 2", Type = CardTypes.Physical };
        Card card3 = new Card() { Name = "Card 3", Type = CardTypes.Intellectual };
        Card card4 = new Card() { Name = "Card 4", Type = CardTypes.Social };

        Cards.Add(card1);
        Cards.Add(card2);
        Cards.Add(card3);
        Cards.Add(card4);
    }

    public void HealFully()
    {
        EnergyPoints = MaxEnergyPoints;
        Health = MaxHealth;
        Concentration = MaxConcentration;
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

    internal void ModifyEnergy(int amount)
    {
        this.EnergyPoints += amount;
    }


    public PlayerState Clone()
    {
        // Create a new PlayerState instance
        PlayerState clone = new PlayerState();

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
        clone.MaxEnergyPoints = this.MaxEnergyPoints;
        clone.EnergyPoints = this.EnergyPoints;
        clone.Coins = this.Coins;
        clone.Food = this.Food;
        clone.MinHealth = this.MinHealth;
        clone.Health = this.Health;
        clone.Concentration = this.Concentration;
        clone.MaxHealth = this.MaxHealth;
        clone.MaxConcentration = this.MaxConcentration;
        clone.IsInitialized = this.IsInitialized;

        // Deep copy Inventory
        clone.Inventory = new Inventory(this.Inventory.Capacity);
        foreach (string item in this.Inventory.GetAllItems())
        {
            clone.Inventory.AddItem(item);
        }

        // Deep copy of RelationshipList
        clone.Relationships = this.Relationships.Clone();

        // Deep copy of card collections
        clone.UnlockedCards = new List<CardDefinition>(this.UnlockedCards);
        clone.KnownCards = new List<CardDefinition>(this.KnownCards);

        // Deep copy of location knowledge
        clone.DiscoveredLocationIds = new List<string>(this.DiscoveredLocationIds);
        clone.KnownLocations = new List<string>(this.KnownLocations);
        clone.KnownLocationSpots = new List<string>(this.KnownLocationSpots);

        // Deep copy of travel methods
        clone.UnlockedTravelMethods = new List<string>(this.UnlockedTravelMethods);

        // Deep copy of LocationActionAvailability HashSet
        clone.LocationActionAvailability = new HashSet<(string, EncounterCategories)>(
            this.LocationActionAvailability);

        // Deep copy of NegativeStatusTypes
        clone.NegativeStatusTypes = new List<PlayerNegativeStatus>(this.NegativeStatusTypes);

        // Deep copy of Skills
        clone.Skills = this.Skills.Clone();

        // Deep copy of Cards
        clone.Cards = new List<Card>();
        foreach (Card card in this.Cards)
        {
            Card cardCopy = new Card
            {
                Name = card.Name,
                Type = card.Type
                // Copy other Card properties as needed
            };
            clone.Cards.Add(cardCopy);
        }

        return clone;
    }
}
