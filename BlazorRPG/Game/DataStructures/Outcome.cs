using System.Numerics;

public abstract class Outcome
{
    public abstract void Apply(PlayerState player);
    public abstract string GetDescription();

    // Method to preview outcome without applying it
    public abstract string GetPreview(PlayerState player);
}


// Example of a concrete outcome
public class EnergyOutcome : Outcome
{
    public EnergyTypes EnergyType { get; }
    public int Amount { get; }

    public EnergyOutcome(EnergyTypes type, int amount)
    {
        EnergyType = type;
        Amount = amount;
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
    public int Amount { get; }

    public HealthOutcome(int amount)
    {
        Amount = amount;
    }

    public override void Apply(PlayerState player)
    {
        // Uses Player's built-in clamping
        player.ModifyHealth(Amount);
    }

    public override string GetDescription()
    {
        return $"{(Amount >= 0 ? "+" : "")}{Amount} Health";
    }

    public override string GetPreview(PlayerState player)
    {
        int newValue = Math.Clamp(player.Health + Amount, 0, player.MaxHealth);
        return $"({player.Health} -> {newValue})";
    }
}

public class CoinsOutcome : Outcome
{
    public int Amount { get; }

    public CoinsOutcome(int amount)
    {
        Amount = amount;
    }

    public override void Apply(PlayerState player)
    {
        player.ModifyCoins(Amount);
    }

    public override string GetDescription()
    {
        return $"{(Amount >= 0 ? "+" : "")}{Amount} Coins";
    }

    public override string GetPreview(PlayerState player)
    {
        // Coins can't go below 0
        int newValue = Math.Max(0, player.Coins + Amount);
        return $"({player.Coins} -> {newValue})";
    }
}

// Resource outcomes handle inventory changes
public class ResourceOutcome : Outcome
{
    public ResourceChangeType ChangeType { get; }
    public ResourceTypes Resource { get; }
    public int Count { get; }

    public ResourceOutcome(ResourceTypes resource, int count)
    {
        Resource = resource;
        Count = Math.Abs(count); // Store count as positive
        ChangeType = count >= 0 ? ResourceChangeType.Added : ResourceChangeType.Removed;
    }

    public override void Apply(PlayerState player)
    {
        player.ModifyItem(ChangeType, Resource, Count);
    }

    public override string GetDescription()
    {
        string action = ChangeType == ResourceChangeType.Added ? "Gain" : "Lose";
        return $"{action} {Count} {Resource}";
    }

    public override string GetPreview(PlayerState player)
    {
        int current = player.Inventory.GetItemCount(Resource);
        int change = ChangeType == ResourceChangeType.Added ? Count : -Count;
        int newValue = Math.Max(0, current + change);
        return $"({current} -> {newValue})";
    }
}

// Skill outcomes modify player abilities
public class SkillLevelOutcome : Outcome
{
    public SkillTypes SkillType { get; }
    public int Amount { get; }

    public SkillLevelOutcome(SkillTypes skillType, int amount)
    {
        SkillType = skillType;
        Amount = amount;
    }

    public override void Apply(PlayerState player)
    {
        player.ModifySkillLevel(SkillType, Amount);
    }

    public override string GetDescription()
    {
        return $"{(Amount >= 0 ? "+" : "")}{Amount} {SkillType} Skill";
    }

    public override string GetPreview(PlayerState player)
    {
        int current = player.Skills[SkillType];
        int newValue = Math.Max(0, current + Amount);
        return $"({current} -> {newValue})";
    }
}

// Reputation changes affect standing with groups
public class ReputationOutcome : Outcome
{
    public ReputationTypes ReputationType { get; }
    public int Amount { get; }

    public ReputationOutcome(ReputationTypes type, int amount)
    {
        ReputationType = type;
        Amount = amount;
    }

    public override void Apply(PlayerState player)
    {
        // This would need a ReputationSystem reference or player method
        player.ModifyReputation(ReputationType, Amount);
    }

    public override string GetDescription()
    {
        return $"{(Amount >= 0 ? "+" : "")}{Amount} {ReputationType} Reputation";
    }

    public override string GetPreview(PlayerState player)
    {
        int current = player.GetReputationLevel(ReputationType);
        int newValue = Math.Clamp(current + Amount, 0, 100); // Assuming 0-100 scale
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
