using System.Xml.Schema;

public abstract class Outcome
{
    public abstract void Apply(PlayerState player);
    public abstract string GetDescription();

    // Method to preview outcome without applying it
    public abstract string GetPreview(PlayerState player);

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
        }
    }

    public override string GetDescription()
    {
        return "Item Change";
    }

    public override string GetPreview(PlayerState player)
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

    public override void Apply(PlayerState player)
    {
        player.Knowledge.Add(new KnowledgePiece(knowledgeTag, KnowledgeCategories.Commerce));
    }

    public override string GetDescription()
    {
        return "Knowledge";
    }

    public override string GetPreview(PlayerState player)
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

    public override void Apply(PlayerState player)
    {
        player.Energy = Math.Clamp(
            player.Energy + Amount,
            0,
            player.MaxEnergy);
    }

    public override string GetDescription()
    {
        return $"{(Amount >= 0 ? "+" : "")}{Amount} Energy";
    }

    public override string GetPreview(PlayerState player)
    {
        int currentValue = player.Energy;
        int newValue = Math.Clamp(currentValue + Amount, 0, player.MaxEnergy);
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

public class ConcentrationOutcome : Outcome
{
    public int Count { get; }

    public ConcentrationOutcome(int count)
    {
        Count = count;
    }

    public override void Apply(PlayerState player)
    {
        player.ModifyConcentration(Count);
    }

    public override string GetDescription()
    {
        return $"{(Count >= 0 ? "+" : "")}{Count} Concentration";
    }

    public override string GetPreview(PlayerState player)
    {
        int newValue = Math.Clamp(player.Concentration + Count, 0, player.MaxConcentration);
        return $"({player.Concentration} -> {newValue})";
    }
}

public class ConfidenceOutcome : Outcome
{
    public int Count { get; }

    public ConfidenceOutcome(int count)
    {
        Count = count;
    }

    public override void Apply(PlayerState player)
    {
        player.ModifyConfidence(Count);
    }

    public override string GetDescription()
    {
        return $"{(Count >= 0 ? "+" : "")}{Count} Confidence";
    }

    public override string GetPreview(PlayerState player)
    {
        int newValue = Math.Clamp(player.Confidence + Count, 0, player.MaxConfidence);
        return $"({player.Confidence} -> {newValue})";
    }
}

public class CoinsOutcome : Outcome
{
    public int Amount { get; }

    public CoinsOutcome(int count)
    {
        Amount = count;
    }

    public override void Apply(PlayerState player)
    {
        player.AddCoins(Amount);
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

    public override void Apply(PlayerState player)
    {
        player.ModifyResource(ChangeType, ResourceType, Amount);
    }

    public override string GetDescription()
    {
        string action = ChangeType == ResourceChangeTypes.Added ? "Gain" : "Lose";
        return $"{action} {Amount} {ResourceType}";
    }

    public override string GetPreview(PlayerState player)
    {
        int current = player.Inventory.GetItemCount(ResourceType.ToString());
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