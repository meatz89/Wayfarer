public interface IRequirement
{
    bool IsMet(GameState gameState);
    string GetDescription();
}

public class ActionPointRequirement : IRequirement
{
    public int RequiredAmount { get; }

    public ActionPointRequirement(int requiredAmount)
    {
        RequiredAmount = requiredAmount;
    }

    public bool IsMet(GameState gameState)
    {
        return gameState.PlayerState.ActionPoints >= RequiredAmount;
    }

    public string GetDescription()
    {
        return $"Requires {RequiredAmount} Action Point(s)";
    }
}

public class VigorRequirement : IRequirement
{
    public int RequiredAmount { get; }

    public VigorRequirement(int requiredAmount)
    {
        RequiredAmount = requiredAmount;
    }

    public bool IsMet(GameState gameState)
    {
        return gameState.PlayerState.Vigor >= RequiredAmount;
    }

    public string GetDescription()
    {
        return $"Requires {RequiredAmount} Vigor";
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
            return gameState.PlayerState.Focus >= RequiredAmount;
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
            return gameState.PlayerState.Spirit >= RequiredAmount;
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
        int currentLevel = gameState.PlayerState.Skills.GetLevelForSkill(SkillType);
        return currentLevel >= RequiredLevel;
    }

    public string GetDescription()
    {
        return $"Requires {SkillType} level {RequiredLevel}+";
    }
}