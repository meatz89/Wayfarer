public abstract class Requirement
{
    public abstract bool IsSatisfied(GameState gameState);
    public abstract string GetDescription();
}

public class EnergyRequirement : Requirement
{
    public int Amount { get; set; }

    public EnergyRequirement(int count)
    {
        Amount = count;
    }

    public override bool IsSatisfied(GameState gameState)
    {
        return gameState.PlayerState.Energy >= Amount;
    }

    public override string GetDescription()
    {
        return $"Energy Required: {Amount}";
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
        return gameState.PlayerState.Health >= Count;
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
        return gameState.PlayerState.Concentration >= Count;
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

    public override bool IsSatisfied(GameState gameState)
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

    public override bool IsSatisfied(GameState gameState)
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

    public override bool IsSatisfied(GameState gameState)
    {
        return gameState.PlayerState.Inventory.GetEmptySlots() >= Count;
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
        return gameState.PlayerState.HasKnowledge(value, requiredKnowledgeLevel);
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

    public override bool IsSatisfied(GameState gameState)
    {
        return gameState.PlayerState.HasStatusEffect(Status);
    }

    public override string GetDescription()
    {
        return $"Player Status Required: {Status}";
    }
}

public class PlayerConfidenceRequirement : Requirement
{
    public PlayerConfidenceTypes Confidence { get; }

    public PlayerConfidenceRequirement(PlayerConfidenceTypes confidenceType)
    {
        Confidence = confidenceType;
    }

    public override bool IsSatisfied(GameState gameState)
    {
        return gameState.PlayerState.HasConfidence(Confidence);
    }

    public override string GetDescription()
    {
        return $"Confidence Required: {Confidence}";
    }
}
