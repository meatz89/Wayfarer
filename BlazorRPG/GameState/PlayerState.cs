public class PlayerState
{
    // Core identity
    public string Name { get; set; }
    public string Background { get; set; }

    // Progression systems
    public int Level { get; set; }
    public int ExperiencePoints { get; set; }
    public Dictionary<string, int> Skills { get; set; } = new Dictionary<string, int>();

    // Resources
    public int Money { get; set; }
    public int Food { get; set; }
    public Inventory Inventory { get; set; } = new Inventory(10);

    // Encounter resources (reset at start of encounters)
    public int MaxHealth { get; set; }
    public int MaxConcentration { get; set; }
    public int MaxConfidence { get; set; }

    // Relationships with characters
    public Dictionary<string, Relationship> Relationships { get; set; } = new Dictionary<string, Relationship>();

    // Card collection (player skills)
    public List<ChoiceCard> UnlockedCards { get; set; } = new List<ChoiceCard>();

    // Location knowledge
    public List<string> DiscoveredLocationIds { get; set; } = new List<string>();
    public string CurrentLocationId { get; set; }

    // Travel capabilities
    public List<string> UnlockedTravelMethods { get; set; } = new List<string>();

    public int Coins { get; set; }

    public int Health { get; set; }
    public int MinHealth { get; set; }

    public int PhysicalEnergy { get; set; }
    public int MaxPhysicalEnergy { get; set; }

    public int Concentration { get; set; }

    public int Confidence { get; set; }

    public Equipment Equipment { get; set; }
    public List<KnowledgePiece> Knowledge { get; set; } = new();
    public string StartingLocation { get; set; }
    public List<string> KnownLocations { get; set; } = new();
    public HashSet<(string, BasicActionTypes)> LocationActionAvailability { get; set; } = new();

    public List<PlayerNegativeStatus> NegativeStatusTypes { get; set; }
    public PlayerConfidenceTypes ConfidenceType { get; set; }

    public PlayerState()
    {
        Inventory = new Inventory(GameRules.StandardRuleset.StartingInventorySize);
        Equipment = new Equipment();

        Coins = GameRules.StandardRuleset.StartingCoins;

        PhysicalEnergy = GameRules.StandardRuleset.StartingPhysicalEnergy;
        MaxPhysicalEnergy = 10;

        Health = GameRules.StandardRuleset.StartingHealth;
        MinHealth = GameRules.StandardRuleset.MinimumHealth;
        MaxHealth = 20;

        Concentration = GameRules.StandardRuleset.StartingConcentration;
        MaxConcentration = 20;

        Confidence = GameRules.StandardRuleset.StartingConfidence;
        MaxConfidence = 20;
    }

    public bool ModifyCoins(int count)
    {
        int newCoins = Math.Max(0, Coins + count);
        if (newCoins != Coins)
        {
            Coins = newCoins;
            return true;
        }
        return false;
    }

    public bool ModifyFood(int count)
    {
        Inventory inventory = Inventory;
        int currentFood = inventory.GetItemCount(ItemTypes.Food);

        int updatedFood = Math.Clamp(currentFood + count, 0, inventory.GetCapacityFor(ItemTypes.Food));
        if (updatedFood != currentFood)
        {
            inventory.SetItemCount(ItemTypes.Food, updatedFood);
            return true;
        }
        return false;
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

    public void ModifyEnergy(EnergyTypes energyType, int amount)
    {
        switch (energyType)
        {
            case EnergyTypes.Physical: ModifyPhysicalEnergy(amount); break;
            case EnergyTypes.Concentration: ModifyConcentratin(amount); break;
            default: throw new NotImplementedException();
        }
        ;
    }

    public bool ModifyPhysicalEnergy(int count)
    {
        int newEnergy = Math.Clamp(PhysicalEnergy + count, 0, MaxPhysicalEnergy);
        if (newEnergy != PhysicalEnergy)
        {
            PhysicalEnergy = newEnergy;
            return true;
        }
        return false;
    }

    public bool ModifyConcentratin(int count)
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

    public bool ModifyItem(ResourceChangeTypes itemChange, ItemTypes resourceType, int count)
    {
        if (itemChange == ResourceChangeTypes.Added)
        {
            int itemsAdded = Inventory.AddItems(resourceType, count);
            return itemsAdded == count;
        }
        else if (itemChange == ResourceChangeTypes.Removed)
        {
            int itemsRemoved = Inventory.RemoveItems(resourceType, count);
            return itemsRemoved == count;
        }

        return false;
    }

    public bool CanPayEnergy(EnergyTypes energyType, int amount)
    {
        switch (energyType)
        {
            case EnergyTypes.Physical: return PhysicalEnergy >= amount;
            case EnergyTypes.Concentration: return Concentration >= amount;
        }
        ;
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

    public bool HasResource(ItemTypes resourceType, int v)
    {
        return true;
    }


    public bool HasCoins(int value)
    {
        return Coins >= value;
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

    public void AddActionAvailabilityAt(string locationName, BasicActionTypes actionType)
    {
        if (LocationActionAvailability.Contains((locationName, actionType))) return;
        LocationActionAvailability.Add((locationName, actionType));
    }

    public bool HasKnowledge(KnowledgeTags value, int requiredKnowledgeLevel)
    {
        return true;
    }

    public void SetStartingLocation(string startingLocation)
    {
        StartingLocation = startingLocation;
        AddLocationKnowledge(StartingLocation);
    }

    internal void ApplySkillExperience(string skill)
    {

    }
}