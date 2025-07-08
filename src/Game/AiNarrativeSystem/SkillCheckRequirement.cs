public class SkillCheckRequirement
{
    public SkillCategories SkillCategory { get; private set; }
    public int StandardCheckDifficulty { get; private set; }

    public SkillCheckRequirement(SkillCategories skillCategory, int standardCheckDifficulty)
    {
        SkillCategory = skillCategory;
        StandardCheckDifficulty = standardCheckDifficulty;
    }

    // For JSON serialization
    public object ToJsonObject()
    {
        return new
        {
            SkillCategory = SkillCategory.ToString(),
            StandardCheckDifficulty = StandardCheckDifficulty
        };
    }
}