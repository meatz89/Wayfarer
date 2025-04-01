using System.Security.AccessControl;

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

        Energy = GameRules.StandardRuleset.StartingPhysicalEnergy;
        MaxEnergy = 10;

        Health = GameRules.StandardRuleset.StartingHealth;
        MinHealth = GameRules.StandardRuleset.MinimumHealth;
        MaxHealth = 20;

        Concentration = GameRules.StandardRuleset.StartingConcentration;
        MaxConcentration = 20;

        Confidence = GameRules.StandardRuleset.StartingConfidence;
        MaxConfidence = 20;

        // Default to Warrior archetype
        SetArchetype(ArchetypeTypes.Warrior);
    }

    public void SetArchetype(ArchetypeTypes archetype)
    {
        Archetype = archetype;

        // Set archetype configuration based on type
        switch (archetype)
        {
            case ArchetypeTypes.Warrior:
                ArchetypeConfig = ArchetypeConfig.CreateWarrior();
                break;
            case ArchetypeTypes.Scholar:
                ArchetypeConfig = ArchetypeConfig.CreateScholar();
                break;
            case ArchetypeTypes.Ranger:
                ArchetypeConfig = ArchetypeConfig.CreateRanger();
                break;
            case ArchetypeTypes.Bard:
                ArchetypeConfig = ArchetypeConfig.CreateBard();
                break;
            case ArchetypeTypes.Thief:
                ArchetypeConfig = ArchetypeConfig.CreateThief();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(archetype));
        }
    }

    public List<ApproachTags> GetNaturalApproaches(EncounterTypes encounterType)
    {
        return ArchetypeConfig.GetApproachesWithAffinity(AffinityTypes.Natural, encounterType);
    }

    public List<ApproachTags> GetDangerousApproaches(EncounterTypes encounterType)
    {
        return ArchetypeConfig.GetApproachesWithAffinity(AffinityTypes.Dangerous, encounterType);
    }

    public string GetNaturalApproachesText(EncounterTypes encounterType)
    {
        List<ApproachTags> approaches = GetNaturalApproaches(encounterType);
        return string.Join(", ", approaches);
    }

    public string GetDangerousApproachesText(EncounterTypes encounterType)
    {
        List<ApproachTags> approaches = GetDangerousApproaches(encounterType);
        return string.Join(", ", approaches);
    }

    public AffinityTypes GetApproachAffinity(ApproachTags approach, EncounterTypes encounterType)
    {
        return ArchetypeConfig.GetAffinity(approach, encounterType);
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

    public void SetStartingLocation(string startingLocation)
    {
        StartingLocation = startingLocation;
        AddLocationKnowledge(StartingLocation);
    }

}
