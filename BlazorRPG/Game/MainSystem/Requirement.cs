public interface IRequirement
{
    bool IsMet(GameWorld gameWorld);
    string GetDescription();
}

public class ActionPointRequirement : IRequirement
{
    public int RequiredAmount { get; }

    public ActionPointRequirement(int requiredAmount)
    {
        RequiredAmount = requiredAmount;
    }

    public bool IsMet(GameWorld gameWorld)
    {
        return gameWorld.GetPlayer().CurrentActionPoints() >= RequiredAmount;
    }

    public string GetDescription()
    {
        return $"Requires {RequiredAmount} Action Point(s)";
    }
}

public class StaminaRequirement : IRequirement
{
    public int RequiredAmount { get; }

    public StaminaRequirement(int requiredAmount)
    {
        RequiredAmount = requiredAmount;
    }

    public bool IsMet(GameWorld gameWorld)
    {
        return gameWorld.GetPlayer().Stamina >= RequiredAmount;
    }

    public string GetDescription()
    {
        return $"Requires {RequiredAmount} Stamina";
    }
}

public class HealthRequirement : IRequirement
{
    public int RequiredAmount { get; }

    public HealthRequirement(int requiredAmount)
    {
        RequiredAmount = requiredAmount;
    }

    public bool IsMet(GameWorld gameWorld)
    {
        return gameWorld.GetPlayer().Health >= RequiredAmount;
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

    public bool IsMet(GameWorld gameWorld)
    {
        return gameWorld.GetPlayer().Concentration >= RequiredAmount;
    }

    public string GetDescription()
    {
        return $"Requires {RequiredAmount} Concentration";
    }
}

public class CoinRequirement : IRequirement
{
    public int RequiredAmount { get; }

    public CoinRequirement(int requiredAmount)
    {
        RequiredAmount = requiredAmount;
    }

    public bool IsMet(GameWorld gameWorld)
    {
        return gameWorld.GetPlayer().Coins >= RequiredAmount;
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

    public bool IsMet(GameWorld gameWorld)
    {
        return gameWorld.GetPlayer().Food >= RequiredAmount;
    }

    public string GetDescription()
    {
        return $"Requires {RequiredAmount} Food";
    }
}

public class TimeWindowRequirement : IRequirement
{
    public List<TimeBlocks> AllowedWindows { get; }

    public TimeWindowRequirement(List<TimeBlocks> allowedWindows)
    {
        AllowedWindows = allowedWindows;
    }

    public bool IsMet(GameWorld gameWorld)
    {
        TimeBlocks currentWindow = gameWorld.TimeManager.GetCurrentTimeWindow();
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

    public bool IsMet(GameWorld gameWorld)
    {
        int currentValue = gameWorld.GetPlayer().GetRelationshipLevel(CharacterName);
        return currentValue >= MinimumValue;
    }

    public string GetDescription()
    {
        return $"Requires {MinimumValue}+ relationship with {CharacterName}";
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

    public bool IsMet(GameWorld gameWorld)
    {
        int currentLevel = gameWorld.GetPlayer().Skills.GetLevelForSkill(SkillType);
        return currentLevel >= RequiredLevel;
    }

    public string GetDescription()
    {
        return $"Requires {SkillType} level {RequiredLevel}+";
    }
}