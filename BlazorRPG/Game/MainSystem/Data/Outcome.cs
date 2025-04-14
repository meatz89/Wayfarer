using System.Xml.Schema;

public abstract class Outcome
{
    public abstract void Apply(GameState gameState);
    public abstract string GetDescription();

    // Method to preview outcome without applying it
    public abstract string GetPreview(GameState gameState);

    public override string ToString()
    {
        return GetDescription();
    }
}

public class ItemOutcome : Outcome
{
    public ItemTypes ItemType { get; set; }
    public int QuantityChange { get; set; }

    public ItemOutcome(ItemTypes itemType, int quantityChange)
    {
        ItemType = itemType;
        QuantityChange = quantityChange;
    }

    public override void Apply(GameState gameState)
    {
        if (QuantityChange > 0)
        {
            // Add item(s)
            for (int i = 0; i < QuantityChange; i++)
            {
                gameState.PlayerState.Inventory.AddItem(ItemType);
            }
        }
        else
        {
        }
    }

    public override string GetDescription()
    {
        return "Item Change";
    }

    public override string GetPreview(GameState gameState)
    {
        return "Item Change";
    }
}

public class KnowledgeOutcome : Outcome
{
    private readonly KnowledgeTags knowledgeTag;
    private readonly KnowledgeCategories knowledgeCategory;

    public KnowledgeOutcome(KnowledgeTags value, KnowledgeCategories knowledgeCategory)
    {
        this.knowledgeTag = value;
        this.knowledgeCategory = knowledgeCategory;
    }

    public override void Apply(GameState gameState)
    {
        gameState.PlayerState.Knowledge.Add(new KnowledgePiece(knowledgeTag, KnowledgeCategories.Commerce));
    }

    public override string GetDescription()
    {
        return "Knowledge";
    }

    public override string GetPreview(GameState gameState)
    {
        return "Knowledge";
    }
}

public class EnergyOutcome : Outcome
{
    public int Amount { get; set; }

    public EnergyOutcome(int count)
    {
        Amount = count;
    }

    public override void Apply(GameState gameState)
    {
        gameState.PlayerState.Energy = Math.Clamp(
            gameState.PlayerState.Energy + Amount,
            0,
            gameState.PlayerState.MaxEnergy);
    }

    public override string GetDescription()
    {
        return $"{(Amount >= 0 ? "+" : "")}{Amount} Energy";
    }

    public override string GetPreview(GameState gameState)
    {
        int currentValue = gameState.PlayerState.Energy;
        int newValue = Math.Clamp(currentValue + Amount, 0, gameState.PlayerState.MaxEnergy);
        return $"({currentValue} -> {newValue})";
    }
}

public class TimeOutcome : Outcome
{
    public int hours { get; set; }

    public TimeOutcome(int count)
    {
        hours = count;
    }

    public override void Apply(GameState gameState)
    {
        gameState.WorldState.AdvanceTime(hours);
    }

    public override string GetDescription()
    {
        return $"{(hours >= 0 ? "+" : "")}{hours} Energy";
    }

    public override string GetPreview(GameState gameState)
    {
        int currentValue = gameState.WorldState.CurrentTimeInHours;
        int newValue = gameState.WorldState.CurrentTimeInHours + hours;
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

    public override void Apply(GameState gameState)
    {
        gameState.PlayerState.ModifyHealth(Count);
    }

    public override string GetDescription()
    {
        return $"{(Count >= 0 ? "+" : "")}{Count} Health";
    }

    public override string GetPreview(GameState gameState)
    {
        int newValue = Math.Clamp(gameState.PlayerState.Health + Count, 0, gameState.PlayerState.MaxHealth);
        return $"({gameState.PlayerState.Health} -> {newValue})";
    }
}

public class ConcentrationOutcome : Outcome
{
    public int Count { get; }

    public ConcentrationOutcome(int count)
    {
        Count = count;
    }

