public class SkillCheckResolver
{
    public SkillCheckResult Resolve(SkillOption option, ConversationState state)
    {
        // Skill cards removed - using letter queue and token system
        int effectiveLevel = 1;
        int modifiedDifficulty = option.Difficulty.Length;
        bool isUntrained = false;

        bool success = effectiveLevel >= modifiedDifficulty;

        return new SkillCheckResult
        {
            SkillName = option.RequiredSkillName,
            PlayerLevel = effectiveLevel,
            RequiredLevel = modifiedDifficulty,
            IsSuccess = success,
            IsUntrained = isUntrained,
            SuccessChance = CalculateSuccessChance(effectiveLevel, modifiedDifficulty)
        };
    }

    // Skill cards removed - conversation system uses tokens and relationships

    private int CalculateSuccessChance(int level, int difficulty)
    {
        int difference = level - difficulty;
        if (difference >= 2) return 100;
        if (difference == 1) return 75;
        if (difference == 0) return 50;
        if (difference == -1) return 25;
        return 5;
    }
}