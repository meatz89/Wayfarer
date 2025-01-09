public abstract class Requirement
{
    public abstract bool IsSatisfied(PlayerState player);
    public abstract string GetDescription();
}

public class InsightRequirement : Requirement
{
    private readonly int requiredLevel;

    public InsightRequirement(int level)
    {
        requiredLevel = level;
    }

    public override bool IsSatisfied(PlayerState player)
    {
        Encounter encounter = player.CurrentEncounter;
        return encounter != null && encounter.EncounterContext.CurrentValues.Insight >= requiredLevel;
    }

    public override string GetDescription()
    {
        return $"Requires Insight level {requiredLevel}";
    }
}

public class EnergyRequirement : Requirement
{
    public EnergyTypes EnergyType { get; }
    public int Amount { get; set; }

    public EnergyRequirement(EnergyTypes type, int count)
    {
        EnergyType = type;
        Amount = count;
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
    public int Count { get; }

    public HealthRequirement(int count)
    {
        Count = count;
    }

    public override bool IsSatisfied(PlayerState player)
    {
        return player.Health >= Count;
    }

    public override string GetDescription()
    {
        return $"Health Required: {Count}";
    }
}

public class CoinsRequirement : Requirement
{
    public int Count { get; }

    public CoinsRequirement(int count)
    {
        Count = count;
    }

    public override bool IsSatisfied(PlayerState player)
    {
        return player.Coins >= Count;
    }

    public override string GetDescription()
    {
        return $"Coins Required: {Count}";
    }
}

public class SkillRequirement : Requirement
{
    public SkillTypes SkillType { get; }
    public int Level { get; }

    public SkillRequirement(SkillTypes skillType, int level)
    {
        SkillType = skillType;
        Level = level;
    }

    public override bool IsSatisfied(PlayerState player)
    {
        if (!player.Skills.ContainsKey(SkillType)) return false;
        int actualLevel = player.Skills[SkillType];
        return actualLevel >= Level;
    }

    public override string GetDescription()
    {
        return $"{SkillType} Level Required: {Level}";
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
    public int Count { get; }

    public SkillLevelRequirement(SkillTypes skillType, int count)
    {
        SkillType = skillType;
        Count = count;
    }

    public override bool IsSatisfied(PlayerState player)
    {
        return player.Skills.ContainsKey(SkillType) &&
               player.Skills[SkillType] >= Count;
    }

    public override string GetDescription()
    {
        return $"{SkillType} Skill Level Required: {Count}";
    }
}

public class ReputationRequirement : Requirement
{
    public ReputationTypes ReputationType { get; }
    public int Count { get; }

    public ReputationRequirement(ReputationTypes type, int count)
    {
        ReputationType = type;
        Count = count;
    }

    public override bool IsSatisfied(PlayerState player)
    {
        return player.GetReputationLevel(ReputationType) >= Count;
    }

    public override string GetDescription()
    {
        return $"{ReputationType} Reputation Required: {Count}";
    }
}

public class StatusRequirement : Requirement
{
    public PlayerStatusTypes Status { get; }

    public StatusRequirement(PlayerStatusTypes status)
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


