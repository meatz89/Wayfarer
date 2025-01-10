public class PlayerState
{
    public int Level { get; set; } = 1;
    public int Coins { get; set; }
    public int Health { get; set; }
    public int MinHealth { get; set; }
    public int MaxHealth { get; set; }

    public int Stress { get; set; }
    public int MinStress { get; set; }
    public int MaxStress { get; set; }

    public int Reputation { get; set; }
    public int MinReputation { get; set; }
    public int MaxReputation { get; set; }

    public int PhysicalEnergy { get; set; }
    public int MaxPhysicalEnergy { get; set; }

    public int FocusEnergy { get; set; }
    public int MaxFocusEnergy { get; set; }

    public int SocialEnergy { get; set; }
    public int MaxSocialEnergy { get; set; }

    public Dictionary<SkillTypes, int> Skills { get; set; } = new();
    public Inventory Inventory { get; set; }
    public Equipment Equipment { get; set; }
    public List<Knowledge> Knowledge { get; set; } = new();
    public Dictionary<ReputationTypes, int> Reputations { get; set; } = new();
    public Encounter CurrentEncounter { get; set; }

    public PlayerState()
    {
        Inventory = new Inventory(GameRules.StandardRuleset.StartingInventorySize);
        Equipment = new Equipment();

        Skills.Add(SkillTypes.Strength, 5);
        Skills.Add(SkillTypes.Perception, 5);
        Skills.Add(SkillTypes.Charisma, 8);

        Reputations.Add(ReputationTypes.Honest, 3); // Initialize a reputation
        Reputations.Add(ReputationTypes.Reliable, 5);
        Reputations.Add(ReputationTypes.Trusted, 2);
        Reputations.Add(ReputationTypes.Courageous, 1);
        Reputations.Add(ReputationTypes.Generous, 1);
        Reputations.Add(ReputationTypes.Sharp, 5);
        Reputations.Add(ReputationTypes.Unbreakable, 1);
        Reputations.Add(ReputationTypes.Wise, 1);
        Reputations.Add(ReputationTypes.Pious, 1);
        Reputations.Add(ReputationTypes.Ruthless, 1);

        Knowledge.Add(new Knowledge(KnowledgeTypes.Clue));
        Knowledge.Add(new Knowledge(KnowledgeTypes.Secret));

        Coins = GameRules.StandardRuleset.StartingCoins;

        Health = GameRules.StandardRuleset.StartingHealth;
        MinHealth = GameRules.StandardRuleset.MinimumHealth;
        MaxHealth = 40;

        PhysicalEnergy = GameRules.StandardRuleset.StartingPhysicalEnergy;
        MaxPhysicalEnergy = 40;

        FocusEnergy = GameRules.StandardRuleset.StartingFocusEnergy;
        MaxFocusEnergy = 40;

        SocialEnergy = GameRules.StandardRuleset.StartingSocialEnergy;
        MaxSocialEnergy = 40;

        Stress = MinStress = 0;
        MaxStress = 40;

        Reputation = 0;
        MinReputation = 0;
        MaxReputation = 40;
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
        int currentFood = inventory.GetItemCount(ResourceTypes.Food);

        int updatedFood = Math.Clamp(currentFood + count, 0, inventory.GetCapacityFor(ResourceTypes.Food));
        if (updatedFood != currentFood)
        {
            inventory.SetItemCount(ResourceTypes.Food, updatedFood);
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

    internal void ModifyEnergy(EnergyTypes energyType, int amount)
    {
        switch (energyType)
        {
            case EnergyTypes.Physical:  ModifyPhysicalEnergy(amount); break;
            case EnergyTypes.Focus:  ModifyFocusEnergy(amount); break;
            case EnergyTypes.Social: ModifySocialEnergy(amount); break;
        };
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

    public bool ModifyFocusEnergy(int count)
    {
        int newEnergy = Math.Clamp(FocusEnergy + count, 0, MaxFocusEnergy);
        if (newEnergy != FocusEnergy)
        {
            FocusEnergy = newEnergy;
            return true;
        }
        return false;
    }

    public bool ModifySocialEnergy(int count)
    {
        int newEnergy = Math.Clamp(SocialEnergy + count, 0, MaxSocialEnergy);
        if (newEnergy != SocialEnergy)
        {
            SocialEnergy = newEnergy;
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

    public bool ModifyItem(ResourceChangeTypes itemChange, ResourceTypes resourceType, int count)
    {
        if (itemChange == ResourceChangeTypes.Added)
        {
            int itemsAdded = Inventory.AddResources(resourceType, count);
            return itemsAdded == count;
        }
        else if (itemChange == ResourceChangeTypes.Removed)
        {
            int itemsRemoved = Inventory.RemoveResources(resourceType, count);
            return itemsRemoved == count;
        }

        return false;
    }

    public void ChangeHealth(int healthGain)
    {
        Health = Math.Min(MaxHealth, Math.Max(MinHealth, Health + healthGain));
    }

    public void ModifyReputation(ReputationTypes reputationType, int count)
    {
        if (!Reputations.ContainsKey(reputationType))
        {
            Reputations.Add(reputationType, 0);
        }
        Reputations[reputationType] += count;
    }

    public int GetReputationLevel(ReputationTypes reputationType)
    {
        if (!Reputations.ContainsKey(reputationType))
        {
            return 0;
        }
        return Reputations[reputationType];
    }

    public void SetReputationLevel(ReputationTypes reputationType, int level)
    {
        if (Reputations.ContainsKey(reputationType))
        {
            Reputations[reputationType] = level;
        }
    }

    public void ModifyKnowledge(KnowledgeTypes knowledgeType, int count)
    {
        // Add the knowledge to the player's knowledge list
        for (int i = 0; i < count; i++)
        {
            Knowledge.Add(new Knowledge(knowledgeType));
        }
    }

    public bool HasReputation(ReputationTypes reputationType)
    {
        return Reputations.ContainsKey(reputationType);
    }

    public bool HasKnowledge(KnowledgeTypes knowledgeType)
    {
        return Knowledge.Any(k => k.KnowledgeType == knowledgeType);
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

    public void ModifyResource(ResourceChangeTypes changeType, ResourceTypes resourceType, int amount)
    {
    }

    public bool HasStatus(PlayerStatusTypes status)
    {
        return true;
    }

    public bool CanPayEnergy(EnergyTypes energyType, int amount)
    {
        return true;
    }

    public bool HasResource(ResourceTypes resourceType, int v)
    {
        return true;
    }

    public bool CanLoseReputation(ReputationTypes reputationType, object value)
    {
        return true;
    }

    public bool HasCoins(object value)
    {
        return true;
    }

}