public class SkillCheckResult
{
    public SkillTypes Skill { get; }
    public int EffectiveLevel { get; }
    public int Difficulty { get; }
    public bool IsSuccess { get; }
    public bool IsUntrained { get; }

    public SkillCheckResult(SkillTypes skill, int effectiveLevel, int difficulty, bool isSuccess, bool isUntrained)
    {
        Skill = skill;
        EffectiveLevel = effectiveLevel;
        Difficulty = difficulty;
        IsSuccess = isSuccess;
        IsUntrained = isUntrained;
    }

    public string GetDifficultyLabel()
    {
        int difference = EffectiveLevel - Difficulty;

        if (difference >= 2) return "Trivial (100%)";
        if (difference == 1) return "Easy (75%)";
        if (difference == 0) return "Standard (50%)";
        if (difference == -1) return "Hard (25%)";
        return "Exceptional (5%)";
    }

    public int GetSuccessChance()
    {
        int difference = EffectiveLevel - Difficulty;

        if (difference >= 2) return 100;
        if (difference == 1) return 75;
        if (difference == 0) return 50;
        if (difference == -1) return 25;
        return 5;
    }
}