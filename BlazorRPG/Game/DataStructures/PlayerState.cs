public class PlayerState
{
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

    public Dictionary<SkillTypes, int> Skills { get; set; }
    public Inventory Inventory { get; set; }
    public Equipment Equipment { get; set; }
    public int Level { get; set; } = 1;

    public PlayerState()
    {
        Inventory = new Inventory(GameRules.StandardRuleset.StartingInventorySize);
        Equipment = new Equipment();
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

    public void ChangeHealth(int healthGain)
    {
        Health = Math.Min(MaxHealth, Math.Max(MinHealth, Health + healthGain));
    }

    public void ModifyReputation(ReputationTypes reputationType, int count)
    {
    }

    public int GetReputationLevel(ReputationTypes reputationType)
    {
        return 1;
    }

    public void UnlockAchievement(AchievementTypes achievementType)
    {
    }

    public bool HasAchievement(AchievementTypes achievementType)
    {
        return true;
    }

    public bool HasStatus(PlayerStatusTypes status)
    {
        return true;
    }

    public int GetKnowledgeLevel(KnowledgeTypes knowledgeType)
    {
        return 1;
    }

    public void ModifyKnowledge(KnowledgeTypes knowledgeType, int count)
    {
    }

    public bool HasReputation(ReputationTypes honest)
    {
        return true;
    }

    public int GetReputationValue(ReputationTypes honest)
    {
        return 5;
    }

    public bool HasKnowledge(KnowledgeTypes clue)
    {
        return true;
    }
}