    public override void Apply(GameState gameState)
    {
        gameState.PlayerState.ModifyConcentration(Count);
    }

    public override string GetDescription()
    {
        return $"{(Count >= 0 ? "+" : "")}{Count} Concentration";
    }

    public override string GetPreview(GameState gameState)
    {
        int newValue = Math.Clamp(gameState.PlayerState.Concentration + Count, 0, gameState.PlayerState.MaxConcentration);
        return $"({gameState.PlayerState.Concentration} -> {newValue})";
    }
}

public class ConfidenceOutcome : Outcome
{
    public int Count { get; }

    public ConfidenceOutcome(int count)
    {
        Count = count;
    }

    public override void Apply(GameState gameState)
    {
        gameState.PlayerState.ModifyConfidence(Count);
    }

    public override string GetDescription()
    {
        return $"{(Count >= 0 ? "+" : "")}{Count} Confidence";
    }

    public override string GetPreview(GameState gameState)
    {
        int newValue = Math.Clamp(gameState.PlayerState.Confidence + Count, 0, gameState.PlayerState.MaxConfidence);
        return $"({gameState.PlayerState.Confidence} -> {newValue})";
    }
}

public class CoinsOutcome : Outcome
{
    public int Amount { get; }

    public CoinsOutcome(int count)
    {
        Amount = count;
    }

    public override void Apply(GameState gameState)
    {
        gameState.PlayerState.AddCoins(Amount);
    }

    public override string GetDescription()
    {
        return $"{(Amount >= 0 ? "+" : "")}{Amount} Coins";
    }

    public override string GetPreview(GameState gameState)
    {
        // Coins can't go below 0
        int newValue = Math.Max(0, gameState.PlayerState.Coins + Amount);
        return $"({gameState.PlayerState.Coins} -> {newValue})";
    }
}

public class ResourceOutcome : Outcome
{
    public ResourceChangeTypes ChangeType { get; }
    public ItemTypes ResourceType { get; }
    public int Amount { get; set; }

    public ResourceOutcome(ItemTypes resource, int count)
    {
        ResourceType = resource;
        Amount = Math.Abs(count); // Store count as positive
        ChangeType = count >= 0 ? ResourceChangeTypes.Added : ResourceChangeTypes.Removed;
    }

    public override void Apply(GameState gameState)
    {
        gameState.PlayerState.ModifyResource(ChangeType, ResourceType, Amount);
    }

    public override string GetDescription()
    {
        string action = ChangeType == ResourceChangeTypes.Added ? "Gain" : "Lose";
        return $"{action} {Amount} {ResourceType}";
    }

    public override string GetPreview(GameState gameState)
    {
        int current = gameState.PlayerState.Inventory.GetItemCount(ResourceType.ToString());
        int change = ChangeType == ResourceChangeTypes.Added ? Amount : -Amount;
        int newValue = Math.Max(0, current + change);
        return $"({current} -> {newValue})";
    }
}

public class AchievementOutcome : Outcome
{
    public AchievementTypes AchievementType { get; }

    public AchievementOutcome(AchievementTypes type)
    {
        AchievementType = type;
    }

    public override void Apply(GameState gameState)
    {
        gameState.PlayerState.UnlockAchievement(AchievementType);
    }

    public override string GetDescription()
    {
        return $"Achievement: {AchievementType}";
    }

    public override string GetPreview(GameState gameState)
    {
        bool hasAchievement = gameState.PlayerState.HasAchievement(AchievementType);
        return hasAchievement ? "(Already Unlocked)" : "(Will Unlock)";
    }
}

public class DayChangeOutcome : Outcome
{
    public override void Apply(GameState gameState)
    {
        // This outcome is just a marker - it doesn't actually do anything
        // The GameManager will see this outcome and handle day change effects
    }

    public override string GetDescription()
    {
        return "End the day";
    }

    public override string GetPreview(GameState gameState)
    {
        return "(Time will advance to morning)";
    }
}