public abstract class Requirement
{
    public abstract bool IsSatisfied(GameState gameState);
    public abstract string GetDescription();
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

    public override bool IsSatisfied(GameState gameState)
    {
        return EnergyType switch
        {
            EnergyTypes.Physical => gameState.Player.PhysicalEnergy >= Amount,
            EnergyTypes.Focus => gameState.Player.Focus >= Amount,
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

    public override bool IsSatisfied(GameState gameState)
    {
        return gameState.Player.Health >= Count;
    }

    public override string GetDescription()
    {
        return $"Health Required: {Count}";
    }
}

public class FocusRequirement : Requirement
{
    public int Count { get; }

    public FocusRequirement(int count)
    {
        Count = count;
    }

    public override bool IsSatisfied(GameState gameState)
    {
        return gameState.Player.Focus >= Count;
    }

    public override string GetDescription()
    {
        return $"Focus Required: {Count}";
    }
}

public class CoinsRequirement : Requirement
{
    public int Count { get; }

    public CoinsRequirement(int count)
    {
        Count = count;
    }

    public override bool IsSatisfied(GameState gameState)
    {
        return gameState.Player.Coins >= Count;
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

    public override bool IsSatisfied(GameState gameState)
    {
        if (!gameState.Player.Skills.ContainsKey(SkillType)) return false;
        int actualLevel = gameState.Player.Skills[SkillType];
        return actualLevel >= Level;
    }

    public override string GetDescription()
    {
        return $"{SkillType} Level Required: {Level}";
    }
}

public class ItemRequirement : Requirement
{
    public ItemTypes ResourceType { get; }

    public ItemRequirement(ItemTypes resourceType)
    {
        ResourceType = resourceType;
    }

    public override bool IsSatisfied(GameState gameState)
    {
        return gameState.Player.Inventory.GetItemCount(ResourceType) > 0;
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

    public override bool IsSatisfied(GameState gameState)
    {
        return gameState.Player.Inventory.GetItemCount(ResourceType) >= Count;
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

    public override bool IsSatisfied(GameState gameState)
    {
        return gameState.Player.Inventory.GetEmptySlots() >= Count;
    }

    public override string GetDescription()
    {
        return $"Empty Inventory Slots Required: {Count}";
    }
}

public class KnowledgeRequirement : Requirement
{
    private KnowledgeTags value;
    private int requiredKnowledgeLevel;

    public KnowledgeTags KnowledgeType { get; }

    public KnowledgeRequirement(KnowledgeTags value, int requiredKnowledgeLevel)
    {
        this.value = value;
        this.requiredKnowledgeLevel = requiredKnowledgeLevel;
    }

    public override bool IsSatisfied(GameState gameState)
    {
        return gameState.Player.HasKnowledge(value, requiredKnowledgeLevel);
    }

    public override string GetDescription()
    {
        return $"{KnowledgeType} Skill Level Required";
    }
}
public class RelationshipRequirement : Requirement
{
    public CharacterTypes Character { get; }
    public int Level { get; }

    public RelationshipRequirement(CharacterTypes character, int level)
    {
        Character = character;
        Level = level;
    }

    public override bool IsSatisfied(GameState gameState)
    {
        return gameState.Player.GetRelationshipLevel(Character) >= Level;
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

    public override bool IsSatisfied(GameState gameState)
    {
        return gameState.Player.HasStatusEffect(Status);
    }

    public override string GetDescription()
    {
        return $"Player Status Required: {Status}";
    }
}

public class PlayerConfidenceRequirement : Requirement
{
    public PlayerConfidenceTypes Confidence { get; }

    public PlayerConfidenceRequirement(PlayerConfidenceTypes reputationType)
    {
        Confidence = reputationType;
    }

    public override bool IsSatisfied(GameState gameState)
    {
        return gameState.Player.HasConfidence(Confidence);
    }

    public override string GetDescription()
    {
        return $"Confidence Required: {Confidence}";
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

    public override bool IsSatisfied(GameState gameState)
    {
        return gameState.Player.Skills.ContainsKey(SkillType) &&
               gameState.Player.Skills[SkillType] >= Count;
    }

    public override string GetDescription()
    {
        return $"{SkillType} Skill Level Required: {Count}";
    }
}
