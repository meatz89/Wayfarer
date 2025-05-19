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


    // Resources
    public int Silver { get; set; } = 10;
    public int ActionPoints { get; set; } = 18;
    public int EnergyPoints { get; set; } = 10;
    public int Concentration { get; set; } = 10;
    public int Reputation { get; set; } = 0;
    public int Health { get; set; }
    public int Food { get; set; }

    public int MaxActionPoints { get; set; } = 4;
    public int MaxEnergyPoints { get; set; } = 12;
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


    public HashSet<(string, CardTypes)> LocationActionAvailability { get; set; } = new();

    public List<PlayerNegativeStatus> NegativeStatusTypes { get; set; }
    public bool IsInitialized { get; set; } = false;

    public List<string> KnownLocations { get; private set; } = new List<string>();
    public List<string> KnownLocationSpots { get; private set; } = new List<string>();
    public PlayerSkills Skills { get; private set; } = new();

    // Card collection (player skills)
    public List<CardDefinition> PlayerSkillCards { get; set; } = new List<CardDefinition>();
    public List<CardDefinition> PlayerHandCards { get; set; } = new List<CardDefinition>();
    public Location CurrentLocation { get; set; }
    public LocationSpot CurrentLocationSpot { get; set; }

    public PlayerState()
    {
        Background = GameRules.StandardRuleset.Background;
        Inventory = new Inventory(10);

        Silver = 5;
        Level = 1;
        CurrentXP = 0;
        XPToNextLevel = 100;

        NegativeStatusTypes = new();

        PlayerHandCards = new List<CardDefinition>();

        CardDefinition card1 = new CardDefinition("1", "1") { Type = CardTypes.Physical, };
        CardDefinition card2 = new CardDefinition("1", "1") { Type = CardTypes.Physical, };
        CardDefinition card3 = new CardDefinition("1", "1") { Type = CardTypes.Intellectual, };
        CardDefinition card4 = new CardDefinition("1", "1") { Type = CardTypes.Social, IsExhausted = true };

        PlayerHandCards.Add(card1);
        PlayerHandCards.Add(card2);
        PlayerHandCards.Add(card3);
        PlayerHandCards.Add(card4);
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
        EnergyPoints = MaxEnergyPoints;
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

    public bool HasAvailableCard(CardTypes cardTypes)
    {
        foreach (CardDefinition card in PlayerHandCards)
        {
            if (card.Type == cardTypes && !card.IsExhausted)
            {
                return true;
            }
        }
        return false;
    }

    public void ExhaustCard(CardDefinition card)
    {
        card.IsExhausted = true;
    }
    
    public void RefreshCard(CardDefinition card)
    {
        card.IsExhausted = false;
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
        int newCoins = Math.Max(0, Silver + count);
        if (newCoins != Silver)
        {
            Silver = newCoins;
        }
    }

    public void UnlockCard(CardDefinition card)
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
        return EnergyPoints;
    }

    public void SetNewEnergy(int newEnergy)
    {
        this.EnergyPoints = newEnergy;
    }

    public void ModifyEnergy(int amount)
    {
        this.EnergyPoints += amount;
    }

    internal bool HasNonExhaustedCardOfType(CardTypes requiredCardType)
    {
        return true;
    }

    internal int GetSkillLevel(SkillTypes skill)
    {
        int level = Skills.GetLevelForSkill(skill);
        return level;
    }

    internal CardDefinition GetSelectedCardForSkill(SkillTypes skill)
    {
        return null;
    }

    internal void AddSilver(int silverReward)
    {
        throw new NotImplementedException();
    }

    internal void AddReputation(int reputationReward)
    {
        throw new NotImplementedException();
    }

    internal void AddInsightPoints(int insightPointReward)
    {
        throw new NotImplementedException();
    }

    internal PlayerState Serialize()
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
        clone.Silver = this.Silver;
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

        // Deep copy of NegativeStatusTypes
        clone.NegativeStatusTypes = [.. this.NegativeStatusTypes];

        // Deep copy of Skills
        clone.Skills = this.Skills.Clone();

        // Deep copy of Cards
        clone.PlayerHandCards = new List<CardDefinition>();
        foreach (CardDefinition card in this.PlayerHandCards)
        {
            CardDefinition cardCopy = new CardDefinition(card.Id, card.Name)
            {
                Type = card.Type
            };
            clone.PlayerHandCards.Add(cardCopy);
        }

        return clone;
    }

    internal CardDefinition GetBestNonExhaustedCardOfType(CardTypes requiredCardType)
    {
        throw new NotImplementedException();
    }
}
