public class PlayerState
{
    public int Level { get; set; } = 1;
    public int Coins { get; set; }

    public int Health { get; set; }
    public int MinHealth { get; set; }
    public int MaxHealth { get; set; }

    public int PhysicalEnergy { get; set; }
    public int MaxPhysicalEnergy { get; set; }

    public int Focus { get; set; }
    public int MaxFocus { get; set; }

    public int Confidence { get; set; }
    public int MaxConfidence { get; set; }

    public Dictionary<SkillTypes, int> Skills { get; set; } = new();
    public Inventory Inventory { get; set; }
    public Equipment Equipment { get; set; }
    public List<KnowledgePiece> Knowledge { get; set; } = new();
    public LocationNames StartingLocation { get; set; }
    public List<LocationNames> KnownLocations { get; set; } = new();
    public HashSet<(LocationNames, BasicActionTypes)> LocationActionAvailability { get; set; } = new();

    public List<PlayerNegativeStatus> NegativeStatusTypes { get; set; }
    public PlayerReputationTypes ReputationType { get; set; }

    public PlayerState()
    {
        Inventory = new Inventory(GameRules.StandardRuleset.StartingInventorySize);
        Equipment = new Equipment();

        Skills.Add(SkillTypes.Strength, 5);
        Skills.Add(SkillTypes.Perception, 5);
        Skills.Add(SkillTypes.Charisma, 8);

        Coins = GameRules.StandardRuleset.StartingCoins;

        Health = GameRules.StandardRuleset.StartingHealth;
        MinHealth = GameRules.StandardRuleset.MinimumHealth;
        MaxHealth = 40;

        PhysicalEnergy = GameRules.StandardRuleset.StartingPhysicalEnergy;
        MaxPhysicalEnergy = 20;

        Focus = GameRules.StandardRuleset.StartingFocus;
        MaxFocus = 20;

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
            case EnergyTypes.Concentration: ModifyConcentration(amount); break;
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

    public bool ModifyReputation(int count)
    {
        int newReputation = Math.Clamp(Confidence + count, 0, MaxConfidence);
        if (newReputation != Confidence)
        {
            Confidence = newReputation;
            return true;
        }
        return false;
    }

    public bool ModifySkillLevel(SkillTypes skillType, int count)
    {
        int newSkillLevel = Math.Max(0, Skills[skillType] + count);
        if (newSkillLevel != Skills[skillType])
        {
            Skills[skillType] = newSkillLevel;
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
            case EnergyTypes.Concentration: return Focus >= amount;
        }
        ;
        return false;
    }

    public int GetSkillLevel(SkillTypes primarySkillType)
    {
        return Skills[primarySkillType];
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

    public bool HasReputation(PlayerReputationTypes expectedValue)
    {
        return ReputationType == expectedValue;
    }

    public bool HasStatusEffect(PlayerNegativeStatus expectedValue)
    {
        return NegativeStatusTypes.Contains(expectedValue);
    }

    public void AddLocationKnowledge(LocationNames locationName)
    {
        if (KnownLocations.Contains(locationName)) return;
        KnownLocations.Add(locationName);
    }

    public void AddActionAvailabilityAt(LocationNames locationName, BasicActionTypes actionType)
    {
        if (LocationActionAvailability.Contains((locationName, actionType))) return;
        LocationActionAvailability.Add((locationName, actionType));
    }

    public bool HasKnowledge(KnowledgeTags value, int requiredKnowledgeLevel)
    {
        return true;
    }

    public void SetStartingLocation(LocationNames startingLocation)
    {
        StartingLocation = startingLocation;
        AddLocationKnowledge(StartingLocation);
    }
}