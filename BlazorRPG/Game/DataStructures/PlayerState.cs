public class PlayerState
{
    public int Coins { get; set; }
    public int Health { get; set; }
    public int MinHealth { get; set; }
    public int MaxHealth { get; set; }

    public int PhysicalEnergy { get; set; }
    public int MaxPhysicalEnergy { get; set; }

    public int FocusEnergy { get; set; }
    public int MaxFocusEnergy { get; set; }

    public int SocialEnergy { get; set; }
    public int MaxSocialEnergy { get; set; }

    public Dictionary<SkillTypes, int> Skills { get; set; }
    public Inventory Inventory { get; set; }

    public bool ModifyCoins(int amount)
    {
        int newCoins = Math.Max(0, Coins + amount);
        if (newCoins != Coins)
        {
            Coins = newCoins;
            return true;
        }
        return false;
    }

    public bool ModifyFood(int amount)
    {
        Inventory inventory = Inventory;
        int currentFood = inventory.GetItemCount(ResourceTypes.Food);

        int updatedFood = Math.Clamp(currentFood + amount, 0, inventory.GetCapacityFor(ResourceTypes.Food));
        if (updatedFood != currentFood)
        {
            inventory.SetItemCount(ResourceTypes.Food, updatedFood);
            return true;
        }
        return false;
    }

    public bool ModifyHealth(int amount)
    {
        int newHealth = Math.Clamp(Health + amount, 0, MaxHealth);
        if (newHealth != Health)
        {
            Health = newHealth;
            return true;
        }
        return false;
    }

    public bool ModifyPhysicalEnergy(int amount)
    {
        int newEnergy = Math.Clamp(PhysicalEnergy + amount, 0, MaxPhysicalEnergy);
        if (newEnergy != PhysicalEnergy)
        {
            PhysicalEnergy = newEnergy;
            return true;
        }
        return false;
    }

    public bool ModifyFocusEnergy(int amount)
    {
        int newEnergy = Math.Clamp(FocusEnergy + amount, 0, MaxFocusEnergy);
        if (newEnergy != FocusEnergy)
        {
            FocusEnergy = newEnergy;
            return true;
        }
        return false;
    }

    public bool ModifySocialEnergy(int amount)
    {
        int newEnergy = Math.Clamp(SocialEnergy + amount, 0, MaxSocialEnergy);
        if (newEnergy != SocialEnergy)
        {
            SocialEnergy = newEnergy;
            return true;
        }
        return false;
    }

    public bool ModifySkillLevel(SkillTypes skillType, int amount)
    {
        int newSkillLevel = Math.Max(0, Skills[skillType] + amount);
        if (newSkillLevel != Skills[skillType])
        {
            Skills[skillType] = newSkillLevel;
            return true;
        }
        return false;
    }

    public bool ModifyItem(ResourceChangeType itemChange, ResourceTypes resourceType, int count)
    {
        if (itemChange == ResourceChangeType.Added)
        {
            int itemsAdded = Inventory.AddItems(resourceType, count);
            return itemsAdded == count;
        }
        else if (itemChange == ResourceChangeType.Removed)
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

    internal void ModifyReputation(ReputationTypes reputationType, int amount)
    {
        throw new NotImplementedException();
    }

    internal int GetReputationLevel(ReputationTypes reputationType)
    {
        throw new NotImplementedException();
    }

    internal void UnlockAchievement(AchievementTypes achievementType)
    {
        throw new NotImplementedException();
    }

    internal bool HasAchievement(AchievementTypes achievementType)
    {
        throw new NotImplementedException();
    }
}
