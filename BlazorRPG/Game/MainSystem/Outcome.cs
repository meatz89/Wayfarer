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

public class ActionPointOutcome : Outcome
{
    public int Amount { get; set; }

    public ActionPointOutcome(int count)
    {
        Amount = count;
    }

    public override void Apply(GameState gameState)
    {
        gameState.PlayerState.ActionPoints = Math.Clamp(
            gameState.PlayerState.ActionPoints + Amount,
            0,
            gameState.PlayerState.MaxActionPoints);
    }

    public override string GetDescription()
    {
        return $"{(Amount >= 0 ? "+" : "")}{Amount} Action Points";
    }

    public override string GetPreview(GameState gameState)
    {
        int currentValue = gameState.PlayerState.ActionPoints;
        int newValue = Math.Clamp(currentValue + Amount, 0, gameState.PlayerState.MaxActionPoints);
        return $"({currentValue} -> {newValue})";
    }
}

public class VigorOutcome : Outcome
{
    public int Amount { get; set; }

    public VigorOutcome(int count)
    {
        Amount = count;
    }

    public override void Apply(GameState gameState)
    {
        gameState.PlayerState.Vigor = Math.Clamp(
            gameState.PlayerState.Vigor + Amount,
            0,
            gameState.PlayerState.MaxVigor);
    }

    public override string GetDescription()
    {
        return $"{(Amount >= 0 ? "+" : "")}{Amount} Vigor";
    }

    public override string GetPreview(GameState gameState)
    {
        int currentValue = gameState.PlayerState.Vigor;
        int newValue = Math.Clamp(currentValue + Amount, 0, gameState.PlayerState.MaxVigor);
        return $"({currentValue} -> {newValue})";
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
        int newValue = Math.Clamp(gameState.PlayerState.Focus + Count, 0, gameState.PlayerState.MaxFocus);
        return $"({gameState.PlayerState.Focus} -> {newValue})";
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
        int newValue = Math.Clamp(gameState.PlayerState.Spirit + Count, 0, gameState.PlayerState.MaxSpirit);
        return $"({gameState.PlayerState.Spirit} -> {newValue})";
    }
}

public class CoinOutcome : Outcome
{
    public int Amount { get; }

    public CoinOutcome(int count)
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

public class RelationshipOutcome : Outcome
{
    public string CharacterName { get; }
    public int ChangeAmount { get; }

    public RelationshipOutcome(string characterName, int changeAmount)
    {
        CharacterName = characterName;
        ChangeAmount = changeAmount;
    }

    public override void Apply(GameState gameState)
    {
        gameState.PlayerState.UpdateRelationship(CharacterName, ChangeAmount);
    }

    public override string GetDescription()
    {
        return $"{(ChangeAmount >= 0 ? "+" : "")}{ChangeAmount} relationship with {CharacterName}";
    }

    public override string GetPreview(GameState gameState)
    {
        // Coins can't go below 0
        int newValue = Math.Max(0, gameState.PlayerState.GetRelationshipLevel(CharacterName) + ChangeAmount);
        return $"({gameState.PlayerState.GetRelationshipLevel(CharacterName)} -> {newValue})";
    }
}



public class FoodOutcome : Outcome
{
    public int Amount { get; }

    public FoodOutcome(int amount)
    {
        Amount = amount;
    }

    public override void Apply(GameState gameState)
    {
        gameState.PlayerState.ModifyFood(Amount);
    }

    public override string GetDescription()
    {
        return $"{(Amount >= 0 ? "+" : "")}{Amount} Food";
    }

    public override string GetPreview(GameState gameState)
    {
        int newValue = Math.Max(0, gameState.PlayerState.Food + Amount);
        return $"({gameState.PlayerState.Food} -> {newValue})";
    }
}

public class LocationKnowledgeOutcome : Outcome
{
    public string Location { get; }

    public LocationKnowledgeOutcome(string location)
    {
        Location = location;
    }

    public override void Apply(GameState gameState)
    {
        gameState.PlayerState.AddKnownLocation(Location);
    }

    public override string GetDescription()
    {
        return $"{Location} discovered";
    }

    public override string GetPreview(GameState gameState)
    {
        return $"{Location} discovered";
    }
}

public class LocationSpotKnowledgeOutcome : Outcome
{
    public string LocationSpot { get; }

    public LocationSpotKnowledgeOutcome(string locationSpot)
    {
        LocationSpot = locationSpot;
    }

    public override void Apply(GameState gameState)
    {
        gameState.PlayerState.AddKnownLocationSpot(LocationSpot);
    }

    public override string GetDescription()
    {
        return $"{LocationSpot} discovered";
    }

    public override string GetPreview(GameState gameState)
    {
        return $"{LocationSpot} discovered";
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