public abstract class Outcome
{
    public abstract void Apply(PlayerState player);
    public abstract string GetDescription();

    // Method to preview outcome without applying it
    public abstract string GetPreview(PlayerState player);
}

public class ItemOutcome : Outcome
{
    public ItemTypes ItemType { get; set; }
    public int QuantityChange { get; set; }
    public ItemConditionChangeTypes ConditionChangeType { get; set; }

    public ItemOutcome(ItemTypes itemType, int quantityChange, ItemConditionChangeTypes conditionChangeType)
    {
        ItemType = itemType;
        QuantityChange = quantityChange;
        ConditionChangeType = conditionChangeType;
    }

    public override void Apply(PlayerState player)
    {
        if (QuantityChange > 0)
        {
            // Add item(s)
            for (int i = 0; i < QuantityChange; i++)
            {
                player.Inventory.AddItem(ItemType);
            }
        }
        else
        {
            // Remove or damage item(s)
            var itemsToRemove = player.Inventory.GetItemsOfType(ItemType).Take(Math.Abs(QuantityChange)).ToList();
            foreach (var item in itemsToRemove)
            {
                switch (ConditionChangeType)
                {
                    case ItemConditionChangeTypes.Damage:
                        item.Condition -= 10; // Example: Reduce condition
                        break;
                    case ItemConditionChangeTypes.Consume:
                        player.Inventory.RemoveResources(ResourceTypes.Food, 1); // Consume/remove the item
                        break;
                        // Handle other condition change types
                }
            }
        }
    }

    public override string GetDescription()
    {
        throw new NotImplementedException();
    }

    public override string GetPreview(PlayerState player)
    {
        throw new NotImplementedException();
    }
}

public class KnowledgeOutcome : Outcome
{
    public KnowledgeTypes KnowledgeType { get; set; }
    public int Amount { get; set; }

    public KnowledgeOutcome(KnowledgeTypes knowledgeType, int amount)
    {
        KnowledgeType = knowledgeType;
        Amount = amount;
    }

    public override void Apply(PlayerState player)
    {
        // Add the knowledge to the player's knowledge list
        for (int i = 0; i < Amount; i++)
        {
            player.Knowledge.Add(new Knowledge(KnowledgeType));
        }
    }

    public override string GetDescription()
    {
        throw new NotImplementedException();
    }

    public override string GetPreview(PlayerState player)
    {
        throw new NotImplementedException();
    }
}

public class ReputationOutcome : Outcome
{
    public ReputationTypes ReputationType { get; set; }
    public int Amount { get; set; }

    public ReputationOutcome(ReputationTypes reputationType, int amount)
    {
        ReputationType = reputationType;
        Amount = amount;
    }

    public override void Apply(PlayerState player)
    {
        if (!player.Reputations.ContainsKey(ReputationType))
        {
            player.Reputations[ReputationType] = 0;
        }
        player.SetReputationLevel(ReputationType, player.GetReputationLevel(ReputationType) + Amount);

    }

    public override string GetDescription()
    {
        return $"{ReputationType} ReputationType";
    }

    public override string GetPreview(PlayerState player)
    {
        int current = player.GetReputationLevel(ReputationType);
        int newValue = Math.Clamp(current + 1, 0, 100); // Assuming 0-100 scale
        return $"({current} -> {newValue})";
    }
}

public class EnergyOutcome : Outcome
{
    public EnergyTypes EnergyType { get; }
    public int Amount { get; set; }

    public EnergyOutcome(EnergyTypes type, int count)
    {
        EnergyType = type;
        Amount = count;
    }

    public override void Apply(PlayerState player)
    {
        switch (EnergyType)
        {
            case EnergyTypes.Physical:
                player.PhysicalEnergy = Math.Clamp(
                    player.PhysicalEnergy + Amount,
                    0,
                    player.MaxPhysicalEnergy);
                break;
            case EnergyTypes.Focus:
                player.FocusEnergy = Math.Clamp(
                    player.FocusEnergy + Amount,
                    0,
                    player.MaxFocusEnergy);
                break;
            case EnergyTypes.Social:
                player.SocialEnergy = Math.Clamp(
                    player.SocialEnergy + Amount,
                    0,
                    player.MaxSocialEnergy);
                break;
        }
    }

    public override string GetDescription()
    {
        return $"{(Amount >= 0 ? "+" : "")}{Amount} {EnergyType} Energy";
    }

