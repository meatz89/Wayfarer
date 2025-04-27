public interface IRequirement
{
    bool IsMet(GameState gameState);
    string GetDescription();
}

public class TimeWindowRequirement : IRequirement
{
    public List<TimeWindow> AllowedWindows { get; }

    public TimeWindowRequirement(List<TimeWindow> allowedWindows)
    {
        AllowedWindows = allowedWindows;
    }

    public bool IsMet(GameState gameState)
    {
        TimeWindow currentWindow = gameState.TimeManager.GetCurrentTimeWindow();
        return AllowedWindows.Contains(currentWindow);
    }

    public string GetDescription()
    {
        return $"Available during: {string.Join(", ", AllowedWindows)}";
    }
}

public class RelationshipRequirement : IRequirement
{
    public string CharacterName { get; }
    public int MinimumValue { get; }

    public RelationshipRequirement(string characterName, int minimumValue)
    {
        CharacterName = characterName;
        MinimumValue = minimumValue;
    }

    public bool IsMet(GameState gameState)
    {
        int currentValue = gameState.PlayerState.GetRelationshipLevel(CharacterName);
        return currentValue >= MinimumValue;
    }

    public string GetDescription()
    {
        return $"Requires {MinimumValue}+ relationship with {CharacterName}";
    }
}

public class ReputationRequirement : IRequirement
{
    public string Location { get; }
    public int MinimumValue { get; }

    public ReputationRequirement(string location, int minimumValue)
    {
        Location = location;
        MinimumValue = minimumValue;
    }

    public bool IsMet(GameState gameState)
    {
        int currentValue = gameState.PlayerState.GetReputation(Location);
        return currentValue >= MinimumValue;
    }

    public string GetDescription()
    {
        return $"Requires {MinimumValue}+ reputation in {Location}";
    }
}

public class SkillRequirement : IRequirement
{
    public SkillTypes SkillType { get; }
    public int RequiredLevel { get; }

    public SkillRequirement(SkillTypes skillType, int minimumLevel)
    {
        SkillType = skillType;
        RequiredLevel = minimumLevel;
    }

    public bool IsMet(GameState gameState)
    {
        int currentLevel = gameState.PlayerState.PlayerSkills.GetLevelForSkill(SkillType);
        return currentLevel >= RequiredLevel;
    }

    public string GetDescription()
    {
        return $"Requires {SkillType} level {RequiredLevel}+";
    }
}
public class EnergyRequirement : IRequirement
{
    public int RequiredAmount { get; }

    public EnergyRequirement(int requiredAmount)
    {
        RequiredAmount = requiredAmount;
    }

    public bool IsMet(GameState gameState)
    {
        return gameState.PlayerState.Energy >= RequiredAmount;
    }

    public string GetDescription()
    {
        return $"Requires {RequiredAmount} Energy";
    }
}

public class HealthRequirement : IRequirement
{
    public int RequiredAmount { get; }

    public HealthRequirement(int requiredAmount)
    {
        RequiredAmount = requiredAmount;
    }

    public bool IsMet(GameState gameState)
    {
        return gameState.PlayerState.Health >= RequiredAmount;
    }

    public string GetDescription()
    {
        return $"Requires {RequiredAmount} Health";
    }
}

public class ConcentrationRequirement : IRequirement
{
    public int RequiredAmount { get; }

    public ConcentrationRequirement(int requiredAmount)
    {
        RequiredAmount = requiredAmount;
    }

    public bool IsMet(GameState gameState)
    {
        return gameState.PlayerState.Concentration >= RequiredAmount;
    }

    public string GetDescription()
    {
        return $"Requires {RequiredAmount} Concentration";
    }
}

public class ConfidenceRequirement : IRequirement
{
    public int RequiredAmount { get; }

    public ConfidenceRequirement(int requiredAmount)
    {
        RequiredAmount = requiredAmount;
    }

    public bool IsMet(GameState gameState)
    {
        return gameState.PlayerState.Confidence >= RequiredAmount;
    }

    public string GetDescription()
    {
        return $"Requires {RequiredAmount} Confidence";
    }
}

public class CoinRequirement : IRequirement
{
    public int RequiredAmount { get; }

    public CoinRequirement(int requiredAmount)
    {
        RequiredAmount = requiredAmount;
    }

    public bool IsMet(GameState gameState)
    {
        return gameState.PlayerState.Coins >= RequiredAmount;
    }

    public string GetDescription()
    {
        return $"Requires {RequiredAmount} Coins";
    }
}

public class FoodRequirement : IRequirement
{
    public int RequiredAmount { get; }

    public FoodRequirement(int requiredAmount)
    {
        RequiredAmount = requiredAmount;
    }

    public bool IsMet(GameState gameState)
    {
        return gameState.PlayerState.Food >= RequiredAmount;
    }

    public string GetDescription()
    {
        return $"Requires {RequiredAmount} Food";
    }
}
