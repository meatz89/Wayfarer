public abstract class Requirement
{
    public abstract bool IsSatisfied(GameState gameState);
    public abstract string GetDescription();
}

public class PressureRequirement : Requirement
{
    public int maxPressure;

    public PressureRequirement(int pressure)
    {
        maxPressure = pressure;
    }

    public override bool IsSatisfied(GameState gameState)
    {
        Encounter encounter = gameState.Player.CurrentEncounter;
        return encounter != null && encounter.Context.CurrentValues.Pressure <= maxPressure;
    }

    public override string GetDescription()
    {
        return $"Requires Pressure at {maxPressure} or below";
    }
}

public class MomentumRequirement : Requirement
{
    public int requiredMomentum;

    public MomentumRequirement(int momentum)
    {
        requiredMomentum = momentum;
    }

    public override bool IsSatisfied(GameState gameState)
    {
        Encounter encounter = gameState.Player.CurrentEncounter;
        return encounter != null && encounter.Context.CurrentValues.Momentum >= requiredMomentum;
    }

    public override string GetDescription()
    {
        return $"Requires Momentum at {requiredMomentum} or above";
    }
}


public class InsightRequirement : Requirement
{
    public int requiredInsight;

    public InsightRequirement(int insight)
    {
        requiredInsight = insight;
    }

    public override bool IsSatisfied(GameState gameState)
    {
        Encounter encounter = gameState.Player.CurrentEncounter;
        return encounter != null && encounter.Context.CurrentValues.Insight >= requiredInsight;
    }

    public override string GetDescription()
    {
        return $"Requires Insight at {requiredInsight} or above";
    }
}


public class ResonanceRequirement : Requirement
{
    public int requiredResonance;

    public ResonanceRequirement(int resonance)
    {
        requiredResonance = resonance;
    }

    public override bool IsSatisfied(GameState gameState)
    {
        Encounter encounter = gameState.Player.CurrentEncounter;
        return encounter != null && encounter.Context.CurrentValues.Resonance >= requiredResonance;
    }

    public override string GetDescription()
    {
        return $"Requires Resonance at {requiredResonance} or above";
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

    public override bool IsSatisfied(GameState gameState)
    {
        return EnergyType switch
        {
            EnergyTypes.Physical => gameState.Player.PhysicalEnergy >= Amount,
            EnergyTypes.Focus => gameState.Player.FocusEnergy >= Amount,
            EnergyTypes.Social => gameState.Player.SocialEnergy >= Amount,
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

public class ConcentrationRequirement : Requirement
{
    public int Count { get; }

    public ConcentrationRequirement(int count)
    {
        Count = count;
    }

    public override bool IsSatisfied(GameState gameState)
    {
        return gameState.Player.Concentration >= Count;
    }

    public override string GetDescription()
    {
        return $"Concentration Required: {Count}";
    }
}

public class ReputationRequirement : Requirement
{
    public int Count { get; }

    public ReputationRequirement(int count)
    {
        Count = count;
    }

    public override bool IsSatisfied(GameState gameState)
    {
        return gameState.Player.Reputation >= Count;
    }

    public override string GetDescription()
    {
        return $"Reputation Required: {Count}";
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
    public ResourceTypes ResourceType { get; }
    public int Count { get; }

    public ResourceRequirement(ResourceTypes resourceType, int count)
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
    public KnowledgeTypes KnowledgeType { get; }

    public KnowledgeRequirement(KnowledgeTypes knowledgeType)
    {
        KnowledgeType = knowledgeType;
    }

    public override bool IsSatisfied(GameState gameState)
    {
        return gameState.Player.HasKnowledge(KnowledgeType);
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

public class LocationPropertyRequirement : Requirement
{
    public LocationPropertyTypes Property { get; }

    public LocationPropertyRequirement(LocationPropertyTypes property)
    {
        Property = property;
    }

    public override bool IsSatisfied(GameState gameState)
    {
        return gameState.World.CurrentLocation.HasProperty(Property);
    }

    public override string GetDescription()
    {
        return $"Location Property Required: {Property}";
    }
}

public class TimeWindowRequirement : Requirement
{
    public TimeSlots TimeWindow { get; }

    public TimeWindowRequirement(TimeSlots timeWindow)
    {
        TimeWindow = timeWindow;
    }

    public override bool IsSatisfied(GameState gameState)
    {
        return gameState.World.CurrentTimeSlot == TimeWindow;
    }

    public override string GetDescription()
    {
        return $"Time Required: {TimeWindow}";
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

public class StatusRequirement : Requirement
{
    public PlayerStatusTypes Status { get; }

    public StatusRequirement(PlayerStatusTypes status)
    {
        Status = status;
    }

    public override bool IsSatisfied(GameState gameState)
    {
        return gameState.Player.HasStatus(Status);
    }

    public override string GetDescription()
    {
        return $"Status Required: {Status}";
    }
}