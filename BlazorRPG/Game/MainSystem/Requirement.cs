public abstract class Requirement
{
    public abstract bool IsMet(GameState gameState);
    public abstract string GetDescription();
}

public class SkillRequirement : Requirement
{
    public SkillTypes SkillType { get; set; }
    public int RequiredLevel { get; set; }

    public SkillRequirement(SkillTypes skillType, int requiredLevel)
    {
        SkillType = skillType;
        RequiredLevel = requiredLevel;
    }

    public override bool IsMet(GameState gameState)
    {
        return gameState.PlayerState.PlayerSkills.GetLevelForSkill(SkillType) >= RequiredLevel;
    }

    public override string GetDescription()
    {
        return $"Skill Level Required: {SkillType} {RequiredLevel}";
    }
}

public class TimeRequirement : Requirement
{
    public List<TimeWindow> PossibleTimeWindows { get; set; }

    public TimeRequirement(List<TimeWindow> timeWindows)
    {
        PossibleTimeWindows = timeWindows;
    }

    public override bool IsMet(GameState gameState)
    {
        if (PossibleTimeWindows == null || PossibleTimeWindows.Count == 0)
            return true;

        return PossibleTimeWindows.Contains(gameState.WorldState.TimeWindow);
    }

    public override string GetDescription()
    {
        if (PossibleTimeWindows == null || PossibleTimeWindows.Count == 0)
            return "Any Time";

        return $"Only Possible when one of: {string.Join(", ", PossibleTimeWindows)}";
    }
}

public class EnergyRequirement : Requirement
{
    public int Amount { get; set; }

    public EnergyRequirement(int count)
    {
        Amount = count;
    }

    public override bool IsMet(GameState gameState)
    {
        return gameState.PlayerState.Energy >= Amount;
    }

    public override string GetDescription()
    {
        return $"Energy Required: {Amount}";
    }
}

public class CoidRequirement : Requirement
{
    public int Amount { get; set; }

    public CoidRequirement(int count)
    {
        Amount = count;
    }

    public override bool IsMet(GameState gameState)
    {
        return gameState.PlayerState.Coins >= Amount;
    }

    public override string GetDescription()
    {
        return $"Coid Required: {Amount}";
    }
}

public class FoodRequirement : Requirement
{
    public int Amount { get; set; }

    public FoodRequirement(int count)
    {
        Amount = count;
    }

    public override bool IsMet(GameState gameState)
    {
        return gameState.PlayerState.Food >= Amount;
    }

    public override string GetDescription()
    {
        return $"Food Required: {Amount}";
    }
}

public class HealthRequirement : Requirement
{
    public int Count { get; }

    public HealthRequirement(int count)
    {
        Count = count;
    }

    public override bool IsMet(GameState gameState)
    {
        return gameState.PlayerState.Health >= Count;
    }

    public override string GetDescription()
    {
        return $"Health Required: {Count}";
    }
}

public class ConcentrationRequirement : Requirement
{
    public int Count { get; }

    public ConcentrationRequirement(int count)
    {
        Count = count;
    }

    public override bool IsMet(GameState gameState)
    {
        return gameState.PlayerState.Concentration >= Count;
    }

    public override string GetDescription()
    {
        return $"Concentration Required: {Count}";
    }
}

public class ConfidenceRequirement : Requirement
{
    public int Count { get; }

    public ConfidenceRequirement(int count)
    {
        Count = count;
    }

    public override bool IsMet(GameState gameState)
    {
        return gameState.PlayerState.Confidence >= Count;
    }

    public override string GetDescription()
    {
        return $"ConfidenceRequirement Required: {Count}";
    }
}

public class CoinRequirement : Requirement
{
    public int Count { get; }

    public CoinRequirement(int count)
    {
        Count = count;
    }

    public override bool IsMet(GameState gameState)
    {
        return gameState.PlayerState.Coins >= Count;
    }

    public override string GetDescription()
    {
        return $"Coins Required: {Count}";
    }
}

public class ItemRequirement : Requirement
{
    public ItemTypes ResourceType { get; }

    public ItemRequirement(ItemTypes resourceType)
    {
        ResourceType = resourceType;
    }

    public override bool IsMet(GameState gameState)
    {
        return gameState.PlayerState.Inventory.GetItemCount(ResourceType.ToString()) > 0;
    }

    public override string GetDescription()
    {
        return $"{ResourceType} Required";
    }
}

public class ResourceRequirement : Requirement
{
    public ItemTypes ResourceType { get; }
    public int Count { get; }

    public ResourceRequirement(ItemTypes resourceType, int count)
    {
        ResourceType = resourceType;
        Count = count;
    }

    public override bool IsMet(GameState gameState)
    {
        return gameState.PlayerState.Inventory.GetItemCount(ResourceType.ToString()) >= Count;
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

    public override bool IsMet(GameState gameState)
    {
        return gameState.PlayerState.Inventory.GetEmptySlots() >= Count;
    }

    public override string GetDescription()
    {
        return $"Empty Inventory Slots Required: {Count}";
    }
}

public class RelationshipRequirement : Requirement
{
    public string Character { get; }
    public int Level { get; }

    public RelationshipRequirement(string character, int level)
    {
        Character = character;
        Level = level;
    }

    public override bool IsMet(GameState gameState)
    {
        return gameState.PlayerState.GetRelationshipLevel(Character) >= Level;
    }

    public override string GetDescription()
    {
        return $"Relationship with {Character} Required: {Level}";
    }
}

public class PlayerNegativeStatusRequirement : Requirement
{
    public PlayerNegativeStatus Status { get; }

    public PlayerNegativeStatusRequirement(PlayerNegativeStatus property)
    {
        Status = property;
    }

    public override bool IsMet(GameState gameState)
    {
        return gameState.PlayerState.HasStatusEffect(Status);
    }

    public override string GetDescription()
    {
        return $"Player Status Required: {Status}";
    }
}
