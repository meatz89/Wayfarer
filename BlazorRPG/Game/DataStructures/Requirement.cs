public abstract class Requirement
{
    public abstract bool IsSatisfied(PlayerState player);
    public abstract string GetDescription();
}

public class EnergyRequirement : Requirement
{
    public EnergyTypes EnergyType { get; }
    public int Amount { get; }

    public EnergyRequirement(EnergyTypes type, int amount)
    {
        EnergyType = type;
        Amount = amount;
    }

    public override bool IsSatisfied(PlayerState player)
    {
        return EnergyType switch
        {
            EnergyTypes.Physical => player.PhysicalEnergy >= Amount,
            EnergyTypes.Focus => player.FocusEnergy >= Amount,
            EnergyTypes.Social => player.SocialEnergy >= Amount,
            _ => false
        };
    }

    public override string GetDescription()
    {
        return $"{EnergyType} Energy Required: {Amount}";
    }
}

public class HealthRequirement : Requirement
{
    public int Amount { get; }

    public HealthRequirement(int amount)
    {
        Amount = amount;
    }

    public override bool IsSatisfied(PlayerState player)
    {
        return player.Health >= Amount;
    }

    public override string GetDescription()
    {
        return $"Health Required: {Amount}";
    }
}

public class CoinsRequirement : Requirement
{
    public int Amount { get; }

    public CoinsRequirement(int amount)
    {
        Amount = amount;
    }

    public override bool IsSatisfied(PlayerState player)
    {
        return player.Coins >= Amount;
    }

    public override string GetDescription()
    {
        return $"Coins Required: {Amount}";
    }
}

public class ResourceRequirement : Requirement
{
    public ResourceTypes ResourceType { get; }
    public int Count { get; }

    public ResourceRequirement(ResourceTypes resourceType, int count)
    {
        ResourceType = resourceType;
        Count = count;
    }

    public override bool IsSatisfied(PlayerState player)
    {
        return player.Inventory.GetItemCount(ResourceType) >= Count;
    }

    public override string GetDescription()
    {
        return $"{ResourceType} Required: {Count}";
    }
}

public class InventorySlotsRequirement : Requirement
{
    public int Count { get; }

    public InventorySlotsRequirement(int count)
    {
        Count = count;
    }

    public override bool IsSatisfied(PlayerState player)
    {
        return player.Inventory.GetEmptySlots() >= Count;
    }

    public override string GetDescription()
    {
        return $"Empty Inventory Slots Required: {Count}";
    }
}

public class SkillLevelRequirement : Requirement
{
    public SkillTypes SkillType { get; }
    public int Amount { get; }

    public SkillLevelRequirement(SkillTypes skillType, int amount)
    {
        SkillType = skillType;
        Amount = amount;
    }

    public override bool IsSatisfied(PlayerState player)
    {
        return player.Skills.ContainsKey(SkillType) &&
               player.Skills[SkillType] >= Amount;
    }

    public override string GetDescription()
    {
        return $"{SkillType} Skill Level Required: {Amount}";
    }
}

public class ReputationRequirement : Requirement
{
    public ReputationTypes ReputationType { get; }
    public int Amount { get; }

    public ReputationRequirement(ReputationTypes type, int amount)
    {
        ReputationType = type;
        Amount = amount;
    }

    public override bool IsSatisfied(PlayerState player)
    {
        return player.GetReputationLevel(ReputationType) >= Amount;
    }

    public override string GetDescription()
    {
        return $"{ReputationType} Reputation Required: {Amount}";
    }
}

public class StatusRequirement : Requirement
{
    public StatusTypes Status { get; }

    public StatusRequirement(StatusTypes status)
    {
        Status = status;
    }

    public override bool IsSatisfied(PlayerState player)
    {
        return player.HasStatus(Status);
    }

    public override string GetDescription()
    {
        return $"Status Required: {Status}";
    }
}