    public override string GetPreview(PlayerState player)
    {
        int currentValue = EnergyType switch
        {
            EnergyTypes.Physical => player.PhysicalEnergy,
            EnergyTypes.Focus => player.FocusEnergy,
            EnergyTypes.Social => player.SocialEnergy,
            _ => 0
        };

        int maxValue = EnergyType switch
        {
            EnergyTypes.Physical => player.MaxPhysicalEnergy,
            EnergyTypes.Focus => player.MaxFocusEnergy,
            EnergyTypes.Social => player.MaxSocialEnergy,
            _ => 0
        };

        int newValue = Math.Clamp(currentValue + Amount, 0, maxValue);
        return $"({currentValue} -> {newValue})";
    }
}

public class HealthOutcome : Outcome
{
    public int Count { get; }

    public HealthOutcome(int count)
    {
        Count = count;
    }

    public override void Apply(PlayerState player)
    {
        // Uses Player's built-in clamping
        player.ModifyHealth(Count);
    }

    public override string GetDescription()
    {
        return $"{(Count >= 0 ? "+" : "")}{Count} Health";
    }

    public override string GetPreview(PlayerState player)
    {
        int newValue = Math.Clamp(player.Health + Count, 0, player.MaxHealth);
        return $"({player.Health} -> {newValue})";
    }
}

public class CoinsOutcome : Outcome
{
    public int Count { get; }

    public CoinsOutcome(int count)
    {
        Count = count;
    }

    public override void Apply(PlayerState player)
    {
        player.ModifyCoins(Count);
    }

    public override string GetDescription()
    {
        return $"{(Count >= 0 ? "+" : "")}{Count} Coins";
    }

    public override string GetPreview(PlayerState player)
    {
        // Coins can't go below 0
        int newValue = Math.Max(0, player.Coins + Count);
        return $"({player.Coins} -> {newValue})";
    }
}

public class ResourceOutcome : Outcome
{
    public ResourceChangeTypes ChangeType { get; }
    public ResourceTypes Resource { get; }
    public int Count { get; set; }

    public ResourceOutcome(ResourceTypes resource, int count)
    {
        Resource = resource;
        Count = Math.Abs(count); // Store count as positive
        ChangeType = count >= 0 ? ResourceChangeTypes.Added : ResourceChangeTypes.Removed;
    }

    public override void Apply(PlayerState player)
    {
        player.ModifyResource(ChangeType, Resource, Count);
    }

    public override string GetDescription()
    {
        string action = ChangeType == ResourceChangeTypes.Added ? "Gain" : "Lose";
        return $"{action} {Count} {Resource}";
    }

    public override string GetPreview(PlayerState player)
    {
        int current = player.Inventory.GetItemCount(Resource);
        int change = ChangeType == ResourceChangeTypes.Added ? Count : -Count;
        int newValue = Math.Max(0, current + change);
        return $"({current} -> {newValue})";
    }
}

public class SkillLevelOutcome : Outcome
{
    public SkillTypes SkillType { get; }
    public int Count { get; }

    public SkillLevelOutcome(SkillTypes skillType, int count)
    {
        SkillType = skillType;
        Count = count;
    }

    public override void Apply(PlayerState player)
    {
        player.ModifySkillLevel(SkillType, Count);
    }

    public override string GetDescription()
    {
        return $"{(Count >= 0 ? "+" : "")}{Count} {SkillType} Skill";
    }

    public override string GetPreview(PlayerState player)
    {
        bool hasKey = player.Skills.ContainsKey(SkillType);
        if (!hasKey) { return $"(0 -> {Count})"; }

        int current = player.Skills[SkillType];
        int newValue = Math.Max(0, current + Count);
        return $"({current} -> {newValue})";
    }
}


// Achievements are one-time unlocks
public class AchievementOutcome : Outcome
{
    public AchievementTypes AchievementType { get; }

    public AchievementOutcome(AchievementTypes type)
    {
        AchievementType = type;
    }

    public override void Apply(PlayerState player)
    {
        player.UnlockAchievement(AchievementType);
    }

    public override string GetDescription()
    {
        return $"Achievement: {AchievementType}";
    }

    public override string GetPreview(PlayerState player)
    {
        bool hasAchievement = player.HasAchievement(AchievementType);
        return hasAchievement ? "(Already Unlocked)" : "(Will Unlock)";
    }
}

public class DayChangeOutcome : Outcome
{
    public override void Apply(PlayerState player)
    {
        // This outcome is just a marker - it doesn't actually do anything
        // The GameManager will see this outcome and handle day change effects
    }

    public override string GetDescription()
    {
        return "End the day";
    }

    public override string GetPreview(PlayerState player)
    {
        return "(Time will advance to morning)";
    }
}
